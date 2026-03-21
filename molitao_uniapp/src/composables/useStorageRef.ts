import { customRef } from 'vue'

export function useStorageRef<T>(key: string, defaultValue: T) {
    return customRef((track, trigger) => {
        return {
            get() {
                track()
                const stored = uni.getStorageSync(key)
                // 如果 storage 中没有值或值为空，返回默认值
                if (stored === '' || stored === null || stored === undefined) {
                    return defaultValue
                }
                return stored
            },
            set(newValue) {
                uni.setStorageSync(key, newValue)
                trigger()
            },
        }
    })
}
