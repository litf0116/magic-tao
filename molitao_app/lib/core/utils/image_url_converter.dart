/// 图片URL转换工具
/// 将 cdn.molitao.top 转换为 image.molitao.top
class ImageUrlConverter {
  /// 转换图片URL
  /// [url] 原始URL
  /// 返回转换后的URL
  static String convert(String? url) {
    if (url == null || url.isEmpty) return '';

    // 将 cdn.molitao.top 替换为 image.molitao.top
    return url.replaceAll(
      RegExp(r'https?://cdn\.molitao\.top'),
      'https://image.molitao.top',
    );
  }

  /// 批量转换图片URL数组
  static List<String> convertList(List<String> urls) {
    return urls.map((url) => convert(url)).toList();
  }
}
