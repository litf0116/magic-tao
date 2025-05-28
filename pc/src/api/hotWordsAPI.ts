import { useRequest } from '@/utils/request'
const axios = useRequest()

/**
 * 分页获取数据列表
 * @returns
 */
export function GetAdminList(params) {
    return axios({
        url: '/api/HotWords/GetAdminList',
        method: 'get',
        params: params,
    })
}
/**
 * 获取数据列表数据
 * @returns
 */
export function GetHotWordsList(data) {
    return axios({
        url: '/api/HotWords/GetList',
        method: 'get',
        params: data,
    })
}
/**
 * 获取数据列详情
 * @returns
 */
export function Detail(id) {
    return axios({
        url: '/api/HotWords/Detail/' + id,
        method: 'get',
    })
}
/**
 * 添加数据
 * @param data
 * @returns
 */
export function Add(data) {
    return axios({
        url: '/api/HotWords/Add',
        method: 'post',
        data: data,
    })
}
/**
 * 编辑数据
 * @param data
 * @returns
 */
export function Edit(data) {
    return axios({
        url: '/api/HotWords/Edit',
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
        url: '/api/HotWords/Delete/' + id,
        method: 'get',
    })
}
