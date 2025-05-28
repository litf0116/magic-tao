<template>
    <div class="flex flex-col space-y-4 bg-[#faf1f0] p-4">
        <div v-for="x in list" :key="x.id" class="relative bg-white rounded-lg px-2 py-4 shadow">
            <div class="flex">
                <div class="flex-1 text-true-gray-700 text-sm" v-html="getHtml(x.content)"></div>
                <image
                    v-if="x.imageUrl"
                    class="size-24 ml-1"
                    :src="getImgUrl(x.imageUrl, true)"
                    mode="aspectFill"
                    @click="show(x)"
                />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import type { AnnounceDto } from '@/composables/types'
import api from '@/utils/api'
import { onLoad } from '@dcloudio/uni-app'
import { getImgUrl } from '@/composables'

const id = ref(0)

onLoad((query) => {
    id.value = parseInt(query!.id as string)
    fetchList()
})
const list = ref<AnnounceDto[]>([])

function fetchList() {
    api.announce.getAll({ pid: id.value }).then((res) => {
        // console.log(res)
        list.value = res.items!
    })
}

function show(item: AnnounceDto) {
    uni.previewImage({
        urls: [item.imageUrl],
    })
}

function getHtml(content: string) {
    return content.replaceAll('\n', '<br>')
}
</script>
<route lang="json">
{
    "style": { "navigationBarTitleText": "公告" }
}
</route>
