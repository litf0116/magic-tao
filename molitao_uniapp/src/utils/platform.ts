/**
 * 平台判断工具
 */

export const Platform = {
    name: 'unknown',
    isMiniprogram: false,
    isApp: false,
    isH5: false,
}

// #ifdef MP-WEIXIN
Platform.name = 'miniprogram'
Platform.isMiniprogram = true
// #endif

// #ifdef APP-PLUS
Platform.name = 'app'
Platform.isApp = true
// #endif

// #ifdef H5
Platform.name = 'h5'
Platform.isH5 = true
// #endif

/**
 * 是否为小程序
 */
export function isMiniprogram(): boolean {
    return Platform.isMiniprogram
}

/**
 * 是否为App
 */
export function isApp(): boolean {
    return Platform.isApp
}

/**
 * 是否为H5
 */
export function isH5(): boolean {
    return Platform.isH5
}

/**
 * 获取当前平台名称
 */
export function getPlatform(): 'miniprogram' | 'app' | 'h5' | 'unknown' {
    return Platform.name as any
}
