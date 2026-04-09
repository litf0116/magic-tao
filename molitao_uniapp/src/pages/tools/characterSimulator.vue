<template>
    <view class="min-h-screen bg-[#f6f6f6] p-4">
        <view class="myCard p-4 mb-4">
            <view class="text-lg font-500 mb-4 text-center">
                <text>人物模拟器</text>
            </view>
            <view class="text-gray-500 text-sm text-center">
                <text>功能开发中，敬请期待...</text>
            </view>
        </view>
    </view>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'

onMounted(() => {
    // 页面加载逻辑
})
</script>

<style lang="scss">
.myCard {
    @apply bg-white rounded-4;
}
</style>

<route lang="json">
{
    "layout": "main",
    "style": {
        "navigationBarTitleText": "人物模拟器"
    }
}
</route>