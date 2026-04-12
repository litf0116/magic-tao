import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/services/storage_service.dart';
import '../../data/services/push_service.dart';
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
  final String? qq;
  final String? wx;

  User({
    this.id,
    this.userName,
    this.fullName,
    this.phoneNumber,
    this.headImgUrl,
    this.depositBalance,
    this.permissions,
    this.roleNames,
    this.qq,
    this.wx,
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
      qq: json['qq'],
      wx: json['wx'],
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
      'qq': qq,
      'wx': wx,
    };
  }

  User copyWith({
    int? id,
    String? userName,
    String? fullName,
    String? phoneNumber,
    String? headImgUrl,
    double? depositBalance,
    List<String>? permissions,
    List<String>? roleNames,
    String? qq,
    String? wx,
  }) {
    return User(
      id: id ?? this.id,
      userName: userName ?? this.userName,
      fullName: fullName ?? this.fullName,
      phoneNumber: phoneNumber ?? this.phoneNumber,
      headImgUrl: headImgUrl ?? this.headImgUrl,
      depositBalance: depositBalance ?? this.depositBalance,
      permissions: permissions ?? this.permissions,
      roleNames: roleNames ?? this.roleNames,
      qq: qq ?? this.qq,
      wx: wx ?? this.wx,
    );
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
    print('[UserProvider] _initializeUser: token存在=${token != null}');

    if (token != null && token.isNotEmpty) {
      // 先恢复本地缓存的用户数据（快速显示）
      final userData = await storageService.getUserData();
      User? cachedUser;
      if (userData != null) {
        cachedUser = User.fromJson(userData);
        state = state.copyWith(
          token: token,
          isLoggedIn: true,
          user: cachedUser,
        );
      }

      // 使用 token 验证并获取最新用户信息
      try {
        print('[UserProvider] 使用 token 获取用户信息...');
        final authRepository = _ref.read(authRepositoryProvider);
        final response = await authRepository.getCurrentLoginInformations();

        if (response != null && response['user'] != null) {
          final user = User.fromJson(response['user'] as Map<String, dynamic>);
          final roles = (response['roles'] as List<dynamic>?)
              ?.map((e) => e.toString())
              .toList();

          print('[UserProvider] 解析用户信息:');
          print('[UserProvider] - user.id: ${user.id}');
          print('[UserProvider] - user.userName: ${user.userName}');
          print('[UserProvider] - user.fullName: ${user.fullName}');
          print('[UserProvider] - user.headImgUrl: ${user.headImgUrl}');
          print('[UserProvider] - roles: $roles');

          // 更新本地存储和状态
          await storageService.setUserData(user.toJson());
          print('[UserProvider] 已保存到本地存储');

          state = state.copyWith(
            token: token,
            isLoggedIn: true,
            user: user,
            roles: roles,
          );

          print('[UserProvider] state 已更新:');
          print('[UserProvider] - state.isLoggedIn: ${state.isLoggedIn}');
          print(
            '[UserProvider] - state.user?.userName: ${state.user?.userName}',
          );
          print('[UserProvider] 自动登录成功: ${user.userName}');

          final alias = 'user_${user.id}';
          print('[Push] 设置别名: $alias');
          PushService().setAlias(alias);
        } else {
          // token 无效，清除登录状态
          print('[UserProvider] token 无效，清除登录状态');
          _logoutInternal();
        }
      } catch (e) {
        print('[UserProvider] 自动登录失败: $e');
        // API 调用失败，但保留本地缓存的用户状态（可能是网络问题）
        if (cachedUser != null) {
          print('[UserProvider] 使用本地缓存的用户数据');
          state = state.copyWith(
            token: token,
            isLoggedIn: true,
            user: cachedUser,
          );
        }
      }
    }
  }

  /// 登录成功后保存用户信息
  Future<bool> login(String token, User user, {List<String>? roles}) async {
    print('[UserProvider] login() 被调用');
    print('[UserProvider] - token: ${token.substring(0, 20)}...');
    print('[UserProvider] - user.id: ${user.id}');
    print('[UserProvider] - user.userName: ${user.userName}');
    print('[UserProvider] - user.fullName: ${user.fullName}');

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

      print('[UserProvider] login() 成功');
      print('[UserProvider] - state.isLoggedIn: ${state.isLoggedIn}');
      print('[UserProvider] - state.user: ${state.user?.userName}');

      if (user.id != null) {
        final alias = 'user_${user.id}';
        print('[Push] 设置别名: $alias');
        PushService().setAlias(alias);
      }

      return true;
    } catch (e) {
      print('[UserProvider] login() 异常: $e');
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

    PushService().deleteAlias();

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
