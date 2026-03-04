<template>
  <nav class="sticky top-0 z-50 bg-white/80 dark:bg-gray-900/80 backdrop-blur border-b border-gray-200 dark:border-gray-800">
    <div class="max-w-6xl mx-auto px-6 h-14 flex items-center gap-8">

      <nuxt-link to="/" class="flex items-center gap-2 shrink-0">
        <div class="w-7 h-7 rounded-lg bg-primary-500 flex items-center justify-center">
          <u-icon name="i-heroicons-bolt" class="text-white w-4 h-4" />
        </div>
        <span class="font-semibold text-gray-900 dark:text-white tracking-tight">Webhooks UI</span>
      </nuxt-link>

      <div class="flex items-center gap-1">
        <nuxt-link
          v-for="link in navLinks"
          :key="link.to"
          :to="link.to"
          class="flex items-center gap-1.5 px-3 py-1.5 rounded-md text-sm font-medium transition-colors"
          :class="$route.path === link.to
            ? 'bg-primary-50 dark:bg-primary-900/30 text-primary-600 dark:text-primary-400'
            : 'text-gray-500 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-800'"
        >
          <u-icon :name="link.icon" class="w-4 h-4" />
          {{ link.label }}
        </nuxt-link>
      </div>

    </div>
  </nav>
</template>

<script setup lang="ts">
const navLinks = [
  { to: '/sampledata', label: 'Sample Data', icon: 'i-heroicons-circle-stack' },
  { to: '/subscriptions', label: 'Subscriptions', icon: 'i-heroicons-rss' },
  { to: '/delivery-attempts', label: 'Delivery Attempts', icon: 'i-heroicons-paper-airplane' },
]
</script>
