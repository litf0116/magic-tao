<template>
  <div class="auction-card">
    <!-- 标题 -->
    <h2 class="title">拍卖行</h2>

    <!-- 内容区域 -->
    <div class="content-area">
      <!-- 拍品列表内容将在这里添加 -->
      <div class="placeholder">拍卖行内容即将上线...</div>
    </div>
  </div>
</template>

<script setup lang="ts">
// 拍卖行卡片组件
</script>

<style lang="scss" scoped>
.auction-card {
  width: 630px;
  height: 760px;
  background: url('@/assets/images/panel_background_2.png') no-repeat center center;
  background-size: 100% 100%;
  display: flex;
  flex-direction: column;
  align-items: center;

  .title {
    color: white;
    font-size: 36px;
    font-weight: bold;
    margin-top: 50px;
    text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.5);
  }

  .content-area {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 20px;
    margin-top: 30px;

    .placeholder {
      color: #666;
      font-size: 18px;
      text-align: center;
    }
  }
}
</style>