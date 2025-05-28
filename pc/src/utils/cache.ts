import dayjs from 'dayjs'

function setWithExpiry(key: string, value: any, seconds = 3600) {
    const now = new Date()

    // `item` is an object which contains the original value
    // as well as the time when it's supposed to expire
    const item = {
        value: value,
        expiry: dayjs(now).add(seconds, 'seconds'),
    }
    localStorage.setItem(key, JSON.stringify(item))
}

function getWithExpiry(key: string) {
    const itemStr = localStorage.getItem(key)

    // if the item doesn't exist, return null
    if (!itemStr) {
        return null
    }

    const item = JSON.parse(itemStr)
    const now = new Date()
    // compare the expiry time of the item with the current time
    const _diff = dayjs(item.expiry).diff(dayjs(now), 'second')
    if (_diff < 0) {
        // If the item is expired, delete the item from storage
        // and return null
        localStorage.removeItem(key)
        return null
    }
    return item.value
}

export default {
    setWithExpiry,
    getWithExpiry,
}
