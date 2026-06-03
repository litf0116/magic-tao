import 'dart:convert';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// 存储服务
/// - 敏感数据（access_token / refresh_token / token_expire_time）走 FlutterSecureStorage
///   （iOS Keychain / Android Keystore / macOS Keychain / Windows DPAPI）
/// - 非敏感数据（username / user data / SMS countdown）保留 SharedPreferences
class StorageService {
  StorageService();

  static const String _tokenKey = 'access_token';
  static const String _refreshTokenKey = 'refresh_token';
  static const String _tokenExpireTimeKey = 'token_expire_time';
  static const String _userKey = 'user_data';
  static const String _rememberedUsernameKey = 'remembered_username';

  static const _secureOptions = AndroidOptions(encryptedSharedPreferences: true);

  final FlutterSecureStorage _secure = const FlutterSecureStorage(
    aOptions: _secureOptions,
  );

  // ===================== Token (Secure Storage) =====================

  Future<String?> getToken() async {
    final secure = await _secure.read(key: _tokenKey);
    if (secure != null) return secure;
    return _migrateTokenFromPrefs();
  }

  Future<void> setToken(String token) async {
    await _secure.write(key: _tokenKey, value: token);
  }

  Future<void> clearToken() async {
    await _secure.delete(key: _tokenKey);
  }

  // ===================== Refresh Token (Secure Storage) =====================

  Future<String?> getRefreshToken() async {
    final secure = await _secure.read(key: _refreshTokenKey);
    if (secure != null) return secure;
    return _migrateRefreshTokenFromPrefs();
  }

  Future<void> setRefreshToken(String refreshToken) async {
    await _secure.write(key: _refreshTokenKey, value: refreshToken);
  }

  Future<void> clearRefreshToken() async {
    await _secure.delete(key: _refreshTokenKey);
  }

  // ===================== Token Expire Time (Secure Storage) =====================

  Future<int?> getTokenExpireTime() async {
    final secure = await _secure.read(key: _tokenExpireTimeKey);
    if (secure != null) {
      return int.tryParse(secure);
    }
    return _migrateExpireTimeFromPrefs();
  }

  Future<void> setTokenExpireTime(int expireInSeconds) async {
    final expireTime =
        DateTime.now().millisecondsSinceEpoch + expireInSeconds * 1000;
    await _secure.write(key: _tokenExpireTimeKey, value: expireTime.toString());
  }

  Future<void> clearTokenExpireTime() async {
    await _secure.delete(key: _tokenExpireTimeKey);
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

  // ===================== Migration (shared_preferences → SecureStorage) =====================

  Future<String?> _migrateTokenFromPrefs() async {
    final prefs = await SharedPreferences.getInstance();
    final old = prefs.getString(_tokenKey);
    if (old == null) return null;
    await _secure.write(key: _tokenKey, value: old);
    await prefs.remove(_tokenKey);
    return old;
  }

  Future<String?> _migrateRefreshTokenFromPrefs() async {
    final prefs = await SharedPreferences.getInstance();
    final old = prefs.getString(_refreshTokenKey);
    if (old == null) return null;
    await _secure.write(key: _refreshTokenKey, value: old);
    await prefs.remove(_refreshTokenKey);
    return old;
  }

  Future<int?> _migrateExpireTimeFromPrefs() async {
    final prefs = await SharedPreferences.getInstance();
    final old = prefs.getInt(_tokenExpireTimeKey);
    if (old == null) return null;
    await _secure.write(key: _tokenExpireTimeKey, value: old.toString());
    await prefs.remove(_tokenExpireTimeKey);
    return old;
  }

  // ===================== User data (SharedPreferences) =====================

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

  // ===================== Generic storage methods (SharedPreferences) =====================

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

  // ===================== Remember username (SharedPreferences) =====================

  Future<void> setRememberedUsername(String username) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_rememberedUsernameKey, username);
  }

  Future<String?> getRememberedUsername() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_rememberedUsernameKey);
  }

  // ===================== SMS countdown persistence (SharedPreferences) =====================

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
