import { ref, watch } from 'vue'

export function useStorage(key: any, defaultValue = 0) {
    // 创建响应式引用
    const storageRef = ref(getStorageValue())

    // 获取存储值
    function getStorageValue() {
        try {
            const value = uni.getStorageSync(key)
            return value !== '' ? value : defaultValue
        } catch (e) {
            console.error(`读取${key}失败:`, e)
            return defaultValue
        }
    }

    // 设置存储值
    function setStorageValue(newVal: any) {
        try {
            uni.setStorageSync(key, newVal)
            storageRef.value = newVal
        } catch (e) {
            console.error(`写入${key}失败:`, e)
        }
    }

    // 监听值变化自动同步到 Storage
    watch(
        storageRef,
        (newVal) => {
            setStorageValue(newVal)
        },
        { deep: true }
    )

    // 移除存储
    function removeStorage() {
        try {
            uni.removeStorageSync(key)
            storageRef.value = defaultValue
        } catch (e) {
            console.error(`删除${key}失败:`, e)
        }
    }

    return {
        value: storageRef,
        setValue: setStorageValue,
        remove: removeStorage,
    }
}
