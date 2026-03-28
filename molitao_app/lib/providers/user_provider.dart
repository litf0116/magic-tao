import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../domain/entities/user_entity.dart';

final userProvider = StateNotifierProvider<UserNotifier, UserState>((ref) {
  return UserNotifier();
});

class UserState {
  final bool isLogin;
  final UserEntity? user;

  UserState({this.isLogin = false, this.user});
}

class UserNotifier extends StateNotifier<UserState> {
  UserNotifier() : super(UserState());

  void login(UserEntity user) {
    state = UserState(isLogin: true, user: user);
  }

  void logout() {
    state = UserState(isLogin: false, user: null);
  }

  void updateUser(UserEntity user) {
    state = UserState(isLogin: true, user: user);
  }
}
