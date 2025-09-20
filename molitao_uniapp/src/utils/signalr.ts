const protocal = {
    protocol: 'json',
    version: 1,
}

const MessageType = {
    /** Indicates the message is an Invocation message and implements the {@link InvocationMessage} interface. */
    Invocation: 1,
    /** Indicates the message is a StreamItem message and implements the {@link StreamItemMessage} interface. */
    StreamItem: 2,
    /** Indicates the message is a Completion message and implements the {@link CompletionMessage} interface. */
    Completion: 3,
    /** Indicates the message is a Stream Invocation message and implements the {@link StreamInvocationMessage} interface. */
    StreamInvocation: 4,
    /** Indicates the message is a Cancel Invocation message and implements the {@link CancelInvocationMessage} interface. */
    CancelInvocation: 5,
    /** Indicates the message is a Ping message and implements the {@link PingMessage} interface. */
    Ping: 6,
    /** Indicates the message is a Close message and implements the {@link CloseMessage} interface. */
    Close: 7,
}

export function isValidKey(key: string | number | symbol, object: object): key is keyof typeof object {
    return key in object
}

export class HubConnection {
    private static instance: HubConnection

    openStatus = false
    methods: object = {}
    negotiateResponse: {
        availableTransports?: Array<{ transferFormats?: Array<string>; transport?: string }>
        connectionId?: string
        negotiateVersion?: number
    } = {}
    connection: WechatMiniprogram.SocketTask | null = null
    url = ''
    invocationId = 0
    callbacks: any = {}

    private ping_timerId: number | null = null
    private ping_running: boolean = false

    private constructor() {
        // 私有构造函数，防止外部实例化
    }

    private intervalFunction(): void {
        // 这里是你想要每30秒执行一次的函数
        this.sendData({ type: 6 })
    }

    private ping_start(): void {
        if (!this.ping_running) {
            this.ping_timerId = setInterval(() => {
                this.intervalFunction()
            }, 15000) // 15秒
            this.ping_running = true
        } else {
            // Timer is already running
        }
    }

    private ping_stop(): void {
        if (this.ping_running && this.ping_timerId) {
            clearInterval(this.ping_timerId)
            this.ping_timerId = null
            this.ping_running = false
        } else {
            // Timer is not running
        }
    }

    public static getInstance(): HubConnection {
        if (!HubConnection.instance) {
            HubConnection.instance = new HubConnection()
        }
        return HubConnection.instance
    }

    public start(url: string, args: object | null = null): void {
        let negotiateUrl = url + '/negotiate'
        if (args) {
            for (const key in args) {
                negotiateUrl +=
                    (negotiateUrl.indexOf('?') < 0 ? '?' : '&') +
                    (`${key}=` + encodeURIComponent(isValidKey(key, args) ? args[key] : ''))
            }
        }

        uni.request({
            url: negotiateUrl,
            method: 'POST',
            async: false,
            header: { Authorization: `Bearer ${uni.getStorageSync('token') || ''}` },
            success: (res) => {
                this.negotiateResponse = res.data as any
                this.startSocket(negotiateUrl.replace('/negotiate', ''))
            },
            fail: (res) => {
                return
            },
        })
    }

    public startSocket(url: string): void {
        const connectId = this.negotiateResponse.connectionId
        const token = uni.getStorageSync('token')
        url += `${url.indexOf('?') < 0 ? '?' : '&'}id=${connectId}&access_token=${token}`
        url = url.replace(/^http/, 'ws')
        this.url = url
        if (this.connection != null && this.openStatus) {
            return
        }

        this.connection = wx.connectSocket({
            url: url,
        })

        this.connection.onOpen((res) => {
            this.sendData(protocal)
            this.ping_start()
            uni.setStorageSync('signalr_ping_time', new Date())
            this.openStatus = true
            this.onOpen(res)
        })

        this.connection.onClose((res) => {
            this.connection = null
            this.openStatus = false
            this.onClose(res)
        })

        this.connection.onError((res) => {
            this.close({
                reason: res.errMsg,
            })
            this.onError(res.errMsg)
        })

        this.connection.onMessage((res: { data: string | ArrayBuffer }) => this.receive(res))
    }

    public on(key: string, fun: any): void {
        //@ts-ignore
        this.methods[key] = fun
    }

    public onOpen(data: object) {
        // WebSocket connection opened
    }

    public onPing() {}

    public onClose(res: { code: number; reason: string }): void {
        this.ping_stop()
    }

    public onError(msg: string): void {
        // WebSocket error occurred
    }

    public close(options: any = {}): void {
        if (this.connection) {
            if (options) {
                this.connection.close(options)
            } else {
                this.connection.close({})
            }
        }
        this.ping_stop()
        this.openStatus = false
    }

    public sendData(
        data: object,
        success?: (res?: { errMsg: string }) => void | undefined,
        fail?: (res?: { errMsg: string }) => void | undefined,
        complete?: (res?: { errMsg: string }) => void | undefined
    ): void {
        if (this.connection)
            this.connection.send({
                data: JSON.stringify(data) + '', //
                success: success,
                fail: fail,
                complete: complete,
            })
    }

    public receive(data: { data: string | ArrayBuffer }): void {
        if (data.data === '{}') {
            return
        }
        const responses = (data.data as string).split('').filter((x) => x)
        // data.data = (data.data as string).replace("", "")
        // SignalR message received

        responses.forEach((x) => {
            const message = JSON.parse(x)
            switch (message.type) {
                case MessageType.Invocation:
                    this.invokeClientMethod(message)
                    break
                case MessageType.StreamItem:
                    break
                case MessageType.Completion:
                    const callback = this.callbacks[message.invocationId]
                    if (callback != null) {
                        delete this.callbacks[message.invocationId]
                        callback(message)
                    }
                    break
                case MessageType.Ping:
                    // Don't care about pings
                    uni.setStorageSync('signalr_ping_time', new Date())
                    this.onPing()
                    break
                case MessageType.Close:
                    this.close({
                        reason: 'Server returned an error on close',
                    })
                    break
                default:
                    // Invalid message type handling
            }
        })
    }

    public send(functionName: string, argument: any): void {
        const args = []
        for (let _i = 1; _i < argument.length; _i++) {
            args[_i - 1] = argument[_i]
        }

        this.sendData({
            target: functionName,
            arguments: args,
            type: MessageType.Invocation,
            invocationId: this.invocationId.toString(),
        })
        this.invocationId++
    }

    public invoke(functionName: string, ...args: any[]): Promise<unknown> {
        const p = new Promise((resolve, reject) => {
            this.callbacks[this.invocationId] = function (message: any) {
                if (message.error) {
                    reject(new Error(message.error))
                } else {
                    resolve(message.result)
                }
            }

            this.sendData(
                {
                    target: functionName,
                    arguments: args,
                    type: MessageType.Invocation,
                    invocationId: this.invocationId.toString(),
                },
                () => {
                    //nothing
                },
                (e) => {
                    reject(e)
                }
            )
        })
        this.invocationId++
        return p
    }

    public invokeClientMethod(message: {
        type: number
        target: string
        arguments: any[]
        invocationId?: number
    }): void {
        //@ts-ignore
        const methods = this.methods[message.target]
        if (methods) {
            methods.apply(this, message.arguments)
            if (message.invocationId) {
                // This is not supported in v1. So we return an error to avoid blocking the server waiting for the response.
                const errormsg = 'Server requested a response, which is not supported in this version of the client.'
                this.close({
                    reason: errormsg,
                })
            }
        } else {
            // No client method found
        }
    }
}
