import 'dart:convert';

/// 扫码获取用户信息响应
class QrCodeUserInfo {
  final int userId;
  final String nickname;
  final String avatar;
  final String phone;

  const QrCodeUserInfo({
    required this.userId,
    required this.nickname,
    required this.avatar,
    required this.phone,
  });

  factory QrCodeUserInfo.fromJson(Map<String, dynamic> json) {
    return QrCodeUserInfo(
      userId: json['userId'] as int? ?? 0,
      nickname: json['nickname'] as String? ?? '',
      avatar: json['avatar'] as String? ?? '',
      phone: json['phone'] as String? ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'userId': userId,
      'nickname': nickname,
      'avatar': avatar,
      'phone': phone,
    };
  }

  @override
  String toString() => jsonEncode(toJson());
}

/// 确认登录结果响应
class QrCodeLoginResult {
  final String token;
  final String tokenType;
  final int expiresIn;
  final QrCodeUserInfo user;

  const QrCodeLoginResult({
    required this.token,
    required this.tokenType,
    required this.expiresIn,
    required this.user,
  });

  factory QrCodeLoginResult.fromJson(Map<String, dynamic> json) {
    return QrCodeLoginResult(
      token: json['token'] as String? ?? '',
      tokenType: json['tokenType'] as String? ?? 'Bearer',
      expiresIn: json['expiresIn'] as int? ?? 0,
      user: json['user'] != null
          ? QrCodeUserInfo.fromJson(json['user'] as Map<String, dynamic>)
          : const QrCodeUserInfo(userId: 0, nickname: '', avatar: '', phone: ''),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'token': token,
      'tokenType': tokenType,
      'expiresIn': expiresIn,
      'user': user.toJson(),
    };
  }

  @override
  String toString() => jsonEncode(toJson());
}
