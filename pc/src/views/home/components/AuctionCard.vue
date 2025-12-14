<template>
  <div class="auction-card">
    <!-- 标题栏 -->
    <div class="title-bar">
      <!-- 左侧容器：标题和描述 -->
      <div class="title-container">
        <!-- 标题图片 -->
        <img class="title-image" src="@/assets/images/title_auction.png" alt="拍卖行" />

        <!-- 描述文字 -->
        <div class="description">
          竞拍稀有道具，获取限定装备
        </div>
      </div>

      <!-- 右侧操作按钮 -->
      <button class="action-button">
        <span>点击进入</span>
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
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    padding: 0px;
    gap: 20px;

    width: 520px;
    height: 46px;

    .title-container {
      display: flex;
      flex-direction: row;
      align-items: center;
      padding: 0px;
      gap: 20px;

      margin: 0 auto;
      width: 257px;
      height: 46px;

      flex: none;
      order: 0;
      flex-grow: 0;

      .title-image {
        width: auto;
        height: 46px; /* 充满容器高度 */
        object-fit: contain;
        flex: none;
        order: 0;
        flex-grow: 0;
      }

      .description {
        width: 128px;
        height: 16px;

        font-family: 'Source Han Sans CN';
        font-style: normal;
        font-weight: 400;
        font-size: 16px;
        line-height: 100%;
        color: #5B3B2D;

        flex: none;
        order: 1;
        flex-grow: 0;
      }
    }

    .action-button {
      display: flex;
      flex-direction: row;
      justify-content: center;
      align-items: center;
      padding: 8px 12px;
      gap: 10px;

      width: 80px;
      height: 30px;

      background: #62331E;
      border: none;
      border-radius: 60px;
      cursor: pointer;
      transition: all 0.3s ease;

      flex: none;
      order: 1;
      flex-grow: 0;

      span {
        width: 56px;
        height: 14px;

        font-family: 'Source Han Sans CN';
        font-style: normal;
        font-weight: 500;
        font-size: 14px;
        line-height: 100%;
        color: #E6AC7A;

        flex: none;
        order: 0;
        flex-grow: 0;
      }

      &:hover {
        background: #7A4326;
        transform: scale(1.05);
      }
    }
  }

  .content-area {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 100px 72px;
    margin: 30px auto 0;
    width: calc(100% - 144px); /* 减去左右padding */

    .placeholder {
      color: #666;
      font-size: 18px;
      text-align: center;
    }
  }
}
</style>