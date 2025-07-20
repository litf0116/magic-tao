// 这个文件用于测试生产环境构建时console.log是否被移除
// 在生产环境构建后，这些console.log应该被自动移除

export function testConsoleRemoval() {
    console.log('这个日志在生产环境应该被移除')
    console.warn('这个警告在生产环境应该被移除')
    console.info('这个信息在生产环境应该被移除')

    // 测试条件语句中的console.log
    if (import.meta.env.DEV) {
        console.log('开发环境日志')
    } else {
        console.log('生产环境日志 - 应该被移除')
    }

    // 测试函数中的console.log
    const testFunction = () => {
        console.log('函数中的日志 - 应该被移除')
    }

    testFunction()
}

// 导出测试函数
export default testConsoleRemoval
