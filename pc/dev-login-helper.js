/**
 * 开发调试登录助手
 * 用于在开发环境中快速获取用户token并设置到localStorage
 */

const API_BASE = 'http://127.0.0.1:12580';

/**
 * 为指定用户生成token
 * @param {number} userId 用户ID
 * @returns {Promise<Object>} token信息
 */
async function generateTokenForUser(userId) {
    try {
        console.log(`🚀 开始为用户 ${userId} 生成token...`);
        console.log(`📡 API地址: ${API_BASE}/api/TokenAuth/GenerateTokenForUser`);

        const response = await fetch(`${API_BASE}/api/TokenAuth/GenerateTokenForUser`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ userId: userId })
        });

        console.log(`📡 HTTP状态: ${response.status} ${response.statusText}`);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        console.log('📊 API响应:', data);

        if (!data.success) {
            console.error('❌ API返回失败:', data.error);
            throw new Error(data.error?.message || '生成token失败');
        }

        console.log('✅ Token生成成功，返回result:', data.result);
        return data.result;
    } catch (error) {
        console.error('❌ 生成token失败:', error);
        throw error;
    }
}

/**
 * 设置用户token到localStorage和Cookie
 * @param {Object} tokenData token数据
 */
function setUserToken(tokenData) {
    console.log('🔧 开始设置token...');
    console.log('📊 接收到的tokenData:', tokenData);
    console.log('📋 tokenData.accessToken类型:', typeof tokenData.accessToken);
    console.log('📋 tokenData.accessToken长度:', tokenData.accessToken?.length || 0);

    // 检查accessToken是否存在
    if (!tokenData.accessToken) {
        console.error('❌ accessToken为空或未定义！');
        console.error('tokenData对象:', JSON.stringify(tokenData, null, 2));
        return;
    }

    // 设置token到localStorage (与cookies.ts中的机制保持一致)
    localStorage.setItem('token', tokenData.accessToken);

    // 设置token到cookie (与cookies.ts中的机制保持一致)
    document.cookie = `token=${tokenData.accessToken}; path=/; max-age=${tokenData.expireInSeconds}`;

    // 设置用户信息到localStorage (与userStore中的机制保持一致)
    const userInfo = {
        id: tokenData.userId,
        userName: tokenData.userName,
        emailAddress: tokenData.emailAddress || '',
        // 其他用户信息会通过getUserInfo()获取
    };
    localStorage.setItem('user', JSON.stringify(userInfo));

    // 验证存储
    const storedToken = localStorage.getItem('token');
    const storedUser = localStorage.getItem('user');

    console.log(`✅ 已设置用户 ${tokenData.userName} (ID: ${tokenData.userId}) 的token到localStorage和Cookie`);
    console.log(`- localStorage token: ${storedToken ? storedToken.substring(0, 20) + '...' : '❌ 未设置'}`);
    console.log(`- localStorage user: ${storedUser ? storedUser.substring(0, 50) + '...' : '❌ 未设置'}`);
    console.log(`- 实际存储的token长度: ${storedToken?.length || 0}`);

    if (!storedToken || storedToken === '') {
        console.error('❌ Token存储失败！');
        console.error('尝试设置的token:', tokenData.accessToken);
        console.error('localStorage实际值:', localStorage.getItem('token'));
    }
}

/**
 * 验证token是否正确设置
 * @returns {boolean}
 */
function verifyTokenStorage() {
    const localStorageToken = localStorage.getItem('token');
    const userInfo = localStorage.getItem('user');
    const cookies = document.cookie.split(';').map(c => c.trim());
    const tokenCookie = cookies.find(c => c.startsWith('token='));

    console.log('🔍 Token存储验证:');
    console.log(`- localStorage token: ${localStorageToken ? '✅ 已设置' : '❌ 未设置'}`);
    console.log(`- localStorage user: ${userInfo ? '✅ 已设置' : '❌ 未设置'}`);
    console.log(`- Cookie token: ${tokenCookie ? '✅ 已设置' : '❌ 未设置'}`);

    return !!(localStorageToken && userInfo && tokenCookie);
}

/**
 * 开发调试登录
 * @param {number} userId 用户ID
 */
async function devLogin(userId) {
    try {
        console.log(`正在为用户 ${userId} 生成token...`);
        const tokenData = await generateTokenForUser(userId);
        setUserToken(tokenData);

        console.log('✅ 登录成功！', tokenData);

        // 验证token存储
        const isTokenStored = verifyTokenStorage();
        if (isTokenStored) {
            console.log('✅ Token存储验证通过！');
        } else {
            console.error('❌ Token存储验证失败！');
        }

        // 3秒后刷新页面
        setTimeout(() => {
            console.log('正在刷新页面...');
            window.location.reload();
        }, 3000);

    } catch (error) {
        console.error('❌ 登录失败:', error.message);
        alert(`登录失败: ${error.message}`);
    }
}

// 导出到全局作用域，方便在控制台中使用
window.devLogin = devLogin;
window.generateTokenForUser = generateTokenForUser;
window.verifyTokenStorage = verifyTokenStorage;

// 在控制台显示使用说明
console.log(`
🔧 开发调试登录助手已加载！

使用方法：
1. 在控制台中输入: devLogin(1)   // 登录用户1 (如果存在)
2. 在控制台中输入: devLogin(2)   // 登录用户2 (admin)
3. 在控制台中输入: devLogin(3)   // 登录用户3 (如果存在)
4. 在控制台中输入: devLogin(14)  // 登录用户14 (oFzSV6st7nn8ZeoTEQqbveyjfMAU)

可用用户：
- 用户2: admin - 管理员用户
- 用户14: oFzSV6st7nn8ZeoTEQqbveyjfMAU - 多角色用户 (Admin, AuctionManager, AuctionUser, Manager)

可用函数：
- devLogin(userId) - 快速登录指定用户
- generateTokenForUser(userId) - 仅生成token，不设置到localStorage
- verifyTokenStorage() - 验证token是否正确设置到localStorage和Cookie

使用示例：
- verifyTokenStorage() // 检查当前登录状态

注意：此工具仅在开发环境中使用！
`);