enum ArticleStatusEnum { draft, published }

class AnnounceDto {
  final int? categoryId;
  final String? content;
  final String? imageUrl;
  final DateTime? creationTime;
  final int? creatorUserId;
  final int? id;

  const AnnounceDto({
    this.categoryId,
    this.content,
    this.imageUrl,
    this.creationTime,
    this.creatorUserId,
    this.id,
  });

  factory AnnounceDto.fromJson(Map<String, dynamic> json) {
    return AnnounceDto(
      categoryId: json['categoryId'],
      content: json['content'],
      imageUrl: json['imageUrl'],
      creationTime: json['creationTime'] != null
          ? DateTime.tryParse(json['creationTime'].toString())
          : null,
      creatorUserId: json['creatorUserId'],
      id: json['id'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'categoryId': categoryId,
      'content': content,
      'imageUrl': imageUrl,
      'creationTime': creationTime?.toIso8601String(),
      'creatorUserId': creatorUserId,
      'id': id,
    };
  }
}

class CmsArticleDto {
  final int? categoryId;
  final String? title;
  final String? titleImageUrl;
  final String? content;
  final ArticleStatusEnum? status;
  final DateTime? creationTime;
  final int? creatorUserId;
  final int? id;

  const CmsArticleDto({
    this.categoryId,
    this.title,
    this.titleImageUrl,
    this.content,
    this.status,
    this.creationTime,
    this.creatorUserId,
    this.id,
  });

  factory CmsArticleDto.fromJson(Map<String, dynamic> json) {
    return CmsArticleDto(
      categoryId: json['categoryId'] ?? json['id'],
      title: json['title'] ?? json['name'], // 兼容 name 字段
      titleImageUrl: json['titleImageUrl'],
      content: json['content'],
      status: _parseArticleStatusEnum(json['status']),
      creationTime: json['creationTime'] != null
          ? DateTime.tryParse(json['creationTime'].toString())
          : null,
      creatorUserId: json['creatorUserId'],
      id: json['id'] ?? json['categoryId'], // 兼容 categoryId 作为 id
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'categoryId': categoryId,
      'title': title,
      'titleImageUrl': titleImageUrl,
      'content': content,
      'status': _articleStatusEnumToString(status),
      'creationTime': creationTime?.toIso8601String(),
      'creatorUserId': creatorUserId,
      'id': id,
    };
  }

  static ArticleStatusEnum? _parseArticleStatusEnum(dynamic value) {
    if (value == null) return null;
    final stringValue = value.toString();
    switch (stringValue) {
      case '草稿':
        return ArticleStatusEnum.draft;
      case '已发布':
        return ArticleStatusEnum.published;
      default:
        return null;
    }
  }

  static String? _articleStatusEnumToString(ArticleStatusEnum? status) {
    if (status == null) return null;
    switch (status) {
      case ArticleStatusEnum.draft:
        return '草稿';
      case ArticleStatusEnum.published:
        return '已发布';
    }
  }
}
