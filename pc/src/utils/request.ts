import axios from 'axios'
import { getToken } from './cookies'
import { ElMessage } from 'element-plus'

export const BASE_API_URL = (import.meta.env.VITE_APP_BASE_API || '/') + ''

// ==================== 统一响应类型定义 ====================

/**
 * ABP 标准响应格式
 */
export interface AbpResponse<T = any> {
    __abp?: boolean
    success: boolean
    result?: T
    error?: {
        code?: number
        message: string
        details?: any
        validationErrors?: { message: string; members?: string[] }[]
    }
    targetUrl?: string | null
    unAuthorizedRequest?: boolean
}

/**
 * 自定义简化响应格式（部分接口使用）
 */
export interface SimpleResponse<T = any> {
    success: boolean
    result?: T
    message?: string
    data?: T
}

/**
 * axios 响应结构
 */
export interface AxiosResponse<T = any> {
    data: T
    status?: number
    statusText?: string
    headers?: any
    config?: any
}

// ==================== 统一响应解析 ====================

/**
 * 判断是否为 ABP 标准响应
 * 格式: { __abp: true, success: true/false, result: ..., error: ... }
 */
function isAbpResponse(response: any): boolean {
    if (!response?.data) return false
    if (response.data.__abp === true) return true
    return false
}

/**
 * 判断是否为简化响应格式
 * 格式: { success: true/false, result: ... } 或 { success: true/false, data: ... }
 */
function isSimpleResponse(response: any): boolean {
    if (!response?.data) return false
    if (typeof response.data.success !== 'boolean') return false
    if (response.data.result !== undefined || response.data.data !== undefined) return true
    return false
}

/**
 * 从 ABP 响应中提取业务数据
 */
function extractAbpResult<T>(response: any): T | undefined {
    return response.data?.result as T | undefined
}

/**
 * 从简化响应中提取业务数据
 */
function extractSimpleResult<T>(response: any): T | undefined {
    // 只使用 result，与标准格式保持一致
    return response.data?.result as T | undefined
}

/**
 * 统一响应解析函数
 * 按优先级提取业务数据：
 * 1. ABP 标准格式 (__abp: true) → 提取 result
 * 2. 简化格式 (success + result/data) → 提取 result/data
 * 3. 普通格式 (data 存在) → 提取 data
 * 4. 其他 → 返回原始数据
 */
export function normalizeResponse<T = any>(response: any): T {
    if (isAbpResponse(response)) {
        return extractAbpResult<T>(response)
    }

    if (isSimpleResponse(response)) {
        return extractSimpleResult<T>(response)
    }

    if (response?.data !== undefined) {
        return response.data as T
    }

    return response as T
}

/**
 * 判断响应是否表示成功
 */
export function isSuccessResponse(response: any): boolean {
    if (!response?.data) return false
    if (isAbpResponse(response)) return response.data.success === true
    if (isSimpleResponse(response)) return response.data.success === true
    // 普通响应视为成功
    return true
}

/**
 * 获取错误信息
 */
export function getErrorMessage(response: any): string | undefined {
    if (!response?.data) return undefined

    // ABP 错误格式
    if (isAbpResponse(response)) {
        return response.data.error?.message
    }

    // 简化错误格式
    if (isSimpleResponse(response)) {
        return response.data.message
    }

    return undefined
}

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
        if (!isSuccessResponse(response)) {
            const errorMsg = getErrorMessage(response) || '操作失败'
            ElMessage.error(errorMsg)
            return Promise.reject(response.data?.error || response.data)
        }
        return normalizeResponse(response)
    },
    (err: any) => {
        console.log('%c http response error', 'color:red;')
        console.log(err)
        const errResponse = err.response

        if (errResponse?.data) {
            const errorData = errResponse.data
            const validationErrors = errorData?.error?.validationErrors
            const status = errResponse.status

            if (validationErrors && validationErrors.length > 0) {
                const message = []
                message.push(`<div class="ml-2">`)
                message.push(`<h4 class="mb-4 font-bold">表单未能通过验证</h4>`)
                message.push('<ul class="ml-4 list-disc">')
                validationErrors.forEach((errItem: any) =>
                    message.push(`<li class="leading-5">${errItem.message}</li>`)
                )
                message.push('</ul>')
                message.push(`</div>`)
                ElMessage({
                    dangerouslyUseHTMLString: true,
                    message: message.join(''),
                    type: 'error',
                })
            } else if (status === 401 || errorData.unAuthorizedRequest) {
                Tips.confirm('请重新登录', '错误', 'error').then(() => {
                    location.reload()
                })
            } else if (status === 403) {
                Tips.confirm('权限不足，无法访问此资源', '错误', 'error').then(() => {
                    location.href = '/'
                })
            } else {
                const errorMsg = errorData?.error?.message || errorData?.message || '请求失败'
                if (errorData?.error?.code) {
                    Tips.confirm(errorMsg, '错误', 'error')
                } else {
                    ElMessage.error(errorMsg)
                }
            }
            return Promise.reject(errorData.error || errorData)
        }

        ElMessage.error(err.message || '网络请求失败')
        return Promise.reject(err)
    }
)

export default service

const baseConfig = {
    baseURL: BASE_API_URL,
    headers: {
        'Content-Type': 'application/json',
        'Register-Terminal': '20',
        'tenant-id': 1,
    },
}

export function useRequest(config = baseConfig) {
    const request = axios.create(config)
    request.interceptors.request.use(
        (request) => {
            request.headers['Abp.Tenantid'] = 1
            request.headers['Authorization'] = `Bearer ${getToken() || ''}`
            request.headers['Content-Type'] = 'application/json'
            request.headers['Appname'] = import.meta.env.VITE_APP_AppName
            return request
        },
        (error: any) => {
            Promise.reject(error)
        }
    )
    request.interceptors.response.use(
        (response: any) => {
            if (!isSuccessResponse(response)) {
                const errorMsg = getErrorMessage(response) || '操作失败'
                ElMessage.error(errorMsg)
                return Promise.reject(response.data?.error || response.data)
            }
            return normalizeResponse(response)
        },
        (err: any) => {
            console.log('%c http response error', 'color:red;')
            console.log(err)
            const errResponse = err.response

            if (errResponse?.data) {
                const errorData = errResponse.data
                const validationErrors = errorData?.error?.validationErrors
                const status = errResponse.status

                if (validationErrors && validationErrors.length > 0) {
                    const message = []
                    message.push(`<div class="ml-2">`)
                    message.push(`<h4 class="mb-4 font-bold">表单未能通过验证</h4>`)
                    message.push('<ul class="ml-4 list-disc">')
                    validationErrors.forEach((errItem: any) =>
                        message.push(`<li class="leading-5">${errItem.message}</li>`)
                    )
                    message.push('</ul>')
                    message.push(`</div>`)
                    ElMessage({
                        dangerouslyUseHTMLString: true,
                        message: message.join(''),
                        type: 'error',
                    })
                } else if (status === 401 || errorData.unAuthorizedRequest) {
                    Tips.confirm('请重新登录', '错误', 'error').then(() => {
                        location.reload()
                    })
                } else if (status === 403) {
                    Tips.confirm('权限不足，无法访问此资源', '错误', 'error').then(() => {
                        location.href = '/'
                    })
                } else {
                    const errorMsg = errorData?.error?.message || errorData?.message || '请求失败'
                    if (errorData?.error?.code) {
                        Tips.confirm(errorMsg, '错误', 'error')
                    } else {
                        ElMessage.error(errorMsg)
                    }
                }
                return Promise.reject(errorData.error || errorData)
            }

            ElMessage.error(err.message || '网络请求失败')
            return Promise.reject(err)
        }
    )
    return request
}
