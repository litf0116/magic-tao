<template>
    <div class="flex-1" style="max-height: calc(700px - 2.5rem)" @contextmenu.prevent>
        <div class="h-24 border-0 border-b-1 border-solid border-gray-400 overflow-hidden">
            <announce-div :category-id="1" />
        </div>
        <!-- <div class="h-8">
            <el-input v-model="filterText" class="border-0" placeholder="筛选组队频道" clearable />
        </div> -->
        <div id="ps_container" style="max-height: calc(700px - 12rem)" class="space-y-2 overflow-hidden h-full p-1">
            <!-- <div
                v-for="(group, k) in chatStore.groups
                    .filter((x) => x.chan !== '0_lobby' && x.chan !== '-1_auction' && x.chan.indexOf(filterText) > -1)
                    .reverse()"
                :key="k"
            >
                <div
                    v-motion-fade-visible
                    class="px-2 py-1 rounded shadow-md cursor-pointer relative"
                    :class="[
                        group.creatorUserId === userStore.user.id
                            ? 'bg-[#f4835a] text-white'
                            : 'text-true-gray-7 bg-gray-100',
                    ]"
                    @click="goChan(group.chan)"
                >
                    <div class="flex items-center justify-between text-sm">
                        <div class="flex-1 line-clamp-1">{{ group.title }}</div>
                        <div>限{{ group.limit }}人</div>
                    </div>
                    <div class="mt-1 text-true-gray-5 flex items-center justify-between text-xs">
                        <div>发布人:{{ group.creatorUser.name }}</div>
                        <div>{{ formatDate(group.creationTime, 'fromNow') }}</div>
                    </div>
                </div>
            </div> -->
            <div>
                <div v-motion-fade-visible class="px-2 py-1 rounded shadow-md relative">
                    <div class="flex items-center justify-between text-sm">
                        <img :src="convertImageUrl(item.avatar)" class="chat-avatar" />
                        <div>呢称：{{ item.name }}</div>
                    </div>
                    <div class="mt-1 text-true-gray-5 flex items-center justify-between text-xs">
                        <div>QQ：{{ item.qq }}</div>
                        <div>微信：{{ item.weChat }}</div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- <div class="shadow h-10 grid grid-cols-3">
        <div class="bg-gray-500 text-white cursor-pointer flex flex-center" @click="chatStore.getGropus">刷新列表</div>
        <div class="col-span-2 bg-[#f4835a] text-white cursor-pointer flex flex-center" @click="createGroup">
            创建组队聊天
        </div>
        <createGroupDislog ref="createGroupDislogRef" @on-saved="chatStore.getGropus" />
    </div> -->
</template>

<script setup lang="ts">
import announceDiv from './announceDiv.vue'
import PerfectScrollbar from 'perfect-scrollbar'
import createGroupDislog from './createGroupDislog.vue'
import { convertImageUrl } from '@/utils/imageUrlConverter'
const chatStore = useChatStore()
const userStore = useUserStore()
const router = useRouter()

const props = defineProps({
    item: {
        type: Object,
        required: true,
    },
})

onMounted(() => {
    //
})

let ps: PerfectScrollbar | null = null

const createGroupDislogRef = ref<InstanceType<typeof createGroupDislog> | null>(null)
const filterText = ref('')

onMounted(() => {
    ps = new PerfectScrollbar('#ps_container')
    ps.update()
})

watch(
    () => chatStore.groups.length,
    () => {
        if (ps) {
            ps.update()
        }
    }
)

function goChan(name: string) {
    router.replace({
        path: '/chat/index/groupChat/' + name,
    })
}

// function createGroup() {
//     ElMessageBox.prompt('请输入组队内容', '创建组队聊天', {
//         confirmButtonText: '确定',
//         cancelButtonText: '取消',
//         inputPattern:
//             /^((?:[\u3400-\u4DB5\u4E00-\u9FEA\uFA0E\uFA0F\uFA11\uFA13\uFA14\uFA1F\uFA21\uFA23\uFA24\uFA27-\uFA29]|[\uD840-\uD868\uD86A-\uD86C\uD86F-\uD872\uD874-\uD879][\uDC00-\uDFFF]|\uD869[\uDC00-\uDED6\uDF00-\uDFFF]|\uD86D[\uDC00-\uDF34\uDF40-\uDFFF]|\uD86E[\uDC00-\uDC1D\uDC20-\uDFFF]|\uD873[\uDC00-\uDEA1\uDEB0-\uDFFF]|\uD87A[\uDC00-\uDFE0])|([0-9a-zA-Z])){4,12}$/,
//         inputErrorMessage: '只能含有中文、字母或数字,4-12个字符',
//     })
//         .then(async ({ value }) => {
//             console.log(value)
//             await chatStore.createChannel(value)
//             await chatStore.getGropus()
//         })
//         .catch(() => {
//             Tips.info('取消创建')
//         })
// }

function createGroup() {
    createGroupDislogRef.value?.show(true)
}
</script>
