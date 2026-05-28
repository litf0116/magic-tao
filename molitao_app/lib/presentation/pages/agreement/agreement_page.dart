import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:url_launcher/url_launcher.dart';

/// 协议展示页面（用户协议/隐私政策）
/// 自动跳转 PC 网站查看最新协议内容
class AgreementPage extends StatelessWidget {
  const AgreementPage({super.key});

  @override
  Widget build(BuildContext context) {
    // 从路由参数获取类型
    final type =
        GoRouterState.of(context).uri.queryParameters['type'] ?? 'user-agreement';

    final isUserAgreement = type == 'user-agreement';
    final title = isUserAgreement ? '用户协议' : '隐私政策';

    final url = isUserAgreement
        ? 'https://www.molitao.top/#/agreement?type=user-agreement'
        : 'https://www.molitao.top/#/agreement?type=privacy-policy';

    // 自动跳转到浏览器打开
    WidgetsBinding.instance.addPostFrameCallback((_) {
      launchUrl(Uri.parse(url));
    });

    return Scaffold(
      appBar: AppBar(
        title: Text(
          title,
          style: const TextStyle(fontSize: 20, color: Colors.white),
        ),
        backgroundColor: const Color(0xfff4835a),
        foregroundColor: Colors.white,
      ),
      body: const Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            CircularProgressIndicator(),
            SizedBox(height: 16),
            Text(
              '正在打开浏览器查看最新内容...',
              style: TextStyle(fontSize: 14, color: Color(0xff999999)),
            ),
          ],
        ),
      ),
    );
  }
}
