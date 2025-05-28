<template>
    <el-form ref="dataForm" label-position="left" label-suffix=":" label-width="100px" :model="form" :rules="roleRule">
        <el-form-item label="机构名称" prop="displayName">
            <el-input v-model="form.displayName" />
        </el-form-item>
        <div>
            <el-button type="default" @click="onCancel">取消</el-button>
            <el-button type="primary" @click="onSave">保存</el-button>
            <el-button type="primary" @click="onSave(1)">继续添加</el-button>
        </div>
    </el-form>
</template>

<script lang="ts">
import api from '@/api'

export default defineComponent({
    setup() {
        const dataForm = ref(null as any)
        const submit = inject('ouEditSubmit', () => {})
        const ouList = inject('ouList', [])
        const schema = inject('ouEditSchema', () => {})
        const data = reactive({
            roleRule: {
                displayName: [
                    {
                        required: true,
                        message: '请输入名称',
                        trigger: 'blur',
                    },
                ],
            } as any,
            onCancel: () => {
                submit()
            },
            onSave: (e = 0) => {
                let _api
                if (form.value.id && form.value.id != api.guid) {
                    _api = api.organizationUnit.updateOrganizationUnit
                } else {
                    _api = api.organizationUnit.createOrganizationUnit
                }
                _api({ body: Object.assign({}, form.value) }).then((res) => {
                    ElMessage.success({ message: '提交成功' })
                    submit(res, e)
                })
            },
            change: (e: any) => {
                console.log(e)
                console.log(form.value)
            },
        })

        const form = inject('ouEditForm', {} as any)

        onMounted(() => {
            console.log('onMounted', form.value)
        })

        return { form, schema, dataForm, ouList, ...toRefs(data) }
    },
})
</script>
