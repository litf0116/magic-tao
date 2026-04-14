<template>
    <div>
        <el-card>
            <template #header>
                <div class="flex justify-between items-center">
                    <span>版本控制</span>
                    <el-button v-permission="'Pages.Administration'" type="primary" @click="showUpdateDialog"
                        >更新版本号</el-button
                    >
                </div>
            </template>

            <div class="version-info">
                <el-descriptions :column="1" border>
                    <el-descriptions-item label="当前稳定版本">
                        <el-tag type="success" size="large">{{ currentVersion || '未设置' }}</el-tag>
                    </el-descriptions-item>
                    <el-descriptions-item label="版本格式">
                        <code class="version-format">YYYYMMDD@主.次.补</code>
                        <span class="format-example">（例: 20260408@1.2.0）</span>
                    </el-descriptions-item>
                    <el-descriptions-item label="功能说明">
                        <div class="function-desc">
                            <p>用于控制客户端<strong>拍卖场频道</strong>的显示：</p>
                            <ul>
                                <li>客户端版本 ≤ 稳定版本 → <el-tag type="success" size="small">显示拍卖场</el-tag></li>
                                <li>客户端版本 > 稳定版本 → <el-tag type="info" size="small">隐藏拍卖场</el-tag></li>
                            </ul>
                        </div>
                    </el-descriptions-item>
                    <el-descriptions-item label="更新时间">
                        {{ lastUpdateTime || '-' }}
                    </el-descriptions-item>
                </el-descriptions>
            </div>

            <el-divider />

            <div class="help-section">
                <h4>使用说明</h4>
                <ol>
                    <li>版本号格式为 <code>日期@语义版本</code>，例如 <code>20260408@1.2.0</code></li>
                    <li>日期部分为 8 位数字，表示发布日期</li>
                    <li>语义版本遵循 主版本.次版本.补丁版本 格式</li>
                    <li>更新版本号后，所有客户端实例将立即生效（已清除缓存）</li>
                    <li>此版本控制与「版本发布」功能独立，用于控制功能可见性，而非 App 安装包版本管理</li>
                </ol>
            </div>
        </el-card>

        <el-dialog v-model="updateDialogVisible" title="更新稳定版本号" width="500px" :close-on-click-modal="false">
            <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
                <el-form-item label="当前版本">
                    <el-tag>{{ currentVersion || '未设置' }}</el-tag>
                </el-form-item>
                <el-form-item label="新版本号" prop="version">
                    <el-input v-model="form.version" placeholder="格式: YYYYMMDD@主.次.补" clearable />
                </el-form-item>
                <el-form-item label="格式说明">
                    <div class="format-hint">
                        <p>示例: <code>20260408@1.2.0</code></p>
                        <p>日期: 8 位数字（如 20260408）</p>
                        <p>版本: 主.次.补（如 1.2.0）</p>
                    </div>
                </el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="updateDialogVisible = false">取消</el-button>
                <el-button type="primary" :loading="updating" @click="handleUpdate">确认更新</el-button>
            </template>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import api from '@/api'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'

const currentVersion = ref('')
const lastUpdateTime = ref('')
const updateDialogVisible = ref(false)
const updating = ref(false)
const formRef = ref<FormInstance>()

const form = ref({
    version: '',
})

const validateVersion = (_rule: any, value: string, callback: (error?: Error) => void) => {
    if (!value) {
        callback(new Error('请输入版本号'))
        return
    }

    const parts = value.split('@')
    if (parts.length !== 2) {
        callback(new Error('格式错误，应为: YYYYMMDD@主.次.补'))
        return
    }

    const [datePart, versionPart] = parts

    if (datePart.length !== 8 || !/^\d{8}$/.test(datePart)) {
        callback(new Error('日期部分应为 8 位数字，如: 20260408'))
        return
    }

    if (!/^\d+\.\d+\.\d+$/.test(versionPart)) {
        callback(new Error('版本部分应为 主.次.补 格式，如: 1.2.0'))
        return
    }

    callback()
}

const rules: FormRules = {
    version: [{ validator: validateVersion, trigger: 'blur' }],
}

async function loadVersion() {
    try {
        const version = await api.versionControl.getLatestStableVersion()
        currentVersion.value = version || ''
        lastUpdateTime.value = new Date().toLocaleString('zh-CN')
    } catch (e) {
        ElMessage.error('获取版本信息失败')
    }
}

function showUpdateDialog() {
    form.value.version = currentVersion.value
    updateDialogVisible.value = true
}

async function handleUpdate() {
    const valid = await formRef.value?.validate().catch(() => false)
    if (!valid) return

    updating.value = true
    try {
        await api.versionControl.updateLatestStableVersion(form.value.version)
        ElMessage.success('版本号更新成功')
        updateDialogVisible.value = false
        await loadVersion()
    } catch (e: any) {
        ElMessage.error(e.message || '更新失败')
    } finally {
        updating.value = false
    }
}

onMounted(() => {
    loadVersion()
})
</script>

<style lang="scss" scoped>
.version-info {
    max-width: 600px;
}

.version-format {
    background: #f5f7fa;
    padding: 2px 8px;
    border-radius: 4px;
    font-family: monospace;
}

.format-example {
    color: #909399;
    margin-left: 8px;
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

.help-section {
    max-width: 600px;

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
