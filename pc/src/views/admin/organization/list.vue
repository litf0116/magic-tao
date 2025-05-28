<template>
    <div class="p-4">
        <div>
            <el-row :gutter="20">
                <el-col :span="12">
                    <div class="p-4"></div>
                    <el-card class="box-card">
                        <template #header>
                            <div class="flex items-center" style="width: 100%">
                                <div class="flex-1">机构管理</div>
                                <el-switch
                                    v-model="defaultExpandAllOrg"
                                    class="pr-4"
                                    active-text="全部展开"
                                    inactive-text="不展开"
                                    :active-value="true"
                                    :inactive-value="false"
                                ></el-switch>
                                <el-button type="primary" @click="onAdd()">
                                    <text class="i-mdi-plus"></text>
                                    添加机构</el-button
                                >
                            </div>
                        </template>
                        <div>
                            <el-tree
                                :data="treeData"
                                node-key="id"
                                :show-checkbox="false"
                                :default-expand-all="defaultExpandAllOrg"
                                :expand-on-click-node="false"
                                @node-click="nodeclick"
                            >
                                <template #default="{ node, data }">
                                    <div class="flex items-center justify-between w-full">
                                        <span class="flex-1">{{ node.label }}</span>
                                        <span>
                                            <el-button size="small" type="primary" @click="onEdit(node, data)">
                                                <div class="i-mdi-edit" />
                                                编辑
                                            </el-button>
                                            <el-button size="small" type="primary" @click="onAdd(node, data)">
                                                <div class="i-mdi-plus" />
                                                添加子机构
                                            </el-button>
                                            <el-button size="small" type="danger" @click="onRemove(node, data)">
                                                <div class="i-mdi-trash" />
                                                删除
                                            </el-button>
                                        </span>
                                    </div>
                                </template>
                            </el-tree>
                        </div>
                    </el-card>
                </el-col>
                <el-col :span="12">
                    <el-card v-if="select.id" class="box-card transition-all">
                        <template #header>
                            <div class="card-header flex justify-between items-center">
                                <div>{{ select.displayName }}</div>

                                <el-dropdown>
                                    <span class="el-dropdown-link text-blue-500">
                                        操作菜单
                                        <i class="el-icon-arrow-down el-icon--right"></i>
                                    </span>
                                    <template #dropdown>
                                        <el-dropdown-menu>
                                            <el-dropdown-item @click="onEdit(null, select)">编辑</el-dropdown-item>
                                        </el-dropdown-menu>
                                    </template>
                                </el-dropdown>
                            </div>
                        </template>
                        <OrgUserList :id="select.id"></OrgUserList>
                        <el-divider />
                    </el-card>
                </el-col>
            </el-row>
        </div>
    </div>
    <el-drawer
        v-model="drawerShow"
        custom-class="overflow-auto"
        direction="rtl"
        size="45%"
        destroy-on-close
        append-to-body
    >
        <div class="p-4">
            <EditOrg />
        </div>
    </el-drawer>
</template>
<script lang="ts">
import { createOUTree } from '@/utils/tree'
import api from '@/api'
import EditOrg from './edit.vue'
import OrgUserList from './orgUserList.vue'

import { ElMessageBox } from 'element-plus'
import { useStorage } from '@vueuse/core'

export default defineComponent({
    components: { EditOrg, OrgUserList },
    setup() {
        const defaultExpandAllOrg = useStorage('defaultExpandAll', true)
        const data = reactive({
            drawerShow: false,
            select: {} as any,
            treeData: [] as any[],
            ouEditForm: {
                parentId: null,
                displayName: '',
                detail: {},
            },
            fetchData: () => {
                api.organizationUnit
                    .getAllOrganizationUnits({
                        sorting: ' asc',
                        maxResultCount: 1000,
                    })
                    .then((res) => {
                        const tree = createOUTree(res.items, 'parentId', 'id', null, 'children', '')
                        console.log(tree)
                        data.treeData = tree
                    })
            },
            onAdd: (parentNode: any = {}, e: any = {}) => {
                //   data.drawerShow = true
                api.organizationUnit.getForEdit().then((res) => {
                    ouEditForm.value = { ...res.data!, parentId: e.id }
                    ouEditSchema.value = res.schema!
                    data.drawerShow = true
                })
            },
            onEdit: (parentNode: any = {}, e: any = {}) => {
                //   data.drawerShow = true
                //   console.log('onEdit', parentNode, e)

                api.organizationUnit.getForEdit({ id: e.id }).then((res) => {
                    ouEditForm.value = res.data!
                    ouEditSchema.value = res.schema!
                    data.drawerShow = true
                })

                //   ouEditForm.value = { parentId: e.id, displayName: '', detail: {} }
                //   data.drawerShow = true
            },
            onRemove: (parentNode: any = {}, e: any = {}) => {
                //   data.drawerShow = true
                console.log('onRemove', parentNode, e)
                ElMessageBox.confirm('你确定删除吗?', '提示', {
                    confirmButtonText: '确定',
                    cancelButtonText: '取消',
                    type: 'warning',
                }).then(() => {
                    api.organizationUnit.deleteOrganizationUnit({ id: e.id }).then(() => {
                        data.fetchData()
                    })
                })
            },

            nodeclick: (e: any) => {
                console.log(e)
                data.select = e.data
            },
        })

        const ouEditSubmit = async (val: any = {}, e = 0) => {
            console.log('ouEditSubmit', val)
            data.drawerShow = false
            if (val.id) {
                await data.fetchData()
                if (e === 1) {
                    ouEditForm.value = { parentId: val.parentId, displayName: '', detail: {} }
                    data.drawerShow = true
                }
            }
        }

        const ouEditForm = ref({})
        const ouEditSchema = ref({})

        const ouList = computed(() => {
            return data.treeData
        })

        provide('ouEditForm', ouEditForm)
        provide('ouEditSchema', ouEditSchema)
        provide('ouEditSubmit', ouEditSubmit)
        provide('ouList', ouList)

        onMounted(() => {
            data.fetchData()
        })

        return { ...toRefs(data), defaultExpandAllOrg }
    },
})
</script>
