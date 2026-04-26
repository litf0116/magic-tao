import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:mobile_scanner/mobile_scanner.dart';
import 'package:cached_network_image/cached_network_image.dart';
import 'package:permission_handler/permission_handler.dart';
import '../../../core/theme/app_colors.dart';
import '../../../data/repositories/qr_code_repository.dart';
import '../../../data/models/qr_code_model.dart';
import '../../providers/user_provider.dart';

enum _PageState { scanning, confirming, loading, error }

class QrCodeConfirmPage extends ConsumerStatefulWidget {
  const QrCodeConfirmPage({super.key});

  @override
  ConsumerState<QrCodeConfirmPage> createState() => _QrCodeConfirmPageState();
}

class _QrCodeConfirmPageState extends ConsumerState<QrCodeConfirmPage> {
  final MobileScannerController _scannerController = MobileScannerController();
  final QrCodeRepository _qrCodeRepository = QrCodeRepository();

  _PageState _pageState = _PageState.scanning;
  QrCodeUserInfo? _userInfo;
  String? _scannedCode;
  String? _errorMessage;
  bool _isProcessing = false;

  @override
  void initState() {
    super.initState();
    _checkCameraPermission();
  }

  Future<void> _checkCameraPermission() async {
    final status = await Permission.camera.status;
    if (!status.isGranted) {
      final result = await Permission.camera.request();
      if (!result.isGranted && mounted) {
        setState(() {
          _pageState = _PageState.error;
          _errorMessage = '需要相机权限才能扫描二维码';
        });
      }
    }
  }

  void _onDetect(BarcodeCapture capture) {
    if (_isProcessing || _pageState != _PageState.scanning) return;

    final barcodes = capture.barcodes;
    if (barcodes.isEmpty) return;

    final rawValue = barcodes.first.rawValue;
    if (rawValue == null) return;

    final code = _parseQrCode(rawValue);
    if (code == null) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('无效的二维码格式')),
        );
      }
      return;
    }

    _isProcessing = true;
    _scannerController.stop();
    _fetchUserInfo(code);
  }

  String? _parseQrCode(String rawValue) {
    final uri = Uri.tryParse(rawValue);
    if (uri == null) return null;

    if (uri.scheme == 'molitao' && uri.host == 'qrcode') {
      return uri.queryParameters['code'];
    }

    if (uri.pathSegments.isNotEmpty) {
      final lastSegment = uri.pathSegments.last;
      if (lastSegment.isNotEmpty) return lastSegment;
    }

    if (uri.queryParameters.containsKey('code')) {
      return uri.queryParameters['code'];
    }

    if (!rawValue.contains('://') && !rawValue.contains('/') && rawValue.length > 5) {
      return rawValue;
    }

    return null;
  }

  Future<void> _fetchUserInfo(String code) async {
    setState(() {
      _pageState = _PageState.loading;
      _scannedCode = code;
    });

    try {
      final userInfo = await _qrCodeRepository.getUserInfoByCode(code);
      if (userInfo != null && mounted) {
        setState(() {
          _userInfo = userInfo;
          _pageState = _PageState.confirming;
          _isProcessing = false;
        });
      } else if (mounted) {
        setState(() {
          _pageState = _PageState.error;
          _errorMessage = '获取用户信息失败';
          _isProcessing = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _pageState = _PageState.error;
          _errorMessage = '获取用户信息失败: ${e.toString()}';
          _isProcessing = false;
        });
      }
    }
  }

  Future<void> _confirmLogin() async {
    if (_scannedCode == null) return;

    setState(() {
      _pageState = _PageState.loading;
    });

    try {
      final result = await _qrCodeRepository.confirmLogin(_scannedCode!);
      if (result != null && mounted) {
        await ref.read(userProvider.notifier).login(
          result.token,
          User(
            id: result.user.userId,
            userName: result.user.nickname,
            fullName: result.user.nickname,
            phoneNumber: result.user.phone,
            headImgUrl: result.user.avatar,
          ),
        );

        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('登录成功')),
          );
          context.go('/home');
        }
      } else if (mounted) {
        setState(() {
          _pageState = _PageState.confirming;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('登录失败，请重试')),
        );
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _pageState = _PageState.confirming;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('登录失败: ${e.toString()}')),
        );
      }
    }
  }

  void _cancelAndRescan() {
    setState(() {
      _pageState = _PageState.scanning;
      _userInfo = null;
      _scannedCode = null;
      _errorMessage = null;
      _isProcessing = false;
    });
    _scannerController.start();
  }

  void _openAppSettings() {
    openAppSettings();
  }

  @override
  void dispose() {
    _scannerController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFFAF1F0),
      appBar: AppBar(
        title: const Text('扫码登录'),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        elevation: 0,
      ),
      body: _buildBody(),
    );
  }

  Widget _buildBody() {
    switch (_pageState) {
      case _PageState.scanning:
        return _buildScanningView();
      case _PageState.loading:
        return _buildLoadingView();
      case _PageState.confirming:
        return _buildConfirmView();
      case _PageState.error:
        return _buildErrorView();
    }
  }

  Widget _buildScanningView() {
    return Stack(
      children: [
        MobileScanner(
          controller: _scannerController,
          onDetect: _onDetect,
          errorBuilder: (context, error, child) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.error, color: AppColors.error, size: 64),
                  const SizedBox(height: 16),
                  Text(
                    _getScannerErrorMessage(error),
                    textAlign: TextAlign.center,
                    style: const TextStyle(color: AppColors.error),
                  ),
                  if (error.errorCode == MobileScannerErrorCode.permissionDenied)
                    Padding(
                      padding: const EdgeInsets.only(top: 16),
                      child: ElevatedButton(
                        onPressed: _openAppSettings,
                        style: ElevatedButton.styleFrom(
                          backgroundColor: AppColors.primary,
                          foregroundColor: Colors.white,
                        ),
                        child: const Text('打开设置'),
                      ),
                    ),
                ],
              ),
            );
          },
        ),
        _buildScanOverlay(),
      ],
    );
  }

  Widget _buildScanOverlay() {
    return LayoutBuilder(
      builder: (context, constraints) {
        final scanWindowSize = 250.0;
        final left = (constraints.maxWidth - scanWindowSize) / 2;
        final top = (constraints.maxHeight - scanWindowSize) / 2 - 50;

        return Stack(
          children: [
            Positioned.fill(
              child: Container(
                color: Colors.black.withValues(alpha: 0.5),
              ),
            ),
            Positioned(
              left: left,
              top: top,
              width: scanWindowSize,
              height: scanWindowSize,
              child: Container(
                decoration: BoxDecoration(
                  border: Border.all(color: AppColors.primary, width: 3),
                  borderRadius: BorderRadius.circular(12),
                ),
              ),
            ),
            Positioned(
              left: 0,
              right: 0,
              bottom: constraints.maxHeight * 0.2,
              child: const Text(
                '将二维码放入框内扫描',
                textAlign: TextAlign.center,
                style: TextStyle(
                  color: Colors.white,
                  fontSize: 16,
                  fontWeight: FontWeight.w500,
                ),
              ),
            ),
          ],
        );
      },
    );
  }

  Widget _buildLoadingView() {
    return const Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          CircularProgressIndicator(color: AppColors.primary),
          SizedBox(height: 24),
          Text(
            '正在处理...',
            style: TextStyle(
              fontSize: 16,
              color: AppColors.textSecondary,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildConfirmView() {
    if (_userInfo == null) return _buildLoadingView();

    return SafeArea(
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Column(
          children: [
            const SizedBox(height: 40),
            ClipRRect(
              borderRadius: BorderRadius.circular(12),
              child: Image.network(
                'https://image.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png',
                width: 120,
                height: 80,
                fit: BoxFit.contain,
                errorBuilder: (_, __, ___) => Container(
                  width: 120,
                  height: 80,
                  color: AppColors.divider,
                  child: const Icon(
                    Icons.account_circle,
                    size: 60,
                    color: AppColors.textHint,
                  ),
                ),
              ),
            ),
            const SizedBox(height: 32),
            const Text(
              '确认登录以下账号？',
              style: TextStyle(
                fontSize: 20,
                fontWeight: FontWeight.w500,
                color: AppColors.textPrimary,
              ),
            ),
            const SizedBox(height: 40),
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(24),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(16),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withValues(alpha: 0.05),
                    blurRadius: 10,
                    offset: const Offset(0, 2),
                  ),
                ],
              ),
              child: Column(
                children: [
                  Container(
                    width: 80,
                    height: 80,
                    decoration: BoxDecoration(
                      shape: BoxShape.circle,
                      border: Border.all(color: AppColors.divider, width: 2),
                    ),
                    child: ClipOval(
                      child: CachedNetworkImage(
                        imageUrl: _userInfo!.avatar,
                        fit: BoxFit.cover,
                        placeholder: (_, __) => Container(
                          color: AppColors.divider,
                          child: const Icon(
                            Icons.person,
                            size: 40,
                            color: AppColors.textHint,
                          ),
                        ),
                        errorWidget: (_, __, ___) => Container(
                          color: AppColors.divider,
                          child: const Icon(
                            Icons.person,
                            size: 40,
                            color: AppColors.textHint,
                          ),
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),
                  Text(
                    _userInfo!.nickname,
                    style: const TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.w500,
                      color: AppColors.textPrimary,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    _userInfo!.phone,
                    style: const TextStyle(
                      fontSize: 14,
                      color: AppColors.textSecondary,
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 40),
            SizedBox(
              width: double.infinity,
              height: 48,
              child: ElevatedButton(
                onPressed: _confirmLogin,
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primary,
                  foregroundColor: Colors.white,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(24),
                  ),
                ),
                child: const Text(
                  '确认登录',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ),
            ),
            const SizedBox(height: 16),
            SizedBox(
              width: double.infinity,
              height: 48,
              child: OutlinedButton(
                onPressed: _cancelAndRescan,
                style: OutlinedButton.styleFrom(
                  foregroundColor: AppColors.textSecondary,
                  side: const BorderSide(color: AppColors.divider),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(24),
                  ),
                ),
                child: const Text(
                  '取消',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildErrorView() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.error_outline, color: AppColors.error, size: 64),
            const SizedBox(height: 16),
            Text(
              _errorMessage ?? '发生错误',
              textAlign: TextAlign.center,
              style: const TextStyle(
                fontSize: 16,
                color: AppColors.textSecondary,
              ),
            ),
            const SizedBox(height: 24),
            ElevatedButton(
              onPressed: _cancelAndRescan,
              style: ElevatedButton.styleFrom(
                backgroundColor: AppColors.primary,
                foregroundColor: Colors.white,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(24),
                ),
              ),
              child: const Text('重新扫描'),
            ),
          ],
        ),
      ),
    );
  }

  String _getScannerErrorMessage(MobileScannerException error) {
    switch (error.errorCode) {
      case MobileScannerErrorCode.permissionDenied:
        return '相机权限被拒绝，请在设置中授权';
      case MobileScannerErrorCode.controllerUninitialized:
        return '扫描器未正确初始化';
      case MobileScannerErrorCode.controllerDisposed:
        return '扫描器已被释放';
      default:
        return error.errorDetails?.message ?? '扫描器发生未知错误';
    }
  }
}
