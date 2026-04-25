<template>
    <div>
        <!-- 调试信息 -->
        <el-card class="mb-4">
            <template #header>
                <span>调试信息</span>
            </template>
            <div>
                <p>当前用户: {{ userStore.user?.userName || '未登录' }}</p>
                <p>角色: {{ userStore.roles?.join(', ') || '无' }}</p>
                <p>权限列表:</p>
                <pre
                    style="background: #f5f7fa; padding: 10px; border-radius: 4px; max-height: 200px; overflow: auto"
                    >{{ JSON.stringify(userStore.permissions, null, 2) }}</pre
                >
                <p>是否有 Pages.Administration 权限: {{ hasAdminPermission }}</p>
            </div>
        </el-card>

        <el-card>
            <template #header>
                <div class="flex justify-between items-center">
                    <span>审核版本管理</span>
                </div>
            </template>

            <div class="version-info">
                <el-descriptions :column="1" border>
                    <el-descriptions-item label="功能说明">
                        <div class="function-desc">
                            <p>用于控制各平台<strong>审核模式</strong>的开启：</p>
                            <ul>
                                <li>
                                    当前版本 = 审核版本 →
                                    <el-tag type="warning" size="small">审核模式</el-tag> 隐藏敏感功能
                                </li>
                                <li>
                                    当前版本 ≠ 审核版本 →
                                    <el-tag type="success" size="small">正常模式</el-tag> 显示所有功能
                                </li>
                            </ul>
                        </div>
                    </el-descriptions-item>
                </el-descriptions>
            </div>

            <el-divider />

            <div class="platform-list">
                <el-table :data="platformList" border>
                    <el-table-column prop="name" label="平台" width="150" />
                    <el-table-column prop="platform" label="平台标识" width="150">
                        <template #default="{ row }">
                            <code class="platform-code">{{ row.platform }}</code>
                        </template>
                    </el-table-column>
                    <el-table-column prop="version" label="审核版本">
                        <template #default="{ row }">
                            <el-tag v-if="row.version" type="warning" size="large">{{ row.version }}</el-tag>
                            <el-tag v-else type="info">未设置</el-tag>
                        </template>
                    </el-table-column>
                    <el-table-column label="状态" width="120">
                        <template #default="{ row }">
                            <el-tag v-if="row.version" type="warning">已启用</el-tag>
                            <el-tag v-else type="info">未启用</el-tag>
                        </template>
                    </el-table-column>
                    <el-table-column label="操作" width="150" fixed="right">
                        <template #default="{ row }">
                            <!-- 调试：无权限时显示提示 -->
                            <el-button
                                v-if="hasAdminPermission"
                                type="primary"
                                size="small"
                                @click="showUpdateDialog(row)"
                            >
                                设置
                            </el-button>
                            <el-tag v-else type="info">无权限</el-tag>
                        </template>
                    </el-table-column>
                </el-table>
            </div>

            <el-divider />

            <div class="help-section">
                <h4>使用说明</h4>
                <ol>
                    <li>审核版本用于控制小程序/App 提交审核时的功能展示</li>
                    <li>当客户端版本号与审核版本号匹配时，进入审核模式，隐藏敏感功能</li>
                    <li>
                        版本号格式支持两种形式：
                        <ul>
                            <li>纯语义版本：<code>1.2.1</code></li>
                            <li>带日期格式：<code>20260410@1.2.1</code></li>
                        </ul>
                    </li>
                    <li>留空表示不启用审核模式，所有版本都显示正常功能</li>
                </ol>
            </div>
        </el-card>

        <el-dialog
            v-model="updateDialogVisible"
            :title="`设置 ${currentPlatform?.name} 审核版本`"
            width="500px"
            :close-on-click-modal="false"
        >
            <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
                <el-form-item label="当前版本">
                    <el-tag v-if="currentPlatform?.version" type="warning">
                        {{ currentPlatform?.version }}
                    </el-tag>
                    <el-tag v-else type="info">未设置</el-tag>
                </el-form-item>
                <el-form-item label="审核版本" prop="version">
                    <el-input v-model="form.version" placeholder="如: 1.2.1 或 20260410@1.2.1" clearable />
                </el-form-item>
                <el-form-item label="格式说明">
                    <div class="format-hint">
                        <p>支持格式：</p>
                        <p>1. 纯语义版本：<code>1.2.1</code></p>
                        <p>2. 带日期格式：<code>20260410@1.2.1</code></p>
                        <p>3. 留空表示禁用审核模式</p>
                    </div>
                </el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="updateDialogVisible = false">取消</el-button>
                <el-button type="primary" :loading="updating" @click="handleUpdate">确认</el-button>
            </template>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import api from '@/api'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'

// 获取用户 store
const userStore = useUserStore()

// 调试：计算是否有管理员权限
const hasAdminPermission = computed(() => {
    const permissions = userStore.permissions || []
    const has = permissions.includes('Pages.Administration')
    console.log('[Debug] 权限检查:', { permissions, has })
    return has
})

// 页面加载时打印调试信息
onMounted(() => {
    console.log('[Debug] 页面加载 - 用户信息:', {
        user: userStore.user,
        roles: userStore.roles,
        permissions: userStore.permissions,
    })
    loadVersions()
})

interface PlatformItem {
    platform: string
    name: string
    version: string
}

const platformList = ref<PlatformItem[]>([
    { platform: 'mp-weixin', name: '微信小程序', version: '' },
    { platform: 'app-plus', name: 'App', version: '' },
    { platform: 'h5', name: 'H5', version: '' },
])

const updateDialogVisible = ref(false)
const updating = ref(false)
const formRef = ref<FormInstance>()
const currentPlatform = ref<PlatformItem | null>(null)

const form = ref({
    version: '',
})

const validateVersion = (_rule: any, value: string, callback: (error?: Error) => void) => {
    if (!value) {
        callback()
        return
    }

    const parts = value.split('@')
    if (parts.length === 1) {
        if (!/^\d+\.\d+\.\d+$/.test(value)) {
            callback(new Error('版本格式应为: 主.次.补，如: 1.2.1'))
            return
        }
    } else if (parts.length === 2) {
        const [datePart, versionPart] = parts
        if (datePart.length !== 8 || !/^\d{8}$/.test(datePart)) {
            callback(new Error('日期部分应为 8 位数字，如: 20260410'))
            return
        }
        if (!/^\d+\.\d+\.\d+$/.test(versionPart)) {
            callback(new Error('版本部分应为: 主.次.补，如: 1.2.1'))
            return
        }
    } else {
        callback(new Error('格式错误，支持: 1.2.1 或 20260410@1.2.1'))
        return
    }

    callback()
}

const rules: FormRules = {
    version: [{ validator: validateVersion, trigger: 'blur' }],
}

async function loadVersions() {
    try {
        const versions = await api.appFeature.getAllReviewVersions()
        platformList.value = platformList.value.map((item) => ({
            ...item,
            version: versions[item.platform as keyof typeof versions] || '',
        }))
    } catch (e) {
        ElMessage.error('获取审核版本失败')
    }
}

function showUpdateDialog(platform: PlatformItem) {
    currentPlatform.value = platform
    form.value.version = platform.version
    updateDialogVisible.value = true
}

async function handleUpdate() {
    const valid = await formRef.value?.validate().catch(() => false)
    if (!valid) return

    if (!currentPlatform.value) return

    updating.value = true
    try {
        await api.appFeature.updateReviewVersion(currentPlatform.value.platform, form.value.version)
        ElMessage.success('审核版本更新成功')
        updateDialogVisible.value = false
        await loadVersions()
    } catch (e: any) {
        ElMessage.error(e.message || '更新失败')
    } finally {
        updating.value = false
    }
}
</script>

<style lang="scss" scoped>
.version-info {
    max-width: 600px;
}

.function-desc {
    p {
        margin: 0 0 8px 0;
    }

    ul {
        margin: 0;
        padding-left: 20px;

        li {
            margin: 4px 0;
        }
    }
}

.platform-code {
    background: #f5f7fa;
    padding: 2px 8px;
    border-radius: 4px;
    font-family: monospace;
}

.platform-list {
    margin-bottom: 20px;
}

.help-section {
    max-width: 700px;

    h4 {
        margin: 0 0 12px 0;
        color: #303133;
    }

    ol {
        margin: 0;
        padding-left: 20px;
        color: #606266;
        line-height: 2;

        code {
            background: #f5f7fa;
            padding: 2px 6px;
            border-radius: 4px;
            font-family: monospace;
            color: #409eff;
        }

        ul {
            margin: 4px 0;
            padding-left: 20px;
        }
    }
}

.format-hint {
    font-size: 12px;
    color: #909399;
    line-height: 1.8;

    code {
        background: #f5f7fa;
        padding: 2px 6px;
        border-radius: 4px;
        font-family: monospace;
    }
}
</style>
