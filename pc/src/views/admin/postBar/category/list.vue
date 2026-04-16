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
            <el-table-column label="类型名称" prop="name" />
            <el-table-column label="状态" prop="status" width="100">
                <template #default="{ row }">
                    <el-switch
                        v-model="row.status"
                        active-color="#13ce66"
                        inactive-color="#ff4949"
                        :active-value="1"
                        :inactive-value="0"
                        @change="handleChange(row)"
                    />
                </template>
            </el-table-column>
            <el-table-column label="排序" prop="sort" />
            <el-table-column label="创建时间" prop="createdAt" />
            <el-table-column label="操作" align="center" width="180px">
                <template #default="scope">
                    <div class="flex items-center justify-between">
                        <div>
                            <el-button type="primary" @click="onEdit(scope.row)"> 编辑 </el-button>
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
import { GetList, UpdateState, Delete } from '@/api/postCategoryAPI'
import EditFrom from './edit.vue'
import PagedTable from '@/components/paged-table/index.vue'
import { ElMessage, ElMessageBox } from 'element-plus'

const tableRef = ref<InstanceType<typeof PagedTable>>(null)
const editRef = ref<InstanceType<typeof EditFrom>>(null)

onMounted(() => {
    //
})

function fetchData(params: any) {
    return GetList(params)
}

function reload() {
    console.log('onView')
    tableRef.value.fetchData()
}

const onCreate = () => {
    console.log('onCreate')
    editRef.value.show(null)
}

const onEdit = (dto: any) => {
    console.log('onEdit')
    editRef.value.show(dto)
}
const onDelete = (dto: any) => {
    console.log('onDelete')
    ElMessageBox.confirm('你确定删除吗?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
    })
        .then(async () => {
            await Delete(dto.categoryId!).then(() => {
                reload()
            })
            ElMessage({ type: 'success', message: '删除成功!' })
        })
        .catch(() => {
            ElMessage({ type: 'info', message: '已取消删除' })
        })
}
//更新状态
const handleChange = async (val) => {
    var res = await UpdateState(val.categoryId, val.status)
    Tips.success('操作成功')
}
</script>
