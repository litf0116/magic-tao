import request from '@/utils/request'

/**
 * 二维码生成响应
 */
export interface QrCodeGenerateOutputDto {
    /** 二维码code */
    code: string
    /** 二维码内容 (molitao://scan?code=xxx) */
    qrContent: string
    /** 有效期（秒） */
    expiresIn: number
}

/**
 * 用户信息（扫码确认后返回）
 */
export interface QrCodeUserInfoDto {
    /** 用户ID */
    userId: number
    /** 昵称 */
    nickname: string
    /** 头像 */
    avatar: string
    /** 手机号（脱敏: 138****1234） */
    phone: string
}

/**
 * 轮询状态响应
 */
export interface QrCodeStatusDto {
    /** 状态 (pending/scanned/confirmed/expired) */
    status: 'pending' | 'scanned' | 'confirmed' | 'expired'
    /** 用户信息（仅 confirmed 时返回） */
    user?: QrCodeUserInfoDto
}

/**
 * 生成二维码
 * PC端生成扫码登录二维码
 */
export function generateQrCode(): Promise<QrCodeGenerateOutputDto> {
    return request.post('/api/auth/qrcode')
}

/**
 * 获取二维码状态
 * PC端轮询扫码登录状态
 * @param code 二维码code
 */
export function getQrCodeStatus(code: string): Promise<QrCodeStatusDto> {
    return request.get(`/api/auth/qrcode/${code}/status`)
}
