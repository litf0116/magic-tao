<template>
	<view class="p-4">
		<z-paging ref="paging" v-model="list" :auto-scroll-to-top-when-reload="false" :use-page-scroll="true"
			@query="queryList">
			<view v-for="(item, k) in list" :key="k"
				class="flex items-center justify-between border-0 border-solid border-b-1px border-true-gray-200 p-2">
				<view class="text-gray-500">
					{{ item.successTime }} {{ item.type }}
				</view>
				<view class="text-lg" :class="item.amount > 0 ? 'text-green-500' : 'text-red-500'">
					{{ item.amount > 0 ? '+' : '' }}{{ item.amount }}
				</view>
			</view>
		</z-paging>
	</view>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { onLoad, onPullDownRefresh } from '@dcloudio/uni-app'
import type { ZPagingInstance } from 'z-paging'
import api from '@/utils/api'

const paging = ref<ZPagingInstance | null>(null)
const list = ref<any[]>([])
let eventChannel: any = null

onLoad(() => {
	const pages = getCurrentPages()
	const page = pages[pages.length - 1] as any
	eventChannel = page.getOpenerEventChannel()
})

onPullDownRefresh(async () => {
	try {
		await paging.value?.reload()
	} catch (error) {
		console.error('刷新失败:', error)
	} finally {
		uni.stopPullDownRefresh()
	}
})

async function queryList(pageNo: number, pageSize: number) {
	try {
		const res = await api.UserDepositLog.GetMyAll({
			sorting: 'creationTime desc',
			skipCount: (pageNo - 1) * 10,
			maxResultCount: pageSize,
		})
		paging.value?.complete(res.items)
	} catch (error) {
		console.error('查询失败:', error)
		paging.value?.complete(false)
	}
}
</script>