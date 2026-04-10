import 'dart:convert';
import 'package:http/http.dart' as http;

class ApiService {
  static const String baseUrl = "https://localhost:7174/api";

  Future<List<dynamic>> getTeachers() async {
    try {
      final response = await http.get(Uri.parse('$baseUrl/TeachersApi'));
      if (response.statusCode == 200) {
        return json.decode(response.body);
      } else {
        return [];
      }
    } catch (e) {
      print("Error fetching teachers: $e");
      return [];
    }
  }

  Future<Map<String, dynamic>> getDashboardStats() async {
    try {
      final response = await http.get(Uri.parse('$baseUrl/TeachersApi/stats'));
      if (response.statusCode == 200) {
        return json.decode(response.body);
      }
      // إرجاع قيم افتراضية في حال وجود خطأ من السيرفر
      return {
        'circlesCount': 0,
        'teachersCount': 0,
        'studentsCount': 0,
        'maxCapacity': 52,
      };
    } catch (e) {
      print("Error fetching stats: $e");
      return {
        'circlesCount': 0,
        'teachersCount': 0,
        'studentsCount': 0,
        'maxCapacity': 52,
      };
    }
  }
}
