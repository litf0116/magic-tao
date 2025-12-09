<template>
    <div class="banner-section">
        <!-- 横幅内容 -->
        <div class="banner-content">
            <!-- 标题区域 -->
            <div class="title-area">
                <div class="logo-icons">
                    <div class="icon-container">
                        <div class="icon white-border"></div>
                        <div class="icon blue-border"></div>
                        <div class="icon gradient"></div>
                    </div>
                </div>
            </div>

            <!-- 丝带标题 -->
            <div class="ribbon-title">
                <div class="ribbon">
                    <span class="title-text">魔力宝贝怀旧服交流交易平台</span>
                </div>
            </div>
        </div>

        <!-- 侧边装饰 -->
        <div class="side-decorations">
            <!-- 左侧装饰 -->
            <div class="left-decoration">
                <div class="decoration-group">
                    <img src="@/assets/left-character.png" alt="左侧角色" class="character-image" />
                </div>
            </div>

            <!-- 右侧装饰 -->
            <div class="right-decoration">
                <div class="decoration-group">
                    <img src="@/assets/right-character.png" alt="右侧角色" class="character-image" />
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
// 横幅组件无需额外逻辑
</script>

<style lang="scss" scoped>
.banner-section {
    position: relative;
    width: 100%;
    max-width: 1920px;
    margin: 0 auto;
    height: 350px;
    margin-top: 60px;

    .banner-content {
        position: relative;
        z-index: 10;
        text-align: center;

        .title-area {
            position: relative;
            margin-bottom: 20px;

            .logo-icons {
                position: relative;
                width: 317px;
                height: 146.45px;
                margin: 0 auto;

                .icon-container {
                    position: relative;
                    width: 260.77px;
                    height: 114.32px;
                    margin: 0 auto;

                    .icon {
                        position: absolute;
                        width: 68px;
                        height: 68px;
                        border-radius: 50%;

                        &.white-border {
                            left: 14.41%;
                            border: 8px solid #FFFFFF;
                            top: 23.65%;
                        }

                        &.blue-border {
                            left: 41.98%;
                            border: 8px solid #69BFFF;
                            top: 25.08%;
                        }

                        &.gradient {
                            left: 69.08%;
                            background: linear-gradient(180deg, #57BBF9 0%, #3583EC 100%);
                            top: 25.34%;
                        }
                    }
                }
            }
        }

        .ribbon-title {
            position: relative;
            width: 317px;
            height: 48.2px;
            margin: 0 auto;

            .ribbon {
                position: absolute;
                width: 100%;
                height: 100%;
                background-image: url('@/assets/ribbon.png');
                background-size: cover;
                background-position: center;
                display: flex;
                align-items: center;
                justify-content: center;

                .title-text {
                    font-family: 'Source Han Sans CN', sans-serif;
                    font-weight: 900;
                    font-size: 18.5px;
                    color: #FFFFFF;
                    text-align: center;
                    border: 1.85px solid #D5282F;
                    padding: 4px 8px;
                    background: rgba(0, 0, 0, 0.6);
                }
            }
        }
    }

    .side-decorations {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        pointer-events: none;

        .left-decoration,
        .right-decoration {
            position: absolute;
            top: 0;
            height: 100%;

            .decoration-group {
                position: relative;
                height: 100%;

                .character-image {
                    height: 100%;
                    width: auto;
                    object-fit: contain;
                }
            }
        }

        .left-decoration {
            left: 36px;

            .decoration-group {
                width: 276.28px;
            }
        }

        .right-decoration {
            right: 36px;

            .decoration-group {
                width: 249.72px;
            }
        }
    }
}

/* 响应式适配 */
@media (max-width: 1920px) {
    .banner-section {
        height: 280px;

        .banner-content {
            .title-area {
                .logo-icons {
                    width: 254px;
                    height: 117px;

                    .icon-container {
                        width: 209px;
                        height: 91px;

                        .icon {
                            width: 54px;
                            height: 54px;
                            border-width: 6px;
                        }
                    }
                }
            }

            .ribbon-title {
                width: 254px;
                height: 38px;

                .ribbon {
                    .title-text {
                        font-size: 15px;
                        padding: 3px 6px;
                    }
                }
            }
        }

        .side-decorations {
            .left-decoration {
                left: 30px;

                .decoration-group {
                    width: 220px;
                }
            }

            .right-decoration {
                right: 30px;

                .decoration-group {
                    width: 200px;
                }
            }
        }
    }
}

@media (max-width: 1440px) {
    .banner-section {
        height: 240px;
        margin-top: 50px;

        .banner-content {
            .title-area {
                .logo-icons {
                    width: 220px;
                    height: 100px;

                    .icon-container {
                        width: 180px;
                        height: 78px;

                        .icon {
                            width: 48px;
                            height: 48px;
                            border-width: 5px;
                        }
                    }
                }
            }

            .ribbon-title {
                width: 220px;
                height: 32px;

                .ribbon {
                    .title-text {
                        font-size: 14px;
                    }
                }
            }
        }

        .side-decorations {
            .left-decoration {
                left: 20px;

                .decoration-group {
                    width: 180px;
                }
            }

            .right-decoration {
                right: 20px;

                .decoration-group {
                    width: 160px;
                }
            }
        }
    }
}

@media (max-width: 768px) {
    .banner-section {
        height: 200px;
        margin-top: 40px;

        .banner-content {
            .title-area {
                .logo-icons {
                    width: 180px;
                    height: 80px;

                    .icon-container {
                        width: 150px;
                        height: 65px;

                        .icon {
                            width: 40px;
                            height: 40px;
                            border-width: 4px;
                        }
                    }
                }
            }

            .ribbon-title {
                width: 180px;
                height: 28px;

                .ribbon {
                    .title-text {
                        font-size: 12px;
                        padding: 2px 4px;
                    }
                }
            }
        }

        .side-decorations {
            .left-decoration {
                left: 10px;

                .decoration-group {
                    width: 140px;
                }
            }

            .right-decoration {
                right: 10px;

                .decoration-group {
                    width: 120px;
                }
            }
        }
    }
}
</style>