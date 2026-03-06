<script setup lang="ts">
interface WebhookSubscription {
  id: string
  eventType: string
  webhookUrl: string
  createTimeUtc: string
}

const config = useRuntimeConfig()
const apiBase = config.public.apiBase

const { data: items, refresh, status } = await useFetch<WebhookSubscription[]>(
  `${apiBase}/api/webhooks/subscriptions`
)

const EVENT_TYPES = ['sampledata.created', 'sampledata.deleted', 'webform.fired']

const showForm = ref(false)
const form = reactive({ eventType: EVENT_TYPES[0], webhookUrl: '' })
const saving = ref(false)
const deletingId = ref<string | null>(null)

async function addSubscription() {
  if (!form.webhookUrl.trim()) return
  saving.value = true
  try {
    await $fetch(`${apiBase}/api/webhooks/subscriptions`, {
      method: 'POST',
      body: { eventType: form.eventType, webhookUrl: form.webhookUrl }
    })
    form.eventType = EVENT_TYPES[0]
    form.webhookUrl = ''
    showForm.value = false
    await refresh()
  } finally {
    saving.value = false
  }
}

async function deleteSubscription(id: string) {
  deletingId.value = id
  try {
    await $fetch(`${apiBase}/api/webhooks/subscriptions/${id}`, { method: 'DELETE' })
    await refresh()
  } finally {
    deletingId.value = null
  }
}

function formatDate(utc: string) {
  return new Date(utc).toLocaleString()
}

const eventTypeColor: Record<string, 'primary' | 'success' | 'secondary'> = {
  'sampledata.created': 'success',
  'sampledata.deleted': 'secondary',
   'webform.fired': 'primary'
}

const copiedId = ref<string | null>(null)

async function copyId(id: string) {
  await navigator.clipboard.writeText(id)
  copiedId.value = id
  setTimeout(() => { copiedId.value = null }, 1500)
}
</script>

<template>
  <div class="max-w-6xl mx-auto px-6 py-8">

    <!-- Page header -->
    <div class="flex items-start justify-between mb-8">
      <div class="flex items-center gap-4">
        <div class="w-10 h-10 rounded-xl bg-violet-100 dark:bg-violet-900/40 flex items-center justify-center">
          <u-icon name="i-heroicons-rss" class="w-5 h-5 text-violet-600 dark:text-violet-400" />
        </div>
        <div>
          <h1 class="text-xl font-semibold text-gray-900 dark:text-white">Webhook Subscriptions</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Configure which endpoints receive webhook events
          </p>
        </div>
        <u-badge v-if="items?.length" color="neutral" variant="subtle" class="ml-1">
          {{ items.length }}
        </u-badge>
      </div>
      <u-button
        :icon="showForm ? 'i-heroicons-x-mark' : 'i-heroicons-plus'"
        :color="showForm ? 'neutral' : 'primary'"
        :variant="showForm ? 'ghost' : 'solid'"
        @click="showForm = !showForm"
      >
        {{ showForm ? 'Cancel' : 'Add Subscription' }}
      </u-button>
    </div>

    <!-- Add form -->
    <Transition
      enter-active-class="transition ease-out duration-200"
      enter-from-class="opacity-0 -translate-y-2"
      enter-to-class="opacity-100 translate-y-0"
      leave-active-class="transition ease-in duration-150"
      leave-from-class="opacity-100 translate-y-0"
      leave-to-class="opacity-0 -translate-y-2"
    >
      <u-card v-if="showForm" class="mb-6 ring-1 ring-primary-200 dark:ring-primary-800">
        <template #header>
          <div class="flex items-center gap-2">
            <u-icon name="i-heroicons-plus-circle" class="w-4 h-4 text-primary-500" />
            <span class="font-semibold text-gray-900 dark:text-white">New Subscription</span>
          </div>
        </template>
        <div class="grid grid-cols-2 gap-4">
          <u-form-field label="Event Type" required>
            <u-select v-model="form.eventType" :items="EVENT_TYPES" class="w-full" />
          </u-form-field>
          <u-form-field label="Webhook URL" required>
            <u-input
              v-model="form.webhookUrl"
              placeholder="https://your-endpoint.com/webhook"
              class="w-full"
            />
          </u-form-field>
        </div>
        <template #footer>
          <div class="flex gap-2 justify-end">
            <u-button color="neutral" variant="ghost" @click="showForm = false">Cancel</u-button>
            <u-button
              icon="i-heroicons-check"
              :loading="saving"
              :disabled="!form.webhookUrl.trim()"
              @click="addSubscription"
            >
              Save
            </u-button>
          </div>
        </template>
      </u-card>
    </Transition>

    <!-- Loading -->
    <div v-if="status === 'pending'" class="flex items-center justify-center py-20">
      <div class="flex flex-col items-center gap-3 text-gray-400">
        <u-icon name="i-heroicons-arrow-path" class="w-6 h-6 animate-spin" />
        <span class="text-sm">Loading...</span>
      </div>
    </div>

    <!-- Empty state -->
    <u-card v-else-if="!items?.length">
      <div class="flex flex-col items-center py-12 gap-3">
        <div class="w-12 h-12 rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center">
          <u-icon name="i-heroicons-rss" class="w-6 h-6 text-gray-400" />
        </div>
        <p class="text-sm font-medium text-gray-900 dark:text-white">No subscriptions yet</p>
        <p class="text-xs text-gray-400">Subscribe an endpoint to start receiving webhook events.</p>
        <u-button variant="soft" size="sm" icon="i-heroicons-plus" class="mt-1" @click="showForm = true">
          Add Subscription
        </u-button>
      </div>
    </u-card>

    <!-- Table -->
    <u-card v-else>
      <table class="w-full text-sm">
        <thead>
          <tr class="border-b border-gray-100 dark:border-gray-800">
            <th class="text-left pb-3 px-4 text-xs font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500 w-40">Event Type</th>
            <th class="text-left pb-3 px-4 text-xs font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500">Webhook URL</th>
            <th class="text-left pb-3 px-4 text-xs font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500">Subscription ID</th>
            <th class="text-left pb-3 px-4 text-xs font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500 w-40">Created</th>
            <th class="pb-3 px-4 w-16" />
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-50 dark:divide-gray-800/60">
          <tr
            v-for="item in items"
            :key="item.id"
            class="group hover:bg-gray-50/80 dark:hover:bg-gray-800/40 transition-colors"
          >
            <td class="py-3 px-4">
              <u-badge
                :color="eventTypeColor[item.eventType] ?? 'neutral'"
                variant="subtle"
                size="sm"
              >
                {{ item.eventType }}
              </u-badge>
            </td>
            <td class="py-3 px-4 font-mono text-xs text-gray-600 dark:text-gray-400 max-w-xs truncate">
              {{ item.webhookUrl }}
            </td>
            <td class="py-3 px-4">
              <div class="group/id flex items-center gap-1.5">
                <span class="font-mono text-xs bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 px-2 py-1 rounded-md border border-gray-200 dark:border-gray-700 select-all">
                  {{ item.id }}
                </span>
                <u-button
                  :icon="copiedId === item.id ? 'i-heroicons-check' : 'i-heroicons-clipboard-document'"
                  :color="copiedId === item.id ? 'success' : 'neutral'"
                  variant="ghost"
                  size="xs"
                  class="opacity-0 group-hover/id:opacity-100 transition-opacity shrink-0"
                  @click.stop="copyId(item.id)"
                />
              </div>
            </td>
            <td class="py-3 px-4 text-xs text-gray-400 dark:text-gray-500">
              {{ formatDate(item.createTimeUtc) }}
            </td>
            <td class="py-3 px-4 text-right opacity-0 group-hover:opacity-100 transition-opacity">
              <u-button
                icon="i-heroicons-trash"
                color="error"
                variant="ghost"
                size="xs"
                :loading="deletingId === item.id"
                @click="deleteSubscription(item.id)"
              />
            </td>
          </tr>
        </tbody>
      </table>
    </u-card>

  </div>
</template>
