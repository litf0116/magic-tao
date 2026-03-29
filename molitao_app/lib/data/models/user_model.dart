enum UserRole { admin, user }

class UserDto {
  final String? userName;
  final String? name;
  final String? surname;
  final String? emailAddress;
  final bool? isActive;
  final String? fullName;
  final DateTime? lastLoginTime;
  final DateTime? creationTime;
  final List<String>? roleNames;
  final String? phoneNumber;
  final String? headImgUrl;
  final int? fromClient;
  final List<String>? permissions;
  final String? qq;
  final String? wx;
  final int? id;
  final double? depositBalance;

  const UserDto({
    this.userName,
    this.name,
    this.surname,
    this.emailAddress,
    this.isActive,
    this.fullName,
    this.lastLoginTime,
    this.creationTime,
    this.roleNames,
    this.phoneNumber,
    this.headImgUrl,
    this.fromClient,
    this.permissions,
    this.qq,
    this.wx,
    this.id,
    this.depositBalance,
  });

  factory UserDto.fromJson(Map<String, dynamic> json) {
    return UserDto(
      userName: json['userName'],
      name: json['name'],
      surname: json['surname'],
      emailAddress: json['emailAddress'],
      isActive: json['isActive'],
      fullName: json['fullName'],
      lastLoginTime: json['lastLoginTime'] != null
          ? DateTime.tryParse(json['lastLoginTime'].toString())
          : null,
      creationTime: json['creationTime'] != null
          ? DateTime.tryParse(json['creationTime'].toString())
          : null,
      roleNames: (json['roleNames'] as List<dynamic>?)
          ?.map((e) => e.toString())
          .toList(),
      phoneNumber: json['phoneNumber'],
      headImgUrl: json['headImgUrl'],
      fromClient: json['fromClient'],
      permissions: (json['permissions'] as List<dynamic>?)
          ?.map((e) => e.toString())
          .toList(),
      qq: json['qq'],
      wx: json['wx'],
      id: json['id'],
      depositBalance: json['depositBalance']?.toDouble(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'userName': userName,
      'name': name,
      'surname': surname,
      'emailAddress': emailAddress,
      'isActive': isActive,
      'fullName': fullName,
      'lastLoginTime': lastLoginTime?.toIso8601String(),
      'creationTime': creationTime?.toIso8601String(),
      'roleNames': roleNames,
      'phoneNumber': phoneNumber,
      'headImgUrl': headImgUrl,
      'fromClient': fromClient,
      'permissions': permissions,
      'qq': qq,
      'wx': wx,
      'id': id,
      'depositBalance': depositBalance,
    };
  }
}

class UserDtoBase {
  final String? userName;
  final String? name;
  final String? phoneNumber;
  final String? surname;
  final String? headImgUrl;
  final String? qq;
  final String? wx;
  final int? id;

  const UserDtoBase({
    this.userName,
    this.name,
    this.phoneNumber,
    this.surname,
    this.headImgUrl,
    this.qq,
    this.wx,
    this.id,
  });

  factory UserDtoBase.fromJson(Map<String, dynamic> json) {
    return UserDtoBase(
      userName: json['userName'],
      name: json['name'],
      phoneNumber: json['phoneNumber'],
      surname: json['surname'],
      headImgUrl: json['headImgUrl'],
      qq: json['qq'],
      wx: json['wx'],
      id: json['id'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'userName': userName,
      'name': name,
      'phoneNumber': phoneNumber,
      'surname': surname,
      'headImgUrl': headImgUrl,
      'qq': qq,
      'wx': wx,
      'id': id,
    };
  }
}

class UserDtoBaseListResultDto {
  final List<UserDtoBase>? items;

  const UserDtoBaseListResultDto({this.items});

  factory UserDtoBaseListResultDto.fromJson(Map<String, dynamic> json) {
    return UserDtoBaseListResultDto(
      items: (json['items'] as List<dynamic>?)
          ?.map((e) => UserDtoBase.fromJson(e))
          .toList(),
    );
  }

  Map<String, dynamic> toJson() {
    return {'items': items?.map((e) => e.toJson()).toList()};
  }
}
