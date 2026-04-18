import 'package:flutter/material.dart';
import 'package:syncfusion_flutter_charts/charts.dart';
import '../services/api_service.dart';

/// شاشة تقدم الطالب - تعرض إحصائيات الحضور والحفظ
/// تُستخدم من قبل ولي الأمر والطالب لمتابعة الأداء
class StudentProgressScreen extends StatefulWidget {
  final int studentId;
  const StudentProgressScreen({super.key, required this.studentId});

  @override
  State<StudentProgressScreen> createState() => _StudentProgressScreenState();
}

class _StudentProgressScreenState extends State<StudentProgressScreen> {
  final ApiService _apiService = ApiService();
  Map<String, dynamic> _progressData = {};
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadProgress();
  }

  Future<void> _loadProgress() async {
    final data = await _apiService.getStudentProgress(widget.studentId);
    if (mounted) {
      setState(() {
        _progressData = data;
        _isLoading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    const Color primaryTeal = Color(0xFF1D5D5D);
    const Color goldAccent = Color(0xFFC5A059);

    if (_isLoading) {
      return Scaffold(
        appBar: AppBar(
          title: const Text('تقدم الطالب'),
          backgroundColor: primaryTeal,
          foregroundColor: Colors.white,
        ),
        body: const Center(child: CircularProgressIndicator()),
      );
    }

    final student = _progressData['student'] ?? {};
    final attendance = _progressData['attendance'] ?? {};
    final memorization = _progressData['memorization'] ?? {};

    return Scaffold(
      appBar: AppBar(
        title: Text(student['name'] ?? 'تقدم الطالب'),
        backgroundColor: primaryTeal,
        foregroundColor: Colors.white,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // بطاقة معلومات الطالب
            Card(
              elevation: 2,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(16),
              ),
              child: Padding(
                padding: const EdgeInsets.all(20),
                child: Column(
                  children: [
                    CircleAvatar(
                      radius: 35,
                      backgroundColor: primaryTeal.withOpacity(0.1),
                      child: const Icon(
                        Icons.person,
                        size: 40,
                        color: primaryTeal,
                      ),
                    ),
                    const SizedBox(height: 12),
                    Text(
                      student['name'] ?? '',
                      style: const TextStyle(
                        fontSize: 22,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 4,
                      ),
                      decoration: BoxDecoration(
                        color: primaryTeal.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: Text(
                        student['circleName'] ?? 'غير محدد',
                        style: const TextStyle(
                          color: primaryTeal,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),

            // إحصائيات الحضور
            const Text(
              'الحضور',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 8),
            Row(
              children: [
                _buildStatCard(
                  'حاضر',
                  '${attendance['present'] ?? 0}',
                  Colors.green,
                ),
                const SizedBox(width: 8),
                _buildStatCard(
                  'غائب',
                  '${attendance['absent'] ?? 0}',
                  Colors.red,
                ),
                const SizedBox(width: 8),
                _buildStatCard(
                  'النسبة',
                  attendance['attendanceRate'] ?? 'N/A',
                  primaryTeal,
                ),
              ],
            ),
            const SizedBox(height: 20),

            // رسم بياني للتقييمات
            const Text(
              'توزيع التقييمات',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 8),
            Container(
              height: 250,
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(16),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.05),
                    blurRadius: 10,
                    offset: const Offset(0, 4),
                  ),
                ],
              ),
              padding: const EdgeInsets.all(12),
              child: SfCircularChart(
                legend: const Legend(
                  isVisible: true,
                  position: LegendPosition.bottom,
                ),
                series: <PieSeries<_GradeData, String>>[
                  PieSeries<_GradeData, String>(
                    dataSource: [
                      _GradeData(
                        'ممتاز',
                        (memorization['excellent'] ?? 0).toDouble(),
                        Colors.green,
                      ),
                      _GradeData(
                        'جيد جداً',
                        (memorization['veryGood'] ?? 0).toDouble(),
                        Colors.blue,
                      ),
                      _GradeData(
                        'جيد',
                        (memorization['good'] ?? 0).toDouble(),
                        Colors.orange,
                      ),
                      _GradeData(
                        'مقبول',
                        (memorization['fair'] ?? 0).toDouble(),
                        Colors.red,
                      ),
                    ],
                    xValueMapper: (d, _) => d.grade,
                    yValueMapper: (d, _) => d.count,
                    pointColorMapper: (d, _) => d.color,
                    dataLabelSettings: const DataLabelSettings(isVisible: true),
                    enableTooltip: true,
                  ),
                ],
              ),
            ),
            const SizedBox(height: 20),

            // آخر السجلات
            const Text(
              'آخر سجلات الحفظ',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 8),
            if (memorization['grades'] != null)
              ...List.generate(
                (memorization['grades'] as List).length.clamp(0, 10),
                (index) {
                  final record = memorization['grades'][index];
                  return Card(
                    margin: const EdgeInsets.only(bottom: 8),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: ListTile(
                      leading: CircleAvatar(
                        backgroundColor: _gradeColor(
                          record['grade'],
                        ).withOpacity(0.15),
                        child: Text(
                          _gradeEmoji(record['grade']),
                          style: const TextStyle(fontSize: 20),
                        ),
                      ),
                      title: Text(record['surahName'] ?? ''),
                      subtitle: Text(
                        '${record['type'] ?? ''} • ${record['date'] ?? ''}',
                      ),
                      trailing: Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 12,
                          vertical: 4,
                        ),
                        decoration: BoxDecoration(
                          color: _gradeColor(record['grade']).withOpacity(0.15),
                          borderRadius: BorderRadius.circular(20),
                        ),
                        child: Text(
                          record['grade'] ?? '',
                          style: TextStyle(
                            color: _gradeColor(record['grade']),
                            fontWeight: FontWeight.bold,
                            fontSize: 12,
                          ),
                        ),
                      ),
                    ),
                  );
                },
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildStatCard(String label, String value, Color color) {
    return Expanded(
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: color.withOpacity(0.1),
          borderRadius: BorderRadius.circular(12),
          border: Border.all(color: color.withOpacity(0.3)),
        ),
        child: Column(
          children: [
            Text(
              value,
              style: TextStyle(
                fontSize: 22,
                fontWeight: FontWeight.bold,
                color: color,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              label,
              style: TextStyle(fontSize: 12, color: color.withOpacity(0.8)),
            ),
          ],
        ),
      ),
    );
  }

  Color _gradeColor(String? grade) {
    switch (grade) {
      case 'ممتاز':
        return Colors.green;
      case 'جيد جداً':
        return Colors.blue;
      case 'جيد':
        return Colors.orange;
      default:
        return Colors.red;
    }
  }

  String _gradeEmoji(String? grade) {
    switch (grade) {
      case 'ممتاز':
        return '⭐';
      case 'جيد جداً':
        return '👍';
      case 'جيد':
        return '📖';
      default:
        return '📝';
    }
  }
}

class _GradeData {
  _GradeData(this.grade, this.count, this.color);
  final String grade;
  final double count;
  final Color color;
}
