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
                    <list-auction-item
                        v-for="(x, index) in waitList"
                        :key="x.id"
                        :item="x"
                        :index="getItemIndex(x)"
                        @showDetail="showDetail"
                    />
                    <div class="h-4"></div>
                </div>
            </template>
            <!-- <template v-else-if="activeName === '2'">
                <div class="grid grid-cols-1 gap-2">
                    <list-auction-item v-for="(x, index) in auctionStore.auctionMid" :key="x.id" :item="x"
                        :index="index + 1" @showDetail="showDetail" />
                    <div class="h-4"></div>
                </div>
            </template> -->
            <template v-else-if="activeName === '3'">
                <div class="grid grid-cols-1 gap-2">
                    <list-auction-item
                        v-for="(x, index) in auctionStore.list4"
                        :key="x.id"
                        :item="x"
                        :index="index + 1"
                        @showDetail="showDetail"
                    />
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
                <img :src="`${onAuctionItem.imageUrl}`" class="w-full h-48 cursor-pointer object-cover" />
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
                <el-tooltip :content="isKasec ? '关闭卡秒' : '开启卡秒'" effect="customized">
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
                    <div class="i-mdi:refresh size-6 text-white" @click.stop="auctionStore.getList"></div>
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
import { ElMessage, ElMessageBox } from 'element-plus'
import { Tips } from '@/composables'
import { useChatStore } from '@/stores/chatStore'
import { ChatMessageType } from '@/api/appService'
import { GetUserGroupLevel } from '@/api/groupChatLevel'

let ps: PerfectScrollbar | null = null

const userStore = useUserStore()

const auctionStore = useAuctionStore()
const editRef = ref<InstanceType<typeof editAuctionItem> | null>(null)
const detailRef = ref<InstanceType<typeof auctionItemDetail> | null>(null)
const withdrawalApprovaRef = ref<InstanceType<typeof auctionItemDetail> | null>(null)
const addMsgRef = ref<InstanceType<typeof editAuctionItem> | null>(null)

const activeName = ref('1')

const waitList = computed(() => {
    return auctionStore.list.filter((item) => item.status === '上架')
})

const onAuctionItem = computed(() => {
    return auctionStore.list.find((item) => item.status === '拍卖中') || null
})

const chatStore = useChatStore()

onMounted(() => {
    ps = new PerfectScrollbar('#ps_container', {
        wheelSpeed: 1,
        wheelPropagation: false,
        minScrollbarLength: 20,
    })
    auctionStore.getList().then(() => {
        ps!.update()
        if (onAuctionItem.value) auctionStore.syncKasecStatus(onAuctionItem.value.id)
    })
})

watch(
    () => activeName.value,
    (val) => {
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

let normalIndex = 0
const getItemIndex = (item) => {
    if (item.name.includes('空降')) {
        return ''
    }
    normalIndex++
    return normalIndex
}

//LINK - 结束竞拍
function end() {
    if (!onAuctionItem.value) {
        ElMessage.error('没有正在拍卖的商品')
        return
    }
    ElMessageBox.confirm('确定结束当前竞拍并发送得主？', '结束竞拍', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
    })
        .then(() => {
            auctionStore.end(onAuctionItem.value!.id!)
        })
        .catch(() => {
            Tips.info('取消结束')
        })
}
// LINK 出价
function bid() {
    const userId = userStore.user.id
    const deposit = userStore.user.depositBalance || 0
    // 获取用户等级
    GetUserGroupLevel(userId).then((res) => {
        const userLevel = res.data?.level ?? 0
        if (userLevel === 0 && deposit < 50) {
            ElMessageBox.alert(
                `<div>
                    新用户参与拍卖，需要缴纳50元保证金。<br/>
                    <b>网站无法直接缴纳保证金，请扫码进入微信小程序缴纳。</b>
                    <div style="margin:10px 0;">
                        <img src="/images/miniapp_qrcode.png" style="width:150px;" />
                    </div>
                </div>`,
                '出价须知',
                { dangerouslyUseHTMLString: true }
            )
            return
        }
        // 原有出价弹窗逻辑
        auctionStore.getList().then(() => {
            if (!onAuctionItem.value) {
                ElMessage.error('没有正在拍卖的商品')
                return
            }
            let minPrice = 0
            if (onAuctionItem.value.currentPrice) {
                if (onAuctionItem.value.currentPrice < 100) {
                    minPrice = onAuctionItem.value.currentPrice + 1
                } else if (onAuctionItem.value.currentPrice < 1000) {
                    minPrice = onAuctionItem.value.currentPrice + 5
                } else if (onAuctionItem.value.currentPrice < 2000) {
                    minPrice = onAuctionItem.value.currentPrice + 10
                } else if (onAuctionItem.value.currentPrice < 5000) {
                    minPrice = onAuctionItem.value.currentPrice + 20
                } else if (onAuctionItem.value.currentPrice < 10000) {
                    minPrice = onAuctionItem.value.currentPrice + 50
                } else {
                    minPrice = onAuctionItem.value.currentPrice + 100
                }
            }
            if (auctionStore.isKasec) {
                minPrice = onAuctionItem.value.currentPrice + (minPrice - onAuctionItem.value.currentPrice) * 3
            }
            let message = `请输入出价金额(最低出价${minPrice})`
            if (auctionStore.isKasec) {
                message =
                    `<div style='color:red;border:1px solid red;padding:4px;margin-bottom:8px;'>您已卡秒出价，需加够三倍竞拍价才有效（最低出价：${minPrice}）</div>` +
                    message
            }
            ElMessageBox.prompt(message, '出价', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                inputPattern: /\d+/,
                inputValue: '',
                inputType: 'number',
                inputErrorMessage: '请输入正确的金额',
                dangerouslyUseHTMLString: true,
            })
                .then(({ value }) => {
                    auctionStore.bid(onAuctionItem.value!.id!, parseInt(value))
                })
                .catch(() => {
                    Tips.info('取消出价')
                })
        })
    })
}

async function toggleKasec() {
    if (!onAuctionItem.value) {
        ElMessage.error('没有正在拍卖的商品')
        return
    }
    const isKasec = !auctionStore.isKasec
    console.log('isKasec', isKasec)
    const result = await auctionStore.setKasec(onAuctionItem.value.id, isKasec)
    if (isKasec) {
        // 发送卡秒提示消息到拍卖群
        chatStore.sendChannelMsg('商品进入成交倒计时，卡秒出价需加够三倍一口价！', '-1_auction', ChatMessageType.Text, {
            highlight: true,
            border: 'red',
        })
    } else {
        // 发送卡秒结束消息到拍卖群
        chatStore.sendChannelMsg('卡秒结束，竞拍继续！', '-1_auction', ChatMessageType.Text, {
            highlight: true,
            border: 'green',
        })
    }
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
