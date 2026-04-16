class UserDepositLogDto {
  final int? id;
  final double? amount;
  final String? type;
  final DateTime? successTime;
  final DateTime? creationTime;

  const UserDepositLogDto({
    this.id,
    this.amount,
    this.type,
    this.successTime,
    this.creationTime,
  });

  factory UserDepositLogDto.fromJson(Map<String, dynamic> json) {
    return UserDepositLogDto(
      id: json['id'],
      amount: (json['amount'] as num?)?.toDouble(),
      type: json['type'],
      successTime: json['successTime'] != null
          ? DateTime.tryParse(json['successTime'].toString())
          : null,
      creationTime: json['creationTime'] != null
          ? DateTime.tryParse(json['creationTime'].toString())
          : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'amount': amount,
      'type': type,
      'successTime': successTime?.toIso8601String(),
      'creationTime': creationTime?.toIso8601String(),
    };
  }
}
