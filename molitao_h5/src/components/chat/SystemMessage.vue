<template>
    <div class="system-message">
        {{ msg }}
    </div>
</template>

<script setup lang="ts">
const props = defineProps<{
    type: string
    msg?: string
    fromName?: string
}>()
const { type, msg, fromName } = props
</script>

<style scoped>
.system-message {
    display: flex;
    justify-content: center;
    align-items: center;
    color: #888;
    font-size: 14px;
    margin: 10px 0;
    text-align: center;
    width: 100%;
}
</style>
