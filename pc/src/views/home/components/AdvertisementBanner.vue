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
                    <img :src="item.imageUrl" :alt="item.title" class="ad-image" />
                    <div class="ad-title">{{ item.title }}</div>
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

const advertisementList = ref<AdvertisementItem[]>([])

onMounted(async () => {
    await fetchAdvertisements()
})

const fetchAdvertisements = async () => {
    try {
        const res = await GetTypeList(1)
        if (res.data && res.data.items) {
            advertisementList.value = res.data.items
        }
    } catch (error) {
        console.error('获取广告数据失败:', error)
    }
}

const openLink = (url: string) => {
    if (url) {
        window.open(url, '_blank', 'noopener,noreferrer')
    }
}
</script>

<style lang="scss" scoped>
.advertisement-banner {
    width: 100%;
    margin-top: 20px;
    display: flex;
    justify-content: center;

    .banner-content {
        width: 1300px;
        max-width: 95vw;

        /* 应用提供的样式 */
        box-sizing: border-box;
        position: relative;
        width: 1300px;
        height: 204px;

        background-image: url('@/assets/images/ad_background.png');
        background-size: cover;
        background-position: center;
        background-repeat: no-repeat;
        border: 14px solid #AE6F4D;
        border-radius: 20px;

        /* 响应式调整 */
        @media (max-width: 1400px) {
            width: 100%;
            max-width: 1200px;
            height: auto;
            min-height: 180px;
        }

        @media (max-width: 768px) {
            border-width: 8px;
            height: auto;
            min-height: 150px;
        }
    }

    .ad-container {
        width: 100%;
        height: 100%;
        display: flex;
        justify-content: center;
        align-items: center;
        gap: 20px;
        padding: 20px;

        @media (max-width: 768px) {
            flex-wrap: wrap;
            gap: 10px;
            padding: 10px;
        }
    }

    .ad-item {
        position: relative;
        cursor: pointer;
        transition: transform 0.3s ease;

        /* 响应式尺寸 */
        width: 200px;
        height: 150px;

        @media (max-width: 1400px) {
            width: 180px;
            height: 135px;
        }

        @media (max-width: 768px) {
            width: 150px;
            height: 112px;
        }

        &:hover {
            transform: scale(1.05);
        }
    }

    .ad-image {
        width: 100%;
        height: 100%;
        object-fit: cover;
        border-radius: 6px;
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