import cache from './cache'

const CACHE_KEY = 'upyun_signature'

function Upyun(options) {
    this.bucket = options.bucket
    this.operator = options.operator
    this.getSignatureUrl = options.getSignatureUrl
    this.domainHost = options.domainHost
}

Upyun.prototype.upload = function (options) {
    var self = this
    // console.log(options)
    var date = new Date().toGMTString()
    var opts = {
        'save-key': `/${self.operator}/{year}-{mon}-{day}/upload_{random32}{.suffix}`,
        bucket: self.bucket,
        expiration: Math.round(new Date().getTime() / 1000) + 3600 * 60,
        date: date,
    }
    var _policy = Base64.encode(JSON.stringify(opts))
    var data = ['POST', '/' + self.bucket, date, _policy].join('&')
    self.getSignature(data, _policy, function (err, { signature, bucket, operator, domainHost, policy }) {
        // console.log('getSignature', err, signature, bucket, operator, domainHost)
        if (err) {
            options.fail && options.fail(err)
            options.complete && options.complete(err)
            return
        }
        self.domainHost = domainHost || self.domainHost
        // console.log(`UPYUN ${operator}:${signature}`)
        uni.uploadFile({
            url: `https://v0.api.upyun.com/${bucket}`,
            filePath: options.localPath,
            name: 'file',
            formData: {
                authorization: `UPYUN ${operator}:${signature}`,
                policy: policy,
            },
            success: options.success,
            fail: options.fail,
            complete: options.complete,
        })
    })
}

Upyun.prototype.getSignature = function (data, _policy, cb) {
    let upyun_signature = cache.getWithExpiry(CACHE_KEY)

    if (upyun_signature) {
        cb(null, upyun_signature)
    } else {
        uni.request({
            url: this.getSignatureUrl,
            method: 'GET',
            data: {
                data: data,
                policy: _policy,
            },
            success: async (res) => {
                await cache.setWithExpiry(CACHE_KEY, res.data, 60)
                cb(null, res.data)
            },
            fail: (err) => {
                cb(err)
            },
        })
    }
}

/* eslint-disable */
var Base64 = {
    // private property
    _keyStr: 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=',
    // public method for encoding
    encode: function (input) {
        var output = ''
        var chr1, chr2, chr3, enc1, enc2, enc3, enc4
        var i = 0
        input = Base64._utf8_encode(input)
        while (i < input.length) {
            chr1 = input.charCodeAt(i++)
            chr2 = input.charCodeAt(i++)
            chr3 = input.charCodeAt(i++)
            enc1 = chr1 >> 2
            enc2 = ((chr1 & 3) << 4) | (chr2 >> 4)
            enc3 = ((chr2 & 15) << 2) | (chr3 >> 6)
            enc4 = chr3 & 63
            if (isNaN(chr2)) {
                enc3 = enc4 = 64
            } else if (isNaN(chr3)) {
                enc4 = 64
            }
            output =
                output +
                this._keyStr.charAt(enc1) +
                this._keyStr.charAt(enc2) +
                this._keyStr.charAt(enc3) +
                this._keyStr.charAt(enc4)
        }
        return output
    },
    // public method for decoding
    decode: function (input) {
        var output = ''
        var chr1, chr2, chr3
        var enc1, enc2, enc3, enc4
        var i = 0
        input = input.replace(/[^A-Za-z0-9\+\/\=]/g, '')
        while (i < input.length) {
            enc1 = this._keyStr.indexOf(input.charAt(i++))
            enc2 = this._keyStr.indexOf(input.charAt(i++))
            enc3 = this._keyStr.indexOf(input.charAt(i++))
            enc4 = this._keyStr.indexOf(input.charAt(i++))
            chr1 = (enc1 << 2) | (enc2 >> 4)
            chr2 = ((enc2 & 15) << 4) | (enc3 >> 2)
            chr3 = ((enc3 & 3) << 6) | enc4
            output = output + String.fromCharCode(chr1)
            if (enc3 != 64) {
                output = output + String.fromCharCode(chr2)
            }
            if (enc4 != 64) {
                output = output + String.fromCharCode(chr3)
            }
        }
        output = Base64._utf8_decode(output)
        return output
    },
    // private method for UTF-8 encoding
    _utf8_encode: function (string) {
        string = string.replace(/\r\n/g, '\n')
        var utftext = ''
        for (var n = 0; n < string.length; n++) {
            var c = string.charCodeAt(n)
            if (c < 128) {
                utftext += String.fromCharCode(c)
            } else if (c > 127 && c < 2048) {
                utftext += String.fromCharCode((c >> 6) | 192)
                utftext += String.fromCharCode((c & 63) | 128)
            } else {
                utftext += String.fromCharCode((c >> 12) | 224)
                utftext += String.fromCharCode(((c >> 6) & 63) | 128)
                utftext += String.fromCharCode((c & 63) | 128)
            }
        }
        return utftext
    },
    // private method for UTF-8 decoding
    _utf8_decode: function (utftext) {
        var string = ''
        var i = 0
        var c = (c1 = c2 = 0)
        while (i < utftext.length) {
            c = utftext.charCodeAt(i)
            if (c < 128) {
                string += String.fromCharCode(c)
                i++
            } else if (c > 191 && c < 224) {
                c2 = utftext.charCodeAt(i + 1)
                string += String.fromCharCode(((c & 31) << 6) | (c2 & 63))
                i += 2
            } else {
                c2 = utftext.charCodeAt(i + 1)
                c3 = utftext.charCodeAt(i + 2)
                string += String.fromCharCode(((c & 15) << 12) | ((c2 & 63) << 6) | (c3 & 63))
                i += 3
            }
        }
        return string
    },
}
/* eslint-enable */

// module.exports = Upyun
export default {
    Upyun,

    CACHE_KEY,
}
