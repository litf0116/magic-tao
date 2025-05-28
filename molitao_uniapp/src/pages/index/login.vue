<template>
	<tui-page>
		<view class="h-[100vh] px-4 relative flex flex-col">
			<view class="flex-1 flex flex-col items-center flex-center">
				<image src="https://cdn.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png" class="h-[15vh]"
					mode="aspectFit" />
				<!-- <text class="font-bold text-2xl my-6">魔力淘</text> -->
			</view>
			<button class="w-full bg-[#f4835a] text-white rounded-lg mb-4 zoom-in" @click="wxLogin(true)">
				快捷登录
			</button>
			<!-- <button
                class="w-full bg-[#f4835a] text-white rounded-lg mb-4"
                open-type="getPhoneNumber"
                @getphonenumber="getphonenumber($event, true)"
            >
                手机登录
            </button> -->

			<button class="w-full mb-32 rounded-6" :disabled="isloading" @tap="toHome">返回</button>
		</view>
	</tui-page>
</template>

<script setup lang="ts">
import api from '@/utils/api'
import { onShow } from '@dcloudio/uni-app'
const userStore = useUserStore()

const isloading = ref(false)

onShow(async () => {
	// await userStore.code2Session().then(() => {});
})

const { toHome } = useTo()

const form = ref({
	phoneNumber: '',
	password: '',
})

function wxLogin(back: boolean) {
	userStore.wxLogin().then(() => {
		if (back) {
			// 发送事件通知
			uni.$emit('refreshView');
			uni.navigateBack({})
		}
	})
}

async function login() {
	console.log(form.value)
	if (!/^1\d{10}$/.test(form.value.phoneNumber)) {
		Tips.info('请输入正确的手机号码')
		return
	}

	if (!/^[^ ]{6,32}$/.test(form.value.password)) {
		Tips.info('请输入正确的密码')
		return
	}

	debounce(realLogin, 300)()
}

async function realLogin() {
	console.log('realLogin')
	isloading.value = true
	await userStore.login(form.value.phoneNumber, form.value.password).then(() => {
		isloading.value = false
		uni.navigateBack({})
	})
}

function getphonenumber(e: any, back: boolean) {
	const { detail } = e
	if (detail.errMsg === 'getPhoneNumber:ok') {
		userStore
			.phoneLogin({
				iv: detail.iv,
				encryptedData: detail.encryptedData,
			})
			.then(() => {
				if (back) {
					uni.navigateBack({})
				}
			})
	} else if (detail.errMsg === 'getPhoneNumber:fail user deny') {
		uni.showToast({
			// icon: "error",
			title: '手机登录失败',
		})
	} else {
		uni.showToast({
			icon: 'none',
			title: detail.errMsg,
		})
	}
}
</script>
<route lang="json">{
	"layout": "main",
	"style": {
		"navigationBarTitleText": "用户登录"
	}
}</route>
