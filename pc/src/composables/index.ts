import { ElMessage, ElMessageBox } from 'element-plus'
import dayjs from 'dayjs'
import 'dayjs/locale/zh-cn'
import relativeTime from 'dayjs/plugin/relativeTime'
import { convertImageUrl } from '@/utils/imageUrlConverter'
dayjs.extend(relativeTime)
dayjs.locale('zh-cn')

export const Tips = {
    info: (msg: string, duration = 3000) => {
        ElMessage({
            // showClose: true,
            message: msg,
            type: 'info',
            duration,
        })
    },

    success: (msg: string, duration = 3000) => {
        ElMessage({
            // showClose: true,
            message: msg,
            type: 'success',
            duration,
        })
    },

    error: (msg: string, duration = 3000) => {
        ElMessage({
            // showClose: true,
            message: msg,
            type: 'error',
            duration,
        })
    },
    noCancelConfirm(msg: string, title = '警告', type: 'success' | 'info' | 'warning' | 'error' = 'warning') {
        return new Promise((resolve, reject) => {
            ElMessageBox.confirm(msg, title, {
                confirmButtonText: '确定',
                showCancelButton: false,
                type: type,
            })
                .then(() => {
                    return resolve(true)
                })
                .catch(() => {
                    return reject(false)
                })
        })
    },
    confirm: (msg: string, title = '警告', type: 'success' | 'info' | 'warning' | 'error' = 'warning') => {
        return new Promise((resolve, reject) => {
            ElMessageBox.confirm(msg, title, {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: type,
            })
                .then(() => {
                    return resolve(true)
                })
                .catch(() => {
                    return reject(false)
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

export function formatDate(value, arg) {
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

export function getImgUrl(url?: string, thub = true) {
    const prefix = '!w300'
    const cndUrl = import.meta.env.VITE_APP_UPYUN_IMG_URL
    if (!url) return ''
    if (url.startsWith('http')) {
        if (!url.startsWith(cndUrl)) return convertImageUrl(url)
        return url
    } else {
        url = `${cndUrl}${url}`
    }

    if (url.endsWith(prefix)) {
        if (thub) return url
        else return url.replace(prefix, '')
    } else {
        if (thub) return url + prefix
        else return url
    }
}

export function copyText(text: string) {
    const input = document.createElement('input')
    input.value = text
    document.body.appendChild(input)
    input.select()
    document.execCommand('copy')
    document.body.removeChild(input)
    Tips.success('复制成功')
}
