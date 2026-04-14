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

interface PublishByUrlInput {
    versionName: string
    versionCode: number
    description: string
    downloadUrl: string
    fileName: string
    fileSize: number
    isForceUpdate: boolean
    platform: string
}

const appRelease = {
    checkUpdate: (currentVersionCode: number, platform = 'android') => {
        return request.get<any, CheckUpdateResponse>('/api/services/app/AppRelease/CheckUpdate', {
            params: { currentVersionCode, platform },
        })
    },

    getHistory: (platform = 'android') => {
        return request.get<any, HistoryResponse>('/api/services/app/AppRelease/GetReleaseHistory', {
            params: { platform },
        })
    },

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

    publishByUrl: (data: PublishByUrlInput) => {
        return request.post<any, number>('/api/services/app/AppRelease/PublishAppReleaseByUrl', data)
    },

    delete: (id: number) => {
        return request.delete<any, void>(`/api/services/app/AppRelease/DeleteRelease?id=${id}`)
    },

    toggle: (id: number) => {
        return request.post<any, void>(`/api/services/app/AppRelease/ToggleRelease?id=${id}`)
    },
}

export default appRelease
