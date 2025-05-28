<template>
    <div class="flex w-full">
        <div class="p-2 flex-1">
            <lineChart
                v-if="chatList1"
                :smooth="true"
                :line-color="['#4F46E5']"
                :area-color="['#C7D2FE']"
                :chart-data="chatList1"
                :title="['拍卖金额']"
                :y-key="'label'"
                :x-key="'count'"
            />
        </div>
        <div class="p-2 flex-1">
            <lineChart
                v-if="chatList2"
                :smooth="true"
                :line-color="['#be185d']"
                :area-color="['#f472b6']"
                :chart-data="chatList2"
                :title="['成交数量']"
                :y-key="'label'"
                :x-key="'count'"
            />
        </div>
    </div>
    <div class="flex">
        <div class="p-2 flex-1">
            <lineChart
                v-if="chatList3"
                :smooth="true"
                :line-color="['#a16207']"
                :area-color="['#fcd34d']"
                :chart-data="chatList3"
                :title="['出价次数']"
                :y-key="'label'"
                :x-key="'count'"
            />
        </div>
        <div class="p-2 flex-1"></div>
    </div>
</template>

<script setup lang="ts">
import api from '@/api'
import LineChart from './LineChart.vue'

const chatList1 = ref(null)
const chatList2 = ref(null)
const chatList3 = ref(null)

onMounted(() => {
    api.auctionItem.dateAnlayse({}).then((res) => {
        chatList1.value = res
    })
    api.auctionItem.dateAnlayse2({}).then((res) => {
        chatList2.value = res
    })
    api.bidHistory.dateAnlayse({}).then((res) => {
        chatList3.value = res
    })
})
</script>
