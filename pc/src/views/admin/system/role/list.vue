<template>
    <div>
        <PagedTable ref="tableRef" :table-sort="'id asc'" :show-search="false" :fetch-function="fetchData">
            <el-table-column label="#" type="index" width="50" align="center" />
            <el-table-column label="ID" width="80" prop="id" align="center" />
            <el-table-column label="系统名称" width="180" prop="name" align="center" />
            <el-table-column label="显示名称" width="180" prop="displayName" align="center" />

            <el-table-column align="center" width="180px">
                <template #default="{ row }">
                    <div class="flex items-center justify-between"></div>
                </template>
            </el-table-column>
        </PagedTable>
    </div>
</template>

<script setup lang="ts">
import api from '@/api'
import PagedTable from '@/components/paged-table/index.vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import dayjs from 'dayjs'
import relativeTime from 'dayjs/plugin/relativeTime'
dayjs.extend(relativeTime)

const _api = api.role

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
