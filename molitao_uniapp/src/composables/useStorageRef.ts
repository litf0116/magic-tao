import { customRef } from 'vue'

export function useStorageRef<T>(key: string, defaultValue: T) {
    return customRef((track, trigger) => {
        return {
            get() {
                track()
                return uni.getStorageSync(key) || defaultValue
            },
            set(newValue) {
                uni.setStorageSync(key, newValue)
                trigger()
            },
        }
    })
}
