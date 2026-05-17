import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:molitao_app/data/services/agreement_service.dart';

/// 协议展示页面（用户协议/隐私政策）
class AgreementPage extends StatelessWidget {
  const AgreementPage({super.key});

  @override
  Widget build(BuildContext context) {
    // 从路由参数获取类型
    final type =
        GoRouterState.of(context).uri.queryParameters['type'] ?? 'user-agreement';

    final isUserAgreement = type == 'user-agreement';
    final title = isUserAgreement ? '用户协议' : '隐私政策';
    final content = isUserAgreement
        ? AgreementService.userAgreement
        : AgreementService.privacyPolicy;

    return Scaffold(
      appBar: AppBar(
        title: Text(
          title,
          style: const TextStyle(fontSize: 20, color: Colors.white),
        ),
        backgroundColor: const Color(0xfff4835a),
        foregroundColor: Colors.white,
      ),
      body: Container(
        color: const Color(0xfff6f6f6),
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(16),
          child: Text(
            content,
            style: const TextStyle(
              fontSize: 14,
              height: 1.8,
              color: Color(0xff333333),
            ),
          ),
        ),
      ),
    );
  }
}

/// 路由参数说明：
/// - type=user-agreement: 显示用户协议
/// - type=privacy-policy: 显示隐私政策
///
/// 使用方式：
/// context.push('/agreement?type=user-agreement')
/// context.push('/agreement?type=privacy-policy')