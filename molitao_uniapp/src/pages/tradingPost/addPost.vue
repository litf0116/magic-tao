<template>
    <view class="post-page">
        <view class="page-body">
            <!-- 选择分类 -->
            <view class="form-item">
                <text class="label">分类</text>
                <view class="category-list">
                    <checkbox-group @change="onCategoryChange">
                        <label v-for="item in categoryList" :key="item.categoryId" class="category-item">
                            <checkbox
                                :value="String(item.categoryId)"
                                :checked="selectedCategories.includes(String(item.categoryId))"
                            />
                            <text class="category-name">{{ item.name }}</text>
                        </label>
                    </checkbox-group>
                </view>
            </view>

            <!-- 标题输入 -->
            <view class="form-item">
                <text class="label"
                    >标题
                    <text style="color: red; margin-left: 5px">*</text>
                </text>
                <uv-input v-model="formData.title" placeholder="请输入标题"></uv-input>
            </view>

            <!-- 内容输入 -->
            <view class="form-item">
                <text class="label"
                    >内容
                    <text style="color: red; margin-left: 5px">*</text>
                </text>
                <view class="editor-box" style="height: 500px">
                    <sp-editor
                        editorId="editor"
                        :toolbar-config="toolbarConfig"
                        @input="inputOver"
                        @upinImage="upinImage"
                        @init="initEditor"
                    ></sp-editor>
                </view>
            </view>
        </view>

        <view class="page-footer">
            <button class="submit-btn" @tap="submitPost">发布</button>
        </view>
    </view>
</template>

<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import { onShow, onPullDownRefresh, onReachBottom, onShareTimeline, onLoad } from '@dcloudio/uni-app'
import api from '@/utils/api'
import cache from '@/utils/cache'
import base64 from '@/utils/base64'

const editorIns: any = ref(null)
const categoryList: any = ref([])

const selectedCategories = ref<string[]>([])
const formData = reactive({
    title: '',
    content: '',
    categoryId: '',
    postId: 0,
})

const toolbarConfig = ref({
    iconSize: '16px',
    iconColumns: 10,
    keys: [
        'header',
        'H1',
        'H2',
        'H3',
        'bold',
        'italic',
        'underline',
        'strike',
        'align',
        'alignLeft',
        'alignCenter',
        'color',
        'backgroundColor',
        'image',
    ],
})
//图片上传相关配置
const signature = ref('')
const bucketName = 'molitao'
const policy = ref('')
const actionUrl = computed(() => `https://v0.api.upyun.com/${bucketName}`)
const imgUrl = 'https://image.molitao.top'
const userName = 'molitao'

onLoad((option: any) => {
    const id = option.id
    //根据id查询数据详情
    if (id) {
        api.post.GetPostDetail(id).then((res: any) => {
            if (res.categoryId) {
                selectedCategories.value = res.categoryId.split(',').map(String)
            }
            Object.assign(formData, res)
            // API返回后，更新编辑器内容
            if (editorIns.value && formData.content) {
                preRender(formData.content)
            }
        })
    }
    getAuth()
    loadCategoryList()
})
//获取又拍云上传信息
const getAuth = () => {
    const cachedata = cache.getWithExpiry('upyun')
    if (cachedata && cachedata.policy && cachedata.signature) {
        signature.value = cachedata.signature
        policy.value = cachedata.policy
    } else {
        // @ts-ignore
        const date = new Date().toGMTString()
        const opts = {
            'save-key': `/{year}{mon}{day}/{random32}{.suffix}`,
            bucket: bucketName,
            expiration: Math.round(new Date().getTime() / 1000) + 43200, //12hour
            date: date,
        }
        policy.value = base64.encode(JSON.stringify(opts))
        const data = ['POST', '/' + bucketName, date, policy.value].join('&')
        // H5 开发模式下使用相对路径，其他模式使用配置的后端地址
        let baseApi = import.meta.env.VITE_APP_BASE_API || ''
        // #ifdef H5
        if (import.meta.env.DEV) {
            baseApi = ''
        }
        // #endif
        uni.request({
            method: 'GET',
            url: baseApi + api.upload.getSignature,
            data: { data: data },
            //请求成功后返回
            success: (res: any) => {
                signature.value = res.data.signature
                cache.setWithExpiry('upyun', { signature: signature.value, policy: policy.value }, 600)
            },
        })
    }
}
//加载分类列表
const loadCategoryList = () => {
    api.post.GetCategoryList().then((res: any) => {
        nextTick(() => {
            categoryList.value.push(...res)
        })
    })
}
// 分类选择
const categoryOptions = computed(() => categoryList.value)
const onCategoryChange = (e: any) => {
    const selectedValues = e.detail.value
    selectedCategories.value = selectedValues
}

// 提交表单
const submitPost = async () => {
    if (!formData.title.trim()) {
        uni.showToast({ title: '请输入标题', icon: 'none' })
        return
    }
    if (!formData.content.trim()) {
        uni.showToast({ title: '请输入内容', icon: 'none' })
        return
    }
    if (selectedCategories.value.length > 0) {
        formData.categoryId = selectedCategories.value.join(',')
    }
    // 这里处理提交逻辑
    if (formData.postId === 0) {
        api.post.Add(formData).then((res: any) => {
            Tips.success('发帖成功')
            setTimeout(() => {
                uni.navigateTo({
                    url: '/pages/tradingPost/index',
                })
            }, 1000)
        })
    } else {
        // 编辑时只提交需要更新的字段，排除服务器返回的冗余字段
        const editData = {
            title: formData.title,
            content: formData.content,
            categoryId: formData.categoryId,
            postId: formData.postId,
        }
        api.post.Edit(editData).then((res: any) => {
            Tips.success('编辑成功')
            setTimeout(() => {
                uni.navigateTo({
                    url: '/pages/tradingPost/index',
                })
            }, 1000)
        })
    }
}
/**------------------- 富文本配置开始-------------------------- */
//获取输入内容
function inputOver(e: any) {
    formData.content = e.html
}

//初始化编辑器
function initEditor(editor: any) {
    editorIns.value = editor
    preRender(formData.content)
}
//富文本框赋值
function preRender(data: any) {
    editorIns.value.setContents({
        html: data,
    })
}
//上传文件
function upinImage(tempFiles: any, editorCtx: any) {
    tempFiles.forEach(async (item: any) => {
        uni.showLoading({
            title: '上传中请稍后',
            mask: true,
        })
        uni.uploadFile({
            url: actionUrl.value,
            name: 'file',
            filePath: item.tempFilePath,
            formData: {
                authorization: `UPYUN ${userName}:${signature.value}`,
                policy: policy.value,
            },
            success(res: any) {
                const imageData: any = ref(JSON.parse(res.data))
                var tempImgUrl = imageData.value.url
                const url = `${imgUrl}${tempImgUrl}`
                // 最后插入图片
                editorCtx.insertImage({
                    src: url,
                    width: '80%', // 默认不建议铺满宽度100%，预留一点空隙以便用户编辑
                    success: function () {
                        uni.hideLoading()
                    },
                })
            },
            fail(err) {
                Tips.error('上传失败')
            },
        })
    })
}
/**------------------- 富文本配置结束-------------------------- */
</script>

<style>
.label {
    display: block;
    font-size: 28rpx;
    color: #333;
    margin-bottom: 10rpx;
}

.category-list {
    padding: 20rpx;
    background-color: #f8f8f8;
    border-radius: 8rpx;
}

.category-item {
    display: inline-flex;
    align-items: center;
    margin-right: 30rpx;
    margin-bottom: 20rpx;
}

.category-name {
    font-size: 28rpx;
    margin-left: 8rpx;
}

.post-page {
    min-height: 100vh;
    background: #fff;
    padding: 30rpx;
    box-sizing: border-box;
}

.page-header {
    padding: 20rpx 0;
    margin-bottom: 30rpx;
}

.title {
    font-size: 32rpx;
    font-weight: bold;
}

.form-item {
    margin-bottom: 30rpx;
}

.label {
    display: block;
    font-size: 28rpx;
    color: #333;
    margin-bottom: 10rpx;
}

.input,
.textarea,
.picker {
    width: 100%;
    border: 1rpx solid #eee;
    border-radius: 8rpx;
    padding: 20rpx;
    font-size: 28rpx;
    box-sizing: border-box;
}

.textarea {
    height: 200rpx;
}

.page-footer {
    margin-top: 40rpx;
    padding: 20rpx 0;
}

.submit-btn {
    width: 100%;
    height: 80rpx;
    line-height: 80rpx;
    text-align: center;
    border-radius: 40rpx;
    font-size: 28rpx;
    background: #007aff;
    color: #fff;
}
</style>
