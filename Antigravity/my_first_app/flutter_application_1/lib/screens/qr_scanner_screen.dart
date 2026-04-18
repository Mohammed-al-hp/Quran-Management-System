import 'package:flutter/material.dart';
import 'package:mobile_scanner/mobile_scanner.dart';
import '../services/api_service.dart';

/// شاشة مسح رمز QR لتسجيل حضور الطلاب
/// يستخدمها المحفظ لمسح رمز الطالب وتسجيل حضوره تلقائياً
class QrScannerScreen extends StatefulWidget {
  const QrScannerScreen({super.key});

  @override
  State<QrScannerScreen> createState() => _QrScannerScreenState();
}

class _QrScannerScreenState extends State<QrScannerScreen> {
  final MobileScannerController _scannerController = MobileScannerController();
  final ApiService _apiService = ApiService();
  bool _isProcessing = false;
  String? _lastScannedCode;

  @override
  void dispose() {
    _scannerController.dispose();
    super.dispose();
  }

  Future<void> _handleScan(String code) async {
    // منع المسح المتكرر لنفس الرمز
    if (_isProcessing || code == _lastScannedCode) return;

    setState(() {
      _isProcessing = true;
      _lastScannedCode = code;
    });

    final result = await _apiService.scanQrAttendance(code);

    if (mounted) {
      final isSuccess = result['success'] == true;

      showDialog(
        context: context,
        builder: (ctx) => AlertDialog(
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
          ),
          icon: Icon(
            isSuccess ? Icons.check_circle : Icons.error,
            color: isSuccess ? Colors.green : Colors.red,
            size: 60,
          ),
          title: Text(
            isSuccess ? 'تم التسجيل بنجاح' : 'خطأ',
            style: const TextStyle(fontWeight: FontWeight.bold),
          ),
          content: Text(
            result['message'] ?? 'حدث خطأ غير متوقع',
            textAlign: TextAlign.center,
          ),
          actions: [
            TextButton(
              onPressed: () {
                Navigator.pop(ctx);
                setState(() {
                  _isProcessing = false;
                  _lastScannedCode = null;
                });
              },
              child: const Text('متابعة المسح'),
            ),
          ],
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    const Color primaryTeal = Color(0xFF1D5D5D);

    return Scaffold(
      appBar: AppBar(
        title: const Text(
          'مسح QR للحضور',
          style: TextStyle(fontWeight: FontWeight.bold),
        ),
        backgroundColor: primaryTeal,
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.flash_on),
            onPressed: () => _scannerController.toggleTorch(),
          ),
          IconButton(
            icon: const Icon(Icons.flip_camera_android),
            onPressed: () => _scannerController.switchCamera(),
          ),
        ],
      ),
      body: Stack(
        children: [
          MobileScanner(
            controller: _scannerController,
            onDetect: (capture) {
              final barcode = capture.barcodes.firstOrNull;
              if (barcode?.rawValue != null) {
                _handleScan(barcode!.rawValue!);
              }
            },
          ),
          // إطار المسح المخصص
          Center(
            child: Container(
              width: 280,
              height: 280,
              decoration: BoxDecoration(
                border: Border.all(color: primaryTeal, width: 3),
                borderRadius: BorderRadius.circular(20),
              ),
            ),
          ),
          // التعليمات
          Positioned(
            bottom: 80,
            left: 0,
            right: 0,
            child: Container(
              margin: const EdgeInsets.symmetric(horizontal: 30),
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: Colors.black.withOpacity(0.7),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Text(
                _isProcessing
                    ? 'جاري معالجة الرمز...'
                    : 'وجّه الكاميرا نحو رمز QR الخاص بالطالب',
                textAlign: TextAlign.center,
                style: const TextStyle(color: Colors.white, fontSize: 16),
              ),
            ),
          ),
          // مؤشر التحميل
          if (_isProcessing)
            Container(
              color: Colors.black26,
              child: const Center(
                child: CircularProgressIndicator(color: Colors.white),
              ),
            ),
        ],
      ),
    );
  }
}
