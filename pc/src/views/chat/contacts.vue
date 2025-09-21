<template>
    <div class="contact">
        <div class="contact-left max-h-700px overflow-y-scroll">
            <template v-if="chatStore.friends0 && chatStore.friends0.length">
                <div class="contact-list-title">好友申请</div>
                <div class="friend-list">
                    <div
                        v-for="(friend, idx) in chatStore.friends0"
                        :key="idx"
                        class="friend-item"
                        :class="{ actived: profile.friend && profile.friend.id === friend.id }"
                    >
                        <div class="friend-avatar">
                            <img :src="convertImageUrl(friend.headImgUrl)" />
                        </div>
                        <div class="friend">
                            <div class="friend-name">{{ friend.name }}</div>
                            <!-- <div class="friend-mail">{{ friend.email }}</div> -->
                            <div>
                                <el-button size="small" type="success" @click="aggree(friend.id!, true)">同意</el-button
                                ><el-button size="small" @click="aggree(friend.id!, false)">拒绝</el-button>
                            </div>
                        </div>
                    </div>
                </div>
            </template>
            <div class="contact-list-title">好友</div>

            <div class="friend-list">
                <div class="h-8 mx-2">
                    <el-input v-model="filterText" size="small" class="border-0" placeholder="筛选" clearable />
                </div>
                <div
                    v-for="(friend, idx) in (chatStore.friends || []).filter((x) => x.name.indexOf(filterText) > -1)"
                    :key="idx"
                    class="friend-item"
                    :class="{ actived: profile.friend && profile.friend.id === friend.id }"
                    @click="showFriendProfile(friend)"
                >
                    <div class="friend-avatar">
                        <img :src="convertImageUrl(friend.headImgUrl)" />
                    </div>
                    <div class="friend">
                        <div class="friend-name">{{ friend.name }}</div>
                        <!-- <div class="friend-mail">{{ friend.email }}</div> -->
                    </div>
                </div>
            </div>
        </div>
        <div class="contact-main">
            <div v-if="!profile.friend"></div>
            <div v-else class="profile-card">
                <div class="profile-card-title">
                    <div class="profile-name">
                        <div class="i-mdi:account-box size-12 text-blue-600"></div>
                        <div>{{ profile.friend.name }}</div>
                    </div>
                    <div class="profile-avatar">
                        <img :src="convertImageUrl(profile.friend.headImgUrl)" />
                    </div>
                </div>
                <div class="friend-info">
                    <div class="info-name">QQ</div>
                    <div class="info-text">{{ profile.friend.qq }}</div>
                </div>
                <div class="friend-info">
                    <div class="info-name">微信</div>
                    <div class="info-text">{{ profile.friend.wx }}</div>
                </div>
                <div class="friend-info">
                    <div class="info-name">手 机</div>
                    <div class="info-text">{{ profile.friend.phoneNumber }}</div>
                </div>
                <div class="flex flex-center space-x-10 p-4">
                    <el-button type="primary" @click="privateChat">发消息</el-button>
                    <el-button @click="profile.friend = null">关闭</el-button>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import api from '@/api'
import { UserDtoBase } from '@/api/appService'
import { convertImageUrl } from '@/utils/imageUrlConverter'

const chatStore = useChatStore()
const router = useRouter()

const profile = ref({
    friend: null as UserDtoBase | null,
    group: null,
})
const filterText = ref('')
onMounted(() => {
    fetchFriends()
})

function fetchFriends() {
    chatStore.getUserFriends(true)
    chatStore.getUserFriends(false)
}

function showFriendProfile(friend: UserDtoBase) {
    profile.value.group = null
    profile.value.friend = friend
}

function privateChat() {
    router.replace({
        path: '/chat/index/privatechat/' + profile.value.friend!.id,
        query: {
            name: profile.value.friend!.name,
            avatar: convertImageUrl(profile.value.friend!.headImgUrl),
        },
    })
}
// 注入母版页方法
const layoutMethods: any = inject('layoutMethods')

function aggree(id: number, s: boolean) {
    api.userFriend.agree({ id: id, status: s }).then(() => {
        fetchFriends()
        //调用母版页方法
        layoutMethods?.getUserFriendCount()
    })
}
</script>

<style scoped>
.contact {
    width: 100%;
    height: 100%;
    display: flex;
    background: #f7f7f7;
    color: #333333;
}

.contact-left {
    width: 220px;
    height: 100%;
    border-right: #dbd6d6 1px solid;
}

.contact-list-title {
    margin: 10px 20px;
    font-size: 14px;
}

.friend-list {
    display: flex;
    flex-direction: column;
}

.group-list {
    display: flex;
    flex-direction: column;
}

.actived {
    background: #ffffff;
    border-radius: 10px;
    box-shadow: 0 1px 6px 0 rgba(0, 0, 0, 0.1);
}

.friend-item {
    display: flex;
    padding: 5px 10px;
}

.friend-avatar img {
    width: 40px;
    height: 40px;
    border-radius: 10%;
    margin-left: 10px;
}

.friend {
    width: 65%;
    margin: 0;
    display: flex;
    flex-direction: column;
    text-align: left;
    padding-left: 10px;
}

.friend-name {
    margin: 0;
    font-size: 14px;
    font-weight: 400;
}

.friend-mail {
    line-height: 21px;
    color: #888888;
}

.group-item {
    display: flex;
    padding: 5px 10px;
    cursor: pointer;
    align-items: center;
}

.group-avatar {
    width: 40px;
    height: 40px;
    margin-left: 10px;
}

.group-avatar img {
    width: 40px;
    height: 40px;
}

.group-name {
    margin-left: 10px;
    width: 160px;
    text-align: left;
    font-size: 14px;
    line-height: 40px;
}

.contact-main {
    flex: 1;
    background: #ffffff;
}

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

.group-profile-name {
    font-size: 18px;
    padding: 20px 70px;
    border-bottom: 1px solid #eeeeee;
}

.group-members {
    width: 400px;
    min-height: 200px;
    margin: 20px auto;
    display: flex;
    flex-wrap: wrap;
    align-content: flex-start;
}

.group-members .member {
    width: 25%;
    display: flex;
    flex-direction: column;
    align-items: center;
}

.group-members .member-avatar {
    width: 58px;
    margin-top: 20px;
    border-radius: 5%;
}

.group-members .member-name {
    color: gray;
    margin-top: 10px;
    font-size: 12px;
}

.button-box {
    padding: 40px 0;
}

.card-button {
    background: #eeeeee;
    color: #000000;
    font-size: 14px;
    border: none;
    display: flex;
    width: 120px;
    height: 35px;
    cursor: pointer;
    border-radius: 5px;
    margin: 0 auto;
    align-items: center;
    justify-content: center;
}
</style>
