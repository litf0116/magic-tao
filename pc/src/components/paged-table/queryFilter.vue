<template>
    <div class="flex space-x-1 gap-1 flex-wrap">
        <el-button
            v-if="showAll"
            :type="modelValue === '' ? 'primary' : 'default'"
            plain
            @click="onFiter({ status: '' })"
        >
            全部
        </el-button>
        <el-button
            v-for="(x, k) in list"
            :key="k"
            :type="modelValue === x[valueProp] ? 'primary' : 'default'"
            plain
            @click="onFiter({ status: x[valueProp] })"
        >
            {{ x[labelProp] }}
        </el-button>
    </div>
</template>

<script lang="ts" setup>
const prop = defineProps({
    modelValue: {
        type: [String, Number],
        default: '',
    },
    list: {
        type: Array,
        required: true,
        default: () => [],
    },
    labelProp: {
        type: String,
        default: 'label',
    },
    valueProp: {
        type: String,
        default: 'id',
    },
    showAll: {
        type: Boolean,
        default: true,
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
