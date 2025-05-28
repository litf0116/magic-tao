<template>
	<el-dialog v-model="dialogVisible" title="发帖" width="800px" draggable destroy-on-close append-to-body
		:close-on-click-modal="false">
		<el-form ref="formRef" :model="form" :rules="formRules" style="max-width: 800px" label-width="auto"
			class="demo-ruleForm" status-icon>
			<el-form-item label="标题" prop="title" class="col-span-2">
				<el-input v-model="form.title" />
			</el-form-item>
			<el-form-item label="类型" prop="categoryId" class="col-span-2">
				<div class="classification type">
					<div class="type-list">
						<template v-for="item in categoriesItems">
							<div class="item" :class="{ active: activeKey.includes(item.key) }"
								@click="onPostCategoryActive(item.key)" v-if="item.key != -1">
								{{ item.name }}
							</div>
						</template>
					</div>
				</div>
			</el-form-item>
			<el-form-item label="内容" prop="content">
				<div style="border: 1px solid #EEEFF0;border-radius: 5px;overflow: hidden;width: 100%;">
					<Toolbar style="border-bottom: 1px solid #EEEFF0" :editor="editorRef" :defaultConfig="toolbarConfig"
						:mode="mode" />
					<Editor style="height: 300px; overflow-y: hidden;" v-model="form.content" @on-change="handleChange"
						:defaultConfig="editorConfig" :mode="mode" @onCreated="handleCreated" />
				</div>
			</el-form-item>
			<div>
				<el-button type="primary" @click="submitForm"> 更新 </el-button>
				<el-button @click="dialogVisible = false">关闭</el-button>
			</div>
		</el-form>
	</el-dialog>
</template>

<script setup lang="ts">
import { Editor, Toolbar } from '@wangeditor/editor-for-vue'
import { IToolbarConfig } from '@wangeditor/editor'
import '@wangeditor/editor/dist/css/style.css'
import { shallowRef } from 'vue';
import cache from '@/utils/cache'
import base64 from '@/utils/base64'
import axios from 'axios'
import api from '@/api'
import { Add, Edit, GetPostDetail } from "@/api/postAPI"
import { GetTypeList } from '@/api/postCategoryAPI'

//分类数据
const categoriesItems = ref([])

const toolbarConfig: Partial<IToolbarConfig> = {
	toolbarKeys: ['headerSelect', 'bold', 'italic', 'underline', 'through', 'bulletedList', 'justifyLeft', 'justifyCenter', 'justifyRight', 'undo', 'redo', 'uploadImage', 'insertLink']
}
const editorConfig = {
	MENU_CONF: {
		uploadImage: {
			// 自定义图片上传
			async customUpload(file: any, insertFn: any) {
				let formData = new FormData();
				formData.append('authorization', `UPYUN ${userName}:${signature.value}`);
				formData.append('policy', policy.value);
				formData.append('file', file.raw || file);
				axios.post(actionUrl.value, formData).then(async (res) => {
					console.log('upload result', res);
					const url = `${imgUrl}${res.data.url}`;
					// 最后插入图片
					const editor = editorRef.value;
					editor.dangerouslyInsertHtml(` <img src="${url}" alt="${file.name}" />`);
					// 手动触发表单验证
					if (formRef.value) {
						formRef.value.validateField('content')
					}
				}).catch((err) => {
					console.log(err);
					Tips.error('上传失败');
				})
			},
		},
	},
	// 添加以下配置
	onblur: function (editor: any) {
		return false  // 返回 false 阻止默认失焦行为
	}
}
const signature = ref('')
const imgUrl = import.meta.env.VITE_APP_UPYUN_IMG_URL
const bucketName = import.meta.env.VITE_APP_UPYUN_BUCKET_NAME
const userName = import.meta.env.VITE_APP_UPYUN_USERNAME
const policy = ref('')
const actionUrl = computed(() => `https://v0.api.upyun.com/${bucketName}`)

//富文本框值更改
const handleChange = (editor: any) => {
	form.value.content = editor.isEmpty() ? "" : editor.getHtml()
	// 手动触发表单验证
	if (formRef.value) {
		formRef.value.validateField('content')
	}
}
const editorRef = shallowRef()
const mode = 'default'
const handleCreated = (editor: any) => {
	editorRef.value = editor
}
const dialogVisible = ref(false)
const show = (e: boolean, data) => {
	dialogVisible.value = e
	if (data != null) {
		form.value = data;
		activeKey.value = form.value.categoryId.split(",").map(Number);
	}
}
defineExpose({
	show,
})

const emit = defineEmits(['onSaved'])

const activeKey = ref<number[]>([])
const formRef = ref(null)
const form: any = ref({ postId: 0 });
const formRules = computed(() => {
	const rules = {
		title: [{ required: true, message: '请输入标题', trigger: 'blur' }],
		content: [
			{
				required: true,
				message: '请输入内容',
				trigger: 'blur',
				validator: (rule: any, value: string, callback: any) => {
					// 去除HTML标签后检查是否为空
					const textContent = value ? value.replace(/<[^>]+>/g, '').trim() : '';
					if (!value || !textContent) {
						callback(new Error('请输入内容'));
					} else {
						callback();
					}
				}
			}
		]
	}

	return rules
})
onMounted(() => {
	getAuth();
	getData();
})
//获取分类数据
const getData = async () => {
	var res = await GetTypeList();
	if (res.data) {
		res.data.forEach((item, index) => {
			categoriesItems.value.push({
				key: item.categoryId,
				name: item.name,
			});
		})
	}
}
//获取又拍云上传信息
const getAuth = async () => {
	const cachedata = cache.getWithExpiry('upyun')
	if (cachedata && cachedata.policy && cachedata.signature) {
		signature.value = cachedata.signature
		policy.value = cachedata.policy
	} else {
		// @ts-ignore
		const date = new Date().toGMTString()
		const opts = {
			'save-key': `/{year}{mon}{day}/{random32}{.suffix}`,
			bucket: bucketName,
			expiration: Math.round(new Date().getTime() / 1000) + 43200, //12hour
			date: date,
		}
		policy.value = base64.encode(JSON.stringify(opts))
		const data = ['POST', '/' + bucketName, date, policy.value].join('&')
		await api.upload.getSignature({ data: data }).then((res) => {
			signature.value = res.signature
			cache.setWithExpiry('upyun', { signature: signature.value, policy: policy.value }, 600)
			// emit("onKeyReady", { url: actionUrl, key: authorization.value, policy: policy });
		})
	}
}
//点击分类筛选
const onPostCategoryActive = (key) => {
	const index = activeKey.value.indexOf(key)
	if (index === -1) {
		activeKey.value.push(key)
	} else {
		activeKey.value.splice(index, 1)
	}
}
//提交表单
const submitForm = () => {
	if (!formRef.value) return
	formRef.value.validate(async (valid) => {
		if (valid) {
			form.value.categoryId = activeKey.value.join(',')
			var res;
			if (form.value.postId == 0) {
				res = await Add(form.value);
			} else {
				res = await Edit(form.value);
			}
			if (res.status === 200) {
				Tips.success(form.value.postId == 0 ? "发帖成功" : "修改成功");
				dialogVisible.value = false;
				emit('onSaved')
			} else {
				Tips.error("发帖失败，请稍后重试！");
			}
		}
	})
}

</script>
<style scoped>
.type {
	display: flex;
	height: 44px;
	align-items: center;
	line-height: 44px;
	flex-wrap: wrap;
	margin-top: -6px;

	.title {
		font-weight: 400;
		font-size: 13px;
		color: #C3C3C3;
		margin-right: 55px;
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
</style>
