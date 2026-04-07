<template>
    <el-dialog
        v-model="dialogVisible"
        title="提现审核"
        width="850px"
        draggable
        destroy-on-close
        append-to-body
        :close-on-click-modal="false"
    >
        <el-table :data="tableData" style="width: 100%">
            <el-table-column label="用户名" prop="name" />
            <el-table-column label="提现金额" prop="amount" />
            <el-table-column label="提现时间" prop="withdrawalTime" />
            <el-table-column label="审核状态" prop="statusStr" />
            <el-table-column align="right">
                <template #default="scope">
                    <el-button size="small" @click="approve(scope.row)"> 同意 </el-button>
                    <el-button size="small" type="danger" @click="reject(scope.row)"> 拒绝 </el-button>
                </template>
            </el-table-column>
        </el-table>
    </el-dialog>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessageBox } from 'element-plus'
import { PageWithdrawalAmount } from '@/api/auctionMidAPI'

const emit = defineEmits(['onSaved', 'onEdit'])

const search = ref('')
const pageNo = ref(1)
const pageSize = ref(20)
const tableData = ref([])

const dialogVisible = ref(false)
const show = (e: boolean, id: number) => {
    dialogVisible.value = e
    if (e) {
        pageList()
    }
}
defineExpose({
    show,
})
// 组件挂载时开始倒计时
onMounted(() => undefined)
//分页查询数据
const pageList = () => {
    PageWithdrawalAmount({ pageNo: pageNo.value, pageSize: pageSize.value }).then((res) => {
        if (res.status == 200) {
            tableData.value.length = 0
            tableData.value = res.data.item
        }
    })
}
//同意
const approve = (data) => {
    ElMessageBox.prompt('请输入金额', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
    })
        .then(({ value }) => {
            console.log('用户输入的邮箱:', value)
        })
        .catch(() => {
            console.log('取消输入')
        })
}
//拒绝
const reject = (data) => {}
</script>
<style scoped>
.avatar-uploader .avatar {
    width: 96px;
    height: 96px;
    display: block;
}
</style>
