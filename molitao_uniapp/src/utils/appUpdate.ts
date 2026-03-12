import api from '@/utils/api'
import { getAppVersion } from '@/utils/version'

interface UpdateInfo {
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

class AppUpdateManager {
    private checking = false
    private downloading = false
    private platform: string = 'android'

    async checkUpdate(): Promise<UpdateInfo | null> {
        if (this.checking) {
            return null
        }

        this.checking = true

        try {
            const currentVersion = parseInt(getAppVersion()) || 1
            const result = await api.appRelease.checkUpdate(currentVersion, this.platform)
            return result.hasUpdate ? result : null
        } catch (error) {
            console.error('检查更新失败', error)
            return null
        } finally {
            this.checking = false
        }
    }

    async downloadAPK(downloadUrl: string, fileName: string, onProgress: (progress: number) => void): Promise<string> {
        if (this.downloading) {
            throw new Error('正在下载中')
        }

        this.downloading = true

        return new Promise((resolve, reject) => {
            const downloadTask = plus.downloader.createDownload(downloadUrl)

            downloadTask.addEventListener('statechanged', (download: any, status: any) => {
                if (download.downloadedSize && download.totalSize) {
                    const progress = Math.round((download.downloadedSize / download.totalSize) * 100)
                    onProgress(progress)
                }

                if (download.state === 4) {
                    if (status === 200) {
                        const filePath = download.filename
                        plus.runtime.install(filePath, {}, () => {
                            this.downloading = false
                            resolve(filePath)
                        }, (error: any) => {
                            this.downloading = false
                            reject(error)
                        })
                    } else {
                        this.downloading = false
                        reject(new Error(`下载失败: ${status}`))
                    }
                }
            })

            downloadTask.start()
        })
    }

    formatFileSize(bytes: number): string {
        if (bytes < 1024) {
            return bytes + ' B'
        } else if (bytes < 1024 * 1024) {
            return (bytes / 1024).toFixed(2) + ' KB'
        } else {
            return (bytes / (1024 * 1024)).toFixed(2) + ' MB'
        }
    }
}

export const appUpdateManager = new AppUpdateManager()