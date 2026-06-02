enum LoginBindingType {
  wechat,
  phone,
  password,
  apple,
}

class LoginBindingDto {
  final int? id;
  final String? loginProvider;
  final String? providerKey;
  final String? displayName;
  final String? icon;
  final bool? isBound;
  final DateTime? boundTime;

  const LoginBindingDto({
    this.id,
    this.loginProvider,
    this.providerKey,
    this.displayName,
    this.icon,
    this.isBound,
    this.boundTime,
  });

  factory LoginBindingDto.fromJson(Map<String, dynamic> json) {
    return LoginBindingDto(
      id: json['id'],
      loginProvider: json['loginProvider'],
      providerKey: json['providerKey'],
      displayName: json['displayName'],
      icon: json['icon'],
      isBound: json['isBound'],
      boundTime: json['boundTime'] != null
          ? DateTime.tryParse(json['boundTime'].toString())
          : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'loginProvider': loginProvider,
      'providerKey': providerKey,
      'displayName': displayName,
      'icon': icon,
      'isBound': isBound,
      'boundTime': boundTime?.toIso8601String(),
    };
  }

  LoginBindingType? get bindingType {
    switch (loginProvider?.toLowerCase()) {
      case 'wechat':
      case 'wechatunionid':
      case 'wechatapp':
      case 'wechatmini':
        return LoginBindingType.wechat;
      case 'phone':
        return LoginBindingType.phone;
      case 'password':
        return LoginBindingType.password;
      case 'apple':
        return LoginBindingType.apple;
      default:
        return null;
    }
  }

  String get displayIcon {
    switch (bindingType) {
      case LoginBindingType.wechat:
        return 'wechat';
      case LoginBindingType.phone:
        return 'phone';
      case LoginBindingType.password:
        return 'password';
      case LoginBindingType.apple:
        return 'apple';
      default:
        return 'unknown';
    }
  }
}

class LoginBindingListResultDto {
  final List<LoginBindingDto>? items;

  const LoginBindingListResultDto({this.items});

  factory LoginBindingListResultDto.fromJson(Map<String, dynamic> json) {
    return LoginBindingListResultDto(
      items: (json['items'] as List<dynamic>?)
          ?.map((e) => LoginBindingDto.fromJson(e))
          .toList(),
    );
  }

  Map<String, dynamic> toJson() {
    return {'items': items?.map((e) => e.toJson()).toList()};
  }
}
