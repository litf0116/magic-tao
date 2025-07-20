<template>
    <div class="flex space-x-4 mt-4">
        <el-button :type="[modelValue === 0 ? 'primary' : 'default']" @click="onFiter(0)"> 审批中 </el-button>
        <el-button :type="[modelValue === 1 ? 'primary' : 'default']" @click="onFiter(1)"> 我的审核记录 </el-button>
    </div>
</template>

<script lang="ts" setup>
defineProps({
    modelValue: {
        type: Number,
        default: 0,
    },
})
const emit = defineEmits(['update:modelValue', 'change'])

function onFiter(e: any) {
    emit('update:modelValue', e)
    emit('change')
}
</script>
