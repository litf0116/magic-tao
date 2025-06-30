var auth = window.auth || {};

// 设置认证相关的常量
auth.tokenCookieName = "aspnetcore.authauth";
auth.loginUrl = "/api/TokenAuth/Authenticate";
auth.logoutUrl = "/api/TokenAuth/LogOut";

// 登出函数
auth.logout = function (callback) {
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (xhr.readyState === XMLHttpRequest.DONE) {
            if (xhr.status === 200) {
                var res = xhr.response;
                // 清除本地存储的登录状态和token
                localStorage.removeItem("auth.hasLogin");
                localStorage.removeItem("token");
                callback();
            } else {
                console.warn("Logout failed !");
            }
        }
    };
    //配置请求
    xhr.open('Get', auth.logoutUrl, true);
    // 设置请求头
    xhr.setRequestHeader('Content-type', 'application/json; charset=utf-8');
    // 设置响应类型
    xhr.responseType = 'json';
    // 发送请求
    xhr.send();
}

// 检查是否已登录
auth.hasLogin = function () {
    return localStorage.getItem("auth.hasLogin");
}

// 打开认证对话框
auth.openAuthDialog = function (loginCallback) {
    auth.closeAuthDialog();

    // 创建对话框的DOM结构
    var authAuthDialog = document.createElement('div');
    authAuthDialog.className = 'dialog-ux';
    authAuthDialog.id = 'auth-auth-dialog';
    document.getElementsByClassName("swagger-ui")[1].appendChild(authAuthDialog);

    // 创建背景遮罩
    var backdropUx = document.createElement('div');
    backdropUx.className = 'backdrop-ux';
    authAuthDialog.appendChild(backdropUx);

    // 创建模态框
    var modalUx = document.createElement('div');
    modalUx.className = 'modal-ux';
    authAuthDialog.appendChild(modalUx);

    // 创建模态框对话框
    var modalDialogUx = document.createElement('div');
    modalDialogUx.className = 'modal-dialog-ux';
    modalUx.appendChild(modalDialogUx);

    // 创建模态框内部容器
    var modalUxInner = document.createElement('div');
    modalUxInner.className = 'modal-ux-inner';
    modalDialogUx.appendChild(modalUxInner);

    // 创建模态框头部
    var modalUxHeader = document.createElement('div');
    modalUxHeader.className = 'modal-ux-header';
    modalUxInner.appendChild(modalUxHeader);
    var modalHeader = document.createElement('h3');
    modalHeader.innerText = 'Authorize';
    modalUxHeader.appendChild(modalHeader);

    // 创建模态框内容
    var modalUxContent = document.createElement('div');
    modalUxContent.className = 'modal-ux-content';
    modalUxInner.appendChild(modalUxContent);
    modalUxContent.onkeydown = function (e) {
        if (e.keyCode === 13) {
            //当用户在认证模态框中按下回车键时尝试登录
            auth.login(loginCallback);
        }
    };

    // 创建输入框
    createInput(modalUxContent, 'account', '用户名或电子邮件地址');
    createInput(modalUxContent, 'password', '密码', 'password');

    // 创建按钮容器
    var authBtnWrapper = document.createElement('div');
    authBtnWrapper.className = 'auth-btn-wrapper';
    modalUxContent.appendChild(authBtnWrapper);

    // 创建关闭按钮
    var closeButton = document.createElement('button');
    closeButton.className = 'btn modal-btn auth btn-done button';
    closeButton.innerText = 'Close';
    closeButton.style.marginRight = '5px';
    closeButton.onclick = auth.closeAuthDialog;
    authBtnWrapper.appendChild(closeButton);

    // 创建登录按钮
    var authorizeButton = document.createElement('button');
    authorizeButton.className = 'btn modal-btn auth authorize button';
    authorizeButton.innerText = 'Login';
    authorizeButton.onclick = function () {
        auth.login(loginCallback);
    };
    authBtnWrapper.appendChild(authorizeButton);
}

// 关闭认证对话框
auth.closeAuthDialog = function () {
    if (document.getElementById('auth-auth-dialog')) {
        document.getElementsByClassName("swagger-ui")[1].removeChild(document.getElementById('auth-auth-dialog'));
    }
}

// 登录函数
auth.login = function (callback) {
    var usernameOrEmailAddress = document.getElementById('account').value;
    if (!usernameOrEmailAddress) {
        alert('用户名不能为空！');
        return false;
    }

    var password = document.getElementById('password').value;
    if (!password) {
        alert('密码不能为空！');
        return false;
    }
    //密码加密
    //var pubkeyHex = "0484C7466D950E120E5ECE5DD85D0C90EAA85081A3A2BD7C57AE6DC822EFCCBD66620C67B0103FC8DD280E36C3B282977B722AAEC3C56518EDCEBAFB72C5A05312";
    //var encryptData = sm2Encrypt(password, pubkeyHex, 1);
    var xhr = new XMLHttpRequest();
    // 创建要发送的数据对象
    var data = {
        usernameOrEmailAddress: usernameOrEmailAddress,
        password: password
    };

    xhr.onreadystatechange = function () {
        if (xhr.readyState === XMLHttpRequest.DONE) {
            if (xhr.status === 200) {
               
                var res = xhr.response;
                if (!res.success) {
                    alert(res.message);
                    return;
                }
                // 存储登录状态和token
                localStorage.setItem("auth.hasLogin", true);
                localStorage.setItem("token", res.result.accessToken);
                callback();
            } else {
                alert('登录失败 !');
            }
        }
    };
    //配置请求
    xhr.open('POST', auth.loginUrl, true);
    // 设置请求头
    xhr.setRequestHeader('Content-type', 'application/json; charset=utf-8');
    // 设置响应类型
    xhr.responseType = 'json';
    // 发送请求
    xhr.send(JSON.stringify(data));
}

// 创建输入框的辅助函数
function createInput(container, id, title, type) {
    var wrapper = document.createElement('div');
    wrapper.className = 'wrapper';
    container.appendChild(wrapper);
    var label = document.createElement('label');
    label.innerText = title;
    wrapper.appendChild(label);
    var section = document.createElement('section');
    section.className = 'block-tablet col-10-tablet block-desktop col-10-desktop';
    wrapper.appendChild(section);
    var input = document.createElement('input');
    input.id = id;
    input.type = type ? type : 'text';
    input.style.width = '100%';
    section.appendChild(input);
}