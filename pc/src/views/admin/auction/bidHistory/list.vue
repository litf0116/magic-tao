<template>
    <div>
        <PagedTable ref="tableRef" :table-sort="'creationTime desc'" :fetch-function="fetchData">
            <el-table-column label="#" type="index" width="50" align="center" />
            <el-table-column label="拍卖品编号" prop="auctionItemId" width="100" />
            <el-table-column label="出价人" prop="bidUserName" />
            <el-table-column label="头像" prop="bidUserAvatar" width="100" :formatter="imagePreview" />
            <el-table-column label="出价" prop="bidPrice" />
            <el-table-column label="时间" width="180" prop="bidTime" />
        </PagedTable>
    </div>
</template>

<script setup lang="ts">
import api from '@/api'
import PagedTable from '@/components/paged-table/index.vue'
const { imagePreview } = useFormatter()
const _api = api.bidHistory

const tableRef = ref<InstanceType<typeof PagedTable>>(null)
onMounted(() => {})

function fetchData(params: any) {
    return _api.getAll(params)
}

function reload() {
    console.log('onView')
    tableRef.value.fetchData()
}
</script>
