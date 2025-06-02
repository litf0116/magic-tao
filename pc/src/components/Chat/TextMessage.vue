<template>
    <div class="content-text" @click="handleAction">
        <div v-html="renderedText"></div>
    </div>
</template>

<script lang="ts">
import { computed } from 'vue'
import { type ChatMessage } from '@/composables/types'

export default {
    name: 'TextMessage',
    props: {
        message: {
            type: Object as () => ChatMessage,
            required: true,
        },
        decoder: {
            type: Object,
            required: true,
        },
    },
    emits: ['action'],
    setup(props, { emit }) {
        // 渲染文本内容，支持表情解码和换行
        const renderedText = computed(() => {
            if (props.message && props.message.msg) {
                return (
                    props.decoder.decode(props.message.msg.replaceAll('\n', '<br/>'))
                )
            }
            return ''
        })
        // 处理点击事件
        const handleAction = () => {
            emit('action', { message: props.message })
        }
        return {
            renderedText,
            handleAction,
        }
    },
}
</script>
