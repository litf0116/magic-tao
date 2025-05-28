<template>
	<view class="container" style="margin-bottom: 52px;">
		<scroll-view class="scroll-container" scroll-y="true" refresher-enabled="true" :refresher-triggered="refresh"
			@refresherrefresh="onRefresh" @scrolltolower="onLoadMore" @scroll="onScroll" :lower-threshold="150"
			:enhanced="true" :bounce="true" :show-scrollbar="true" fast-deceleration>
			<!-- 顶部筛选区 -->
			<view class="filter-section">
				<!-- 左侧滑动分类 -->
				<scroll-view scroll-x class="category-scroll" :show-scrollbar="false">
					<view class="category-list">
						<view class="category-item" v-for="item in postCategoryList" :key="item.key"
							:class="{ active: activeKey === item.categoryId }" @tap="switchCategory(item.categoryId)">
							{{ item.name }}
						</view>
					</view>
				</scroll-view>

				<!-- 滚动公告 -->
				<view class="notice-bar">
					<view class="notice-icon">
						<image style="width: 26px;height: 26px;" src="../../static/公告.png" mode="aspectFill"
							class="size-12 rounded-full"></image>
					</view>
					<view class="notice-content barrage-box" @tap="getMore(latestBulletin.content)">
						<view class="notice-text text" v-if="latestBulletin">
							<hbxw-roll-text :list="bulletinList" :duration="50"></hbxw-roll-text>
						</view>
					</view>
				</view>

			</view>

			<!-- 搜索框 -->
			<view class="search-box">
				<input type="text" class="search-input" placeholder="请输入关键词" placeholder-class="placeholder-style"
					v-model="keywords" />
				<image src="../../static/搜索.png" class="search-icon" mode="aspectFit" @tap="loadData"></image>
			</view>
			<view class="hotWords">
				<view class="hotWords-item" v-for="item in hotWordsList"
					:class="{ active: hotWordsActiveKey === item.id }" @tap="switchHotWords(item)">
					{{ item.title }}
				</view>
			</view>
			<!-- 发帖按钮 -->
			<view class="post-button" @tap="onPostTap">
				我要发帖
			</view>
			<!-- 分隔区域 -->
			<view class="divider"></view>
			<!-- 正常帖子区域 -->
			<view class="section normal-posts">
				<!-- 帖子列表 -->
				<view class="post-list" v-if="postList.length > 0">
					<view class="post-item" v-for="(item, index) in postList" :key="index" @tap="toDetail(item.postId)">
						<view class="post-title">{{ item.title }}</view>
						<view class="post-content">
							<!-- 左侧头像 -->
							<image class="avatar" :src="item.userAvatar" mode="aspectFill"></image>
							<!-- 分类标签 -->
							<text class="category-tag" :class="'category-' + item.categoryType">
								{{ item.category }}
							</text>
							<!-- 用户名和时间 -->
							<text class="username">{{ item.userName }}</text>
							<text class="update-time">{{ item.createdAt }}</text>
						</view>
					</view>
				</view>
				<!-- 空状态显示 -->
				<view class="empty-state" v-else>
					<image class="empty-image" src="../../static/nodata.png" mode="aspectFit"></image>
					<text class="empty-text">暂无帖子</text>
					<!-- <button class="post-button" @tap="onPost">发布新帖子</button> -->
				</view>
			</view>

		</scroll-view>
	</view>

</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onBeforeUnmount } from 'vue'
import api from '@/utils/api'


const activeKey = ref(-1)
const keywords = ref("")
const page = ref(1)
const pageSize = ref(20)
const postList: any = ref([])//帖子列表
const hasNextPages = ref(false)//是否有下一页
const latestBulletin: any = ref({})//最新公告
const bulletinList: any = ref([])//公告列表
const hotWordsList: any = ref([])//热词列表
const hotWordsActiveKey = ref(-1)//热词选中
const refresh = ref(false);
const modalConfig = reactive({
	show: false,
	title: '【魔力淘】交易行使用规范',
	content: '<div style="color: red;">这是一段<strong>HTML</strong>内容</div>',
	showCancel: false,
	cancelText: '取消',
	confirmText: '确定'
});

const emit = defineEmits(['updateModalConfig'])
//帖子类型
const postCategoryList: any = ref([
	{
		categoryId: -1,
		name: '全部',
	},
])



onMounted(() => {
	loadCategoryList();
	loadLatestBulletin();
	loadData();
	loadHotWords();
	uni.hideHomeButton();
})

// 切换分类
const switchCategory = (key: number) => {
	activeKey.value = key;
	page.value = 1;
	postList.value.length = 0;
	loadData();
}
//是否切换过热词
const isSwitchHotWords = ref(false);
//热词切换
const switchHotWords = (key: any) => {
	hotWordsActiveKey.value = key.id;
	keywords.value = key.title;
	page.value = 1;
	postList.value.length = 0;
	isSwitchHotWords.value = true;
	loadData();
}
// 监听滚动
const onScroll = (e: any) => {

}

//下拉刷新
const onRefresh = async () => {
	refresh.value = true;
	page.value = 1;
	pageSize.value = 50;
	postList.value.length = 0;
	await loadData();
	setTimeout(() => {
		refresh.value = false;
	}, 300);
}
// 上拉加载
const onLoadMore = (async () => {
	if (hasNextPages.value) {
		page.value++
		if (pageSize.value != 20) {
			pageSize.value = 20;
		}
		await loadData();
	} else {
		Tips.info("没有更多数据了")
	}
})
//加载数据
const loadData = () => {
	if (keywords.value === "") {
		if (isSwitchHotWords.value) {
			postList.value.length = 0;
			isSwitchHotWords.value = false;
		}
		hotWordsActiveKey.value = -1;
	}
	api.post.GetPostAll({ type: activeKey.value, Keyword: keywords.value, SkipCount: page.value, MaxResultCount: pageSize.value })
		.then((res: any) => {
			hasNextPages.value = res.hasNextPages;
			postList.value.push(...res.items)
		});
}
//加载最新公告
const loadLatestBulletin = () => {
	api.post.GetLatestBulletin().then((res: any) => {
		latestBulletin.value = res;
		var content = res.content.replace(/↵/g, '');  // 先去掉换行符
		var arr = content
			.replace(/<\/p><p>/g, '</p>|SPLIT|<p>')  // 用特殊标记替代</p><p>
			.replace(/<[^>]+>/g, '')  // 去掉所有HTML标签
			.split('|SPLIT|')  // 根据特殊标记分割成数组
			.map((item: any) => item.trim().replace(/↵/g, '')); // 清理每项的空格和换行
		bulletinList.value = arr;
	})
}
//加载热词
const loadHotWords = () => {
	api.post.GetHotWordsList().then((res: any) => {
		hotWordsList.value = res.items;
	})
}
//加载分类列表
const loadCategoryList = () => {
	api.post.GetCategoryList().then((res: any) => {
		postCategoryList.value.push(...res);
	})
}

// 处理发帖按钮点击事件
const onPostTap = () => {
	uni.navigateTo({
		url: "/pages/tradingPost/addPost"
	})
}

//跳转到详情
const toDetail = (id: any) => {
	uni.navigateTo({
		url: '/pages/tradingPost/postDetail?id=' + id
	})
}
//查看公告
const getMore = (text: any) => {
	modalConfig.show = true;
	modalConfig.content = text;
	emit('updateModalConfig', modalConfig)
}
const onCancel = () => {
	modalConfig.show = false;
}
const onConfirm = () => {
	modalConfig.show = false;
}


</script>
<style>
.scroll-container {
	height: 100vh;
	width: 100%;

	-webkit-overflow-scrolling: touch;
	overscroll-behavior: contain;
}

.scroll-content {
	min-height: 101%;
	padding: 20rpx;
	box-sizing: border-box;
	transform: translateZ(0);
	-webkit-transform: translateZ(0);
	will-change: transform;
}

/* 自定义滚动条样式 */
::-webkit-scrollbar {
	width: 0;
	height: 0;
	background: transparent;
}

uni-modal .uni-modal__bd {
	white-space: pre-wrap;
}

.barrage-box {
	padding: 0 10rpx;
	width: 90vw;
	transform-origin: 65vw 75vw;
	transform: rotate(0deg);
	white-space: nowrap;
	display: flex;
	justify-content: center;
	align-items: center;
	z-index: 3;
}

.text {
	width: 200vw;
	font-size: 16px;
	color: #000;
}

/* 文字滚动 */
@keyframes aniMove {
	0% {
		transform: translateX(100%);
	}

	100% {
		transform: translateX(-100%);
	}
}

.filter-section {
	flex-direction: column;
	background: #fff;
	padding: 10rpx;
	border-radius: 16rpx;
}

.category-scroll {
	width: 100%;
	white-space: nowrap;
	margin-bottom: 20rpx;
}

.category-list {
	display: inline-flex;
	padding: 0 10rpx;
}

.category-item {
	padding: 12rpx 30rpx;
	margin: 0 10rpx;
	font-size: 28rpx;
	color: #666;
	background: #f5f5f5;
	border-radius: 30rpx;
	transition: all 0.3s;
}

.category-item.active {
	color: #fff;
	background-color: #007AFF;
}

.notice-bar {
	display: flex;
	align-items: center;
	background: #fff7e6;
	padding: 16rpx 24rpx;
	border-radius: 8rpx;
	overflow: hidden;
}

.notice-icon {
	flex-shrink: 0;
	margin-right: 16rpx;
	font-size: 32rpx;
}

.notice-content {
	flex: 1;
	overflow: hidden;
	position: relative;
}

.notice-text {
	white-space: nowrap;
	font-size: 26rpx;
	color: #fa8c16;
	transition: transform 0.3s linear;
}

.hotWords {
	display: flex;
	align-items: center;
	margin: 10px;
	flex-wrap: wrap;
}

.hotWords-item {
	padding: 8rpx 15rpx;
	margin: 5px 10rpx;
	font-size: 24rpx;
	color: #666;
	background: #e2e2e2;
	border-radius: 30rpx;
	transition: all 0.3s;
}

.hotWords-item.active {
	color: #fff;
	background-color: #007AFF;
}
</style>
<style>
/* 分隔区域样式 */
.divider {
	height: 20rpx;
	background-color: #f5f6f7;
	box-shadow: inset 0 1px 2rpx rgba(0, 0, 0, 0.05);
}

.post-list {
	padding: 0 20rpx;
}

.post-item {
	display: flex;
	flex-wrap: wrap;
	align-items: center;
	padding: 20rpx 0;
	border-bottom: 1rpx solid #dadada;
}

.avatar {
	width: 60rpx;
	height: 60rpx;
	border-radius: 50%;
	margin-right: 20rpx;
	flex-shrink: 0;
}

.category-tag {
	padding: 4rpx 12rpx;
	border-radius: 6rpx;
	font-size: 24rpx;
	margin-right: 16rpx;
	flex-shrink: 0;
}

/* 分类标签颜色 */
.category-question {
	background-color: #e6f7ff;
	color: #1890ff;
}

.category-share {
	background-color: #f6ffed;
	color: #52c41a;
}

.post-title {
	flex: 1;
	font-size: 28rpx;
	color: #333;
	margin-right: 20rpx;
	/* 标题过长时显示省略号 */
	white-space: nowrap;
	overflow: hidden;
	text-overflow: ellipsis;
	width: 100%;
}

.post-content {
	width: 100%;
	margin-top: 5px;
	display: flex;
	align-items: center;
}

.username {
	font-size: 24rpx;
	color: #999;
	margin-right: 20rpx;
	flex-shrink: 0;
}

.update-time {
	font-size: 24rpx;
	color: #999;
	flex-shrink: 0;
}

/* 空状态样式 */
.empty-state {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	padding: 100rpx 0;
	background: #fff;
}

.empty-image {
	width: 240rpx;
	height: 240rpx;
	margin-bottom: 30rpx;
}

.empty-text {
	font-size: 28rpx;
	color: #999;
	margin-bottom: 40rpx;
}
</style>
<style>
.container {
	padding: 20rpx;
	height: 100vh;
}

.filter-section {
	display: flex;
	justify-content: space-between;
	margin-bottom: 20rpx;
}

.filter-item {
	flex: 1;
	background-color: #f5f5f5;
	padding: 20rpx;
	margin: 0 10rpx;
	border-radius: 8rpx;
	text-align: center;
}

.subtitle {
	font-size: 24rpx;
	color: #999;
	display: block;
	height: 40px;
	/* 行高 × 行数 */
	line-height: 20px;
	overflow: hidden;
	text-overflow: ellipsis;
}

.search-box {
	position: relative;
	margin-bottom: 20rpx;
	padding: 0 20rpx;
}

.search-input {
	width: 100%;
	height: 80rpx;
	border: 1px solid #eaa8a3;
	/* 添加边框 */
	border-radius: 8rpx;
	padding: 0 80rpx 0 30rpx;
	box-sizing: border-box;
	font-size: 28rpx;
}

.placeholder-style {
	color: #999;
	font-size: 28rpx;
}

.search-icon {
	position: absolute;
	right: 40rpx;
	top: 50%;
	transform: translateY(-50%);
	color: #999;
	width: 25px;
}

.post-button {
	background-color: #007AFF;
	color: #fff;
	text-align: center;
	padding: 20rpx;
	border-radius: 8rpx;
	margin-bottom: 20rpx;
}

.section {
	background-color: #f5f5f5;
	padding: 20rpx;
	border-radius: 8rpx;
	margin-bottom: 20rpx;
}


.normal-posts {
	min-height: 400rpx;
	/* 可根据实际需求调整 */
}
</style>