class UserEntity {
  final String? id;
  final String? name;
  final String? phoneNumber;
  final String? headImgUrl;

  UserEntity({this.id, this.name, this.phoneNumber, this.headImgUrl});

  factory UserEntity.fromJson(Map<String, dynamic> json) {
    return UserEntity(
      id: json['id'],
      name: json['name'],
      phoneNumber: json['phoneNumber'],
      headImgUrl: json['headImgUrl'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'phoneNumber': phoneNumber,
      'headImgUrl': headImgUrl,
    };
  }
}
