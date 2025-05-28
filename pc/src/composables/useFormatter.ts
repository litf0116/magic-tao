import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'
import timezone from 'dayjs/plugin/timezone'
import { ElImage } from 'element-plus'
dayjs.extend(utc)
dayjs.extend(timezone)

export default () => {
    const imagePreview = (row: any, column: any, value: any, index: any) => {
        const prefix = '!w300'
        let result = Array.isArray(value) ? (value.length >= 1 ? value[0] : '') : value
        if (result) {
            //判断图片URL是不是以!w300结尾,如果不是,加上
            const thumUrl = result.endsWith(prefix) ? result : `${result}${prefix}`
            //判断图片URL是不是以!w300结尾,如果是,去掉
            result = result.endsWith(prefix) ? result.slice(0, -prefix.length) : result
            console.log('thumUrl', thumUrl)
            console.log('result', result)
            return h(ElImage, {
                src: `${thumUrl}`,
                style: { width: '64px', height: '64px' },
                fit: 'contain',
                'preview-src-list': [result],
                'zoom-rate': 1.2,
                'z-index': 9999,
                'hide-on-click-modal': true,
                'preview-teleported': true,
            })
        } else return ''
    }
    function utcToLocalFull(row: any, column: any, value: any, index: any) {
        let timestamp = dayjs.utc(value, 'YYYY-MM-DD HH:mm:ss') // parse the timestring as utc
        timestamp = dayjs(timestamp).tz(dayjs.tz.guess()) // convert to user's timezone
        return timestamp.format('YYYY-MM-DD HH:mm:ss')
    }
    function utcToLocalDay(row: any, column: any, value: any, index: any) {
        let timestamp = dayjs.utc(value, 'YYYY-MM-DD HH:mm:ss') // parse the timestring as utc
        timestamp = dayjs(timestamp).tz(dayjs.tz.guess()) // convert to user's timezone
        return timestamp.format('YYYY-MM-DD')
    }

    function toDay(row: any, column: any, value: any, index: any) {
        if (value) {
            const timestamp = dayjs(value).tz(dayjs.tz.guess()) // convert to user's timezone
            return timestamp.format('YYYY-MM-DD')
        }
        return value
    }

    return { imagePreview, utcToLocalDay, utcToLocalFull, toDay }
}
