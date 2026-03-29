class PostModel {
  final int? id;
  final String? title;
  final String? content;
  final String? imageUrl;
  final int? userId;
  final String? userName;
  final DateTime? creationTime;
  final int? categoryId;
  final String? categoryName;
  final int? viewCount;
  final int? likeCount;
  final int? commentCount;
  final bool? isLiked;
  final bool? isBookmarked;

  const PostModel({
    this.id,
    this.title,
    this.content,
    this.imageUrl,
    this.userId,
    this.userName,
    this.creationTime,
    this.categoryId,
    this.categoryName,
    this.viewCount,
    this.likeCount,
    this.commentCount,
    this.isLiked,
    this.isBookmarked,
  });

  factory PostModel.fromJson(Map<String, dynamic> json) {
    return PostModel(
      id: json['id'],
      title: json['title'],
      content: json['content'],
      imageUrl: json['imageUrl'],
      userId: json['userId'],
      userName: json['userName'],
      creationTime: json['creationTime'] != null
          ? DateTime.tryParse(json['creationTime'].toString())
          : null,
      categoryId: json['categoryId'],
      categoryName: json['categoryName'],
      viewCount: json['viewCount'],
      likeCount: json['likeCount'],
      commentCount: json['commentCount'],
      isLiked: json['isLiked'],
      isBookmarked: json['isBookmarked'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'title': title,
      'content': content,
      'imageUrl': imageUrl,
      'userId': userId,
      'userName': userName,
      'creationTime': creationTime?.toIso8601String(),
      'categoryId': categoryId,
      'categoryName': categoryName,
      'viewCount': viewCount,
      'likeCount': likeCount,
      'commentCount': commentCount,
      'isLiked': isLiked,
      'isBookmarked': isBookmarked,
    };
  }
}
