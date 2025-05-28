<template>
    <div class="h-screen flex flex-col flex-grow border-r border-gray-200 overflow-y-auto overflow-hidden bg-dark">
        <el-menu
            active-text-color="#ffd04b"
            background-color="#222222"
            class="el-menu-vertical"
            text-color="#fff"
            :collapse="appStore.sidebar_is_open"
            :default-active="activeMenu"
            :unique-opened="true"
            :collapse-transition="false"
            mode="vertical"
        >
            <side-bar-item
                v-for="r in adminMenu"
                :key="r.path"
                :item="r"
                :base-path="r.path"
                :is-collapse="appStore.sidebar_is_open"
            />
        </el-menu>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import SideBarItem from './SideBarItem.vue'
const route = useRoute()
const appStore = useAppStore()
const permissionStore = usePermissionStore()

const activeMenu = computed(() => {
    const { meta, path } = route
    // if set path, the sidebar will highlight the path you set
    if (meta && meta.activeMenu) {
        return meta.activeMenu as string
    }
    return path
})

const adminMenu = computed(() => {
    return permissionStore.visibleRouter.find((r) => r.path === '/admin')?.children || []
})
</script>
<style lang="scss" scoped>
.el-menu {
    border: none;
}

.el-menu-vertical:not(.el-menu--collapse) {
    @apply w-40;
}
</style>
