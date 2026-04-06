// 图片URL转换工具
// 与 UniApp imageUrlConverter.ts 保持一致

/// 转换图片URL
/// 将 cdn.molitao.top 转换为 image.molitao.top
String convertImageUrl(String? url) {
  if (url == null || url.isEmpty) return '';

  // 将 cdn.molitao.top 替换为 image.molitao.top
  return url.replaceAll(RegExp(r'https?://cdn\.molitao\.top'), 'https://image.molitao.top');
}

/// 批量转换图片URL数组
List<String> convertImageUrls(List<String>? urls) {
  if (urls == null || urls.isEmpty) return [];
  return urls.map((url) => convertImageUrl(url)).toList();
}

/// 处理HTML中的图片URL
/// 替换 img 标签中的 src 和 data-url 属性
String convertHtmlImageUrls(String html) {
  if (html.isEmpty) return html;

  var result = html;

  // 替换 data-url 属性中的URL (双引号)
  result = result.replaceAllMapped(
    RegExp(r'data-url="([^"]*)"'),
    (match) {
      final url = match.group(1);
      if (url != null) {
        final converted = convertImageUrl(url);
        return 'data-url="$converted"';
      }
      return match.group(0)!;
    },
  );

  // 替换 data-url 属性中的URL (单引号)
  result = result.replaceAllMapped(
    RegExp(r"data-url='([^']*)'"),
    (match) {
      final url = match.group(1);
      if (url != null) {
        final converted = convertImageUrl(url);
        return "data-url='$converted'";
      }
      return match.group(0)!;
    },
  );

  // 替换 src 属性中的URL (双引号)，同时移除 !w300 缩略图参数
  result = result.replaceAllMapped(
    RegExp(r'src="([^"]*)"'),
    (match) {
      final url = match.group(1);
      if (url != null) {
        // 移除 !w300 缩略图参数
        final cleanUrl = url.replaceAll(RegExp(r'!w300$'), '');
        final converted = convertImageUrl(cleanUrl);
        return 'src="$converted"';
      }
      return match.group(0)!;
    },
  );

  // 替换 src 属性中的URL (单引号)，同时移除 !w300 缩略图参数
  result = result.replaceAllMapped(
    RegExp(r"src='([^']*)'"),
    (match) {
      final url = match.group(1);
      if (url != null) {
        // 移除 !w300 缩略图参数
        final cleanUrl = url.replaceAll(RegExp(r'!w300$'), '');
        final converted = convertImageUrl(cleanUrl);
        return "src='$converted'";
      }
      return match.group(0)!;
    },
  );

  return result;
}

/// 从HTML中提取所有图片URL
List<String> extractImageUrlsFromHtml(String html) {
  if (html.isEmpty) return [];

  final List<String> urls = [];

  // 优先从 data-url 属性提取 (双引号)
  var dataUrlRegExp = RegExp(r'<img[^>]+data-url="([^"]*)"', caseSensitive: false);
  var matches = dataUrlRegExp.allMatches(html);
  for (final match in matches) {
    final url = match.group(1);
    if (url != null && url.isNotEmpty) {
      urls.add(convertImageUrl(url));
    }
  }

  // data-url (单引号)
  dataUrlRegExp = RegExp(r"<img[^>]+data-url='([^']*)'", caseSensitive: false);
  matches = dataUrlRegExp.allMatches(html);
  for (final match in matches) {
    final url = match.group(1);
    if (url != null && url.isNotEmpty) {
      urls.add(convertImageUrl(url));
    }
  }

  // 如果没有 data-url，从 src 属性提取 (双引号)
  if (urls.isEmpty) {
    var srcRegExp = RegExp(r'<img[^>]+src="([^"]*)"', caseSensitive: false);
    matches = srcRegExp.allMatches(html);
    for (final match in matches) {
      final url = match.group(1);
      if (url != null && url.isNotEmpty) {
        // 移除 !w300 缩略图参数
        final cleanUrl = url.replaceAll(RegExp(r'!w300$'), '');
        urls.add(convertImageUrl(cleanUrl));
      }
    }

    // src (单引号)
    srcRegExp = RegExp(r"<img[^>]+src='([^']*)'", caseSensitive: false);
    matches = srcRegExp.allMatches(html);
    for (final match in matches) {
      final url = match.group(1);
      if (url != null && url.isNotEmpty) {
        // 移除 !w300 缩略图参数
        final cleanUrl = url.replaceAll(RegExp(r'!w300$'), '');
        urls.add(convertImageUrl(cleanUrl));
      }
    }
  }

  return urls;
}
