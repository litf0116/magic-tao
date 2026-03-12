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

type UpdateType = 'apk' | 'wgt'

class AppUpdateManager {
    private checking = false
    private downloading = false
    private downloadTask: any = null
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

    private getUpdateType(fileName: string): UpdateType {
        const ext = fileName.toLowerCase()
        if (ext.endsWith('.wgt') || ext.endsWith('.wgtu')) {
            return 'wgt'
        }
        return 'apk'
    }

    async downloadAndInstall(
        downloadUrl: string,
        fileName: string,
        isForceUpdate: boolean,
        onProgress: (progress: number) => void
    ): Promise<void> {
        if (this.downloading) {
            throw new Error('正在下载中')
        }

        this.downloading = true
        const updateType = this.getUpdateType(fileName)

        return new Promise((resolve, reject) => {
            this.downloadTask = plus.downloader.createDownload(downloadUrl)

            this.downloadTask.addEventListener('statechanged', (download: any, status: any) => {
                if (download.downloadedSize && download.totalSize) {
                    const progress = Math.round((download.downloadedSize / download.totalSize) * 100)
                    onProgress(progress)
                }

                if (download.state === 4) {
                    if (status === 200) {
                        const filePath = download.filename
                        this.install(filePath, updateType, isForceUpdate)
                            .then(() => {
                                this.downloading = false
                                this.downloadTask = null
                                resolve()
                            })
                            .catch((error: any) => {
                                this.downloading = false
                                this.downloadTask = null
                                reject(error)
                            })
                    } else {
                        this.downloading = false
                        this.downloadTask = null
                        reject(new Error(`下载失败: ${status}`))
                    }
                }
            })

            this.downloadTask.start()
        })
    }

    private install(filePath: string, updateType: UpdateType, isForceUpdate: boolean): Promise<void> {
        return new Promise((resolve, reject) => {
            const options: any = {
                force: isForceUpdate
            }

            plus.runtime.install(filePath, options, () => {
                console.log(`${updateType.toUpperCase()} 安装成功`)
                if (updateType === 'wgt') {
                    plus.runtime.restart()
                }
                resolve()
            }, (error: any) => {
                console.error(`${updateType.toUpperCase()} 安装失败`, error)
                reject(error)
            })
        })
    }

    cancelDownload(): void {
        if (this.downloadTask) {
            this.downloadTask.abort()
            this.downloadTask = null
            this.downloading = false
        }
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