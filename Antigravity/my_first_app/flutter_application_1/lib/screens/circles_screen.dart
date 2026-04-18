import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/quran_center_provider.dart';
import '../models/circle_model.dart';
import '../theme/app_theme.dart';

class CirclesScreen extends StatelessWidget {
  const CirclesScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final provider = Provider.of<QuranCenterProvider>(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text(
          'إدارة الحلقات',
          style: TextStyle(fontWeight: FontWeight.bold),
        ),
      ),
      body: GridView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: provider.circles.length,
        gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
          crossAxisCount: 2,
          crossAxisSpacing: 16,
          mainAxisSpacing: 16,
          childAspectRatio: 0.85,
        ),
        itemBuilder: (context, index) {
          return _buildCircleCard(context, provider.circles[index], provider);
        },
      ),
    );
  }

  Widget _buildCircleCard(
    BuildContext context,
    CircleModel circle,
    QuranCenterProvider provider,
  ) {
    final teacher = provider.teachers.cast<dynamic>().firstWhere(
      (t) => t.id == circle.teacherId,
      orElse: () => null,
    );
    final teacherName = teacher?.name ?? 'غير محدد';
    final isFull = circle.currentStudents >= circle.maxCapacity;

    return Container(
      decoration: AppTheme.neumorphicBox(context),
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: isFull
                  ? Colors.red.withOpacity(0.1)
                  : AppTheme.lightEmerald,
              borderRadius: BorderRadius.circular(8),
            ),
            child: Icon(
              isFull ? Icons.warning_amber_rounded : Icons.library_books,
              color: isFull ? Colors.red : AppTheme.emeraldGreen,
            ),
          ),
          const Spacer(),
          Text(
            circle.name,
            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
          ),
          const SizedBox(height: 4),
          Text(
            'المحفظ: $teacherName',
            style: const TextStyle(fontSize: 12, color: Colors.grey),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
          ),
          const SizedBox(height: 16),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                '${circle.currentStudents} / ${circle.maxCapacity}',
                style: const TextStyle(
                  fontWeight: FontWeight.w600,
                  fontSize: 12,
                ),
              ),
              Text(
                isFull ? 'مكتملة' : 'متاحة',
                style: TextStyle(
                  fontSize: 10,
                  fontWeight: FontWeight.bold,
                  color: isFull ? Colors.red : AppTheme.emeraldGreen,
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          ClipRRect(
            borderRadius: BorderRadius.circular(4),
            child: LinearProgressIndicator(
              value: circle.capacityPercentage,
              backgroundColor: Colors.grey.withOpacity(0.2),
              valueColor: AlwaysStoppedAnimation<Color>(
                isFull ? Colors.red : AppTheme.matteGold,
              ),
              minHeight: 6,
            ),
          ),
        ],
      ),
    );
  }
}
