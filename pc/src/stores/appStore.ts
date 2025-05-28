import { defineStore } from 'pinia'
import { useLocalStorage } from '@vueuse/core'
import type { Ref } from 'vue'

export const useAppStore = defineStore('app', () => {
    //state
    const sidebar_status: Ref<string> = useLocalStorage('sidebar_status', 'opened')

    //getter
    const sidebar_is_open: Ref<boolean> = computed(() => {
        return sidebar_status.value == 'opened'
    })

    //action
    const sidebar_toggle = () => {
        if (sidebar_is_open.value) {
            sidebar_status.value = 'closed'
        } else {
            sidebar_status.value = 'opened'
        }
    }
    return {
        sidebar_is_open,
        sidebar_toggle,
    }
})
