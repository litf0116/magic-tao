import 'package:flutter/foundation.dart';
import 'package:sign_in_with_apple/sign_in_with_apple.dart';

class AppleService {
  static final AppleService _instance = AppleService._internal();
  factory AppleService() => _instance;
  AppleService._internal();

  Future<bool> isAvailable() async {
    return defaultTargetPlatform == TargetPlatform.iOS;
  }

  Future<AppleSignInResult?> signIn() async {
    if (!await isAvailable()) return null;

    try {
      final credential = await SignInWithApple.getAppleIDCredential(
        scopes: [
          AppleIDAuthorizationScopes.email,
          AppleIDAuthorizationScopes.fullName,
        ],
      );

      // identityToken 直接就是 String?
      final identityToken = credential.identityToken;

      // 首次登录有 email/name，后续为 null
      // Apple 官方：仅首次登录返回
      return AppleSignInResult(
        identityToken: identityToken,
        userIdentifier: credential.userIdentifier,
        email: credential.email,
        givenName: credential.givenName,
        familyName: credential.familyName,
      );
    } catch (e) {
      debugPrint('[AppleService] signIn error: $e');
      return null;
    }
  }
}

class AppleSignInResult {
  final String? identityToken;
  final String? userIdentifier;
  final String? email;
  final String? givenName;
  final String? familyName;

  const AppleSignInResult({
    this.identityToken,
    this.userIdentifier,
    this.email,
    this.givenName,
    this.familyName,
  });
}
