<template>
    <div>
        <PagedTable
            ref="tableRef"
            :table-sort="'creationTime desc'"
            :show-search="false"
            :fetch-function="fetchData"
            :query-status="1"
        >
            <template #filter>
                <div v-if="tableRef && tableRef.queryForm" class>
                    <queryFilter
                        v-model="tableRef.queryForm.status"
                        :show-all="false"
                        :list="filterList"
                        @change="reload"
                    />
                </div>
            </template>

            <el-table-column label="#" type="index" width="50" align="center" />
            <el-table-column label="用户" align="center">
                <template #default="{ row }">
                    <div class="flex items-center">
                        <img :src="getImgUrl(row.user.headImgUrl, true)" class="size-12" />
                        <div>{{ row.user.name }}</div>
                    </div>
                </template>
            </el-table-column>
            <el-table-column label="操作用户" align="center">
                <template #default="{ row }">
                    <div class="flex items-center">
                        <img :src="getImgUrl(row.creatorUser.headImgUrl, true)" class="size-12" />
                        <div>{{ row.creatorUser.name }}</div>
                    </div>
                </template>
            </el-table-column>
            <el-table-column label="房间" width="100" prop="chan" align="center" />
            <el-table-column label="禁言时间" width="100" prop="EndTime" align="center">
                <template #default="{ row }">
                    <div>{{ dayjs(row.endTime).diff(dayjs(row.creationTime), 'minutes') }} 分钟</div>
                </template>
            </el-table-column>
            <el-table-column label="禁言结束时间" width="180" prop="EndTime" align="center">
                <template #default="{ row }">
                    <div>{{ row.endTime }}</div>
                </template>
            </el-table-column>
            <el-table-column label="创建时间" width="180" prop="creationTime" align="center" />
            <el-table-column
                v-if="tableRef && tableRef.queryForm && tableRef.queryForm.status === 1"
                align="center"
                width="180px"
            >
                <template #default="{ row }">
                    <div class="flex items-center justify-between">
                        <div>
                            <el-button v-permission="'Pages.Administration'" type="danger" @click="onDelete(row)">
                                删除
                            </el-button>
                        </div>
                    </div>
                </template>
            </el-table-column>
        </PagedTable>
    </div>
</template>

<script setup lang="ts">
import api from '@/api'
import PagedTable from '@/components/paged-table/index.vue'
import queryFilter from '@/components/paged-table/queryFilter.vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getImgUrl } from '@/composables'
import dayjs from 'dayjs'
import relativeTime from 'dayjs/plugin/relativeTime'
dayjs.extend(relativeTime)

const _api = api.banedUser

const tableRef = ref<InstanceType<typeof PagedTable>>(null)
onMounted(() => {
    tableRef.value.queryForm = {
        ...tableRef.value.queryForm,
    }
})

const filterList = [
    { id: 1, label: '禁言中' },
    { id: 0, label: '历史记录' },
]

function fetchData(params: any) {
    return _api.getAll(params)
}

function reload() {
    console.log('onView')
    tableRef.value.fetchData()
}

const onDelete = (dto: any) => {
    console.log('onDelete')
    ElMessageBox.confirm('你确定删除吗?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
    })
        .then(async () => {
            await _api.delete({ id: dto.id! }).then(() => {
                reload()
            })
            ElMessage({ type: 'success', message: '删除成功!' })
        })
        .catch(() => {
            ElMessage({ type: 'info', message: '已取消删除' })
        })
}
</script>
