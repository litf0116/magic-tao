import { useRequest } from '@/utils/request'
const axios = useRequest()

/**
 * 分页获取数据列表
 * @returns
 */
export function GetList(params) {
    return axios({
        url: '/api/PostCategory/GetList',
        method: 'get',
        params: params,
    })
}
/**
 * 获取数据列表数据
 * @returns
 */
export function GetTypeList() {
    return axios({
        url: '/api/PostCategory/GetCategoryList',
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
        url: '/api/PostCategory/Add',
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
        url: '/api/PostCategory/Edit',
        method: 'post',
        data: data,
    })
}
/**
 * 更新状态数据
 * @param data
 * @returns
 */
export function UpdateState(id, state) {
    return axios({
        url: '/api/PostCategory/UpdateState/' + id + '/' + state,
        method: 'get',
    })
}

/**
 * 删除数据
 * @param data
 * @returns
 */
export function Delete(id) {
    return axios({
        url: '/api/PostCategory/Delete/' + id,
        method: 'get',
    })
}
