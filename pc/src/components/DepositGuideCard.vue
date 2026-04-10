<template>
    <div class="deposit-guide-card">
        <div class="card-content">
            <!-- 左侧图标 -->
            <div class="icon-section">
                <div class="icon-wrapper">
                    <el-icon class="deposit-icon"><Wallet /></el-icon>
                </div>
            </div>

            <!-- 中间文字 -->
            <div class="text-section">
                <h3 class="title">魔力值充值</h3>
                <p class="description">
                    <span v-if="userStore.isLogin">
                        当前魔力值：<strong class="balance">¥{{ userStore.user.depositBalance || 0 }}</strong>
                    </span>
                    <span v-else> 参与竞拍需要缴纳保证金，支持微信扫码支付 </span>
                </p>
            </div>

            <!-- 右侧按钮 -->
            <div class="action-section">
                <el-button type="primary" class="recharge-btn" @click="handleRecharge">
                    <el-icon><Plus /></el-icon>
                    立即充值
                </el-button>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { Wallet, Plus } from '@element-plus/icons-vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const userStore = useUserStore()

const handleRecharge = () => {
    // 跳转到统一支付页面
    router.push({
        path: '/payment',
        query: {
            type: 'deposit',
        },
    })
}
</script>

<style lang="scss" scoped>
// 网站主色调 - 暖色系复古游戏风格
$primary-color: #833a00;
$primary-light: #ae6f4d;
$bg-light: #fff2e8;
$bg-card: #f3d9b3;
$border-color: #ae6f4d;

.deposit-guide-card {
    width: 100%;
    max-width: 1232px;
    margin: 20px auto;
    animation: fadeIn 0.5s ease;
}

@keyframes fadeIn {
    from {
        opacity: 0;
        transform: translateY(-10px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.card-content {
    display: flex;
    align-items: center;
    gap: 20px;
    padding: 20px 30px;
    background: linear-gradient(135deg, $bg-card 0%, #ffe8d6 100%);
    border: 3px solid $border-color;
    border-radius: 16px;
    box-shadow: 0 4px 12px rgba(131, 58, 0, 0.15);
    transition: all 0.3s ease;

    &:hover {
        transform: translateY(-2px);
        box-shadow: 0 6px 20px rgba(131, 58, 0, 0.25);
    }

    @media (max-width: 768px) {
        flex-direction: column;
        padding: 16px 20px;
        gap: 12px;
        text-align: center;
    }
}

.icon-section {
    flex-shrink: 0;

    .icon-wrapper {
        width: 60px;
        height: 60px;
        display: flex;
        align-items: center;
        justify-content: center;
        background: $primary-color;
        border-radius: 12px;
        border: 2px solid darken($primary-color, 10%);

        @media (max-width: 768px) {
            width: 50px;
            height: 50px;
        }
    }

    .deposit-icon {
        font-size: 32px;
        color: #fff;

        @media (max-width: 768px) {
            font-size: 26px;
        }
    }
}

.text-section {
    flex: 1;

    .title {
        font-size: 20px;
        font-weight: 600;
        color: $primary-color;
        margin: 0 0 6px 0;

        @media (max-width: 768px) {
            font-size: 18px;
            margin-bottom: 4px;
        }
    }

    .description {
        font-size: 14px;
        color: $primary-light;
        margin: 0;

        @media (max-width: 768px) {
            font-size: 13px;
        }
    }

    .balance {
        color: $primary-color;
        font-size: 16px;

        @media (max-width: 768px) {
            font-size: 15px;
        }
    }
}

.action-section {
    flex-shrink: 0;

    .recharge-btn {
        padding: 12px 24px;
        background: $primary-color;
        border-color: $primary-color;
        font-size: 15px;
        font-weight: 600;
        border-radius: 10px;
        transition: all 0.3s ease;

        &:hover {
            background: darken($primary-color, 10%);
            border-color: darken($primary-color, 10%);
            transform: scale(1.05);
        }

        &:active {
            transform: scale(0.98);
        }

        .el-icon {
            margin-right: 4px;
        }

        @media (max-width: 768px) {
            padding: 10px 20px;
            font-size: 14px;
        }
    }
}

// 移动端适配
@media (max-width: 768px) {
    .deposit-guide-card {
        margin: 15px auto;
        padding: 0 10px;
    }
}
</style>
