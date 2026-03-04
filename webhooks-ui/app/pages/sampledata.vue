<script setup lang="ts">
interface SampleData {
  id: string
  name: string
  description: string | null
}

const config = useRuntimeConfig()
const apiBase = config.public.apiBase

const { data: items, refresh, status } = await useFetch<SampleData[]>(`${apiBase}/api/sampledata`)

const showForm = ref(false)
const form = reactive({ name: '', description: '' })
const saving = ref(false)
const deletingId = ref<string | null>(null)

async function addSampleData() {
  if (!form.name.trim()) return
  saving.value = true
  try {
    await $fetch(`${apiBase}/api/sampledata`, {
      method: 'POST',
      body: { name: form.name, description: form.description || null }
    })
    form.name = ''
    form.description = ''
    showForm.value = false
    await refresh()
  } finally {
    saving.value = false
  }
}

async function deleteSampleData(id: string) {
  deletingId.value = id
  try {
    await $fetch(`${apiBase}/api/sampledata/${id}`, { method: 'DELETE' })
    await refresh()
  } finally {
    deletingId.value = null
  }
}
</script>

<template>
  <div class="max-w-6xl mx-auto px-6 py-8">

    <!-- Page header -->
    <div class="flex items-start justify-between mb-8">
      <div class="flex items-center gap-4">
        <div class="w-10 h-10 rounded-xl bg-primary-100 dark:bg-primary-900/40 flex items-center justify-center">
          <u-icon name="i-heroicons-circle-stack" class="w-5 h-5 text-primary-600 dark:text-primary-400" />
        </div>
        <div>
          <h1 class="text-xl font-semibold text-gray-900 dark:text-white">Sample Data</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            Manage sample data records and trigger webhook events
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
        {{ showForm ? 'Cancel' : 'Add Sample Data' }}
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
            <span class="font-semibold text-gray-900 dark:text-white">New Sample Data</span>
          </div>
        </template>
        <div class="grid grid-cols-2 gap-4">
          <u-form-field label="Name" required>
            <u-input v-model="form.name" placeholder="e.g. Order #1234" class="w-full" autofocus />
          </u-form-field>
          <u-form-field label="Description">
            <u-input v-model="form.description" placeholder="Optional description" class="w-full" />
          </u-form-field>
        </div>
        <template #footer>
          <div class="flex gap-2 justify-end">
            <u-button color="neutral" variant="ghost" @click="showForm = false">Cancel</u-button>
            <u-button
              icon="i-heroicons-check"
              :loading="saving"
              :disabled="!form.name.trim()"
              @click="addSampleData"
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
          <u-icon name="i-heroicons-circle-stack" class="w-6 h-6 text-gray-400" />
        </div>
        <p class="text-sm font-medium text-gray-900 dark:text-white">No sample data yet</p>
        <p class="text-xs text-gray-400">Add your first record to start triggering webhook events.</p>
        <u-button variant="soft" size="sm" icon="i-heroicons-plus" class="mt-1" @click="showForm = true">
          Add Sample Data
        </u-button>
      </div>
    </u-card>

    <!-- Table -->
    <u-card v-else>
      <table class="w-full text-sm">
        <thead>
          <tr class="border-b border-gray-100 dark:border-gray-800">
            <th class="text-left pb-3 px-4 text-xs font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500">Name</th>
            <th class="text-left pb-3 px-4 text-xs font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500">Description</th>
            <th class="pb-3 px-4 text-xs font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500 w-20" />
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-50 dark:divide-gray-800/60">
          <tr
            v-for="item in items"
            :key="item.id"
            class="group hover:bg-gray-50/80 dark:hover:bg-gray-800/40 transition-colors"
          >
            <td class="py-3 px-4 font-medium text-gray-900 dark:text-white">
              {{ item.name }}
            </td>
            <td class="py-3 px-4 text-gray-500 dark:text-gray-400">
              {{ item.description ?? '—' }}
            </td>
            <td class="py-3 px-4 text-right opacity-0 group-hover:opacity-100 transition-opacity">
              <u-button
                icon="i-heroicons-trash"
                color="error"
                variant="ghost"
                size="xs"
                :loading="deletingId === item.id"
                @click="deleteSampleData(item.id)"
              />
            </td>
          </tr>
        </tbody>
      </table>
    </u-card>

  </div>
</template>
