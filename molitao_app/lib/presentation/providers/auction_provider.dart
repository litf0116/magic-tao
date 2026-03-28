import 'package:flutter_riverpod/flutter_riverpod.dart';

// Auction status enum
enum AuctionStatusEnum { draft, listed, active, sold }

// Auction item model
class AuctionItem {
  final int? id;
  final String? name;
  final AuctionStatusEnum? status;
  final String? imageUrl;
  final double? currentPrice;
  final double? startingPrice;

  AuctionItem({
    this.id,
    this.name,
    this.status,
    this.imageUrl,
    this.currentPrice,
    this.startingPrice,
  });

  factory AuctionItem.fromJson(Map<String, dynamic> json) {
    return AuctionItem(
      id: json['id'],
      name: json['name'],
      status: json['status'] != null
          ? AuctionStatusEnum.values[json['status']]
          : null,
      imageUrl: json['imageUrl'],
      currentPrice: json['currentPrice']?.toDouble(),
      startingPrice: json['startingPrice']?.toDouble(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'status': status?.index,
      'imageUrl': imageUrl,
      'currentPrice': currentPrice,
      'startingPrice': startingPrice,
    };
  }
}

// Auction state
class AuctionState {
  final List<AuctionItem> auctionList;
  final List<AuctionItem> filteredAuctionList;
  final AuctionStatusEnum? filterStatus;
  final bool isLoading;
  final String? errorMessage;

  AuctionState({
    required this.auctionList,
    required this.filteredAuctionList,
    this.filterStatus,
    required this.isLoading,
    this.errorMessage,
  });

  AuctionState copyWith({
    List<AuctionItem>? auctionList,
    List<AuctionItem>? filteredAuctionList,
    AuctionStatusEnum? filterStatus,
    bool? isLoading,
    String? errorMessage,
  }) {
    return AuctionState(
      auctionList: auctionList ?? this.auctionList,
      filteredAuctionList: filteredAuctionList ?? this.filteredAuctionList,
      filterStatus: filterStatus ?? this.filterStatus,
      isLoading: isLoading ?? this.isLoading,
      errorMessage: errorMessage ?? this.errorMessage,
    );
  }

  factory AuctionState.initial() {
    return AuctionState(
      auctionList: [],
      filteredAuctionList: [],
      isLoading: false,
    );
  }
}

// Auction notifier
class AuctionNotifier extends StateNotifier<AuctionState> {
  final Ref _ref;

  AuctionNotifier(this._ref) : super(AuctionState.initial());

  Future<void> loadAuctions() async {
    state = state.copyWith(isLoading: true, errorMessage: null);

    try {
      // In a real implementation, you would fetch auctions from API
      // For now, we'll simulate with an empty list
      final mockAuctions = <AuctionItem>[];

      state = state.copyWith(
        auctionList: mockAuctions,
        filteredAuctionList: mockAuctions,
        isLoading: false,
      );
    } catch (e) {
      state = state.copyWith(isLoading: false, errorMessage: e.toString());
    }
  }

  Future<void> loadAuctionsByStatus(AuctionStatusEnum status) async {
    state = state.copyWith(isLoading: true, errorMessage: null);

    try {
      // In a real implementation, you would fetch auctions from API with status filter
      // For now, we'll simulate with an empty list
      final mockAuctions = <AuctionItem>[];

      state = state.copyWith(
        auctionList: mockAuctions,
        filteredAuctionList: mockAuctions,
        filterStatus: status,
        isLoading: false,
      );
    } catch (e) {
      state = state.copyWith(isLoading: false, errorMessage: e.toString());
    }
  }

  void filterAuctionsByStatus(AuctionStatusEnum? status) {
    if (status == null) {
      state = state.copyWith(
        filteredAuctionList: state.auctionList,
        filterStatus: null,
      );
      return;
    }

    final filtered = state.auctionList
        .where((item) => item.status == status)
        .toList();
    state = state.copyWith(filteredAuctionList: filtered, filterStatus: status);
  }

  void refreshAuctions() {
    if (state.filterStatus != null) {
      loadAuctionsByStatus(state.filterStatus!);
    } else {
      loadAuctions();
    }
  }

  AuctionItem? getAuctionById(int id) {
    return state.auctionList.firstWhere(
      (item) => item.id == id,
      orElse: () => state.auctionList.first,
    );
  }
}

// Auction provider
final auctionProvider = StateNotifierProvider<AuctionNotifier, AuctionState>((
  ref,
) {
  return AuctionNotifier(ref);
});

// Auction status filter provider
final auctionStatusFilterProvider = StateProvider<AuctionStatusEnum?>((ref) {
  return null; // No filter by default
});
