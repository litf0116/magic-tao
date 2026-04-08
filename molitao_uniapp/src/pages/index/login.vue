<template>
	<tui-page>
		<view class="h-[100vh] px-4 relative flex flex-col">
			<view class="flex-1 flex flex-col items-center flex-center">
				<image src="https://cdn.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png" class="h-[15vh]"
					mode="aspectFit" />
			</view>

			<button class="w-full bg-[#f4835a] text-white rounded-lg mb-4 zoom-in" :disabled="!agreePrivacy"
				@click="wxLogin(true)">
				快捷登录
			</button>

			<view class="flex items-center justify-center mb-4">
				<view class="flex items-center" @tap="togglePrivacy">
					<view class="w-5 h-5 rounded border-2 border-gray-300 flex items-center justify-center mr-2"
						:class="{ 'bg-green-500 border-green-500': agreePrivacy }">
						<text v-if="agreePrivacy" class="text-white text-xs">✓</text>
					</view>
					<text class="text-xs text-gray-500">
						我已阅读并同意
						<text class="text-[#f4835a]" @tap.stop="toAgreement">《用户协议》</text>
						和
						<text class="text-[#f4835a]" @tap.stop="toPrivacy">《隐私政策》</text>
					</text>
				</view>
			</view>

			<button class="w-full mb-32 rounded-6" :disabled="isloading" @tap="toHome">返回</button>
		</view>
	</tui-page>
</template>

<script setup lang="ts">
import api from '@/utils/api'
import { onShow } from '@dcloudio/uni-app'
const userStore = useUserStore()

const isloading = ref(false)
const agreePrivacy = ref(false)

onShow(async () => {
	// await userStore.code2Session().then(() => {});
})

const { toHome } = useTo()

const togglePrivacy = () => {
	agreePrivacy.value = !agreePrivacy.value
}

const toAgreement = () => {
	uni.navigateTo({ url: '/pages/protocol/agreement' })
}

const toPrivacy = () => {
	uni.navigateTo({ url: '/pages/protocol/privacy' })
}

function wxLogin(back: boolean) {
	if (!agreePrivacy.value) {
		uni.showToast({
			title: '请先阅读并同意用户协议和隐私政策',
			icon: 'none',
			duration: 2000
		})
		return
	}

	userStore.wxLogin().then(() => {
		if (back) {
			uni.$emit('refreshView');
			uni.navigateBack({})
		}
	})
}
</script>
<route lang="json">{
	"layout": "main",
	"style": {
		"navigationBarTitleText": "用户登录"
	}
}</route>
