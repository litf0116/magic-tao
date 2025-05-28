import { defineStore } from "pinia"
import { ref } from "vue"

export const useCounterStore = defineStore("counter", () => {
    const counter = ref(0)
    const add = () => {
        counter.value++
    }
    return { counter, add }
})
