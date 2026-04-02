<template>
    <div class="payment-container">
        <div class="payment-card">
            <!-- 标题 -->
            <div class="header">
                <h1 class="title">保证金支付</h1>
            </div>

            <!-- 金额 -->
            <div class="amount-section">
                <div class="label">支付金额</div>
                <div class="amount">¥ 51.00</div>
                <div class="hint">含手续费 1 元，实际到账 50 元魔力值</div>
            </div>

            <!-- 二维码 -->
            <div class="qrcode-section">
                <!-- QrcodeDisplay 组件占位符 -->
                <div class="qrcode-placeholder">
                    <p>二维码加载中...</p>
                </div>
                <div class="hint">使用微信扫描二维码支付</div>
            </div>

            <!-- 状态 -->
            <div class="status-section">
                <!-- StatusChecker 组件占位符 -->
                <p>🔄 支付中...</p>
            </div>

            <!-- 提示 -->
            <div class="tips-section">
                <h3>温馨提示</h3>
                <ul>
                    <li>保证金用于参与竞拍，可申请退还</li>
                    <li>支付成功后立即到账，无需等待</li>
                </ul>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
// TODO: Task 8 实现完整逻辑
// 后续将引入 QrcodeDisplay 和 StatusChecker 组件
// import QrcodeDisplay from '@/components/QrcodeDisplay/index.vue'
// import StatusChecker from '@/components/StatusChecker/index.vue'
</script>

<style scoped>
.payment-container {
    min-height: 100vh;
    background: #f5f5f5;
    padding: 40px 20px;
    display: flex;
    justify-content: center;
    align-items: flex-start;
}

.payment-card {
    background: white;
    width: 100%;
    max-width: 600px;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    padding: 32px;
    margin-top: 20px;
}

.header {
    margin-bottom: 24px;
    border-bottom: 1px solid #e8e8e8;
    padding-bottom: 16px;
}

.title {
    font-size: 20px;
    font-weight: bold;
    color: #333;
    text-align: center;
    margin: 0;
}

.amount-section {
    background: #f5f5f5;
    border-radius: 8px;
    padding: 24px;
    text-align: center;
    margin-bottom: 32px;
}

.amount-section .label {
    font-size: 14px;
    color: #666;
}

.amount-section .amount {
    font-size: 36px;
    font-weight: bold;
    color: #52c41a;
    margin: 16px 0;
}

.amount-section .hint {
    font-size: 12px;
    color: #999;
}

.qrcode-section {
    text-align: center;
    margin-bottom: 32px;
}

.qrcode-placeholder {
    width: 256px;
    height: 256px;
    border: 1px solid #e8e8e8;
    border-radius: 4px;
    padding: 16px;
    margin: 0 auto 16px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: white;
}

.qrcode-placeholder p {
    color: #999;
    font-size: 14px;
}

.qrcode-section .hint {
    font-size: 14px;
    color: #666;
}

.status-section {
    text-align: center;
    padding: 16px;
    background: #e6f7ff;
    border-radius: 4px;
    margin-bottom: 24px;
}

.status-section p {
    margin: 0;
    font-size: 14px;
    color: #1890ff;
}

.tips-section {
    background: #fffbe6;
    border: 1px solid #ffe58f;
    border-radius: 4px;
    padding: 16px;
}

.tips-section h3 {
    font-size: 14px;
    font-weight: bold;
    color: #333;
    margin: 0 0 12px 0;
}

.tips-section ul {
    margin: 0;
    padding-left: 20px;
}

.tips-section li {
    font-size: 12px;
    color: #666;
    line-height: 2;
}

/* 响应式设计 */
@media (max-width: 768px) {
    .payment-card {
        padding: 24px;
    }

    .qrcode-placeholder {
        width: 220px;
        height: 220px;
    }

    .amount-section .amount {
        font-size: 28px;
    }
}
</style>
