import { useRequest } from '@/utils/request'
const axios = useRequest()

/**
 * 获取数据列表
 * @returns 
 */
export function GetList(params) {
    return axios({
        url: '/api/AdvertisingSpace/GetList',
        method: 'get',
        params:params
    })
}
/**
 * 根据类型获取数据
 * @returns 
 */
export function GetTypeList(type) {
    return axios({
        url: '/api/AdvertisingSpace/GetTypeList/'+type,
        method: 'get'
    })
}
/**
 * 添加数据
 * @param data 
 * @returns 
 */
export function Add(data) {
    return axios({
        url: '/api/AdvertisingSpace/Add',
        method: 'post',
        data:data
    })
}
/**
 * 编辑数据
 * @param data 
 * @returns 
 */
export function Edit(data) {
    return axios({
        url: '/api/AdvertisingSpace/Edit',
        method: 'post',
        data:data
    })
}

/**
 * 更新状态数据
 * @param data 
 * @returns 
 */
export function UpdateState(id,state) {
    return axios({
        url: '/api/AdvertisingSpace/UpdateState/'+id+"/"+state,
        method: 'get'
    })
}
/**
 * 删除数据
 * @param data 
 * @returns 
 */
export function Delete(id) {
    return axios({
        url: '/api/AdvertisingSpace/Delete/'+id,
        method: 'get'
    })
}
