/**
 * 状态转换逻辑测试文件
 * 用于验证 convertAuctionPayload 函数的状态转换功能
 */

// 导入需要测试的函数
import { convertAuctionPayload } from './propertyConverter'

// 测试用例
const testCases = [
    // 数字状态值测试
    { input: { status: 0 }, expected: '草稿' },
    { input: { status: 1 }, expected: '上架' },
    { input: { status: 2 }, expected: '拍卖中' },
    { input: { status: 4 }, expected: '已成交' },
    { input: { status: 8 }, expected: '交易成功' },
    { input: { status: 16 }, expected: '卖家失约' },
    { input: { status: 32 }, expected: '买家失约' },
    { input: { status: 128 }, expected: '交易关闭' },

    // 字符串状态值测试
    { input: { status: '草稿' }, expected: '草稿' },
    { input: { status: '上架' }, expected: '上架' },
    { input: { status: '拍卖中' }, expected: '拍卖中' },
    { input: { status: '已成交' }, expected: '已成交' },
    { input: { status: '交易成功' }, expected: '交易成功' },
    { input: { status: '卖家失约' }, expected: '卖家失约' },
    { input: { status: '买家失约' }, expected: '买家失约' },
    { input: { status: '交易关闭' }, expected: '交易关闭' },

    // 边界情况测试
    { input: { status: null }, expected: '' },
    { input: { status: undefined }, expected: '' },
    { input: { status: 999 }, expected: '999' }, // 未知数字状态
    { input: { status: '未知状态' }, expected: '未知状态' },

    // 完整payload测试
    {
        input: {
            Status: 4,
            Name: '测试商品',
            FinalPrice: 1000,
            DealUserName: '测试用户',
        },
        expected: '已成交',
    },
]

// 运行测试
function runTests() {
    console.log('开始测试状态转换逻辑...')

    let passedTests = 0
    const totalTests = testCases.length

    testCases.forEach((testCase, index) => {
        try {
            const result = convertAuctionPayload(testCase.input)
            const actualStatus = result.status

            if (actualStatus === testCase.expected) {
                console.log(`✅ 测试 ${index + 1} 通过: ${JSON.stringify(testCase.input)} => ${actualStatus}`)
                passedTests++
            } else {
                console.error(
                    `❌ 测试 ${index + 1} 失败: ${JSON.stringify(testCase.input)} => 期望 ${
                        testCase.expected
                    }, 实际 ${actualStatus}`
                )
            }
        } catch (error) {
            console.error(`❌ 测试 ${index + 1} 异常: ${JSON.stringify(testCase.input)} => ${error}`)
        }
    })

    console.log(`\n测试完成: ${passedTests}/${totalTests} 通过`)

    if (passedTests === totalTests) {
        console.log('🎉 所有测试通过！状态转换逻辑工作正常。')
    } else {
        console.log('⚠️ 部分测试失败，请检查状态转换逻辑。')
    }
}

// 如果在浏览器环境中，可以调用 runTests() 来执行测试
// 在开发环境中，可以在控制台中运行: runTests()

export { runTests }
