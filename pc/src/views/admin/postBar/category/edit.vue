<template>
	<el-dialog v-model="isShow" :title="title" :width="width" :close-on-click-modal="false" append-to-body
		destroy-on-close>
		<el-form ref="formRef" :rules="formRules" :model="form" label-position="top">
			<el-form-item label="类型名称" prop="name">
				<el-input v-model="form.name" />
			</el-form-item>
			<el-form-item label="状态" prop="status">
				<el-switch v-model="form.status" active-color="#13ce66" inactive-color="#ff4949" :active-value="1"
					:inactive-value="0" />
			</el-form-item>
			<el-form-item label="排序[数字越大越前]" prop="sort">
				<el-input v-model="form.sort" type="number" />
			</el-form-item>
		</el-form>
		<template #footer>
			<el-button type="default" @click="isShow = false">取消</el-button>
			<el-button type="primary" @click="handleSave">确定</el-button>
		</template>
	</el-dialog>
</template>

<script setup lang="ts">
import { Add, Edit } from '@/api/postCategoryAPI'

defineProps({
	width: {
		type: String,
		default: '60%',
	},
})
const emit = defineEmits(['change'])

const form = ref({
	id: 0,
	name: "",
	sort: "",
	status: 1,
})
// 表单验证规则
const formRules = computed(() => {
	const rules = {
		name: [{ required: true, message: '请输入类型名称', trigger: 'blur' }],
		sort: [{ required: true, message: '请输入排序', trigger: 'blur' }],
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
		name: "",
		sort: "",
		status: 1,
	};
	if (dto != null) {
		form.value = dto
		form.value.id = dto.categoryId;
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
