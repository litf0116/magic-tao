<template>
    <view class="roll-container">
        <!-- #ifndef H5 -->
        <view class="roll-wrap">
            <view
                class="roll-main"
                :style="{
                    transform: `translateX(${translateX}px)`,
                    transition: isTransition ? `transform ${duration}s linear` : 'none',
                }"
                @transitionend="onTransitionEnd"
            >
                <text
                    v-for="(item, index) in displayList"
                    :key="index"
                    class="text-item"
                    :style="{
                        fontSize: `${fontSize}rpx`,
                        color: color,
                    }"
                    >{{ item }}</text
                >
            </view>
        </view>
        <!-- #endif -->
        <!-- #ifdef H5 -->
        <view class="roll-wrap h5-roll-wrap">
            <view
                class="roll-main h5-roll-main"
                :style="{
                    animationDuration: `${duration}s`,
                }"
            >
                <text
                    v-for="(item, index) in displayList"
                    :key="index"
                    class="text-item"
                    :style="{
                        fontSize: `${fontSize}rpx`,
                        color: color,
                    }"
                    >{{ item }}</text
                >
            </view>
        </view>
        <!-- #endif -->
    </view>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount, nextTick, watch } from 'vue'
import { getCurrentInstance } from 'vue'

const props = defineProps({
    list: {
        type: Array,
        default: () => [],
    },
    duration: {
        type: Number,
        default: 10,
    },
    fontSize: {
        type: Number,
        default: 28,
    },
    color: {
        type: String,
        default: '#fa8c16',
    },
})

// 公共的 displayList，所有平台都需要
const displayList = computed(() => [...props.list, ...props.list])

// #ifndef H5
const translateX = ref(0)
const isTransition = ref(false)

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
    query
        .select('.roll-wrap')
        .boundingClientRect((wrapRect) => {
            if (!wrapRect) {
                setTimeout(initSize, 3000)
                return
            }
            wrapWidth = wrapRect.width
            query
                .select('.roll-main')
                .boundingClientRect((contentRect) => {
                    if (!contentRect) {
                        setTimeout(initSize, 3000)
                        return
                    }
                    contentWidth = contentRect.width / 2
                    if (contentWidth > wrapWidth) {
                        startScroll()
                    }
                })
                .exec()
        })
        .exec()
}

watch(
    () => props.list,
    (newVal) => {
        if (newVal && newVal.length) {
            nextTick(() => {
                setTimeout(() => {
                    initSize()
                }, 300)
            })
        }
    },
    { deep: true }
)

onMounted(() => {
    if (props.list && props.list.length) {
        setTimeout(() => {
            initSize()
        }, 300)
    }
})

onBeforeUnmount(() => {
    isTransition.value = false
    translateX.value = 0
})
// #endif
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

/* H5 CSS动画模式 */
.h5-roll-wrap {
    overflow: hidden;
}

.h5-roll-main {
    white-space: nowrap;
    animation: h5RollMove linear infinite;
}

@keyframes h5RollMove {
    0% {
        transform: translateX(0);
    }
    100% {
        transform: translateX(-50%);
    }
}
</style>
