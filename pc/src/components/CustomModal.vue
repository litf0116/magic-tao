<template>
    <el-dialog
        v-model="dialogVisible"
        :title="props.title"
        :show-close="true"
        :close-on-click-modal="true"
        :close-on-press-escape="true"
        width="30%"
        @close="handleClose"
    >
        <div class="dialog-body">
            <slot></slot>
        </div>
        <template #footer>
            <span class="dialog-footer">
                <el-button v-if="props.showCancel" @click="cancel">
                    {{ props.cancelText }}
                </el-button>
                <el-button type="primary" @click="confirm">
                    {{ props.confirmText }}
                </el-button>
            </span>
        </template>
    </el-dialog>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { ElDialog, ElButton } from 'element-plus'

const props = defineProps({
    show: {
        type: Boolean,
        default: false,
    },
    title: {
        type: String,
        default: '',
    },
    showCancel: {
        type: Boolean,
        default: true,
    },
    cancelText: {
        type: String,
        default: '取消',
    },
    confirmText: {
        type: String,
        default: '确定',
    },
})

const emit = defineEmits(['update:show', 'cancel', 'confirm'])

const dialogVisible = computed({
    get: () => props.show,
    set: (value) => emit('update:show', value),
})

const handleClose = () => {
    emit('update:show', false)
}

const cancel = () => {
    emit('cancel')
    emit('update:show', false)
}

const confirm = () => {
    emit('confirm')
    emit('update:show', false)
}
</script>

<style scoped>
:deep(.el-dialog__body) {
    padding: 20px;
}

.dialog-footer {
    width: 100%;
    display: flex;
    justify-content: flex-end;
    gap: 12px;
}
</style>
