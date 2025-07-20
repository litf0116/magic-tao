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
            <el-table-column label="标题" prop="title" />
            <el-table-column label="内容" prop="content" width="300">
                <template #default="{ row }">
                    <div class="content" v-html="row.content"></div>
                </template>
            </el-table-column>
            <el-table-column label="发帖人" prop="userName" />
            <el-table-column label="是否置顶" prop="content" width="100">
                <template #default="{ row }">
                    <el-switch
                        v-model="row.isTop"
                        active-color="#13ce66"
                        inactive-color="#ff4949"
                        :active-value="true"
                        :inactive-value="false"
                        @change="setIsTop(row)"
                    />
                </template>
            </el-table-column>
            <el-table-column label="是否精华帖" prop="content" width="100">
                <template #default="{ row }">
                    <el-switch
                        v-model="row.isEssence"
                        active-color="#13ce66"
                        inactive-color="#ff4949"
                        :active-value="true"
                        :inactive-value="false"
                        @change="setPostEssence(row)"
                    />
                </template>
            </el-table-column>
            <el-table-column label="创建时间" prop="createdAt" />
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
import { GetAdminList, Delete, SetPostTop, SetPostEssence } from '@/api/postAPI'
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
//设置置顶
const setIsTop = async (dto: any) => {
    await SetPostTop(dto.postId).then(() => {
        ElMessage({ type: 'success', message: '设置成功' })
    })
}
//设置精华帖
const setPostEssence = async (dto: any) => {
    await SetPostEssence(dto.postId).then(() => {
        ElMessage({ type: 'success', message: '设置成功' })
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
            await Delete(dto.postId!).then(() => {
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
