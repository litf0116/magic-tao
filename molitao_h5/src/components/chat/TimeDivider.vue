<template>
    <view class="time-divider">
        <text>{{ time }}</text>
    </view>
</template>

<script setup lang="ts">
defineProps<{
    time: string
}>()
</script>

<style scoped>
.time-divider {
    text-align: center;
    padding: 20rpx 0;
    color: #999;
    font-size: 24rpx;
}

.time-divider text {
    background: rgba(0, 0, 0, 0.1);
    padding: 8rpx 16rpx;
    border-radius: 16rpx;
}
</style>