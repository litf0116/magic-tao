<template>
	<view class="roll-container">
		<view class="roll-wrap">
			<view class="roll-main" :style="{
				transform: `translateX(${translateX}px)`,
				transition: isTransition ? `transform ${duration}s linear` : 'none'
			}" @transitionend="onTransitionEnd">
				<text v-for="(item, index) in displayList" :key="index" class="text-item" :style="{
					fontSize: `${fontSize}rpx`,
					color: color
				}">{{ item }}</text>
			</view>
		</view>
	</view>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { getCurrentInstance } from 'vue'

const props = defineProps({
	list: {
		type: Array,
		default: () => []
	},
	duration: {
		type: Number,
		default: 10
	},
	fontSize: {
		type: Number,
		default: 28
	},
	color: {
		type: String,
		default: '#fa8c16'
	}
})

const translateX = ref(0)
const isTransition = ref(false)
const displayList = computed(() => [...props.list, ...props.list])

let wrapWidth = 0
let contentWidth = 0
const instance = getCurrentInstance()

// 开始滚动
const startScroll = () => {
	isTransition.value = true
	translateX.value = -contentWidth
}

// 重置位置
const resetPosition = () => {
	isTransition.value = false
	translateX.value = 0

	// 使用 nextTick 确保状态更新后再开始新的滚动
	nextTick(() => {
		setTimeout(() => {
			startScroll()
		}, 50)
	})
}

// 过渡结束处理
const onTransitionEnd = () => {
	resetPosition()
}

// 初始化尺寸
const initSize = () => {
	const query = uni.createSelectorQuery().in(instance)
	query.select('.roll-wrap').boundingClientRect(wrapRect => {
		if (!wrapRect) {
			setTimeout(initSize, 3000)
			return
		}
		wrapWidth = wrapRect.width
		query.select('.roll-main').boundingClientRect(contentRect => {
			if (!contentRect) {
				setTimeout(initSize, 3000)
				return
			}
			contentWidth = contentRect.width / 2 // 因为内容复制了一份
			// 只有当内容宽度大于容器宽度时才滚动
			if (contentWidth > wrapWidth) {
				startScroll()
			}
		}).exec()
	}).exec()
}

// 监听列表变化
watch(() => props.list, (newVal) => {
	if (newVal && newVal.length) {
		nextTick(() => {
			setTimeout(() => {
				initSize()
			}, 300)
		})
	}
}, { deep: true })

onMounted(() => {
	if (props.list && props.list.length) {
		setTimeout(() => {
			initSize()
		}, 300)
	}
})

// 组件销毁时清理
onBeforeUnmount(() => {
	isTransition.value = false
	translateX.value = 0
})
</script>

<style lang="scss" scoped>
.roll-container {
	width: 100%;
	height: 80rpx;
	overflow: hidden;
}

.roll-wrap {
	width: 100%;
	height: 100%;
	overflow: hidden;
	position: relative;
}

.roll-main {
	height: 100%;
	display: flex;
	align-items: center;
	position: absolute;
	left: 0;
	top: 0;
	white-space: nowrap;
}

.text-item {
	display: inline-block;
	padding: 0 30rpx;
}
</style>