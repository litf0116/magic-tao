import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/api/api_client.dart';
import '../../data/api/api_endpoints.dart';
import 'user_provider.dart';

// Friend item model
class FriendItem {
  final int id;
  final String name;
  final String? avatar;

  FriendItem({required this.id, required this.name, this.avatar});

  factory FriendItem.fromJson(Map<String, dynamic> json) {
    return FriendItem(
      id: json['id'] ?? json['userId'] ?? 0,
      name: json['name'] ?? json['userName'] ?? '',
      avatar: json['headImgUrl'] ?? json['avatar'],
    );
  }
}

// Contacts state
class ContactsState {
  final List<FriendItem> friends;
  final List<FriendItem> friendRequests;
  final String filterText;
  final bool isLoading;
  final String? errorMessage;

  ContactsState({
    this.friends = const [],
    this.friendRequests = const [],
    this.filterText = '',
    this.isLoading = false,
    this.errorMessage,
  });

  ContactsState copyWith({
    List<FriendItem>? friends,
    List<FriendItem>? friendRequests,
    String? filterText,
    bool? isLoading,
    String? errorMessage,
  }) {
    return ContactsState(
      friends: friends ?? this.friends,
      friendRequests: friendRequests ?? this.friendRequests,
      filterText: filterText ?? this.filterText,
      isLoading: isLoading ?? this.isLoading,
      errorMessage: errorMessage ?? this.errorMessage,
    );
  }

  // Filtered friends based on search text
  List<FriendItem> get filteredFriends {
    if (filterText.isEmpty) return friends;
    return friends.where((f) => f.name.contains(filterText)).toList();
  }
}

// Contacts notifier
class ContactsNotifier extends StateNotifier<ContactsState> {
  final Ref _ref;

  ContactsNotifier(this._ref) : super(ContactsState());

  Future<void> loadFriends() async {
    state = state.copyWith(isLoading: true, errorMessage: null);

    try {
      // Load friends and friend requests in parallel
      final results = await Future.wait([
        _loadFriendsList(),
        _loadFriendRequests(),
      ]);

      state = state.copyWith(
        friends: results[0],
        friendRequests: results[1],
        isLoading: false,
      );
    } catch (e) {
      state = state.copyWith(isLoading: false, errorMessage: e.toString());
    }
  }

  Future<List<FriendItem>> _loadFriendsList() async {
    final userId = _ref.read(userProvider).user?.id;
    if (userId == null) return [];
    try {
      // Get accepted friends (status = true)
      final response = await ApiClient().dio.get(
        ApiEndpoints.getUserFriends,
        queryParameters: {'id': userId, 'status': true},
      );

      if (response.data != null) {
        // 响应格式: {"items": [...]}
        final items =
            response.data['items'] as List? ?? response.data as List? ?? [];
        return items.map((json) => FriendItem.fromJson(json)).toList();
      }
      return [];
    } catch (e) {
      print('Error loading friends: $e');
      return [];
    }
  }

  Future<List<FriendItem>> _loadFriendRequests() async {
    final userId = _ref.read(userProvider).user?.id;
    if (userId == null) return [];
    try {
      // Get pending friend requests (status = false means pending)
      final response = await ApiClient().dio.get(
        ApiEndpoints.getUserFriends,
        queryParameters: {'id': userId, 'status': false},
      );

      if (response.data != null) {
        // 响应格式: {"items": [...]}
        final items =
            response.data['items'] as List? ?? response.data as List? ?? [];
        return items.map((json) => FriendItem.fromJson(json)).toList();
      }
      return [];
    } catch (e) {
      print('Error loading friend requests: $e');
      return [];
    }
  }

  Future<void> handleFriendRequest(int friendId, bool accept) async {
    try {
      await ApiClient().dio.post(
        ApiEndpoints.agreeFriend,
        data: {'id': friendId, 'status': accept},
      );

      // Refresh the lists
      await loadFriends();
    } catch (e) {
      throw Exception('Failed to handle friend request: $e');
    }
  }

  void setFilterText(String text) {
    state = state.copyWith(filterText: text);
  }
}

// Contacts provider
final contactsProvider = StateNotifierProvider<ContactsNotifier, ContactsState>(
  (ref) {
    return ContactsNotifier(ref);
  },
);
