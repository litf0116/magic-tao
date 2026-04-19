<template>
    <view
        v-if="level"
        class="user-level-badge"
        :class="{
            'level-7-premium': level.level === 7,
            'level-8-ultimate': level.level === 8,
        }"
        :style="getLevelStyle(level)"
        @click="$emit('click')"
    >
        <span>{{ level.name }}</span>
    </view>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
    level: {
        level: number
        name: string
        color?: string
        backgroundColor?: string
    }
}>()

defineEmits<{
    click: []
}>()

const getLevelStyle = (level: any) => {
    return {
        color: level.color || '#fff',
        backgroundColor: level.backgroundColor || '#333',
        marginRight: '10rpx',
    }
}
</script>

<style scoped>
.user-level-badge {
    border-radius: 8rpx;
    font-weight: bold;
    padding: 4rpx 8rpx;
    font-size: 20rpx;
    display: inline-block;
}

.level-7-premium {
    border: 2rpx solid #ffd700;
    box-shadow: 0 0 6rpx rgba(255, 215, 0, 0.6);
    font-family: 'Microsoft YaHei', '微软雅黑', sans-serif;
}

.level-8-ultimate {
    position: relative;
    padding: 4rpx 10rpx;
    background: linear-gradient(45deg, #ff0000, #ff7300, #fffb00, #48ff00, #00ffd5, #002bff, #7a00ff, #ff00c8, #ff0000);
    background-size: 400%;
    animation: rainbow-bg 6s linear infinite;
    font-family: 'Microsoft YaHei', '微软雅黑', sans-serif;
}

@keyframes rainbow-bg {
    0% {
        background-position: 0 0;
    }
    50% {
        background-position: 400% 0;
    }
    100% {
        background-position: 0 0;
    }
}
</style>
