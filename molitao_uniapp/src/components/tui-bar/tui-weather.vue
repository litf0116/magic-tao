<template>
    <view class="bg-white px-4 py-2 text-xs rounded font-bold flex justify-between items-center">
        <view> {{ appStore.adm2 }}·{{ appStore.city }} </view>
        <image :src="appStore.weatherIcon" class="mx-2 w-6 h-6" />
        <view>{{ appStore.weather }}</view>
    </view>
</template>

<script setup lang="ts">
import { onLoad } from '@dcloudio/uni-app'

const appStore = useAppStore()

onLoad(async () => {
    console.log('weather onLoad')
    await appStore.getLocation().then(async (res) => {
        // console.log(res)
        await appStore.getCity(res.latitude, res.longitude)
        await appStore.getWeather(res.latitude, res.longitude)
    })
})
</script>
