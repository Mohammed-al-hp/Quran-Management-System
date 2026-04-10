import 'package:flutter/material.dart';
import 'package:syncfusion_flutter_charts/charts.dart';
import '../theme/app_theme.dart';
import '../services/api_service.dart';

class DashboardScreen extends StatelessWidget {
  const DashboardScreen({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text(
          'لوحة التحكم',
          style: TextStyle(fontWeight: FontWeight.bold),
        ),
      ),
      body: FutureBuilder<Map<String, dynamic>>(
        future: ApiService().getDashboardStats(),
        builder: (context, snapshot) {
          // 1. حالة التحميل: إظهار مؤشر الدوران بدلاً من الشاشة الحمراء
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }

          // 2. معالجة البيانات: التأكد من وجود قيم افتراضية حتى لو فشل الـ API
          final data =
              snapshot.data ??
              {
                'circlesCount': 0,
                'teachersCount': 0,
                'studentsCount': 0,
                'maxCapacity': 52,
              };

          // استخراج القيم بأمان كأرقام
          final int students = (data['studentsCount'] ?? 0) as int;
          final int maxCap = (data['maxCapacity'] ?? 52) as int;

          return SingleChildScrollView(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Text(
                  'نظرة عامة',
                  style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 16),
                Row(
                  children: [
                    Expanded(
                      child: _buildNeumorphicStatCard(
                        context,
                        title: 'الحلقات',
                        value: (data['circlesCount'] ?? 0).toString(),
                        icon: Icons.group_work,
                        gradientColors: [
                          AppTheme.emeraldGreen,
                          const Color(0xFF14937B),
                        ],
                      ),
                    ),
                    const SizedBox(width: 16),
                    Expanded(
                      child: _buildNeumorphicStatCard(
                        context,
                        title: 'المحفظين',
                        value: (data['teachersCount'] ?? 0).toString(),
                        icon: Icons.person_pin,
                        gradientColors: [
                          AppTheme.matteGold,
                          const Color(0xFFF0CE5E),
                        ],
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 16),
                _buildNeumorphicStatCard(
                  context,
                  title: 'إجمالي الطلاب المقيدين',
                  value: '$students / $maxCap',
                  icon: Icons.menu_book,
                  gradientColors: [
                    const Color(0xFF2C3E50),
                    const Color(0xFF4CA1AF),
                  ],
                  isFullWidth: true,
                ),
                const SizedBox(height: 32),
                const Text(
                  'توزيع الطلاب على الحلقات',
                  style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 16),
                Container(
                  height: 300,
                  decoration: AppTheme.neumorphicBox(context),
                  padding: const EdgeInsets.all(16),
                  child: SfCircularChart(
                    legend: const Legend(
                      isVisible: true,
                      position: LegendPosition.bottom,
                    ),
                    series: <PieSeries<_ChartData, String>>[
                      PieSeries<_ChartData, String>(
                        dataSource: [
                          _ChartData('الطلاب المقيدين', students.toDouble()),
                          _ChartData(
                            'المقاعد الشاغرة',
                            (maxCap - students).toDouble(),
                          ),
                        ],
                        xValueMapper: (_ChartData d, _) => d.x,
                        yValueMapper: (_ChartData d, _) => d.y,
                        dataLabelSettings: const DataLabelSettings(
                          isVisible: true,
                        ),
                        enableTooltip: true,
                      ),
                    ],
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _buildNeumorphicStatCard(
    BuildContext context, {
    required String title,
    required String value,
    required IconData icon,
    required List<Color> gradientColors,
    bool isFullWidth = false,
  }) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: AppTheme.neumorphicBox(context),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              gradient: LinearGradient(
                colors: gradientColors,
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
              ),
              shape: BoxShape.circle,
            ),
            child: Icon(icon, color: Colors.white, size: 28),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: const TextStyle(
                    fontSize: 14,
                    color: Colors.grey,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  value,
                  style: const TextStyle(
                    fontSize: 24,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _ChartData {
  _ChartData(this.x, this.y);
  final String x;
  final double y;
}
