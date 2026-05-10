import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// خدمة API الرئيسية - تتصل بمشروع ASP.NET
class ApiService {
  // ⚠️ غيّر هذا الرابط إذا تغير البورت
  static const String baseUrl = "https://localhost:7174/api";
  static const FlutterSecureStorage _storage = FlutterSecureStorage();

  // ==================== إدارة التوكن ====================

  static Future<void> saveToken(String token) async =>
      await _storage.write(key: 'jwt_token', value: token);

  static Future<String?> getToken() async =>
      await _storage.read(key: 'jwt_token');

  static Future<void> saveUserInfo(
    String role,
    String userId,
    String studentId,
  ) async {
    await _storage.write(key: 'user_role', value: role);
    await _storage.write(key: 'user_id', value: userId);
    await _storage.write(key: 'student_id', value: studentId);
  }

  static Future<String?> getUserRole() async =>
      await _storage.read(key: 'user_role');

  static Future<String?> getStudentId() async =>
      await _storage.read(key: 'student_id');

  static Future<bool> isLoggedIn() async {
    final token = await getToken();
    return token != null && token.isNotEmpty;
  }

  static Future<void> logout() async => await _storage.deleteAll();

  static Future<Map<String, String>> _authHeaders() async {
    final token = await getToken();
    return {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
  }

  // ==================== تسجيل الدخول ====================

  /// تسجيل الدخول والحصول على JWT
  Future<Map<String, dynamic>> login(String username, String password) async {
    try {
      final response = await http
          .post(
            Uri.parse('$baseUrl/auth/login'),
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode({'username': username, 'password': password}),
          )
          .timeout(const Duration(seconds: 15));

      if (response.statusCode == 200) {
        final data = json.decode(response.body);
        if (data['token'] != null) {
          await saveToken(data['token']);
          await saveUserInfo(
            data['role'] ?? 'Student',
            data['userId'] ?? '',
            '',
          );
        }
        return {'success': true, ...data};
      }

      final error = json.decode(response.body);
      return {
        'success': false,
        'message': error['message'] ?? 'بيانات الدخول غير صحيحة',
      };
    } catch (e) {
      return {'success': false, 'message': 'تعذر الاتصال بالسيرفر'};
    }
  }

  // ==================== بيانات الطالب ====================

  /// جلب كل بيانات الطالب (الحضور + الحفظ + الحلقة)
  Future<Map<String, dynamic>> getStudentData() async {
    try {
      final headers = await _authHeaders();
      final response = await http
          .get(Uri.parse('$baseUrl/flutter/student/me'), headers: headers)
          .timeout(const Duration(seconds: 15));

      if (response.statusCode == 200) {
        return json.decode(response.body);
      }
      return {};
    } catch (e) {
      return {};
    }
  }

  /// جلب إشعارات الطالب
  Future<List<dynamic>> getStudentNotifications(int studentId) async {
    try {
      final headers = await _authHeaders();
      final response = await http
          .get(
            Uri.parse('$baseUrl/flutter/student/$studentId/notifications'),
            headers: headers,
          )
          .timeout(const Duration(seconds: 15));

      if (response.statusCode == 200) {
        return json.decode(response.body);
      }
      return [];
    } catch (e) {
      return [];
    }
  }

  /// جلب جدول الطالب
  Future<Map<String, dynamic>> getStudentSchedule(int studentId) async {
    try {
      final headers = await _authHeaders();
      final response = await http
          .get(
            Uri.parse('$baseUrl/flutter/student/$studentId/schedule'),
            headers: headers,
          )
          .timeout(const Duration(seconds: 15));

      if (response.statusCode == 200) {
        return json.decode(response.body);
      }
      return {};
    } catch (e) {
      return {};
    }
  }

  // ==================== بيانات ولي الأمر ====================

  /// جلب بيانات ولي الأمر وأبنائه
  Future<Map<String, dynamic>> getParentData() async {
    try {
      final headers = await _authHeaders();
      final response = await http
          .get(Uri.parse('$baseUrl/flutter/parent/me'), headers: headers)
          .timeout(const Duration(seconds: 15));

      if (response.statusCode == 200) {
        return json.decode(response.body);
      }
      return {};
    } catch (e) {
      return {};
    }
  }

  /// جلب تفاصيل ابن محدد
  Future<Map<String, dynamic>> getChildDetails(int studentId) async {
    try {
      final headers = await _authHeaders();
      final response = await http
          .get(
            Uri.parse('$baseUrl/flutter/parent/child/$studentId'),
            headers: headers,
          )
          .timeout(const Duration(seconds: 15));

      if (response.statusCode == 200) {
        return json.decode(response.body);
      }
      return {};
    } catch (e) {
      return {};
    }
  }
}
