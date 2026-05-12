import 'dart:convert';
import 'package:http/http.dart' as http;

class ApiService {
  static const String baseUrl = "https://localhost:7174/api";
  static String? _username;
  static String? _email;
  // تخزين بسيط في الذاكرة (يعمل على الويب)
  static String? _token;
  static String? _userRole;

  static Future<void> saveToken(String token) async => _token = token;
  static Future<String?> getToken() async => _token;
  static Future<void> saveUserInfo(
    String role,
    String userId,
    String studentId,
  ) async {
    _userRole = role;
  }

  static Future<String?> getUserRole() async => _userRole;
  static Future<bool> isLoggedIn() async => _token != null;
  static Future<void> logout() async {
    _token = null;
    _userRole = null;
  }

  static Map<String, String> _authHeaders() => {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
    if (_token != null) 'Authorization': 'Bearer $_token',
  };

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
          _username = username;
          _email = username;
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
      return {'success': false, 'message': 'تعذر الاتصال بالسيرفر: $e'};
    }
  }

  Future<Map<String, dynamic>> getStudentData() async {
    try {
      final username = _username ?? '';
      final response = await http
          .get(
            Uri.parse('$baseUrl/flutter/student/me?username=$username'),
            headers: _authHeaders(),
          )
          .timeout(const Duration(seconds: 15));

      if (response.statusCode == 200) return json.decode(response.body);
      return {};
    } catch (e) {
      return {};
    }
  }

  Future<Map<String, dynamic>> getParentData() async {
    try {
      final email = _email ?? '';
      final response = await http
          .get(
            Uri.parse('$baseUrl/flutter/parent/me?email=$email'),
            headers: _authHeaders(),
          )
          .timeout(const Duration(seconds: 15));

      if (response.statusCode == 200) return json.decode(response.body);
      return {};
    } catch (e) {
      return {};
    }
  }

  Future<Map<String, dynamic>> getChildDetails(int studentId) async {
    try {
      final response = await http
          .get(
            Uri.parse('$baseUrl/flutter/parent/child/$studentId'),
            headers: _authHeaders(),
          )
          .timeout(const Duration(seconds: 15));

      if (response.statusCode == 200) return json.decode(response.body);
      return {};
    } catch (e) {
      return {};
    }
  }
}
