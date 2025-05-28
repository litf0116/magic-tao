<template>
    <div>
        <PagedTable ref="table" table-sort="auditName asc" :fetch-function="fetchData">
            <el-table-column label="审核类型">
                <template #default="scope">
                    {{ scope.row.auditDisplayName || scope.row.auditName }}
                </template>
            </el-table-column>
            <el-table-column label="类型" prop="providerName" width="100" align="center">
                <template #default="scope">
                    <el-tag v-if="scope.row.providerName === 'G'">全局</el-tag>
                    <el-tag v-else-if="scope.row.providerName === 'D'" type="success">分类</el-tag>
                    <el-tag v-else>{{ scope.row.providerName }}</el-tag>
                </template>
            </el-table-column>
            <el-table-column label="键值1" prop="providerKey" />
            <el-table-column align="center" width="180px">
                <template #default="scope">
                    <!-- <button @click="reload" class=" mr-2">刷新</button> -->
                    <!-- <button @click="onView(scope.row)" class=" mr-2">查看</button> -->
                    <el-button type="primary" @click="onEdit(scope.row)">编辑</el-button>
                    <el-button type="danger" @click="onDelete(scope.row)">删除</el-button>
                </template>
            </el-table-column>
        </PagedTable>
    </div>
</template>

<script lang="ts">
import api from "@/api";
import { useRouter } from "vue-router";
import { ElMessage, ElMessageBox } from "element-plus";

export default defineComponent({
    name: "FormList",
    setup() {
        const router = useRouter();
        const data = reactive({
            table: null as any,
            fetchData: (params: any) => {
                return api.admin.auditFlow.getAll(params);
            },
            reload: () => {
                console.log("onView");
                data.table.fetchData();
            },
            onEdit: (dto: any) => {
                console.log("onEdit");
                router.push({ name: "AuditEdit", params: { id: dto.id! } });
            },
            onDelete: (dto: any) => {
                console.log("onDelete");
                ElMessageBox.confirm("你确定删除吗?", "提示", {
                    confirmButtonText: "确定",
                    cancelButtonText: "取消",
                    type: "warning",
                })
                    .then(async () => {
                        await api.admin.auditFlow.delete({ id: dto.id! }).then(() => {
                            data.reload();
                        });
                        ElMessage({ type: "success", message: "删除成功!" });
                    })
                    .catch(() => {
                        ElMessage({ type: "info", message: "已取消删除" });
                    });
            },
        });
        return { ...toRefs(data) };
    },
    methods: {},
});
</script>

<style></style>
