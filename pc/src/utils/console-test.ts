/**
 * 测试文件：验证生产环境中console.log是否被移除
 * 这个文件仅用于测试，可以在测试完成后删除
 */

// 这些console.log在生产环境中应该被移除
console.log('这是测试日志 - 生产环境应该被移除')
console.warn('这是测试警告 - 生产环境应该被移除')
console.info('这是测试信息 - 生产环境应该被移除')
console.debug('这是测试调试信息 - 生产环境应该被移除')

// 这些debugger语句在生产环境中应该被移除
debugger

// 测试函数中的console.log
export function testConsoleLog() {
    console.log('函数中的测试日志 - 生产环境应该被移除')
    return '测试完成'
}

// 测试异步函数中的console.log
export async function testAsyncConsoleLog() {
    console.log('异步函数中的测试日志 - 生产环境应该被移除')
    return '异步测试完成'
}

// 测试条件语句中的console.log
export function testConditionalConsoleLog(condition: boolean) {
    if (condition) {
        console.log('条件为真时的测试日志 - 生产环境应该被移除')
    } else {
        console.log('条件为假时的测试日志 - 生产环境应该被移除')
    }
}
