import { defineStore } from 'pinia'
import api from '@/utils/api'

const FEATURE_CACHE_KEY = 'app_feature_switch'

export const useAppFeatureStore = defineStore('appFeatureStore', () => {
    const isReviewMode = ref(false)
    const platform = ref('')
    const version = ref('')
    const isLoaded = ref(false)

    async function loadFeatureSwitch(force = false) {
        if (!force && isLoaded.value) {
            return
        }

        try {
            const res: any = await api.appFeature.getFeatureSwitch()
            console.log('[AppFeature] 接口返回:', res)
            if (res) {
                isReviewMode.value = res.isReviewMode ?? false
                platform.value = res.platform || ''
                version.value = res.version || ''
                console.log(
                    '[AppFeature] isReviewMode:',
                    isReviewMode.value,
                    'platform:',
                    platform.value,
                    'version:',
                    version.value
                )
                uni.setStorageSync(FEATURE_CACHE_KEY, res)
                isLoaded.value = true
            }
        } catch (err) {
            const cached = uni.getStorageSync(FEATURE_CACHE_KEY)
            if (cached) {
                isReviewMode.value = cached.isReviewMode ?? false
            }
            console.error('[AppFeature] 加载功能开关失败:', err)
        }
    }

    return {
        isReviewMode,
        platform,
        version,
        isLoaded,
        loadFeatureSwitch,
    }
})
