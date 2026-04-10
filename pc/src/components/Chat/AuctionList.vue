<template>
    <div class="flex-1" style="max-height: 700px" @contextmenu.prevent>
        <div class="h-24 border-0 border-b-1 border-solid border-gray-400 overflow-hidden">
            <announce-div :category-id="2" />
        </div>
        <div id="ps_container" class="relative overflow-hidden px-1 relative" :style="{ height: getListHeight() }">
            <div class="flex my-1 sticky top-0 left-0 right-0">
                <el-radio-group v-model="activeName" fill="#f4835a" text="#fff">
                    <el-radio-button value="1">待拍卖</el-radio-button>
                    <!-- <el-radio-button value="2">拍卖中</el-radio-button> -->
                    <el-radio-button value="3">已成交</el-radio-button>
                </el-radio-group>
            </div>
            <template v-if="activeName === '1'">
                <div class="grid grid-cols-1 gap-2">
                    <list-auction-item v-for="x in waitList" :key="x.id" :item="x" @showDetail="showDetail" />
                    <div class="h-4"></div>
                </div>
            </template>
            <!-- <template v-else-if="activeName === '2'">
                <div class="grid grid-cols-1 gap-2">
                    <list-auction-item
                        v-for="x in auctionStore.auctionMid"
                        :key="x.id"
                        :item="x"
                        @showDetail="showDetail"
                    />
                    <div class="h-4"></div>
                </div>
            </template> -->
            <template v-else-if="activeName === '3'">
                <div class="grid grid-cols-1 gap-2">
                    <list-auction-item v-for="x in auctionStore.list4" :key="x.id" :item="x" @showDetail="showDetail" />
                    <div class="h-4"></div>
                </div>
            </template>
        </div>

        <div
            v-if="onAuctionItem && onAuctionItem.id"
            v-motion-fade-visible
            class="h-250px flex flex-col justify-between relative"
            style="height: 250px"
        >
            <!-- {{ onAuctionItem }} -->
            <div class="relative h-48 overflow-hidden" @click.stop="showDetail(onAuctionItem.id)">
                <img
                    :src="convertImageUrl(onAuctionItem.imageUrl)"
                    class="w-full h-48 cursor-pointer object-cover"
                    @click.stop="showDetail(onAuctionItem.id)"
                />
                <div class="absolute bottom-0 left-0 right-0 bg-dark/80 z-2 h-24">
                    <div class="line-clamp-2 text-white text-sm h-12 flex flex-center px-2">
                        <b>{{ onAuctionItem.name }}</b>
                    </div>
                    <div v-if="onAuctionItem.currentPrice" class="px-2 h-12 text-white text-sm flex flex-col">
                        <div>
                            <span class="text-white">当前价：</span>
                            <b class="text-[#F4835A]">￥{{ onAuctionItem.currentPrice }}</b>
                        </div>
                        <div>
                            <span class="text-white">出价人：</span>
                            <b class="text-[#F4835A]">{{ onAuctionItem.currentPriceUserName }}</b>
                        </div>
                    </div>
                </div>
            </div>
            <div class="p-2">
                <el-button color="#f4835a" class="w-full" @click="bid">
                    <div class="text-lg font-700 text-white">出价</div>
                </el-button>
            </div>
        </div>
        <!-- <div class="absolute top-0 left-0 bg-[#f4835a] text-white shadow rounded-rb-lg px-2 font-bold">
                正在竞拍
            </div> -->
        <!-- </div> -->
        <div class="fixed bottom-24 right-4 flex flex-col z-99 space-y-4">
            <!-- //管理员菜单 -->
            <template v-if="userStore.isAuctionAdmin">
                <!-- 卡秒按钮 -->
                <el-tooltip v-if="onAuctionItem" :content="isKasec ? '关闭卡秒' : '开启卡秒'" effect="customized">
                    <div class="bg-blue-5 size-12 rounded-full flex flex-center cursor-pointer mb-2">
                        <div
                            :class="['i-mdi:timer', isKasec ? 'text-red-500' : 'text-white']"
                            @click.stop="toggleKasec"
                        ></div>
                    </div>
                </el-tooltip>
                <!-- 结束竞拍按钮 -->
                <el-tooltip v-if="onAuctionItem" content="结束当前竞拍并发送得主" effect="customized">
                    <div class="bg-red-5 size-12 rounded-full flex flex-center cursor-pointer">
                        <div class="i-carbon:stop-filled-alt size-6 text-white" @click.stop="end"></div>
                    </div>
                </el-tooltip>
                <!-- 新增拍品按钮 -->
                <el-tooltip content="新增拍品" effect="customized">
                    <div class="bg-orange-5 size-12 rounded-full flex flex-center cursor-pointer">
                        <div class="i-mdi:add size-6 text-white" @click.stop="addNew"></div>
                    </div>
                </el-tooltip>
            </template>
            <!-- //用户菜单 -->
            <el-tooltip content="刷新拍品" effect="customized">
                <div class="bg-gray-3 size-12 rounded-full flex flex-center cursor-pointer">
                    <div class="i-mdi:refresh size-6 text-white" @click.stop="() => auctionStore.getList()"></div>
                </div>
            </el-tooltip>
        </div>

        <edit-auction-item ref="editRef" @on-saved="auctionStore.getList()" />
        <auction-item-detail ref="detailRef" @onEdit="edit" />
        <withdrawalApprovaltem ref="withdrawalApprovaRef" @on-saved="auctionStore.getList()" />
        <addMsgConfiguration ref="addMsgRef" />
    </div>
</template>

<script lang="ts" setup name="AuctionList">
import editAuctionItem from '@/components/Chat/editAuctionItem.vue'
import announceDiv from '@/components/Chat/announceDiv.vue'
import auctionItemDetail from '@/components/Chat/auctionItemDetail.vue'
import listAuctionItem from '@/components/Chat/listAuctionItem.vue'
import PerfectScrollbar from 'perfect-scrollbar'
import withdrawalApprovaltem from '@/components/Chat/withdrawalApprovaltem.vue'
import addMsgConfiguration from '@/components/Chat/addMsgConfiguration.vue'
import { ElMessage, ElMessageBox, ElLoading } from 'element-plus'
import { Tips } from '@/composables'
import { useChatStore } from '@/stores/chatStore'
import { ChatMessageType } from '@/api/appService'
import { GetUserLevelInfo } from '@/api/groupChatLevel'
import type { UserLevelInfo } from '@/api/groupChatLevel'
import api from '@/api'
import type { UserDto } from '@/api/appService'
import { calculateMinBidPrice } from '@/utils/auction'
import { useRouter } from 'vue-router'

let ps: PerfectScrollbar | null = null

const router = useRouter()
const userStore = useUserStore()

const auctionStore = useAuctionStore()
const editRef = ref<InstanceType<typeof editAuctionItem> | null>(null)
const detailRef = ref<InstanceType<typeof auctionItemDetail> | null>(null)
const withdrawalApprovaRef = ref<InstanceType<typeof auctionItemDetail> | null>(null)
const addMsgRef = ref<InstanceType<typeof editAuctionItem> | null>(null)
const isKasec = computed(() => auctionStore.isKasec)
import { ref, computed, onMounted, watch } from 'vue'
import { useUserStore } from '@/stores/userStore'
import { useAuctionStore } from '@/stores/auctionStore'
import { ElRadioGroup, ElRadioButton, ElButton } from 'element-plus'
import { convertImageUrl } from '@/utils/imageUrlConverter'

const activeName = ref('1')

const waitList = computed(() => {
    return auctionStore.list.filter((item) => item.status === '上架')
})

const onAuctionItem = computed(() => {
    return auctionStore.list.find((item) => item.status === '拍卖中') || null
})

const chatStore = useChatStore()

// 注入图片查看器
const showImageViewer = inject('showImageViewer') as (list: string[]) => void

// 处理图片点击事件
const handleImageClick = (imageUrl: string) => {
    if (!imageUrl) return
    // 移除缩略图参数，获取原图
    let src = convertImageUrl(imageUrl.replace(/!w300$/, ''))
    showImageViewer([src])
}

onMounted(() => {
    ps = new PerfectScrollbar('#ps_container', {
        wheelSpeed: 1,
        wheelPropagation: false,
        minScrollbarLength: 20,
    })
    auctionStore.getList().then(() => {
        ps!.update()
        if (onAuctionItem.value && onAuctionItem.value.id) {
            auctionStore.syncKasecStatus(onAuctionItem.value.id)
        }
    })
})

watch(
    () => activeName.value,
    (val) => {
        // 重置索引计数器（如果有需要的话）
        // normalIndex = 0

        if (val === '1') {
            auctionStore.getList().then(() => {
                ps!.update()
            })
        } else if (val === '2') {
            auctionStore.getList(2).then(() => {
                ps!.update()
            })
        } else if (val === '3') {
            auctionStore.getList(4).then(() => {
                ps!.update()
            })
        }
    }
)

watch(
    () => onAuctionItem.value && onAuctionItem.value.id,
    (id) => {
        if (id) auctionStore.syncKasecStatus(id)
    }
)

// ANCHOR 管理员Function Start
//添加商品
function addNew() {
    editRef.value?.show(true, 0)
}
//编辑商品
function edit(id: number) {
    editRef.value?.show(true, id)
}

//提现审批
function withdrawalApproval() {
    withdrawalApprovaRef.value?.show(true, 0)
}
//添加消息配置
function addMsg() {
    addMsgRef.value?.show(true, 0)
}

// ANCHOR 管理员Function End

function showDetail(id: number) {
    detailRef.value?.show(true, id)
}

function getListHeight() {
    return onAuctionItem.value ? '354px' : '624px'
}

// getItemIndex 函数已移除，序号现在存储在 item.displayIndex 中

//LINK - 结束竞拍
function end() {
    if (!onAuctionItem.value || !onAuctionItem.value.id) {
        ElMessage.error('没有正在拍卖的商品或商品ID无效')
        return
    }
    ElMessageBox.confirm('确定结束当前竞拍并发送得主？', '结束竞拍', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
    })
        .then(async () => {
            const success = await auctionStore.end(onAuctionItem.value!.id!)
            if (success) {
                // 刷新拍卖列表
                await auctionStore.getList()
            }
        })
        .catch(() => {
            Tips.info('取消结束')
        })
}
// LINK 出价
async function bid() {
    const userId = userStore.user.id
    console.log('开始出价流程 - 用户ID:', userId)

    try {
        console.log('正在获取拍卖列表...')
        await auctionStore.getList()
        if (!onAuctionItem.value) {
            console.log('没有正在拍卖的商品')
            ElMessage.error('没有正在拍卖的商品')
            return
        }

        console.log('当前拍卖商品信息:', {
            id: onAuctionItem.value.id,
            name: onAuctionItem.value.name,
            currentPrice: onAuctionItem.value.currentPrice,
            status: onAuctionItem.value.status,
        })

        // 获取实时用户信息
        console.log('正在获取实时用户信息...')
        const currentUser = await api.user.get({ id: userId })
        const deposit = currentUser.depositBalance || 0
        console.log('用户信息获取成功:', {
            userId: currentUser.id,
            userName: currentUser.userName,
            depositBalance: deposit,
            isActive: currentUser.isActive,
        })

        // 使用工具方法计算最低出价
        const minPrice = calculateMinBidPrice(onAuctionItem.value.currentPrice, auctionStore.isKasec)
        console.log('计算最低出价:', minPrice)

        // 获取用户等级信息
        console.log('正在获取用户等级信息...')
        const levelResponse = await GetUserLevelInfo(userId)
        const levelInfo = levelResponse.data
        const userLevel = levelInfo?.levelSettings?.level ?? 0
        const cumulativeAmount = levelInfo?.userLevel?.cumulativeAmount ?? 0
        console.log('用户等级信息:', {
            userLevel,
            cumulativeAmount,
            levelSettings: levelInfo?.levelSettings,
            userLevelInfo: levelInfo?.userLevel,
        })

        if (userLevel === 0 && deposit < 50) {
            console.log('新用户保证金不足:', { userLevel, deposit })
            ElMessageBox.confirm('新用户参与拍卖，需要缴纳 51 元（50 元保证金 +1 元提现手续费）。', '出价须知', {
                confirmButtonText: '去缴纳',
                cancelButtonText: '取消',
                type: 'warning',
            })
                .then(() => {
                    router.push({
                        path: '/payment',
                        query: {
                            type: 'deposit',
                            returnUrl: '/chat/auction',
                            returnContext: JSON.stringify({
                                auctionItemId: onAuctionItem.value.id,
                                bidPrice: minPrice,
                            }),
                        },
                    })
                })
                .catch(() => {
                    // 用户点击取消，不显示提示
                })
            return
        }

        // 原有出价弹窗逻辑

        let message = `请输入出价金额(最低出价${minPrice})`
        let dialogTitle = '出价'

        if (auctionStore.isKasec) {
            dialogTitle = '卡秒出价 - 需三倍加价'
            message =
                `<div style='color:red;border:1px solid red;padding:4px;margin-bottom:8px;'>您已卡秒出价，需加够三倍加价才有效（最低出价：${minPrice}）</div>` +
                message
        }

        console.log('显示出价弹窗...')
        ElMessageBox.prompt(message, dialogTitle, {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            inputPattern: /\d+/,
            inputValue: '',
            inputType: 'number',
            inputErrorMessage: '请输入正确的金额',
            dangerouslyUseHTMLString: true,
        })
            .then(({ value }) => {
                console.log('用户输入出价金额:', value)
                const bidAmount = parseInt(value)

                // 验证最低出价为5R
                if (bidAmount < 5) {
                    ElMessage.error('最低出价为5R，请重新出价')
                    return
                }

                auctionStore.bid(onAuctionItem.value!.id!, bidAmount)
            })
            .catch(() => {
                console.log('用户取消出价')
                Tips.info('取消出价')
            })
    } catch (error) {
        console.error('出价过程发生错误:', error)
        ElMessage.error('获取用户信息失败，请稍后重试')
    }
}

// 构建卡秒确认消息的辅助函数
function buildKasecConfirmMessage(auctionItem: any, isKasec: boolean) {
    const currentPrice = auctionItem.currentPrice || auctionItem.startingPrice
    const currentBidder = auctionItem.currentPriceUserName || '暂无出价'
    const action = isKasec ? '开启' : '关闭'
    const tipText = isKasec ? '开启后用户需以三倍最低加价进行出价' : '关闭后将恢复正常加价规则'

    return `
    <div style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; color: #303133;">
        <div style="margin-bottom: 16px;">
            <div style="font-size: 16px; font-weight: 600; color: #303133; margin-bottom: 8px;">拍品信息</div>
            <div style="background: #f5f7fa; border: 1px solid #e4e7ed; border-radius: 4px; padding: 12px;">
                <div style="margin-bottom: 8px;">
                    <span style="color: #909399; font-size: 13px;">拍品名称：</span>
                    <span style="color: #303133; font-size: 14px; font-weight: 500;">${auctionItem.name}</span>
                </div>
                <div style="margin-bottom: 8px;">
                    <span style="color: #909399; font-size: 13px;">当前价格：</span>
                    <span style="color: #f4835a; font-size: 16px; font-weight: 600;">￥${currentPrice}</span>
                </div>
                <div>
                    <span style="color: #909399; font-size: 13px;">当前出价人：</span>
                    <span style="color: #303133; font-size: 14px;">${currentBidder}</span>
                </div>
            </div>
        </div>

        <div style="background: ${isKasec ? '#fef0f0' : '#f0f9ff'}; border: 1px solid ${
        isKasec ? '#fbc4c4' : '#b3d8ff'
    }; border-radius: 4px; padding: 12px;">
            <div style="color: #606266; font-size: 13px; line-height: 1.5;">
                <span style="color: ${isKasec ? '#f56c6c' : '#409eff'}; font-weight: 600;">操作提示：</span>
                ${tipText}
            </div>
        </div>
    </div>`
}

async function toggleKasec() {
    console.log('=== 开始卡秒操作 ===')
    console.log('当前拍品信息:', onAuctionItem.value)
    console.log('当前卡秒状态:', auctionStore.isKasec)

    if (!onAuctionItem.value || !onAuctionItem.value.id) {
        ElMessage.error('没有正在拍卖的商品或商品ID无效')
        return
    }

    const isKasec = !auctionStore.isKasec
    const action = isKasec ? '开启' : '关闭'
    const type = isKasec ? 'warning' : 'info'

    console.log('卡秒操作参数:', {
        auctionItemId: onAuctionItem.value.id,
        currentKasec: auctionStore.isKasec,
        targetKasec: isKasec,
        action: action,
        type: type,
    })

    try {
        // 构建确认消息
        const message = buildKasecConfirmMessage(onAuctionItem.value, isKasec)
        console.log('确认框消息已构建')

        // 显示确认框 - 使用更简单的配置
        console.log('准备显示确认框...')
        await ElMessageBox.confirm(message, `${action}卡秒模式`, {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            type: type,
            dangerouslyUseHTMLString: true,
        })
        console.log('用户已确认操作')

        // 用户确认后执行操作
        console.log('开始执行API调用:', { auctionItemId: onAuctionItem.value.id, isKasec })

        // 显示加载状态
        const loadingInstance = ElLoading.service({
            lock: true,
            text: `${action}卡秒模式中...`,
            background: 'rgba(0, 0, 0, 0.7)',
        })
        console.log('加载状态已显示')

        try {
            console.log('调用 auctionStore.setKasec...')
            const result = await auctionStore.setKasec(onAuctionItem.value.id, isKasec)
            console.log('setKasec 调用完成，结果:', result)

            if (result) {
                // 检查状态是否真的改变了
                const actualKasecState = auctionStore.isKasec
                if (actualKasecState === isKasec) {
                    // 状态确实改变了
                    ElMessage.success(`${action}卡秒模式成功`)
                    console.log('操作成功，显示成功消息')
                } else {
                    // 状态没有改变，说明后端状态已经是目标状态
                    ElMessage.info(`卡秒状态已经是${isKasec ? '开启' : '关闭'}状态`)
                    console.log('状态已经是目标状态，显示提示消息')
                }
            } else {
                ElMessage.error(`${action}卡秒模式失败，请重试`)
                console.log('操作失败，显示失败消息')
            }
        } catch (error) {
            console.error('卡秒操作异常:', error)

            // 根据错误类型提供不同的错误信息
            if (error.code === 'NETWORK_ERROR' || error.message?.includes('Network Error')) {
                ElMessage.error('网络连接异常，请检查网络后重试')
            } else if (error.code === 'TIMEOUT') {
                ElMessage.error('操作超时，请稍后重试')
            } else if (error.message?.includes('状态冲突')) {
                ElMessage.error('拍品状态已发生变化，请刷新页面后重试')
            } else {
                ElMessage.error(`${action}卡秒模式失败：${error.message || '未知错误'}`)
            }
        } finally {
            loadingInstance.close()
            console.log('加载状态已关闭')
        }
    } catch (error) {
        console.log('确认框异常处理，错误类型:', error)

        // 用户取消操作
        if (error === 'cancel') {
            console.log('用户取消卡秒操作')
            return
        }

        // 确认框关闭（非确认）
        if (error === 'close') {
            console.log('用户关闭确认框')
            return
        }

        // 其他错误
        console.error('确认框异常:', error)
        ElMessage.error('操作异常，请重试')
    }

    console.log('=== 卡秒操作结束 ===')
}
</script>

<style>
.ps__rail-x,
.ps__rail-y {
    opacity: 0.6;
}

.el-popper.is-customized {
    /* Set padding to ensure the height is 32px */
    padding: 6px 12px;
    background: linear-gradient(90deg, rgb(159, 229, 151), rgb(204, 229, 129));
}

.el-popper.is-customized .el-popper__arrow::before {
    background: linear-gradient(45deg, #b2e68d, #bce689);
    right: 0;
}
</style>
