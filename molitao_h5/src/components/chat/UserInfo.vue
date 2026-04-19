<template>
    <view class="user-info" style="display: flex" :class="[message.fromAdmin ? ' !text-red-500' : '']">
        <div v-if="chatType === 'private'">
            <span v-if="message.fromAdmin && message.fromTag" :class="[message.tagClass ? message.tagClass : '']">{{
                message.fromTag
            }}</span>
        </div>
        <div v-else>
            <span v-if="message.fromAdmin && message.fromTag" :class="[message.tagClass ? message.tagClass : '']">{{
                message.fromTag
            }}</span>
            <UserLevelBadge
                v-else-if="message.userChatLevel"
                :level="message.userChatLevel"
                @click="$emit('toggleRules')"
            />
        </div>
        {{ message.fromName }}
    </view>
</template>

<script setup lang="ts">
import { ChatMessage } from '@/composables/types'
import UserLevelBadge from './UserLevelBadge.vue'

const props = defineProps<{
    message: ChatMessage
    chatType: 'private' | 'group'
    showRules: boolean
}>()

defineEmits<{
    toggleRules: []
}>()
</script>

<style scoped>
.user-info {
    font-size: 24rpx;
    color: #666;
    margin-bottom: 8rpx;
}

.user-info.self {
    text-align: right;
}
</style>
