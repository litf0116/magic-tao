import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';


/// 协议展示页面（用户协议/隐私政策）
/// 从本地 HTML 资产文件加载内容，解析为富文本展示
class AgreementPage extends StatefulWidget {
  const AgreementPage({super.key});

  @override
  State<AgreementPage> createState() => _AgreementPageState();
}

class _AgreementPageState extends State<AgreementPage> {
  List<InlineSpan>? _spans;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadAgreement();
  }

  Future<void> _loadAgreement() async {
    final type =
        GoRouterState.of(context).uri.queryParameters['type'] ?? 'user-agreement';

    final assetPath = type == 'user-agreement'
        ? 'assets/agreements/user_agreement.html'
        : 'assets/agreements/privacy_policy.html';

    try {
      final html = await rootBundle.loadString(assetPath);
      if (mounted) {
        setState(() {
          _spans = _parseHtml(html);
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _spans = [
            TextSpan(
              text: '加载失败，请稍后重试',
              style: TextStyle(color: Colors.grey[600], fontSize: 15),
            ),
          ];
          _isLoading = false;
        });
      }
    }
  }

  /// 解析简单 HTML（h1/h2/p/ul/li/br）为 TextSpan 列表
  List<InlineSpan> _parseHtml(String html) {
    final spans = <InlineSpan>[];
    final lines = _extractLines(html);

    for (final line in lines) {
      final trimmed = line.trim();
      if (trimmed.isEmpty) continue;

      if (trimmed.startsWith('<h1>') && trimmed.endsWith('</h1>')) {
        spans.add(TextSpan(
          text: '\n${_stripTags(trimmed)}\n',
          style: const TextStyle(
            fontSize: 22,
            fontWeight: FontWeight.w700,
            color: Color(0xfff4835a),
          ),
        ));
      } else if (trimmed.startsWith('<h2>') && trimmed.endsWith('</h2>')) {
        spans.add(TextSpan(
          text: '\n${_stripTags(trimmed)}\n',
          style: const TextStyle(
            fontSize: 17,
            fontWeight: FontWeight.w600,
            color: Color(0xff222222),
          ),
        ));
      } else if (trimmed.startsWith('<li>')) {
        // 提取 <li> 内容
        final text = _stripTags(trimmed);
        spans.add(TextSpan(
          text: '\n  •  $text',
          style: const TextStyle(fontSize: 15, color: Color(0xff333333)),
        ));
      } else if (trimmed == '<br>' || trimmed == '<br/>') {
        spans.add(const TextSpan(text: '\n'));
      } else {
        // p 标签或纯文本
        final text = _stripTags(trimmed);
        if (text.isNotEmpty) {
          spans.add(TextSpan(
            text: '\n$text\n',
            style: const TextStyle(fontSize: 15, color: Color(0xff333333)),
          ));
        }
      }
    }

    return spans;
  }

  /// 将 HTML 按行拆分，保留标签完整性
  List<String> _extractLines(String html) {
    // 移除 DOCTYPE, html, head, meta, style, body 等包裹标签
    var cleaned = html
        .replaceAll(RegExp(r'<!DOCTYPE[^>]*>', caseSensitive: false), '')
        .replaceAll(RegExp(r'</?html[^>]*>', caseSensitive: false), '')
        .replaceAll(RegExp(r'</?head[^>]*>', caseSensitive: false), '')
        .replaceAll(RegExp(r'</?body[^>]*>', caseSensitive: false), '')
        .replaceAll(RegExp(r'<meta[^>]*/?>', caseSensitive: false), '')
        .replaceAll(RegExp(r'<style[^>]*>.*?</style>', caseSensitive: false, dotAll: true), '')
        .replaceAll(RegExp(r'<title[^>]*>.*?</title>', caseSensitive: false, dotAll: true), '')
        .replaceAll('&nbsp;', ' ')
        .trim();

    // 在块级标签前后加分隔符
    cleaned = cleaned
        .replaceAll(RegExp(r'</?h1[^>]*>'), '\n<h1>')
        .replaceAll(RegExp(r'</h1>'), '</h1>\n')
        .replaceAll(RegExp(r'</?h2[^>]*>'), '\n<h2>')
        .replaceAll(RegExp(r'</h2>'), '</h2>\n')
        .replaceAll(RegExp(r'</?p[^>]*>'), '\n')
        .replaceAll(RegExp(r'</?ul[^>]*>'), '\n')
        .replaceAll(RegExp(r'</?li[^>]*>'), '\n<li>')
        .replaceAll('</li>', '</li>\n')
        .replaceAll('<br>', '\n<br>\n')
        .replaceAll('<br/>', '\n<br>\n')
        .replaceAll('<br />', '\n<br>\n')
        .replaceAll(RegExp(r'\n{3,}'), '\n\n')
        .trim();

    return cleaned.split('\n');
  }

  /// 去除所有 HTML 标签
  String _stripTags(String html) {
    return html
        .replaceAll(RegExp(r'<[^>]*>'), '')
        .replaceAll(RegExp(r'\s+'), ' ')
        .trim();
  }

  @override
  Widget build(BuildContext context) {
    final type =
        GoRouterState.of(context).uri.queryParameters['type'] ?? 'user-agreement';
    final isUserAgreement = type == 'user-agreement';
    final title = isUserAgreement ? '用户协议' : '隐私政策';

    return Scaffold(
      appBar: AppBar(
        title: Text(
          title,
          style: const TextStyle(fontSize: 20, color: Colors.white),
        ),
        backgroundColor: const Color(0xfff4835a),
        foregroundColor: Colors.white,
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 20),
              child: RichText(
                text: TextSpan(
                  children: _spans ?? [],
                ),
              ),
            ),
    );
  }
}
