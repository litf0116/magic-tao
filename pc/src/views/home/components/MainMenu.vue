<template>
    <div class="main-menu">
        <div class="menu-container">
            <!-- 导航按钮组 -->
            <div class="nav-buttons">
                <button
                    v-for="(item, index) in menuItems"
                    :key="item.path"
                    :class="['nav-button', { 'active': isActiveRoute(item.path) }]"
                    @click="navigateTo(item.path)"
                >
                    <span class="button-text">{{ item.name }}</span>
                </button>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { useRouter, useRoute } from 'vue-router'

const router = useRouter()
const route = useRoute()

interface MenuItem {
    name: string
    path: string
    icon?: string
}

const menuItems: MenuItem[] = [
    { name: '首页', path: '/' },
    { name: '交易站', path: '/forum/tradingPost' },
    { name: '拍卖行', path: '/a' },
    { name: '魔力淘橱窗', path: '/b' },
    { name: '魔力宝贝官网', path: '/c' },
    { name: '魔力百科资料站', path: '/d' },
    { name: '自助中介系统', path: '/e' }
]

const navigateTo = (path: string) => {
    if (path === '/') {
        router.push('/index')
    } else {
        router.push(path)
    }
}

const isActiveRoute = (path: string): boolean => {
    if (path === '/') {
        return route.path === '/index' || route.path === '/'
    }
    return route.path === path
}
</script>

<style lang="scss" scoped>
.main-menu {
    position: relative;
    width: 100%;
    margin: 40px 0;

    .menu-container {
        max-width: 1300px;
        margin: 0 auto;
        padding: 0 20px;

        .nav-buttons {
            display: flex;
            flex-direction: row;
            align-items: center;
            justify-content: center;
            gap: 10px;
            padding: 20px 30px;
            background: #FFF2E8;
            border: 14px solid #AE6F4D;
            border-radius: 20px;

            .nav-button {
                position: relative;
                width: 170px;
                height: 56px;
                background: linear-gradient(180deg, #FFCC00 2.5%, #F07A13 93.38%);
                border: none;
                border-radius: 8px;
                cursor: pointer;
                transition: all 0.3s ease;
                overflow: hidden;

                /* 游戏风格按钮效果 */
                &::before {
                    content: '';
                    position: absolute;
                    top: 0;
                    left: 0;
                    right: 0;
                    height: 53.82%;
                    background: linear-gradient(90deg, #FFCC96 0%, #9B7C5B 35%, #47392A 67%, #14100B 89%, #000000 100%);
                    background-blend-mode: screen;
                    mix-blend-mode: screen;
                    opacity: 0.61;
                    pointer-events: none;
                }

                /* 按钮边框效果 */
                &::after {
                    content: '';
                    position: absolute;
                    top: 15.3%;
                    left: 0;
                    right: 0;
                    bottom: 5.32%;
                    border-radius: 4px;
                    pointer-events: none;
                }

                .button-text {
                    position: relative;
                    z-index: 2;
                    font-family: 'Source Han Sans CN', sans-serif;
                    font-weight: 700;
                    font-size: 18px;
                    line-height: 100%;
                    color: #833A00;
                    text-align: center;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    width: 100%;
                    height: 100%;
                }

                /* 首页按钮特殊样式 - 红色主题 */
                &:first-child {
                    background: linear-gradient(180deg, #FF8484 2.5%, #FF2424 93.38%);

                    .button-text {
                        color: #FFFFFF;
                    }
                }

                /* 悬停效果 */
                &:hover {
                    transform: translateY(-2px);
                    box-shadow: 0 8px 16px rgba(0, 0, 0, 0.2);

                    .button-text {
                        color: #FFFFFF;
                        text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.3);
                    }
                }

                /* 激活状态 */
                &.active {
                    background: linear-gradient(180deg, #57BBF9 0%, #3583EC 100%);
                    box-shadow: 0 4px 12px rgba(53, 131, 236, 0.3);

                    .button-text {
                        color: #FFFFFF;
                        font-weight: 800;
                    }
                }

                /* 点击效果 */
                &:active {
                    transform: translateY(0);
                    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
                }
            }
        }
    }
}

/* 响应式适配 */
@media (max-width: 1920px) {
    .main-menu {
        margin: 35px 0;

        .menu-container {
            .nav-buttons {
                padding: 18px 25px;
                gap: 8px;

                .nav-button {
                    width: 150px;
                    height: 50px;

                    .button-text {
                        font-size: 16px;
                    }
                }
            }
        }
    }
}

@media (max-width: 1440px) {
    .main-menu {
        margin: 30px 0;

        .menu-container {
            .nav-buttons {
                padding: 15px 20px;
                gap: 6px;
                flex-wrap: wrap;

                .nav-button {
                    width: 140px;
                    height: 45px;

                    .button-text {
                        font-size: 15px;
                    }
                }
            }
        }
    }
}

@media (max-width: 768px) {
    .main-menu {
        margin: 25px 0;

        .menu-container {
            padding: 0 15px;

            .nav-buttons {
                padding: 12px 15px;
                gap: 8px;
                border-width: 10px;
                flex-direction: column;
                align-items: stretch;

                .nav-button {
                    width: 100%;
                    height: 50px;
                    max-width: 300px;
                    margin: 0 auto;

                    .button-text {
                        font-size: 16px;
                    }

                    &:hover {
                        transform: translateY(-1px);
                    }
                }
            }
        }
    }
}

@media (max-width: 480px) {
    .main-menu {
        .menu-container {
            .nav-buttons {
                padding: 10px 12px;

                .nav-button {
                    height: 45px;
                    max-width: 250px;

                    .button-text {
                        font-size: 14px;
                    }
                }
            }
        }
    }
}
</style>