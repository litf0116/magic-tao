<template>
    <el-popover trigger="click" placement="left" :width="400">
        <div class="block p-4">
            <el-timeline>
                <el-timeline-item
                    v-for="(node, index) in nodes"
                    :key="index"
                    :color="
                        node.index <= (modelValue.auditStatus === null ? -1 : modelValue.auditStatus) ? '#0bbd87' : ''
                    "
                    :timestamp="node.time"
                >
                    <div>
                        <div>{{ node.organizationUnitName }} [ {{ node.roleName }} ]</div>
                        <div class="mt-2">
                            <el-tag
                                v-for="(u, k) in node.users"
                                :key="k"
                                class="m-2"
                                :type="getUserNodeType(node.auditNodeId, u)"
                                >{{ u.name }} ({{ u.userName }})</el-tag
                            >
                        </div>
                    </div>
                </el-timeline-item>
            </el-timeline>
        </div>
        <template #reference>
            <template v-if="modelValue">
                <el-button v-if="!modelValue.isAudited" type="primary" size="small" @click="showAuditDetail">
                    审核中
                    <span v-if="modelValue.audit != null">
                        {{
                            `${modelValue.auditStatus === null ? 0 : modelValue.auditStatus + 1}/${
                                modelValue.audit + 1
                            }`
                        }}
                    </span>
                </el-button>
                <el-button v-else type="success" size="small" @click="showAuditDetail">
                    {{ `审核通过` }}
                </el-button>
            </template>
        </template>
    </el-popover>
    <!-- <el-tag v-if="modelValue.state === 5">审核中</el-tag> -->

    <!-- <el-tag v-if="modelValue.state === 1" type="danger">退稿:{{ modelValue.rejectText }}</el-tag>
    <el-tag v-if="modelValue.state === -1" type="warning">不通过</el-tag> -->
</template>
<script lang="ts">
export default defineComponent({
    name: 'StateTag',
})
</script>
<script lang="ts" setup>
import { AuditUserLogDto } from '@/api/appService'

const props = defineProps(['modelValue', 'api'])

const visible = ref(false)
const nodes = ref([] as any[])
const logs = ref([] as AuditUserLogDto[])

function getUserNodeType(nodeId: string, u: { id: number }) {
    console.log(
        logs.value,
        logs.value.filter((x) => x.status === 1)
    )
    if (
        logs.value
            .filter((x) => x.status === 1)
            .findIndex((x) => x.auditNodeId === nodeId && x.creatorUserId === u.id) > -1
    ) {
        return 'success'
    }
    return 'info'
}

function showAuditDetail() {
    visible.value = true
    props.api.getAuditDetail({ id: props.modelValue.id }).then((res: any) => {
        nodes.value = [...res.nodes]
        logs.value = [...res.auditUserLogs]
    })
}
</script>
