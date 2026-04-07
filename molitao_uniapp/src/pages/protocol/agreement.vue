<template>
    <view class="protocol-page">
        <view v-if="loading" class="loading-container">
            <text>加载中...</text>
        </view>
        <view v-else-if="error" class="error-container">
            <text>{{ error }}</text>
            <button class="retry-btn" @click="loadContent">重试</button>
        </view>
        <view v-else class="protocol-content">
            <view class="title">{{ article?.title || '用户服务协议' }}</view>
            <view class="content-text">
                <text>{{ article?.content || '' }}</text>
            </view>
        </view>
    </view>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import api from '@/utils/api'
import type { CmsArticleDto, IListType } from '@/composables/types'

const loading = ref(true)
const error = ref('')
const article = ref<CmsArticleDto | null>(null)

const loadContent = async () => {
    loading.value = true
    error.value = ''

    try {
        const res = (await api.cmsArticle.getAll({ pid: 10 })) as IListType
        if (res && res.items && res.items.length > 0) {
            // 查找用户服务协议
            article.value = res.items.find((item: CmsArticleDto) => item.title === '用户服务协议') || null

            if (!article.value) {
                error.value = '未找到用户服务协议内容'
            }
        } else {
            error.value = '暂无协议内容'
        }
    } catch (e) {
        console.error('加载协议失败:', e)
        error.value = '加载失败，请稍后重试'
    } finally {
        loading.value = false
    }
}

onMounted(() => {
    loadContent()
})
</script>

<style lang="scss" scoped>
.protocol-page {
    min-height: 100vh;
    background: #ffffff;
    padding: 32rpx;
}

.loading-container,
.error-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 60vh;
    color: #666666;
    font-size: 28rpx;
}

.retry-btn {
    margin-top: 32rpx;
    padding: 16rpx 48rpx;
    font-size: 28rpx;
    background: #f4835a;
    color: #ffffff;
    border: none;
    border-radius: 8rpx;
}

.protocol-content {
    max-width: 100%;
}

.title {
    font-size: 40rpx;
    font-weight: bold;
    color: #333333;
    text-align: center;
    margin-bottom: 48rpx;
}

.content-text {
    font-size: 28rpx;
    color: #666666;
    line-height: 1.8;
    white-space: pre-wrap;
    word-break: break-all;
}
</style>

<route lang="json">
{
    "style": {
        "navigationBarTitleText": "用户服务协议"
    }
}
</route>
