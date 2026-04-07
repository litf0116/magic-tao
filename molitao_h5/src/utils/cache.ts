import dayjs from 'dayjs'

function setWithExpiry(key: string, value: any, seconds = 3600) {
    // `item` is an object which contains the original value
    // as well as the time when it's supposed to expire
    const item = {
        value: value,
        expiry: dayjs().add(seconds, 'seconds'),
    }
    uni.setStorageSync(key, JSON.stringify(item))
}

function remove(key: string) {
    uni.removeStorageSync(key)
}

function getWithExpiry(key: string) {
    const itemStr = uni.getStorageSync(key)

    // if the item doesn't exist, return null
    if (!itemStr) {
        return null
    }

    const item = JSON.parse(itemStr)
    // compare the expiry time of the item with the current time
    const _diff = dayjs(item.expiry).diff(dayjs(), 'second')
    if (_diff < 0) {
        // If the item is expired, delete the item from storage
        // and return null
        uni.removeStorageSync(key)
        return null
    }
    return item.value
}

export default {
    setWithExpiry,
    getWithExpiry,
    remove,
}
