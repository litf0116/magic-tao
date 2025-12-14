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
    @apply w-[90vw] xl:w-[1232px] h-30px md:h-59px flex justify-center space-x-10px -mb-30px z-20;

    .nav-item {
        @apply w-163px flex flex-center text-center text-[#045a39] md:font-bold text-10px md:text-14px lg:text-18px cursor-pointer hover:text-[#b45000] hover:scale-105 transition-all duration-300;
        background: url('@/assets/images/menu_normal.png') no-repeat center 3px / 100% 100%;
        position: relative;

        /* 选中状态使用CSS效果替代 */
        &::after {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(180, 80, 0, 0.1);
            border-radius: inherit;
            opacity: 0;
            transition: opacity 0.3s ease;
        }
    }

    .nav-item.current {
        color: #b45000;
        transform: scale(1.05);

        &::after {
            opacity: 1;
        }
    }
}
</style>
