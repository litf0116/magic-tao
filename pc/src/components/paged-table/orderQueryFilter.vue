<template>
    <div class="flex space-x-1 gap-1 flex-wrap">
        <el-button :type="modelValue === '' ? 'primary' : 'default'" plain @click="onFiter({ status: '' })">
            全部
        </el-button>
        <el-button
            v-for="(x, k) in list"
            :key="k"
            :type="modelValue === x.id ? 'primary' : 'default'"
            plain
            @click="onFiter({ status: x.id })"
        >
            {{ x.name }}
        </el-button>
    </div>
</template>

<script lang="ts" setup>
import api from '@/api'

const list = ref([
    {
        id: 0,
        name: '待支付',
    },
    {
        id: 1,
        name: '待发货',
    },
    {
        id: 2,
        name: '待收货',
    },
    {
        id: 9,
        name: '已完成',
    },
    {
        id: 11,
        name: '售后',
    },
    {
        id: 12,
        name: '退款中',
    },
    {
        id: 13,
        name: '退款完成',
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
