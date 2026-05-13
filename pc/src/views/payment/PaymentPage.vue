<template>
    <div class="payment-page min-h-screen flex items-center justify-center p-4">
        <div class="payment-container w-full max-w-4xl mx-auto">
            <!-- 商家信息区域 -->
            <div class="merchant-info bg-white rounded-lg shadow-sm p-6 mb-6">
                <div class="flex items-center gap-4">
                    <div
                        class="merchant-avatar w-16 h-16 bg-blue-500 rounded-lg flex items-center justify-center text-white text-xl font-bold"
                    >
                        魔
                    </div>
                    <div class="merchant-details">
                        <h2 class="text-xl font-bold text-gray-900">魔力淘平台</h2>
                        <p class="text-gray-600">官方认证服务</p>
                    </div>
                </div>
            </div>

            <!-- 认证介绍区 (当 type='membership' 时显示) -->
            <div
                v-if="paymentType === 'membership'"
                class="certification-info bg-yellow-50 border border-yellow-200 rounded-lg p-6 mb-6"
            >
                <h3 class="text-lg font-bold text-yellow-800 mb-3">诚信履约金（51 元）</h3>
                <p class="text-gray-700 mb-4">您本次需支付 51 元，包含：</p>
                <ul class="list-disc pl-6 text-gray-700 space-y-2 mb-4">
                    <li>诚信履约金：50 元（正常退出可退，违规将作为违约金扣除）</li>
                    <li>平台手续费：1 元（不可退还）</li>
                </ul>
                <p class="text-gray-700 mb-4">支付后您将获得：</p>
                <ul class="list-disc pl-6 text-gray-700 space-y-2 mb-4">
                    <li>诚信认证标识</li>
                    <li>无限次信息发布权限</li>
                    <li>信息优先展示</li>
                    <li>交易纠纷优先协调</li>
                </ul>
                <div class="text-sm text-gray-600">
                    <p class="mb-2">
                        履约金规则：用户遵守平台规则、无违规行为，注销账号时可申请全额退还 50
                        元。若用户发布虚假信息、诈骗、恶意骚扰等违规行为，平台有权根据违规严重程度扣除部分或全部履约金作为违约金。具体见《诚信履约金管理规则》。
                    </p>
                </div>
            </div>

            <!-- 二维码和支付信息区域 -->
            <div class="payment-section bg-white rounded-lg shadow-sm p-6 mb-6">
                <div class="grid md:grid-cols-2 gap-8">
                    <!-- 二维码区域 -->
                    <div class="qrcode-area">
                        <h3 class="text-lg font-bold text-gray-900 mb-4">扫码支付</h3>

                        <!-- 二维码容器 -->
                        <div class="qrcode-container flex flex-col items-center">
                            <div v-if="isLoading" class="loading-state flex flex-col items-center justify-center py-12">
                                <el-icon class="is-loading text-4xl text-gray-400 mb-4"><i-ep-loading /></el-icon>
                                <p class="text-gray-600">正在生成支付二维码...</p>
                            </div>

                            <div
                                v-else-if="errorMessage"
                                class="error-state flex flex-col items-center justify-center py-12"
                            >
                                <el-icon class="text-4xl text-red-500 mb-4"><i-ep-circle-close /></el-icon>
                                <p class="text-gray-600 mb-4">{{ errorMessage }}</p>
                                <el-button type="primary" @click="retryPayment">重新获取二维码</el-button>
                            </div>

                            <div
                                v-else-if="paymentSuccess"
                                class="success-state flex flex-col items-center justify-center py-12"
                            >
                                <el-icon class="text-4xl text-green-500 mb-4"><i-ep-circle-check /></el-icon>
                                <p class="text-gray-600 mb-4">支付成功！</p>
                                <p class="text-sm text-gray-500">正在跳转...</p>
                            </div>

                            <div v-else class="qrcode-wrapper bg-white p-4 rounded-lg border border-gray-200 shadow-sm">
                                <QrcodeDisplay :code-url="qrCodeUrl" :size="220" />
                                <div v-if="countdown > 0" class="countdown mt-4 text-center">
                                    <p class="text-sm text-gray-600">⏱️ 有效期剩余 {{ formatCountdown(countdown) }}</p>
                                </div>
                                <div v-else class="countdown mt-4 text-center">
                                    <p class="text-sm text-red-600 font-medium">二维码已过期</p>
                                    <el-button type="primary" size="small" class="mt-2" @click="retryPayment"
                                        >刷新二维码</el-button
                                    >
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- 支付详情区域 -->
                    <div class="payment-details">
                        <h3 class="text-lg font-bold text-gray-900 mb-4">支付详情</h3>

                        <!-- 支付金额 -->
                        <div class="amount-section bg-gray-50 rounded-lg p-4 mb-4">
                            <div class="text-center">
                                <p class="text-sm text-gray-600 mb-1">支付金额</p>
                                <p class="text-3xl font-bold text-green-600">¥{{ amount }}</p>
                                <p class="text-xs text-gray-500 mt-1">订单号：{{ orderNo || '待生成' }}</p>
                            </div>
                        </div>

                        <!-- 支付说明 -->
                        <div class="payment-instructions text-sm text-gray-700 mb-4">
                            <p class="mb-2">支付金额：¥{{ amount }}.00</p>
                            <ul class="list-disc pl-5 space-y-1">
                                <li>50 元为诚信履约金（正常退出可退，违规将扣除）</li>
                                <li>1 元为平台手续费（不退）</li>
                                <li>支付成功即成为认证会员</li>
                            </ul>
                            <p class="mt-3 text-blue-600 cursor-pointer underline" @click="showRules = !showRules">
                                点击查看《诚信履约金管理规则》
                            </p>
                        </div>

                        <!-- 规则详情 -->
                        <div v-if="showRules" class="rules-section bg-gray-50 rounded-lg p-4 mb-4">
                            <h4 class="font-bold text-gray-900 mb-2">《诚信履约金管理规则》</h4>
                            <div class="text-sm text-gray-700 space-y-2">
                                <p>
                                    <strong>一、履约金性质</strong
                                    ><br />诚信履约金是用户为获得平台认证权益而支付的可退还资金，用于约束用户行为、保障平台信息真实性。用户正常履约、无违规行为时，可全额退还。
                                </p>

                                <p>
                                    <strong>二、退还条件</strong><br />1.
                                    用户主动申请注销账号，并确认已清空所有发布信息；<br />2.
                                    账号不存在未处理的违规记录；<br />3. 经平台审核通过后，7 个工作日内原路退还 50
                                    元履约金（1 元手续费不退）。
                                </p>

                                <p>
                                    <strong>三、违规扣罚规则</strong
                                    ><br />用户出现以下违规行为，平台有权扣除部分或全部履约金作为违约金：<br />-
                                    轻微违规（如发布信息与描述严重不符）：扣除 10-20 元；<br />-
                                    一般违规（如重复发布虚假信息、恶意引流）：扣除 30-40 元；<br />-
                                    严重违规（如诈骗、发布违禁内容、骚扰他人）：扣除全部 50 元并永久封禁账号。<br />具体违规认定标准及申诉流程详见《平台用户协议》。
                                </p>

                                <p>
                                    <strong>四、异议申诉</strong><br />用户对扣罚有异议的，可在收到通知后 7
                                    个工作日内通过客服渠道提交申诉，平台将在 5 个工作日内复核并反馈结果。
                                </p>
                            </div>
                        </div>

                        <!-- 操作按钮 -->
                        <div class="payment-actions flex flex-col gap-3">
                            <template v-if="!paymentSuccess">
                                <el-button
                                    v-if="!isLoading && qrCodeUrl"
                                    type="primary"
                                    size="large"
                                    class="w-full"
                                    :loading="checking"
                                    @click="manualCheck"
                                >
                                    我已支付
                                </el-button>
                                <el-button type="default" size="large" class="w-full" @click="goBack">
                                    取消支付
                                </el-button>
                            </template>
                        </div>
                    </div>
                </div>
            </div>

            <!-- 安全提示 -->
            <div class="security-tips bg-blue-50 border border-blue-200 rounded-lg p-4">
                <h4 class="font-bold text-blue-800 mb-2">安全提示</h4>
                <ul class="text-sm text-blue-700 list-disc pl-5 space-y-1">
                    <li>请使用微信"扫一扫"扫描上方二维码完成支付</li>
                    <li>请勿从相册识别二维码，以防诈骗</li>
                    <li>支付过程中如有疑问，请联系客服</li>
                </ul>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElButton, ElIcon } from 'element-plus'
import { usePayment } from '@/composables/usePayment'
import QrcodeDisplay from '@/components/Payment/QrcodeDisplay.vue'

// 路由和导航
const route = useRoute()
const router = useRouter()

// 从路由参数获取配置
const paymentType = ref<string>((route.query.type as string) || 'deposit')
const returnUrl = ref<string>((route.query.returnUrl as string) || '/')
const returnContext = ref<string>((route.query.returnContext as string) || '')

// 支付相关状态
const showRules = ref(false)
const checking = ref(false)

// 计算支付金额（固定51元，不从URL获取）
const amount = 51

// 使用支付 Hook（金额固定51元）
const { qrCodeUrl, orderNo, countdown, errorMessage, isPolling, initPayment, retry, formatCountdown, cleanup } =
    usePayment({ amount })

const isLoading = ref(false)
const paymentSuccess = ref(false)

// 监听倒计时结束 - 二维码过期
watch(countdown, (newCountdown) => {
    if (newCountdown <= 0 && !paymentSuccess.value) {
        ElMessage.warning('二维码已过期，请刷新后重试')
    }
})

// 重试支付
const retryPayment = async () => {
    isLoading.value = true
    try {
        await retry()
        ElMessage.success('二维码已刷新')
    } catch (error) {
        console.error('刷新二维码失败:', error)
        ElMessage.error('刷新失败，请稍后重试')
    } finally {
        isLoading.value = false
    }
}

// 手动检查支付状态
const manualCheck = async () => {
    checking.value = true
    try {
        if (orderNo.value) {
            const result = await getPaymentStatus({ outTradeNo: orderNo.value })
            if (result.status === '已支付') {
                paymentSuccess.value = true
                ElMessage.success('支付成功！')

                // 延迟跳转
                setTimeout(() => {
                    if (returnUrl.value) {
                        router.push(returnUrl.value)
                    } else {
                        router.go(-1)
                    }
                }, 1500)
            } else {
                ElMessage.info('支付尚未完成，请继续支付')
            }
        }
    } catch (error) {
        console.error('手动检查支付状态失败:', error)
        ElMessage.error('检查支付状态失败')
    } finally {
        checking.value = false
    }
}

// 跳转回原页面
const goBack = () => {
    if (returnUrl.value) {
        router.push(returnUrl.value)
    } else {
        router.go(-1)
    }
}

// 页面加载时初始化支付
onMounted(async () => {
    isLoading.value = true
    try {
        await initPayment()
    } catch (error) {
        console.error('初始化支付失败:', error)
        ElMessage.error('初始化支付失败')
    } finally {
        isLoading.value = false
    }
})

// 组件卸载时清理
onUnmounted(() => {
    cleanup()
})
</script>

<style scoped>
.payment-page {
    min-height: 100vh;
    background: linear-gradient(135deg, #f5f7fa 0%, #e4e8ec 100%);
}

.qrcode-wrapper {
    display: inline-block;
}

.countdown {
    margin-top: 12px;
    font-size: 14px;
    color: #666;
}

.countdown.expired {
    color: #ff4d4f;
    font-weight: bold;
}

/* 覆盖 Element Plus 按钮相邻的 margin-left */
.payment-actions .el-button + .el-button {
    margin-left: 0;
}

@media (max-width: 768px) {
    .payment-container {
        padding: 0;
    }

    .payment-section,
    .merchant-info,
    .security-tips {
        border-radius: 0;
        box-shadow: none;
        border-left: none;
        border-right: none;
        margin-left: 0;
        margin-right: 0;
    }

    .payment-section {
        padding-top: 1rem;
        padding-bottom: 1rem;
    }

    .grid.md\:grid-cols-2 {
        grid-template-columns: 1fr !important;
    }
}
</style>
