<template>
    <div class="p-4 bg-gray-200">
        <div class="sm:px-6 lg:px-0 lg:col-span-9">
            <div class="shadow sm:rounded-md sm:overflow-hidden">
                <el-form v-if="form" ref="dataForm" :model="form" :rules="roleRule" label-position="top">
                    <div class="bg-white py-6 px-4 space-y-6 sm:p-6">
                        <div>
                            <h3 class="text-lg leading-6 font-medium text-gray-900">{{ title }}</h3>
                        </div>
                        <div class="grid grid-cols-12 gap-y-2 gap-x-6">
                            <div class="col-span-12 sm:col-span-4">
                                <el-form-item label="审核类型" prop="auditName" required>
                                    <el-select v-model="form.auditName" class="w-full" clearable>
                                        <el-option
                                            v-for="item in schema['auditDefinitions']"
                                            :key="item.value"
                                            :label="item.label"
                                            :value="item.value"
                                        />
                                    </el-select>
                                </el-form-item>
                            </div>
                            <div class="col-span-12 sm:col-span-4">
                                <el-form-item label="类型" prop="providerName" required>
                                    <el-select v-model="form.providerName">
                                        <el-option key="G" :value="'G'" label="全局" />
                                        <!-- <el-option :value="'T'" key="D" label="租户" /> -->
                                    </el-select>
                                </el-form-item>
                            </div>
                            <div class="col-span-12 sm:col-span-4">
                                <el-form-item label="键值" prop="providerKey">
                                    <el-input v-model="form.providerKey" />
                                </el-form-item>
                            </div>
                            <!-- <div class="col-span-12 sm:col-span-12">
                        <el-form-item label="编辑器" required>
                           <el-button type="button" @click="editShow = true">打开编辑器</el-button>
                        </el-form-item>
                     </div> -->
                        </div>
                        <div class="flex items-center justify-center">
                            <el-button type="primary" @click="onSubmit">提交审核流程</el-button>
                        </div>
                    </div>
                </el-form>
            </div>
        </div>
        <div class="mt-4 sm:px-6 lg:px-0 lg:col-span-9">
            <div class="shadow sm:rounded-md sm:overflow-hidden">
                <div class="bg-white py-6 px-4 space-y-6 sm:p-6">
                    <Editor />
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import api from "@/api";
import { ElMessage } from "element-plus";
import { useRoute, useRouter } from "vue-router";
import Editor from "./editor.vue";
import { createNodes, flatenNodes } from "@/utils/tree";

export default defineComponent({
    components: { Editor },
    setup(props: any, context: any) {
        const router = useRouter();
        const route = useRoute();

        onMounted(() => {
            const id = route.params.id;
            console.log("onMounted params", route.params);
            console.log("onMounted query", route.query);

            if (route.params.id) {
                data.form.id = route.params.id + "";
                data.title = "编辑审核流程";
            }
        });

        const data = reactive({
            title: "创建审核流程",
            previewShow: false,
            editShow: false,
            roleRule: {} as any,
            form: {} as any,
            schema: {} as any,
            formData: {} as any,
            onSubmit: () => {
                let _api;
                if (data.form.id && data.form.id != api.guid) {
                    _api = api.admin.auditFlow.update;
                } else {
                    _api = api.admin.auditFlow.create;
                }

                data.form.auditNodes = flatenNodes(rows.value);

                _api({ body: Object.assign({}, data.form) }).then((res) => {
                    // console.log(res)
                    ElMessage.success({
                        message: "提交成功,正在跳转至列表",
                        type: "success",
                    });
                    setTimeout(() => {
                        router.push({ name: "AuditList" });
                    }, 500);
                });
            },
        });

        const editorSubmit = (val: any) => {
            console.log("editorSubmit", val);
            data.editShow = false;
            data.form.value = val;
            console.log(data.form);
        };

        provide("editorSubmit", editorSubmit);

        const onload = () => {
            api.admin.auditFlow.getForEdit({ id: route.params.id }).then((res) => {
                data.form = res.data!;
                data.schema = res.schema!;
                rows.value = createNodes(res.data!.auditNodes!);

                if (route.query.name) data.form.name = route.query.name;
                if (route.query.providerName) data.form.providerName = route.query.providerName;
                if (route.query.providerKey) data.form.providerKey = route.query.providerKey;
            });
        };

        onload();

        const form = computed(() => data.form);
        const rows = ref([]);

        provide("form", form);
        provide("rows", rows);

        return {
            ...toRefs(data),
        };
    },
});
</script>
