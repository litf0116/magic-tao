<template>
    <div class="kasec-status-message" :class="kasecStatusClass">
        <div class="kasec-icon">⚡</div>
        <div class="kasec-content" v-html="renderedText"></div>
    </div>
</template>

<script lang="ts">
import { computed } from 'vue'
import { type ChatMessage } from '@/api/appService'
import emojiDecoder from '@/composables/emojiDecoder'
import { useEmojiStore } from '@/stores/emojiStore'

export default {
    name: 'KasecStatusMessage',
    props: {
        message: {
            type: Object as () => ChatMessage,
            required: true,
        },
    },
    setup(props) {
        const emojiStore = useEmojiStore()

        // 创建内部的emoji解码器
        const decoder = new emojiDecoder(emojiStore.emojiUrl, emojiStore.emojiMap)

        // 渲染文本内容，支持表情解码和换行
        const renderedText = computed(() => {
            if (props.message && props.message.msg) {
                const html = decoder.decode(props.message.msg.replaceAll('\n', '<br/>'))
                return html
            }
            return ''
        })

        // 根据payload中的isKasec状态确定样式类
        const kasecStatusClass = computed(() => {
            let payload = props.message.payload
            if (typeof payload === 'string') {
                payload = JSON.parse(payload)
            }
            const isKasec = payload?.isKasec
            return isKasec ? 'kasec-enabled' : 'kasec-disabled'
        })

        return {
            renderedText,
            kasecStatusClass,
        }
    },
}
</script>

<style scoped>
.kasec-status-message {
    display: flex;
    align-items: center;
    text-align: left;
    font-size: 14px;
    font-weight: 600;
    padding: 8px 12px;
    margin: 6px 0;
    line-height: 25px;
    white-space: pre-line;
    overflow-wrap: anywhere;
    border-radius: 12px;
    word-break: break-all;
    border: 2px solid;
}

.kasec-enabled {
    background: linear-gradient(135deg, #fff5f5 0%, #fed7d7 100%);
    border-color: #e53e3e;
    color: #c53030;
}

.kasec-disabled {
    background: linear-gradient(135deg, #f0fff4 0%, #c6f6d5 100%);
    border-color: #38a169;
    color: #2f855a;
}

.kasec-icon {
    font-size: 18px;
    margin-right: 8px;
}

.kasec-enabled .kasec-icon {
    color: #e53e3e;
}

.kasec-disabled .kasec-icon {
    color: #38a169;
}

.kasec-content {
    flex: 1;
}
</style>
