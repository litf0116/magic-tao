<template>
    <page-container :topBg="false">
        <slot></slot>
    </page-container>
</template>

<script setup>
import PageContainer from '@/components/page-container/page-container.vue'
</script>