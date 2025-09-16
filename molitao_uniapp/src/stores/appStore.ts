import { defineStore } from 'pinia'
const DARKMODE = 'darkMode'

export const useAppStore = defineStore('appStore', () => {
    const darkMode = ref(uni.getStorageSync(DARKMODE) || false)
    const loading = ref(false)

    const adm1 = ref('')
    const adm2 = ref('')
    const city = ref('')

    const latitude = ref(0)
    const longitude = ref(0)

    const weatherIcon = ref('')
    const weather = ref('')

    function toggleDarkMode() {
        darkMode.value = !darkMode.value
        uni.setStorageSync(DARKMODE, darkMode.value)
    }

    /**
     * 获取位置信息
     * @returns 返回一个 Promise，当获取位置信息成功时，将返回包含经纬度的对象，否则将返回错误信息。
     */
    function getLocation() {
        return new Promise((resolve, reject) => {
            uni.getLocation({
                type: 'wgs84',
                success: (res: any) => {
                    console.log('getLocation', res)
                    latitude.value = res.latitude
                    longitude.value = res.longitude
                    resolve({ latitude: res.latitude, longitude: res.longitude })
                },
                fail: (err: any) => {
                    console.log('getLocation', err)
                    reject(err)
                },
            })
        })
    }

    function getCity(latitude: number, longitude: number) {
        if (!latitude || !longitude) return
        const url = `https://geoapi.heweather.net/v2/city/lookup?location=${longitude},${latitude}&key=c6c09cdfe21145ce899908a97aaa0855`
        uni.request({
            url: url,
            success: (res: any) => {
                console.log('getCity', res)

                if (res && res.data && res.data.location && res.data.location.length > 0) {
                    // console.log("getCity", res.data.location[0])
                    const _v = res.data.location[0]

                    city.value = _v.name //城市
                    adm1.value = _v.adm1 //省
                    adm2.value = _v.adm2 //市
                }
                // if (res.data.location.length > 0) {
                //     that.setData({
                //         location: res.data.location[0].adm2 + "·" + res.data.location[0].name,
                //     })
                // }
            },
        })
    }

    function getWeather(latitude: number, longitude: number) {
        const url = `https://devapi.heweather.net/v7/weather/now?location=${longitude},${latitude}&key=c6c09cdfe21145ce899908a97aaa0855`
        uni.request({
            url: url,
            success: (res: any) => {
                if (res && res.data && res.data.now) {
                    if (res.data.now.icon) {
                        weatherIcon.value = '/static/weather/' + res.data.now.icon + '.png'
                    }
                    if (res.data.now.temp && res.data.now.text) {
                        weather.value = res.data.now.temp + '℃ ' + res.data.now.text
                    }
                }
                // that.setData({
                //     weatherIcon: "/images/weather/" + res.data.now.icon + ".png",
                //     weather: res.data.now.temp + "℃ " + res.data.now.text,
                // })
            },
        })
    }

    return {
        darkMode,
        loading,
        adm2,
        city,
        weatherIcon,
        weather,

        //ACTION
        toggleDarkMode,
        getLocation,
        getCity,
        getWeather,
    }
})
