<template>
    <div class="left-sidebar-tools">
        <!-- 主工具容器 -->
        <div class="main-wrapper">
            <!-- 悬浮功能列 -->
            <div class="tools-container">
                <!-- 魔力值充值 -->
                <div class="tool-item" @click="toggleRechargePanel">
                    <div class="tool-icon">
                        <el-icon><Wallet /></el-icon>
                    </div>
                    <span class="tool-label">充值</span>
                    <!-- 余额提示 -->
                    <div v-if="userStore.isLogin && userStore.user.depositBalance > 0" class="balance-badge">
                        ¥{{ userStore.user.depositBalance }}
                    </div>
                </div>

                <!-- 拍卖行 -->
                <div class="tool-item" @click="goToAuction">
                    <div class="tool-icon">
                        <el-icon><Trophy /></el-icon>
                    </div>
                    <span class="tool-label">拍卖</span>
                </div>

                <!-- 交易站 -->
                <div class="tool-item" @click="goToTrading">
                    <div class="tool-icon">
                        <el-icon><Shop /></el-icon>
                    </div>
                    <span class="tool-label">交易</span>
                </div>

                <!-- 个人中心 -->
                <div class="tool-item" @click="goToProfile">
                    <div class="tool-icon">
                        <el-icon><User /></el-icon>
                    </div>
                    <span class="tool-label">我的</span>
                </div>

                <!-- 回到顶部 -->
                <div v-show="showBackTop" class="tool-item back-top" @click="scrollToTop">
                    <div class="tool-icon">
                        <el-icon><ArrowUp /></el-icon>
                    </div>
                    <span class="tool-label">顶部</span>
                </div>
            </div>
        </div>

        <!-- 用户设置弹窗 -->
        <UserSetting ref="userSettingRef" />
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { Wallet, Trophy, Shop, User, ArrowUp } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import UserSetting from '@/components/UserSetting.vue'

const router = useRouter()
const userStore = useUserStore()
const userSettingRef = ref<InstanceType<typeof UserSetting> | null>(null)

// 状态管理
const showBackTop = ref(false)

// 监听滚动显示回到顶部按钮
const handleScroll = () => {
    showBackTop.value = window.scrollY > 300
}

onMounted(() => {
    window.addEventListener('scroll', handleScroll)
})

onUnmounted(() => {
    window.removeEventListener('scroll', handleScroll)
})

// 回到顶部
const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' })
}

// 导航到支付页面
const toggleRechargePanel = () => {
    if (!userStore.isLogin) {
        router.push('/auth/login?redirect=/payment')
        return
    }
    router.push({
        path: '/payment',
        query: {
            type: 'deposit',
            returnUrl: '/chat',
        },
    })
}

// 去拍卖行
const goToAuction = () => {
    router.push('/chat/auction/auction')
}

// 去交易站
const goToTrading = () => {
    router.push('/forum/tradingPost')
}

// 去个人中心
const goToProfile = () => {
    if (!userStore.isLogin) {
        ElMessage.info('请先登录')
        router.push('/auth/login')
        return
    }
    userSettingRef.value?.show(true)
}
</script>

<style lang="scss" scoped>
// 网站主色调 - 暖色系复古游戏风格
$primary-color: #833a00;
$primary-light: #ae6f4d;
$bg-light: #fff2e8;
$bg-card: #f3d9b3;
$border-color: #ae6f4d;

.left-sidebar-tools {
    position: fixed;
    left: 20px;
    top: 50%;
    transform: translateY(-50%);
    z-index: 100;

    @media (max-width: 1400px) {
        left: 10px;
    }

    @media (max-width: 1200px) {
        display: none;
    }
}

.main-wrapper {
    display: flex;
    transition: all 0.3s ease;
}

.tools-container {
    display: flex;
    flex-direction: column;
    gap: 12px;
    padding: 16px 12px;
    background: linear-gradient(135deg, $bg-card 0%, #ffe8d6 100%);
    border: 3px solid $border-color;
    border-radius: 16px;
    box-shadow: 0 4px 12px rgba(131, 58, 0, 0.15);
    z-index: 2;
}

.tool-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 6px;
    padding: 12px 8px;
    cursor: pointer;
    border-radius: 12px;
    transition: all 0.3s ease;
    position: relative;

    &:hover {
        background: rgba(131, 58, 0, 0.15);
        transform: translateY(-2px);

        .tool-icon {
            transform: scale(1.1);
        }
    }

    &:active {
        transform: scale(0.95);
    }
}

.tool-icon {
    width: 44px;
    height: 44px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: $primary-color;
    border-radius: 50%;
    border: 2px solid darken($primary-color, 10%);
    transition: all 0.3s ease;

    .el-icon {
        font-size: 22px;
        color: #fff;
    }
}

.tool-label {
    font-size: 12px;
    font-weight: 600;
    color: $primary-color;
    white-space: nowrap;
}

// 余额徽章
.balance-badge {
    position: absolute;
    top: -4px;
    right: -4px;
    background: #d02129;
    color: #fff;
    font-size: 10px;
    font-weight: bold;
    padding: 2px 6px;
    border-radius: 10px;
    border: 1px solid #fff;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

// 回到顶部按钮
.back-top {
    margin-top: 8px;
    border-top: 2px dashed $border-color;
    padding-top: 16px;

    .tool-icon {
        background: $primary-light;
        border-color: darken($primary-light, 10%);
    }
}
</style>
