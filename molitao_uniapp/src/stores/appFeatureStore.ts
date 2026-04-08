import { defineStore } from 'pinia'
import api from '@/utils/api'

const FEATURE_CACHE_KEY = 'app_feature_switch'

export const useAppFeatureStore = defineStore('appFeatureStore', () => {
    const features = ref<Record<string, boolean>>({})
    const platform = ref('')
    const version = ref('')
    const isLoaded = ref(false)

    async function loadFeatureSwitch() {
        if (isLoaded.value) {
            return
        }

        try {
            const res: any = await api.appFeature.getFeatureSwitch()
            if (res) {
                features.value = res.features ?? {}
                platform.value = res.platform || ''
                version.value = res.version || ''
                uni.setStorageSync(FEATURE_CACHE_KEY, res)
                isLoaded.value = true
            }
        } catch (err) {
            const cached = uni.getStorageSync(FEATURE_CACHE_KEY)
            if (cached && cached.features) {
                features.value = cached.features
            }
            console.error('[AppFeature] 加载功能开关失败:', err)
        }
    }

    function isFeatureEnabled(featureName: string): boolean {
        return features.value[featureName] ?? false
    }

    function getShowAuction(): boolean {
        return isFeatureEnabled('ShowAuction')
    }

    function getShowTradingPost(): boolean {
        return isFeatureEnabled('ShowTradingPost')
    }

    return {
        features,
        platform,
        version,
        isLoaded,
        loadFeatureSwitch,
        isFeatureEnabled,
        getShowAuction,
        getShowTradingPost,
    }
})