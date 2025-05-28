<template>
	<view>
		<chatMain ref="chatRef" @onSend="send" @loadHistoryMessage="loadHistoryMessage" />
	</view>
</template>
<script setup lang="ts">
import { onLoad, onShow } from '@dcloudio/uni-app'
import chatMain from '@/components/chat/chatMain.vue'
import { ChatMessageType } from '@/composables/types'
const chatStore = useChatStore()
const chan = ref('')
const chatRef = ref<InstanceType<typeof chatMain> | null>(null)

onShow(() => { })

onLoad(async (pamams: any) => {
	//todo
	if (pamams != null) {
		const t = pamams.id + ''
		console.log('pamams', t)
		chan.value = t
		chatStore.connectServer().then(async () => {
			if (chatStore.hasGroup(t)) {
				await chatStore.SetCurrentChatId(parseInt(t.split('_')[0]), t.split('_')[1], true).then(() => {
					initGroup(t)
				})
			} else {
				Tips.error('未找到该群聊')
				uni.redirectTo({ url: '/pages/chat/index' })
			}
		})
	}
})

const initGroup = async (name: string) => {
	await loadHistoryMessage(true)
}

const historyMsgs = computed(() => {
	console.log('historyMsgs', chatStore.chatMap)
	return chatStore.chatMap.get(`${chatStore.currentChat.id}`) || []
})

async function loadHistoryMessage(force = false) {
	if (force) {
		await chatStore.joinChannel(chan.value)
	}

	chatRef.value!.history.loading = true
	const name = chan.value
	let lastTime = new Date().getTime()
	if (!force)
		if (historyMsgs.value && historyMsgs.value.length) {
			lastTime = historyMsgs.value[0].time!
		}
	await chatStore.getGroupHistory(name, lastTime, force).then((res) => {
		chatRef.value!.history.loading = false
		if (res.length < 20) {
			chatRef.value!.history.allLoaded = true
		}
	})
}

//LINK[epic=消息发送] - 群消息发送逻辑
function send(e: { type: ChatMessageType; data: string | object }) {
	if (e.type === ChatMessageType.Image) {
		chatStore.sendChannelMsg('[图片]', '', ChatMessageType.Image, e.data).then(() => {
			//
		})
	} else if (e.type === ChatMessageType.Text) {
		chatStore.sendChannelMsg(e.data as string, '', ChatMessageType.Text).then(() => {
			//
		})
	}
}
</script>

<route lang="json">{
	"style": {
		"navigationBarTitleText": "群聊"
	}
}</route>
