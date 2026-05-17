<template>
    <div class="bg">
        <div class="post-detail">
            <!-- 左侧主体内容 -->
            <div class="main-content">
                <!-- 标题 -->
                <div class="title">{{ post.title }}</div>

                <!-- 楼主信息 -->
                <div class="author-info">
                    <el-avatar :size="40" :src="post.userAvatar"></el-avatar>
                    <div class="info">
                        <div class="name">{{ post.userName }}</div>
                        <div class="time">{{ post.createdAt }}</div>
                    </div>
                </div>
                <div style="border-bottom: 1px dashed #d8d8d8; padding-bottom: 5px">
                    <template v-if="postCategory">
                        <el-tag
                            v-for="(item, index) in postCategory"
                            :key="index"
                            :type="tagTypes[index % tagTypes.length]"
                            effect="plain"
                            size="small"
                            style="margin-left: 5px"
                        >
                            {{ item }}
                        </el-tag>
                    </template>
                </div>
                <!-- 内容区域 -->
                <div class="content" @click="catchImage(post.content)" v-html="post.content"></div>
            </div>

            <!-- 右侧信息栏 -->
            <div class="side-info">
                <!-- 发帖人信息卡片 -->
                <div class="author-card">
                    <div class="header">发帖人: {{ post.userName }}</div>
                    <div class="stats">
                        <div class="item">
                            <div class="label">微信:</div>
                            <div class="value">{{ post.wechat }}</div>
                        </div>
                        <div class="item">
                            <div class="label">QQ:</div>
                            <div class="value">{{ post.qq }}</div>
                        </div>
                    </div>
                </div>
                <el-button class="back-btn" type="primary" plain @click="adminSend(post)">点击留言</el-button>
                <div style="display: flex; justify-content: center; margin-top: 10px">
                    <el-button
                        v-if="userStore.user.id === post.userId"
                        class="edit-btn"
                        type="primary"
                        plain
                        @click="editData(post)"
                    >
                        编辑
                    </el-button>
                    <el-button
                        v-if="userStore.isAuctionAdmin || userStore.user.id === post.userId"
                        class="edit-btn"
                        type="danger"
                        plain
                        @click="delData(post)"
                    >
                        删除
                    </el-button>
                    <el-button
                        v-if="userStore.isAuctionAdmin"
                        class="edit-btn"
                        type="primary"
                        plain
                        @click="setEssence(post)"
                    >
                        精华
                    </el-button>
                    <el-button
                        v-if="userStore.isAuctionAdmin"
                        class="edit-btn"
                        type="primary"
                        plain
                        @click="setTop(post)"
                    >
                        置顶
                    </el-button>
                </div>
            </div>
        </div>

        <el-image-viewer v-if="showViewer" :url-list="urlList" @close="showViewer = false" />
    </div>
    <postItem ref="postRef" @on-saved="loadPosts" />
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { GetPostDetail, Delete, SetPostTop, SetPostEssence } from '@/api/postAPI'
import { ElMessage, ElMessageBox } from 'element-plus'
import postItem from './postItem.vue'
import { convertImageUrl } from '@/utils/imageUrlConverter'

const route = useRoute()
const router = useRouter()
const postRef = ref(null)
const tagTypes = ['', 'success', 'warning', 'danger', 'info']
// 帖子数据
const post: any = ref({})
const postCategory = ref({}) //帖子类型
// 获取帖子详情
const getPostDetail = async (id) => {
    const res = await GetPostDetail(id)
    if (res) {
        post.value = res
        if (post.value.categoryName) {
            var arr = post.value.categoryName.split(',')
            postCategory.value = arr
        }
    }
}

//ANCHOR - 私聊
const chatStore = useChatStore()
const userStore = useUserStore()
const adminSend = (res) => {
    if (userStore.user.id === res.lastModifierUserId) {
        Tips.error('不能给自己发信息')
        return
    }
    if (res) {
        let chat = chatStore.chatList.find((item) => item.id === res.lastModifierUserId)
        if (!chat) {
            chat = {
                id: res.lastModifierUserId,
                name: res.userName,
                type: ChatListItemType.user,
                avatar: res.userAvatar,
                unread: 0,
                order: 0,
            }
            chatStore.addChatList(chat.id!, chat.name, chat.avatar!)
        }
        chatStore.SetCurrentChat(chat)
        router.push({
            path: `/chat/index/privateChat/${chat.id}`,
            query: { name: chat.name, avatar: convertImageUrl(chat.avatar) || 'https://image.molitao.top/avater.png' },
        })
    }
}
const pageId = ref('')
onMounted(() => {
    // 获取路由参数中的帖子id
    pageId.value = route.params.id as string
    loadPosts()
})
//加载数据
const loadPosts = () => {
    getPostDetail(pageId.value)
}
//编辑数据
const editData = (dto) => {
    postRef.value?.show(true, dto)
}
//删除数据
const delData = async (dto) => {
    ElMessageBox.confirm('你确定删除吗?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
    })
        .then(async () => {
            await Delete(dto.postId)
            Tips.success('删除成功')
            router.push({
                path: 'forum/tradingPost',
            })
        })
        .catch(() => {
            ElMessage({ type: 'info', message: '已取消删除' })
        })
}
//设置置顶
const setTop = async (dto) => {
    await SetPostTop(dto.postId)
    Tips.success('设置成功')
}
//设置精华帖
const setEssence = async (dto) => {
    await SetPostEssence(dto.postId)
    Tips.success('设置成功')
}
const showViewer = ref(false)
const urlList = ref([])
//查看图片
const catchImage = (e) => {
    console.log('catchImage', e)
    try {
        const list = []
        //从 string中img标签中获取data-url的属性放入数组中
        const reg = /<img.*?src=['"](.*?)['"].*?>/g
        let result
        while ((result = reg.exec(e)) !== null) {
            list.push(result[1])
        }

        if (list.length === 0) return
        console.log('catchImage', list)
        // wx.previewImage({
        //     current: list[0], // 当前显示图片的http链接
        //     urls: list, // 需要预览的图片http链接列表
        // })
        showViewer.value = true
        urlList.value = list
    } catch (e) {
        console.log('catchImage', e)
    }
}
</script>

<style scoped lang="scss">
::v-deep img {
    width: 100%;
}

.bg {
    width: 100%;
    height: 100%;
    // background-image: url("../../../assets/bg.png");
    // background-size: 100% 100%;
}

.post-detail {
    display: flex;
    padding: 3cap;
    gap: 20px;
    max-width: 1200px;
    margin: 0 auto;

    .main-content {
        flex: 1;
        background: #fff;
        border-radius: 8px;
        padding: 20px;
        box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
        width: 940px;

        .title {
            font-size: 24px;
            font-weight: bold;
            margin-bottom: 20px;
            border-bottom: 1px dashed #d8d8d8;
            padding-bottom: 5px;
        }

        .author-info {
            display: flex;
            align-items: center;
            margin-bottom: 10px;
            padding-bottom: 5px;
            border-bottom: 1px dashed #d8d8d8;

            .info {
                margin-left: 12px;

                .name {
                    font-size: 16px;
                    font-weight: 500;
                }

                .time {
                    font-size: 14px;
                    color: #999;
                    margin-top: 4px;
                }
            }
        }

        .content {
            font-size: 16px;
            line-height: 1.8;
            width: 100%;
            height: 100%;
        }
    }

    .side-info {
        width: 300px;

        .author-card {
            background: #fff;
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 20px;
            box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);

            .header {
                font-size: 16px;
                font-weight: bold;
                margin-bottom: 16px;
            }

            .stats {
                .item {
                    display: flex;
                    margin-bottom: 12px;

                    .label {
                        color: #666;
                        width: 60px;
                    }

                    .value {
                        color: #333;
                    }
                }
            }
        }

        .back-btn {
            width: 100%;
            font-size: 18px;
            font-weight: 500;
            height: 40px;
            letter-spacing: 2px;
        }
    }
}
</style>
