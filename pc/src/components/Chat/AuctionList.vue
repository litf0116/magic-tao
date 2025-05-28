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
                    <list-auction-item v-for="(x, index) in waitList" :key="x.id" :item="x" :index="getItemIndex(x)"
                        @showDetail="showDetail" />
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
                    <list-auction-item v-for="(x, index) in auctionStore.list4" :key="x.id" :item="x" :index="index + 1"
                        @showDetail="showDetail" />
                    <div class="h-4"></div>
                </div>
            </template>
        </div>


        <div v-if="onAuctionItem && onAuctionItem.id" v-motion-fade-visible
            class="h-250px flex flex-col justify-between relative" style="height: 250px">
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
                <el-tooltip v-if="onAuctionItem" content="结束当前竞拍并发送得主" effect="customized">
                    <div class="bg-red-5 size-12 rounded-full flex flex-center cursor-pointer">
                        <div class="i-carbon:stop-filled-alt size-6 text-white" @click.stop="end"></div>
                    </div>
                </el-tooltip>
                <el-tooltip content="新增拍品" effect="customized">
                    <div class="bg-orange-5 size-12 rounded-full flex flex-center cursor-pointer">
                        <div class="i-mdi:add size-6 text-white" @click.stop="addNew"></div>
                    </div>
                </el-tooltip></template>
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


onMounted(() => {
    ps = new PerfectScrollbar('#ps_container', {
        wheelSpeed: 1,
        wheelPropagation: false,
        minScrollbarLength: 20,
    })
    auctionStore.getList().then(() => {
        ps!.update()
    })
    // startTimer()
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
        }
        else if (val === '3') {
            auctionStore.getList(4).then(() => {
                ps!.update()
            })
        }
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


let normalIndex = 0;
const getItemIndex = (item) => {
    if (item.name.includes('空降')) {
        return '';
    }
    normalIndex++;
    return normalIndex;
}
// 当数据变化时重置计数器
watch(() => waitList, () => {
    normalIndex = 0
}, { deep: true })

//LINK - 结束竞拍
function end() {
    if (!onAuctionItem.value) {
        ElMessage.error('没有正在拍卖的商品');
        return;
    }
    ElMessageBox.confirm('确定结束当前竞拍并发送得主？', '结束竞拍', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
    }).then(() => {
        auctionStore.end(onAuctionItem.value!.id!);
    }).catch(() => {
        Tips.info('取消结束');
    })
}
// LINK 出价
function bid() {
    auctionStore.getList().then(() => {
        if (!onAuctionItem.value) {
            ElMessage.error('没有正在拍卖的商品');
            return;
        }

        let minPrice = 0;
        if (onAuctionItem.value.currentPrice) {
            // 算法：
            // 100以内，1R一加
            // 100~1000，5R一加
            // 1000-2000，10R一加
            // 2000-5000，20R一加
            // 50000-1W，50一加
            // 1W以上，100一加
            if (onAuctionItem.value.currentPrice < 100) {
                minPrice = onAuctionItem.value.currentPrice + 1;
            } else if (onAuctionItem.value.currentPrice < 1000) {
                minPrice = onAuctionItem.value.currentPrice + 5;
            } else if (onAuctionItem.value.currentPrice < 2000) {
                minPrice = onAuctionItem.value.currentPrice + 10;
            } else if (onAuctionItem.value.currentPrice < 5000) {
                minPrice = onAuctionItem.value.currentPrice + 20;
            } else if (onAuctionItem.value.currentPrice < 10000) {
                minPrice = onAuctionItem.value.currentPrice + 50;
            } else {
                minPrice = onAuctionItem.value.currentPrice + 100;
            }
        }

        ElMessageBox.prompt(`请输入出价金额(最低出价${minPrice})`, '出价', {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            inputPattern: /\d+/,
            inputValue: '',
            // inputValue: minPrice ? minPrice.toString() : '',
            inputType: 'number',
            inputErrorMessage: '请输入正确的金额',
        })
            .then(({ value }) => {
                console.log('value', value, typeof value);
                // if (parseInt(value) < minPrice) {
                //     ElMessageBox.alert(
                //         '100以内，1R一加。100~1000，5R一加。1000-2000，10R一加。2000-5000，20R一加。50000-1W，50一加。1W以上，100一加',
                //         '不能低于最低出价',
                //         {
                //             // if you want to disable its autofocus
                //             // autofocus: false,
                //             confirmButtonText: 'OK',
                //         }
                //     )
                //     return
                // }
                auctionStore.bid(onAuctionItem.value!.id!, parseInt(value));
            })
            .catch(() => {
                Tips.info('取消出价');
            })
    })
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
