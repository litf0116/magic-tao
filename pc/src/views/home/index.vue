<template>
  <div class="home-container">
    <!-- 广告展示区域 -->
    <AdvertisementBanner/>

    <!-- 双模块容器 -->
    <div class="modules-container">
      <TradingPostCard />
      <AuctionCard />
    </div>
  </div>
</template>

<script setup lang="ts">
import AdvertisementBanner from './components/AdvertisementBanner.vue'
import TradingPostCard from './components/TradingPostCard.vue'
import AuctionCard from './components/AuctionCard.vue'
</script>

<style lang="scss" scoped>
.home-container {
  position: relative;
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.modules-container {
  display: flex;
  justify-content: center;
  gap: 40px;
  margin-top: 0;

  @media (max-width: 1400px) {
    flex-direction: column;
    align-items: center;
    gap: 30px;
  }

  @media (max-width: 768px) {
    gap: 20px;
    padding: 0 10px;
  }
}
</style>
