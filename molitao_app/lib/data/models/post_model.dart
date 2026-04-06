class PostModel {
  final int? id;
  final int? postId;
  final String? title;
  final String? content;
  final String? imageUrl;
  final int? userId;
  final String? userName;
  final String? userAvatar;
  final String? wechat;
  final String? qq;
  final DateTime? creationTime;
  final DateTime? lastModificationTime;
  final int? lastModifierUserId;
  final int? categoryId;
  final String? categoryName;
  final int? viewCount;
  final int? likeCount;
  final int? commentCount;
  final bool? isLiked;
  final bool? isBookmarked;

  const PostModel({
    this.id,
    this.postId,
    this.title,
    this.content,
    this.imageUrl,
    this.userId,
    this.userName,
    this.userAvatar,
    this.wechat,
    this.qq,
    this.creationTime,
    this.lastModificationTime,
    this.lastModifierUserId,
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
      postId: json['postId'] ?? json['id'],
      title: json['title'],
      content: json['content'],
      imageUrl: json['imageUrl'],
      userId: json['userId'],
      userName: json['userName'] ?? json['lastModifierUserName'],
      userAvatar: json['userAvatar'] ?? json['lastModifierUserAvatar'],
      wechat: json['wechat'],
      qq: json['qq'],
      creationTime: json['creationTime'] != null
          ? DateTime.tryParse(json['creationTime'].toString())
          : null,
      lastModificationTime: json['lastModificationTime'] != null
          ? DateTime.tryParse(json['lastModificationTime'].toString())
          : null,
      lastModifierUserId: json['lastModifierUserId'] ?? json['userId'],
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
      'postId': postId,
      'title': title,
      'content': content,
      'imageUrl': imageUrl,
      'userId': userId,
      'userName': userName,
      'userAvatar': userAvatar,
      'wechat': wechat,
      'qq': qq,
      'creationTime': creationTime?.toIso8601String(),
      'lastModificationTime': lastModificationTime?.toIso8601String(),
      'lastModifierUserId': lastModifierUserId,
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
