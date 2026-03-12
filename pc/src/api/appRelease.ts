import request from '@/utils/request'

interface CheckUpdateResponse {
    hasUpdate: boolean
    latestVersionCode: number
    latestVersionName: string
    description: string
    downloadUrl: string
    fileName: string
    fileSize: number
    isForceUpdate: boolean
    releaseDate: string
}

interface HistoryResponse {
    items: Array<{
        id: number
        versionName: string
        versionCode: number
        description: string
        fileName: string
        fileSize: number
        isForceUpdate: boolean
        platform: string
        releaseDate: string
        isActive: boolean
        downloadUrl: string
        creationTime: string
    }>
}

const appRelease = {
    checkUpdate: (currentVersionCode: number, platform: string = 'android') => {
        return request.get<any, CheckUpdateResponse>('/api/services/app/AppRelease/CheckUpdate', {
            params: { currentVersionCode, platform }
        })
    },

    getHistory: (platform: string = 'android') => {
        return request.get<any, HistoryResponse>('/api/services/app/AppRelease/GetReleaseHistory', {
            params: { platform }
        })
    }
}

export default appRelease