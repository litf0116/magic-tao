import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/models/announce_model.dart';
import '../../data/repositories/announce_repository.dart';

class AnnounceState {
  final List<AnnounceDto> announces;
  final bool isLoading;
  final String? error;

  const AnnounceState({
    this.announces = const [],
    this.isLoading = false,
    this.error,
  });

  AnnounceState copyWith({
    List<AnnounceDto>? announces,
    bool? isLoading,
    String? error,
  }) {
    return AnnounceState(
      announces: announces ?? this.announces,
      isLoading: isLoading ?? this.isLoading,
      error: error,
    );
  }
}

class AnnounceNotifier extends StateNotifier<AnnounceState> {
  final AnnounceRepository _repository;

  AnnounceNotifier(this._repository) : super(const AnnounceState());

  Future<void> loadAnnounces({int? categoryId}) async {
    if (state.isLoading) return;

    state = state.copyWith(isLoading: true, error: null);

    try {
      final announces = await _repository.getAllPublicAnnounces(
        categoryId: categoryId,
      );
      state = state.copyWith(announces: announces, isLoading: false);
    } catch (e) {
      state = state.copyWith(isLoading: false, error: e.toString());
    }
  }

  Future<void> refresh({int? categoryId}) async {
    await loadAnnounces(categoryId: categoryId);
  }
}

final announceProvider = StateNotifierProvider<AnnounceNotifier, AnnounceState>(
  (ref) {
    return AnnounceNotifier(AnnounceRepository());
  },
);
