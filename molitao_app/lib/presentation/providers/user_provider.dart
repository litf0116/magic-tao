import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/services/storage_service.dart';

// User data model
class User {
  final int? id;
  final String? userName;
  final String? fullName;
  final String? phoneNumber;
  final String? headImgUrl;
  final double? depositBalance;
  final List<String>? permissions;
  final List<String>? roleNames;

  User({
    this.id,
    this.userName,
    this.fullName,
    this.phoneNumber,
    this.headImgUrl,
    this.depositBalance,
    this.permissions,
    this.roleNames,
  });

  factory User.fromJson(Map<String, dynamic> json) {
    return User(
      id: json['id'],
      userName: json['userName'],
      fullName: json['fullName'],
      phoneNumber: json['phoneNumber'],
      headImgUrl: json['headImgUrl'],
      depositBalance: json['depositBalance']?.toDouble(),
      permissions: List<String>.from(json['permissions'] ?? []),
      roleNames: List<String>.from(json['roleNames'] ?? []),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'userName': userName,
      'fullName': fullName,
      'phoneNumber': phoneNumber,
      'headImgUrl': headImgUrl,
      'depositBalance': depositBalance,
      'permissions': permissions,
      'roleNames': roleNames,
    };
  }
}

// User state
class UserState {
  final bool isLoggedIn;
  final String? token;
  final User? user;
  final bool isLoading;

  UserState({
    required this.isLoggedIn,
    this.token,
    this.user,
    required this.isLoading,
  });

  UserState copyWith({
    bool? isLoggedIn,
    String? token,
    User? user,
    bool? isLoading,
  }) {
    return UserState(
      isLoggedIn: isLoggedIn ?? this.isLoggedIn,
      token: token ?? this.token,
      user: user ?? this.user,
      isLoading: isLoading ?? this.isLoading,
    );
  }

  factory UserState.initial() {
    return UserState(isLoggedIn: false, isLoading: false);
  }
}

// Storage service provider
final storageServiceProvider = Provider<StorageService>((ref) {
  return StorageService();
});

// User notifier
class UserNotifier extends StateNotifier<UserState> {
  final Ref _ref;

  UserNotifier(this._ref) : super(UserState.initial()) {
    _initializeUser();
  }

  Future<void> _initializeUser() async {
    final storageService = _ref.read(storageServiceProvider);

    final token = await storageService.getToken();
    if (token != null) {
      // Token exists, user might be logged in
      state = state.copyWith(token: token, isLoggedIn: true);
    }
  }

  Future<bool> login(String token, User user) async {
    state = state.copyWith(isLoading: true);

    try {
      final storageService = _ref.read(storageServiceProvider);
      await storageService.setToken(token);
      await storageService.setUserData(user.toJson());

      state = state.copyWith(
        isLoggedIn: true,
        token: token,
        user: user,
        isLoading: false,
      );

      return true;
    } catch (e) {
      state = state.copyWith(isLoading: false);
      return false;
    }
  }

  Future<void> logout() async {
    state = state.copyWith(isLoading: true);

    try {
      final storageService = _ref.read(storageServiceProvider);
      await storageService.clearToken();
      await storageService.clearUserData();

      state = state.copyWith(
        isLoggedIn: false,
        token: null,
        user: null,
        isLoading: false,
      );
    } catch (e) {
      state = state.copyWith(isLoading: false);
    }
  }

  Future<void> updateUser(User user) async {
    state = state.copyWith(user: user);

    try {
      final storageService = _ref.read(storageServiceProvider);
      await storageService.setUserData(user.toJson());
    } catch (e) {
      // Handle error silently or log
    }
  }

  Future<void> refreshToken(String newToken) async {
    state = state.copyWith(token: newToken);

    try {
      final storageService = _ref.read(storageServiceProvider);
      await storageService.setToken(newToken);
    } catch (e) {
      // Handle error silently or log
    }
  }
}

// User provider
final userProvider = StateNotifierProvider<UserNotifier, UserState>((ref) {
  return UserNotifier(ref);
});
