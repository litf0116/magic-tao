import 'package:flutter/material.dart';
import 'dart:developer' as developer;

enum ErrorType {
  network,
  authentication,
  permission,
  validation,
  server,
  unknown,
}

class AppException implements Exception {
  final String message;
  final String? code;
  final ErrorType type;
  final dynamic originalError;

  AppException({
    required this.message,
    this.code,
    this.type = ErrorType.unknown,
    this.originalError,
  });

  factory AppException.fromDynamic(dynamic error) {
    if (error is AppException) return error;
    
    String message = '发生未知错误';
    ErrorType type = ErrorType.unknown;
    String? code;

    if (error.toString().contains('SocketException') ||
        error.toString().contains('TimeoutException')) {
      type = ErrorType.network;
      message = '网络连接失败，请检查网络设置';
    } else if (error.toString().contains('401') ||
               error.toString().contains('Unauthorized')) {
      type = ErrorType.authentication;
      message = '登录已过期，请重新登录';
      code = '401';
    } else if (error.toString().contains('403') ||
               error.toString().contains('Forbidden')) {
      type = ErrorType.permission;
      message = '没有权限执行此操作';
      code = '403';
    } else if (error.toString().contains('400') ||
               error.toString().contains('validation')) {
      type = ErrorType.validation;
      message = '输入数据验证失败';
      code = '400';
    } else if (error.toString().contains('500') ||
               error.toString().contains('Internal Server Error')) {
      type = ErrorType.server;
      message = '服务器错误，请稍后重试';
      code = '500';
    }

    return AppException(
      message: message,
      code: code,
      type: type,
      originalError: error,
    );
  }

  @override
  String toString() => 'AppException: $message (code: $code, type: $type)';
}

class ErrorHandler {
  static void log(dynamic error, [StackTrace? stackTrace]) {
    final appException = AppException.fromDynamic(error);
    
    developer.log(
      appException.message,
      error: appException.originalError,
      stackTrace: stackTrace,
      name: 'ErrorHandler',
      level: _getLogLevel(appException.type),
    );
  }

  static void showErrorSnackBar(BuildContext context, dynamic error) {
    final appException = AppException.fromDynamic(error);
    
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(appException.message),
        backgroundColor: _getErrorColor(appException.type),
        behavior: SnackBarBehavior.floating,
        duration: const Duration(seconds: 3),
        action: appException.type == ErrorType.network
            ? SnackBarAction(
                label: '重试',
                textColor: Colors.white,
                onPressed: () {
                  // 可以在这里触发重试逻辑
                },
              )
            : null,
      ),
    );
  }

  static String getUserFriendlyMessage(dynamic error) {
    return AppException.fromDynamic(error).message;
  }

  static Color _getErrorColor(ErrorType type) {
    switch (type) {
      case ErrorType.network:
        return Colors.orange;
      case ErrorType.authentication:
        return Colors.red;
      case ErrorType.permission:
        return Colors.red;
      case ErrorType.validation:
        return Colors.orange;
      case ErrorType.server:
        return Colors.red;
      case ErrorType.unknown:
        return Colors.grey;
    }
  }

  static int _getLogLevel(ErrorType type) {
    switch (type) {
      case ErrorType.network:
      case ErrorType.validation:
        return 900; // WARNING
      case ErrorType.authentication:
      case ErrorType.permission:
        return 800; // SEVERE
      case ErrorType.server:
        return 1000; // SEVERE
      case ErrorType.unknown:
        return 700; // INFO
    }
  }

  static Future<T> handleAsync<T>(
    Future<T> Function() operation, {
    BuildContext? context,
    String? customMessage,
    bool showError = true,
  }) async {
    try {
      return await operation();
    } catch (error, stackTrace) {
      log(error, stackTrace);
      
      if (showError && context != null && context.mounted) {
        showErrorSnackBar(context, error);
      }
      
      rethrow;
    }
  }

  static bool shouldLogout(dynamic error) {
    final appException = AppException.fromDynamic(error);
    return appException.type == ErrorType.authentication;
  }
}