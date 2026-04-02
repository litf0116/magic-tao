<template>
    <div class="bg-white relative shadow-sm z-50 px-5 py-2 border-b-1 flex justify-between items-center">
        <div class="flex items-center">
            <div
                class="mr-2 w-5 h-5 text-gray-400"
                :class="[appStore.sidebar_is_open ? 'i-mdi-arrow-collapse-left' : 'i-mdi-menu']"
                @click.stop="appStore.sidebar_toggle"
            ></div>
            <Breadcrumbs />
        </div>
        <div class="flex justify-center items-center h-full gap-4">
            <!-- BEGIN: Account Menu -->
            <el-dropdown v-if="userStore.isLogin" class="h-full" :hide-on-click="true" @command="handleCommand">
                <div class="el-dropdown-link flex items-center">
                    <text class="i-mdi-user-circle w-6 h-6 text-blue-500 mr-2"></text>

                    <text class="text-lg"> {{ userStore.user.name }}</text>
                    <text class="ml-1 i-mdi-arrow-down w-4 h-4 text-gray-500/40"></text>
                </div>
                <template #dropdown>
                    <el-dropdown-menu>
                        <div class="px-4 py-2 text-sm text-gray-600 border-b">
                            保证金余额：¥{{ userStore.user.depositBalance || 0 }}
                        </div>
                        <el-dropdown-item :command="Command.depositPayment">
                            <span class="i-mdi:wallet-plus mr-2"></span>
                            保证金充值
                        </el-dropdown-item>
                        <el-dropdown-item :command="Command.changePassword">修改密码</el-dropdown-item>
                        <el-dropdown-item divided :command="Command.logout">退出登录</el-dropdown-item>
                    </el-dropdown-menu>
                </template>
            </el-dropdown>

            <el-button v-else type="primary" @click="toLogin">Login</el-button>
        </div>
    </div>
</template>
<script setup lang="ts">
import Breadcrumbs from '@/layouts/Breadcrumbs.vue'

const appStore = useAppStore()
const userStore = useUserStore()
const router = useRouter()

const toLogin = () => {
    router.push('/auth/login')
}

const goToDepositPayment = () => {
    router.push('/deposit-payment')
}

enum Command {
    'depositPayment',
    'changePassword',
    'logout',
}

const logout = async () => {
    await userStore.logout()
    router.push('/auth/login')
}

const handleCommand = (command: Command) => {
    switch (command) {
        case Command.depositPayment:
            goToDepositPayment()
            break
        case Command.logout:
            logout()
            break
        case Command.changePassword:
            console.log(Command.changePassword)
            break
        default:
            const _exhaustiveCheck: never = command
            console.log(_exhaustiveCheck)
    }
}
</script>
