import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/models/user_balance_log_model.dart';
import '../../data/repositories/balance_log_repository.dart';

class BalanceLogState {
  final List<UserBalanceLogDto> logs;
  final bool isLoading;
  final String? error;
  final bool hasMore;
  final int currentPage;

  const BalanceLogState({
    this.logs = const [],
    this.isLoading = false,
    this.error,
    this.hasMore = true,
    this.currentPage = 1,
  });

  BalanceLogState copyWith({
    List<UserBalanceLogDto>? logs,
    bool? isLoading,
    String? error,
    bool? hasMore,
    int? currentPage,
  }) {
    return BalanceLogState(
      logs: logs ?? this.logs,
      isLoading: isLoading ?? this.isLoading,
      error: error,
      hasMore: hasMore ?? this.hasMore,
      currentPage: currentPage ?? this.currentPage,
    );
  }
}

class BalanceLogNotifier extends StateNotifier<BalanceLogState> {
  final BalanceLogRepository _repository;

  BalanceLogNotifier(this._repository) : super(const BalanceLogState());

  Future<void> loadLogs({bool refresh = false}) async {
    if (state.isLoading) return;

    if (refresh) {
      state = const BalanceLogState();
    }

    state = state.copyWith(isLoading: true, error: null);

    try {
      final skipCount = refresh ? 0 : state.logs.length;
      final logs = await _repository.getMyBalanceLogs(
        skipCount: skipCount,
        maxResultCount: 20,
      );

      state = state.copyWith(
        logs: refresh ? logs : [...state.logs, ...logs],
        isLoading: false,
        hasMore: logs.length >= 20,
        currentPage: state.currentPage + (refresh ? 0 : 1),
      );
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
    }
  }

  Future<void> refresh() async {
    await loadLogs(refresh: true);
  }
}

final balanceLogProvider =
    StateNotifierProvider<BalanceLogNotifier, BalanceLogState>((ref) {
      return BalanceLogNotifier(BalanceLogRepository());
    });
