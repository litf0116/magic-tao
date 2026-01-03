<template>
    <main class="min-h-screen">
        <div class="wrap">
            <div class="header">
                <img
                    class="logo2"
                    :src="'https://image.molitao.top/molitao/2025-03-30/upload_4kascmjk9aaz06s7j0frqdjfq8m3zmxc.png'"
                />
            </div>
            <public-nav/>
            <div class="content px-4">
                <router-view v-slot="{ Component, route }">
                    <component :is="Component" :key="route.fullPath"/>
                </router-view>
            </div>
            <Footer/>
        </div>
    </main>
</template>

<script lang="ts" setup>
import PublicNav from './publicNav.vue'
import Footer from '@/components/Footer.vue'
</script>

<style lang="scss" scoped>
main {
    display: flex;
    justify-content: center;
    // align-items: center;
    background: #eefaff url('@/assets/images/migrated-image.png') no-repeat center top;
    background-size: cover;
    background-attachment: fixed;
}

.wrap {
    @apply w-[95vw] lg:w-[1232px] flex flex-col items-center relative;

    .header {
        @apply w-full h-[110px] flex flex-col justify-end items-start lg:w-1100px lg:mx-auto;
        background: url('https://image.molitao.top/20250330/04j40l4ynlbh3v3h4bgfe7j2pxiqjg8d.png') no-repeat;
        background-position: top -40px right -20px;
        background-size: 434px 363px;

        .logo2 {
            @apply w-344px;
        }
    }

    .content {
        @apply w-full min-h-400px relative mt-24px lg:mt-13px w-[90vw] xl:w-[1232px];
        background: url('https://image.molitao.top/molitao/2025-03-30/upload_qxgt8fo3iymdi0heth3rnqipc83rzawn.png') repeat-y center center / 100% 100%;
    }

    .content::before {
        content: '';
        @apply block absolute w-full h-18px -top-18px lg:-top-53px lg:h-53px left-0 right-0;
        background: url('https://image.molitao.top/molitao/2025-03-30/upload_iw2aq9rsovog4lr3v036irwm90nyos20.png') no-repeat center center / 100% 100%;
    }

    .content::after {
        content: '';
        @apply block absolute w-full h-18px -bottom-18px lg:h-45px lg:-bottom-45px left-0 right-0;
        background: url('https://image.molitao.top/molitao/2025-03-30/upload_to45oxex09l2uu1ltntj09n6z1x4y0df.png') no-repeat center center / 100% 100%;
    }
}
</style>
