<template>
    <Teleport to="body">
        <Transition @beforeEnter="handleBeforeEnter" @afterEnter="handleAfterEnter" @enter="handleEnter">
            <div v-if="show" class="fixed z-50 context-menu overflow-hidden" :style="{ top: y + 'px', left: x + 'px' }">
                <div v-for="action in actions" :key="action.action" @click="emitAction(action.action)">
                    {{ action.label }}
                </div>
            </div></Transition
        ></Teleport
    >
</template>

<script setup>
const { actions, x, y, show } = defineProps(['actions', 'x', 'y', 'show'])
const emit = defineEmits(['action-clicked', 'close'])

const emitAction = (action) => {
    emit('action-clicked', action)
}

const closeContextMenu = () => {
    // console.log('closeContextMenu')
    // show.value = false
    emit('close')
}

onMounted(() => {
    window.addEventListener('click', closeContextMenu, true)
    window.addEventListener('contextmenu', closeContextMenu, true)
})

onUnmounted(() => {
    window.removeEventListener('click', closeContextMenu, true)
    window.removeEventListener('contextmenu', closeContextMenu, true)
})

function handleBeforeEnter(el) {
    // console.log('handleBeforeEnter', el, typeof el)
    el.style.height = 0
}

function handleAfterEnter(el) {
    // console.log('handleAfterEnter', el)
    el.style.transition = 'none'
}

function handleEnter(el) {
    // console.log('handleEnter', el)
    el.style.height = 'auto'
    const h = el.clientHeight
    el.style.height = 0
    console.log('h', h)
    requestAnimationFrame(() => {
        requestAnimationFrame(() => {
            el.style.height = h + 'px'
            el.style.transition = 'height 0.2s'
        })
    })
}
</script>

<style scoped>
.context-menu {
    position: absolute;
    background: white;
    border: 1px solid #ccc;
    box-shadow: 2px 2px 4px rgba(0, 0, 0, 0.2);
    min-width: 150px;
}

.context-menu div {
    padding: 10px;
    cursor: pointer;
}

.context-menu div:hover {
    background-color: #f0f0f0;
}
</style>
