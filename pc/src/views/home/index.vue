<template>
    <div class="magic-homepage">
        <!-- 背景层 -->
        <Background />

        <!-- 头部导航 -->
        <HeaderNavigation />

        <!-- 主要内容区域 -->
        <div class="main-wrapper">
            <!-- 横幅区域 -->
            <BannerSection />

            <!-- 主导航菜单 -->
            <MainMenu />

            <!-- 内容展示区域 -->
            <div class="content-area">
                <!-- 左侧游戏公告 -->
                <div class="left-section">
                    <GameAnnouncement />
                </div>

                <!-- 中间卡片区域 -->
                <div class="center-section">
                    <TradingPostCard />
                </div>

                <!-- 右侧拍卖卡片 -->
                <div class="right-section">
                    <AuctionCard />
                </div>
            </div>

            <!-- 底部信息栏 -->
            <FooterInfo />
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Background from './components/Background.vue'
import HeaderNavigation from './components/HeaderNavigation.vue'
import BannerSection from './components/BannerSection.vue'
import MainMenu from './components/MainMenu.vue'
import GameAnnouncement from './components/GameAnnouncement.vue'
import TradingPostCard from './components/TradingPostCard.vue'
import AuctionCard from './components/AuctionCard.vue'
import FooterInfo from './components/FooterInfo.vue'

const router = useRouter()

onMounted(() => {
    // 初始化数据
    document.title = '魔力淘 - 首页'
})

const toLobby = () => {
    router.push({ path: 'forum/tradingPost' })
}

const toAuction = () => {
    router.push({ name: 'auction' })
}
</script>

<style lang="scss" scoped>
.magic-homepage {
    position: relative;
    width: 100%;
    min-height: 100vh;
    overflow-x: hidden;

    .main-wrapper {
        position: relative;
        z-index: 10;
        width: 100%;
        max-width: 1920px;
        margin: 0 auto;

        .content-area {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            width: 100%;
            padding: 40px 20px;
            gap: 30px;
            margin-top: 20px;

            .left-section {
                flex: 0 0 auto;
                width: 470px;
                max-width: 25%;
            }

            .center-section {
                flex: 0 0 auto;
                width: 623px;
                max-width: 35%;
            }

            .right-section {
                flex: 0 0 auto;
                width: 623px;
                max-width: 35%;
            }
        }
    }
}

/* 响应式适配 */
@media (max-width: 1920px) {
    .magic-homepage {
        .main-wrapper {
            .content-area {
                padding: 35px 20px;
                gap: 25px;

                .left-section {
                    width: 420px;
                }

                .center-section,
                .right-section {
                    width: 560px;
                }
            }
        }
    }
}

@media (max-width: 1440px) {
    .magic-homepage {
        .main-wrapper {
            .content-area {
                flex-direction: column;
                align-items: center;
                padding: 30px 20px;
                gap: 30px;

                .left-section,
                .center-section,
                .right-section {
                    width: 100%;
                    max-width: 623px;
                    flex: 1 1 auto;
                }
            }
        }
    }
}

@media (max-width: 768px) {
    .magic-homepage {
        .main-wrapper {
            .content-area {
                padding: 20px 15px;
                gap: 25px;

                .left-section,
                .center-section,
                .right-section {
                    max-width: 100%;
                }
            }
        }
    }
}
</style>
