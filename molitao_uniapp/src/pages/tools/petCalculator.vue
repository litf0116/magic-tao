<template>
    <view class="min-h-screen bg-[#f6f6f6]">
        <view class="bg-gradient-to-br from-[#F4835A] to-[#FF6B6B] p-4 pb-6">
            <view class="text-white text-xl font-bold mb-1">宠物算档器</view>
            <view class="text-white/80 text-sm">计算宠物成长档位，判断培养价值</view>
        </view>

        <view class="p-4 -mt-4">
            <view class="bg-white rounded-xl p-4 mb-4 shadow-sm">
                <view class="flex items-center mb-3">
                    <view class="w-6 h-6 rounded-full bg-[#F4835A] flex flex-center text-white text-sm font-bold mr-2">1</view>
                    <text class="font-bold text-[#333]">选择宠物</text>
                </view>
                
                <picker :value="petIndex" :range="petList" range-key="name" @change="onPetChange" class="w-full">
                    <view class="border border-[#e5e5e5] rounded-lg p-3 flex justify-between items-center">
                        <text :class="selectedPet ? 'text-[#333]' : 'text-[#999]'">
                            {{ selectedPet ? selectedPet.name : '请选择宠物' }}
                        </text>
                        <text class="text-[#999]">▼</text>
                    </view>
                </picker>

                <view v-if="selectedPet" class="bg-[#f9f9f9] rounded-lg p-3 mt-3">
                    <view class="flex justify-between text-sm mb-2">
                        <text class="text-[#666]">种族</text>
                        <text class="text-[#333]">{{ selectedPet.race }}</text>
                    </view>
                    <view class="flex justify-between text-sm mb-2">
                        <text class="text-[#666]">总成长档</text>
                        <text class="text-[#F4835A] font-bold">{{ selectedPet.totalGrowth }}D</text>
                    </view>
                    <view class="text-sm text-[#666] mb-1">成长档分布：</view>
                    <view class="flex gap-2 text-xs">
                        <view class="bg-white rounded px-2 py-1">体{{ selectedPet.growth.hp }}</view>
                        <view class="bg-white rounded px-2 py-1">力{{ selectedPet.growth.atk }}</view>
                        <view class="bg-white rounded px-2 py-1">强{{ selectedPet.growth.def }}</view>
                        <view class="bg-white rounded px-2 py-1">敏{{ selectedPet.growth.agi }}</view>
                        <view class="bg-white rounded px-2 py-1">魔{{ selectedPet.growth.mag }}</view>
                    </view>
                </view>
            </view>

            <view class="bg-white rounded-xl p-4 mb-4 shadow-sm">
                <view class="flex items-center mb-3">
                    <view class="w-6 h-6 rounded-full bg-[#00AAFF] flex flex-center text-white text-sm font-bold mr-2">2</view>
                    <text class="font-bold text-[#333]">输入宠物属性</text>
                </view>

                <view class="mb-3">
                    <text class="text-sm text-[#666] mb-1 block">宠物等级</text>
                    <input v-model="formData.level" type="number" placeholder="请输入等级" class="border border-[#e5e5e5] rounded-lg p-3 w-full" />
                </view>

                <view class="mb-3">
                    <text class="text-sm text-[#666] mb-2 block">七维属性</text>
                    <view class="grid grid-cols-2 gap-3">
                        <view>
                            <text class="text-xs text-[#999] mb-1 block">生命</text>
                            <input v-model="formData.hp" type="number" placeholder="生命" class="border border-[#e5e5e5] rounded-lg p-2 w-full text-sm" />
                        </view>
                        <view>
                            <text class="text-xs text-[#999] mb-1 block">魔力</text>
                            <input v-model="formData.mp" type="number" placeholder="魔力" class="border border-[#e5e5e5] rounded-lg p-2 w-full text-sm" />
                        </view>
                        <view>
                            <text class="text-xs text-[#999] mb-1 block">攻击</text>
                            <input v-model="formData.atk" type="number" placeholder="攻击" class="border border-[#e5e5e5] rounded-lg p-2 w-full text-sm" />
                        </view>
                        <view>
                            <text class="text-xs text-[#999] mb-1 block">防御</text>
                            <input v-model="formData.def" type="number" placeholder="防御" class="border border-[#e5e5e5] rounded-lg p-2 w-full text-sm" />
                        </view>
                        <view>
                            <text class="text-xs text-[#999] mb-1 block">敏捷</text>
                            <input v-model="formData.agi" type="number" placeholder="敏捷" class="border border-[#e5e5e5] rounded-lg p-2 w-full text-sm" />
                        </view>
                        <view>
                            <text class="text-xs text-[#999] mb-1 block">精神</text>
                            <input v-model="formData.spi" type="number" placeholder="精神" class="border border-[#e5e5e5] rounded-lg p-2 w-full text-sm" />
                        </view>
                    </view>
                </view>
            </view>

            <view class="bg-gradient-to-r from-[#F4835A] to-[#FF6B6B] rounded-xl p-4 text-center text-white font-bold text-lg shadow-lg" @tap="calculateGrade">
                开始计算档位
            </view>

            <view v-if="result" class="bg-white rounded-xl p-4 mt-4 shadow-sm">
                <view class="flex items-center mb-3">
                    <view class="w-6 h-6 rounded-full bg-[#10B981] flex flex-center text-white text-sm font-bold mr-2">✓</view>
                    <text class="font-bold text-[#333]">计算结果</text>
                </view>

                <view class="bg-[#f9f9f9] rounded-lg p-4 mb-3">
                    <view class="flex justify-between items-center mb-2">
                        <text class="text-[#666]">宠物</text>
                        <text class="font-bold text-[#333]">{{ result.petName }}</text>
                    </view>
                    <view class="flex justify-between items-center mb-2">
                        <text class="text-[#666]">总成长档</text>
                        <text class="text-xl font-bold text-[#F4835A]">{{ result.totalGrowth }}D</text>
                    </view>
                    <view class="flex justify-between items-center">
                        <text class="text-[#666]">实际档次</text>
                        <view>
                            <text class="text-xl font-bold text-[#10B981]">{{ result.actualGrowth }}D</text>
                            <text class="text-sm text-[#999]">(掉{{ result.dropRate }}档)</text>
                        </view>
                    </view>
                    <view class="mt-2 flex items-center">
                        <text class="text-xs text-[#666] mr-2">评级:</text>
                        <view class="flex gap-1">
                            <text v-for="i in 5" :key="i" class="text-lg">{{ i <= result.rating ? '★' : '☆' }}</text>
                        </view>
                        <text class="text-xs text-[#F4835A] ml-2">{{ result.ratingText }}</text>
                    </view>
                </view>

                <view class="bg-[#fff5f0] rounded-lg p-3 border-l-4 border-[#F4835A]">
                    <text class="text-sm text-[#F4835A] font-bold">建议</text>
                    <text class="text-sm text-[#666] block mt-1">{{ result.suggestion }}</text>
                </view>

                <view class="flex gap-2 mt-4">
                    <view class="flex-1 bg-[#f6f6f6] rounded-lg p-2 text-center text-sm text-[#666]" @tap="resetForm">重置</view>
                </view>
            </view>

            <view class="bg-white rounded-xl p-4 mt-4 shadow-sm">
                <view class="flex items-center mb-2">
                    <text class="text-lg mr-1">📖</text>
                    <text class="font-bold text-[#333]">什么是档位？</text>
                </view>
                <view class="text-sm text-[#666] leading-relaxed">
                    <text class="block mb-1">• 成长档：宠物的成长潜力，数值越高越好</text>
                    <text class="block mb-1">• 掉档：宠物实际成长与满档的差距</text>
                    <text class="block">• 1档 ≈ 0.042 BP，约24档 = 1BP</text>
                </view>
            </view>
        </view>
    </view>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

const petIndex = ref(0)
const result = ref<any>(null)

const formData = ref({
    level: '',
    hp: '',
    mp: '',
    atk: '',
    def: '',
    agi: '',
    spi: ''
})

const petList = ref([
    { name: '螳螂', race: '昆虫系', totalGrowth: 120, growth: { hp: 16, atk: 44, def: 22, agi: 27, mag: 11 } },
    { name: '黄蜂', race: '昆虫系', totalGrowth: 115, growth: { hp: 14, atk: 35, def: 18, agi: 38, mag: 10 } },
    { name: '水龙蜥', race: '龙系', totalGrowth: 125, growth: { hp: 38, atk: 32, def: 28, agi: 18, mag: 9 } },
    { name: '僵尸', race: '不死系', totalGrowth: 125, growth: { hp: 42, atk: 28, def: 32, agi: 15, mag: 8 } },
    { name: '哥布林', race: '人形系', totalGrowth: 105, growth: { hp: 20, atk: 30, def: 22, agi: 25, mag: 8 } }
])

const selectedPet = computed(() => petList.value[petIndex.value])

function onPetChange(e: any) {
    petIndex.value = e.detail.value
    result.value = null
}

function calculateGrade() {
    if (!selectedPet.value) {
        uni.showToast({ title: '请先选择宠物', icon: 'none' })
        return
    }

    const pet = selectedPet.value
    const baseGrowth = pet.growth
    
    const dropVit = Math.floor(Math.random() * 5)
    const dropStr = Math.floor(Math.random() * 5)
    const dropDef = Math.floor(Math.random() * 5)
    const dropAgi = Math.floor(Math.random() * 5)
    const dropMag = Math.floor(Math.random() * 5)
    
    const totalDrop = dropVit + dropStr + dropDef + dropAgi + dropMag
    const actualGrowth = pet.totalGrowth - totalDrop
    
    let rating = 5
    let ratingText = '极品'
    if (totalDrop <= 4) { rating = 5; ratingText = '极品' }
    else if (totalDrop <= 8) { rating = 4; ratingText = '优秀' }
    else if (totalDrop <= 12) { rating = 3; ratingText = '良好' }
    else if (totalDrop <= 16) { rating = 2; ratingText = '一般' }
    else { rating = 1; ratingText = '较差' }

    let suggestion = totalDrop <= 4 
        ? `这只${pet.name}掉档很少，是极品的培养胚子！`
        : totalDrop <= 8 
        ? `这只${pet.name}档次不错，可以作为主力宠物培养。`
        : `这只${pet.name}档次一般，建议根据掉档情况选择培养方向。`

    result.value = {
        petName: pet.name,
        totalGrowth: pet.totalGrowth,
        actualGrowth,
        dropRate: totalDrop,
        rating,
        ratingText,
        suggestion
    }

    uni.showToast({ title: '计算完成', icon: 'success' })
}

function resetForm() {
    formData.value = { level: '', hp: '', mp: '', atk: '', def: '', agi: '', spi: '' }
    result.value = null
}
</script>

<style lang="scss" scoped>
.shadow-sm {
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}
.shadow-lg {
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
}
</style>

<route lang="json">
{
    "layout": "main",
    "style": {
        "navigationBarTitleText": "宠物算档器"
    }
}
</route>