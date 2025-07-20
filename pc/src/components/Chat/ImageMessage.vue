<template>
    <div v-if="payloadData.url" class="content-image" @click="handleAction" @contextmenu.prevent="handleContextMenu">
        <img :data-url="imgUrl" :src="imgUrl" :style="{ maxHeight: getImageHeight(200, 150) + 'px' }" />
    </div>
</template>

<script lang="ts">
import { computed } from 'vue'
import { type ChatMessage } from '@/composables/types'

export default {
    name: 'ImageMessage',
    props: {
        message: {
            type: Object as () => ChatMessage,
            required: true,
        },
    },
    emits: ['action', 'contextMenu'],
    setup(props, { emit }) {
        // 解析payload数据
        const payloadData = computed(() => {
            let payload = props.message.payload
            if (typeof payload === 'string') {
                payload = JSON.parse(payload!)
            }
            return payload
        })

        // 计算图片URL
        const imgUrl = computed(() => {
            if (!payloadData.value.url) return ''
            if (payloadData.value.url.startsWith('http')) return payloadData.value.url + '!w300'
            return `${import.meta.env.VITE_APP_UPYUN_IMG_URL}${payloadData.value.url}!w300`
        })

        // 计算图片高度
        function getImageHeight(width: number, height: number) {
            const IMAGE_MAX_WIDTH = 200
            const IMAGE_MAX_HEIGHT = 150
            if (width < IMAGE_MAX_WIDTH && height < IMAGE_MAX_HEIGHT) {
                return height
            } else if (width > height) {
                return (IMAGE_MAX_WIDTH / width) * height
            } else if (width === height || width < height) {
                return IMAGE_MAX_HEIGHT
            }
        }

        // 处理点击事件
        const handleAction = () => {
            emit('action', { message: props.message, payload: payloadData.value })
        }
        // 处理右键菜单
        const handleContextMenu = () => {
            emit('contextMenu', { message: props.message, payload: payloadData.value })
        }

        return {
            payloadData,
            imgUrl,
            getImageHeight,
            handleAction,
            handleContextMenu,
        }
    },
}
</script>
