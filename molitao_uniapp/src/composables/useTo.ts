export default () => {
    function toLogin(url: string, options: any) {
        let param = '?'
        if (options)
            Object.keys(options).forEach((key) => {
                param += `${key}=${options[key]}&`
            })
        const redirectUrl = encodeURIComponent(`${url}${param.substring(0, param.length - 1)}`)
        // uni.removeStorageSync("redirectUrl");
        uni.navigateTo({ url: `/pages/index/login?url=${redirectUrl}` })
    }

    function toHome() {
        uni.redirectTo({ url: '/pages/tabbar/index' })
    }

    function toMy() {
        uni.redirectTo({ url: '/pages/index/my' })
    }

    function toRegister() {
        uni.navigateTo({ url: '/pages/index/register' })
    }

    function toForgotPassword() {
        uni.navigateTo({ url: '/pages/index/forgot-password' })
    }

    function toCall(phone: any) {
        uni.makePhoneCall({
            phoneNumber: phone,
        })
    }

    /**
     * 统一跳转接口,拦截未登录路由
     * navigator标签现在默认没有转场动画，所以用view
     */
    function navTo(url: any, isTab = false, state = 0) {
        uni.setStorageSync('Tab_Select_Index', state)
        // if (!this.openid) {
        //     url = "/pages/index/login";
        // }
        if (isTab) {
            uni.setStorageSync('Tab_Select_Index', state)
            uni.redirectTo({ url })
        } else {
            uni.navigateTo({ url: url })
        }
    }
    return { toLogin, toHome, toMy, toRegister, toForgotPassword, toCall, navTo }
}
