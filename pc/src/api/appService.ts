/** Generate by swagger-axios-codegen */
// @ts-nocheck
/* eslint-disable */

/** Generate by swagger-axios-codegen */
/* eslint-disable */
// @ts-nocheck
import axiosStatic, { AxiosInstance, AxiosRequestConfig } from 'axios'

export interface IRequestOptions extends AxiosRequestConfig {
    /** only in axios interceptor config*/
    loading?: boolean
    showError?: boolean
}

export interface IRequestConfig {
    method?: any
    headers?: any
    url?: any
    data?: any
    params?: any
}

// Add options interface
export interface ServiceOptions {
    axios?: AxiosInstance
    /** only in axios interceptor config*/
    loading: boolean
    showError: boolean
}

// Add default options
export const serviceOptions: ServiceOptions = {}

// Instance selector
export function axios(configs: IRequestConfig, resolve: (p: any) => void, reject: (p: any) => void): Promise<any> {
    if (serviceOptions.axios) {
        return serviceOptions.axios
            .request(configs)
            .then((res) => {
                resolve(res)
            })
            .catch((err) => {
                reject(err)
            })
    } else {
        throw new Error('please inject yourself instance like axios  ')
    }
}

export function getConfigs(method: string, contentType: string, url: string, options: any): IRequestConfig {
    const configs: IRequestConfig = {
        loading: serviceOptions.loading,
        showError: serviceOptions.showError,
        ...options,
        method,
        url,
    }
    configs.headers = {
        ...options.headers,
        'Content-Type': contentType,
    }
    return configs
}

export const basePath = ''

export interface IList<T> extends Array<T> { }
export interface List<T> extends Array<T> { }
export interface IDictionary<TValue> {
    [key: string]: TValue
}
export interface Dictionary<TValue> extends IDictionary<TValue> { }

export interface IListResult<T> {
    items?: T[]
}

export class ListResultDto<T> implements IListResult<T> {
    items?: T[]
}

export interface IPagedResult<T> extends IListResult<T> {
    totalCount?: number
    items?: T[]
}

export class PagedResultDto<T = any> implements IPagedResult<T> {
    totalCount?: number
    items?: T[]
}

export interface LoginBindingDto {
    loginProvider: string
    providerKey: string
    providerDisplayName: string
    bindTime?: string
}

// customer definition
// empty

export class AccountService {
    static getLoginBindings(options: IRequestOptions = {}): Promise<{ items: LoginBindingDto[] }> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Account/GetLoginBindings'
            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            axios(configs, resolve, reject)
        })
    }

    static bindPhone(
        params: { body?: { phoneNumber: string; code: string } } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Account/BindPhone'
            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)
            configs.data = params.body
            axios(configs, resolve, reject)
        })
    }

    static unbindLogin(
        params: { body?: { loginProvider: string; providerKey?: string } } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Account/UnbindLogin'
            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)
            configs.data = params.body
            axios(configs, resolve, reject)
        })
    }

    static isTenantAvailable(
        params: {
            /** requestBody */
            body?: IsTenantAvailableInput
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<IsTenantAvailableOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Account/IsTenantAvailable'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static register(
        params: {
            /** requestBody */
            body?: RegisterInput
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<RegisterOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Account/Register'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static updatePhone(options: IRequestOptions = {}): Promise<UserDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Account/UpdatePhone'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
}

export class AnnounceService {
    /**
     * 取得分类下最新的公告
     */
    static getLatest(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AnnounceDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Announce/GetLatest'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAllPublic(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AnnounceDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Announce/GetAllPublic'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getForEdit(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AnnounceCreateOrUpdateDtoGetForEditOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Announce/GetForEdit'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static get(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AnnounceDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Announce/Get'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAll(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AnnounceDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Announce/GetAll'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static update(
        params: {
            /** requestBody */
            body?: AnnounceCreateOrUpdateDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AnnounceDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Announce/Update'

            const configs: IRequestConfig = getConfigs('put', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static create(
        params: {
            /** requestBody */
            body?: AnnounceCreateOrUpdateDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AnnounceDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Announce/Create'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static delete(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Announce/Delete'

            const configs: IRequestConfig = getConfigs('delete', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
}

export class AppService {
    /**
     *
     */
    static getPublishList(options: IRequestOptions = {}): Promise<AppDtoListResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/AppManagement/App/GetPublishList'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static get(
        params: {
            /**  */
            id?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AppDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/AppManagement/App/Get'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAll(
        params: {
            /**  */
            sorting?: string
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AppDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/AppManagement/App/GetAll'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                Sorting: params['sorting'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static create(
        params: {
            /** requestBody */
            body?: AppCreateOrUpdateDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AppDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/AppManagement/App/Create'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static update(
        params: {
            /** requestBody */
            body?: AppCreateOrUpdateDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AppDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/AppManagement/App/Update'

            const configs: IRequestConfig = getConfigs('put', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static delete(
        params: {
            /**  */
            id?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/AppManagement/App/Delete'

            const configs: IRequestConfig = getConfigs('delete', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
}

export class AuctionItemService {
    /**
     *
     */
    static subStartNotify(
        params: {
            /** requestBody */
            body?: SubStartNotifyRequest
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/AuctionItem/SubStartNotify'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     * 出价
     */
    static bid(
        params: {
            /** requestBody */
            body?: BidHistoryCreateDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AuctionItemDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/AuctionItem/Bid'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static get(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AuctionItemDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/AuctionItem/Get'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static endAuction(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AuctionItemDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/AuctionItem/EndAuction'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static startAuction(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AuctionItemDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/AuctionItem/StartAuction'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     * 获取待拍卖跟已完成列表
     */
    static getPublicList(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AuctionItemDtoListResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/AuctionItem/GetPublicList'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     * 获取拍卖中列表
     */
    static GetAuctionMidList(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AuctionItemDtoListResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/AuctionItem/GetPublicList'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getMySuccessList(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AuctionItemDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/AuctionItem/GetMySuccessList'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAll(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AuctionItemDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/AuctionItem/GetAll'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static dateAnlayse(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/AuctionItem/DateAnlayse'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static dateAnlayse2(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/AuctionItem/DateAnlayse2'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getForEdit(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AuctionItemCreateOrUpdateDtoGetForEditOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/AuctionItem/GetForEdit'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static update(
        params: {
            /** requestBody */
            body?: AuctionItemCreateOrUpdateDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AuctionItemDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/AuctionItem/Update'

            const configs: IRequestConfig = getConfigs('put', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static create(
        params: {
            /** requestBody */
            body?: AuctionItemCreateOrUpdateDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AuctionItemDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/AuctionItem/Create'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static delete(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/AuctionItem/Delete'

            const configs: IRequestConfig = getConfigs('delete', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
}

export class AuditLogService {
    /**
     *
     */
    static getAuditLogs(
        params: {
            /**  */
            startDate?: string
            /**  */
            endDate?: string
            /**  */
            userName?: string
            /**  */
            serviceName?: string
            /**  */
            methodName?: string
            /**  */
            browserInfo?: string
            /**  */
            hasException?: boolean
            /**  */
            minExecutionDuration?: number
            /**  */
            maxExecutionDuration?: number
            /**  */
            sorting?: string
            /**  */
            maxResultCount?: number
            /**  */
            skipCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AuditLogListDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/AuditLog/GetAuditLogs'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                StartDate: params['startDate'],
                EndDate: params['endDate'],
                UserName: params['userName'],
                ServiceName: params['serviceName'],
                MethodName: params['methodName'],
                BrowserInfo: params['browserInfo'],
                HasException: params['hasException'],
                MinExecutionDuration: params['minExecutionDuration'],
                MaxExecutionDuration: params['maxExecutionDuration'],
                Sorting: params['sorting'],
                MaxResultCount: params['maxResultCount'],
                SkipCount: params['skipCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getEntityPropertyChanges(
        params: {
            /**  */
            entityChangeId?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<EntityPropertyChangeDto[]> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/AuditLog/GetEntityPropertyChanges'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { entityChangeId: params['entityChangeId'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
}

export class BanedUserService {
    /**
     *
     */
    static update(
        params: {
            /** requestBody */
            body?: BanedUserDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<BanedUserDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/BanedUser/Update'

            const configs: IRequestConfig = getConfigs('put', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static create(
        params: {
            /** requestBody */
            body?: BanedUserDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<BanedUserDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/BanedUser/Create'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getForEdit(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<BanedUserDtoGetForEditOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/BanedUser/GetForEdit'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static get(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<BanedUserDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/BanedUser/Get'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAll(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<BanedUserDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/BanedUser/GetAll'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static delete(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/BanedUser/Delete'

            const configs: IRequestConfig = getConfigs('delete', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
}

export class BidHistoryService {
    /**
     *
     */
    static update(
        params: {
            /** requestBody */
            body?: BidHistoryDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<BidHistoryDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/BidHistory/Update'

            const configs: IRequestConfig = getConfigs('put', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static create(
        params: {
            /** requestBody */
            body?: BidHistoryDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<BidHistoryDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/BidHistory/Create'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static delete(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/BidHistory/Delete'

            const configs: IRequestConfig = getConfigs('delete', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static dateAnlayse(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/BidHistory/DateAnlayse'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getForEdit(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<BidHistoryDtoGetForEditOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/BidHistory/GetForEdit'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static get(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<BidHistoryDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/BidHistory/Get'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAll(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<BidHistoryDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/BidHistory/GetAll'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
}

export class ChatEmojiService {
    /**
     *
     */
    static update(
        params: {
            /** requestBody */
            body?: ChatEmojiDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<ChatEmojiDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/ChatEmoji/Update'

            const configs: IRequestConfig = getConfigs('put', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAll(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<ChatEmojiDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/ChatEmoji/GetAll'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static create(
        params: {
            /** requestBody */
            body?: ChatEmojiDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<ChatEmojiDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/ChatEmoji/Create'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static delete(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/ChatEmoji/Delete'

            const configs: IRequestConfig = getConfigs('delete', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getForEdit(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<ChatEmojiDtoGetForEditOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/ChatEmoji/GetForEdit'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static get(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<ChatEmojiDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/ChatEmoji/Get'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
}

export class ChatGroupService {
    /**
     *
     */
    static toggleHidden(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<ChatGroupDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/ChatGroup/ToggleHidden'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getGroupUser(
        params: {
            /**  */
            chan?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserDtoListResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/ChatGroup/GetGroupUser'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { chan: params['chan'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static get(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<ChatGroupDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/ChatGroup/Get'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static create(
        params: {
            /** requestBody */
            body?: ChatGroupCreateOrUpdateDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<ChatGroupDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/ChatGroup/Create'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     * 删除组队频道
     */
    static delete(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/ChatGroup/Delete'

            const configs: IRequestConfig = getConfigs('delete', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     * 踢出用户
     */
    static kickUser(
        params: {
            /**  */
            id?: number
            /**  */
            userId?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/ChatGroup/KickUser'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { id: params['id'], userId: params['userId'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAllPublic(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<ChatGroupDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/ChatGroup/GetAllPublic'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getForEdit(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<ChatGroupCreateOrUpdateDtoGetForEditOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/ChatGroup/GetForEdit'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAll(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<ChatGroupDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/ChatGroup/GetAll'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static update(
        params: {
            /** requestBody */
            body?: ChatGroupCreateOrUpdateDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<ChatGroupDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/ChatGroup/Update'

            const configs: IRequestConfig = getConfigs('put', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
}

export class ClientService {
    /**
     * 诚信履约金支付
     */
    static payDeposit(
        params: {
            /**  */
            openid?: string
            /**  */
            type?: string
            /** 支付金额，如果不指定则使用默认诚信履约金金额 */
            amount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Client/PayDeposit'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { openid: params['openid'], type: params['type'], amount: params['amount'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     * 删除与某人的聊天记录
     */
    static deleteChatList(
        params: {
            /** 对方用户ID */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Client/DeleteChatList'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getChatList(options: IRequestOptions = {}): Promise<ChatListItem[]> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Client/GetChatList'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     * 用户充值
     */
    static topUp(
        params: {
            /**  */
            openid?: string
            /**  */
            amount?: number
            /**  */
            type?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Client/TopUp'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { openid: params['openid'], amount: params['amount'], type: params['type'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getMyCount(options: IRequestOptions = {}): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Client/GetMyCount'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
}

export class CmsArticleService {
    /**
     *
     */
    static getAllPublic(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<CmsArticleDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/CmsArticle/GetAllPublic'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getForEdit(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<CmsArticleCreateOrUpdateDtoGetForEditOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/CmsArticle/GetForEdit'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static get(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<CmsArticleDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/CmsArticle/Get'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAll(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<CmsArticleDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/CmsArticle/GetAll'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static update(
        params: {
            /** requestBody */
            body?: CmsArticleCreateOrUpdateDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<CmsArticleDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/CmsArticle/Update'

            const configs: IRequestConfig = getConfigs('put', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static create(
        params: {
            /** requestBody */
            body?: CmsArticleCreateOrUpdateDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<CmsArticleDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/CmsArticle/Create'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static delete(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/CmsArticle/Delete'

            const configs: IRequestConfig = getConfigs('delete', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
}

export class CmsCategoryService {
    /**
     *
     */
    static getForEdit(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<CmsArticleCreateOrUpdateDtoGetForEditOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/CmsCategory/GetForEdit'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static get(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<CmsCategoryDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/CmsCategory/Get'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAll(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<CmsCategoryDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/CmsCategory/GetAll'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static update(
        params: {
            /** requestBody */
            body?: CmsArticleCreateOrUpdateDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<CmsCategoryDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/CmsCategory/Update'

            const configs: IRequestConfig = getConfigs('put', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static create(
        params: {
            /** requestBody */
            body?: CmsArticleCreateOrUpdateDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<CmsCategoryDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/CmsCategory/Create'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static delete(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/CmsCategory/Delete'

            const configs: IRequestConfig = getConfigs('delete', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
}

export class MessageService {
    /**
     *
     */
    static getChanHistory(
        params: {
            /**  */
            chan?: string
            /**  */
            lastTime?: number
            /**  */
            size?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<ChatMessageListResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Message/GetChanHistory'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { chan: params['chan'], lastTime: params['lastTime'], size: params['size'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getChanLastId(
        params: {
            /**  */
            chan?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<string> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Message/GetChanLastId'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { chan: params['chan'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getPrivateHistory(
        params: {
            /**  */
            id?: number
            /**  */
            lastTime?: number
            /**  */
            size?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<ChatMessageListResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Message/GetPrivateHistory'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { id: params['id'], lastTime: params['lastTime'], size: params['size'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getPrivateLastId(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<string> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Message/GetPrivateLastId'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
}

export class RoleService {
    /**
     *
     */
    static create(
        params: {
            /** requestBody */
            body?: CreateRoleDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<RoleDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Role/Create'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getRoles(
        params: {
            /**  */
            permission?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<RoleListDtoListResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Role/GetRoles'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Permission: params['permission'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static update(
        params: {
            /** requestBody */
            body?: RoleDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<RoleDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Role/Update'

            const configs: IRequestConfig = getConfigs('put', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static delete(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Role/Delete'

            const configs: IRequestConfig = getConfigs('delete', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAllPermissions(options: IRequestOptions = {}): Promise<PermissionDtoListResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Role/GetAllPermissions'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getRoleForEdit(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<GetRoleForEditOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Role/GetRoleForEdit'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static get(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<RoleDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Role/Get'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAll(
        params: {
            /**  */
            keyword?: string
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<RoleDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Role/GetAll'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                Keyword: params['keyword'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
}

export class SensitiveWordService {
    /**
     *
     */
    static batchCreate(
        params: {
            /** requestBody */
            body?: BatchCreateRequest
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/SensitiveWord/BatchCreate'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static reBuildCache(options: IRequestOptions = {}): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/SensitiveWord/ReBuildCache'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getForEdit(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<SensitiveWordDtoGetForEditOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/SensitiveWord/GetForEdit'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static get(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<SensitiveWordDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/SensitiveWord/Get'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAll(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<SensitiveWordDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/SensitiveWord/GetAll'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static update(
        params: {
            /** requestBody */
            body?: SensitiveWordDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<SensitiveWordDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/SensitiveWord/Update'

            const configs: IRequestConfig = getConfigs('put', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static create(
        params: {
            /** requestBody */
            body?: SensitiveWordDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<SensitiveWordDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/SensitiveWord/Create'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static delete(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/SensitiveWord/Delete'

            const configs: IRequestConfig = getConfigs('delete', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
}

export class SessionService {
    /**
     *
     */
    static getCurrentLoginInformations(options: IRequestOptions = {}): Promise<GetCurrentLoginInformationsOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Session/GetCurrentLoginInformations'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
}

export class TenantService {
    /**
     *
     */
    static create(
        params: {
            /** requestBody */
            body?: CreateTenantDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<TenantDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Tenant/Create'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static delete(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Tenant/Delete'

            const configs: IRequestConfig = getConfigs('delete', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static get(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<TenantDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Tenant/Get'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAll(
        params: {
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<TenantDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Tenant/GetAll'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static update(
        params: {
            /** requestBody */
            body?: TenantDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<TenantDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Tenant/Update'

            const configs: IRequestConfig = getConfigs('put', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
}

export class TokenAuthService {
    /**
     *
     */
    static qrToken(
        params: {
            /**  */
            key?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<string> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/TokenAuth/QrToken'

            const configs: IRequestConfig = getConfigs('get', 'text/plain', url, options)
            configs.params = { key: params['key'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static pubQrLogin(
        params: {
            /**  */
            state?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<string> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/TokenAuth/PubQrLogin'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { state: params['state'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static qrLogin(
        params: {
            /**  */
            code?: string
            /**  */
            state?: string
            /**  */
            tenantId?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/TokenAuth/QrLogin'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { code: params['code'], state: params['state'], tenantId: params['tenantId'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static authenticate(
        params: {
            /** requestBody */
            body?: AuthenticateModel
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<AuthenticateResultModel> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/TokenAuth/Authenticate'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static refreshToken(
        params: {
            /**  */
            refreshToken?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<RefreshTokenResult> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/TokenAuth/RefreshToken'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)
            configs.params = { refreshToken: params['refreshToken'] }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static logOut(options: IRequestOptions = {}): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/TokenAuth/LogOut'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static weixinMiniAuthenticate(
        params: {
            /** requestBody */
            body?: WeChatMiniProgramAuthenticateModel
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<ExternalAuthenticateResultModel> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/TokenAuth/WeixinMiniAuthenticate'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static weixinMiniPhoneAuthenticate(
        params: {
            /** requestBody */
            body?: WeChatMiniProgramAuthenticateModel
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<ExternalAuthenticateResultModel> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/TokenAuth/WeixinMiniPhoneAuthenticate'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }

    static sendSmsCode(
        params: {
            /** requestBody */
            body?: { phoneNumber: string; purpose?: string }
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/TokenAuth/SendSmsCode'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }

    static phoneAuthenticate(
        params: {
            /** requestBody */
            body?: { phoneNumber: string; code: string }
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<ExternalAuthenticateResultModel> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/TokenAuth/PhoneAuthenticate'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
}

export class UploadService {
    /**
     *
     */
    static getSignature(
        params: {
            /**  */
            data?: string
            /**  */
            policy?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/Upload/GetSignature'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { data: params['data'], policy: params['policy'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
}

export class UserService {
    /**
     *
     */
    static get(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/User/Get'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static create(
        params: {
            /** requestBody */
            body?: CreateUserDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/User/Create'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getCurrentUser(options: IRequestOptions = {}): Promise<GetUserForEditOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/User/GetCurrentUser'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getUserForEdit(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<GetUserForEditOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/User/GetUserForEdit'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static createOrUpdateUser(
        params: {
            /** requestBody */
            body?: CreateOrUpdateUserInput
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/User/CreateOrUpdateUser'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     * 个人修改资料
     */
    static update(
        params: {
            /** requestBody */
            body?: UserEditDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/User/Update'

            const configs: IRequestConfig = getConfigs('put', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static delete(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/User/Delete'

            const configs: IRequestConfig = getConfigs('delete', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getUsersInRole(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserDtoListResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/User/GetUsersInRole'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getRoles(options: IRequestOptions = {}): Promise<RoleDtoListResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/User/GetRoles'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static changeLanguage(
        params: {
            /** requestBody */
            body?: ChangeUserLanguageDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/User/ChangeLanguage'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static changePassword(
        params: {
            /** requestBody */
            body?: ChangePasswordDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<boolean> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/User/ChangePassword'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static resetPassword(
        params: {
            /** requestBody */
            body?: ResetPasswordDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<boolean> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/User/ResetPassword'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getForEdit(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<CreateUserDtoGetForEditOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/User/GetForEdit'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAll(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/User/GetAll'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     * 跳过完善个人信息引导
     */
    static skipProfileCompletion(options: IRequestOptions = {}): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/User/SkipProfileCompletion'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            axios(configs, resolve, reject)
        })
    }
    /**
     * 完善个人信息
     */
    static completeProfile(
        params: {
            /** requestBody */
            body?: CompleteProfileInput
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/User/CompleteProfile'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
}

export class UserBalanceLogService {
    /**
     *
     */
    static getMyAll(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserBalanceLogDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/UserBalanceLog/GetMyAll'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAll(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserBalanceLogDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/UserBalanceLog/GetAll'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getForEdit(
        params: {
            /**  */
            id?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserBalanceLogDtoGetForEditOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/UserBalanceLog/GetForEdit'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static get(
        params: {
            /**  */
            id?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserBalanceLogDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/UserBalanceLog/Get'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static update(
        params: {
            /** requestBody */
            body?: UserBalanceLogDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserBalanceLogDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/UserBalanceLog/Update'

            const configs: IRequestConfig = getConfigs('put', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static create(
        params: {
            /** requestBody */
            body?: UserBalanceLogDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserBalanceLogDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/UserBalanceLog/Create'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static delete(
        params: {
            /**  */
            id?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/UserBalanceLog/Delete'

            const configs: IRequestConfig = getConfigs('delete', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
}

export class UserDepositLogService {
    /**
     *
     */
    static getMyAll(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserDepositLogDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/UserDepositLog/GetMyAll'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getAll(
        params: {
            /**  */
            appName?: string
            /**  */
            type?: number
            /**  */
            self?: boolean
            /**  */
            organizationUnitId?: number
            /**  */
            shopId?: number
            /**  */
            status?: number
            /**  */
            userId?: number
            /**  */
            pid?: number
            /**  */
            gPid?: string
            /**  */
            keyword?: string
            /**  */
            isActive?: boolean
            /**  */
            sorting?: string
            /**  */
            from?: string
            /**  */
            to?: string
            /**  */
            publicHidden?: boolean
            /**  */
            skipCount?: number
            /**  */
            maxResultCount?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserDepositLogDtoPagedResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/UserDepositLog/GetAll'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                AppName: params['appName'],
                Type: params['type'],
                Self: params['self'],
                OrganizationUnitId: params['organizationUnitId'],
                ShopId: params['shopId'],
                Status: params['status'],
                UserId: params['userId'],
                Pid: params['pid'],
                GPid: params['gPid'],
                Keyword: params['keyword'],
                IsActive: params['isActive'],
                Sorting: params['sorting'],
                From: params['from'],
                To: params['to'],
                PublicHidden: params['publicHidden'],
                SkipCount: params['skipCount'],
                MaxResultCount: params['maxResultCount'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getForEdit(
        params: {
            /**  */
            id?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserDepositLogDtoGetForEditOutput> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/UserDepositLog/GetForEdit'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static get(
        params: {
            /**  */
            id?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserDepositLogDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/UserDepositLog/Get'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static update(
        params: {
            /** requestBody */
            body?: UserDepositLogDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserDepositLogDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/UserDepositLog/Update'

            const configs: IRequestConfig = getConfigs('put', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static create(
        params: {
            /** requestBody */
            body?: UserDepositLogDto
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserDepositLogDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/UserDepositLog/Create'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static delete(
        params: {
            /**  */
            id?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/UserDepositLog/Delete'

            const configs: IRequestConfig = getConfigs('delete', 'application/json', url, options)
            configs.params = { Id: params['id'] }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
}

export class UserFriendService {
    /**
     *
     */
    static addFriend(
        params: {
            /**  */
            id?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/UserFriend/AddFriend'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { id: params['id'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static getUserFriends(
        params: {
            /**  */
            id?: number
            /**  */
            status?: boolean
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<UserDtoBaseListResultDto> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/UserFriend/GetUserFriends'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { id: params['id'], status: params['status'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static agree(
        params: {
            /**  */
            id?: number
            /**  */
            status?: boolean
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/UserFriend/Agree'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { id: params['id'], status: params['status'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
}

export class WebSocketService {
    /**
     * 获取websocket分区
     */
    static preConnect(options: IRequestOptions = {}): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/ws/pre-connect'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static offline(
        params: {
            /**  */
            websocketId?: number
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/ws/offline'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { websocketId: params['websocketId'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     * 群聊，获取群列表
     */
    static getChannels(options: IRequestOptions = {}): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/ws/get-channels'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     * 群聊，绑定消息频道
     */
    static subChannel(
        params: {
            /** requestBody */
            body?: SubscrChannelInput
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/ws/sub-channel'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static leaveChannel(
        params: {
            /**  */
            chan?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/ws/leave-channel'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { chan: params['chan'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static delChannel(
        params: {
            /**  */
            chan?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/ws/del-channel'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)
            configs.params = { chan: params['chan'] }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     * 撤回消息
     */
    static backout(
        params: {
            /** requestBody */
            body?: ChatMessage
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/ws/backout'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     * 用户禁言
     */
    static banUser(
        params: {
            /** requestBody */
            body?: BanUserInput
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/ws/ban-user'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     * 群聊，发送频道消息，绑定频道的所有人将收到消息
     */
    static sendChannelMsg(
        params: {
            /** requestBody */
            body?: SendChangeMsgInput
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/ws/SendChannelMsg'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     * 单聊
     */
    static sendMsg(
        params: {
            /** requestBody */
            body?: SendMsgInput
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/ws/send-msg'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = params.body

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
}

export class WeChatPayService {
    /**
     * JS-SDK支付回调地址（在统一下单接口中设置notify_url）
     */
    static tenPay(
        params: {
            /**  */
            appName: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/PayNotify/TenPay/{appName}'
            url = url.replace('{appName}', params['appName'] + '')

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static test2(
        params: {
            /**  */
            openid?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/PayNotify/test2'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = { openid: params['openid'] }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
}

export class WxService {
    /**
     *
     */
    static wx(
        params: {
            /**  */
            signature?: string
            /**  */
            timestamp?: string
            /**  */
            nonce?: string
            /**  */
            echostr?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<string> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/wx'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)
            configs.params = {
                signature: params['signature'],
                timestamp: params['timestamp'],
                nonce: params['nonce'],
                echostr: params['echostr'],
            }

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
    /**
     *
     */
    static wx1(
        params: {
            /**  */
            signature?: string
            /**  */
            timestamp?: string
            /**  */
            nonce?: string
            /**  */
            openid?: string
            /**  */
            encryptType?: string
            /**  */
            msgSignature?: string
        } = {} as any,
        options: IRequestOptions = {}
    ): Promise<any> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/wx'

            const configs: IRequestConfig = getConfigs('post', 'application/json', url, options)
            configs.params = {
                signature: params['signature'],
                timestamp: params['timestamp'],
                nonce: params['nonce'],
                openid: params['openid'],
                encrypt_type: params['encryptType'],
                msg_signature: params['msgSignature'],
            }

            let data = null

            configs.data = data

            axios(configs, resolve, reject)
        })
    }
}

export class WxUserInfoService {
    /**
     *
     */
    static getWechatUserinfos(options: IRequestOptions = {}): Promise<WechatUserinfo[]> {
        return new Promise((resolve, reject) => {
            let url = basePath + '/api/services/app/WxUserInfo/GetWechatUserinfos'

            const configs: IRequestConfig = getConfigs('get', 'application/json', url, options)

            /** 适配ios13，get请求不允许带body */

            axios(configs, resolve, reject)
        })
    }
}

export interface AnnounceCreateOrUpdateDto {
    /**  */
    categoryId?: number

    /**  */
    content?: string

    /**  */
    imageUrl?: string

    /**  */
    sort?: number

    /**  */
    id?: number
}

export interface AnnounceCreateOrUpdateDtoGetForEditOutput {
    /**  */
    data?: AnnounceCreateOrUpdateDto

    /**  */
    schema?: any | null
}

export interface AnnounceDto {
    /**  */
    categoryId?: number

    /**  */
    content?: string

    /**  */
    imageUrl?: string

    /**  */
    sort?: number

    /**  */
    creationTime?: Date

    /**  */
    creatorUserId?: number

    /**  */
    id?: number
}

export interface AnnounceDtoPagedResultDto {
    /**  */
    totalCount?: number

    /**  */
    items?: AnnounceDto[]
}

export interface AppCreateOrUpdateDto {
    /**  */
    name?: string

    /**  */
    clientName?: string

    /**  */
    providerName?: string

    /**  */
    providerKey?: string

    /**  */
    value?: object

    /**  */
    id?: string
}

export interface AppDto {
    /**  */
    name?: string

    /**  */
    clientName?: string

    /**  */
    clientType?: string

    /**  */
    value?: object

    /**  */
    providerName?: string

    /**  */
    providerKey?: string

    /**  */
    id?: string
}

export interface AppDtoListResultDto {
    /**  */
    items?: AppDto[]
}

export interface AppDtoPagedResultDto {
    /**  */
    totalCount?: number

    /**  */
    items?: AppDto[]
}

export interface ApplicationInfoDto {
    /**  */
    name?: string

    /**  */
    version?: string

    /**  */
    releaseDate?: Date

    /**  */
    features?: object
}

export interface AuctionItemCreateOrUpdateDto {
    /**  */
    name?: string

    /**  */
    status?: number

    /**  */
    imageUrl?: string

    /**  */
    description?: string

    /**  */
    startingPrice?: number

    /**  */
    sellerInfo?: string

    /**  */
    order?: number

    /**  */
    sellerId?: number

    /**  */
    id?: number
}

export interface AuctionItemCreateOrUpdateDtoGetForEditOutput {
    /**  */
    data?: AuctionItemCreateOrUpdateDto

    /**  */
    schema?: any | null
}

export interface AuctionItemDto {
    /**  */
    name?: string

    /**  */
    status?: AuctionStatusEnum

    /**  */
    imageUrl?: string

    /**  */
    description?: string

    /**  */
    startingPrice?: number

    /**  */
    currentPrice?: number

    /**  */
    currentPriceUserId?: number

    /**  */
    currentPriceUserName?: string

    /**  */
    finalPrice?: number

    /**  */
    dealTime?: Date

    /**  */
    dealUserId?: number

    /**  */
    dealUserName?: string

    /**  */
    sellerInfo?: string

    /**  */
    sellerId?: number

    /**  */
    order?: number

    /**  */
    toUserMsg?: string

    /**  */
    dealUserAvatar?: string

    /**  */
    id?: number

    /**  */
    isKasec?: boolean

    /** 显示序号（空降商品为空字符串） */
    displayIndex?: string | number
}

export interface AuctionItemDtoListResultDto {
    /**  */
    items?: AuctionItemDto[]
}

export interface AuctionItemDtoPagedResultDto {
    /**  */
    totalCount?: number

    /**  */
    items?: AuctionItemDto[]
}

export interface AuditLogListDto {
    /**  */
    userId?: number

    /**  */
    userName?: string

    /**  */
    impersonatorTenantId?: number

    /**  */
    impersonatorUserId?: number

    /**  */
    serviceName?: string

    /**  */
    methodName?: string

    /**  */
    parameters?: string

    /**  */
    executionTime?: Date

    /**  */
    executionDuration?: number

    /**  */
    clientIpAddress?: string

    /**  */
    clientName?: string

    /**  */
    browserInfo?: string

    /**  */
    exception?: string

    /**  */
    customData?: string

    /**  */
    id?: number
}

export interface AuditLogListDtoPagedResultDto {
    /**  */
    totalCount?: number

    /**  */
    items?: AuditLogListDto[]
}

export interface AuthenticateModel {
    /**  */
    userNameOrEmailAddress?: string

    /**  */
    password?: string

    /**  */
    rememberClient?: boolean

    /**  */
    singleSignIn?: boolean

    /**  */
    returnUrl?: string

    /**  */
    captchaResponse?: string
}

export interface AuthenticateResultModel {
    /**  */
    accessToken?: string

    /**  */
    encryptedAccessToken?: string

    /**  */
    expireInSeconds?: number

    /**  */
    shouldResetPassword?: boolean

    /**  */
    passwordResetCode?: string

    /**  */
    userId?: number

    /**  */
    requiresTwoFactorVerification?: boolean

    /**  */
    twoFactorAuthProviders?: string[]

    /**  */
    twoFactorRememberClientToken?: string

    /**  */
    returnUrl?: string

    /**  */
    refreshToken?: string

    /**  */
    refreshTokenExpireInSeconds?: number
}

export interface BanUserInput {
    /**  */
    userId?: number

    /**  */
    minutes?: number

    /**  */
    chan?: string
}

export interface BanedUserDto {
    /**  */
    endTime?: Date

    /**  */
    userId?: number

    /**  */
    chan?: string

    /**  */
    creationTime?: Date

    /**  */
    creatorUserId?: number

    /**  */
    creatorUser?: UserDtoBase

    /**  */
    user?: UserDtoBase

    /**  */
    id?: number
}

export interface BanedUserDtoGetForEditOutput {
    /**  */
    data?: BanedUserDto

    /**  */
    schema?: any | null
}

export interface BanedUserDtoPagedResultDto {
    /**  */
    totalCount?: number

    /**  */
    items?: BanedUserDto[]
}

export interface BatchCreateRequest {
    /**  */
    words?: string
}

export interface BidHistoryCreateDto {
    /**  */
    auctionItemId?: number

    /**  */
    bidPrice?: number

    /**  */
    bidUserName?: string

    /**  */
    bidUserAvatar?: string

    /**  */
    bidTime?: Date

    /**  */
    id?: number
}

export interface BidHistoryDto {
    /**  */
    auctionItemId?: number

    /**  */
    bidPrice?: number

    /**  */
    bidTime?: Date

    /**  */
    bidUserName?: string

    /**  */
    bidUserAvatar?: string

    /**  */
    id?: number
}

export interface BidHistoryDtoGetForEditOutput {
    /**  */
    data?: BidHistoryDto

    /**  */
    schema?: any | null
}

export interface BidHistoryDtoPagedResultDto {
    /**  */
    totalCount?: number

    /**  */
    items?: BidHistoryDto[]
}

export interface ChangePasswordDto {
    /**  */
    currentPassword?: string

    /**  */
    newPassword?: string
}

export interface ChangeUserLanguageDto {
    /**  */
    languageName?: string
}

export interface ChatEmojiDto {
    /**  */
    url?: string

    /**  */
    payload?: string

    /**  */
    id?: number
}

export interface ChatEmojiDtoGetForEditOutput {
    /**  */
    data?: ChatEmojiDto

    /**  */
    schema?: any | null
}

export interface ChatEmojiDtoPagedResultDto {
    /**  */
    totalCount?: number

    /**  */
    items?: ChatEmojiDto[]
}

export interface ChatGroupCreateOrUpdateDto {
    /**  */
    title?: string

    /**  */
    limit?: number

    /**  */
    id?: number
}

export interface ChatGroupCreateOrUpdateDtoGetForEditOutput {
    /**  */
    data?: ChatGroupCreateOrUpdateDto

    /**  */
    schema?: any | null
}

export interface ChatGroupDto {
    /**  */
    title?: string

    /**  */
    limit?: number

    /**  */
    isHidden?: boolean

    /**  */
    creatorUser?: UserDtoBase

    /**  */
    creatorUserId?: number

    /**  */
    chan?: string

    /**  */
    creationTime?: Date

    /**  */
    id?: number
}

export interface ChatGroupDtoPagedResultDto {
    /**  */
    totalCount?: number

    /**  */
    items?: ChatGroupDto[]
}

export interface ChatListItem {
    /**  */
    id?: number

    /**  */
    lastMsg?: string

    /**  */
    name?: string

    /**  */
    order?: number

    /**  */
    time?: number

    /**  */
    type?: number

    /**  */
    unread?: number

    /**  */
    avatar?: string
}

export interface ChatMessage {
    /**  */
    id?: string

    /**  */
    type?: ChatMessageType

    /**  */
    status?: ChatMessageStatus

    /**  */
    chan?: string

    /**  */
    from?: number

    /**  */
    fromName?: string

    /**  */
    fromAdmin?: boolean

    /**  */
    fromTag?: string

    /**  */
    tagClass?: string

    /**  */
    avatar?: string

    /**  */
    to?: number

    /**  */
    time?: number

    /**  */
    msg?: string

    /**  */
    payload?: any | null

    /**  */
    receipt?: string

    /**  */
    sequenceNumber?: number
}

export interface ChatMessageListResultDto {
    /**  */
    items?: ChatMessage[]
}

export interface CmsArticleCreateOrUpdateDto {
    /**  */
    categoryId?: number

    /**  */
    title?: string

    /**  */
    titleImageUrl?: string

    /**  */
    content?: string

    /**  */
    sort?: number

    /**  */
    status?: AlticleStatusEnum

    /**  */
    id?: number
}

export interface CmsArticleCreateOrUpdateDtoGetForEditOutput {
    /**  */
    data?: CmsArticleCreateOrUpdateDto

    /**  */
    schema?: any | null
}

export interface CmsArticleDto {
    /**  */
    categoryId?: number

    /**  */
    title?: string

    /**  */
    titleImageUrl?: string

    /**  */
    content?: string

    /**  */
    sort?: number

    /**  */
    status?: AlticleStatusEnum

    /**  */
    creationTime?: Date

    /**  */
    creatorUserId?: number

    /**  */
    id?: number
}

export interface CmsArticleDtoPagedResultDto {
    /**  */
    totalCount?: number

    /**  */
    items?: CmsArticleDto[]
}

export interface CmsCategoryDto {
    /**  */
    title?: string

    /**  */
    titleImageUrl?: string

    /**  */
    sort?: number

    /**  */
    id?: number
}

export interface CmsCategoryDtoPagedResultDto {
    /**  */
    totalCount?: number

    /**  */
    items?: CmsCategoryDto[]
}

export interface CreateOrUpdateUserInput {
    /**  */
    user?: UserEditDto

    /**  */
    assignedRoleNames?: string[]

    /**  */
    organizationUnits?: number[]

    /**  */
    setRandomPassword?: boolean
}

export interface CreateRoleDto {
    /**  */
    name?: string

    /**  */
    displayName?: string

    /**  */
    normalizedName?: string

    /**  */
    isDefault?: boolean

    /**  */
    isStatic?: boolean

    /**  */
    description?: string

    /**  */
    grantedPermissions?: string[]

    /**  */
    id?: number
}

export interface CreateTenantDto {
    /**  */
    tenancyName?: string

    /**  */
    name?: string

    /**  */
    adminEmailAddress?: string

    /**  */
    connectionString?: string

    /**  */
    isActive?: boolean
}

export interface CreateUserDto {
    /** 用户名 */
    userName?: string

    /** 邮箱 */
    emailAddress?: string

    /** 密码 */
    password?: string
}

export interface CreateUserDtoGetForEditOutput {
    /**  */
    data?: CreateUserDto

    /**  */
    schema?: any | null
}

export interface EntityPropertyChangeDto {
    /**  */
    entityChangeId?: number

    /**  */
    newValue?: string

    /**  */
    originalValue?: string

    /**  */
    propertyName?: string

    /**  */
    propertyTypeFullName?: string

    /**  */
    tenantId?: number

    /**  */
    id?: number
}

export interface ExternalAuthenticateResultModel {
    /**  */
    accessToken?: string

    /**  */
    encryptedAccessToken?: string

    /**  */
    expireInSeconds?: number

    /**  */
    waitingForActivation?: boolean

    /**  */
    returnUrl?: string

    /**  */
    refreshToken?: string

    /**  */
    refreshTokenExpireInSeconds?: number

    /**  */
    extension?: any | null

    /**  */
    roleNames?: string[]
}

export interface FlatPermissionDto {
    /**  */
    parentName?: string

    /**  */
    name?: string

    /**  */
    displayName?: string

    /**  */
    description?: string
}

export interface GetCurrentLoginInformationsOutput {
    /**  */
    application?: ApplicationInfoDto

    /**  */
    user?: UserLoginInfoDto

    /**  */
    tenant?: TenantLoginInfoDto

    /**  */
    permissions?: string[]

    /**  */
    roles?: string[]
}

export interface GetRoleForEditOutput {
    /**  */
    role?: RoleEditDto

    /**  */
    permissions?: FlatPermissionDto[]

    /**  */
    grantedPermissionNames?: string[]
}

export interface GetUserForEditOutput {
    /**  */
    headImgUrl?: string

    /**  */
    user?: UserEditDto

    /**  */
    roles?: UserRoleDto[]

    /**  */
    memberedOrganizationUnits?: string[]
}

export interface IsTenantAvailableInput {
    /**  */
    tenancyName?: string
}

export interface IsTenantAvailableOutput {
    /**  */
    state?: TenantAvailabilityState

    /**  */
    tenantId?: number
}

export interface PermissionDto {
    /**  */
    name?: string

    /**  */
    displayName?: string

    /**  */
    description?: string

    /**  */
    id?: number
}

export interface PermissionDtoListResultDto {
    /**  */
    items?: PermissionDto[]
}

export interface RefreshTokenResult {
    /**  */
    accessToken?: string

    /**  */
    encryptedAccessToken?: string

    /**  */
    expireInSeconds?: number
}

export interface RegisterInput {
    /**  */
    name?: string

    /**  */
    surname?: string

    /**  */
    phoneNumber?: string

    /**  */
    userName?: string

    /**  */
    emailAddress?: string

    /**  */
    password?: string

    /**  */
    captchaResponse?: string
}

export interface RegisterOutput {
    /**  */
    canLogin?: boolean
}

export interface ResetPasswordDto {
    /**  */
    adminPassword?: string

    /**  */
    userId?: number

    /**  */
    newPassword?: string
}

export interface RoleDto {
    /**  */
    name?: string

    /**  */
    displayName?: string

    /**  */
    isDefault?: boolean

    /**  */
    isStatic?: boolean

    /**  */
    normalizedName?: string

    /**  */
    description?: string

    /**  */
    grantedPermissions?: string[]

    /**  */
    id?: number
}

export interface RoleDtoListResultDto {
    /**  */
    items?: RoleDto[]
}

export interface RoleDtoPagedResultDto {
    /**  */
    totalCount?: number

    /**  */
    items?: RoleDto[]
}

export interface RoleEditDto {
    /**  */
    name?: string

    /**  */
    displayName?: string

    /**  */
    description?: string

    /**  */
    isStatic?: boolean

    /**  */
    isDefault?: boolean

    /**  */
    id?: number
}

export interface RoleListDto {
    /**  */
    name?: string

    /**  */
    displayName?: string

    /**  */
    isStatic?: boolean

    /**  */
    isDefault?: boolean

    /**  */
    creationTime?: Date

    /**  */
    id?: number
}

export interface RoleListDtoListResultDto {
    /**  */
    items?: RoleListDto[]
}

export interface SendChangeMsgInput {
    /**  */
    from?: number

    /**  */
    chan?: string

    /**  */
    message?: ChatMessage
}

export interface SendMsgInput {
    /**  */
    id?: string

    /**  */
    from?: number

    /**  */
    to?: number

    /**  */
    message?: ChatMessage

    /**  */
    isReceipt?: boolean
}

export interface SensitiveWordDto {
    /**  */
    content?: string

    /**  */
    id?: number
}

export interface SensitiveWordDtoGetForEditOutput {
    /**  */
    data?: SensitiveWordDto

    /**  */
    schema?: any | null
}

export interface SensitiveWordDtoPagedResultDto {
    /**  */
    totalCount?: number

    /**  */
    items?: SensitiveWordDto[]
}

export interface SubStartNotifyRequest {
    /**  */
    auctionItemId?: number

    /**  */
    openid?: string
}

export interface SubscrChannelInput {
    /**  */
    websocketId?: number

    /**  */
    channel?: string
}

export interface TenantDto {
    /**  */
    tenancyName?: string

    /**  */
    name?: string

    /**  */
    isActive?: boolean

    /**  */
    id?: number
}

export interface TenantDtoPagedResultDto {
    /**  */
    totalCount?: number

    /**  */
    items?: TenantDto[]
}

export interface TenantLoginInfoDto {
    /**  */
    tenancyName?: string

    /**  */
    name?: string

    /**  */
    id?: number
}

export interface UserBalanceLogDto {
    /**  */
    amount?: number

    /**  */
    type?: BalanceLogType

    /**  */
    reason?: string

    /**  */
    isSuccess?: boolean

    /**  */
    successTime?: Date

    /**  */
    afterAmount?: number

    /**  */
    id?: string
}

export interface UserBalanceLogDtoGetForEditOutput {
    /**  */
    data?: UserBalanceLogDto

    /**  */
    schema?: any | null
}

export interface UserBalanceLogDtoPagedResultDto {
    /**  */
    totalCount?: number

    /**  */
    items?: UserBalanceLogDto[]
}

export interface UserDepositLogDto {
    /**  */
    amount?: number

    /**  */
    type?: BalanceLogType

    /**  */
    reason?: string

    /**  */
    isSuccess?: boolean

    /**  */
    successTime?: Date

    /**  */
    afterAmount?: number

    /**  */
    id?: string
}

export interface UserDepositLogDtoGetForEditOutput {
    /**  */
    data?: UserDepositLogDto

    /**  */
    schema?: any | null
}

export interface UserDepositLogDtoPagedResultDto {
    /**  */
    totalCount?: number

    /**  */
    items?: UserDepositLogDto[]
}

export interface UserDto {
    /**  */
    userName?: string

    /**  */
    name?: string

    /**  */
    surname?: string

    /**  */
    emailAddress?: string

    /**  */
    isActive?: boolean

    /**  */
    fullName?: string

    /**  */
    lastLoginTime?: Date

    /**  */
    creationTime?: Date

    /**  */
    roleNames?: string[]

    /**  */
    phoneNumber?: string

    /**  */
    headImgUrl?: string

    /**  */
    fromClient?: number

    /**  */
    permissions?: string[]

    /**  */
    qq?: string

    /**  */
    wx?: string

    /**  */
    balance?: number

    /**  */
    depositBalance?: number

    /** 累计拍卖金额 */
    cumulativeAmount?: number

    /**  */
    id?: number
}

export interface UserDtoBase {
    /**  */
    userName?: string

    /**  */
    name?: string

    /**  */
    phoneNumber?: string

    /**  */
    surname?: string

    /**  */
    headImgUrl?: string

    /**  */
    qq?: string

    /**  */
    wx?: string

    /**  */
    id?: number
}

export interface UserDtoBaseListResultDto {
    /**  */
    items?: UserDtoBase[]
}

export interface UserDtoListResultDto {
    /**  */
    items?: UserDto[]
}

export interface UserDtoPagedResultDto {
    /**  */
    totalCount?: number

    /**  */
    items?: UserDto[]
}

export interface UserEditDto {
    /** Set null to create a new user. Set user's Id to update a user */
    id?: number

    /**  */
    userName?: string

    /**  */
    emailAddress?: string

    /**  */
    name?: string

    /**  */
    surname?: string

    /**  */
    headImgUrl?: string

    /**  */
    phoneNumber?: string

    /**  */
    password?: string

    /**  */
    isActive?: boolean

    /** QQ */
    qq?: string

    /** 微信号 */
    wx?: string
    /**
     * 诚信履约金
     */
    depositBalance: number
}

export interface UserLoginInfoDto {
    /**  */
    name?: string

    /**  */
    surname?: string

    /**  */
    userName?: string

    /**  */
    headImgUrl?: string

    /**  */
    phoneNumber?: string

    /**  */
    id?: number

    /**
     * 余额
     */
    balance: number

    /**
     * 诚信履约金
     */
    depositBalance: number

    /**
     * 是否需要完善个人信息
     */
    needProfileCompletion?: boolean

    /**
     * 是否跳过个人信息完善
     */
    skipProfileCompletion?: boolean
}

export interface UserRoleDto {
    /**  */
    roleId?: number

    /**  */
    roleName?: string

    /**  */
    roleDisplayName?: string

    /**  */
    isAssigned?: boolean
}

export interface WeChatMiniProgramAuthenticateModel {
    /**  */
    code?: string

    /**  */
    encryptedData?: string

    /**  */
    iv?: string

    /**  */
    session_key?: string

    /**  */
    openid?: string

    /**  */
    unionid?: string

    /**  */
    appid?: string
}

export interface WechatUserinfo {
    /**  */
    openid?: string

    /**  */
    unionid?: string

    /**  */
    nickname?: string

    /**  */
    headimgurl?: string

    /**  */
    city?: string

    /**  */
    province?: string

    /**  */
    country?: string

    /**  */
    sex?: number

    /**  */
    tenantId?: number

    /**  */
    fromClient?: FromClient

    /**  */
    creationTime?: Date

    /**  */
    creatorUserId?: number

    /**  */
    lastModificationTime?: Date

    /**  */
    lastModifierUserId?: number
}

export enum AlticleStatusEnum {
    '草稿' = '草稿',
    '已发布' = '已发布',
}

export enum AuctionStatusEnum {
    '草稿' = '草稿',
    '上架' = '上架',
    '拍卖中' = '拍卖中',
    '已成交' = '已成交',
    '交易成功' = '交易成功',
    '卖家失约' = '卖家失约',
    '买家失约' = '买家失约',
    '交易关闭' = '交易关闭',
}

export enum BalanceLogType {
    '支付' = '支付',
    '扣除' = '扣除',
    '退还' = '退还',
}

export enum ChatMessageStatus {
    'Sending' = 'Sending',
    'Fail' = 'Fail',
    'Success' = 'Success',
}

export enum ChatMessageType {
    'Text' = 'Text',
    'Image' = 'Image',
    'File' = 'File',
    'Receipt' = 'Receipt',
    'Welcome' = 'Welcome',
    'Goodbye' = 'Goodbye',
    'BanUser' = 'BanUser',
    'Backout' = 'Backout',
    'AuctionStart' = 'AuctionStart',
    'AuctionBid' = 'AuctionBid',
    'AuctionEnd' = 'AuctionEnd',
    'AuctionDeal' = 'AuctionDeal',
    'Error' = 'Error',
    'KasecStatusChanged' = 'KasecStatusChanged',
    'System' = "System",
}

export interface CompleteProfileInput {
    /** 手机号 */
    phoneNumber?: string

    /** 用户名 */
    userName?: string

    /** 密码 */
    password?: string
}

export interface QrLoginResult {
    /** JWT访问令牌 */
    accessToken?: string

    /** 是否需要完善个人信息 */
    needProfileCompletion?: boolean

    /** 用户ID */
    userId?: number
}

export enum FromClient {
    'Default' = 'Default',
    'WechatMini' = 'WechatMini',
    'WechatPublic' = 'WechatPublic',
}

export enum TenantAvailabilityState {
    'Available' = 'Available',
    'InActive' = 'InActive',
    'NotFound' = 'NotFound',
}
