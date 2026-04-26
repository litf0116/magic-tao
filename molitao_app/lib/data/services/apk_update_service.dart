import 'package:url_launcher/url_launcher.dart';

class ApkUpdateService {
  String? lastError;

  Future<bool> openDownloadUrl(String apkUrl) async {
    if (apkUrl.isEmpty) {
      lastError = '下载地址无效';
      return false;
    }

    try {
      final uri = Uri.parse(apkUrl);
      if (await canLaunchUrl(uri)) {
        await launchUrl(uri, mode: LaunchMode.externalApplication);
        return true;
      } else {
        lastError = '无法打开链接';
        return false;
      }
    } catch (e) {
      lastError = e.toString();
      return false;
    }
  }
}