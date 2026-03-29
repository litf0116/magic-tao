class CmsArticle {
  final int? id;
  final String? title;
  final String? titleImageUrl;
  final String? content;
  final int? pid;
  final int? sort;
  final String? linkUrl;
  final String? description;

  CmsArticle({
    this.id,
    this.title,
    this.titleImageUrl,
    this.content,
    this.pid,
    this.sort,
    this.linkUrl,
    this.description,
  });

  factory CmsArticle.fromJson(Map<String, dynamic> json) {
    return CmsArticle(
      id: json['id'],
      title: json['title'],
      titleImageUrl: json['titleImageUrl'],
      content: json['content'],
      pid: json['pid'],
      sort: json['sort'],
      linkUrl: json['linkUrl'],
      description: json['description'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'title': title,
      'titleImageUrl': titleImageUrl,
      'content': content,
      'pid': pid,
      'sort': sort,
      'linkUrl': linkUrl,
      'description': description,
    };
  }
}
