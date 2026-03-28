class MyCountEntity {
  final int friend;
  final int depositBalance;

  MyCountEntity({required this.friend, required this.depositBalance});

  factory MyCountEntity.fromJson(Map<String, dynamic> json) {
    return MyCountEntity(
      friend: json['friend'] ?? 0,
      depositBalance: json['depositBalance'] ?? 0,
    );
  }

  Map<String, dynamic> toJson() {
    return {'friend': friend, 'depositBalance': depositBalance};
  }
}
