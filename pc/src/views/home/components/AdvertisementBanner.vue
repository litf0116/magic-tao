<template>
    <div class="advertisement-banner">
        <div class="banner-content">
            <div class="ad-container">
                <div
                    v-for="(item, index) in advertisementList"
                    :key="index"
                    class="ad-item"
                    @click="openLink(item.url)"
                >
                    <!-- 处理占位符图片 -->
                    <div v-if="!item.imageUrl" class="ad-placeholder"></div>
                    <img v-else :src="item.imageUrl" :alt="item.title" class="ad-image" @error="handleImageError" />
                    <!-- 只有有图片时才显示标题 -->
                    <div v-if="item.imageUrl" class="ad-title">{{ item.title }}</div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { GetTypeList } from '@/api/advertisingSpaceAPI'

interface AdvertisementItem {
    id?: number
    title: string
    imageUrl: string
    url: string
}

// 创建6个空的广告位占位符
const createPlaceholderAds = (): AdvertisementItem[] => {
    return Array.from({ length: 6 }, (_, index) => ({
        id: index,
        title: '', // 空广告位不需要标题
        imageUrl: '',
        url: '',
    }))
}

// 初始化时设置6个占位符
const advertisementList = ref<AdvertisementItem[]>(createPlaceholderAds())

onMounted(async () => {
    await fetchAdvertisements()
})

const fetchAdvertisements = async () => {
    try {
        const res = await GetTypeList(1)
        if (res.data && res.data.items) {
            // 获取真实数据
            const realAds = res.data.items

            // 创建新的数组，保持6个位置
            const updatedAds: AdvertisementItem[] = []

            // 用真实数据替换前N个占位符
            for (let i = 0; i < 6; i++) {
                if (i < realAds.length) {
                    updatedAds.push(realAds[i])
                } else {
                    // 如果真实数据不足6个，剩余位置使用占位符
                    updatedAds.push({
                        id: i,
                        title: '', // 空广告位不需要标题
                        imageUrl: '',
                        url: '',
                    })
                }
            }

            // 更新广告列表
            advertisementList.value = updatedAds
        }
    } catch (error) {
        console.error('获取广告数据失败:', error)
        // 获取失败时继续使用占位符
    }
}

const openLink = (url: string) => {
    // 只有真实广告（有URL）才能点击打开
    if (url && url !== '') {
        window.open(url, '_blank', 'noopener,noreferrer')
    }
}

// 处理图片加载失败
const handleImageError = (event: Event) => {
    const target = event.target as HTMLImageElement
    // 隐藏失败的图片，显示占位符
    target.style.display = 'none'
    const placeholder = target.previousElementSibling as HTMLElement
    if (placeholder && placeholder.classList.contains('ad-placeholder')) {
        placeholder.style.display = 'block'
    }
}
</script>

<style lang="scss" scoped>
.advertisement-banner {
    /* Rectangle 31 样式 */
    box-sizing: border-box;
    width: 1300px;
    height: 204px;
    background: #fff2e8;
    border: 14px solid #ae6f4d;
    border-radius: 20px;

    /* 响应式调整 */
    @media (max-width: 1400px) {
        width: calc(100vw - 40px);
        height: auto;
        min-height: 180px;
    }

    @media (max-width: 768px) {
        border-width: 8px;
        height: auto;
        min-height: 150px;
        width: calc(100% - 40px);
    }

    .banner-content {
        width: 100%;
        height: 100%;
        display: flex;
        align-items: center;
        justify-content: center;
        padding: 30px 0;

        @media (max-width: 768px) {
            padding: 15px 0;
        }
    }

    .ad-container {
        /* 应用提供的Frame 78样式 */
        display: flex;
        flex-direction: row;
        align-items: flex-start;
        padding: 0 30px;
        gap: 16px;
        width: 100%;
        height: 154px;

        @media (max-width: 1400px) {
            height: auto;
            min-height: 154px;
            flex-wrap: wrap;
            justify-content: center;
        }

        @media (max-width: 768px) {
            padding: 0 15px;
            gap: 10px;
            height: auto;
        }
    }

    .ad-item {
        position: relative;
        cursor: pointer;
        transition: all 0.3s ease;
        flex: none;
        flex-grow: 1;

        /* 应用提供的Rectangle 32样式 */
        width: 193.33px;
        height: 114px;
        background: #f3d9b3;
        border-radius: 10px;
        overflow: hidden;

        @media (max-width: 1400px) {
            width: calc(20% - 12.8px);
            max-width: 180px;
            height: 106px;
        }

        @media (max-width: 1200px) {
            width: calc(33.33% - 10.67px);
            max-width: 160px;
        }

        @media (max-width: 768px) {
            width: calc(50% - 5px);
            height: 90px;
            max-width: 140px;
        }

        @media (max-width: 480px) {
            width: 100%;
            height: 100px;
            max-width: 200px;
        }

        &:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
            background: #e5c299;
        }
    }

    .ad-image {
        width: 100%;
        height: 100%;
        object-fit: cover;
        border-radius: 6px;
    }

    .ad-placeholder {
        width: 100%;
        height: 100%;
        background: #f3d9b3;
        border-radius: 6px;
        display: none; /* 默认隐藏，图片加载失败时显示 */
    }

    .ad-title {
        position: absolute;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        color: #fff;
        font-weight: bold;
        font-size: 14px;
        text-align: center;
        text-shadow: 0 1px 3px rgba(0, 0, 0, 0.5);
        padding: 5px 10px;
        background: rgba(0, 0, 0, 0.3);
        border-radius: 4px;

        @media (max-width: 768px) {
            font-size: 12px;
            padding: 3px 6px;
        }
    }
}
</style>
