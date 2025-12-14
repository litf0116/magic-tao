<template>
    <div class="mt-2 md:mt-0 grid gap-3 grid-cols-1 md:grid-cols-[auto_1fr]">
        <div class="flex flex-col md:h-390px items-center">
            <div @click.stop="toLobby">
                <img class="w-full cursor-pointer" src="../../assets/jyz.png" />
            </div>
            <div @click.stop="toAuction">
                <img class="w-full cursor-pointer" src="../../assets/pmh.png" />
            </div>
        </div>
        <div class="flex-1">
            <el-carousel v-if="articleList.length" height="auto" indicator-position="inside">
                <el-carousel-item v-for="(x, k) in articleList" :key="k" class="h-250px md:h-390px">
                    <div class="w-full h-full flex flex-center text-4xl font-bold text-white">
                        <img :src="x.titleImageUrl" class="w-full h-full object-cover" />
                        <!-- {{ x.title }} -->
                    </div>
                </el-carousel-item>
            </el-carousel>
        </div>
    </div>

    <!-- 广告展示区域 -->
    <AdvertisementBanner />
</template>

<script setup lang="ts">
import api from '@/api'
import { GetTypeList } from '@/api/advertisingSpaceAPI'
import { CmsArticleDto } from '@/api/appService'
import AdvertisementBanner from './components/AdvertisementBanner.vue'

const router = useRouter()

const toLobby = () => {
    router.push({ path: 'forum/tradingPost' })
}

const toAuction = () => {
    router.push({ name: 'auction' })
}

onMounted(() => {
    fetchCmsData()
})
const articleList = ref<CmsArticleDto[]>([])

function fetchCmsData() {
    api.cmsArticle.getAllPublic({ pid: 1 }).then((res) => {
        articleList.value = res.items
    })
}
</script>
