<template>
    <div>
        <PagedTable ref="tableRef" :table-sort="'order asc'" :fetch-function="fetchData">
            <template #btns>
                <el-button v-permission="'Pages.Administration'" type="primary" @click="onCreate">新建</el-button>
            </template>

            <template #filter>
                <div v-if="tableRef && tableRef.queryForm">
                    <auctionItemQueryFilter v-model="tableRef.queryForm.status" @change="reload" />
                </div>
            </template>

            <el-table-column label="ID" prop="id" width="80" align="center" />
            <el-table-column label="标题" prop="name" />
            <el-table-column label="卖家信息" prop="sellerInfo" />
            <el-table-column label="图片" prop="imageUrl" width="100" :formatter="imagePreview"> </el-table-column>
            <!-- <el-table-column label="邮箱" prop="emailAddress" /> -->
            <el-table-column label="排序" prop="order" width="60" />
            <el-table-column label="状态" prop="status" width="60" />
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
        <Edit ref="editRef" @onSaved="reload" />
    </div>
</template>

<script setup lang="ts">
import api from '@/api'
import Edit from '@/components/Chat/editAuctionItem.vue'
import auctionItemQueryFilter from '@/components/paged-table/auctionItemQueryFilter.vue'
import PagedTable from '@/components/paged-table/index.vue'
const { imagePreview, utcToLocalFull, utcToLocalDay } = useFormatter()
import { ElMessage, ElMessageBox } from 'element-plus'

const _api = api.auctionItem

const tableRef = ref<InstanceType<typeof PagedTable>>(null)
const editRef = ref<InstanceType<typeof Edit>>(null)
onMounted(() => undefined)

function fetchData(params: any) {
    return _api.getAll(params)
}

function reload() {
    console.log('onView')
    tableRef.value.fetchData()
}

const onCreate = () => {
    console.log('onCreate')
    _api.getForEdit({}).then((res) => {
        editRef.value.show(true, 0)
    })
}

const onEdit = (dto: any) => {
    console.log('onEdit')
    editRef.value.show(true, dto.id!)
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
