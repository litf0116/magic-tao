<template>
    <div class="footer text-center text-14px text-[#47839E] mt-150px mb-50px">
        <div>所有资料来源于广大网友或者各大魔力资料站，本站所有图档等资料版权一一史克威尔艾尼克斯（中国）</div>
        <div class="mt-2">
            © 2024 www.molitao.top 公司：<span @click="toAdmin">黑龙江省魔淡网络科技有限公司</span>
            <a href="https://beian.miit.gov.cn/">黑ICP备2024017401号-1</a>
            增值电信业务经营许可证：黑B2-20240058
        </div>
        <div class="mt-2 flex justify-center gap-2">
            <router-link to="/agreement?type=user-agreement" class="agreement-link">用户协议</router-link>
            <span class="text-[#999]">|</span>
            <router-link to="/agreement?type=privacy-policy" class="agreement-link">隐私政策</router-link>
        </div>
    </div>
</template>

<script setup lang="ts">
const cnt = ref(0)

const router = useRouter()

function toAdmin() {
    cnt.value++
    if (cnt.value >= 3) {
        const link = router.resolve({ path: '/admin' })

        window.open(link.href, '_blank')
    }
}
</script>

<style scoped>
a {
    @apply text-[#47839E] underline;
}
.agreement-link {
    @apply text-[#47839E] no-underline hover:underline cursor-pointer;
}
</style>
