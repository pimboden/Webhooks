# Webhooks POC — Getting Started Guide

## Credits

This POC is based on the excellent tutorial series by Milan Jovanović:
**[Building a Webhook System in .NET](https://www.youtube.com/watch?v=vaVZSh8QqH8&list=PLYpjLpq5ZDGsnBmwJCdFv5PhQbUZruKy3)**

The series walks through the design and implementation of a production-grade webhook fan-out system in .NET, covering subscriptions, reliable delivery, retries, and observability. Highly recommended watching before diving into this codebase.

---

## Overview

This POC demonstrates a webhook fan-out system where any event source (modern .NET API or legacy .NET Framework app) can fire a typed event, and all registered subscribers receive an HTTP POST to their configured URL.

---

## 0. Configure Required Secrets

The solution uses **User Secrets** to keep passwords out of source control. Two projects need secrets configured before the first run.

### Webhooks.AppHost (Aspire orchestrator)

Run these commands from the `Webhooks.AppHost` project folder, or use **Visual Studio → right-click project → Manage User Secrets**:

```bash
cd Webhooks.AppHost

dotnet user-secrets set "Parameters:postgres-password" "your-postgres-password"
dotnet user-secrets set "Parameters:rabbitmq-password" "your-rabbitmq-password"
```

Choose any strong password for each. These values are injected at runtime into the PostgreSQL and RabbitMQ containers.

### WebForms (legacy .NET Framework app)

The WebForms project uses the older **XML-based** user secrets format. Open the secrets file for the WebForms project:

```
%APPDATA%\Microsoft\UserSecrets\4b6afb8e-bb6a-4061-82a1-a10dfa424c12\secrets.xml
```

> **Tip:** In Visual Studio, right-click the `WebForms` project → **Manage User Secrets** to open this file directly.

Set the `rabbitmq` connection string, using the **same password** you set in `Parameters:rabbitmq-password` above:

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <secrets ver="1.0">
    <secret name="rabbitmq" value="amqp://guest:your-rabbitmq-password@localhost:5672" />
  </secrets>
</root>
```

> **Important:** The password in the WebForms `secrets.xml` must match `Parameters:rabbitmq-password` in the AppHost secrets exactly, since WebForms connects directly to RabbitMQ on `localhost:5672`.

---

## 1. Start the Aspire System

Open `Webhooks.slnx` in **Visual Studio 2022** and set **`Webhooks.AppHost`** as the startup project.

Press **F5** (or Ctrl+F5). Aspire will start and wait for all services to be healthy:

| Service | URL |
|---|---|
| Aspire Dashboard | http://localhost:18888 (or as shown in console) |
| Webhooks API | shown in Aspire dashboard |
| Webhooks UI | http://localhost:3000 |
| RabbitMQ Management | http://localhost:15672 (guest / *your secret password*) |
| PostgreSQL | localhost:49959 |

> **Note:** The first run pulls Docker images for PostgreSQL and RabbitMQ — this may take a minute.

---

## 2. Create Webhook Subscriptions (Nuxt UI)

Navigate to **http://localhost:3000** and go to the **Subscriptions** page.

To receive webhook deliveries you need a publicly reachable URL. A quick option during development is [https://webhook.site](https://webhook.site) — it gives you a free unique URL that logs every incoming request.

1. Click **Add Subscription**
2. Select an **Event Type** from the dropdown:
   - `sampledata.created` — fired when a sample item is created via the API
   - `sampledata.deleted` — fired when a sample item is deleted via the API
   - `webform.fired` — fired when the button is clicked in the WebForms app
3. Paste your **Webhook URL** (e.g. your webhook.site URL)
4. Click **Save**

Repeat for each event type you want to observe. You can add multiple subscriptions per event type pointing to different URLs.

---

## 3. Fire Events via Sample Data

Sample data can be created and deleted in two ways — pick whichever is more convenient.

### Option A — Nuxt UI (easiest)

Navigate to **http://localhost:3000/sampledata**.

- Click **Create** to add a new item → fires `sampledata.created`
- Click the **Delete** button next to an item → fires `sampledata.deleted`

### Option B — `Webhooks.Api.http` file

Open `Webhooks.Api/Webhooks.Api.http` in Visual Studio (or any HTTP client that supports `.http` files such as the VS Code REST Client extension).

### Create a sample item → fires `sampledata.created`

```http
POST /sampledata
Content-Type: application/json

{
  "name": "Test Item",
  "description": "Created from .http file"
}
```

Copy the `id` from the response — you'll need it to delete the item.

### Delete a sample item → fires `sampledata.deleted`

```http
DELETE /sampledata/{id}
```

Each call fires the corresponding event, which fans out to all subscribers registered for that event type. Check your webhook.site URL (or other target) to confirm the payload arrived.

---

## 4. Fire Events from the WebForms App (Legacy .NET Framework)

The `WebForms` project is a .NET Framework 4.8 ASP.NET WebForms application that demonstrates how a legacy app can dispatch events using the `Webhooks.EventDispatcher.RabbitMQ` package — **without any dependency on Wolverine or .NET Core**.

### Steps

1. Open a **second instance of Visual Studio 2022**
2. Open `Webhooks.slnx` (or just open the `WebForms` project directly)
3. Set **`WebForms`** as the startup project
4. Press **F5** — IIS Express launches the WebForms app
5. Click the **Fire Event** button on the default page

This dispatches a `webform.fired` event with source metadata. Any subscription registered for `webform.fired` will receive the webhook delivery.

> **Important:** The Aspire system (step 1) must already be running before you start WebForms, since WebForms connects directly to RabbitMQ on `localhost:5672`.

---

## 5. Observe Event Flow via Aspire Traces

The Aspire Dashboard includes a **Traces** view that shows the full lifecycle of each event.

1. Open the Aspire Dashboard (URL printed in the AppHost console on startup)
2. Click **Traces** in the left nav
3. Select **`webhooks-processing`** as the resource filter

Each button click or API call produces a trace showing:

- The **receive** span — Wolverine picking up the `WebhookDispatched` message from RabbitMQ
- The **handler** span — `WebhookDispatchedHandler` fanning out to each subscriber
- Child **HTTP** spans — one per subscriber URL, showing the delivery attempt and HTTP status code

A red dot on a trace means something failed (e.g. the subscriber URL returned a non-2xx response or a deserialization error occurred). Click any trace to drill into the individual spans and exception details.

> **Tip:** The RabbitMQ Management UI (http://localhost:15672) is also useful for inspecting queue depths and confirming messages are being published and consumed.
