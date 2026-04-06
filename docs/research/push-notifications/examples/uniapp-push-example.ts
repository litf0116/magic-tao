// UniApp 推送通知示例代码
// 支持平台：iOS、Android

/**
 * 推送通知服务示例
 */
class PushNotificationService {
  private clientId: string = ''
  private isReady: boolean = false

  /**
   * 示例 1: 初始化推送服务
   */
  async init(): Promise<void> {
    console.log('初始化推送服务...')

    // #ifdef APP-PLUS
    return new Promise((resolve, reject) => {
      const pushManager = uni.getPushManager()

      pushManager.on('register', (result: any) => {
        console.log('✅ 推送注册成功:', result)
        this.clientId = result.cid
        this.isReady = true

        // 上传到后端
        this.uploadClientId(result.cid)
        resolve()
      })

      pushManager.on('message', (result: any) => {
        console.log('📨 收到推送消息:', result)
        this.handleMessage(result)
      })

      pushManager.on('click', (result: any) => {
        console.log('👆 点击推送消息:', result)
        this.handleClick(result)
      })

      pushManager.on('error', (error: any) => {
        console.error('❌ 推送错误:', error)
        reject(error)
      })

      // 启动推送
      pushManager.start()
    })
    // #endif

    // #ifndef APP-PLUS
    console.warn('⚠️ 推送仅在 APP 环境下可用')
    return Promise.resolve()
    // #endif
  }

  /**
   * 示例 2: 上传 Client ID 到后端
   */
  private async uploadClientId(clientId: string): Promise<void> {
    try {
      const platform = uni.getSystemInfoSync().platform === 'ios' ? 'iOS' : 'Android'

      const response = await uni.request({
        url: `${import.meta.env.VITE_API_URL}/api/push/device-token/register`,
        method: 'POST',
        header: {
          'Authorization': `Bearer ${uni.getStorageSync('token')}`
        },
        data: {
          deviceToken: clientId,
          platform: platform,
          deviceInfo: JSON.stringify(uni.getSystemInfoSync())
        }
      })

      console.log('✅ Client ID 上传成功')
    } catch (error) {
      console.error('❌ Client ID 上传失败:', error)
    }
  }

  /**
   * 示例 3: 处理接收到的消息
   */
  private handleMessage(message: any): void {
    const { title, content, extras } = message

    // 展示本地通知
    uni.showNotification({
      title: title || '新消息',
      content: content || '',
      success: () => {
        console.log('✅ 通知已展示')
      }
    })

    // 触发自定义事件
    uni.$emit('pushMessage', message)

    // 根据消息类型处理
    if (extras?.type) {
      this.handleMessageType(extras.type, extras)
    }
  }

  /**
   * 示例 4: 处理消息点击
   */
  private handleClick(result: any): void {
    const { extras } = result

    // 根据消息类型导航到对应页面
    if (extras?.type) {
      this.navigateToPage(extras)
    }
  }

  /**
   * 示例 5: 根据消息类型处理
   */
  private handleMessageType(type: string, data: any): void {
    console.log('处理消息类型:', type, data)

    switch (type) {
      case 'bid_placed':
        // 出价成功
        uni.showToast({
          title: '出价成功',
          icon: 'success'
        })
        break

      case 'new_bid':
        // 新出价提醒
        uni.showModal({
          title: '新出价提醒',
          content: '您关注的拍品刚刚有新出价',
          confirmText: '查看',
          success: (res) => {
            if (res.confirm && data.auctionId) {
              uni.navigateTo({
                url: `/pages/auction/detail?id=${data.auctionId}`
              })
            }
          }
        })
        break

      case 'outbid':
        // 被超出价
        uni.showModal({
          title: '出价被超越',
          content: '您的出价已被其他买家超越',
          confirmText: '查看',
          success: (res) => {
            if (res.confirm && data.auctionId) {
              uni.navigateTo({
                url: `/pages/auction/detail?id=${data.auctionId}`
              })
            }
          }
        })
        break

      case 'auction_ending':
        // 拍卖即将结束
        uni.showModal({
          title: '拍卖即将结束',
          content: '您关注的拍品即将结束拍卖',
          confirmText: '查看',
          success: (res) => {
            if (res.confirm && data.auctionId) {
              uni.navigateTo({
                url: `/pages/auction/detail?id=${data.auctionId}`
              })
            }
          }
        })
        break

      case 'auction_ended':
        // 拍卖结束
        uni.showModal({
          title: '拍卖已结束',
          content: '您关注的拍品拍卖已结束',
          confirmText: '查看结果',
          success: (res) => {
            if (res.confirm && data.auctionId) {
              uni.navigateTo({
                url: `/pages/auction/result?id=${data.auctionId}`
              })
            }
          }
        })
        break

      default:
        console.log('未知消息类型:', type)
        break
    }
  }

  /**
   * 示例 6: 导航到对应页面
   */
  private navigateToPage(data: any): void {
    console.log('导航到页面:', data)

    switch (data.type) {
      case 'bid_placed':
      case 'new_bid':
      case 'outbid':
        if (data.auctionId) {
          uni.navigateTo({
            url: `/pages/auction/detail?id=${data.auctionId}`
          })
        }
        break

      case 'auction_ended':
        if (data.auctionId) {
          uni.navigateTo({
            url: `/pages/auction/result?id=${data.auctionId}`
          })
        }
        break

      default:
        break
    }
  }

  /**
   * 示例 7: 设置别名（用于定向推送）
   */
  async setAlias(alias: string): Promise<void> {
    console.log('设置别名:', alias)

    // #ifdef APP-PLUS
    const pushManager = uni.getPushManager()
    await pushManager.setAlias(alias)
    console.log('✅ 别名设置成功')
    // #endif
  }

  /**
   * 示例 8: 设置标签（用于分组推送）
   */
  async setTags(tags: string[]): Promise<void> {
    console.log('设置标签:', tags)

    // #ifdef APP-PLUS
    const pushManager = uni.getPushManager()
    await pushManager.setTags(tags)
    console.log('✅ 标签设置成功')
    // #endif
  }

  /**
   * 示例 9: 清除所有通知
   */
  async clearAllNotifications(): Promise<void> {
    console.log('清除所有通知')

    // #ifdef APP-PLUS
    const pushManager = uni.getPushManager()
    await pushManager.clearAllNotifications()
    console.log('✅ 通知已清除')
    // #endif
  }

  /**
   * 示例 10: 获取 Client ID
   */
  getClientId(): string {
    return this.clientId
  }

  /**
   * 示例 11: 检查是否已初始化
   */
  isPushReady(): boolean {
    return this.isReady
  }

  /**
   * 示例 12: 在页面中监听推送消息
   */
  static setupPageListener(callback: (message: any) => void): () => void {
    const handler = (message: any) => {
      callback(message)
    }

    uni.$on('pushMessage', handler)

    // 返回清理函数
    return () => {
      uni.$off('pushMessage', handler)
    }
  }
}

/**
 * 在 Vue 组件中使用示例
 */
export default {
  data() {
    return {
      pushService: new PushNotificationService(),
      auctionId: '',
      bidAmount: 10000
    }
  },

  async onShow() {
    // 初始化推送服务
    await this.pushService.init()

    // 设置用户别名
    const userId = uni.getStorageSync('userId')
    if (userId) {
      await this.pushService.setAlias(userId.toString())

      // 设置标签
      await this.pushService.setTags(['auction', 'bids'])
    }

    // 监听推送消息
    this.cleanupListener = PushNotificationService.setupPageListener((message) => {
      this.handlePushMessage(message)
    })
  },

  onHide() {
    // 清理监听器
    if (this.cleanupListener) {
      this.cleanupListener()
    }
  },

  methods: {
    /**
     * 处理推送消息
     */
    handlePushMessage(message: any) {
      console.log('收到推送消息:', message)

      const { type, auctionId } = message.extras || {}

      // 如果消息与当前拍卖相关，刷新页面数据
      if (auctionId === this.auctionId) {
        this.loadAuctionDetail()
      }
    },

    /**
     * 加载拍卖详情
     */
    async loadAuctionDetail() {
      try {
        const response = await uni.request({
          url: `${import.meta.env.VITE_API_URL}/api/auction/${this.auctionId}`,
          method: 'GET',
          header: {
            'Authorization': `Bearer ${uni.getStorageSync('token')}`
          }
        })

        if (response.data.success) {
          this.auctionDetail = response.data.data
        }
      } catch (error) {
        console.error('加载拍卖详情失败:', error)
      }
    },

    /**
     * 出价
     */
    async placeBid() {
      try {
        await uni.request({
          url: `${import.meta.env.VITE_API_URL}/api/auction/bid`,
          method: 'POST',
          header: {
            'Authorization': `Bearer ${uni.getStorageSync('token')}`
          },
          data: {
            auctionId: this.auctionId,
            amount: this.bidAmount
          }
        })

        uni.showToast({
          title: '出价成功',
          icon: 'success'
        })

        // 刷新拍卖详情
        await this.loadAuctionDetail()
      } catch (error) {
        uni.showToast({
          title: '出价失败',
          icon: 'none'
        })
      }
    }
  }
}

/**
 * 在 App.vue 中初始化推送服务
 */
export const appInit = () => {
  onLaunch(() => {
    console.log('App Launch')

    // 初始化推送服务
    const pushService = new PushNotificationService()
    pushService.init().catch(error => {
      console.error('初始化推送服务失败:', error)
    })
  })
}

/**
 * 导出推送服务实例
 */
export const pushService = new PushNotificationService()
