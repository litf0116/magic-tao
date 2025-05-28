<template>
    <button>
        {{ name }}
    </button>
    <next-button />
    123
</template>

<script setup lang="ts">
defineProps({
    name: {
        type: String,
        default: '123',
    },
})
</script>
