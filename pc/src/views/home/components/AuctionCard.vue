<template>
  <div class="auction-card">
    <!-- 标题栏 -->
    <div class="title-bar">
      <!-- 左侧标题图片 -->
      <img class="title-image" src="@/assets/images/title_auction.png" alt="拍卖行" />

      <!-- 中间描述文字 -->
      <div class="description">
        竞拍稀有道具，获取限定装备，体验刺激的拍卖乐趣
      </div>

      <!-- 右侧操作按钮 -->
      <button class="action-button">
        进入拍卖行
      </button>
    </div>

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
  padding-top: 50px;

  .title-bar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    width: 90%;
    height: 80px;
    background: rgba(255, 255, 255, 0.9);
    border-radius: 40px;
    padding: 0 30px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);

    .title-image {
      width: auto;
      height: 50px;
      object-fit: contain;
    }

    .description {
      flex: 1;
      margin: 0 30px;
      color: #333;
      font-size: 14px;
      text-align: center;
      line-height: 1.4;
    }

    .action-button {
      padding: 10px 20px;
      background: linear-gradient(135deg, #FF9800, #F57C00);
      color: white;
      border: none;
      border-radius: 20px;
      font-size: 14px;
      font-weight: bold;
      cursor: pointer;
      transition: all 0.3s ease;
      box-shadow: 0 2px 8px rgba(255, 152, 0, 0.3);

      &:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(255, 152, 0, 0.4);
      }
    }
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