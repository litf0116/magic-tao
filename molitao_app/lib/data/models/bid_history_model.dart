class BidHistoryCreateDto {
  final int? auctionItemId;
  final double? bidPrice;
  final String? bidUserName;
  final String? bidUserAvatar;
  final DateTime? bidTime;
  final int? id;

  const BidHistoryCreateDto({
    this.auctionItemId,
    this.bidPrice,
    this.bidUserName,
    this.bidUserAvatar,
    this.bidTime,
    this.id,
  });

  factory BidHistoryCreateDto.fromJson(Map<String, dynamic> json) {
    return BidHistoryCreateDto(
      auctionItemId: json['auctionItemId'],
      bidPrice: json['bidPrice']?.toDouble(),
      bidUserName: json['bidUserName'],
      bidUserAvatar: json['bidUserAvatar'],
      bidTime: json['bidTime'] != null
          ? DateTime.tryParse(json['bidTime'].toString())
          : null,
      id: json['id'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'auctionItemId': auctionItemId,
      'bidPrice': bidPrice,
      'bidUserName': bidUserName,
      'bidUserAvatar': bidUserAvatar,
      'bidTime': bidTime?.toIso8601String(),
      'id': id,
    };
  }
}
