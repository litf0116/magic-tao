import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/models/auction_item_model.dart';
import '../../data/repositories/auction_repository.dart';

/// 拍卖状态
class AuctionState {
  final List<AuctionItemDto> auctionList;
  final bool isLoading;
  final String? errorMessage;
  final bool isKasec;

  const AuctionState({
    this.auctionList = const [],
    this.isLoading = false,
    this.errorMessage,
    this.isKasec = false,
  });

  AuctionState copyWith({
    List<AuctionItemDto>? auctionList,
    bool? isLoading,
    String? errorMessage,
    bool? isKasec,
  }) {
    return AuctionState(
      auctionList: auctionList ?? this.auctionList,
      isLoading: isLoading ?? this.isLoading,
      errorMessage: errorMessage,
      isKasec: isKasec ?? this.isKasec,
    );
  }

  /// 获取当前正在拍卖的商品
  AuctionItemDto? get onAuctionItem {
    try {
      return auctionList.firstWhere(
        (item) => item.status == AuctionStatusEnum.auctioning,
      );
    } catch (e) {
      return null;
    }
  }
}

/// 拍卖 Notifier
class AuctionNotifier extends StateNotifier<AuctionState> {
  final Ref _ref;
  final AuctionRepository _repository = AuctionRepository();

  AuctionNotifier(this._ref) : super(const AuctionState());

  /// 加载拍卖列表
  Future<void> loadAuctions() async {
    state = state.copyWith(isLoading: true, errorMessage: null);

    try {
      final result = await _repository.getPublicAuctionList(
        maxResultCount: 100,
      );

      state = state.copyWith(auctionList: result.items, isLoading: false);
    } catch (e) {
      state = state.copyWith(isLoading: false, errorMessage: e.toString());
    }
  }

  /// 刷新拍卖列表
  Future<void> refresh() async {
    await loadAuctions();
  }

  /// 同步卡秒状态
  Future<bool> syncKasecStatus(int auctionItemId) async {
    try {
      final status = await _repository.getKasecStatus(auctionItemId);
      final isKasec = status == 'true' || status == '1';
      state = state.copyWith(isKasec: isKasec);
      return isKasec;
    } catch (e) {
      state = state.copyWith(isKasec: false);
      return false;
    }
  }

  /// 出价
  Future<bool> bid(int auctionItemId, double bidPrice) async {
    try {
      await _repository.placeBid(
        auctionItemId: auctionItemId,
        bidPrice: bidPrice,
      );
      return true;
    } catch (e) {
      return false;
    }
  }

  /// 获取拍卖详情
  Future<AuctionItemDto?> getAuctionDetail(int auctionItemId) async {
    try {
      return await _repository.getAuctionDetail(auctionItemId);
    } catch (e) {
      return null;
    }
  }
}

/// 拍卖 Provider
final auctionProvider = StateNotifierProvider<AuctionNotifier, AuctionState>((
  ref,
) {
  return AuctionNotifier(ref);
});

/// 当前拍卖商品 Provider
final onAuctionItemProvider = Provider<AuctionItemDto?>((ref) {
  final auctionState = ref.watch(auctionProvider);
  return auctionState.onAuctionItem;
});
