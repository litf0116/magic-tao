<template>
    <div>
        <PagedTable
            ref="tableRef"
            :table-sort="'creationTime desc'"
            :fetch-function="fetchData"
            :is-calculation-paging="false"
        >
            <template #btns>
                <el-button v-permission="'Pages.Administration'" type="primary" @click="onCreate">新建</el-button>
            </template>

            <el-table-column label="#" type="index" width="50" align="center" />
            <el-table-column label="热词标题" prop="title" />
            <el-table-column label="创建时间" prop="createTime" />
            <el-table-column label="操作" align="center" width="180px">
                <template #default="scope">
                    <div class="flex items-center justify-between">
                        <div>
                            <el-button type="danger" @click="onDelete(scope.row)"> 删除 </el-button>
                        </div>
                    </div>
                </template>
            </el-table-column>
        </PagedTable>
        <EditFrom ref="editRef" @change="reload" />
    </div>
</template>

<script setup lang="ts">
import { GetAdminList, Delete } from '@/api/HotWordsAPI'
import EditFrom from './edit.vue'
import PagedTable from '@/components/paged-table/index.vue'
import { ElMessage, ElMessageBox } from 'element-plus'

const tableRef = ref<InstanceType<typeof PagedTable>>(null)
const editRef = ref<InstanceType<typeof EditFrom>>(null)

onMounted(() => {
    //
})

function fetchData(params: any) {
    return GetAdminList(params)
}

function reload() {
    console.log('onView')
    tableRef.value.fetchData()
}

const onCreate = () => {
    console.log('onCreate')
    editRef.value.show(null)
}

const onDelete = (dto: any) => {
    console.log('onDelete')
    ElMessageBox.confirm('你确定删除吗?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
    })
        .then(async () => {
            await Delete(dto.id!).then(() => {
                reload()
            })
            ElMessage({ type: 'success', message: '删除成功!' })
        })
        .catch(() => {
            ElMessage({ type: 'info', message: '已取消删除' })
        })
}
</script>
<style>
.content img {
    width: 50px;
    height: 50px;
}
</style>
