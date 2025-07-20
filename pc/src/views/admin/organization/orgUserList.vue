<template>
    <div class="text-lg py-2">部门管理列表</div>
    <PagedTable ref="table" :fetch-function="fetchData">
        <el-table-column v-permission="'UserManagment.OrganizationManager'" type="selection" width="50" />
        <el-table-column prop="userName" label="用户名" />
        <el-table-column prop="name" label="姓名" />
        <el-table-column prop="phoneNumber" label="手机" />
    </PagedTable>
    <div v-permission="'UserManagment.OrganizationManager'" class="mt-2 space-x-2">
        <el-button @click="onAddUser">添加管理者</el-button>
        <el-button type="danger" @click="onDeleteUser">删除所选管理</el-button>
    </div>
    <UserSelect ref="userSelect" @select="onSelect" />
</template>

<script lang="ts">
import api from '@/api'

export default defineComponent({
    components: {},
    props: {
        id: {
            type: Number,
            default: 0,
        },
    },
    setup(props) {
        onMounted(() => {
            console.log(`props.id = ${props.id}`)
        })
        const data = reactive({
            table: null as any,
            userSelect: null as any,
            fetchData: (params: any) => {
                return api.organizationUnit.getOrganizationUnitUsers(Object.assign({}, params, { id: props.id }))
            },
            reload: () => {
                if (data.table) data.table.fetchData()
            },
            onAddUser: () => {
                props.id && data.userSelect.show()
            },
            onSelect: (e: any[]) => {
                let list = [...e]
                api.organizationUnit
                    .addUsersToOrganizationUnit({
                        body: {
                            userIds: list.map((x: any) => x.id!),
                            organizationUnitId: props.id as number,
                        },
                    })
                    .then((res) => {
                        // console.log(res)
                        ElMessage({ type: 'success', message: '添加成功!' })
                        data.reload()
                    })
            },
            onDeleteUser: () => {
                console.log('selection', data.table.selection)
                const userIds: number[] = []
                const organizationUnitId = props.id
                ElMessageBox.confirm('你确定删除吗?', '提示', {
                    confirmButtonText: '确定',
                    cancelButtonText: '取消',
                    type: 'warning',
                })
                    .then(() => {
                        data.table.selection.forEach((x: any) => {
                            userIds.push(x.id as number)
                        })
                        api.organizationUnit
                            .removeUsersFromOrganizationUnit({
                                body: { userIds, organizationUnitId },
                            })
                            .then((res) => {
                                ElMessage({ type: 'success', message: '删除成功!' })
                                data.reload()
                            })
                    })
                    .catch(() => {
                        ElMessage({ type: 'info', message: '已取消删除' })
                    })
            },
        })

        watch(
            () => props.id,
            (id, prevId) => {
                //   console.log(`id changed,from ${prevId} to ${id}`)
                data.reload()
            }
        )
        return {
            ...toRefs(data),
        }
    },
})
</script>
