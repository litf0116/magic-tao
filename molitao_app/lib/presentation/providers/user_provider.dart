import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/services/storage_service.dart';
import '../../data/repositories/auth_repository.dart';

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
      fullName: json['fullName'] ?? json['name'],
      phoneNumber: json['phoneNumber'],
      headImgUrl: json['headImgUrl'],
      depositBalance: json['depositBalance']?.toDouble(),
      permissions: (json['permissions'] as List<dynamic>?)
          ?.map((e) => e.toString())
          .toList(),
      roleNames: (json['roleNames'] as List<dynamic>?)
          ?.map((e) => e.toString())
          .toList(),
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
  final List<String>? roles;

  UserState({
    required this.isLoggedIn,
    this.token,
    this.user,
    required this.isLoading,
    this.roles,
  });

  UserState copyWith({
    bool? isLoggedIn,
    String? token,
    User? user,
    bool? isLoading,
    List<String>? roles,
  }) {
    return UserState(
      isLoggedIn: isLoggedIn ?? this.isLoggedIn,
      token: token ?? this.token,
      user: user ?? this.user,
      isLoading: isLoading ?? this.isLoading,
      roles: roles ?? this.roles,
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

// Auth repository provider
final authRepositoryProvider = Provider<AuthRepository>((ref) {
  return AuthRepository();
});

// User notifier
class UserNotifier extends StateNotifier<UserState> {
  final Ref _ref;

  UserNotifier(this._ref) : super(UserState.initial()) {
    _initializeUser();
  }

  /// 应用启动时初始化用户状态
  Future<void> _initializeUser() async {
    final storageService = _ref.read(storageServiceProvider);

    final token = await storageService.getToken();
    if (token != null && token.isNotEmpty) {
      // 恢复用户数据
      final userData = await storageService.getUserData();
      User? user;
      if (userData != null) {
        user = User.fromJson(userData);
      }

      state = state.copyWith(token: token, isLoggedIn: true, user: user);
    }
  }

  /// 登录成功后保存用户信息
  Future<bool> login(String token, User user, {List<String>? roles}) async {
    state = state.copyWith(isLoading: true);

    try {
      final storageService = _ref.read(storageServiceProvider);
      await storageService.setToken(token);
      await storageService.setUserData(user.toJson());

      state = state.copyWith(
        isLoggedIn: true,
        token: token,
        user: user,
        roles: roles,
        isLoading: false,
      );

      return true;
    } catch (e) {
      state = state.copyWith(isLoading: false);
      return false;
    }
  }

  /// 检查登录状态
  /// [forceCheck] 如果为 true，会调用 API 验证 token 是否有效
  Future<bool> checkLogin({bool forceCheck = true}) async {
    final token = state.token;

    if (token == null || token.isEmpty) {
      _logoutInternal();
      return false;
    }

    // 如果不需要强制检查且已有用户数据，直接返回
    if (!forceCheck && state.user?.id != null) {
      return true;
    }

    try {
      final authRepository = _ref.read(authRepositoryProvider);
      final response = await authRepository.getCurrentLoginInformations();

      if (response != null && response['user'] != null) {
        final user = User.fromJson(response['user'] as Map<String, dynamic>);
        final roles = (response['roles'] as List<dynamic>?)
            ?.map((e) => e.toString())
            .toList();

        // 更新本地存储
        final storageService = _ref.read(storageServiceProvider);
        await storageService.setUserData(user.toJson());

        state = state.copyWith(user: user, roles: roles, isLoggedIn: true);
        return true;
      } else {
        _logoutInternal();
        return false;
      }
    } catch (e) {
      _logoutInternal();
      return false;
    }
  }

  /// 退出登录
  Future<void> logout() async {
    state = state.copyWith(isLoading: true);

    try {
      final authRepository = _ref.read(authRepositoryProvider);
      await authRepository.logout();
    } catch (e) {
      // 即使 API 调用失败，也清除本地状态
    }

    state = state.copyWith(
      isLoggedIn: false,
      token: null,
      user: null,
      roles: null,
      isLoading: false,
    );
  }

  void _logoutInternal() {
    state = state.copyWith(
      isLoggedIn: false,
      token: null,
      user: null,
      roles: null,
    );
    StorageService().clearToken();
    StorageService().clearUserData();
  }

  /// 更新用户信息
  Future<void> updateUser(User user) async {
    state = state.copyWith(user: user);

    try {
      final storageService = _ref.read(storageServiceProvider);
      await storageService.setUserData(user.toJson());
    } catch (e) {
      // 静默失败
    }
  }

  /// 快速检查是否已登录（不调用 API）
  bool get isAuthenticated => state.isLoggedIn && state.token != null;

  /// 检查是否是管理员
  bool get isAdmin => state.roles?.contains('Admin') ?? false;
}

// User provider
final userProvider = StateNotifierProvider<UserNotifier, UserState>((ref) {
  return UserNotifier(ref);
});
