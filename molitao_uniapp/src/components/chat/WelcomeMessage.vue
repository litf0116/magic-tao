<template>
    <div class="welcome-message">
        <text>{{ fromName }} 加入群聊</text>
    </div>
</template>

<script setup lang="ts">

const props = defineProps<{ fromName: string }>()
</script>

<style scoped>
.welcome-message {
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
