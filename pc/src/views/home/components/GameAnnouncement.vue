<template>
    <div class="game-announcement">
        <!-- 公告标题 -->
        <div class="announcement-header">
            <div class="title-badge">
                <span class="title-text">最新发布</span>
            </div>
        </div>

        <!-- 公告内容 -->
        <div class="announcement-content">
            <div class="announcement-list">
                <div
                    v-for="(announcement, index) in announcements"
                    :key="index"
                    class="announcement-item"
                >
                    <div class="item-content">
                        <div class="bullet-point"></div>
                        <span class="announcement-text">{{ announcement.title }}</span>
                    </div>
                    <div class="announcement-date">{{ formatDate(announcement.date) }}</div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

interface Announcement {
    title: string
    date: string
    id?: number
}

const announcements = ref<Announcement[]>([
    {
        title: '游戏公告内容游戏公告内容游戏公告内容游戏公',
        date: '2024/03/01',
        id: 1
    },
    {
        title: '游戏公告内容游戏公告内容游戏公告内容游戏公告',
        date: '2024/03/01',
        id: 2
    },
    {
        title: '游戏公告内容游戏公告内容游戏公告内容游戏公告内容',
        date: '2024/03/01',
        id: 3
    },
    {
        title: '游戏公告内容游戏公告内容游戏公告内容游戏公告',
        date: '2024/03/01',
        id: 4
    },
    {
        title: '游戏公告内容游戏公告内容游戏公告内容游戏公告',
        date: '2024/03/01',
        id: 5
    }
])

const formatDate = (dateString: string): string => {
    const date = new Date(dateString)
    const month = String(date.getMonth() + 1).padStart(2, '0')
    const day = String(date.getDate()).padStart(2, '0')
    return `${month}/${day}`
}
</script>

<style lang="scss" scoped>
.game-announcement {
    width: 470px;
    height: 270px;
    padding: 20px;
    background: #FFF8EF;
    border: 2px solid #F2DC9F;
    border-radius: 10px;
    box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.1);
    position: relative;
    overflow: hidden;

    /* 布局边框装饰 */
    &::before {
        content: '';
        position: absolute;
        top: 0;
        left: 0;
        width: 19px;
        height: 100%;
        background: linear-gradient(179.93deg, #C59986 0.06%, #A26E58 107.59%);
        z-index: 1;
    }

    &::after {
        content: '';
        position: absolute;
        top: 0;
        right: 0;
        width: 19px;
        height: 100%;
        background: linear-gradient(179.93deg, #C59986 0.06%, #A26E58 107.59%);
        z-index: 1;
    }

    .announcement-header {
        position: relative;
        z-index: 2;
        margin-bottom: 20px;
        text-align: center;

        .title-badge {
            display: inline-block;
            width: 116px;
            height: 35px;
            background: linear-gradient(90deg, #74422C 0%, #D89476 82.76%);
            border-radius: 4px;
            position: relative;

            .title-text {
                font-family: 'Source Han Sans CN', sans-serif;
                font-weight: 700;
                font-size: 18px;
                line-height: 100%;
                color: #5B3B2D;
                position: absolute;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
                width: 72px;
                text-align: center;
            }
        }
    }

    .announcement-content {
        position: relative;
        z-index: 2;
        height: calc(100% - 60px);

        .announcement-list {
            display: flex;
            flex-direction: column;
            gap: 15px;
            height: 100%;
            overflow-y: auto;

            .announcement-item {
                display: flex;
                align-items: center;
                justify-content: space-between;
                padding-bottom: 15px;
                border-bottom: 1px dashed #976464;

                &:last-child {
                    border-bottom: none;
                    padding-bottom: 0;
                }

                .item-content {
                    display: flex;
                    align-items: center;
                    gap: 10px;
                    flex: 1;
                    max-width: 354px;

                    .bullet-point {
                        width: 5.66px;
                        height: 5.66px;
                        background: #E6AC7A;
                        transform: rotate(45deg);
                        flex-shrink: 0;
                    }

                    .announcement-text {
                        font-family: 'Source Han Sans CN', sans-serif;
                        font-weight: 400;
                        font-size: 16px;
                        line-height: 100%;
                        color: #CCA396;
                        overflow: hidden;
                        text-overflow: ellipsis;
                        white-space: nowrap;
                        flex: 1;
                    }
                }

                .announcement-date {
                    font-family: 'Source Han Sans CN', sans-serif;
                    font-weight: 400;
                    font-size: 16px;
                    line-height: 100%;
                    color: #BD8775;
                    white-space: nowrap;
                    flex-shrink: 0;
                }
            }
        }
    }
}

/* 响应式适配 */
@media (max-width: 1920px) {
    .game-announcement {
        width: 420px;
        height: 240px;
        padding: 18px;

        .announcement-header {
            margin-bottom: 18px;

            .title-badge {
                width: 105px;
                height: 32px;

                .title-text {
                    font-size: 16px;
                    width: 65px;
                }
            }
        }

        .announcement-content {
            height: calc(100% - 55px);

            .announcement-list {
                gap: 13px;

                .announcement-item {
                    padding-bottom: 13px;

                    .item-content {
                        gap: 9px;
                        max-width: 320px;

                        .bullet-point {
                            width: 5px;
                            height: 5px;
                        }

                        .announcement-text {
                            font-size: 15px;
                        }
                    }

                    .announcement-date {
                        font-size: 15px;
                    }
                }
            }
        }
    }
}

@media (max-width: 1440px) {
    .game-announcement {
        width: 380px;
        height: 220px;
        padding: 15px;

        .announcement-header {
            margin-bottom: 15px;

            .title-badge {
                width: 95px;
                height: 28px;

                .title-text {
                    font-size: 14px;
                    width: 58px;
                }
            }
        }

        .announcement-content {
            height: calc(100% - 48px);

            .announcement-list {
                gap: 12px;

                .announcement-item {
                    padding-bottom: 12px;

                    .item-content {
                        gap: 8px;
                        max-width: 280px;

                        .bullet-point {
                            width: 4.5px;
                            height: 4.5px;
                        }

                        .announcement-text {
                            font-size: 14px;
                        }
                    }

                    .announcement-date {
                        font-size: 14px;
                    }
                }
            }
        }
    }
}

@media (max-width: 768px) {
    .game-announcement {
        width: 100%;
        max-width: 350px;
        height: 200px;
        padding: 12px;
        margin: 0 auto;

        .announcement-header {
            margin-bottom: 12px;

            .title-badge {
                width: 85px;
                height: 25px;

                .title-text {
                    font-size: 13px;
                    width: 52px;
                }
            }
        }

        .announcement-content {
            height: calc(100% - 42px);

            .announcement-list {
                gap: 10px;

                .announcement-item {
                    padding-bottom: 10px;

                    .item-content {
                        gap: 7px;
                        max-width: 250px;

                        .bullet-point {
                            width: 4px;
                            height: 4px;
                        }

                        .announcement-text {
                            font-size: 13px;
                        }
                    }

                    .announcement-date {
                        font-size: 13px;
                    }
                }
            }
        }
    }
}
</style>