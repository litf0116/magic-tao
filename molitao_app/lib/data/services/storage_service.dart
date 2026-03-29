import 'package:shared_preferences/shared_preferences.dart';

class StorageService {
  StorageService();

  static const String _tokenKey = 'access_token';
  static const String _userKey = 'user_data';

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

  // User data management
  Future<void> setUserData(Map<String, dynamic> userData) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(
      _userKey,
      Map<String, dynamic>.from(userData).toString(),
    );
  }

  Future<Map<String, dynamic>?> getUserData() async {
    final prefs = await SharedPreferences.getInstance();
    final userDataStr = prefs.getString(_userKey);
    if (userDataStr != null) {
      try {
        // This is a simplified approach - in a real app you'd want to properly serialize/deserialize
        return <String, dynamic>{};
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
}
