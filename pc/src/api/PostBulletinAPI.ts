import { useRequest } from '@/utils/request'
const axios = useRequest()

/**
 * 分页获取数据列表
 * @returns 
 */
export function GetList(params) {
    return axios({
        url: '/api/PostBulletin/GetList',
        method: 'get',
        params:params
    })
}
/**
 * 获取最新公告数据
 * @returns 
 */
export function GetLatestBulletin() {
    return axios({
        url: '/api/PostBulletin/GetLatestBulletin',
        method: 'get'
    })
}

/**
 * 添加数据
 * @param params 
 * @returns 
 */
export function Add(params) {
    return axios({
        url: '/api/PostBulletin/Add',
        method: 'post',
        data:params
    })
}
/**
 * 编辑数据
 * @param data 
 * @returns 
 */
export function Edit(data) {
    return axios({
        url: '/api/PostBulletin/Edit',
        method: 'post',
        data:data
    })
}


/**
 * 删除数据
 * @param data 
 * @returns 
 */
export function Delete(id) {
    return axios({
        url: '/api/PostBulletin/Delete/'+id,
        method: 'get'
    })
}