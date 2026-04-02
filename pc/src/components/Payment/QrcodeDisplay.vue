<template>
    <div class="qrcode-display">
        <img :src="qrcodeDataUrl" alt="支付二维码" class="qrcode-image" />
        <p class="hint">使用微信扫描二维码支付</p>
    </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import QRCode from 'qrcode'

interface Props {
    codeUrl: string // 微信支付二维码链接（weixin://wxpay/bizpayurl/...）
    size?: number // 二维码尺寸，默认 256
}

const props = withDefaults(defineProps<Props>(), {
    size: 256,
})

const qrcodeDataUrl = ref<string>('')

const generateQRCode = async () => {
    if (!props.codeUrl) {
        qrcodeDataUrl.value = ''
        return
    }
    try {
        qrcodeDataUrl.value = await QRCode.toDataURL(props.codeUrl, {
            width: props.size,
            margin: 2,
            errorCorrectionLevel: 'M',
        })
    } catch (error) {
        console.error('生成二维码失败:', error)
        qrcodeDataUrl.value = ''
    }
}

watch(() => props.codeUrl, generateQRCode, { immediate: true })
watch(() => props.size, generateQRCode)

onMounted(() => {
    if (props.codeUrl) {
        generateQRCode()
    }
})
</script>

<style scoped>
.qrcode-display {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 16px;
    background: #ffffff;
    border: 1px solid #e8e8e8;
    border-radius: 4px;
}

.qrcode-image {
    width: 256px;
    height: 256px;
    object-fit: contain;
}

.hint {
    margin-top: 12px;
    font-size: 14px;
    color: #999999;
    text-align: center;
}
</style>
