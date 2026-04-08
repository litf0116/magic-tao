import request from '@/utils/request'

const versionControl = {
    /**
     * 获取当前稳定版本号
     */
    getLatestStableVersion: () => {
        return request.get<any, string>('/api/services/app/VersionControl/GetLatestStableVersion')
    },

    /**
     * 更新稳定版本号
     * @param version 版本号 (格式: YYYYMMDD@主.次.补)
     */
    updateLatestStableVersion: (version: string) => {
        return request.post<any, void>('/api/services/app/VersionControl/UpdateLatestStableVersion', null, {
            params: { version },
        })
    },
}

export default versionControl
