<template>
    <main class="admin h-screen w-screen grid md:grid-cols-[auto_1fr]">
        <side-bar />
        <section class="content bg-gray-100 grid grid-rows-[auto_1fr] overflow-hidden">
            <div class="border border-b-px border-gray-500">
                <!-- <div class="i-mdi-menu w-6 h-6 text-blue-500" @click="sidebar_toggle"></div> -->
                <NavBar />
                <!-- <HistoryMenu /> -->
            </div>
            <div class="overflow-y-auto pb-32">
                <router-view v-slot="{ Component, route }">
                    <component :is="Component" :key="route.fullPath" class="m-5" />
                </router-view>
            </div>
        </section>
    </main>
</template>

<script lang="ts" setup>
import SideBar from './SideBar/index.vue'
import NavBar from './NavBar.vue'
</script>
