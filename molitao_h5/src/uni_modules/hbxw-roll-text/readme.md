# hbxw-roll-text组件

## 介绍
文字滚播组件，用于文字滚动播放（如通知，广播等）

## 使用示例
推荐先直接复制示例代码到工程中看效果了解下使用方法再投入项目使用。

```html

<template>
	<view class="test-container">
		<text class="test-title">内容不够长，不会滚动</text>
		<hbxw-roll-text :list="testList0"></hbxw-roll-text>
		
		<view class="separate"></view>
		
		<text class="test-title">内容超出容器，滚动</text>
		<hbxw-roll-text :list="testList" @end="rolltextEnd"></hbxw-roll-text>
		
		<view class="separate"></view>
		
		<text class="test-title">滚动速度</text>
		<hbxw-roll-text :list="testList" :duration="10"></hbxw-roll-text>
		
		<view class="separate"></view>
		
		<text class="test-title">自定义文本样式</text>
		<hbxw-roll-text :list="testList" :duration="10" :customStyle="{color: 'red', fontSize: '32rpx', fontWeight: 'bold'}"></hbxw-roll-text>
	</view>
</template>

<script>
	export default {
		data() {
			return {
				testList0: ['1. 市说教任越你话所度建构专因都长。'],
				testList: ['1. 市说教任越你话所度建构专因都长。','2. 半之给响习明心感六小写直样为什国定加干应。']
			};
		},
		methods: {
			rolltextEnd() {
				console.log('---- rolltextEnd ----');
			}
		}
	}
</script>

<style lang="scss">
.roll-text-wrap{
	width: 100%;
}
.separate{
	width: 100%;
	height: 2rpx;
	margin: 20rpx 0;
	background-color: #ccc;
}
</style>



```

## API

### Props

| 属性名            | 类型          | 默认值         | 必填 | 说明                               |
| ---------------- | --------------| --------      | ---- | -----------------------------------|
| list             | Array         | null          | 是   | 滚播文本列表                     |
| customStyle      | Object        | {}       | 否   | 滚播项文本自定义样式   |
| duration         | Number        | 5            | 否   | 完成一次滚播的缓冲时间       |


### Events

| 事件名       | 说明   | 返回值                                                                                                                                                               |
| ------------ | ------------  |  ------------ 
| end       | 滚播完成一次触发的方法 |  无返回 |