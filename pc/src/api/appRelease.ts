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
    // 检查更新（用户端）
    checkUpdate: (currentVersionCode: number, platform = 'android') => {
        return request.get<any, CheckUpdateResponse>('/api/services/app/AppRelease/CheckUpdate', {
            params: { currentVersionCode, platform },
        })
    },

    // 获取历史版本
    getHistory: (platform = 'android') => {
        return request.get<any, HistoryResponse>('/api/services/app/AppRelease/GetReleaseHistory', {
            params: { platform },
        })
    },

    // 发布新版本
    publish: (data: {
        versionName: string
        versionCode: number
        description: string
        isForceUpdate: boolean
        platform: string
        file: File
    }) => {
        const formData = new FormData()
        formData.append('versionName', data.versionName)
        formData.append('versionCode', String(data.versionCode))
        formData.append('description', data.description)
        formData.append('isForceUpdate', String(data.isForceUpdate))
        formData.append('platform', data.platform)
        formData.append('file', data.file)
        return request.post<any, number>('/api/services/app/AppRelease/PublishAppRelease', formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
        })
    },

    // 删除版本
    delete: (id: number) => {
        return request.delete<any, void>(`/api/services/app/AppRelease/DeleteRelease?id=${id}`)
    },

    // 切换激活状态
    toggle: (id: number) => {
        return request.post<any, void>(`/api/services/app/AppRelease/ToggleRelease?id=${id}`)
    },
}

export default appRelease
