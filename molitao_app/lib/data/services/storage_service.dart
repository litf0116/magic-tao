import 'dart:convert';
import 'package:shared_preferences/shared_preferences.dart';

class StorageService {
  StorageService();

  static const String _tokenKey = 'access_token';
  static const String _refreshTokenKey = 'refresh_token';
  static const String _tokenExpireTimeKey = 'token_expire_time';
  static const String _userKey = 'user_data';
  static const String _rememberedUsernameKey = 'remembered_username';

  // Token management
  Future<String?> getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_tokenKey);
  }

  Future<void> setToken(String token) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_tokenKey, token);
  }

  Future<void> clearToken() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_tokenKey);
  }

  // Refresh Token management
  Future<String?> getRefreshToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_refreshTokenKey);
  }

  Future<void> setRefreshToken(String refreshToken) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_refreshTokenKey, refreshToken);
  }

  Future<void> clearRefreshToken() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_refreshTokenKey);
  }

  // Token Expire Time management
  Future<int?> getTokenExpireTime() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt(_tokenExpireTimeKey);
  }

  Future<void> setTokenExpireTime(int expireInSeconds) async {
    final prefs = await SharedPreferences.getInstance();
    final expireTime = DateTime.now().millisecondsSinceEpoch + expireInSeconds * 1000;
    await prefs.setInt(_tokenExpireTimeKey, expireTime);
  }

  Future<void> clearTokenExpireTime() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_tokenExpireTimeKey);
  }

  Future<bool> isTokenExpiringSoon([int thresholdSeconds = 3600]) async {
    final expireTime = await getTokenExpireTime();
    if (expireTime == null) return false;
    final now = DateTime.now().millisecondsSinceEpoch;
    return now + thresholdSeconds * 1000 > expireTime;
  }

  Future<void> clearAllTokens() async {
    await clearToken();
    await clearRefreshToken();
    await clearTokenExpireTime();
  }

  // User data management
  Future<void> setUserData(Map<String, dynamic> userData) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_userKey, jsonEncode(userData));
  }

  Future<Map<String, dynamic>?> getUserData() async {
    final prefs = await SharedPreferences.getInstance();
    final userDataStr = prefs.getString(_userKey);
    if (userDataStr != null) {
      try {
        return jsonDecode(userDataStr) as Map<String, dynamic>;
      } catch (e) {
        return null;
      }
    }
    return null;
  }

  Future<void> clearUserData() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_userKey);
  }

  // Generic storage methods
  Future<void> setValue(String key, dynamic value) async {
    final prefs = await SharedPreferences.getInstance();
    if (value is String) {
      await prefs.setString(key, value);
    } else if (value is int) {
      await prefs.setInt(key, value);
    } else if (value is double) {
      await prefs.setDouble(key, value);
    } else if (value is bool) {
      await prefs.setBool(key, value);
    } else if (value is List<String>) {
      await prefs.setStringList(key, value);
    }
  }

  Future<dynamic> getValue(String key) async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.get(key);
  }

  Future<void> removeValue(String key) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(key);
  }

  Future<void> clearAll() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.clear();
  }

  // Remember username for login
  Future<void> setRememberedUsername(String username) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_rememberedUsernameKey, username);
  }

  Future<String?> getRememberedUsername() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_rememberedUsernameKey);
  }

  // SMS countdown persistence
  static const String _smsCountdownKey = 'sms_countdown_end_time';

  Future<void> setSmsCountdownEndTime(int endTime) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setInt(_smsCountdownKey, endTime);
  }

  Future<int?> getSmsCountdownEndTime() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getInt(_smsCountdownKey);
  }

  Future<void> clearSmsCountdownEndTime() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_smsCountdownKey);
  }
}
