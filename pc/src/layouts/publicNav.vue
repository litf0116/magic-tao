<template>
    <div class="nav">
        <a
            v-for="(x, k) in navList"
            :key="k"
            class="nav-item"
            :class="[route.path.startsWith(x.path) && x.path ? 'current' : '']"
            :to="x.path"
            @click="go(x)"
        >
            {{ x.name }}</a
        >
    </div>
</template>

<script setup lang="ts">
import { navList } from '@/routes/navList'

const route = useRoute()
const router = useRouter()

function go(x: { name: string; path: string; url?: string | undefined }) {
    if (x.url) {
        window.open(x.url, '_blank')
    } else if (x.path) {
        router.push({ path: x.path })
    }
}
</script>

<style lang="scss" scoped>
.nav {
    @apply w-[90vw] xl:w-[1232px] h-30px md:h-59px flex justify-center space-x-10px z-20;

    .nav-item {
        @apply w-163px flex flex-center text-center md:font-bold text-10px md:text-14px lg:text-18px cursor-pointer hover:scale-105 transition-all duration-300;
        color: #833a00;
        background: url('@/assets/images/menu_normal.png') no-repeat center 3px / 100% 100%;
    }

    .nav-item.current {
        color: #ffffff;
        background: url('@/assets/images/menu_selected.png') no-repeat center 3px / 100% 100%;
    }
}
</style>
