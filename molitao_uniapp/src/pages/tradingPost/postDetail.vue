<template>
	<view class="page">
		<view class="container">
			<!-- 帖子标题 -->
			<view class="post-header">
				<text class="post-title">{{ postDetail.title }}</text>
				<view class="post-meta">
					<image :src="postDetail.userAvatar" mode="aspectFill" class="size-12 rounded-full"></image>
					<text class="userName">{{ postDetail.userName }}</text>
					<text class="time">{{ postDetail.createdAt }}</text>
				</view>
			</view>
			<view class="post-header">
				<view class="time">微信：{{ postDetail.wechat || '—' }}</view>
				<view class="time" style="margin-top: 5px;">QQ：{{ postDetail.qq || '—' }}</view>
			</view>
			<!-- 标签区域 -->
			<view class="tags-section">
				<!-- <view v-for="(tag, index) in postCategory" :key="index" class="tag">
					{{ tag }}
				</view> -->
				<view v-for="(tag, index) in postCategory" :key="index" class="tag"
					:style="{ borderColor: getRandomColor(index), color: getRandomColor(index) }">
					{{ tag }}
				</view>
			</view>

			<!-- 帖子内容 -->
			<view class="content-section" @tap="catchImage(content)">
				<rich-text :nodes="formatRichText(content)"></rich-text>
			</view>

			<!-- 右侧信息卡片 -->
			<view class="info-card">
				<!-- 快速咨询按钮 -->
				<view class="quick-consult">
					<button class="consult-btn" @click="adminSend(postDetail)">点击留言</button>
					<button style="margin-top: 10px;" v-if="userStore.user.id === postDetail.userId" class="consult-btn"
						@click="editData(postDetail)">修改</button>
					<button v-if="userStore.user.id === postDetail.userId"
						style="background-color:#e85252;margin-top: 10px;" class="consult-btn"
						@click="delData(postDetail)">删除</button>
				</view>
			</view>
		</view>
	</view>
</template>

<script setup lang="ts">
import { ref, nextTick } from 'vue'
import { onLoad } from '@dcloudio/uni-app'
import api from '@/utils/api'
const chatStore = useChatStore()
const userStore = useUserStore()

const postDetail = ref<any>({})
const content = ref<String>("")
const postCategory = ref([])

// 定义标签边框颜色数组
const borderColors = [
	'#FF6B6B', // 红色
	'#4ECDC4', // 青色
	'#45B7D1', // 蓝色
	'#96CEB4', // 绿色
	'#FFEEAD', // 黄色
	'#D4A5A5', // 粉色
	'#9B59B6', // 紫色
	'#3498DB', // 深蓝
	'#E67E22', // 橙色
	'#2ECC71'  // 翠绿
]

onLoad((options: any) => {
	const id = options.id
	loadData(id)
})
//加载数据
const loadData = (id: any) => {
	api.post.GetPostDetail(id).then((res: any) => {
		nextTick(() => {
			postDetail.value = res;
			content.value = res.content;
			if (res.categoryName) {
				var arr = res.categoryName.split(",")
				postCategory.value = arr;
			}
		})
	})
}

// 处理富文本里的图片宽度自适应
const formatRichText = (html: any) => {
	return html && html.replace(/<img[^>]*>/gi, function (match: any, capture: any) { // 查找所有的 img 元素
		return match.replace(/style=".*"/gi, '').replace(/style='.*'/gi, '') // 删除找到的所有 img 元素中的 style 属性
	}).replace(/\<img/gi, '<img style="width:100%;"') // 对 img 元素增加 style 属性，并设置宽度为 100%
}
// 获取随机颜色的函数
const getRandomColor = (index: number) => {
	// 使用索引取模来确保相同位置的标签每次都显示相同颜色
	return borderColors[index % borderColors.length];
}
//图片预览
const catchImage = (e: any) => {
	try {
		const list = [];
		//从 string中img标签中获取data-url的属性放入数组中
		const reg = /<img.*?src=['"](.*?)['"].*?>/g;
		let result;
		while ((result = reg.exec(e)) !== null) {
			list.push(result[1]);
		}

		if (list.length === 0) return;
		wx.previewImage({
			current: list[0], // 当前显示图片的http链接
			urls: list, // 需要预览的图片http链接列表
		});

	} catch (e) {
		console.log('catchImage', e)
	}
}
//私聊
const adminSend = (res: any) => {
	if (userStore.user.id === res.lastModifierUserId) {
		Tips.error("不能给自己发信息");
		return;
	}
	if (res) {
		let chat = chatStore.chatList.find((item) => item.id === res.lastModifierUserId);
		if (!chat) {
			chat = {
				id: res.lastModifierUserId,
				name: res.userName,
				type: ChatListItemType.user,
				avatar: res.userAvatar,
				unread: 0,
				order: 0,
			};
			chatStore.addChatList(chat.id!, chat.name, chat.avatar!);
		}
		chatStore.SetCurrentChat(chat);

		Goto.private({
			id: `${chat.id}`,
			name: chat.name,
			avatar: chat.avatar || 'https://cdn.molitao.top/avater.png',
		})
	}
}
//编辑数据
const editData = (dto: any) => {
	uni.navigateTo({
		url: "/pages/tradingPost/addPost?id=" + dto.postId,
	});
}
//删除数据
const delData = (dto: any) => {
	uni.showModal({
		title: '提示',
		content: '确定要删除吗？',
		success: function (res) {
			if (res.confirm) {
				api.post.Delete(dto.postId).then((re: any) => {
					Tips.success("删除成功");
					setTimeout(() => {
						uni.navigateTo({
							url: '/pages/tradingPost/index'
						})
					}, 1000) // 1000毫秒 = 1秒后执行
				});
			}
		}
	})
}
</script>

<style>
.page {
	width: 100%;
	background-color: #f5f5f5;
}

.container {
	width: 100%;
	box-sizing: border-box;
	padding: 20rpx;
}

/* 帖子标题区域 */
.post-header {
	width: 100%;
	box-sizing: border-box;
	background-color: #fff;
	padding: 20rpx;
	border-radius: 12rpx;
	margin-bottom: 20rpx;
}

.post-title {
	font-size: 36rpx;
	font-weight: bold;
	color: #333;
	margin-bottom: 20rpx;
	display: block;
}

.post-meta {
	font-size: 26rpx;
	color: #999;
	display: flex;
	align-items: center;
	font-size: 16px;
}

.userName {
	margin-right: 20rpx;
	margin-left: 10px;
}

/* 标签区域 */
.tags-section {
	width: 100%;
	box-sizing: border-box;
	display: flex;
	flex-wrap: wrap;
	gap: 16rpx;
	padding: 20rpx;
	background-color: #fff;
	border-radius: 12rpx;
	margin-bottom: 20rpx;
}

.tag {
	padding: 8rpx 20rpx;
	background-color: #fff;
	border-width: 1px;
	border-style: solid;
	border-radius: 20rpx;
	font-size: 24rpx;
	color: #666;
	transition: all 0.3s ease;
}

.tag:hover {
	transform: scale(1.05);
}

/* 内容区域 */
.content-section {
	width: 100%;
	box-sizing: border-box;
	background-color: #fff;
	padding: 20rpx;
	border-radius: 12rpx;
	margin-bottom: 20rpx;
}

.content-section img {
	width: 100%;
}


/* 信息卡片样式 */
.info-card {
	width: 100%;
	box-sizing: border-box;
	background-color: #fff;
	padding: 20rpx;
	border-radius: 12rpx;
}

.user-info {
	margin-bottom: 30rpx;
}

.info-title {
	font-size: 28rpx;
	color: #333;
	margin-bottom: 16rpx;
	display: block;
}

.contact-info {
	font-size: 26rpx;
	color: #666;
}

.contact-info text {
	display: block;
	margin-bottom: 10rpx;
}

/* 咨询按钮样式 */
.consult-btn {
	width: 100%;
	height: 80rpx;
	line-height: 80rpx;
	background-color: #007AFF;
	color: #fff;
	font-size: 28rpx;
	border-radius: 40rpx;
	text-align: center;
}

.consult-btn:active {
	opacity: 0.8;
}
</style>