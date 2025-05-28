import { useRequest } from '@/utils/request'
const axios = useRequest()

/**
 * 分页获取数据列表
 * @returns
 */
export function GetList(params) {
    return axios({
        url: '/api/Post/GetList',
        method: 'get',
        params: params,
    })
}
/**
 * 分页获取数据列表
 * @returns
 */
export function GetAdminList(params) {
    return axios({
        url: '/api/Post/GetAdminList',
        method: 'get',
        params: params,
    })
}
/**
 * 获取数据详情
 * @returns
 */
export function GetPostDetail(id) {
    return axios({
        url: '/api/Post/PostDetail/' + id,
        method: 'get',
    })
}
/**
 * 发帖
 * @param params
 * @returns
 */
export function Add(params) {
    return axios({
        url: '/api/Post/Add',
        method: 'post',
        data: params,
    })
}
/**
 * 编辑数据
 * @param data
 * @returns
 */
export function Edit(data) {
    return axios({
        url: '/api/Post/Edit',
        method: 'post',
        data: data,
    })
}

/**
 * 删除数据
 * @param data
 * @returns
 */
export function Delete(id) {
    return axios({
        url: '/api/Post/Delete/' + id,
        method: 'get',
    })
}
/**
 * 设置置顶帖
 * @param data
 * @returns
 */
export function SetPostTop(id) {
    return axios({
        url: '/api/Post/SetTop/' + id,
        method: 'get',
    })
}
/**
 * 设置精华帖
 * @param data
 * @returns
 */
export function SetPostEssence(id) {
    return axios({
        url: '/api/Post/SetEssence/' + id,
        method: 'get',
    })
}
