<template>
    <div class="agreement-container">
        <div v-if="loading" class="text-center py-10">加载中...</div>
        <div v-else-if="error" class="text-center py-10 text-red-500">{{ error }}</div>
        <div v-else class="agreement-content">
            <h1>{{ article?.title }}</h1>
            <div class="content-body" v-html="renderContent"></div>
        </div>
    </div>
</template>

<script setup lang="ts">
import api from '@/api'
import { useRoute } from 'vue-router'

const route = useRoute()
const type = computed(() => (route.query.type as string) || 'user-agreement')
const titleMap: Record<string, string> = {
    'user-agreement': '用户协议',
    'privacy-policy': '隐私政策',
}

const loading = ref(true)
const error = ref('')
const articles = ref<any[]>([])

const article = computed(() => {
    const targetTitle = titleMap[type.value]
    return articles.value.find((a: any) => a.title === targetTitle) || null
})

const renderContent = computed(() => {
    if (!article.value?.content) return ''
    // 将纯文本换行转为 HTML 段落
    return article.value.content
        .split('\n')
        .filter((line: string) => line.trim())
        .map((line: string) => `<p>${line}</p>`)
        .join('')
})

onMounted(async () => {
    try {
        const res = await api.cmsArticle.getAllPublic({ pid: 2, maxResultCount: 10 })
        articles.value = (res as any).items || []
        if (!article.value) {
            error.value = '未找到对应的协议内容'
        }
    } catch (e: any) {
        error.value = '加载失败，请稍后重试'
        console.error('加载协议内容失败:', e)
    } finally {
        loading.value = false
    }
})
</script>

<style scoped>
.agreement-container {
    max-width: 800px;
    margin: 0 auto;
    padding: 40px 20px;
    min-height: 400px;
}

.agreement-content {
    background: #fff;
    border-radius: 8px;
    padding: 40px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.agreement-content h1 {
    font-size: 24px;
    color: #333;
    text-align: center;
    margin-bottom: 30px;
    padding-bottom: 20px;
    border-bottom: 1px solid #eee;
}

.content-body {
    font-size: 14px;
    line-height: 1.8;
    color: #555;
}

.content-body :deep(p) {
    margin-bottom: 12px;
    text-indent: 2em;
}
</style>
