import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';

/// 支付响应结果
class PayResult {
  final String? appId;
  final String? partnerId;
  final String? prepayId;
  final String? packageValue;
  final String? nonceStr;
  final int? timeStamp;
  final String? signType;
  final String? paySign;

  const PayResult({
    this.appId,
    this.partnerId,
    this.prepayId,
    this.packageValue,
    this.nonceStr,
    this.timeStamp,
    this.signType,
    this.paySign,
  });

  factory PayResult.fromJson(Map<String, dynamic> json) {
    return PayResult(
      appId: json['appId'] as String?,
      partnerId: json['partnerId'] as String?,
      prepayId: json['prepayId'] as String?,
      packageValue: json['packageValue'] as String?,
      nonceStr: json['nonceStr'] as String?,
      timeStamp: json['timeStamp'] as int?,
      signType: json['signType'] as String?,
      paySign: json['paySign'] as String?,
    );
  }
}

/// 订单状态响应
class PayOrderStatus {
  final String? orderId;
  final String? outTradeNo;
  final String? status;
  final String? message;
  final double? amount;
  final DateTime? paidTime;
  final String? tradeNo;

  const PayOrderStatus({
    this.orderId,
    this.outTradeNo,
    this.status,
    this.message,
    this.amount,
    this.paidTime,
    this.tradeNo,
  });

  factory PayOrderStatus.fromJson(Map<String, dynamic> json) {
    return PayOrderStatus(
      orderId: json['orderId'] as String?,
      outTradeNo: json['outTradeNo'] as String?,
      status: json['status'] as String?,
      message: json['message'] as String?,
      amount: (json['amount'] as num?)?.toDouble(),
      paidTime: json['paidTime'] != null
          ? DateTime.tryParse(json['paidTime'] as String)
          : null,
      tradeNo: json['tradeNo'] as String?,
    );
  }
}

/// 用户统计信息
class MyCountResult {
  final int auctionSuccess;
  final int friend;
  final double balance;
  final double depositBalance;

  const MyCountResult({
    this.auctionSuccess = 0,
    this.friend = 0,
    this.balance = 0,
    this.depositBalance = 0,
  });

  factory MyCountResult.fromJson(Map<String, dynamic> json) {
    return MyCountResult(
      auctionSuccess: (json['auctionSuccess'] as num?)?.toInt() ?? 0,
      friend: (json['friend'] as num?)?.toInt() ?? 0,
      balance: (json['balance'] as num?)?.toDouble() ?? 0,
      depositBalance: (json['depositBalance'] as num?)?.toDouble() ?? 0,
    );
  }
}

class PaymentRepository {
  final ApiClient _apiClient = ApiClient();

  /// 保证金支付
  /// [openid] 微信 openid
  /// [amount] 支付金额，默认 51
  Future<PayResult> payDeposit({required String openid, double? amount}) async {
    try {
      final queryParams = <String, dynamic>{'openid': openid};
      if (amount != null) {
        queryParams['amount'] = amount;
      }

      final response = await _apiClient.dio.get(
        ApiEndpoints.payDeposit,
        queryParameters: queryParams,
      );

      if (response.data != null) {
        return PayResult.fromJson(response.data as Map<String, dynamic>);
      }
      throw Exception('支付参数获取失败');
    } on DioException catch (e) {
      throw Exception('保证金支付失败: ${e.message}');
    }
  }

  /// 余额充值
  /// [openid] 微信 openid
  /// [amount] 充值金额
  Future<PayResult> topUp({
    required String openid,
    required double amount,
  }) async {
    try {
      final response = await _apiClient.dio.get(
        ApiEndpoints.topUp,
        queryParameters: {'openid': openid, 'amount': amount},
      );

      if (response.data != null) {
        return PayResult.fromJson(response.data as Map<String, dynamic>);
      }
      throw Exception('充值参数获取失败');
    } on DioException catch (e) {
      throw Exception('余额充值失败: ${e.message}');
    }
  }

  /// 提现申请
  /// [amount] 提现金额
  Future<bool> payWithdrawal({required double amount}) async {
    try {
      final response = await _apiClient.dio.post(
        ApiEndpoints.payWithdrawal,
        data: {'amount': amount},
      );

      return response.data != null;
    } on DioException catch (e) {
      throw Exception('提现申请失败: ${e.message}');
    }
  }

  /// 查询支付订单状态
  /// [outTradeNo] 商户订单号
  Future<PayOrderStatus?> getPayOrderStatus(String outTradeNo) async {
    try {
      final response = await _apiClient.dio.get(
        '/api/services/app/Client/GetPayOrderStatus',
        queryParameters: {'outTradeNo': outTradeNo},
      );

      if (response.data != null) {
        return PayOrderStatus.fromJson(response.data as Map<String, dynamic>);
      }
      return null;
    } on DioException catch (e) {
      throw Exception('查询订单状态失败: ${e.message}');
    }
  }

  /// 获取用户统计信息
  Future<MyCountResult> getMyCount() async {
    try {
      final response = await _apiClient.dio.get(ApiEndpoints.getMyCount);

      if (response.data != null) {
        return MyCountResult.fromJson(response.data as Map<String, dynamic>);
      }
      return const MyCountResult();
    } on DioException catch (e) {
      throw Exception('获取统计信息失败: ${e.message}');
    }
  }
}
