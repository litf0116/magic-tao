<template>
	<div class="bg">
		<div class="tb-content">
			<div class="top-div">
				<div class="classification type">
					<div class="title">分类</div>
					<div class="type-list">
						<div class="item" v-for="item in postCategoryList" :class="{ active: activeKey === item.key }"
							@click="onPostCategoryActive(item.key)">
							{{ item.name }}
						</div>
					</div>
				</div>
				<div class="bulletin" @click="onBulletin(latestBulletin)">
					公告：{{ latestBulletin }}
				</div>
			</div>
			<div class="search-div">
				<div class="search-input">
					<ElInput v-model="keywords" placeholder="请输入关键词" @keydown.prevent.enter="emitSearch" clearable>
						<template #append>
							<ElButton @click="emitSearch">搜索</ElButton>
						</template>
					</ElInput>
					<div class="hotWords">
						<div class="title" style="width: 80px;">搜索发现：</div>
						<div style="display: flex;flex-wrap: wrap;">
							<div class="item" v-for="(item, index) in hotWordsList"
								:class="{ active: hotWordsActiveKey === item.id }" @click="onHotWordActive(item)">
								{{ item.title }}
							</div>
						</div>
					</div>
				</div>
				<div class="post-but">
					<el-button type="primary" @click.stop="onPostBut">我要发贴</el-button>
				</div>
			</div>
			<div class="post-content">
				<!-- 帖子列表 -->
				<div class="posts-container" v-loading="loading">
					<!-- 列表为空 -->
					<el-empty v-if="posts.length === 0" description="暂无帖子" />

					<!-- 帖子项 -->
					<div v-else v-for="post in posts" :key="post.postId" class="post-item"
						@click="goToDetail(post.postId)">
						<!-- 头像 -->
						<div class="post-avatar">
							<el-avatar :size="30" :src="post.userAvatar" />
						</div>

						<!-- 分类标签 -->
						<div class="post-categories">
							<el-tag v-if="post.isTop != 0" type="danger" effect="plain" size="small">置顶</el-tag>
							<el-tag v-if="post.isEssence != 0" type="success" effect="plain" size="small">精华</el-tag>
							<el-tag v-if="post.postCategory" v-for="(item, index) in post.postCategory" :key="index"
								:type="tagTypes[index % tagTypes.length]" effect="plain" size="small">
								{{ item }}
							</el-tag>
						</div>

						<!-- 标题 -->
						<div class="post-title">
							<span class="title-text">{{ post.title }}</span>
						</div>

						<!-- 右侧信息 -->
						<div class="post-side">
							<span class="author-name">{{ post.userName }}</span>
							<span class="update-time">{{ post.createdAt }}</span>
						</div>
					</div>
				</div>

				<!-- 分页 -->
				<div class="pagination-container">
					<el-pagination v-model:current-page="page" v-model:page-size="pageSize" :total="total"
						:page-sizes="[10, 20, 50, 100, 300, 500]" layout="total, sizes, prev, pager, next"
						@size-change="onPageSizeChange" @current-change="onPageChange" />
				</div>
			</div>
		</div>

		<postItem ref="postRef" @on-saved="loadPosts" />

		<CustomDialog v-model:show="dialogVisible" :title="bulletinTitle" :showCancel="false" @confirm="handleConfirm">
			<div v-html="dialogContent"></div>
		</CustomDialog>
	</div>
</template>

<script setup lang="ts">
import { ElMessage, ElMessageBox } from 'element-plus'
import postItem from './components/postItem.vue'
import { GetTypeList } from '@/api/postCategoryAPI'
import { GetList } from "@/api/postAPI"
import { useRouter, useRoute } from 'vue-router'
import { GetLatestBulletin } from '@/api/PostBulletinAPI'
import { GetHotWordsList } from '@/api/HotWordsAPI'
import CustomDialog from '@/components/CustomModal.vue'

const router = useRouter()
const route = useRoute() // 获取当前路由信息
const tagTypes = ['', 'success', 'warning', 'danger', 'info'];
//帖子类型
const postCategoryList = reactive([
	{
		key: -1,
		name: '全部',
	},
])
const postRef = ref(null);
const activeKey = ref<number>(-1);
const keywords = ref("");
// 状态定义
const loading = ref(false);
const posts = ref([]);
const total = ref(0);
const page = ref(1);
const pageSize = ref(20);
const latestBulletin: any = ref({});//最新公告信息
const bulletinTitle: any = ref({});//最新公告标题
const hotWordsList = ref([])//热词列表
const hotWordsActiveKey = ref<number>(-1);//热词选中

const dialogVisible = ref(false)
const dialogContent = ref('<p>这是一段HTML内容</p>')

const handleCancel = () => {
	console.log('取消')
}

const handleConfirm = () => {
	console.log('确认')
}

onMounted(async () => {
	getData();
	loadPosts();
	var res = await GetLatestBulletin();
	if (res.data) {
		dialogContent.value = res.data.content;
		bulletinTitle.value = res.data.title;
		var text = res.data.content
			.replace(/<\/p><p>/g, '</p>   <p>')  // 在结束标签和开始标签之间添加空格
			.replace(/<[^>]+>/g, '')  // 然后再移除所有HTML标签
		latestBulletin.value = text;
	}
	//获取热词列表
	var resHotWords = await GetHotWordsList({ SkipCount: 1, MaxResultCount: 50 });
	if (resHotWords.data) {
		hotWordsList.value = resHotWords.data.items;
	}
})
//获取分类数据
const getData = async () => {
	var res = await GetTypeList();
	if (res.data) {
		res.data.forEach((item, index) => {
			postCategoryList.push({
				key: item.categoryId,
				name: item.name,
			});
		})
	}
}
//点击公告
const onBulletin = (key) => {
	dialogVisible.value = true;
}
//点击分类筛选
const onPostCategoryActive = (key) => {
	activeKey.value = key;
	loadPosts();
}
//点击热词筛选
const onHotWordActive = (data) => {
	hotWordsActiveKey.value = data.id;
	keywords.value = data.title;
	loadPosts();
}
//搜索
const emitSearch = () => {
	loadPosts();
}
//发帖
const onPostBut = () => {
	postRef.value?.show(true, null)
}
// 帖子操作
const viewPost = (post) => {
	// 实现查看帖子详情
	console.log('查看帖子:', post.postId)
}

// 分页处理
const onPageSizeChange = (newSize) => {
	pageSize.value = newSize;
	loadPosts();
}
const onPageChange = (newPage) => {
	page.value = newPage;
	loadPosts();
}
// 加载帖子列表
const loadPosts = async () => {
	loading.value = true
	if (keywords.value === "") {
		hotWordsActiveKey.value = -1;
	}
	try {
		var res = await GetList({ Type: activeKey.value, Keyword: keywords.value, SkipCount: page.value, MaxResultCount: pageSize.value });
		if (res.data) {
			posts.value = res.data.items;
			total.value = res.data.totalCount;
			posts.value.forEach((item, index) => {
				if (item.categoryName) {
					var arr = item.categoryName.split(",")
					item.postCategory = arr;
				}
			});
		}
	}
	catch (error) {
		ElMessage.error('获取帖子列表失败')
	} finally {
		loading.value = false
	}
}
//跳转到详情
const goToDetail = (id) => {
	const route = router.resolve({
		name: 'postDetail',
		params: {
			id: id
		}
	})
	window.open(route.href, '_blank')
}
</script>
<style scoped lang='less'>
.bg {
	width: 100%;
	height: 100%;
	// background-image: url("../../assets/bg.png");
	// background-size: 100% 100%;
}

.tb-content {
	padding: 12px;

	.top-div {
		display: flex;
		margin-top: 5px;
	}

	.bulletin {
		cursor: pointer;
		margin-left: 10px;
		border: 1px dashed #D8D8D8;
		width: 350px;
		padding: 10px;
		white-space: nowrap;
		/* 文本不换行 */
		overflow: hidden;
		/* 溢出隐藏 */
		text-overflow: ellipsis;
		/* 溢出显示省略号 */
	}

	.type {
		width: 800px;
		display: flex;
		height: 44px;
		align-items: center;
		line-height: 44px;
		flex-wrap: wrap;

		&.classification {
			border-bottom: 1px dashed #D8D8D8;
		}

		.title {
			font-weight: 400;
			font-size: 13px;
			color: #C3C3C3;
			margin-right: 10px;
			padding-left: 16px;
		}

		.type-list {
			display: flex;
			flex-wrap: wrap;

			.item {
				height: 100%;
				font-weight: 400;
				font-size: 13px;
				color: #333333;
				padding: 0 12px;
				cursor: pointer;

				&.active {
					color: #FF4D00;
				}
			}
		}
	}
}

.search-div {
	display: flex;

	.search-input {
		margin-top: 10px;
		display: flex;
		flex-direction: column;
		width: 750px;

		.el-input {
			height: 40px;
			outline: none;
			background: #FFFFFF;
			border-radius: 4px;
			border: 1px solid;
			border-image: linear-gradient(146deg, rgba(255, 116.00000068545341, 0, 1), rgba(255, 77.00000301003456, 0, 1)) 1 1;

			:deep(.el-input__wrapper) {
				box-shadow: none !important;

				&.is-focus {
					box-shadow: none !important;
				}
			}

			:deep(.el-input-group__prepend),
			:deep(.el-input-group__append) {
				.el-select {
					width: 80px;
					height: 46px;
					border: none;
					background: #fff;

					.el-select__wrapper {
						height: 100%;
						background: transparent;
						box-shadow: none !important;
					}

					.el-select__placeholder {
						color: #333333;
					}

					.el-select__caret {
						color: #333333;
					}
				}

				.el-button {
					width: 107px;
					height: 40px;
					background: #FF4D00;
					color: #fff;
					font-size: 18px;
					border-radius: 0px 4px 4px 0px;
				}
			}
		}

		.hotWords {
			display: flex;
			align-items: center;
			margin-top: 10px;

			.title {
				font-weight: 400;
				font-size: 13px;
				color: #C3C3C3;
				margin-right: 10px;
				padding-left: 16px;
			}

			.item {
				height: 100%;
				font-weight: 400;
				font-size: 13px;
				color: #333333;
				padding: 0 5px;
				cursor: pointer;

				&.active {
					color: #FF4D00;
				}
			}
		}
	}

	.post-but {
		width: 30%;
		display: flex;
		justify-content: end;

		.el-button {
			height: 40px;
			margin-top: 13px;
			width: 130px;
			letter-spacing: 5px;
			font-size: 16px;
		}
	}
}
</style>
<style scoped>
.pagination-container {
	display: flex;
	justify-content: end;
	margin-top: 5px;
}

.post-content {
	margin-top: 15px;
	cursor: pointer;
}

.post-item {
	display: flex;
	align-items: center;
	padding: 5px 16px;
	background: #fff;
	border-bottom: 1px solid #f0f0f0;
	transition: background-color 0.2s;
}

.post-item:hover {
	background-color: #f9f9f9;
}

.post-avatar {
	margin-right: 16px;
	flex-shrink: 0;
}

.post-categories {
	display: flex;
	gap: 8px;
	align-items: center;
	margin-right: 16px;
	flex-shrink: 0;
}

.post-categories .el-tag {
	margin: 0;
}

.post-title {
	flex: 1;
	min-width: 0;
	font-size: 13px;
	color: #333;
}

.title-text {
	overflow: hidden;
	text-overflow: ellipsis;
	white-space: nowrap;
	display: block;
}

.post-side {
	margin-left: 16px;
	flex-shrink: 0;
	white-space: nowrap;
}

.author-name {
	font-size: 13px;
	color: #666;
	margin-right: 16px;
}

.update-time {
	font-size: 12px;
	color: #999;
}
</style>