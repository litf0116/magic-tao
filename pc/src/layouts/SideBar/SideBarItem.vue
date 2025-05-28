<template>
    <div v-if="!item.meta || !item.meta.hidden">
        <template v-if="!alwaysShowRootMenu && theOnlyOneChild && !theOnlyOneChild.children">
            <side-bar-item-link v-if="theOnlyOneChild.meta" :to="resolvePath(theOnlyOneChild.path)">
                <el-menu-item
                    :index="resolvePath(theOnlyOneChild.path)"
                    :class="{ 'submenu-title-noDropdown': isFirstLevel }"
                >
                    <template v-if="theOnlyOneChild.meta && theOnlyOneChild.meta.icon">
                        <el-icon><component :is="theOnlyOneChild.meta.icon" /></el-icon>
                    </template>

                    <!-- <div
                        v-if="theOnlyOneChild.meta && theOnlyOneChild.meta.icon"
                        class="theOnlyOneChild w-[2em] h-[2em]"
                        :class="theOnlyOneChild.meta.icon"
                    /> -->
                    <template v-if="theOnlyOneChild.meta.title" #title
                        ><span class="ml-2">{{ theOnlyOneChild.meta.title }}</span>
                        <template v-if="theOnlyOneChild.meta.num">
                            <div
                                class="bg-yellow-500 text-white w-6 h-6 text-sm rounded-full flex items-center justify-center ml-4 font-bold"
                            >
                                {{ theOnlyOneChild.meta.num }}
                            </div>
                        </template>
                    </template>
                </el-menu-item>
            </side-bar-item-link>
        </template>
        <el-sub-menu v-else :index="resolvePath(item.path)" teleported>
            <template #title>
                <template v-if="item.meta && item.meta.icon">
                    <el-icon><component :is="item.meta.icon" /></el-icon>
                </template>
                <!-- <div v-if="item.meta && item.meta.icon" class="w-[2em] h-[2em]" :class="item.meta.icon" /> -->
                <span v-if="item.meta && item.meta.title && !isCollapse" class="ml-2">
                    <!-- {{ $t('route.' + item.meta.title) }} -->
                    {{ item.meta.title }}
                </span>
            </template>
            <template v-if="item.children">
                <side-bar-item
                    v-for="child in item.children"
                    :key="child.path"
                    :item="child"
                    :is-collapse="isCollapse"
                    :is-first-level="false"
                    :base-path="resolvePath(child.path)"
                    class="nest-menu bg-dark-800"
                />
            </template>
        </el-sub-menu>
    </div>
</template>

<script setup lang="ts">
import { isExternal } from '@/utils/validate'
import SideBarItemLink from './SideBarItemLink.vue'

const props = defineProps({
    item: { type: Object, required: true },
    isCollapse: { type: Boolean, default: false },
    isFirstLevel: { type: Boolean, default: true },
    basePath: { type: String, default: '' },
})

const alwaysShowRootMenu = computed(() => props.item.meta && props.item.meta.alwaysShow)

const showingChildNumber = computed(() => {
    if (props.item.children) {
        const showingChildren = props.item.children.filter((item: any) => {
            if (item.meta && item.meta.hidden) {
                return false
            } else {
                return true
            }
        })
        return showingChildren.length
    }
    return 0
})

const theOnlyOneChild = computed(() => {
    if (showingChildNumber.value > 1) {
        return null
    }
    if (props.item.children) {
        for (const child of props.item.children) {
            if (!child.meta || !child.meta.hidden) {
                return child
            }
        }
    }
    // If there is no children, return itself with path removed,
    // because this.basePath already conatins item's path information
    return { ...props.item, path: '' }
})

function resolvePath(routePath: string) {
    if (isExternal(routePath)) {
        return routePath
    }
    if (isExternal(props.basePath)) {
        return props.basePath
    }
    return `${props.basePath}/${routePath}`.replace(/\/+/g, '/')
}
</script>

<style lang="scss"></style>

<style lang="scss">
.svg-icon {
    margin-right: 16px;
}
.el-sub-menu .el-sub-menu__icon-arrow {
    // overflow: auto !important;
    // right: 5px !important;
    display: none;
}

.el-tooltip__trigger.el-sub-menu__title {
    @apply justify-center p-0;
}
</style>
