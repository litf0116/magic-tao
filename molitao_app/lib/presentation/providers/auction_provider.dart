import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/models/auction_item_model.dart';
import '../../data/repositories/auction_repository.dart';

/// 拍卖状态
class AuctionState {
  final List<AuctionItemDto> auctionList;
  final List<AuctionItemDto> yesterdayList; // New field
  final bool isLoading;
  final String? errorMessage;
  final bool isKasec;
  final int activeAuctionTab; // New field (1 for 今日榜单, 2 for 昨日成交)

  const AuctionState({
    this.auctionList = const [],
    this.yesterdayList = const [], // Default to empty list
    this.isLoading = false,
    this.errorMessage,
    this.isKasec = false,
    this.activeAuctionTab = 1, // Default to 今日榜单
  });

  AuctionState copyWith({
    List<AuctionItemDto>? auctionList,
    List<AuctionItemDto>? yesterdayList, // New parameter
    bool? isLoading,
    String? errorMessage,
    bool? isKasec,
    int? activeAuctionTab, // New parameter
  }) {
    return AuctionState(
      auctionList: auctionList ?? this.auctionList,
      yesterdayList: yesterdayList ?? this.yesterdayList, // Include new field
      isLoading: isLoading ?? this.isLoading,
      errorMessage: errorMessage,
      isKasec: isKasec ?? this.isKasec,
      activeAuctionTab:
          activeAuctionTab ?? this.activeAuctionTab, // Include new field
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

  /// 获取今日榜单（上架和拍卖中）的商品
  List<AuctionItemDto> get todayList {
    return auctionList
        .where(
          (item) =>
              item.status == AuctionStatusEnum.listed ||
              item.status == AuctionStatusEnum.auctioning,
        )
        .toList();
  }
}

/// 拍卖 Notifier
class AuctionNotifier extends StateNotifier<AuctionState> {
  final AuctionRepository _repository = AuctionRepository();

  AuctionNotifier(Ref ref) : super(const AuctionState());

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

  /// 加载昨日成交列表
  Future<void> loadYesterdayAuctions() async {
    try {
      final result = await _repository.getPublicAuctionList(
        maxResultCount: 100,
        status: 4, // Status 4 represents sold items
      );

      state = state.copyWith(yesterdayList: result.items);
    } catch (e) {
      // Keep existing error handling pattern
      state = state.copyWith(errorMessage: e.toString());
    }
  }

  /// 设置当前拍卖标签页
  Future<void> setActiveAuctionTab(int tab) async {
    state = state.copyWith(activeAuctionTab: tab);

    // If switching to 昨日成交 tab and the list is empty, load the data
    if (tab == 2 && state.yesterdayList.isEmpty) {
      await loadYesterdayAuctions();
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
