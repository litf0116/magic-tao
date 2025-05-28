<template>
    <el-dialog v-model="dialogVisible" title="个人信息" width="500" destroy-on-close append-to-body>
        <div class="flex flex-col space-y-4 max-h-50vh">
            <div v-if="profile" class="profile-card">
                <div class="profile-card-title">
                    <div class="profile-name">
                        <div class="i-mdi:account-box size-12 text-blue-600"></div>
                        <div>{{ profile.name }}</div>
                    </div>
                    <div class="profile-avatar">
                        <img :src="profile.headImgUrl" />
                    </div>
                </div>
                <div class="friend-info">
                    <div class="info-name">用户编号</div>
                    <div class="info-text">{{ profile.id }}</div>
                </div>
                <div v-if="profile.qq" class="friend-info">
                    <div class="info-name">QQ</div>
                    <div class="info-text flex items-center" @click.stop="copyText(profile.qq)">
                        {{ profile.qq }}
                        <div class="i-mdi:content-copy ml-2 size-4"></div>
                    </div>
                </div>
                <div v-if="profile.wx" class="friend-info">
                    <div class="info-name">微信</div>
                    <div class="info-text flex items-center" @click.stop="copyText(profile.wx)">
                        {{ profile.wx }}
                        <div class="i-mdi:content-copy ml-2 size-4"></div>
                    </div>
                </div>
                <!-- <div class="friend-info">
                    <div class="info-name">手 机</div>
                    <div class="info-text">{{ profile.phoneNumber }}</div>
                </div> -->
                <!-- <div class="flex flex-center space-x-10 p-4">
                    <el-button type="primary" @click="privateChat">发消息</el-button>
                    <el-button @click="profile.friend = null">关闭</el-button>
                </div> -->
            </div>
        </div>
    </el-dialog>
</template>

<script setup lang="ts">
import { UserDto } from '@/api/appService'
import api from '@/api'
import { copyText } from '@/composables'

const dialogVisible = ref(false)
const profile = ref<UserDto | null>(null)
const show = (e: boolean, userId: number) => {
    if (!userId) return
    profile.value = null
    dialogVisible.value = e
    if (e) {
        fetch(userId)
    }
}

function fetch(userId: number) {
    api.user.get({ id: userId }).then((res) => {
        // console.log(res)
        profile.value = res
    })
}

defineExpose({
    show,
})
</script>

<style scoped>
.profile-card {
    height: 100%;
    display: flex;
    flex-direction: column;
}

.profile-card-title {
    flex: 1;
    border-bottom: 1px solid #eeeeee;
    display: flex;
    justify-content: space-around;
    align-items: center;
}

.profile-name {
    width: 300px;
    font-size: 18px;
    display: flex;
    align-items: center;
}

.icon-zhanghu {
    font-size: 26px;
    color: #eeeeee;
    margin-right: 10px;
}

.profile-avatar {
    width: 80px;
}

.profile-avatar img {
    width: 80px;
    height: 80px;
    border-radius: 10%;
}

.friend-info {
    padding: 10px 30px;
    display: flex;
    justify-content: space-around;
    text-align: left;
    font-size: 14px;
    line-height: 25px;
}

.info-name {
    width: 100px;
}

.info-text {
    width: 200px;
}
</style>
