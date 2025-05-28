<template>
    <div
        class="min-h-300px w-full flex flex-col flex-center relative"
        @contextmenu.prevent="showContextMenu($event, '建设中')"
    >
        <div>建设中</div>

        <!-- Custom Context Menu -->
        <ContextMenu
            :show="showMenu"
            :actions="contextMenuActions"
            :x="menuX"
            :y="menuY"
            @action-clicked="handleActionClick"
            @close="showMenu = false"
        />
    </div>
</template>

<script setup lang="ts">
const showMenu = ref(false)
const menuX = ref(0)
const menuY = ref(0)
const targetRow = ref({})
const contextMenuActions = ref([
    { label: 'Edit', action: 'edit' },
    { label: 'Delete', action: 'delete' },
])

const showContextMenu = (event, user) => {
    event.preventDefault()
    showMenu.value = true
    targetRow.value = user
    requestAnimationFrame(() => {
        menuX.value = event.clientX
        menuY.value = event.clientY
    })
}

function handleActionClick(action) {
    console.log(action)
    console.log(targetRow.value)
}
</script>

<style scoped>
.overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: transparent;
    z-index: 49;
}

.overlay::before {
    content: '';
    position: absolute;
    width: 100%;
    height: 100%;
}

.overlay:hover {
    cursor: pointer;
}
</style>
