<template>
    <div class="auction-card">
        <!-- 背景包装器 - 与卡片同尺寸 -->
        <div class="background-wrapper"></div>

        <!-- 内容包装器 -->
        <div class="content-wrapper">
            <!-- 标题栏 -->
            <div class="title-bar">
                <!-- 左侧容器：标题和描述 -->
                <div class="title-container">
                    <!-- 标题图片 -->
                    <img class="title-image" src="@/assets/images/title_auction.png" alt="拍卖行"/>

                    <!-- 描述文字 -->
                    <div class="description">
                        每晚7：30-12：30
                    </div>
                </div>

                <!-- 右侧操作按钮 -->
                <button class="action-button" @click="goToAuction">
                    <span>点击进入</span>
                </button>
            </div>

            <!-- 内容区域 -->
            <div class="content-area">
                <!-- 拍品列表 -->
                <div class="auction-container">
                    <!-- 标题区域 -->
                    <div class="auction-header">
                        <span class="auction-title">最新拍品</span>
                    </div>

                    <!-- 列表内容 -->
                    <div class="auction-list" v-loading="loading">
                        <div
                            v-for="item in formatAuctionItems"
                            :key="item.id"
                            class="auction-item"
                        >
                            <div class="auction-content">
                                <div class="diamond"></div>
                                <span class="auction-text">{{ item.name }}</span>
                            </div>
                            <span class="auction-date">{{ item.date }}</span>
                        </div>

                        <!-- 空状态处理 -->
                        <div v-if="formatAuctionItems.length === 0 && !loading" class="empty-state">
                            暂无待拍商品
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuctionStore } from '@/stores/auctionStore'
import { ref, onMounted, computed } from 'vue'

// 拍卖行卡片组件
const router = useRouter()
const auctionStore = useAuctionStore()

// 跳转到拍卖行页面
const goToAuction = () => {
    router.push('/chat/auction/auction')
}

// 响应式数据
const loading = ref(false)
const waitList = computed(() => auctionStore.list) // 待拍卖商品列表

// 获取最新10条待拍商品
const getLatestAuctionItems = async () => {
    loading.value = true
    try {
        await auctionStore.getList(undefined, 10) // 获取10条待拍卖商品
    } catch (error) {
        console.error('获取拍卖商品失败:', error)
    } finally {
        loading.value = false
    }
}

// 格式化商品数据
const formatAuctionItems = computed(() => {
    return waitList.value.map(item => ({
        id: item.id,
        name: item.name || '未知商品',
        date: formatDate(item.createdAt || new Date()),
        imageUrl: item.imageUrl,
        seller: item.sellerInfo || '未知卖家'
    }))
})

// 格式化日期为 24/03/01 格式
const formatDate = (dateStr) => {
    if (!dateStr) return ''
    const date = new Date(dateStr)
    const year = date.getFullYear() % 100 // 取后两位
    const month = String(date.getMonth() + 1).padStart(2, '0')
    const day = String(date.getDate()).padStart(2, '0')
    return `${year}/${month}/${day}`
}

// 组件挂载时获取数据
onMounted(() => {
    getLatestAuctionItems()
})
</script>

<style lang="scss" scoped>
.auction-card {
    width: 623px;
    min-height: 491px;
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;

    // 使用background-wrapper实现背景拉伸，与卡片同尺寸
    border: none;

    .background-wrapper {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: calc(100% - 80px);
        // 使用border-image实现背景拉伸 上下border 150 左右 border 0
        border: 100px solid transparent;
        border-left: none;
        border-right: none;
        border-image: url("@/assets/images/panel_background.png") 100 0 100 0 fill stretch;
        pointer-events: none; // 不阻挡用户交互
        z-index: 0;
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
        padding-top: 20px; // 恢复原有padding

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

            .auction-container {
                display: flex;
                flex-direction: column;
                align-items: flex-start;
                padding: 0px;
                gap: 20px;

                width: 470px;
                //height: 470px;

                .auction-header {
                    width: 116px;
                    height: 35px;
                    background: linear-gradient(90deg, #74422C 0%, #D89476 82.76%);
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    border-radius: 5px;

                    .auction-title {
                        font-family: 'Source Han Sans CN';
                        font-style: normal;
                        font-weight: 700;
                        font-size: 18px;
                        line-height: 100%;
                        color: #5B3B2D;
                    }
                }

                .auction-list {
                    display: flex;
                    flex-direction: column;
                    align-items: flex-start;
                    gap: 15px;
                    overflow-y: auto;

                    width: 470px;

                    .auction-item {
                        display: flex;
                        align-items: center;
                        gap: 10px;

                        width: 470px;
                        height: 31px;
                        border-bottom: 1px dashed #976464;

                        .auction-content {
                            display: flex;
                            align-items: center;
                            gap: 10px;
                            flex: 1;
                            min-width: 0; /* 确保flex子项可以收缩 */
                            overflow: hidden; /* 确保内容不会溢出 */

                            .diamond {
                                width: 5.66px;
                                height: 5.66px;
                                background: #E6AC7A;
                                transform: rotate(45deg);
                                flex-shrink: 0;
                            }

                            .auction-text {
                                height: 16px;
                                font-family: 'Source Han Sans CN';
                                font-style: normal;
                                font-weight: 400;
                                font-size: 16px;
                                line-height: 100%;
                                color: #CCA396;
                                overflow: hidden;
                                text-overflow: ellipsis;
                                white-space: nowrap;
                                flex: 1; /* 占据剩余空间 */
                                min-width: 0; /* 允许收缩到最小宽度 */
                            }
                        }

                        .auction-date {
                            height: 16px;
                            font-family: 'Source Han Sans CN';
                            font-style: normal;
                            font-weight: 400;
                            font-size: 16px;
                            line-height: 100%;
                            color: #BD8775;
                            flex-shrink: 0; /* 防止时间文字被压缩 */
                            flex-basis: 80px; /* 确保足够宽度显示完整日期 */
                            min-width: 80px; /* 最小宽度保证时间显示 */
                            max-width: 80px; /* 最大宽度保持一致性 */
                            text-align: right;
                            overflow: visible; /* 确保时间文字不被隐藏 */
                        }
                    }

                    .empty-state {
                        width: 100%;
                        height: 31px;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        font-family: 'Source Han Sans CN';
                        font-style: normal;
                        font-weight: 400;
                        font-size: 14px;
                        color: #BD8775;
                    }
                }
            }
        }
    }
}
</style>
