<template>
	<div class="flex space-x-1 gap-1 flex-wrap">
		<el-button :type="modelValue === '' ? 'primary' : 'default'" plain @click="onFiter({ status: '' })">
			全部
		</el-button>
		<el-button v-for="(x, k) in list" :key="k" :type="modelValue === x.id ? 'primary' : 'default'" plain
			@click="onFiter({ status: x.id })">
			{{ x.name }}
		</el-button>
	</div>
</template>

<script lang="ts" setup>
import api from '@/api'

const list = ref([
	// {
	//     id: 1,
	//     name: '勇者招募所',
	// },
	{
		id: 2,
		name: '拍卖行',
	},
])

defineProps({
	modelValue: {
		type: [String, Number],
		default: '',
	},
})

onMounted(() => {
	// api.mall.productOrder.getAll({ maxResultCount: 10 }).then((res) => {
	//     list.value = res.items;
	// });
})

const emit = defineEmits(['update:modelValue', 'change'])

function onFiter(e: any) {
	emit('update:modelValue', e.status)
	emit('change')
}
</script>
