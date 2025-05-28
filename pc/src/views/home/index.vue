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
	<div style="display: flex;margin-top: 10px;justify-content: center;">
		<div style="width: 200px; height: 150px;margin: 3px;position: relative;cursor: pointer;"
			v-for="(item, index) in advertisingSpaceList" @click="openNewPage(item.url)">
			<img style="width: 100%; height: 100%;" :src="item.imageUrl" />
			<div style="position:absolute;top:50%;left:50%;transform:translate(-50%,-50%);color:#fff;">
				{{ item.title }}
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import api from '@/api'
import { GetTypeList } from '@/api/advertisingSpaceAPI'
import { CmsArticleDto } from '@/api/appService'

const router = useRouter()
const advertisingSpaceList = ref([])

const toLobby = () => {
	router.push({ path: 'forum/tradingPost' })
}

const toAuction = () => {
	router.push({ name: 'auction' })
}
onMounted(() => {
	getData();
	fetchCmsData()
})
const articleList = ref<CmsArticleDto[]>([])

function fetchCmsData() {
	api.cmsArticle.getAllPublic({ pid: 1 }).then((res) => {
		articleList.value = res.items;
	})
}
//获取广告位数据
const getData = async () => {
	var res = await GetTypeList(1);
	if (res.data) {
		advertisingSpaceList.value = res.data.items
	}
}
// 跳转地址
const openNewPage = (url) => {
	// 打开外部链接
	window.open(url, '_blank', 'noopener,noreferrer')
}
</script>
