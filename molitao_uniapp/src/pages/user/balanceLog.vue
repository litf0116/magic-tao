<template>
	<view class="p-4">
		<!-- <z-paging
            ref="paging"
            v-model="list"
            :auto-scroll-to-top-when-reload="false"
            :use-page-scroll="true"
            @query="queryList"
        >
            <view
                v-for="(item, k) in list"
                :key="k"
                class="flex items-center justify-between border-0 border-solid border-b-1px border-true-gray-200 p-2"
            >
                <view class="text-gray-500">{{ item.successTime }} {{ item.type }}</view>
                <view class="text-lg" :class="[item.amount > 0 ? 'text-green-500' : 'text-red-500']">
                    {{ item.amount < 0 ? '' : '+' }}
                    {{ item.amount }}</view
                >
            </view>
        </z-paging> -->
	</view>
</template>
<script setup lang="ts">
import { onLoad, onPullDownRefresh } from '@dcloudio/uni-app'
import api from '@/utils/api'

let eventChannel: any = null
onLoad(() => {
	const pages = getCurrentPages()
	const page: any = pages[pages.length - 1]
	eventChannel = page.getOpenerEventChannel()
	console.log('options', eventChannel)
})

const paging = ref<ZPagingInstance | null>(null)
const list = ref<any[]>([])

onPullDownRefresh(async () => {
	paging.value!.reload().catch(() => { })
	uni.stopPullDownRefresh()
})

function queryList(pageNo: number, pageSize: number) {
	console.log('queryList', pageNo, pageSize)
	api.UserBalanceLog.GetMyAll({
		sorting: 'creationTime desc',
		skipCount: (pageNo - 1) * 10,
		maxResultCount: pageSize,
	})
		.then((res) => {
			// 将请求结果通过complete传给z-paging处理，同时也代表请求结束，这一行必须调用
			paging.value!.complete(res.items)
		})
		.catch((res) => {
			// 如果请求失败写this.$refs.paging.complete(false)，会自动展示错误页面
			// 注意，每次都需要在catch中写这句话很麻烦，z-paging提供了方案可以全局统一处理
			// 在底层的网络请求抛出异常时，写uni.$emit('z-paging-error-emit');即可
			paging.value!.complete(false)
		})
}
</script>
