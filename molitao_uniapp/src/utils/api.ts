import type {
    AnnounceDto,
    AuctionItemDto,
    BidHistoryCreateDto,
    ChatEmojiDto,
    ChatMessage,
    IListType,
    UserDto,
    UserDtoBaseListResultDto,
} from '@/composables/types'
import utils from './utils'
import { getAppVersion } from './version'
import { getPlatform } from './platform'

let host: string = 'https://www.molitao.top'

const getRequest = utils.httpsPromisify(uni.request)

const request = (
    method: 'OPTIONS' | 'GET' | 'HEAD' | 'POST' | 'PUT' | 'DELETE' | 'TRACE' | 'CONNECT',
    url: string,
    data?: string | object | ArrayBuffer | undefined,
    showLoading = true
) => {
    if (showLoading) {
        uni.showLoading({})
        uni.showNavigationBarLoading()
    }

    const _url = url.startsWith('http') ? url : host + url

    const appVersion = getAppVersion()
    const platform = getPlatform()

    return getRequest({
        url: _url,
        data: data,
        method: method,
        timeout: 300000,
        header: {
            'Abp.Tenantid': 1,
            'content-type': 'application/json',
            Authorization: `Bearer ${uni.getStorageSync('token') || ''}`,
            'X-Platform': platform,
            'X-App-Version': appVersion,
            'AppName': 'uniapp',
        },
    })
}

export default {
    guid: '00000000-0000-0000-0000-000000000000',
    //小程序

    tokenAuth: {
        Logout: () => request('GET', `/api/TokenAuth/Logout`),
    },

    authenticate: (data: any) => request('POST', `/api/TokenAuth/Authenticate`, data),
    weixinMiniAuthenticate: (data: any) => request('POST', `/api/TokenAuth/WeixinMiniAuthenticate`, data),
    phoneAuth: (data: any) => request('POST', `/api/TokenAuth/WeixinMiniPhoneAuthenticate`, data),
    getPhone: (data: any) => request('POST', `/api/app/weixin/getPhone`, data),
    code2session: (data: any) => request('GET', `/api/services/app/Client/minicode2session`, data),

    // account
    account: {
        bindPhoneWithPassword: (data: { phoneNumber: string; password: string }) =>
            request('POST', `/api/services/app/Account/BindPhoneWithPassword`, data),
    },

    // user
    getCurrentLoginInformations: () => request('GET', `/api/services/app/Session/GetCurrentLoginInformations`),

    user: {
        get: (data: any) => request('GET', `/api/services/app/User/Get`, data) as Promise<UserDto>,
        update: (data: any) => request('PUT', `/api/services/app/User/Update`, data),
        getAll: (data: any) => request('GET', `/api/services/app/User/GetAll`, data),
    },

    ws: {
        sendChannelMsg: (data: { from: number; chan: string; message: ChatMessage }) =>
            request('POST', `/ws/SendChannelMsg`, data),
        sendMsg: (data: { from: number; to: number; message: ChatMessage; isReceipt: boolean }) => {
            return request('POST', `/ws/send-msg`, data) as Promise<any>
        },

        preConnect: () => request('POST', `/ws/pre-connect`),
        offline: (data: { websocketId?: number }) => request('GET', `/ws/offline`, { websocketId: data.websocketId }),
        getChannels: () => request('POST', `/ws/get-channels`),
        backout: (data: ChatMessage) => request('POST', `/ws/backout`, data),
        leaveChannel: (data: { chan: string }) => request('GET', `/ws/leave-channel`, data),
        subChannel: (data: { websocketId: number; channel: string }) => request('POST', `/ws/sub-channel`, data),
        delChannel: (data: { chan?: string }) => request('GET', `/ws/del-channel`, data),
        banUser: (data: { userId: number; minutes: number; chan: any }) => request('POST', `/ws/ban-user`, data),
    },
    userFriend: {
        addFriend: (data: { id?: number }) => request('GET', `/api/services/app/UserFriend/AddFriend`, data),
        getUserFriends: (data: { id: number; status: boolean }) =>
            request('GET', `/api/services/app/UserFriend/GetUserFriends`, data) as Promise<UserDtoBaseListResultDto>,

        agree: (params: { id?: number; status?: boolean }) =>
            request('GET', `/api/services/app/UserFriend/Agree`, params),
        /**
         * 获取好友申请记录
         * @returns
         */
        GetUserFriendCount: () => request('GET', `/api/services/app/UserFriend/GetUserFriendCount`, undefined, false),
    },
    message: {
        getPrivateHistory: (data: { id: number; lastTime?: number; size?: number }) =>
            request('GET', `/api/services/app/Message/getPrivateHistory`, data, false) as Promise<{
                items?: ChatMessage[]
            }>,
        getChanHistory: (data: { chan: string; lastTime?: number; size?: number }) =>
            request('GET', `/api/services/app/Message/getChanHistory`, data, false) as Promise<{
                items?: ChatMessage[]
            }>,

        getChanLastId: (data: { chan: string }) =>
            request('GET', `/api/services/app/Message/getChanLastId`, data, false) as Promise<string>,

        getPrivateLastId: (data: { id: number }) =>
            request('GET', `/api/services/app/Message/getPrivateLastId`, data, false) as Promise<string>,
    },

    announce: {
        /**
         * 获取通知公告
         * @param data
         * @returns
         */
        getLatest: (data: { id: number }) =>
            request('GET', `/api/services/app/Announce/GetLatest`, data) as Promise<AnnounceDto>,
        /**
         * 获取所有通知列表
         * @param data
         * @returns
         */
        getAll: (data: { pid: number }) =>
            request('GET', `/api/services/app/Announce/GetAllPublic`, data) as Promise<IListType>,
    },

    cmsArticle: {
        getAll: (data: { pid: number }) => request('GET', `/api/services/app/CmsArticle/GetAllPublic`, data, false),
    },

    auctionItem: {
        endAuction: (data: { id: number }) =>
            request('GET', `/api/services/app/AuctionItem/EndAuction`, data) as Promise<AuctionItemDto>,
        bid: (data: BidHistoryCreateDto) =>
            request('POST', `/api/services/app/AuctionItem/Bid`, data) as Promise<AuctionItemDto>,
        getPublicList: (data: { status?: number; MaxResultCount: number }) =>
            request('GET', `/api/services/app/AuctionItem/GetPublicList`, data) as Promise<IListType>,
        startAuction: (data: { id: number }) =>
            request('GET', `/api/services/app/AuctionItem/StartAuction`, data) as Promise<AuctionItemDto>,
        getMySuccessList: (data: { skipCount: number; MaxResultCount: number }) =>
            request('GET', `/api/services/app/AuctionItem/GetMySuccessList`, data) as Promise<IListType>,
        subStartNotify: (data: { auctionItemId: number; openid: string }) =>
            request('POST', `/api/services/app/AuctionItem/SubStartNotify`, data),
        getAuctionMidList: (data: any) =>
            request('POST', `/api/services/app/AuctionItem/GetAuctionMidList`, data) as Promise<IListType>,
        getKasecStatus: (auctionItemId: number) =>
            request(
                'GET',
                `/api/services/app/AuctionItem/GetKasecStatus?auctionItemId=${auctionItemId}`
            ) as Promise<boolean>,
        getDetail: (id: number) => request('GET', `/api/AuctionItem/GetDetail?id=${id}`) as Promise<AuctionItemDto>,
    },

    chatEmoji: {
        getAll: () => request('GET', `/api/services/app/ChatEmoji/GetAll`) as Promise<{ items: ChatEmojiDto[] }>,
        delete: (id: number) => request('DELETE', `/api/services/app/ChatEmoji/Delete?id=${id}`),
        create: (data: ChatEmojiDto) => request('POST', `/api/services/app/ChatEmoji/Create`, data),
    },

    client: {
        payDeposit: (data: any) => request('GET', `/api/services/app/Client/PayDeposit`, data),
        TopUp: (data: any) => request('GET', `/api/services/app/Client/TopUp`, data),
        getMyCount: () => request('GET', `/api/services/app/Client/GetMyCount`),
        getChatList: () => request('GET', `/api/services/app/Client/GetChatList`),
        DeleteChatList: (data: any) => request('GET', `/api/services/app/Client/DeleteChatList`, data),
        PayWithdrawal: (data: any) => request('POST', `/api/services/app/Client/PayWithdrawal`, data),
    },

    testpay: (data: any) => request('GET', `/api/PayNotify/test2`, data) as Promise<any>,
    UserBalanceLog: {
        GetMyAll: (data: any) =>
            request('GET', `/api/services/app/UserBalanceLog/GetMyAll`, data) as Promise<IListType>,
    },
    UserDepositLog: {
        GetMyAll: (data: any) =>
            request('GET', `/api/services/app/UserDepositLog/GetMyAll`, data) as Promise<IListType>,
    },
    /**获取广告位 */
    AdvertisingSpace: {
        GetAdvertisingSpaceAll: (type: any) => request('GET', `/api/AdvertisingSpace/GetTypeList/` + type),
    },

    upload: {
        getSignature: `/api/services/app/Upload/GetSignature`,
    },
    appFeature: {
        getFeatureSwitch: () => request('GET', `/api/services/app/AppFeature/GetFeatureSwitch`),
    },
    /** 获取群聊等级信息列表**/
    groupChatLevelSettings: {
        getList: () => request('GET', `/api/GroupChatLevelSettings/GetList`),
    },
    /** 获取用户群聊等级信息**/
    userGroupLevel: {
        /**
         * 获取指定用户的群聊等级信息（包含等级配置）
         * @param id 用户ID
         * @returns Promise<any>
         */
        getUserLevelInfo: (id: number): Promise<any> =>
            request('GET', `/api/GroupChatLevelSettings/GetUserLevelInfo/${id}`),
        /**
         * 获取指定用户的群聊等级信息
         * @param id 用户ID
         * @returns Promise<any>
         */
        getUserGroupLevel: (id: number): Promise<any> =>
            request('GET', `/api/GroupChatLevelSettings/GetUserGroupLevel/${id}`),
    },

    /** 内容安全审核 */
    contentSecurity: {
        /**
         * 检查文本内容是否安全
         * @param data { content: 文本内容, scene: 场景值, openid: 用户openid }
         * @returns Promise<{ isSafe: boolean, message: string }>
         */
        checkText: (data: { content: string; scene?: number; openid?: string }) =>
            request('POST', `/api/ContentSecurity/CheckContent`, data),
        /**
         * 检查图片内容是否安全
         * @param data { mediaUrl: 图片URL, scene?: 场景值, openid?: 用户openid }
         * @returns Promise<{ isSafe: boolean, message: string }>
         */
        checkImage: (data: { mediaUrl: string; scene?: number; openid?: string }) =>
            request('POST', `/api/ContentSecurity/CheckMedia`, data),
    },
}
