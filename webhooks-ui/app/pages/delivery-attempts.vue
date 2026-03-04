<script setup lang="ts">
interface DeliveryAttempt {
  id: string
  webhookSubscriptionId: string
  payload: string
  responseStatusCode: number | null
  success: boolean
  timestamp: string
}

const config = useRuntimeConfig()
const apiBase = config.public.apiBase

const { data: items, refresh, status } = await useFetch<DeliveryAttempt[]>(
  `${apiBase}/api/webhooks/delivery-attempts`
)

const deletingId = ref<string | null>(null)
const expandedId = ref<string | null>(null)

async function deleteAttempt(id: string) {
  deletingId.value = id
  try {
    await $fetch(`${apiBase}/api/webhooks/delivery-attempts/${id}`, { method: 'DELETE' })
    await refresh()
  } finally {
    deletingId.value = null
  }
}

function toggleExpand(id: string) {
  expandedId.value = expandedId.value === id ? null : id
}

function formatDate(utc: string) {
  return new Date(utc).toLocaleString()
}

function formatPayload(raw: string) {
  try {
    return JSON.stringify(JSON.parse(raw), null, 2)
  } catch {
    return raw
  }
}

function statusColor(code: number | null, success: boolean): 'success' | 'warning' | 'error' {
  if (success) return 'success'
  if (code && code >= 400 && code < 500) return 'warning'
  return 'error'
}

const successCount = computed(() => items.value?.filter(i => i.success).length ?? 0)
const failCount = computed(() => items.value?.filter(i => !i.success).length ?? 0)

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
        <div class="w-10 h-10 rounded-xl bg-sky-100 dark:bg-sky-900/40 flex items-center justify-center">
          <u-icon name="i-heroicons-paper-airplane" class="w-5 h-5 text-sky-600 dark:text-sky-400" />
        </div>
        <div>
          <h1 class="text-xl font-semibold text-gray-900 dark:text-white">Delivery Attempts</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Inspect outgoing webhook delivery history
          </p>
        </div>
      </div>
      <u-button
        icon="i-heroicons-arrow-path"
        color="neutral"
        variant="ghost"
        :class="status === 'pending' ? 'animate-spin' : ''"
        @click="refresh()"
      >
        Refresh
      </u-button>
    </div>

    <!-- Stats row -->
    <div v-if="items?.length" class="grid grid-cols-3 gap-4 mb-6">
      <u-card>
        <div class="flex items-center gap-3 p-1">
          <div class="w-8 h-8 rounded-lg bg-gray-100 dark:bg-gray-800 flex items-center justify-center shrink-0">
            <u-icon name="i-heroicons-queue-list" class="w-4 h-4 text-gray-500" />
          </div>
          <div>
            <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ items.length }}</p>
            <p class="text-xs text-gray-400">Total</p>
          </div>
        </div>
      </u-card>
      <u-card>
        <div class="flex items-center gap-3 p-1">
          <div class="w-8 h-8 rounded-lg bg-green-100 dark:bg-green-900/40 flex items-center justify-center shrink-0">
            <u-icon name="i-heroicons-check-circle" class="w-4 h-4 text-green-600 dark:text-green-400" />
          </div>
          <div>
            <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ successCount }}</p>
            <p class="text-xs text-gray-400">Successful</p>
          </div>
        </div>
      </u-card>
      <u-card>
        <div class="flex items-center gap-3 p-1">
          <div class="w-8 h-8 rounded-lg bg-red-100 dark:bg-red-900/40 flex items-center justify-center shrink-0">
            <u-icon name="i-heroicons-x-circle" class="w-4 h-4 text-red-600 dark:text-red-400" />
          </div>
          <div>
            <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ failCount }}</p>
            <p class="text-xs text-gray-400">Failed</p>
          </div>
        </div>
      </u-card>
    </div>

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
          <u-icon name="i-heroicons-paper-airplane" class="w-6 h-6 text-gray-400" />
        </div>
        <p class="text-sm font-medium text-gray-900 dark:text-white">No delivery attempts yet</p>
        <p class="text-xs text-gray-400">Attempts will appear here after webhooks are dispatched.</p>
      </div>
    </u-card>

    <!-- Table -->
    <u-card v-else>
      <table class="w-full text-sm">
        <thead>
          <tr class="border-b border-gray-100 dark:border-gray-800">
            <th class="pb-3 px-4 w-8" />
            <th class="text-left pb-3 px-4 text-xs font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500 w-28">Status</th>
            <th class="text-left pb-3 px-4 text-xs font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500 w-20">HTTP</th>
            <th class="text-left pb-3 px-4 text-xs font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500">Subscription ID</th>
            <th class="text-left pb-3 px-4 text-xs font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500 w-44">Timestamp</th>
            <th class="pb-3 px-4 w-16" />
          </tr>
        </thead>
        <tbody>
          <template v-for="item in items" :key="item.id">
            <tr
              class="group border-b border-gray-50 dark:border-gray-800/60 hover:bg-gray-50/80 dark:hover:bg-gray-800/40 transition-colors cursor-pointer"
              :class="{ 'bg-gray-50/60 dark:bg-gray-800/20': expandedId === item.id }"
              @click="toggleExpand(item.id)"
            >
              <!-- Chevron -->
              <td class="py-3 px-4">
                <u-icon
                  :name="expandedId === item.id ? 'i-heroicons-chevron-down' : 'i-heroicons-chevron-right'"
                  class="w-4 h-4 text-gray-400 transition-transform"
                />
              </td>
              <!-- Status badge -->
              <td class="py-3 px-4">
                <u-badge
                  :color="statusColor(item.responseStatusCode, item.success)"
                  variant="subtle"
                  size="sm"
                >
                  <u-icon
                    :name="item.success ? 'i-heroicons-check-circle' : 'i-heroicons-x-circle'"
                    class="w-3 h-3 mr-1"
                  />
                  {{ item.success ? 'Success' : 'Failed' }}
                </u-badge>
              </td>
              <!-- HTTP code -->
              <td class="py-3 px-4">
                <span
                  class="font-mono text-xs font-semibold px-2 py-0.5 rounded"
                  :class="item.success
                    ? 'bg-green-50 dark:bg-green-900/20 text-green-700 dark:text-green-400'
                    : 'bg-red-50 dark:bg-red-900/20 text-red-700 dark:text-red-400'"
                >
                  {{ item.responseStatusCode ?? '—' }}
                </span>
              </td>
              <!-- Subscription ID -->
              <td class="py-3 px-4" @click.stop>
                <div class="group/id flex items-center gap-1.5">
                  <span class="font-mono text-xs bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 px-2 py-1 rounded-md border border-gray-200 dark:border-gray-700 select-all">
                    {{ item.webhookSubscriptionId }}
                  </span>
                  <u-button
                    :icon="copiedId === item.webhookSubscriptionId ? 'i-heroicons-check' : 'i-heroicons-clipboard-document'"
                    :color="copiedId === item.webhookSubscriptionId ? 'success' : 'neutral'"
                    variant="ghost"
                    size="xs"
                    class="opacity-0 group-hover/id:opacity-100 transition-opacity shrink-0"
                    @click="copyId(item.webhookSubscriptionId)"
                  />
                </div>
              </td>
              <!-- Timestamp -->
              <td class="py-3 px-4 text-xs text-gray-400 dark:text-gray-500">
                {{ formatDate(item.timestamp) }}
              </td>
              <!-- Delete -->
              <td class="py-3 px-4 text-right" @click.stop>
                <u-button
                  icon="i-heroicons-trash"
                  color="error"
                  variant="ghost"
                  size="xs"
                  class="opacity-0 group-hover:opacity-100 transition-opacity"
                  :loading="deletingId === item.id"
                  @click="deleteAttempt(item.id)"
                />
              </td>
            </tr>

            <!-- Expanded payload -->
            <Transition
              enter-active-class="transition ease-out duration-150"
              enter-from-class="opacity-0"
              enter-to-class="opacity-100"
              leave-active-class="transition ease-in duration-100"
              leave-from-class="opacity-100"
              leave-to-class="opacity-0"
            >
              <tr v-if="expandedId === item.id" class="border-b border-gray-100 dark:border-gray-800">
                <td colspan="6" class="px-6 pb-4 pt-2">
                  <div class="rounded-lg bg-gray-950 dark:bg-gray-900 border border-gray-800 overflow-hidden">
                    <div class="flex items-center justify-between px-4 py-2 border-b border-gray-800">
                      <span class="text-xs font-medium text-gray-400 uppercase tracking-wider">Payload</span>
                      <u-icon name="i-heroicons-code-bracket" class="w-4 h-4 text-gray-600" />
                    </div>
                    <pre class="p-4 text-xs text-green-400 font-mono overflow-x-auto leading-relaxed">{{ formatPayload(item.payload) }}</pre>
                  </div>
                </td>
              </tr>
            </Transition>
          </template>
        </tbody>
      </table>
    </u-card>

  </div>
</template>
