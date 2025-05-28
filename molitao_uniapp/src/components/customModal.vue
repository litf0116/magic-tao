<!-- CustomModal.vue -->
<template>
	<!-- 最外层容器，通过v-if控制整个模态框的显示和隐藏 -->
	<view class="modal" v-if="show">
		<!-- 遮罩层，点击可关闭模态框(如果maskClosable为true) -->
		<view class="mask" @click="maskClick"></view>

		<!-- 模态框主体内容区 -->
		<view class="modal-content">
			<!-- 标题栏，如果没有传入title则不显示 -->
			<view class="modal-title" v-if="title">{{ title }}</view>

			<!-- 内容区域，使用插槽以支持自定义内容 -->
			<view class="modal-body">
				<slot></slot>
			</view>

			<!-- 底部按钮区域，可通过showFooter控制显示隐藏 -->
			<view class="modal-footer" v-if="showFooter">
				<!-- 取消按钮，可通过showCancel控制显示隐藏 -->
				<button class="btn-cancel" v-if="showCancel" @click="cancel">{{ cancelText }}</button>

				<!-- 确认按钮，可通过showConfirm控制显示隐藏 -->
				<!-- 当只显示确认按钮时，自动占满整行 -->
				<button class="btn-confirm" v-if="showConfirm" :style="{ width: showCancel ? '50%' : '100%' }"
					@click="confirm">{{ confirmText }}</button>
			</view>
		</view>
	</view>
</template>

<script>
export default {
	name: 'CustomModal',
	props: {
		// 控制模态框显示隐藏
		show: {
			type: Boolean,
			default: false
		},
		// 模态框标题
		title: {
			type: String,
			default: '提示'
		},
		// 是否显示底部按钮区域
		showFooter: {
			type: Boolean,
			default: true
		},
		// 是否显示取消按钮
		showCancel: {
			type: Boolean,
			default: true
		},
		// 是否显示确认按钮
		showConfirm: {
			type: Boolean,
			default: true
		},
		// 取消按钮文字
		cancelText: {
			type: String,
			default: '取消'
		},
		// 确认按钮文字
		confirmText: {
			type: String,
			default: '确定'
		},
		// 点击遮罩层是否可关闭
		maskClosable: {
			type: Boolean,
			default: true
		}
	},
	methods: {
		// 处理遮罩层点击事件
		maskClick() {
			if (this.maskClosable) {
				// 触发更新show状态
				this.$emit('update:show', false);
				// 触发取消事件
				this.$emit('cancel');
			}
		},
		// 处理取消按钮点击事件
		cancel() {
			this.$emit('update:show', false);
			this.$emit('cancel');
		},
		// 处理确认按钮点击事件
		confirm() {
			this.$emit('update:show', false);
			this.$emit('confirm');
		}
	}
}
</script>

<style scoped>
/* 最外层容器样式 */
.modal {
	position: fixed;
	top: 0;
	right: 0;
	bottom: 0;
	left: 0;
	z-index: 999;
	/* 确保模态框显示在最上层 */
}

/* 遮罩层样式 */
.mask {
	position: fixed;
	top: 0;
	right: 0;
	bottom: 0;
	left: 0;
	background: rgba(0, 0, 0, 0.6);
	/* 半透明黑色背景 */
}

/* 模态框主体内容样式 */
.modal-content {
	position: fixed;
	top: 50%;
	left: 50%;
	transform: translate(-50%, -50%);
	/* 居中显示 */
	width: 80%;
	background: #fff;
	border-radius: 10rpx;
}

/* 标题栏样式 */
.modal-title {
	padding: 20rpx;
	text-align: center;
	font-size: 32rpx;
	font-weight: bold;
	border-bottom: 1rpx solid #eee;
}

/* 内容区域样式 */
.modal-body {
	padding: 30rpx;
	min-height: 100rpx;
}

/* 底部按钮区域样式 */
.modal-footer {
	display: flex;
	border-top: 1rpx solid #eee;
}

/* 按钮基础样式 */
.btn-cancel,
.btn-confirm {
	height: 90rpx;
	line-height: 90rpx;
	text-align: center;
	font-size: 32rpx;
}

/* 取消按钮样式 */
.btn-cancel {
	width: 50%;
	color: #999;
	border-right: 1rpx solid #eee;
}

/* 确认按钮样式 */
.btn-confirm {
	width: 50%;
	color: #007AFF;
}
</style>