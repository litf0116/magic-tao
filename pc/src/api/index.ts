import request from '@/utils/request'

import {
    SessionService,
    TokenAuthService,
    serviceOptions,
    UserService,
    RoleService,
    WebSocketService,
    MessageService,
    UserFriendService,
    UploadService,
    CmsArticleService,
    CmsCategoryService,
    AuctionItemService,
    AnnounceService,
    BidHistoryService,
    SensitiveWordService,
    BanedUserService,
    ChatGroupService,
    ChatEmojiService,
    ClientService,
} from './appService'

import appRelease from './appRelease'
import versionControl from './versionControl'

serviceOptions.axios = request

const announce = AnnounceService
const auctionItem = AuctionItemService
const bidHistory = BidHistoryService
const banedUser = BanedUserService
const client = ClientService
const chatGroup = ChatGroupService
const chatEmoji = ChatEmojiService
const cmsArticle = CmsArticleService
const cmsCategory = CmsCategoryService
const message = MessageService
const role = RoleService
const session = SessionService
const sensitiveWord = SensitiveWordService
const tokenAuth = TokenAuthService
const upload = UploadService
const user = UserService
const userFriend = UserFriendService
const ws = WebSocketService

export default {
    guid: '00000000-0000-0000-0000-000000000000',
    announce,
    appRelease,
    versionControl,
    auctionItem,
    bidHistory,
    banedUser,
    client,
    chatGroup,
    chatEmoji,
    cmsArticle,
    cmsCategory,
    message,
    role,
    session,
    sensitiveWord,
    tokenAuth,
    upload,
    user,
    userFriend,
    ws,
}
