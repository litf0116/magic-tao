import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../domain/entities/my_count_entity.dart';

final apiServiceProvider = Provider<ApiService>((ref) {
  return ApiService();
});

class ApiService {
  // Mock implementation for getMyCount
  Future<MyCountEntity> getMyCount() async {
    // Simulate API call delay
    await Future.delayed(const Duration(milliseconds: 500));

    // Return mock data - in real implementation, this would call the actual API
    return MyCountEntity(friend: 12, depositBalance: 100);
  }
}
