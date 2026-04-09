<template>
    <view class="main" style="padding-bottom: 80px">
        <view class="wrap">
            <view class="header">
                <image
                    class="logo2"
                    :src="convertImageUrl('https://image.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png')"
                />
            </view>
            <view class="content px-4">
                <view class="flex flex-col">
                    <!-- #ifndef MP-WEIXIN -->
                    <!-- H5/APP 显示交易站入口 -->
                    <image
                        v-if="appFeatureStore.getShowTradingPost()"
                        class="w-full h-270rpx"
                        :src="convertImageUrl('https://image.molitao.top/banners/jyz.png')"
                        @tap="gotoTradingPost"
                    />
                    <!-- #endif -->
                    <!-- 秒杀场入口：仅当用户有权限时显示 -->
                    <image
                        v-if="showAuctionEntrance"
                        class="mt-1 w-full h-270rpx"
                        :src="convertImageUrl('https://image.molitao.top/banners/pmh.png')"
                        @tap="Goto.auction()"
                    />
                </view>

                <!-- 功能模块区域 -->
                <view class="my-4 flex items-center">
                    <view class="h-3 w-4px mr-2 bg-[#ccc] rounded-full"></view>
                    <text>常用工具</text>
                </view>
                <view class="myCard py-4 grid grid-cols-2 gap-4 mb-4 text-[#171717]">
                    <view class="flex flex-center zoom-in py-3" @tap="gotoPetCalculator">
                        <view class="mr-3 bg-[#f6f6f6] size-12 rounded-full flex flex-center">
                            <view class="size-7 i-icon-park-outline:dog"></view>
                        </view>
                        <text class="text-sm font-500">宠物算档器</text>
                    </view>
                    <view class="flex flex-center py-3" @tap="onFeatureDeveloping">
                        <view class="mr-3 bg-[#f6f6f6] size-12 rounded-full flex flex-center">
                            <view class="size-7 i-icon-park-outline:calculator"></view>
                        </view>
                        <text class="text-sm font-500">升级计算</text>
                    </view>
                    <view class="flex flex-center py-3" @tap="onFeatureDeveloping">
                        <view class="mr-3 bg-[#f6f6f6] size-12 rounded-full flex flex-center">
                            <view class="size-7 i-icon-park-outline:map"></view>
                        </view>
                        <text class="text-sm font-500">地图导航</text>
                    </view>
                    <view class="flex flex-center py-3" @tap="onFeatureDeveloping">
                        <view class="mr-3 bg-[#f6f6f6] size-12 rounded-full flex flex-center">
                            <view class="size-7 i-icon-park-outline:more-app"></view>
                        </view>
                        <text class="text-sm font-500">敬请期待</text>
                    </view>
                </view>

                <view v-if="appFeatureStore.getShowBanner()" class="mt-2 w-full">
                    <uv-swiper
                        :height="200"
                        :interval="5000"
                        :list="list"
                        indicator
                        indicatorMode="line"
                        circular
                        :display-multiple-items="0"
                    ></uv-swiper>
                </view>

                <view
                    v-if="appFeatureStore.getShowBanner() && advertisingSpaceList.length > 0"
                    class="advertisingSpace"
                >
                    <div v-for="(item, index) in advertisingSpaceList" :key="index" class="advertisingSpace-item">
                        <image
                            class="logo2"
                            :src="convertImageUrl(item.imageUrl, false)"
                            mode="aspectFill"
                            @error="handleImageError(index)"
                        />
                        <div
                            style="
                                position: absolute;
                                top: 50%;
                                left: 50%;
                                transform: translate(-50%, -50%);
                                color: #fff;
                            "
                        >
                            {{ item.title || item.name || '' }}
                        </div>
                    </div>
                </view>
                <view v-else class="text-center text-gray-400 py-4 text-sm">
                    <!-- 暂无广告位信息 -->
                </view>
            </view>
        </view>
    </view>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onBeforeUnmount } from 'vue'
import type { CmsArticleDto } from '@/composables/types'
import api from '@/utils/api'
import { Goto } from '@/composables/goto'
import { getImgUrl } from '@/composables'
import { convertImageUrl } from '@/utils/imageUrlConverter'
import { onShow, onPullDownRefresh, onShareAppMessage, onShareTimeline, onLoad } from '@dcloudio/uni-app'

const appStore = useAppStore()
const userStore = useUserStore()
const chatStore = useChatStore()
const appFeatureStore = useAppFeatureStore()

const { navTo } = useTo()

const showAuctionEntrance = computed(() => {
    return appFeatureStore.getShowAuction()
})

// 跳转到交易站
const gotoTradingPost = () => {
    emit('refreshCurrentVal', 2)
}

// 跳转到宠物算档器
const gotoPetCalculator = () => {
    navTo('/pages/tools/petCalculator')
}

// 功能开发中提示
const onFeatureDeveloping = () => {
    uni.showToast({
        title: '功能开发中',
        icon: 'none',
    })
}

//广告位信息
const advertisingSpaceList: any = ref([])
const emit = defineEmits(['refreshCurrentVal'])
onMounted(() => {
    fetchCmsData()
    advertisingSpace()
    appFeatureStore.loadFeatureSwitch()
})
//获取广告位列表
const advertisingSpace = () => {
    api.AdvertisingSpace.GetAdvertisingSpaceAll(1)
        .then((res: any) => {
            console.log('广告位数据响应:', res)
            if (res && res.items) {
                advertisingSpaceList.value = res.items
                console.log('广告位列表:', res.items)
            } else if (Array.isArray(res)) {
                advertisingSpaceList.value = res
                console.log('广告位列表(数组):', res)
            } else {
                console.warn('广告位数据格式异常:', res)
                advertisingSpaceList.value = []
            }
        })
        .catch((err: any) => {
            console.error('获取广告位数据失败:', err)
            advertisingSpaceList.value = []
        })
}

// 处理图片加载错误
const handleImageError = (index: number) => {
    console.warn(`广告位图片加载失败，索引: ${index}`)
}
const list = computed(() => {
    return articleList.value.map((item) => {
        return {
            url: item.titleImageUrl,
        }
    })
})

const articleList = ref<CmsArticleDto[]>([])

function fetchCmsData() {
    api.cmsArticle.getAll({ pid: 1 }).then((res) => {
        nextTick(() => {
            articleList.value = [...res.items!]
        })
    })
}

watch(
    () => userStore.user, // 监听的数据
    async (val) => {}
)

onShareAppMessage(() => {
    return {
        title: '魔力淘',
        path: '/pages/tabbar/index',
    }
})

onShareTimeline(() => {
    return {
        title: '魔力淘',
    }
})

const font = ref({ size: '2em' })
</script>
<style>
.myCard {
    background: #fff;
    border-radius: 16rpx;
}

.advertisingSpace {
    display: flex;
    flex-wrap: wrap;
    /* 设置图片之间的间距 */
    padding: 10px;
    /* 设置外边距 */
}

.advertisingSpace-item {
    width: 48%;
    padding: 3px;
    position: relative;
}

.advertisingSpace-item image {
    width: 100%;
    height: 150px;
}
</style>
<style>
.text {
    color: v-bind(color);
    font-size: v-bind('font.size');
}
</style>

<style lang="scss" scoped>
.main {
    display: flex;
    justify-content: center;
}

.wrap {
    @apply flex flex-col items-center relative;

    .header {
        @apply w-full text-center h-[160px] flex flex-col justify-end items-center;
        background: url(https://image.molitao.top/20250330/04j40l4ynlbh3v3h4bgfe7j2pxiqjg8d.png) no-repeat
            center -60rpx / cover;

        .logo2 {
            @apply mb-1 w-462rpx h-212rpx;
        }
    }

    .content {
        @apply w-full relative mt-12rpx w-[90vw];
        background: url(https://image.molitao.top/molitao/2025-03-30/upload_qxgt8fo3iymdi0heth3rnqipc83rzawn.png)
            repeat-y center center / 100% 100%;
    }

    .content::before {
        content: '';
        @apply block absolute w-full h-18rpx -top-18rpx left-0 right-0;
        background: url(https://image.molitao.top/molitao/2025-03-30/upload_iw2aq9rsovog4lr3v036irwm90nyos20.png)
            no-repeat center center / 100% 100%;
    }

    .content::after {
        content: '';
        @apply block absolute w-full h-18rpx -bottom-18rpx left-0 right-0;
        background: url(https://image.molitao.top/molitao/2025-03-30/upload_to45oxex09l2uu1ltntj09n6z1x4y0df.png)
            no-repeat center center / 100% 100%;
    }
}
</style>

<route type="home" lang="json">
{
    "layout": "main",
    "style": {
        "navigationBarTitleText": "魔力淘"
    }
}
</route>
