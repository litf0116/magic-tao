import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/models/user_deposit_log_model.dart';
import '../../data/repositories/deposit_log_repository.dart';

class DepositLogState {
  final List<UserDepositLogDto> logs;
  final bool isLoading;
  final String? error;
  final bool hasMore;
  final int currentPage;

  const DepositLogState({
    this.logs = const [],
    this.isLoading = false,
    this.error,
    this.hasMore = true,
    this.currentPage = 1,
  });

  DepositLogState copyWith({
    List<UserDepositLogDto>? logs,
    bool? isLoading,
    String? error,
    bool? hasMore,
    int? currentPage,
  }) {
    return DepositLogState(
      logs: logs ?? this.logs,
      isLoading: isLoading ?? this.isLoading,
      error: error,
      hasMore: hasMore ?? this.hasMore,
      currentPage: currentPage ?? this.currentPage,
    );
  }
}

class DepositLogNotifier extends StateNotifier<DepositLogState> {
  final DepositLogRepository _repository;

  DepositLogNotifier(this._repository) : super(const DepositLogState());

  Future<void> loadLogs({bool refresh = false}) async {
    if (state.isLoading) return;

    if (refresh) {
      state = const DepositLogState();
    }

    state = state.copyWith(isLoading: true, error: null);

    try {
      final skipCount = refresh ? 0 : state.logs.length;
      final logs = await _repository.getMyDepositLogs(
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

final depositLogProvider =
    StateNotifierProvider<DepositLogNotifier, DepositLogState>((ref) {
      return DepositLogNotifier(DepositLogRepository());
    });
