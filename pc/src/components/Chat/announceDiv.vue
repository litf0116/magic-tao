<template>
    <div class="m-2" @click="announceDialogRef!.show(true)">
        <div class="text-true-gray-700 mb-2 flex items-center justify-between cursor-pointer">
            <div class="text-[#82615f] text-22px font-700">公告</div>
            <div class="flex flex-center text-[#82615f]">
                <div class="text-16px">更多</div>
                <div class="i-mdi:chevron-right size-5"></div>
            </div>
        </div>
        <div class="text-sm text-[#82615f] line-clamp-2 text-14px">
            {{ item?.content }}
        </div>
    </div>
    <announce-dialog ref="announceDialogRef" :category-id="props.categoryId" />
</template>

<script setup lang="ts" name="announceDiv">
import { ElMessageBox, ElMessage } from 'element-plus'
import { AnnounceDto } from '@/api/appService'
import announceDialog from './announceDialog.vue'
import api from '@/api'
import DOMPURIFY from 'dompurify'

const announceDialogRef = ref<InstanceType<typeof announceDialog> | null>(null)
const item = ref<AnnounceDto | null>(null)

const props = defineProps({
    categoryId: {
        type: Number,
        required: true,
        default: 0,
    },
})

watch(
    () => props.categoryId,
    (val: number) => {
        api.announce.getLatest({ id: val }).then((res) => {
            item.value = res
        })
    }
)

onMounted(() => {
    //获取公告
    api.announce.getLatest({ id: props.categoryId }).then((res) => {
        item.value = res
        //获取通知公告
        var bulletinInfo: any = localStorage.getItem('bulletin_' + res.id)
        bulletinInfo = bulletinInfo ? JSON.parse(bulletinInfo) : null
        var html = ''
        if (bulletinInfo == null || bulletinInfo.id != res.id) {
            if (res.imageUrl) {
                // 使用 DOMPurify 消毒图片URL和内容，防止 XSS
                const sanitizedImageUrl = DOMPURIFY.sanitize(res.imageUrl, { USE_PROFILES: { html: true } })
                const sanitizedContent = DOMPURIFY.sanitize(res.content || '', { USE_PROFILES: { html: true } })
                html = `<img style="width: 100%;height: 300px;" src="${sanitizedImageUrl}">${sanitizedContent}`
            } else {
                html = DOMPURIFY.sanitize(res.content || '', { USE_PROFILES: { html: true } })
            }

            ElMessageBox.alert(html, '公告', {
                confirmButtonText: '确定',
                dangerouslyUseHTMLString: true,
                customClass: 'msgbox',
                callback: () => {
                    localStorage.setItem('bulletin_' + res.id, JSON.stringify(res))
                },
            })
        }
    })
})
</script>
<style>
.msgbox {
    max-width: 600px;
}
</style>
