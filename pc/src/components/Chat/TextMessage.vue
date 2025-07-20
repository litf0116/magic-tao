<template>
    <div class="content-text" :class="textMessageClass" @click="handleAction">
        <div v-html="renderedText"></div>
    </div>
</template>

<script lang="ts">
import { computed } from 'vue'
import { type ChatMessage } from '@/api/appService'
import emojiDecoder from '@/composables/emojiDecoder'
import { useEmojiStore } from '@/stores/emojiStore'

export default {
    name: 'TextMessage',
    props: {
        message: {
            type: Object as () => ChatMessage,
            required: true,
        },
    },
    emits: ['action'],
    setup(props, { emit }) {
        console.log('TextMessage setup 执行了')
        console.log('props.message:', props.message)
        const emojiStore = useEmojiStore()

        // 创建内部的emoji解码器
        const decoder = new emojiDecoder(emojiStore.emojiUrl, emojiStore.emojiMap)

        // 渲染文本内容，支持表情解码和换行
        const renderedText = computed(() => {
            console.log('props.message:', props.message)
            if (props.message && props.message.msg) {
                console.log('msg:', props.message.msg)
                const html = decoder.decode(props.message.msg.replaceAll('\n', '<br/>'))
                console.log('decode:', html)
                return html
            }
            return ''
        })

        // 根据消息类型确定样式类
        const textMessageClass = computed(() => {
            let payload = props.message.payload
            if (typeof payload === 'string') {
                payload = JSON.parse(payload)
            }

            // 检查是否是卡秒消息
            if (payload?.messageType === 'KasecStatusChanged') {
                return payload?.isKasec ? 'kasec-enabled' : 'kasec-disabled'
            }

            return 'default'
        })

        // 处理点击事件
        const handleAction = () => {
            emit('action', props.message)
        }

        return {
            renderedText,
            textMessageClass,
            handleAction,
        }
    },
}
</script>

<style scoped>
.content-text {
    display: flex;
    align-items: center;
    text-align: left;
    font-size: 14px;
    font-weight: 500;
    padding: 6px 8px;
    margin: 3px 0;
    line-height: 25px;
    white-space: pre-line;
    overflow-wrap: anywhere;
    border-radius: 8px;
    word-break: break-all;
    cursor: pointer;
}

/* 默认文本消息样式 */
.content-text.default {
    background: #eeeeee;
}

/* 卡秒开启状态 */
.content-text.kasec-enabled {
    background: #fef0f0;
    border: 2px solid #f56c6c;
    color: #f56c6c;
    font-weight: 600;
    border-radius: 12px;
}

/* 卡秒关闭状态 */
.content-text.kasec-disabled {
    background: #f0f9ff;
    border: 2px solid #409eff;
    color: #409eff;
    font-weight: 600;
    border-radius: 12px;
}
</style>
