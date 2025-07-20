<template>
    <div class="system-message" :class="systemMessageClass">
        <div class="system-icon">🔔</div>
        <div class="system-content" v-html="renderedText"></div>
    </div>
</template>

<script lang="ts">
import { computed } from 'vue'
import { type ChatMessage } from '@/api/appService'
import emojiDecoder from '@/composables/emojiDecoder'
import { useEmojiStore } from '@/stores/emojiStore'

export default {
    name: 'SystemMessage',
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

        // 根据消息类型确定样式类
        const systemMessageClass = computed(() => {
            let payload = props.message.payload
            if (typeof payload === 'string') {
                payload = JSON.parse(payload)
            }

            // 根据不同的系统消息类型设置不同的样式
            if (payload?.messageType === 'KasecStatusChanged') {
                return payload?.isKasec ? 'system-kasec-enabled' : 'system-kasec-disabled'
            }

            return 'system-default'
        })

        return {
            renderedText,
            systemMessageClass,
        }
    },
}
</script>

<style scoped>
.system-message {
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
    background: #f8f9fa;
}

.system-icon {
    margin-right: 8px;
    font-size: 16px;
}

.system-content {
    flex: 1;
}

/* 卡秒开启状态 */
.system-kasec-enabled {
    background: #fef0f0;
    border-color: #f56c6c;
    color: #f56c6c;
}

/* 卡秒关闭状态 */
.system-kasec-disabled {
    background: #f0f9ff;
    border-color: #409eff;
    color: #409eff;
}

/* 默认系统消息 */
.system-default {
    background: #f8f9fa;
    border-color: #909399;
    color: #606266;
}
</style>
