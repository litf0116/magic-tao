enum AuctionStatusEnum { draft, listed, auctioning, sold }

class AuctionItemDto {
  final String? name;
  final AuctionStatusEnum? status;
  final String? imageUrl;
  final String? description;
  final double? startingPrice;
  final double? currentPrice;
  final int? currentPriceUserId;
  final String? currentPriceUserName;
  final double? finalPrice;
  final DateTime? dealTime;
  final int? dealUserId;
  final String? dealUserName;
  final String? sellerInfo;
  final int? order;
  final int? id;
  final bool? isKasec;

  const AuctionItemDto({
    this.name,
    this.status,
    this.imageUrl,
    this.description,
    this.startingPrice,
    this.currentPrice,
    this.currentPriceUserId,
    this.currentPriceUserName,
    this.finalPrice,
    this.dealTime,
    this.dealUserId,
    this.dealUserName,
    this.sellerInfo,
    this.order,
    this.id,
    this.isKasec,
  });

  factory AuctionItemDto.fromJson(Map<String, dynamic> json) {
    return AuctionItemDto(
      name: json['name'],
      status: _parseAuctionStatusEnum(json['status']),
      imageUrl: json['imageUrl'],
      description: json['description'],
      startingPrice: json['startingPrice']?.toDouble(),
      currentPrice: json['currentPrice']?.toDouble(),
      currentPriceUserId: json['currentPriceUserId'],
      currentPriceUserName: json['currentPriceUserName'],
      finalPrice: json['finalPrice']?.toDouble(),
      dealTime: json['dealTime'] != null
          ? DateTime.tryParse(json['dealTime'].toString())
          : null,
      dealUserId: json['dealUserId'],
      dealUserName: json['dealUserName'],
      sellerInfo: json['sellerInfo'],
      order: json['order'],
      id: json['id'],
      isKasec: json['isKasec'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'name': name,
      'status': _auctionStatusEnumToString(status),
      'imageUrl': imageUrl,
      'description': description,
      'startingPrice': startingPrice,
      'currentPrice': currentPrice,
      'currentPriceUserId': currentPriceUserId,
      'currentPriceUserName': currentPriceUserName,
      'finalPrice': finalPrice,
      'dealTime': dealTime?.toIso8601String(),
      'dealUserId': dealUserId,
      'dealUserName': dealUserName,
      'sellerInfo': sellerInfo,
      'order': order,
      'id': id,
      'isKasec': isKasec,
    };
  }

  static AuctionStatusEnum? _parseAuctionStatusEnum(dynamic value) {
    if (value == null) return null;
    final stringValue = value.toString();
    switch (stringValue) {
      case '草稿':
        return AuctionStatusEnum.draft;
      case '上架':
        return AuctionStatusEnum.listed;
      case '拍卖中':
      case '秒杀中': // Handle both values as auctioning
        return AuctionStatusEnum.auctioning;
      case '已成交':
        return AuctionStatusEnum.sold;
      default:
        return null;
    }
  }

  static String? _auctionStatusEnumToString(AuctionStatusEnum? status) {
    if (status == null) return null;
    switch (status) {
      case AuctionStatusEnum.draft:
        return '草稿';
      case AuctionStatusEnum.listed:
        return '上架';
      case AuctionStatusEnum.auctioning:
        return '拍卖中';
      case AuctionStatusEnum.sold:
        return '已成交';
    }
  }
}
