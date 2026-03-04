export const Goto = {
    lobby: () => {
        uni.navigateTo({ url: '/pages/chat/lobby?id=0_lobby' })
    },
    group: (options: { [key: string]: string }) => {
        const params = getParams(options)
        const url = '/pages/chat/groupChat' + (params ? '?' + params : '')
        uni.navigateTo({ url })
    },
    auction: (options: { [key: string]: string } = {}) => {
        const params = getParams(options)
        const url = '/pages/chat/auction' + (params ? '?' + params : '')
        uni.navigateTo({ url })
    },
    private: (options: { [key: string]: string }) => {
        const params = getParams(options)
        const url = '/pages/chat/privateChat' + (params ? '?' + params : '')
        uni.navigateTo({ url })
    }
}

function getParams(options: { [key: string]: string }) {
    let params = ''
    for (const key in options) {
        params += `${key}=${options[key]}&`
    }
    params = params.slice(0, -1)
    return params
}