/// 图片URL转换工具
/// 将 cdn.molitao.top 转换为 image.molitao.top
/// 将相对路径转换为完整URL
class ImageUrlConverter {
  static const String _imageBaseUrl = 'https://image.molitao.top';

  /// 转换图片URL
  /// [url] 原始URL（可能是完整URL或相对路径）
  /// 返回转换后的完整URL
  static String convert(String? url) {
    if (url == null || url.isEmpty) return '';

    // 已经是完整URL，替换cdn为image
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url.replaceAll(
        RegExp(r'https?://cdn\.molitao\.top'),
        _imageBaseUrl,
      );
    }

    // 相对路径，添加完整域名
    // 去除开头的斜杠（如果有）
    String path = url.startsWith('/') ? url.substring(1) : url;
    return '$_imageBaseUrl/$path';
  }

  /// 批量转换图片URL数组
  static List<String> convertList(List<String> urls) {
    return urls.map((url) => convert(url)).toList();
  }
}
