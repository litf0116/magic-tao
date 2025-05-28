<template>
    <div>
        <PagedTable ref="tableRef" :table-sort="'creationTime desc'" :fetch-function="fetchData">
            <template #btns>
                <el-button v-permission="'Pages.Administration'" type="primary" @click="onCreate">新建</el-button>
            </template>

            <template #filter>
                <div v-if="tableRef && tableRef.queryForm" class>
                    <queryFilter v-model="tableRef.queryForm.pid" @change="reload" />
                </div>
            </template>

            <el-table-column label="#" type="index" width="50" align="center" />
            <el-table-column label="板块" prop="categoryId" width="100" sortable>
                <template #default="{ row }">
                    <el-tag v-if="row.categoryId === 1" type="success">勇者招募</el-tag>
                    <el-tag v-if="row.categoryId === 2" type="success">拍卖行</el-tag>
                </template>
            </el-table-column>
            <el-table-column label="图片" prop="imageUrl" width="100" :formatter="imagePreview"> </el-table-column>
            <el-table-column label="内容" prop="content" />
            <!-- <el-table-column label="邮箱" prop="emailAddress" /> -->
            <el-table-column label="排序" prop="sort" width="60" />
            <el-table-column label="创建时间" width="180" prop="creationTime" />
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
        <Edit ref="editRef" @change="reload" />
    </div>
</template>

<script setup lang="ts">
import api from '@/api'
import Edit from './edit.vue'
import PagedTable from '@/components/paged-table/index.vue'
import queryFilter from '@/components/paged-table/announceQueryFilter.vue'
const { imagePreview, utcToLocalFull, utcToLocalDay } = useFormatter()
import { ElMessage, ElMessageBox } from 'element-plus'

const _api = api.announce

const tableRef = ref<InstanceType<typeof PagedTable>>(null)
const editRef = ref<InstanceType<typeof Edit>>(null)
onMounted(() => {})

function fetchData(params: any) {
    return api.announce.getAll(params)
}

function reload() {
    console.log('onView')
    tableRef.value.fetchData()
}

const onCreate = () => {
    console.log('onCreate')
    _api.getForEdit({}).then((res) => {
        editRef.value.show(res.data)
    })
}

const onEdit = (dto: any) => {
    console.log('onEdit')
    _api.getForEdit({ id: dto.id! }).then((res) => {
        editRef.value.show(res.data)
    })
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
