import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// خدمة API الرئيسية - تدير الاتصال بالخادم مع دعم JWT
/// تستخدم flutter_secure_storage لحفظ الرمز بشكل آمن
class ApiService {
  static const String baseUrl = "https://localhost:7174/api";
  static const FlutterSecureStorage _storage = FlutterSecureStorage();

  // ==================== إدارة الرمز ====================

  /// حفظ رمز JWT بشكل آمن
  static Future<void> saveToken(String token) async {
    await _storage.write(key: 'jwt_token', value: token);
  }

  /// جلب رمز JWT المحفوظ
  static Future<String?> getToken() async {
    return await _storage.read(key: 'jwt_token');
  }

  /// حذف رمز JWT (تسجيل خروج)
  static Future<void> deleteToken() async {
    await _storage.delete(key: 'jwt_token');
  }

  /// حفظ معلومات المستخدم
  static Future<void> saveUserInfo(
    String role,
    String email,
    String userId,
  ) async {
    await _storage.write(key: 'user_role', value: role);
    await _storage.write(key: 'user_email', value: email);
    await _storage.write(key: 'user_id', value: userId);
  }

  /// جلب دور المستخدم
  static Future<String?> getUserRole() async {
    return await _storage.read(key: 'user_role');
  }

  /// التحقق مما إذا كان المستخدم مسجل الدخول
  static Future<bool> isLoggedIn() async {
    final token = await getToken();
    return token != null && token.isNotEmpty;
  }

  /// تسجيل الخروج
  static Future<void> logout() async {
    await _storage.deleteAll();
  }

  /// إنشاء Headers مع رمز المصادقة
  static Future<Map<String, String>> _authHeaders() async {
    final token = await getToken();
    return {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
  }

  // ==================== المصادقة ====================

  /// تسجيل الدخول والحصول على رمز JWT
  Future<Map<String, dynamic>> login(String email, String password) async {
    try {
      final response = await http.post(
        Uri.parse('$baseUrl/auth/login'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({'username': email, 'password': password}),
      );

      if (response.statusCode == 200) {
        final data = json.decode(response.body);
        // حفظ الرمز ومعلومات المستخدم
        if (data['token'] != null) {
          await saveToken(data['token']);
          await saveUserInfo(
            data['role'] ?? 'Student',
            data['email'] ?? '',
            data['userId'] ?? '',
          );
        }
        return data;
      }
      return {'success': false, 'message': 'بيانات الدخول غير صحيحة'};
    } catch (e) {
      return {'success': false, 'message': 'تعذر الاتصال بالسيرفر: $e'};
    }
  }

  // ==================== لوحة التحكم ====================

  /// جلب إحصائيات لوحة التحكم
  Future<Map<String, dynamic>> getDashboardStats() async {
    try {
      final headers = await _authHeaders();
      final response = await http.get(
        Uri.parse('$baseUrl/dashboard/stats'),
        headers: headers,
      );

      if (response.statusCode == 200) {
        return json.decode(response.body);
      }
      return _defaultStats();
    } catch (e) {
      print("Error fetching stats: $e");
      return _defaultStats();
    }
  }

  /// جلب التحليلات الذكية
  Future<Map<String, dynamic>> getAnalytics() async {
    try {
      final headers = await _authHeaders();
      final response = await http.get(
        Uri.parse('$baseUrl/dashboard/analytics'),
        headers: headers,
      );

      if (response.statusCode == 200) {
        return json.decode(response.body);
      }
      return {};
    } catch (e) {
      print("Error fetching analytics: $e");
      return {};
    }
  }

  // ==================== المحفظون ====================

  /// جلب قائمة المحفظين
  Future<List<dynamic>> getTeachers() async {
    try {
      final headers = await _authHeaders();
      final response = await http.get(
        Uri.parse('$baseUrl/TeachersApi'),
        headers: headers,
      );

      if (response.statusCode == 200) {
        return json.decode(response.body);
      }
      return [];
    } catch (e) {
      print("Error fetching teachers: $e");
      return [];
    }
  }

  // ==================== الطلاب ====================

  /// جلب قائمة الطلاب
  Future<List<dynamic>> getStudents({int? circleId}) async {
    try {
      final headers = await _authHeaders();
      var url = '$baseUrl/students';
      if (circleId != null) url += '?circleId=$circleId';

      final response = await http.get(Uri.parse(url), headers: headers);
      if (response.statusCode == 200) {
        return json.decode(response.body);
      }
      return [];
    } catch (e) {
      print("Error fetching students: $e");
      return [];
    }
  }

  /// جلب تقدم طالب
  Future<Map<String, dynamic>> getStudentProgress(int studentId) async {
    try {
      final headers = await _authHeaders();
      final response = await http.get(
        Uri.parse('$baseUrl/students/$studentId/progress'),
        headers: headers,
      );

      if (response.statusCode == 200) {
        return json.decode(response.body);
      }
      return {};
    } catch (e) {
      print("Error fetching student progress: $e");
      return {};
    }
  }

  // ==================== الحضور ====================

  /// مسح QR لتسجيل الحضور
  Future<Map<String, dynamic>> scanQrAttendance(String qrToken) async {
    try {
      final headers = await _authHeaders();
      final response = await http.post(
        Uri.parse('$baseUrl/attendance/qr-scan'),
        headers: headers,
        body: jsonEncode({'qrToken': qrToken}),
      );

      return json.decode(response.body);
    } catch (e) {
      return {'success': false, 'message': 'خطأ في الاتصال: $e'};
    }
  }

  /// جلب حضور اليوم لحلقة
  Future<Map<String, dynamic>> getTodayAttendance(int circleId) async {
    try {
      final headers = await _authHeaders();
      final response = await http.get(
        Uri.parse('$baseUrl/attendance/today/$circleId'),
        headers: headers,
      );

      if (response.statusCode == 200) {
        return json.decode(response.body);
      }
      return {};
    } catch (e) {
      return {};
    }
  }

  // ==================== الإشعارات ====================

  /// جلب الإشعارات
  Future<Map<String, dynamic>> getNotifications({
    bool unreadOnly = false,
  }) async {
    try {
      final headers = await _authHeaders();
      final response = await http.get(
        Uri.parse('$baseUrl/notifications?unreadOnly=$unreadOnly'),
        headers: headers,
      );

      if (response.statusCode == 200) {
        return json.decode(response.body);
      }
      return {'notifications': [], 'unreadCount': 0};
    } catch (e) {
      return {'notifications': [], 'unreadCount': 0};
    }
  }

  /// تحديد إشعار كمقروء
  Future<bool> markNotificationRead(int id) async {
    try {
      final headers = await _authHeaders();
      final response = await http.post(
        Uri.parse('$baseUrl/notifications/$id/read'),
        headers: headers,
      );
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  /// تحديد جميع الإشعارات كمقروءة
  Future<bool> markAllNotificationsRead() async {
    try {
      final headers = await _authHeaders();
      final response = await http.post(
        Uri.parse('$baseUrl/notifications/read-all'),
        headers: headers,
      );
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  // ==================== مساعدات ====================

  Map<String, dynamic> _defaultStats() => {
    'circlesCount': 0,
    'teachersCount': 0,
    'studentsCount': 0,
    'todayAttendance': 0,
    'maxCapacity': 52,
  };
}
