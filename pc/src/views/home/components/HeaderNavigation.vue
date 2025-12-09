<template>
    <header class="header-navigation">
        <div class="header-content">
            <!-- Logo区域 -->
            <div class="logo-section">
                <img src="@/assets/logo.png" alt="魔力淘" class="logo-image" />
            </div>

            <!-- 用户操作区域 -->
            <div class="user-section">
                <div class="user-info" v-if="userStore.user">
                    <span class="username">{{ userStore.user.username }}</span>
                    <el-button @click="handleLogout" size="small" type="danger">退出</el-button>
                </div>
                <div class="auth-buttons" v-else>
                    <el-button @click="handleLogin" size="small">登录</el-button>
                    <el-button @click="handleRegister" size="small" type="primary">注册</el-button>
                </div>
            </div>
        </div>
    </header>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'

const router = useRouter()
const userStore = useUserStore()

const handleLogin = () => {
    router.push('/auth/login')
}

const handleRegister = () => {
    router.push('/register')
}

const handleLogout = async () => {
    await userStore.logout()
    router.push('/auth/login')
}
</script>

<style lang="scss" scoped>
.header-navigation {
    position: relative;
    z-index: 100;
    background: rgba(255, 255, 255, 0.95);
    backdrop-filter: blur(10px);
    border-bottom: 1px solid rgba(0, 0, 0, 0.1);

    .header-content {
        max-width: 1920px;
        margin: 0 auto;
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 12px 40px;

        .logo-section {
            .logo-image {
                height: 50px;
                width: auto;
                object-fit: contain;
            }
        }

        .user-section {
            display: flex;
            align-items: center;
            gap: 16px;

            .user-info {
                display: flex;
                align-items: center;
                gap: 12px;

                .username {
                    font-family: 'Source Han Sans CN', sans-serif;
                    font-weight: 500;
                    color: #5B3B2D;
                    font-size: 14px;
                }
            }

            .auth-buttons {
                display: flex;
                gap: 12px;
            }
        }
    }
}

/* 响应式适配 */
@media (max-width: 768px) {
    .header-navigation {
        .header-content {
            padding: 10px 20px;

            .logo-section {
                .logo-image {
                    height: 40px;
                }
            }

            .user-section {
                gap: 12px;

                .auth-buttons {
                    gap: 8px;
                }
            }
        }
    }
}
</style>