import request from '@/utils/request'

interface ReviewVersion {
    'mp-weixin': string
    'app-plus': string
    h5: string
}

const appFeature = {
    /**
     * 获取所有平台的审核版本
     */
    getAllReviewVersions: () => {
        return request.get<any, ReviewVersion>('/api/services/app/AppFeature/GetAllReviewVersions')
    },

    /**
     * 更新审核版本
     * @param platform 平台标识 (mp-weixin, app-plus, h5)
     * @param reviewVersion 审核版本号
     */
    updateReviewVersion: (platform: string, reviewVersion: string) => {
        return request.post<any, void>('/api/services/app/AppFeature/UpdateReviewVersion', {
            platform,
            reviewVersion,
        })
    },
}

export default appFeature
