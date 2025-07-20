<template>
    <div class="content-text" @click="handleAction">
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

        // 处理点击事件
        const handleAction = () => {
            emit('action', props.message)
        }

        return {
            renderedText,
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
    background: #eeeeee;
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
</style>
