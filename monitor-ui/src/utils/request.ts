import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios'
import { ElMessage } from 'element-plus'
import type { ApiResponse } from '@/types'

// 创建axios实例
const request: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || http://localhost:21021,
  timeout:100,
  headers: {
   Content-Type':application/json
  }
})

// 请求拦截器
request.interceptors.request.use(
  (config: AxiosRequestConfig) => {
    // 可以在这里添加token等认证信息
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// 响应拦截器
request.interceptors.response.use(
  (response: AxiosResponse<ApiResponse>) => {
    const { data } = response
    
    // 如果后端返回的是标准格式
    if (data && typeof data === 'object' && success in data) {
      if (data.success) {
        return data.data
      } else {
        ElMessage.error(data.message || '请求失败')
        return Promise.reject(new Error(data.message ||请求失败))     }
    }
    
    // 如果后端直接返回数据
    return data
  },
  (error) => {
    const message = error.response?.data?.message || error.message || '网络错误'
    ElMessage.error(message)
    return Promise.reject(error)
  }
)

export default request 