<template>
    <div
        class="auction-bid-message border-[#ff7144] bg-[#ffb673] border-2 border-solid py-2 px-4 rounded-xl relative overflow-hidden"
        @tap="onTap(message)"
    >
        <div class="absolute top-0 right-0 bg-[#ff7144] text-white rounded-lb-lg px-2 font-bold text-xs">出价</div>
        <div class="max-w-350px min-w-200px">
            <div>商品名称: {{ payloadData.Name }}</div>
            <div style="color: #fff; font-size: 24px">当前出价：￥{{ payloadData.CurrentPrice }}</div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { defineProps, computed } from 'vue'
const props = defineProps<{
    message: any
    onTap: (message: any) => void
}>()

const payloadData = computed(() => {
    let payload = props.message.payload
    if (typeof payload === 'string') payload = JSON.parse(payload)
    return payload
})
</script>

<style scoped>
.auction-bid-message {
    margin: 8px 0;
}
</style>
