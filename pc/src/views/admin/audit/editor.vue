<template>
    <div class="grid grid-cols-3">
        <div class="side">
            <div>
                <el-button-group>
                    <el-button type="primary" @click="addRow(1)">添加一行</el-button>
                    <el-button type="primary" @click="addRow(0)">添加空行</el-button>
                    <el-button @click="removeEmptyRow">移除空行</el-button>
                    <el-button type="warning" @click="removeEmptyNode">移除无效</el-button>
                </el-button-group>
            </div>
            <div class="r-layout">
                <draggable :list="recycle" item-key="name" :group="{ name: 'nodes', put: true }" @end="dragEnd4">
                    <template #item="{ element }">
                        <!-- <div class="h-32">&nbsp;</div> -->
                    </template>
                    <template #header>
                        <div class="h-32 w-full flex items-center">
                            <div class="i-ph-trash w-24 h-24" :class="drag ? 'dragging' : ''" />
                        </div>
                    </template>
                </draggable>
            </div>
            <div>
                <!-- {{ rows }} -->
            </div>
        </div>
        <div class="col-span-2">
            <draggable
                v-model="rows"
                class="v-layout"
                item-key="desc"
                ghost-class="ghost"
                @start="drag = true"
                @end="drag = false"
            >
                <template #item="{ element, index }">
                    <div class="v-flex padding">
                        <div class="v-index" @click="deleteRow(index)">{{ index }}</div>
                        <draggable
                            :list="element.items"
                            item-key="roleId"
                            class="v-layout"
                            group="nodes"
                            ghost-class="ghost"
                            @start="drag = true"
                            @end="drag = false"
                        >
                            <template #item="{ element }">
                                <div class="v-flex">
                                    <el-card v-if="element" shadow="hover">
                                        <div class="text item margin-bottom bg-white shadow">
                                            <el-input v-model="element.desc" type="text" />
                                        </div>
                                        <div class="mt-2 item flex !flex-col">
                                            <el-select
                                                size="small"
                                                :model-value="element.roleId"
                                                clearable
                                                placeholder="权限选择"
                                                style="width: 100%"
                                                @change="selectRole($event, element)"
                                            >
                                                <el-option
                                                    v-for="(item, index_role) in roles"
                                                    :key="index_role"
                                                    :label="item.displayName"
                                                    :value="item.id"
                                                ></el-option>
                                            </el-select>
                                        </div>
                                    </el-card>
                                </div>
                            </template>
                        </draggable>
                        <div v-if="index !== Object.keys(rows).length - 1" class="v-arrow">
                            <div class="i-mdi-arrow-down w-12 h-12" />
                        </div>
                    </div>
                </template>
            </draggable>
        </div>
    </div>
</template>
<script lang="ts">
import { defineComponent, inject, onMounted, toRefs, reactive, ref, computed } from 'vue'
import draggable from 'vuedraggable'
import api from '@/api'
export default defineComponent({
    components: { draggable },
    setup() {
        const form = inject('form', {})
        const rows = inject('rows', [
            {
                items: [
                    {
                        desc: '审核节点1',
                        roleName: undefined,
                        roleId: undefined,
                        userName: undefined,
                        userId: undefined,
                    },
                ],
            },
        ])

        onMounted(async () => {
            api.role.getAll().then((res) => {
                data.roles = res.items!
            })
        })

        const dialogTitle = computed(() => {
            return data.form.id !== api.guid ? '编辑  审核流程' : '新建  审核流程'
        })

        const data = reactive({
            roles: [] as any[],
            drag: false,
            recycle: [],
            enabled: true,
            auditDefinitions: [],
            dragEnd1(e: any) {
                console.log(e)
            },
            dragEnd2(e: any) {},
            dragEnd4(e: any) {
                console.log('recycle dragend')
                data.recycle = []
            },
            // 移除空行
            removeEmptyRow() {
                rows.value = rows.value.filter((x) => x.items.length > 0)
            },
            // 添加一行
            addRow(r: any) {
                let index = rows.value.length
                let items =
                    r > 0
                        ? [
                              {
                                  desc: `审核节点${index + 1}`,
                                  userId: undefined,
                                  userName: undefined,
                                  //   roleId: undefined,
                                  //   roleName: undefined,
                                  auditFlowId: undefined,
                                  tenantId: undefined,
                              },
                          ]
                        : []
                rows.value = [
                    ...this.rows,
                    {
                        items: items,
                    },
                ]
            },
            // 删除整行
            deleteRow(index: any) {
                rows.value.splice(index, 1)
            },
            // 删除空节点
            removeEmptyNode(index: any) {
                const result: any = []
                rows.value.forEach((r) => {
                    result.push({
                        items: r.items.filter((x: any) => {
                            return !!x.userId || !!x.roleId
                        }),
                    })
                })
                rows.value = result
                data.removeEmptyRow()
            }, // 选权限下拉结束
            selectRole(value: any, item: any) {
                if (!value) {
                    item.roleName = undefined
                    item.roleId = undefined
                } else {
                    item.roleName = data.roles.filter((x) => x.id === value)[0].displayName
                    item.roleId = data.roles.filter((x) => x.id === value)[0].id
                }
            },
        })
        return { dialogTitle, rows, ...toRefs(data) }
    },
})
</script>

<style scoped lang="scss">
.side {
    @apply flex flex-col justify-between my-2;
    .r-layout {
        @apply my-2 flex items-center justify-center text-gray-200 border-2 border-dashed border-green-500;
        .tip {
            @apply text-red-500;
        }
    }

    .p-layout {
        margin: 10px 0;
        min-height: 120px;
        max-height: 200px;
        border: 2px dashed #2ba8fc;
        border-radius: 5px;
        overflow-y: auto;
    }
}

.p-flex {
    @apply m-2 block relative;
}

.v-flex {
    @apply m-2 block relative;
    .v-arrow {
        @apply flex justify-center text-blue-500;
    }

    .v-index {
        @apply absolute text-blue-500 text-center;
        top: 22px;
        left: 22px;
        width: 20px;
        height: 20px;
        font-weight: 600;
        font-size: 18px;
        line-height: 20px;
    }

    .v-layout {
        min-height: 40px;
        border: 2px dashed gray;
        background-color: #fff8e6;
        border-radius: 5px;
        flex-wrap: wrap;
        display: flex;
        flex-direction: row;
        justify-content: center;

        .v-flex {
            border: 1px #2ba8fc solid;
        }
    }
}

.ghost {
    opacity: 0.5;
    background: #c8ebfb;
}

.dragging {
    @apply text-red-500;
    animation: jello 1s;
    animation-iteration-count: infinite;
}

.button-group {
    text-align: right;
}

@keyframes jello {
    from,
    11.1%,
    to {
        transform: none;
    }

    22.2% {
        transform: skewX(-12.5deg) skewY(-12.5deg);
    }

    33.3% {
        transform: skewX(6.25deg) skewY(6.25deg);
    }

    44.4% {
        transform: skewX(-3.125deg) skewY(-3.125deg);
    }

    55.5% {
        transform: skewX(1.5625deg) skewY(1.5625deg);
    }

    66.6% {
        transform: skewX(-0.78125deg) skewY(-0.78125deg);
    }

    77.7% {
        transform: skewX(0.390625deg) skewY(0.390625deg);
    }

    88.8% {
        transform: skewX(-0.1953125deg) skewY(-0.1953125deg);
    }
}
</style>

<style lang="scss">
.el-card {
    &__body {
        padding: 15px;
    }

    .item {
        display: flex;
        flex-direction: row;
        justify-content: space-between;
        align-items: center;
        text-align: center;
    }
}
</style>
