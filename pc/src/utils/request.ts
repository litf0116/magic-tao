import axios from 'axios'
import { getToken } from './cookies'
import { ElMessage } from 'element-plus'

export const BASE_API_URL = (import.meta.env.VITE_APP_BASE_API || '/') + ''

const service = axios.create({
    baseURL: BASE_API_URL,
    timeout: 3000000,
})

//请求拦截器
service.interceptors.request.use(
    (request) => {
        request.headers['Abp.Tenantid'] = 1
        request.headers['Authorization'] = `Bearer ${getToken() || ''}`
        request.headers['Content-Type'] = 'application/json'
        // request.headers['.Aspnetcore-Culture'] = 'c=zh-Hans|uic=zh-CN'
        request.headers['Appname'] = import.meta.env.VITE_APP_AppName
        request.headers['AppVersion'] = import.meta.env.VITE_APP_VERSION || '20260224@1.0.0'
        return request
    },
    (error: any) => {
        Promise.reject(error)
    }
)
// 响应拦截器
service.interceptors.response.use(
    (response: any) => {
        if (isAbpResponse(response)) {
            return doAbpResponse(response).then(
                (abpRes) => {
                    return abpRes
                },
                (error) => {
                    return error
                }
            )
        } else {
            return response
        }
    },
    (err: {
        response: {
            data: {
                error: { validationErrors: any[]; details?: any; code?: number; message: any }
                unAuthorizedRequest?: boolean
            }
            status: number
        }
    }) => {
        console.log('%c http response error', 'color:red;')
        console.log(err)
        if (isAbpResponse(err.response)) {
            if (err.response.data.error.validationErrors && err.response.data.error.validationErrors.length > 0) {
                const message = []
                message.push(`<div class="ml-2">`)
                message.push(`<h4 class="mb-4 font-bold">表单未能通过验证</h4>`)
                message.push('<ul class="ml-4 list-disc">')
                err.response.data.error.validationErrors.forEach((errItem: any) =>
                    message.push(`<li class="leading-5">${errItem.message}</li>`)
                )
                message.push('</ul>')
                message.push(`</div>`)
                ElMessage({
                    dangerouslyUseHTMLString: true,
                    message: message.join(''),
                    type: 'error',
                })
            } else {
                if (err.response.data.unAuthorizedRequest || err.response.status === 401) {
                    Tips.confirm('请重新登录', '错误', 'error').then(() => {
                        location.reload()
                    })
                    return
                }

                // 处理403权限不足错误
                if (err.response.status === 403) {
                    Tips.confirm('权限不足，无法访问此资源', '错误', 'error').then(() => {
                        // 可以跳转到首页或其他合适的页面
                        location.href = '/'
                    })
                    return
                }

                if (err.response.data.error.code) {
                    Tips.confirm(err.response.data.error.message, '错误', 'error')
                } else ElMessage.error(err.response.data.error.message)
            }
            return Promise.reject(err.response.data.error)
        } else {
            // 对于非ABP响应，也检查状态码
            if (err.response && err.response.status === 401) {
                Tips.confirm('请重新登录', '错误', 'error').then(() => {
                    location.reload()
                })
                return
            } else if (err.response && err.response.status === 403) {
                Tips.confirm('权限不足，无法访问此资源', '错误', 'error').then(() => {
                    location.href = '/'
                })
                return
            }
            return Promise.reject(err)
        }
    }
)

function isAbpResponse(response: any) {
    return response && response.data && response.data.__abp
}

function doAbpResponse(response: any) {
    return new Promise((resolve, reject) => {
        if (response.data.success === true) {
            const _response = response
            _response.data = response.data.result
            resolve(_response)
        } else {
            //todo:处理ABP错误
            reject(response.data.error.message)
        }
    })
}

export default service

const baseConfig = {
    baseURL: BASE_API_URL,
    headers: {
        'Content-Type': 'application/json',
        'Register-Terminal': '20',
        'tenant-id': 1,
    },
}

/**
 * 发起http请求
 * @param config
 */
export function useRequest(config = baseConfig) {
    const request = axios.create(config)
    //请求拦截器
    request.interceptors.request.use(
        (request) => {
            request.headers['Abp.Tenantid'] = 1
            request.headers['Authorization'] = `Bearer ${getToken() || ''}`
            request.headers['Content-Type'] = 'application/json'
            // request.headers['.Aspnetcore-Culture'] = 'c=zh-Hans|uic=zh-CN'
            request.headers['Appname'] = import.meta.env.VITE_APP_AppName
            return request
        },
        (error: any) => {
            Promise.reject(error)
        }
    )
    //响应拦截器
    request.interceptors.response.use(
        (response: any) => {
            if (isAbpResponse(response)) {
                return doAbpResponse(response).then(
                    (abpRes) => {
                        return abpRes
                    },
                    (error) => {
                        return error
                    }
                )
            } else {
                return response
            }
        },
        (err: {
            response: {
                data: {
                    error: { validationErrors: any[]; details?: any; code?: number; message: any }
                    unAuthorizedRequest?: boolean
                }
                status: number
            }
        }) => {
            console.log('%c http response error', 'color:red;')
            console.log(err)
            if (isAbpResponse(err.response)) {
                if (err.response.data.error.validationErrors && err.response.data.error.validationErrors.length > 0) {
                    const message = []
                    message.push(`<div class="ml-2">`)
                    message.push(`<h4 class="mb-4 font-bold">表单未能通过验证</h4>`)
                    message.push('<ul class="ml-4 list-disc">')
                    err.response.data.error.validationErrors.forEach((errItem: any) =>
                        message.push(`<li class="leading-5">${errItem.message}</li>`)
                    )
                    message.push('</ul>')
                    message.push(`</div>`)
                    ElMessage({
                        dangerouslyUseHTMLString: true,
                        message: message.join(''),
                        type: 'error',
                    })
                } else {
                    if (err.response.data.unAuthorizedRequest || err.response.status === 401) {
                        Tips.confirm('请重新登录', '错误', 'error').then(() => {
                            location.reload()
                        })
                        return
                    }

                    // 处理403权限不足错误
                    if (err.response.status === 403) {
                        Tips.confirm('权限不足，无法访问此资源', '错误', 'error').then(() => {
                            location.href = '/'
                        })
                        return
                    }

                    if (err.response.data.error.code) {
                        Tips.confirm(err.response.data.error.message, '错误', 'error')
                    } else ElMessage.error(err.response.data.error.message)
                }
                return Promise.reject(err.response.data.error)
            } else {
                // 对于非ABP响应，也检查状态码
                if (err.response && err.response.status === 401) {
                    Tips.confirm('请重新登录', '错误', 'error').then(() => {
                        location.reload()
                    })
                    return
                } else if (err.response && err.response.status === 403) {
                    Tips.confirm('权限不足，无法访问此资源', '错误', 'error').then(() => {
                        location.href = '/'
                    })
                    return
                }
                return Promise.reject(err)
            }
        }
    )
    return request
}
