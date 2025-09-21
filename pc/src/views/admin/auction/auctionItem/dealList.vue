<template>
    <div>
        <PagedTable ref="tableRef" :table-sort="'dealTime desc'" :fetch-function="fetchData">
            <el-table-column label="ID" prop="id" width="80" align="center" />
            <el-table-column label="标题" prop="name" width="150" />
            <el-table-column label="图片" prop="imageUrl" width="100" :formatter="imagePreview"> </el-table-column>
            <!-- <el-table-column label="邮箱" prop="emailAddress" /> -->
            <!-- <el-table-column label="状态" prop="status" width="80" /> -->
            <el-table-column label="卖家信息" prop="sellerInfo" width="200" />
            <el-table-column label="成交时间" prop="dealTime" width="180" />
            <el-table-column label="成交价格" prop="finalPrice" width="100" />
            <el-table-column label="成交用户" prop="dealUserName">
                <template #default="{ row }">
                    <div>
                        用户编号：<b>{{ row.dealUserId }}</b>
                    </div>
                    <div>
                        用户昵称：<b class="mr-2">{{ row.dealUserName }}</b>
                        <el-button
                            size="small"
                            type="primary"
                            href="javascript:void(0)"
                            @click="chat(row.dealUserId, row.dealUserName)"
                            >打开聊天</el-button
                        >
                    </div>
                </template>
            </el-table-column>
        </PagedTable>
        <Edit ref="editRef" @onSaved="reload" />
    </div>
</template>

<script setup lang="ts">
import api from '@/api'
import Edit from '@/components/Chat/editAuctionItem.vue'
import PagedTable from '@/components/paged-table/index.vue'
import { User } from '@element-plus/icons-vue'
const { imagePreview, utcToLocalFull, utcToLocalDay } = useFormatter()
import { ElMessage, ElMessageBox } from 'element-plus'
import { convertImageUrl } from '@/utils/imageUrlConverter'

const _api = api.auctionItem

const tableRef = ref<InstanceType<typeof PagedTable>>(null)
const editRef = ref<InstanceType<typeof Edit>>(null)
onMounted(() => {})

function fetchData(params: any) {
    params = {
        ...params,
        status: 4,
    }

    return _api.getAll(params)
}

function reload() {
    console.log('onView')
    tableRef.value.fetchData()
}

const router = useRouter()

function chat(userId: number, name: string) {
    console.log('chat', userId, name)

    const link = router.resolve({
        path: `/chat/index/privateChat/${userId}`,
        query: { name: name, avatar: 'https://image.molitao.top/avater.png' },
    })
    console.log(link)
    window.open(link.href, '_blank')
}
</script>
