<template>
    <div>
        <div v-permission="'Pages.Administration'" class="flex flex-col w-full mb-4">
            <el-input
                v-model="text"
                type="textarea"
                class="w-full"
                rows="10"
                placeholder="批量导入,中间以英文逗号,隔开"
            />
            <el-button type="primary" @click="submit">导入</el-button>
        </div>
        <PagedTable ref="tableRef" :table-sort="'id desc'" :fetch-function="fetchData">
            <template #btns>
                <el-button type="primary" @click="rebuildCache">重建缓存</el-button>
            </template>

            <el-table-column label="#" type="index" width="50" align="center" />
            <el-table-column label="ID" prop="id" width="100" align="center" />
            <el-table-column label="内容" prop="content" align="center" />
            <el-table-column align="center" width="180px">
                <template #default="scope">
                    <div class="flex items-center justify-between">
                        <div>
                            <el-button v-permission="'Pages.Administration'" type="danger" @click="onDelete(scope.row)">
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
import { ElMessage, ElMessageBox } from 'element-plus'
const _api = api.sensitiveWord

const text = ref('')

const tableRef = ref<InstanceType<typeof PagedTable>>(null)
onMounted(() => {})

function fetchData(params: any) {
    params = { ...params, maxResultCount: 50 }
    return _api.getAll(params)
}

function reload() {
    console.log('onView')
    tableRef.value.fetchData()
}

function rebuildCache() {
    _api.reBuildCache().then(() => {
        Tips.success('重建缓存成功')
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

function submit() {
    console.log('import')
    _api.batchCreate({ body: { words: text.value } }).then(() => {
        Tips.success('导入成功')
        text.value = ''
    })
}
</script>
