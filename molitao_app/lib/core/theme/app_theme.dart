import 'package:flutter/material.dart';
import 'app_colors.dart';
import 'app_text_styles.dart';

class AppTheme {
  static ThemeData get lightTheme {
    return ThemeData(
      useMaterial3: true,
      colorScheme:
          ColorScheme.fromSeed(
            seedColor: AppColors.primary,
            brightness: Brightness.light,
          ).copyWith(
            primary: AppColors.primary,
            secondary: AppColors.primaryLight,
            surface: AppColors.surface,
            error: AppColors.error,
          ),
      textTheme: _buildTextTheme(),
      appBarTheme: _buildAppBarTheme(),
      bottomNavigationBarTheme: _buildBottomNavigationBarTheme(),
      elevatedButtonTheme: _buildElevatedButtonTheme(),
      outlinedButtonTheme: _buildOutlinedButtonTheme(),
    );
  }

  static TextTheme _buildTextTheme() {
    return const TextTheme(
      headlineLarge: AppTextStyles.headline1,
      headlineMedium: AppTextStyles.headline2,
      headlineSmall: AppTextStyles.headline3,
      titleLarge: AppTextStyles.headline4,
      titleMedium: AppTextStyles.headline5,
      titleSmall: AppTextStyles.headline6,
      bodyLarge: AppTextStyles.bodyText1,
      bodyMedium: AppTextStyles.bodyText2,
      labelLarge: AppTextStyles.button,
      bodySmall: AppTextStyles.caption,
      labelSmall: AppTextStyles.overline,
    );
  }

  static AppBarTheme _buildAppBarTheme() {
    return const AppBarTheme(
      backgroundColor: AppColors.surface,
      foregroundColor: AppColors.textPrimary,
      elevation: 0,
      centerTitle: true,
      titleTextStyle: AppTextStyles.headline6,
    );
  }

  static BottomNavigationBarThemeData _buildBottomNavigationBarTheme() {
    return BottomNavigationBarThemeData(
      backgroundColor: AppColors.surface,
      selectedItemColor: AppColors.primary,
      unselectedItemColor: AppColors.textSecondary,
      selectedLabelStyle: AppTextStyles.caption,
      unselectedLabelStyle: AppTextStyles.caption,
      type: BottomNavigationBarType.fixed,
    );
  }

  static ElevatedButtonThemeData _buildElevatedButtonTheme() {
    return ElevatedButtonThemeData(
      style:
          ElevatedButton.styleFrom(
            backgroundColor: AppColors.primary,
            foregroundColor: Colors.white,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(8.0),
            ),
            padding: const EdgeInsets.symmetric(
              vertical: 12.0,
              horizontal: 24.0,
            ),
          ).copyWith(
            textStyle: WidgetStateProperty.all(
              AppTextStyles.button.copyWith(fontWeight: FontWeight.bold),
            ),
          ),
    );
  }

  static OutlinedButtonThemeData _buildOutlinedButtonTheme() {
    return OutlinedButtonThemeData(
      style:
          OutlinedButton.styleFrom(
            foregroundColor: AppColors.primary,
            side: const BorderSide(color: AppColors.primary),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(8.0),
            ),
            padding: const EdgeInsets.symmetric(
              vertical: 12.0,
              horizontal: 24.0,
            ),
          ).copyWith(
            textStyle: WidgetStateProperty.all(
              AppTextStyles.button.copyWith(fontWeight: FontWeight.bold),
            ),
          ),
    );
  }
}
