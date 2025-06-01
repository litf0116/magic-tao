export interface IListType {
    totalCount: number
    items: any[]
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
    'Error' = 'Error',
    'KasecStatusChanged' = 'KasecStatusChanged',
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

export enum ChatListItemType {
    group = 0,
    user = 1,
    system = 2,
}

export type ChatListItem = {
    id?: number
    name: string
    type: ChatListItemType
    time?: number
    avatar?: string
    lastMsg?: string
    unread: number
    order: number
    msg?: ChatMessage
}

export interface UserDtoBaseListResultDto {
    /**  */
    items?: UserDtoBase[]
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
    status?: AlticleStatusEnum

    /**  */
    creationTime?: Date

    /**  */
    creatorUserId?: number

    /**  */
    id?: number
}

export enum AlticleStatusEnum {
    '草稿' = '草稿',
    '已发布' = '已发布',
}

export interface AnnounceDto {
    /**  */
    categoryId?: number

    /**  */
    content?: string

    /**  */
    imageUrl?: string

    /**  */
    creationTime?: Date

    /**  */
    creatorUserId?: number

    /**  */
    id?: number
}

export enum AuctionStatusEnum {
    '草稿' = '草稿',
    '上架' = '上架',
    '拍卖中' = '拍卖中',
    '已成交' = '已成交',
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
    order?: number

    /**  */
    id?: number
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



export interface ChatEmojiDto {
    /**  */
    url?: string;

    /**  */
    payload?: string;

    /**  */
    id?: number;
  }