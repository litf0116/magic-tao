<template>
    <el-dialog
        v-if="item"
        v-model="dialogVisible"
        :title="item.name"
        width="600"
        draggable
        :destroy-on-close="true"
        append-to-body
        @close="onClose"
    >
        <div v-if="userStore.isChatAdmin && item.sellerInfo" class="text-blue my-2">
            卖家信息[仅管理可见]: {{ item.sellerInfo }}
        </div>
        <!-- <div v-if="userStore.isChatAdmin">
            <div v-if="item.description" id="auctionDesc" v-html="item.description"></div>

            <div v-else>
                <img :src="item.imageUrl" class="size-128 auctionDesc" />
            </div>
        </div> -->
        <div
            v-if="item && item.id"
            v-motion-fade-visible
            class="h-250px flex flex-col justify-between relative"
            style="min-height: 300px; overflow-y: scroll"
        >
            <!-- {{ onAuctionItem }} -->
            <div>
                <div v-if="item.description" id="auctionDesc" v-html="item.description"></div>
                <div v-else>
                    <img :src="`${item.imageUrl}`" class="w-full h-48 cursor-pointer object-cover" />
                </div>
            </div>
        </div>
        <template #footer>
            <div
                v-if="item.status != '已成交' && userStore.isChatAdmin === false"
                style="margin-top: 50px"
                class="absolute top-0 left-0 bg-[#f4835a] text-white shadow rounded-rb-lg px-2 font-bold"
            >
                {{ item.status != '拍卖中' ? '等待竞拍' : '竞拍中' }}
            </div>
            <div v-if="userStore.isChatAdmin" class="dialog-footer">
                <el-button v-if="item.status != '拍卖中'" type="danger" @click="toDelete(item)"> 删除 </el-button>

                <el-button type="primary" @click="toEdit(item)"> 修改 </el-button>
                <el-button
                    :disabled="item.status === '拍卖中' ? true : false"
                    type="success"
                    @click="startAuction(item)"
                >
                    开始拍卖
                </el-button>
                <el-button v-if="item.status === '拍卖中'" type="success" @click="end(item)"> 结束竞拍 </el-button>
            </div>
            <div class="p-2" v-if="item.status === '拍卖中' && userStore.isChatAdmin === false">
                <el-button color="#f4835a" class="w-full" @click="bid(item.id)">
                    <div class="text-lg font-700 text-white">出价</div>
                </el-button>
            </div>
        </template>
    </el-dialog>
</template>

<script setup lang="ts" name="auctionItemDetail">
import api from '@/api'
import { AuctionItemDto } from '@/api/appService'
import { ElMessage, ElMessageBox } from 'element-plus'
import { GetAuctionMidList } from '@/api/auctionMidAPI'
import { GetDetail } from '@/api/auctionItemAPI'
import { Tips } from '@/composables'
import { calculateMinBidPrice } from '@/utils/auction'

const userStore = useUserStore()
const emit = defineEmits(['onEdit'])

const auctionStore = useAuctionStore()

const dialogVisible = ref(false)
const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms))

function toEdit(item: AuctionItemDto) {
    emit('onEdit', item.id)
    dialogVisible.value = false
}

function toDelete(item: AuctionItemDto) {
    ElMessageBox.confirm('确定删除吗?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
    }).then(() => {
        api.auctionItem.delete({ id: item.id }).then(() => {
            auctionStore.getList()
            dialogVisible.value = false
        })
    })
}
//开始拍卖商品
function startAuction(item: AuctionItemDto) {
    auctionStore.startAuction(item.id!)
    dialogVisible.value = false
}

//LINK - 结束竞拍
function end(item: AuctionItemDto) {
    if (!item) {
        ElMessage.error('没有正在拍卖的商品')
        return
    }

    ElMessageBox.confirm('确定结束当前竞拍并发送得主？', '结束竞拍', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
    })
        .then(async () => {
            const success = await auctionStore.end(item!.id!)
            if (success) {
                dialogVisible.value = false
                await sleep(1000)
                GetAuctionMidList({ status: 2, maxResultCount: 100 }).then((res) => {
                    if (res.status == 200) {
                        console.log(res)
                        auctionStore.auctionMid.length = 0
                        auctionStore.auctionMid = res.data.items
                    }
                })
            }
        })
        .catch(() => {
            Tips.info('取消结束')
        })
}

//出价
function bid(id) {
    auctionStore.getList(2).then((res) => {
        var info = auctionStore.auctionMid.find((item) => item.id === id)
        if (!info) {
            ElMessage.error('没有正在拍卖的商品')
            return
        }

        let minPrice = 0
        if (info.currentPrice) {
            // 使用工具方法计算最低出价
            minPrice = calculateMinBidPrice(info.currentPrice, false) // 这里没有卡秒模式，所以传false
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
            .then(async ({ value }) => {
                console.log('value', value, typeof value)
                dialogVisible.value = false
                auctionStore.bid(info!.id!, parseInt(value))
                await sleep(1000)
                GetAuctionMidList({ status: 2, maxResultCount: 100 }).then((res) => {
                    if (res.status == 200) {
                        console.log(res)
                        auctionStore.auctionMid.length = 0
                        auctionStore.auctionMid = res.data.items
                    }
                })
            })
            .catch(() => {
                Tips.info('取消出价')
            })
    })
}

function onClose() {
    console.log('onClose')
    dialogVisible.value = false
}

watch(
    () => dialogVisible.value,
    (val) => {
        if (!val) item.value = null
    }
)

const showImageViewer = inject('showImageViewer') as (list: string[]) => void

const show = (e: boolean, id: number) => {
    dialogVisible.value = e
    if (e) {
        GetDetail(id).then((res) => {
            // if (res.status != "拍卖中" && userStore.isChatAdmin == false) {
            //     dialogVisible.value = false;
            //     return;
            // }
            item.value = res.data
            nextTick(() => {
                const images = document.querySelectorAll('#auctionDesc img')
                images.forEach((img) => {
                    // 当被点击时,打开一个新的窗口显示图片
                    img.addEventListener('click', () => {
                        console.log('img click', img.getAttribute('src'))
                        let src = img.getAttribute('src') as string
                        src = src.replace(/!w300$/, '')
                        showImageViewer([src])
                    })
                })
                const images1 = document.querySelectorAll('.auctionDesc')
                images1.forEach((img) => {
                    // 当被点击时,打开一个新的窗口显示图片
                    img.addEventListener('click', () => {
                        console.log('img click', img.getAttribute('src'))
                        let src = img.getAttribute('src') as string
                        src = src.replace(/!w300$/, '')
                        showImageViewer([src])
                    })
                })
            })
        })
    }
}

const item = ref<AuctionItemDto | null>(null)

defineExpose({
    show,
})
</script>
<style>
#auctionDesc {
    display: flex;
    flex-wrap: wrap;

    img {
        width: 177px !important;
        height: 150px !important;
        margin: 3px;
    }

    div {
        width: 100%;
        text-align: left;
    }
}
</style>
