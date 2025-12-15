<template>
    <div class="trading-post-card">
                <div class="content-wrapper">
            <!-- 标题栏 -->
            <div class="title-bar">
                <!-- 左侧容器：标题和描述 -->
                <div class="title-container">
                    <!-- 标题图片 -->
                    <img class="title-image" src="@/assets/images/title_trade.png" alt="交易站"/>

                    <!-- 描述文字 -->
                    <div class="description">
                        商品多多任你挑选
                    </div>
                </div>

                <!-- 右侧操作按钮 -->
                <button class="action-button">
                    <span>点击进入</span>
                </button>
            </div>

            <!-- 内容区域 -->
            <div class="content-area">
                <!-- 新闻列表 -->
                <div class="news-container">
                    <!-- 标题区域 -->
                    <div class="news-header">
                        <span class="news-title">最新发布</span>
                    </div>

                    <!-- 列表内容 -->
                    <div class="news-list">
                        <div class="news-item">
                            <div class="news-content">
                                <div class="diamond"></div>
                                <span class="news-text">游戏公告内容游戏公告内容游戏公告内容游戏公</span>
                            </div>
                            <span class="news-date">24/03/01</span>
                        </div>
                        <div class="news-item">
                            <div class="news-content">
                                <div class="diamond"></div>
                                <span class="news-text">游戏公告内容游戏公告内容游戏公告内容游戏公</span>
                            </div>
                            <span class="news-date">24/03/01</span>
                        </div>
                        <div class="news-item">
                            <div class="news-content">
                                <div class="diamond"></div>
                                <span class="news-text">游戏公告内容游戏公告内容游戏公告内容游戏公</span>
                            </div>
                            <span class="news-date">24/03/01</span>
                        </div>
                        <div class="news-item">
                            <div class="news-content">
                                <div class="diamond"></div>
                                <span class="news-text">游戏公告内容游戏公告内容游戏公告内容游戏公</span>
                            </div>
                            <span class="news-date">24/03/01</span>
                        </div>
                        <div class="news-item">
                            <div class="news-content">
                                <div class="diamond"></div>
                                <span class="news-text">游戏公告内容游戏公告内容游戏公告内容游戏公</span>
                            </div>
                            <span class="news-date">24/03/01</span>
                        </div>
                        <div class="news-item">
                            <div class="news-content">
                                <div class="diamond"></div>
                                <span class="news-text">游戏公告内容游戏公告内容游戏公告内容游戏公</span>
                            </div>
                            <span class="news-date">24/03/01</span>
                        </div>
                        <div class="news-item">
                            <div class="news-content">
                                <div class="diamond"></div>
                                <span class="news-text">游戏公告内容游戏公告内容游戏公告内容游戏公</span>
                            </div>
                            <span class="news-date">24/03/01</span>
                        </div>
                        <div class="news-item">
                            <div class="news-content">
                                <div class="diamond"></div>
                                <span class="news-text">游戏公告内容游戏公告内容游戏公告内容游戏公</span>
                            </div>
                            <span class="news-date">24/03/01</span>
                        </div>
                        <div class="news-item">
                            <div class="news-content">
                                <div class="diamond"></div>
                                <span class="news-text">游戏公告内容游戏公告内容游戏公告内容游戏公</span>
                            </div>
                            <span class="news-date">24/03/01</span>
                        </div>
                        <div class="news-item">
                            <div class="news-content">
                                <div class="diamond"></div>
                                <span class="news-text">游戏公告内容游戏公告内容游戏公告内容游戏公</span>
                            </div>
                            <span class="news-date">24/03/01</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
// 交易站卡片组件
</script>

<style lang="scss" scoped>
.trading-post-card {
    width: 630px;
    min-height: 760px;
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;

    // 使用伪元素作为背景图层，不挤占内容空间
    border: none;

    &::before {
        content: '';
        position: absolute;
        top: -150px;
        left: 0;
        right: 0;
        bottom: -150px;
        background: url("@/assets/images/panel_background.png") no-repeat;
        background-size: 100% 100%;
        background-position: center;
        z-index: -1; // 确保在内容下方
    }

    .content-wrapper {
        position: relative;
        z-index: 1;
        width: 100%;
        height: 100%;
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: start;
        padding: 150px 0 20px; // 为上下背景装饰预留空间

        .title-bar {
            display: flex;
            flex-direction: row;
            justify-content: space-between;
            align-items: center;
            //padding: 0 72px 0;
            //margin-left: 72px;
            //margin-right: 72px;
            width: 100%;
            //height: 46px;

            .title-container {
                display: flex;
                flex-direction: row;
                align-items: center;
                margin-left: 60px;
                gap: 20px;
                //width: 257px;
                height: 46px;

                flex: none;
                order: 0;
                flex-grow: 0;

                .title-image {
                    width: auto;
                    height: 40px; /* 充满容器高度 */
                    object-fit: contain;
                    flex: none;
                    order: 0;
                    flex-grow: 0;
                }

                .description {
                    height: 16px;

                    font-family: 'Source Han Sans CN', serif;
                    font-style: normal;
                    font-weight: 400;
                    font-size: 16px;
                    line-height: 100%;
                    color: #5B3B2D;

                    flex: none;
                    order: 1;
                    flex-grow: 0;
                }
            }

            .action-button {
                display: flex;
                flex-direction: row;
                justify-content: center;
                align-items: center;
                padding: 8px 12px;
                gap: 10px;
                margin-right: 55px;
                width: 80px;
                height: 30px;

                background: #62331E;
                border: none;
                border-radius: 60px;
                cursor: pointer;
                transition: all 0.3s ease;

                flex: none;
                order: 1;
                flex-grow: 0;

                span {
                    width: 56px;
                    height: 14px;

                    font-family: 'Source Han Sans CN';
                    font-style: normal;
                    font-weight: 500;
                    font-size: 14px;
                    line-height: 100%;
                    color: #E6AC7A;

                    flex: none;
                    order: 0;
                    flex-grow: 0;
                }

                &:hover {
                    background: #7A4326;
                    transform: scale(1.05);
                }
            }
        }

        .content-area {
            flex: 1;
            display: flex;
            flex-direction: column;
            align-items: flex-start;
            margin-top: 30px;


            .news-container {
                display: flex;
                flex-direction: column;
                align-items: flex-start;
                padding: 0px;
                gap: 20px;

                width: 470px;
                //height: 470px;

                .news-header {
                    width: 116px;
                    height: 35px;
                    background: linear-gradient(90deg, #74422C 0%, #D89476 82.76%);
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    border-radius: 5px;

                    .news-title {
                        font-family: 'Source Han Sans CN';
                        font-style: normal;
                        font-weight: 700;
                        font-size: 18px;
                        line-height: 100%;
                        color: #5B3B2D;
                    }
                }

                .news-list {
                    display: flex;
                    flex-direction: column;
                    align-items: flex-start;
                    gap: 15px;
                    overflow-y: auto;

                    width: 470px;

                    .news-item {
                        display: flex;
                        flex-direction: row;
                        align-items: flex-start;
                        gap: 10px;

                        width: 470px;
                        height: 31px;
                        border-bottom: 1px dashed #976464;

                        .news-content {
                            display: flex;
                            flex-direction: row;
                            align-items: center;
                            padding: 0px;
                            gap: 10px;

                            //width: 354px;
                            height: 16px;

                            .diamond {
                                width: 5.66px;
                                height: 5.66px;
                                background: #E6AC7A;
                                transform: rotate(45deg);
                                flex: none;
                                order: 0;
                                flex-grow: 0;
                            }

                            .news-text {
                                //width: 336px;
                                height: 16px;
                                font-family: 'Source Han Sans CN';
                                font-style: normal;
                                font-weight: 400;
                                font-size: 16px;
                                line-height: 100%;
                                color: #CCA396;
                                flex: none;
                                order: 1;
                                flex-grow: 0;
                                overflow: hidden;
                                text-overflow: ellipsis;
                                white-space: nowrap;
                            }
                        }

                        .news-date {
                            //width: 66px;
                            height: 16px;
                            font-family: 'Source Han Sans CN';
                            font-style: normal;
                            font-weight: 400;
                            font-size: 16px;
                            line-height: 100%;
                            color: #BD8775;
                            flex: none;
                            order: 1;
                            flex-grow: 0;
                        }
                    }
                }
            }
        }
    }
}
</style>
