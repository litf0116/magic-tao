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
            <el-table-column label="类型" prop="type" width="100" sortable>
                <template #default="{ row }">
                    <el-tag v-if="row.type === 1" type="success">首页</el-tag>
                    <el-tag v-else-if="row.type === 2" type="success">贴吧</el-tag>
                </template>
            </el-table-column>
            <el-table-column label="标题" prop="title" />
            <el-table-column label="图片" prop="imageUrl" :formatter="imagePreview" />
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
            <el-table-column label="跳转地址" prop="url" />
            <el-table-column label="创建时间" prop="createTime" />
            <el-table-column align="center" width="180px">
                <template #default="scope">
                    <div class="flex items-center justify-between">
                        <div>
                            <el-button v-permission="'Pages.Administration'" type="primary" @click="onEdit(scope.row)">
                                编辑
                            </el-button>
                            <el-button v-permission="'Pages.Administration'" type="danger" @click="onDelete(scope.row)">
                                删除
                            </el-button>
                        </div>
                    </div>
                </template>
            </el-table-column>
        </PagedTable>
        <EditFrom ref="editRef" @change="reload" />
    </div>
</template>

<script setup lang="ts">
import { GetList, UpdateState, Delete } from '@/api/advertisingSpaceAPI'
import EditFrom from './edit.vue'
import PagedTable from '@/components/paged-table/index.vue'
const { imagePreview, utcToLocalFull, utcToLocalDay } = useFormatter()
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
            await Delete(dto.id!).then(() => {
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
    var res = await UpdateState(val.id, val.status)
    res.status === 200 ? Tips.success('操作成功') : Tips.error('操作失败')
}
</script>
