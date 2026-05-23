<template>
    <div class="profile-page">
        <!-- 页面头部 -->
        <div class="page-header">
            <h1 class="page-title">个人中心</h1>
            <p class="page-subtitle">管理您的账户信息</p>
        </div>

        <!-- 主内容区域 -->
        <div class="content-wrapper">
            <!-- 用户信息卡片 -->
            <div class="user-info-card">
                <div class="user-avatar-wrapper">
                    <el-avatar :size="80" :src="userStore.user.headImgUrl" class="user-avatar">
                        <el-icon><User /></el-icon>
                    </el-avatar>
                </div>
                <div class="user-meta">
                    <h2 class="user-name">{{ userStore.user.name || '未登录' }}</h2>
                    <span class="user-id">ID: {{ userStore.user.id || '-' }}</span>
                </div>
            </div>

            <!-- 余额信息 -->
            <div class="balance-section">
                <div class="balance-card">
                    <div class="balance-label">账户余额</div>
                    <div class="balance-value">¥{{ userStore.user.balance || 0 }}</div>
                </div>
                <div class="balance-card highlight">
                    <div class="balance-label">诚信履约金</div>
                    <div class="balance-value">¥{{ userStore.user.depositBalance || 0 }}</div>
                </div>
            </div>

            <!-- 操作按钮 -->
            <div class="action-section">
                <el-button type="primary" class="action-btn" @click="handleRecharge">
                    <el-icon><Wallet /></el-icon>
                    诚信履约金充值
                </el-button>
                <el-button class="action-btn" @click="handleSettings">
                    <el-icon><Setting /></el-icon>
                    账户设置
                </el-button>
                <el-button class="action-btn logout-btn" @click="handleLogout">
                    <el-icon><SwitchButton /></el-icon>
                    退出登录
                </el-button>
            </div>

            <!-- 功能菜单 -->
            <div class="menu-section">
                <div class="menu-item" @click="goToAuction">
                    <div class="menu-icon">
                        <el-icon><Trophy /></el-icon>
                    </div>
                    <div class="menu-content">
                        <span class="menu-title">我的拍卖</span>
                        <span class="menu-desc">查看参与过的拍卖活动</span>
                    </div>
                    <el-icon class="menu-arrow"><ArrowRight /></el-icon>
                </div>

                <div class="menu-item" @click="goToTrading">
                    <div class="menu-icon">
                        <el-icon><Shop /></el-icon>
                    </div>
                    <div class="menu-content">
                        <span class="menu-title">我的交易</span>
                        <span class="menu-desc">管理您的交易帖子</span>
                    </div>
                    <el-icon class="menu-arrow"><ArrowRight /></el-icon>
                </div>

                <div class="menu-item" @click="goToPaymentHistory">
                    <div class="menu-icon">
                        <el-icon><Document /></el-icon>
                    </div>
                    <div class="menu-content">
                        <span class="menu-title">充值记录</span>
                        <span class="menu-desc">查看历史充值明细</span>
                    </div>
                    <el-icon class="menu-arrow"><ArrowRight /></el-icon>
                </div>

                <div class="menu-item" @click="goToDownload">
                    <div class="menu-icon">
                        <el-icon><Download /></el-icon>
                    </div>
                    <div class="menu-content">
                        <span class="menu-title">下载 App</span>
                        <span class="menu-desc">获取魔力淘移动应用</span>
                    </div>
                    <el-icon class="menu-arrow"><ArrowRight /></el-icon>
                </div>
            </div>
        </div>

        <!-- 充值弹窗 -->
        <el-dialog
            v-model="showRechargeDialog"
            title="魔力值充值"
            width="420px"
            center
            :close-on-click-modal="false"
            @closed="onRechargeDialogClosed"
        >
            <div class="recharge-dialog-content">
                <div class="balance-section-dialog">
                    <div class="balance-label">当前魔力值</div>
                    <div class="balance-amount">¥{{ userStore.user?.depositBalance || 0 }}</div>
                </div>

                <div class="amount-section">
                    <div class="amount-label">充值金额</div>
                    <div class="amount-value">¥51.00</div>
                    <div class="amount-hint">含手续费¥1，实际到账¥50</div>
                </div>

                <div class="qrcode-section">
                    <div v-if="loading" class="loading-state">
                        <el-icon class="is-loading"><Loading /></el-icon>
                        <span>正在生成支付二维码...</span>
                    </div>

                    <div v-else-if="qrCodeUrl" class="qrcode-display">
                        <img :src="qrCodeUrl" alt="支付二维码" class="qrcode-img" />
                        <div class="qrcode-hint">请使用微信扫一扫完成支付</div>
                        <div v-if="countdown > 0" class="countdown">有效期剩余 {{ formatCountdown(countdown) }}</div>
                        <div v-else class="countdown expired">二维码已过期，请刷新</div>
                    </div>

                    <div v-else-if="error" class="error-state">
                        <el-icon><CircleClose /></el-icon>
                        <span>{{ error }}</span>
                        <el-button type="primary" size="small" @click="generateQRCode">重新生成</el-button>
                    </div>

                    <div v-else class="generate-btn-wrapper">
                        <el-button type="primary" size="large" :loading="loading" @click="generateQRCode">
                            <el-icon><Wallet /></el-icon>
                            生成支付二维码
                        </el-button>
                    </div>
                </div>

                <div class="recharge-tips">
                    <div class="tips-title">充值说明</div>
                    <ul class="tips-list">
                        <li>充值金额：¥51（含手续费¥1）</li>
                        <li>到账金额：¥50 魔力值</li>
                        <li>支持微信扫码支付</li>
                        <li>支付成功后立即到账</li>
                    </ul>
                </div>
            </div>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import {
    User,
    Wallet,
    Setting,
    SwitchButton,
    Trophy,
    Shop,
    Document,
    Download,
    ArrowRight,
    Loading,
    CircleClose,
} from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useUserStore } from '@/stores/userStore'
import { createPaymentOrder, getPaymentStatus } from '@/api/payment'

const router = useRouter()
const userStore = useUserStore()

// 充值弹窗状态
const showRechargeDialog = ref(false)
const loading = ref(false)
const qrCodeUrl = ref('')
const countdown = ref(300)
const error = ref('')
const orderNo = ref('')
let pollTimer: number | null = null
let countdownTimer: number | null = null

onMounted(() => {
    // 确保用户已登录
    if (!userStore.isLogin) {
        ElMessage.warning('请先登录')
        router.push('/auth/login')
    }
})

onUnmounted(() => {
    clearAllTimers()
})

// 格式化倒计时
const formatCountdown = (seconds: number) => {
    const m = Math.floor(seconds / 60)
    const s = seconds % 60
    return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`
}

// 打开充值弹窗
const handleRecharge = () => {
    showRechargeDialog.value = true
}

// 充值弹窗关闭回调
const onRechargeDialogClosed = () => {
    clearAllTimers()
    resetState()
}

const resetState = () => {
    qrCodeUrl.value = ''
    error.value = ''
    countdown.value = 300
}

// 生成支付二维码
const generateQRCode = async () => {
    try {
        loading.value = true
        error.value = ''
        countdown.value = 300

        const response = await createPaymentOrder(51)
        qrCodeUrl.value = response.code_url
        orderNo.value = response.outTradeNo || Date.now().toString()

        startCountdown()
        startPolling()
    } catch (err: any) {
        console.error('生成二维码失败:', err)
        error.value = err.message || '生成二维码失败，请重试'
    } finally {
        loading.value = false
    }
}

// 启动倒计时
const startCountdown = () => {
    clearCountdownTimer()
    countdownTimer = window.setInterval(() => {
        countdown.value--
        if (countdown.value <= 0) {
            clearAllTimers()
        }
    }, 1000)
}

// 启动轮询检查支付状态
const startPolling = () => {
    clearPollTimer()
    pollTimer = window.setInterval(async () => {
        await checkPaymentStatus()
    }, 3000)
}

// 检查支付状态
const checkPaymentStatus = async () => {
    try {
        if (orderNo.value) {
            const status = await getPaymentStatus({ outTradeNo: orderNo.value })
            if (status.status === 'PAID') {
                ElMessage.success('支付成功！魔力值已到账')
                clearAllTimers()
                await userStore.getUserInfo()
                closeRechargeDialog()
            }
        }
    } catch (err) {
        console.error('检查支付状态失败:', err)
    }
}

// 清除所有定时器
const clearAllTimers = () => {
    clearPollTimer()
    clearCountdownTimer()
}

const clearPollTimer = () => {
    if (pollTimer) {
        clearInterval(pollTimer)
        pollTimer = null
    }
}

const clearCountdownTimer = () => {
    if (countdownTimer) {
        clearInterval(countdownTimer)
        countdownTimer = null
    }
}

const closeRechargeDialog = () => {
    showRechargeDialog.value = false
}

// 账户设置
const handleSettings = () => {
    router.push('/account-security')
}

// 退出登录
const handleLogout = async () => {
    try {
        await ElMessageBox.confirm('确定要退出登录吗？', '提示', {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            type: 'warning',
        })
        closeRechargeDialog()
        await userStore.logout()
        router.push('/auth/login')
    } catch {
        // 用户取消
    }
}

// 导航
const goToAuction = () => {
    router.push('/chat/auction/auction')
}

const goToTrading = () => {
    router.push('/forum/tradingPost')
}

const goToPaymentHistory = () => {
    ElMessage.info('充值记录功能开发中')
}

const goToDownload = () => {
    router.push('/app-download')
}
</script>

<style lang="scss" scoped>
$primary-color: #833a00;
$primary-light: #ae6f4d;
$bg-light: #fff2e8;
$bg-card: #f3d9b3;
$border-color: #ae6f4d;

.profile-page {
    width: 100%;
    max-width: 800px;
    margin: 0 auto;
    padding: 30px 20px;
}

.page-header {
    text-align: center;
    margin-bottom: 30px;

    .page-title {
        font-size: 28px;
        font-weight: 600;
        color: $primary-color;
        margin: 0 0 8px 0;
    }

    .page-subtitle {
        font-size: 14px;
        color: #666;
        margin: 0;
    }
}

.content-wrapper {
    display: flex;
    flex-direction: column;
    gap: 24px;
}

// 用户信息卡片
.user-info-card {
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 30px;
    background: linear-gradient(135deg, $bg-card 0%, #ffe8d6 100%);
    border: 3px solid $border-color;
    border-radius: 20px;
    box-shadow: 0 4px 12px rgba(131, 58, 0, 0.15);

    .user-avatar-wrapper {
        margin-bottom: 16px;

        .user-avatar {
            border: 3px solid $border-color;
        }
    }

    .user-meta {
        text-align: center;

        .user-name {
            font-size: 24px;
            font-weight: 600;
            color: $primary-color;
            margin: 0 0 8px 0;
        }

        .user-id {
            font-size: 14px;
            color: $primary-light;
            background: #fff;
            padding: 4px 12px;
            border-radius: 12px;
        }
    }
}

// 余额信息
.balance-section {
    display: flex;
    gap: 16px;

    .balance-card {
        flex: 1;
        padding: 20px;
        background: #fff;
        border: 2px solid $border-color;
        border-radius: 12px;
        text-align: center;

        .balance-label {
            font-size: 14px;
            color: #666;
            margin-bottom: 8px;
        }

        .balance-value {
            font-size: 28px;
            font-weight: 600;
            color: $primary-color;
        }

        &.highlight {
            background: linear-gradient(135deg, $bg-card 0%, #ffe8d6 100%);

            .balance-value {
                color: #d02129;
            }
        }
    }
}

// 操作按钮
.action-section {
    display: flex;
    gap: 12px;

    .action-btn {
        flex: 1;
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 8px;
        padding: 14px 0;
        background: $primary-color;
        border-color: $primary-color;
        color: #fff;
        font-size: 15px;
        font-weight: 500;

        &:hover {
            background: darken($primary-color, 10%);
            border-color: darken($primary-color, 10%);
        }

        &.logout-btn {
            background: $primary-light;
            border-color: $primary-light;

            &:hover {
                background: darken($primary-light, 10%);
                border-color: darken($primary-light, 10%);
            }
        }
    }
}

// 功能菜单
.menu-section {
    display: flex;
    flex-direction: column;
    gap: 12px;

    .menu-item {
        display: flex;
        align-items: center;
        gap: 16px;
        padding: 16px 20px;
        background: #fff;
        border: 2px solid $border-color;
        border-radius: 12px;
        cursor: pointer;
        transition: all 0.3s ease;

        &:hover {
            background: $bg-light;
            transform: translateX(4px);
        }

        .menu-icon {
            width: 44px;
            height: 44px;
            display: flex;
            align-items: center;
            justify-content: center;
            background: $primary-color;
            border-radius: 10px;

            .el-icon {
                font-size: 20px;
                color: #fff;
            }
        }

        .menu-content {
            flex: 1;

            .menu-title {
                display: block;
                font-size: 15px;
                font-weight: 600;
                color: $primary-color;
                margin-bottom: 4px;
            }

            .menu-desc {
                display: block;
                font-size: 13px;
                color: $primary-light;
            }
        }

        .menu-arrow {
            font-size: 18px;
            color: $primary-light;
        }
    }
}

// 充值弹窗样式
.recharge-dialog-content {
    .balance-section-dialog {
        text-align: center;
        padding: 20px;
        background: $bg-light;
        border-radius: 12px;
        margin-bottom: 16px;

        .balance-label {
            font-size: 14px;
            color: $primary-light;
            margin-bottom: 8px;
        }

        .balance-amount {
            font-size: 32px;
            font-weight: bold;
            color: $primary-color;
        }
    }

    .amount-section {
        text-align: center;
        padding: 16px;
        background: #f5f5f5;
        border-radius: 12px;
        margin-bottom: 16px;

        .amount-label {
            font-size: 13px;
            color: #666;
            margin-bottom: 4px;
        }

        .amount-value {
            font-size: 24px;
            font-weight: bold;
            color: #52c41a;
        }

        .amount-hint {
            font-size: 12px;
            color: #999;
            margin-top: 4px;
        }
    }

    .qrcode-section {
        text-align: center;
        padding: 16px;
        background: #fff;
        border: 2px dashed $border-color;
        border-radius: 12px;
        margin-bottom: 16px;
        min-height: 200px;
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;

        .loading-state {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 12px;
            color: $primary-light;

            .el-icon {
                font-size: 32px;
                animation: rotating 2s linear infinite;
            }
        }

        .qrcode-display {
            .qrcode-img {
                width: 180px;
                height: 180px;
                border-radius: 8px;
                border: 2px solid $border-color;
            }

            .qrcode-hint {
                margin-top: 12px;
                font-size: 13px;
                color: $primary-light;
            }

            .countdown {
                margin-top: 8px;
                font-size: 12px;
                color: #666;

                &.expired {
                    color: #d02129;
                    font-weight: bold;
                }
            }
        }

        .error-state {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 12px;
            color: #d02129;

            .el-icon {
                font-size: 32px;
            }
        }

        .generate-btn-wrapper {
            .el-button {
                background: $primary-color;
                border-color: $primary-color;

                &:hover {
                    background: darken($primary-color, 10%);
                    border-color: darken($primary-color, 10%);
                }
            }
        }
    }

    .recharge-tips {
        .tips-title {
            font-size: 14px;
            font-weight: 600;
            color: $primary-color;
            margin-bottom: 8px;
        }

        .tips-list {
            list-style: none;
            padding: 0;
            margin: 0;

            li {
                font-size: 12px;
                color: $primary-light;
                padding: 4px 0;
                padding-left: 16px;
                position: relative;

                &::before {
                    content: '•';
                    position: absolute;
                    left: 6px;
                    color: $primary-color;
                }
            }
        }
    }
}

@keyframes rotating {
    from {
        transform: rotate(0deg);
    }
    to {
        transform: rotate(360deg);
    }
}

@media (max-width: 768px) {
    .profile-page {
        padding: 20px 15px;
    }

    .balance-section {
        flex-direction: column;
    }

    .action-section {
        flex-direction: column;
    }
}
</style>
