<template>
    <view class="bg-white p-4">
        <uv-search v-model="keyword" :clearabled="true" placeholder="请输入姓名或帐号" @search="fetchData"></uv-search>
    </view>

    <view>
        <button @tap="goDetail({ id: 0 })">新建用户</button>
    </view>
    <view class="p-4">
        <view role="list" class="bg-white p-4 rounded-2 shadow-lg">
            <template v-if="list.length === 0">
                <view class="text-center text-gray-500">暂无数据</view>
            </template>
            <template v-else>
                <template v-for="user in list" :key="user.id">
                    <view class="flex py-2 justify-between" @tap="goDetail(user)">
                        <view class="font-semibold text-gray-900 flex items-center">
                            <text>{{ user.name }}</text>
                            <view
                                v-if="!user.isActive"
                                class="ml-2 text-xs bg-red-100 text-rose-500 rounded px-1 py-[2px]"
                                >已禁用</view
                            >
                        </view>
                        <view class="flex items-center">
                            <view class="mr-2 text-gray-500">帐号:{{ user.userName }}</view>
                            <view class="size-4 text-blue-500 i-icon-park-outline:right"></view>
                        </view>
                    </view>
                    <uv-divider />
                </template>
                <view class="text-center text-gray-500">共{{ totalCount }}条数据</view>
            </template>
        </view>
    </view>
</template>
<script setup lang="ts">
import api from '@/utils/api'
import { onLoad, onPullDownRefresh } from '@dcloudio/uni-app'
onLoad(() => {
    fetchData()
})

const list = ref([] as any[])

const pageSize = 9999
const page = ref(1)
const totalCount = ref(999)
const keyword = ref('')

onPullDownRefresh(async () => {
    keyword.value = ''
    await fetchData()
    uni.stopPullDownRefresh()
})

function fetchData() {
    if (page.value > Math.ceil(totalCount.value / pageSize)) {
        return
    }
    api.user
        .getAll({
            maxResultCount: pageSize,
            skipCount: (page.value - 1) * pageSize,
            sorting: 'id asc',
            keyword: keyword.value,
        })
        .then((res: any) => {
            console.log(res)
            list.value = res.items
            totalCount.value = res.totalCount
        })
}

function goDetail(user: any) {
    uni.navigateTo({
        url: `/pages/user/edit?id=${user.id}`,
        events: {
            refresh: () => {
                fetchData()
            },
        },
    })
}
</script>
<style>
.uv-icon {
    @apply mr-2;
}

.uv-list-item__container {
    @apply !py-2;
}
</style>
