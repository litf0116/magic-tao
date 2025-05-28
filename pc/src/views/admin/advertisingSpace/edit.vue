<template>
	<el-dialog v-model="isShow" :title="title" :width="width" :close-on-click-modal="false" append-to-body
		destroy-on-close>
		<el-form ref="formRef" :rules="formRules" :model="form" label-position="top">
			<el-form-item label="类型" prop="type">
				<el-select v-model="form.type" placeholder="请选择">
					<el-option label="首页" :value="1" />
					<el-option label="贴吧" :value="2" />
				</el-select>
			</el-form-item>
			<el-form-item label="标题" prop="title">
				<el-input v-model="form.title" />
			</el-form-item>
			<el-form-item label="图片" prop="imageUrl">
				<tt-upload v-model="form.imageUrl" css-class="avatar-uploader" @onUploaded="handleUploaded">
					<img v-if="form.imageUrl" :src="form.imageUrl" class="max-w-300px" />
					<el-icon v-else class="avatar-uploader-icon">
						<Plus />
					</el-icon>
				</tt-upload>
			</el-form-item>
			<el-form-item label="状态" prop="status">
				<el-switch v-model="form.status" active-color="#13ce66" inactive-color="#ff4949" :active-value="1"
					:inactive-value="0" />
			</el-form-item>
			<el-form-item label="跳转地址" prop="url">
				<el-input v-model="form.url" type="url" />
			</el-form-item>
		</el-form>
		<template #footer>
			<el-button type="default" @click="isShow = false">取消</el-button>
			<el-button type="primary" @click="handleSave">确定</el-button>
		</template>
	</el-dialog>
</template>

<script setup lang="ts">
import { Add, Edit } from '@/api/advertisingSpaceAPI'
import { ElMessage, FormInstance, FormRules } from 'element-plus'

defineProps({
	width: {
		type: String,
		default: '60%',
	},
})
const emit = defineEmits(['change'])

const form = ref({
	id: 0,
	type: 1,
	title: "",
	imageUrl: "",
	status: 1,
	url: ""
})
// 表单验证规则
const formRules = computed(() => {
	const rules = {
		type: [{ required: true, message: '请选择类型', trigger: 'change' }],
		// title: [{ required: true, message: '请输入标题', trigger: 'blur' }],
		url: [{ required: true, message: '请输入跳转地址', trigger: 'blur' }],
	}

	return rules
})
const formRef = ref(null)
const isShow = ref(false)

const title = computed(() => {
	return form.value.id != 0 ? '编辑' : '新增'
})

const show = (dto) => {
	isShow.value = true;
	form.value = {
		id: 0,
		type: 1,
		title: "",
		imageUrl: "",
		status: 1,
		url: ""
	};
	if (dto != null) {
		form.value = dto
	}
}

const handleSave = () => {
	if (!formRef.value) return
	formRef.value.validate(async (valid) => {
		if (valid) {
			let _api
			if (form.value.id && form.value.id != 0) {
				_api = Edit
			} else {
				_api = Add
			}
			_api(form.value)
				.then(() => {
					isShow.value = false
					emit('change', form.value)
					Tips.success("成功");
				})
				.catch((err: any) => {
					Tips.error("服务器异常！");
				})
		}
	})
}



function handleUploaded(e: any) {
	form.value.imageUrl = e.url
}

defineExpose({ show })
</script>
