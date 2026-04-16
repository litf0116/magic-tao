<template>
    <view class="emoji-panel" v-if="visible">
        <view class="emoji-grid">
            <view 
                v-for="emoji in emojis" 
                :key="emoji"
                class="emoji-item"
                @click="selectEmoji(emoji)"
            >
                <text>{{ emoji }}</text>
            </view>
        </view>
        <view class="emoji-actions">
            <button class="close-btn" @click="close">关闭</button>
        </view>
    </view>
</template>

<script setup lang="ts">
import { ref } from 'vue'

const props = defineProps<{
    visible: boolean
}>()

const emit = defineEmits<{
    select: [emoji: string]
    close: []
}>()

const emojis = ref([
    '😀', '😃', '😄', '😁', '😆', '😅', '🤣', '😂', '🙂', '🙃',
    '😉', '😊', '😇', '🥰', '😍', '🤩', '😘', '😗', '😚', '😙',
    '😋', '😛', '😜', '🤪', '😝', '🤑', '🤗', '🤭', '🤫', '🤔',
    '🤐', '🤨', '😐', '😑', '😶', '😏', '😒', '🙄', '😬', '🤥',
    '😌', '😔', '😪', '🤤', '😴', '😷', '🤕', '🤢', '🤮', '🤧',
    '🥵', '🥶', '🥴', '😵', '🤯', '🤠', '🥳', '😎', '🤓', '🧐',
    '😕', '😟', '🙁', '☹️', '😮', '😯', '😲', '😳', '🥺', '😦',
    '😧', '😨', '😰', '😥', '😢', '😭', '😱', '🤗', '🤭', '🤫'
])

const selectEmoji = (emoji: string) => {
    emit('select', emoji)
}

const close = () => {
    emit('close')
}
</script>

<style scoped>
.emoji-panel {
    background: white;
    border-radius: 16rpx 16rpx 0 0;
    padding: 32rpx;
    max-height: 400rpx;
}

.emoji-grid {
    display: grid;
    grid-template-columns: repeat(8, 1fr);
    gap: 16rpx;
    margin-bottom: 32rpx;
}

.emoji-item {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 16rpx;
    border-radius: 8rpx;
    cursor: pointer;
    transition: background-color 0.2s;
}

.emoji-item:hover {
    background-color: #f0f0f0;
}

.emoji-item text {
    font-size: 32rpx;
}

.emoji-actions {
    display: flex;
    justify-content: flex-end;
}

.close-btn {
    padding: 16rpx 32rpx;
    background: #f0f0f0;
    border: none;
    border-radius: 8rpx;
    font-size: 28rpx;
    cursor: pointer;
}

.close-btn:hover {
    background: #e0e0e0;
}
</style>