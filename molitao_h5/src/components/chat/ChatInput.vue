<template>
    <view class="chat-input">
        <view class="input-toolbar">
            <view class="toolbar-left">
                <button class="emoji-btn" :class="{ active: showEmojiPanel }" @click="toggleEmojiPanel">😊</button>
            </view>

            <view class="input-area">
                <textarea
                    v-model="messageText"
                    class="message-input"
                    placeholder="输入消息..."
                    :auto-height="true"
                    :maxlength="500"
                    @focus="onInputFocus"
                    @blur="onInputBlur"
                />
            </view>

            <view class="toolbar-right">
                <button class="send-btn" :disabled="!canSend" @click="sendMessage">发送</button>
            </view>
        </view>

        <EmojiPanel
            v-if="showEmojiPanel"
            :visible="showEmojiPanel"
            @select="insertEmoji"
            @close="showEmojiPanel = false"
        />

        <ActionPanel
            v-if="showActionPanel"
            :visible="showActionPanel"
            @send-image="$emit('sendImage')"
            @send-file="$emit('sendFile')"
            @send-location="$emit('sendLocation')"
            @close="showActionPanel = false"
        />
    </view>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import EmojiPanel from './EmojiPanel.vue'
import ActionPanel from './ActionPanel.vue'

const emit = defineEmits<{
    sendMessage: [text: string]
    sendImage: []
    sendFile: []
    sendLocation: []
}>()

const messageText = ref('')
const showEmojiPanel = ref(false)
const showActionPanel = ref(false)

const canSend = computed(() => {
    return messageText.value.trim().length > 0
})

const sendMessage = () => {
    if (!canSend.value) return

    emit('sendMessage', messageText.value.trim())
    messageText.value = ''
    showEmojiPanel.value = false
}

const toggleEmojiPanel = () => {
    showEmojiPanel.value = !showEmojiPanel.value
    showActionPanel.value = false
}

const insertEmoji = (emoji: string) => {
    messageText.value += emoji
    showEmojiPanel.value = false
}

const onInputFocus = () => {
    showEmojiPanel.value = false
    showActionPanel.value = false
}

const onInputBlur = () => {
    // 延迟处理，避免点击表情按钮时失焦
    setTimeout(() => {
        // 可以在这里处理失焦逻辑
    }, 200)
}
</script>

<style scoped>
.chat-input {
    background: #fff;
    border-top: 1rpx solid #eee;
}

.input-toolbar {
    display: flex;
    align-items: flex-end;
    padding: 20rpx;
    gap: 16rpx;
}

.toolbar-left,
.toolbar-right {
    display: flex;
    align-items: center;
    gap: 16rpx;
}

.input-area {
    flex: 1;
    min-height: 80rpx;
}

.message-input {
    width: 100%;
    min-height: 80rpx;
    max-height: 200rpx;
    padding: 16rpx 20rpx;
    border: 1rpx solid #ddd;
    border-radius: 8rpx;
    background: #f8f8f8;
    font-size: 28rpx;
    line-height: 1.4;
    resize: none;
}

.emoji-btn,
.send-btn {
    padding: 16rpx 24rpx;
    border: none;
    border-radius: 8rpx;
    font-size: 28rpx;
    cursor: pointer;
    transition: all 0.2s;
}

.emoji-btn {
    background: #f0f0f0;
    color: #666;
}

.emoji-btn:hover,
.emoji-btn.active {
    background: #e0e0e0;
    color: #333;
}

.send-btn {
    background: #007aff;
    color: white;
    min-width: 100rpx;
}

.send-btn:hover:not(:disabled) {
    background: #0056b3;
}

.send-btn:disabled {
    background: #ccc;
    cursor: not-allowed;
}
</style>
