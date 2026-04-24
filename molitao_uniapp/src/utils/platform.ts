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
Platform.name = 'mp-weixin'
Platform.isMiniprogram = true
// #endif

// #ifdef APP-PLUS
Platform.name = 'app-plus'
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
 * 获取当前平台名称（与后端约定一致）
 * mp-weixin / app-plus / h5
 */
export function getPlatform(): 'mp-weixin' | 'app-plus' | 'h5' | 'unknown' {
    return Platform.name as any
}
