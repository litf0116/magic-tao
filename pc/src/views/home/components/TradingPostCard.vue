<template>
    <div class="trading-post-card">
        <!-- 卡片标题区域 -->
        <div class="card-header">
            <div class="title-section">
                <h2 class="main-title">交易站</h2>
                <p class="subtitle">商品多多任你挑选</p>
            </div>
            <div class="action-button">
                <el-button @click="handleEnterTrading" type="primary" class="enter-btn">
                    点击进入
                </el-button>
            </div>
        </div>

        <!-- 卡片内容区域 - 今日清单 -->
        <div class="card-content">
            <div class="content-section">
                <div class="section-title">
                    <span class="title-text">今日清单</span>
                </div>
                <div class="items-list">
                    <div
                        v-for="(item, index) in todayItems"
                        :key="index"
                        class="list-item"
                    >
                        <div class="item-content">
                            <div class="bullet-point"></div>
                            <span class="item-text">{{ item.name }}</span>
                        </div>
                    </div>
                </div>
            </div>

            <!-- 快速统计信息 -->
            <div class="stats-section">
                <div class="section-title">
                    <span class="title-text">今日清单</span>
                </div>
                <div class="items-list">
                    <div
                        v-for="(item, index) in quickStats"
                        :key="`stat-${index}`"
                        class="list-item"
                    >
                        <div class="item-content">
                            <div class="bullet-point"></div>
                            <span class="item-text">{{ item.name }}</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

interface TradingItem {
    name: string
    count?: number
}

const todayItems = ref<TradingItem[]>([
    { name: '游戏公告内容游戏公告', count: 15 },
    { name: '游戏公告内容游戏公告内容', count: 23 },
    { name: '游戏公告内容游戏公告内容', count: 8 },
    { name: '游戏公告内容游戏公告内容', count: 12 },
    { name: '游戏公告内容游戏公告内容', count: 5 }
])

const quickStats = ref<TradingItem[]>([
    { name: '游戏公告内容游戏公告' },
    { name: '游戏公告内容游戏公告内容' },
    { name: '游戏公告内容游戏公告内容' },
    { name: '游戏公告内容游戏公告内容' },
    { name: '游戏公告内容游戏公告内容' }
])

const handleEnterTrading = () => {
    router.push('/forum/tradingPost')
}
</script>

<style lang="scss" scoped>
.trading-post-card {
    width: 623px;
    height: 475px;
    position: relative;

    /* 外层阴影 */
    &::before {
        content: '';
        position: absolute;
        width: 623px;
        height: 416px;
        left: 0;
        top: 44px;
        background: linear-gradient(180deg, #5D2D29 95.09%, #532F31 100%);
        border-radius: 30px;
        z-index: 1;
    }

    /* 底部模糊阴影 */
    &::after {
        content: '';
        position: absolute;
        width: 559px;
        height: 88px;
        left: 32px;
        top: 387px;
        background: linear-gradient(180deg, rgba(93, 45, 41, 0.4) 95.09%, rgba(83, 47, 49, 0.4) 100%);
        filter: blur(8px);
        border-radius: 20px;
        z-index: 1;
    }

    .card-header {
        position: absolute;
        width: 520px;
        height: 46px;
        left: 51px;
        top: 0;
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 20px;
        z-index: 3;

        .title-section {
            display: flex;
            align-items: center;
            gap: 20px;

            .main-title {
                font-family: 'DymonShouXieTi', cursive;
                font-weight: 400;
                font-size: 40px;
                line-height: 100%;
                letter-spacing: -0.14em;
                background: linear-gradient(171.09deg, #FFF268 17.27%, #FF9D2D 92.92%);
                -webkit-background-clip: text;
                -webkit-text-fill-color: transparent;
                background-clip: text;
                text-shadow: 0px 4px 0px #AC5D33;
                margin: 0;
            }

            .subtitle {
                font-family: 'Source Han Sans CN', sans-serif;
                font-weight: 400;
                font-size: 16px;
                line-height: 100%;
                color: #5B3B2D;
                margin: 0;
            }
        }

        .action-button {
            .enter-btn {
                background: #62331E;
                border: none;
                border-radius: 60px;
                color: #E6AC7A;
                font-family: 'Source Han Sans CN', sans-serif;
                font-weight: 500;
                font-size: 14px;
                padding: 8px 12px;
                transition: all 0.3s ease;

                &:hover {
                    background: #7a4327;
                    color: #FFFFFF;
                    transform: translateY(-1px);
                }
            }
        }
    }

    .card-content {
        position: absolute;
        width: 584px;
        height: 397px;
        left: 19px;
        top: 58px;
        background: #E6AC7A;
        box-shadow: 0px 4px 8px rgba(102, 23, 23, 0.25);
        border-radius: 16px;
        z-index: 2;
        display: flex;
        justify-content: space-between;
        align-items: flex-end;
        padding: 20px;

        /* 内部边框装饰 */
        &::before {
            content: '';
            position: absolute;
            width: 546px;
            height: 330px;
            left: 19px;
            top: 20px;
            background: #FFF8EF;
            border: 2px solid #F2DC9F;
            filter: blur(0.5px);
            border-radius: 10px;
            z-index: 1;
        }

        /* 左侧装饰边框 */
        &::after {
            content: '';
            position: absolute;
            width: 19px;
            height: 345px;
            left: 19px;
            top: 20px;
            background: linear-gradient(179.93deg, #C59986 0.06%, #A26E58 107.59%);
            z-index: 2;
        }

        .content-section,
        .stats-section {
            position: relative;
            z-index: 3;
            width: 210px;
            height: 270px;

            .section-title {
                margin-bottom: 20px;

                .title-text {
                    display: inline-block;
                    width: 116px;
                    height: 35px;
                    background: linear-gradient(90deg, #74422C 0%, #D89476 82.76%);
                    font-family: 'Source Han Sans CN', sans-serif;
                    font-weight: 700;
                    font-size: 18px;
                    line-height: 35px;
                    color: #74422C;
                    text-align: center;
                    border-radius: 4px;
                }
            }

            .items-list {
                display: flex;
                flex-direction: column;
                gap: 15px;

                .list-item {
                    border-bottom: 1px dashed #976464;
                    padding-bottom: 15px;

                    &:last-child {
                        border-bottom: none;
                        padding-bottom: 0;
                    }

                    .item-content {
                        display: flex;
                        align-items: center;
                        gap: 10px;

                        .bullet-point {
                            width: 5.66px;
                            height: 5.66px;
                            background: #E6AC7A;
                            transform: rotate(45deg);
                            flex-shrink: 0;
                        }

                        .item-text {
                            font-family: 'Source Han Sans CN', sans-serif;
                            font-weight: 400;
                            font-size: 16px;
                            line-height: 100%;
                            color: #CCA396;
                            overflow: hidden;
                            text-overflow: ellipsis;
                            white-space: nowrap;
                        }
                    }
                }
            }
        }

        .content-section {
            align-self: flex-start;
        }

        .stats-section {
            align-self: flex-end;
        }
    }
}

/* 响应式适配 */
@media (max-width: 1920px) {
    .trading-post-card {
        width: 560px;
        height: 428px;

        &::before {
            width: 560px;
            height: 374px;
        }

        &::after {
            width: 503px;
            height: 79px;
            left: 29px;
            top: 349px;
        }

        .card-header {
            width: 468px;
            height: 41px;
            left: 46px;

            .title-section {
                .main-title {
                    font-size: 36px;
                }

                .subtitle {
                    font-size: 14px;
                }
            }
        }

        .card-content {
            width: 526px;
            height: 357px;
            left: 17px;
            top: 52px;
            padding: 18px;

            &::before {
                width: 491px;
                height: 297px;
                left: 17px;
                top: 18px;
            }

            &::after {
                width: 17px;
                height: 311px;
                left: 17px;
                top: 18px;
            }

            .content-section,
            .stats-section {
                width: 190px;
                height: 243px;

                .section-title {
                    margin-bottom: 18px;

                    .title-text {
                        width: 105px;
                        height: 32px;
                        font-size: 16px;
                        line-height: 32px;
                    }
                }

                .items-list {
                    gap: 13px;

                    .list-item {
                        padding-bottom: 13px;

                        .item-content {
                            gap: 9px;

                            .bullet-point {
                                width: 5px;
                                height: 5px;
                            }

                            .item-text {
                                font-size: 14px;
                            }
                        }
                    }
                }
            }
        }
    }
}

@media (max-width: 1440px) {
    .trading-post-card {
        width: 500px;
        height: 380px;

        &::before {
            width: 500px;
            height: 334px;
        }

        &::after {
            width: 448px;
            height: 71px;
            left: 26px;
            top: 309px;
        }

        .card-header {
            width: 416px;
            height: 37px;
            left: 42px;

            .title-section {
                .main-title {
                    font-size: 32px;
                }

                .subtitle {
                    font-size: 13px;
                }
            }
        }

        .card-content {
            width: 467px;
            height: 317px;
            left: 16px;
            top: 46px;
            padding: 15px;

            &::before {
                width: 437px;
                height: 263px;
                left: 15px;
                top: 15px;
            }

            &::after {
                width: 15px;
                height: 276px;
                left: 15px;
                top: 15px;
            }

            .content-section,
            .stats-section {
                width: 170px;
                height: 215px;

                .section-title {
                    margin-bottom: 15px;

                    .title-text {
                        width: 95px;
                        height: 28px;
                        font-size: 14px;
                        line-height: 28px;
                    }
                }

                .items-list {
                    gap: 12px;

                    .list-item {
                        padding-bottom: 12px;

                        .item-content {
                            gap: 8px;

                            .bullet-point {
                                width: 4.5px;
                                height: 4.5px;
                            }

                            .item-text {
                                font-size: 13px;
                            }
                        }
                    }
                }
            }
        }
    }
}

@media (max-width: 768px) {
    .trading-post-card {
        width: 100%;
        max-width: 400px;
        height: auto;
        margin: 0 auto;

        &::before {
            width: 100%;
            height: 267px;
            border-radius: 20px;
        }

        &::after {
            width: 90%;
            height: 57px;
            left: 5%;
            top: 210px;
            border-radius: 15px;
        }

        .card-header {
            width: 90%;
            height: auto;
            left: 5%;
            top: 0;
            flex-direction: column;
            align-items: flex-start;
            gap: 15px;

            .title-section {
                flex-direction: column;
                align-items: flex-start;
                gap: 10px;

                .main-title {
                    font-size: 28px;
                }

                .subtitle {
                    font-size: 12px;
                }
            }

            .action-button {
                width: 100%;

                .enter-btn {
                    width: 100%;
                }
            }
        }

        .card-content {
            width: calc(100% - 30px);
            height: auto;
            left: 15px;
            top: 120px;
            flex-direction: column;
            padding: 15px;

            &::before {
                width: calc(100% - 30px);
                height: auto;
                min-height: 200px;
                left: 15px;
                top: 15px;
            }

            &::after {
                width: 15px;
                height: calc(100% - 30px);
                left: 15px;
                top: 15px;
            }

            .content-section,
            .stats-section {
                width: 100%;
                height: auto;
                margin-bottom: 20px;

                .items-list {
                    .list-item {
                        .item-content {
                            .item-text {
                                white-space: normal;
                                line-height: 1.4;
                            }
                        }
                    }
                }
            }
        }
    }
}
</style>