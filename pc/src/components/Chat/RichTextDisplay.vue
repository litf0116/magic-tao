<template>
    <!-- eslint-disable-next-line vue/no-v-html -->
    <div class="rich-text-display" @click="handleClick" v-html="sanitizedContent"></div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import DOMPurify from 'dompurify'

interface Props {
    content: string
    enableImageClick?: boolean
}

const props = withDefaults(defineProps<Props>(), {
    enableImageClick: true,
})

const emit = defineEmits<{
    imageClick: [event: Event]
}>()

// 清理HTML内容，防止XSS攻击
const sanitizedContent = computed(() => {
    if (!props.content) return ''

    // 使用DOMPurify清理HTML
    const cleanHTML = DOMPurify.sanitize(props.content, {
        ALLOWED_TAGS: [
            'p',
            'div',
            'span',
            'br',
            'img',
            'strong',
            'b',
            'em',
            'i',
            'u',
            'ul',
            'ol',
            'li',
            'h1',
            'h2',
            'h3',
            'h4',
            'h5',
            'h6',
        ],
        ALLOWED_ATTR: ['src', 'alt', 'width', 'height', 'style', 'class'],
        ALLOW_DATA_ATTR: false,
    })

    return cleanHTML
})

// 处理图片点击事件
const handleClick = (event: Event) => {
    if (props.enableImageClick && event.target instanceof HTMLImageElement) {
        emit('imageClick', event)
    }
}
</script>

<style scoped>
.rich-text-display {
    word-wrap: break-word;
    word-break: break-all;
}

.rich-text-display :deep(img) {
    max-width: 100%;
    height: auto;
    cursor: pointer;
    transition: opacity 0.2s;
}

.rich-text-display :deep(img:hover) {
    opacity: 0.8;
}

.rich-text-display :deep(p) {
    margin: 0.5em 0;
}

.rich-text-display :deep(ul),
.rich-text-display :deep(ol) {
    margin: 0.5em 0;
    padding-left: 1.5em;
}

.rich-text-display :deep(li) {
    margin: 0.25em 0;
}

.rich-text-display :deep(strong),
.rich-text-display :deep(b) {
    font-weight: bold;
}

.rich-text-display :deep(em),
.rich-text-display :deep(i) {
    font-style: italic;
}

.rich-text-display :deep(u) {
    text-decoration: underline;
}
</style>
