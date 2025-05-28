<template>
    <el-breadcrumb separator="/">
        <template v-for="(item, index) in list" :key="index">
            <template v-if="index + 1 !== list.length">
                <el-breadcrumb-item
                    ><router-link :to="item.path">{{ item.meta.title }}</router-link></el-breadcrumb-item
                >
            </template>
            <template v-else>
                <!-- <span>{{ item.meta.title }}</span> -->
                <el-breadcrumb-item>{{ item.meta.title }}</el-breadcrumb-item>
            </template>
        </template>
    </el-breadcrumb>
</template>
<script lang="ts">
import { useRoute } from 'vue-router'
import { defineComponent, ref, watch } from 'vue'
export default defineComponent({
    name: 'Breadcrumbs',

    setup() {
        const route = useRoute()
        const list = ref([])
        const matchRouter = (e: any): void => {
            const { matched } = e
            // console.log('route changed ', matched)
            list.value = matched
        }
        // created
        matchRouter(route)

        // 监视路由
        watch(route, matchRouter)

        return { list }
    },
})
</script>
