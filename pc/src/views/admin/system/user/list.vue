<template>
    <div>
        <PagedTable ref="tableRef" :table-sort="'creationTime asc'" :fetch-function="fetchData" :query-status="1">
            <template #filter>
                <div v-if="tableRef && tableRef.queryForm">
                    <queryFilter
                        v-model="tableRef.queryForm.pid"
                        :show-all="true"
                        :list="roleListFilter"
                        @change="reload"
                    />
                    <queryFilter
                        v-model="tableRef.queryForm.status"
                        class="mt-4"
                        :show-all="false"
                        :list="filterList"
                        @change="reload"
                    />
                </div>
            </template>
            <el-table-column label="ID" prop="id" width="80" align="center" />
            <el-table-column label="图片" prop="headImgUrl" width="100" :formatter="imagePreview"> </el-table-column>
            <el-table-column label="昵称" prop="name" sortable />
            <el-table-column label="用户名" prop="userName" sortable />
            <el-table-column label="QQ" prop="qq" sortable />
            <el-table-column label="微信" prop="wx" sortable />
            <el-table-column label="诚信履约金" prop="depositBalance" />
            <el-table-column label="累计消费金额" prop="cumulativeAmount" />
            <!-- <el-table-column label="手机号码" prop="phoneNumber" /> -->
            <el-table-column align="center" width="200px">
                <template #default="scope">
                    <div class="flex items-center justify-between">
                        <div>
                            <el-button v-permission="'Pages.Administration'" type="primary" @click="onBan(scope.row)">
                                禁言
                            </el-button>
                            <el-button v-permission="'Pages.Administration'" type="primary" @click="onEdit(scope.row)">
                                编辑
                            </el-button>
                            <el-button
                                v-permission="'Pages.Administration'"
                                type="primary"
                                @click="onUserGroupLevel(scope.row)"
                            >
                                设置群等级
                            </el-button>
                        </div>
                    </div>
                </template>
            </el-table-column>
        </PagedTable>

        <el-dialog v-model="editor.show">
            <el-form ref="dataForm" :model="editor.form" label-position="top">
                <el-tabs tab-position="left">
                    <el-tab-pane label="基本信息">
                        <Tab1 />
                    </el-tab-pane>
                    <el-tab-pane label="权限">
                        <el-form-item v-permission="'Pages.Administration'" label="权限">
                            <el-checkbox-group v-model="editor.form.assignedRoleNames">
                                <el-checkbox v-for="x in editor.roles" :key="x.roleName" :label="x.roleName"
                                    >{{ x.roleName }}[{{ x.roleDisplayName }}]</el-checkbox
                                >
                            </el-checkbox-group>
                        </el-form-item>
                    </el-tab-pane>
                </el-tabs>
            </el-form>
            <template #footer>
                <span class="dialog-footer">
                    <el-button @click="editor.show = false">取 消</el-button>
                    <el-button type="primary" @click="editor.onSubmit">确 定</el-button>
                </span>
            </template>
        </el-dialog>

        <el-dialog v-model="groupChatLeve.show" width="30%">
            <el-form ref="formRefs" :model="groupChatLeve.form" label-position="top">
                <el-form-item label="用户编号" prop="userId">
                    <el-input v-model="groupChatLeve.form.userId" disabled />
                </el-form-item>
                <el-form-item
                    label="累计金额"
                    prop="cumulativeAmount"
                    :rules="{
                        required: true,
                        message: '累计金额不能为空',
                        trigger: 'blur',
                    }"
                >
                    <el-input v-model="groupChatLeve.form.cumulativeAmount" />
                </el-form-item>
            </el-form>
            <template #footer>
                <span class="dialog-footer">
                    <el-button @click="groupChatLeve.show = false">取 消</el-button>
                    <el-button type="primary" @click="groupChatLeve.onSubmit">确 定</el-button>
                </span>
            </template>
        </el-dialog>
    </div>
</template>

<script lang="ts" setup name="userList">
import api from '@/api'
import { defineComponent, reactive, toRefs, provide, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import Tab1 from './Tab1.vue'
import queryFilter from '@/components/paged-table/queryFilter.vue'
import { RoleDto } from '@/api/appService'
import { GroupChatLevelAdd, GetUserGroupLevel } from '@/api/groupChatLevel'

const { imagePreview, utcToLocalFull, utcToLocalDay } = useFormatter()

onMounted(() => {
    api.role.getAll({}).then((res) => {
        roleList.value = res.items
    })
})

const filterList = [
    { id: 1, label: '正常用户' },
    { id: 0, label: '已封号' },
]

const roleList = ref<RoleDto[]>([])

const roleListFilter = computed(() => {
    if (roleList.value.length > 0) {
        return roleList.value.map((x) => {
            return { id: x.id, label: x.displayName }
        })
    }
    return []
})

const tableRef = ref(null as any)
const fetchData = (params: any) => {
    return api.user.getAll(params)
}
const reload = () => {
    console.log('onView')
    tableRef.value.fetchData()
}

const onBan = (dto: any) => {
    ElMessageBox.prompt('请输入禁言时间(分钟)', '禁言', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        inputPattern: /\d+/,
        inputValue: '60',
        inputType: 'number',
        inputErrorMessage: '请输入正确的数字',
    }).then(({ value }) => {
        console.log('ban', value)
        api.ws
            .banUser({
                body: { userId: dto.id, minutes: Number(value) },
            })
            .then(() => {
                Tips.success('禁言成功')
            })
    })
}
const onEdit = (dto: any) => {
    api.user.getUserForEdit({ id: dto.id }).then((res) => {
        editor.show = true
        editor.form.user = res.user
        editor.form.assignedRoleNames = []
        editor.roles = res.roles
        if (res.roles)
            res.roles.forEach((z: any) => {
                if (z.isAssigned) {
                    editor.form!.assignedRoleNames!.push(z.roleName)
                }
            })
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
            await api.user.delete({ id: dto.id! }).then(() => {
                reload()
            })
            ElMessage({ type: 'success', message: '删除成功!' })
        })
        .catch(() => {
            ElMessage({ type: 'info', message: '已取消删除' })
        })
}
const editor = reactive({
    show: false,
    form: {} as any,
    roles: [] as any[],
    onSubmit: (type: number) => {
        let _api
        if (editor.form.user.id) {
            _api = api.user.createOrUpdateUser
        } else {
            _api = api.user.createOrUpdateUser
        }
        console.log('onSubmit', editor.form)
        _api({
            body: Object.assign({}, editor.form),
        }).then((res) => {
            ElMessage.success({
                message: '保存成功',
                type: 'success',
            })
            editor.show = false
            reload()
        })
    },
} as any)

const formRefs = ref(null as any)
//显示弹窗
const groupChatLeve = reactive({
    show: false,
    form: {} as any,
    onSubmit: (type: number) => {
        formRefs.value.validate((valid) => {
            if (valid) {
                GroupChatLevelAdd(groupChatLeve.form).then((res) => {
                    Tips.success('保存成功')
                    groupChatLeve.show = false
                })
            } else {
                Tips.error('请完善表单相关信息！')
                return false
            }
        })
    },
} as any)
const onUserGroupLevel = async (dto: any) => {
    groupChatLeve.form.userId = dto.id
    await GetUserGroupLevel(dto.id).then((res: any) => {
        groupChatLeve.show = true
        groupChatLeve.form.cumulativeAmount = res.data != null ? res.data.cumulativeAmount : 0
    })
}

const formtoChild = computed(() => editor.form)

provide('form', formtoChild)
</script>

<style scoped></style>
