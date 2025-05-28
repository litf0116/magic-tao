<template>
    <el-dialog v-model="dialogVisible" title="公告" width="500" draggable destroy-on-close append-to-body>
        <div class="flex flex-col space-y-4 max-h-50vh overflow-y-scroll">
            <div v-for="x in list" :key="x.id" class="relative">
                <div v-if="userStore.isChatAdmin" class="absolute top-0 right-0">
                    <el-button type="text" @click="editRef!.show(true, x.id!, x.categoryId!)">编辑</el-button>
                    <el-button type="text" @click="editRef!.show(true, 0, x.categoryId!)">新增</el-button>
                    <el-button type="text" @click="onDelete(x.id)">删除</el-button>
                </div>

                <div class="bg-gray-100 rounded px-4 py-6 flex">
                    <div class="flex-1" v-html="getHtml(x.content)"></div>
                    <el-image
                        v-if="x.imageUrl"
                        :src="x.imageUrl + '!w300'"
                        class="size-24"
                        :zoom-rate="1.2"
                        :max-scale="7"
                        :min-scale="0.2"
                        :preview-src-list="[x.imageUrl]"
                        :initial-index="4"
                        fit="cover"
                    />
                </div>
            </div>
        </div>
        <template #footer>
            <!-- <div class="dialog-footer">
                <el-button @click="dialogVisible = false">Cancel</el-button>
                <el-button type="primary" @click="dialogVisible = false"> Confirm </el-button>
            </div> -->
        </template>
        <edit-announce ref="editRef" @onSaved="fetchList" />
    </el-dialog>
</template>

<script setup lang="ts" name="announceDialog">
import { AnnounceDto } from '@/api/appService'
import editAnnounce from './editAnnounce.vue'
import api from '@/api'
import { ElMessageBox } from 'element-plus'
const userStore = useUserStore()

const editRef = ref<InstanceType<typeof editAnnounce> | null>(null)

const props = defineProps({
    categoryId: {
        type: Number,
        required: true,
        default: 0,
    },
})

const list = ref<AnnounceDto[]>([])

const dialogVisible = ref(false)

const show = (e: boolean) => {
    dialogVisible.value = e
    if (e) {
        fetchList()
    }
}

function onDelete(id: number) {
    ElMessageBox.confirm('你确定删除吗?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
    }).then(async () => {
        api.announce.delete({ id }).then(() => {
            fetchList()
        })
    })
}

function fetchList() {
    api.announce.getAllPublic({ pid: props.categoryId }).then((res) => {
        // console.log(res)
        list.value = res.items!
    })
}
function getHtml(content: string) {
    return content.replaceAll('\n', '<br />')
}

defineExpose({
    show,
})
</script>
