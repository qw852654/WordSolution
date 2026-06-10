<template>
  <div class="weak-scroll-area min-h-0 overflow-y-auto">
    <slot />
  </div>
</template>

<style scoped>
.weak-scroll-area {
  scrollbar-color: color-mix(in oklab, var(--muted-foreground) 35%, transparent) transparent;
  scrollbar-width: thin;
}

.weak-scroll-area::-webkit-scrollbar {
  width: 0.75rem;
}

.weak-scroll-area::-webkit-scrollbar-track {
  border-left: 1px dashed var(--border);
  background: transparent;
}

.weak-scroll-area::-webkit-scrollbar-thumb {
  min-height: 1.25rem;
  border: 0.25rem solid transparent;
  border-radius: 9999px;
  background-color: color-mix(in oklab, var(--muted-foreground) 35%, transparent);
  background-clip: content-box;
}

.weak-scroll-area::-webkit-scrollbar-thumb:hover {
  background-color: color-mix(in oklab, var(--muted-foreground) 50%, transparent);
}
</style>
