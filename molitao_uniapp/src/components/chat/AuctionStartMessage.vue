<template>
    <div class="auction-start-message border-red-500 border-2 border-solid py-2 px-4 rounded-lg relative">
        <div class="absolute top-0 right-0 bg-red-500 text-white rounded-lb-lg px-2 font-bold text-xs">开始拍卖</div>
        <div class="max-w-350px min-w-200px" @tap="catchImage">
            <div>商品名称: {{ payloadData.name }}</div>
            <rich-text :nodes="payloadData.description"></rich-text>
        </div>
    </div>
</template>

<script setup lang="ts">
import { defineProps, computed } from 'vue'
const props = defineProps<{
    message: any
    catchImage: (e: any) => void
}>()

const payloadData = computed(() => {
    let payload = props.message.payload
    if (typeof payload === 'string') {
        payload = JSON.parse(payload)
        console.log('payload', payload)
    }
    return payload
})
</script>

<style scoped>
.auction-start-message {
    margin: 8px 0;
    background: #fff5f5;
}
</style>
