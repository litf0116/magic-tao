<template>
    <div>
        <el-button @click="topUp">充值</el-button>
    </div>
</template>

<script setup lang="ts">
import api from '@/api'
function topUp() {
    console.log('upto')

    api.client.topUp({ openid: '', amount: '0.01', type: 'h5' }).then((res) => {})
}
</script>
