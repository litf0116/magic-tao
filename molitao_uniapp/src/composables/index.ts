import dayjs from 'dayjs'
// dayjs.suppressDeprecationWarnings = true;
import 'dayjs/locale/zh-cn'
dayjs.locale('zh-cn') // 全局使用简体中文
import relativeTime from 'dayjs/plugin/relativeTime'
dayjs.extend(relativeTime)

export const isVideo = (value: string) => {
    if (value) {
        return value.match(/\.(mp4|avi|kmv|3gp|flv|wmv|m4v|mov|rmvb)$/gi)
    }
    return false
}

export const formatDate = (value: any, arg: string | undefined) => {
    if (value) {
        if (arg) {
            if (arg === 'fromNow') {
                return dayjs(String(value)).fromNow()
            }
            return dayjs(String(value)).format(arg)
        }
        return dayjs(String(value)).format('YYYY-MM-DD HH:mm')
    }
}

export const Tips = {
    info: (msg: string, duration = 3000) => {
        uni.showToast({
            title: `${msg}`,
            icon: 'none',
            duration: duration,
        })
    },

    success: (msg: string, duration = 2000) => {
        uni.showToast({
            title: `${msg}`,
            icon: 'success',
            duration: duration,
        })
    },

    error: (msg: string, duration = 2000) => {
        uni.showToast({
            title: `${msg}`,
            icon: 'error',
            duration: duration,
        })
    },

    noCancelModal: (msg: string, title = '') => {
        return new Promise((resolve) => {
            uni.showModal({
                title: title,
                showCancel: false,
                content: msg,
                success: function (rs) {
                    // console.log(rs)
                    return resolve('ok')
                },
            })
        })
    },

    OkConfirm: (msg: string, title = '', showCancel = false) => {
        return new Promise((resolve, reject) => {
            uni.showModal({
                title: title,
                content: msg,
                showCancel: showCancel,
                confirmText: '确认',
                success: function (res) {
                    if (res.confirm) {
                        // console.log('用户点击确定')
                        return resolve('ok')
                    } else if (res.cancel) {
                        // console.log('用户点击取消')
                        return reject('cancel')
                    }
                },
            })
        })
    },
    prompt: (content: string, title = '', placeholderText = '') => {
        return new Promise((resolve, reject) => {
            uni.showModal({
                title: title,
                content: content,
                editable: true,
                placeholderText: placeholderText,
                success: (res: any) => {
                    if (res.confirm) {
                        return resolve(res.content)
                    } else {
                        return reject('cancel')
                    }
                },
            })
        })
    },
}

let timeout: NodeJS.Timeout | null

export function debounce(func: (...args: any[]) => void, wait: number) {
    return function executedFunction(...args: any[]) {
        const later = () => {
            clearTimeout(timeout!)
            timeout = null
            func(...args)
        }
        clearTimeout(timeout!)
        timeout = setTimeout(later, wait)
    }
}

import { convertImageUrl } from '@/utils/imageUrlConverter'

export function getImgUrl(url?: string, thub = true) {
    const prefix = '!w300'
    if (!url) return ''

    if (!url.startsWith('http')) url = `${import.meta.env.VITE_APP_UPYUN_IMG_URL}${url}`

    // 应用URL转换
    url = convertImageUrl(url)

    if (url.endsWith(prefix)) {
        if (thub) return url
        else return url.replace(prefix, '')
    } else {
        if (thub) return url + prefix
        else return url
    }
}
