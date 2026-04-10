import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:animations/animations.dart';
import '../providers/quran_center_provider.dart';
import '../models/teacher_model.dart';
import '../theme/app_theme.dart';
import 'add_teacher_screen.dart';

class TeachersListScreen extends StatelessWidget {
  const TeachersListScreen({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('قائمة المحفظين', style: TextStyle(fontWeight: FontWeight.bold)),
      ),
      body: Consumer<QuranCenterProvider>(
        builder: (context, provider, child) {
          if (provider.teachers.isEmpty) {
            return const Center(child: Text('لا يوجد محفظين حالياً'));
          }

          return Theme(
            data: Theme.of(context).copyWith(
              canvasColor: Colors.transparent,
            ),
            child: ReorderableListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: provider.teachers.length,
              onReorder: (oldIndex, newIndex) {
                provider.reorderTeachers(oldIndex, newIndex);
              },
              itemBuilder: (context, index) {
                final teacher = provider.teachers[index];
                return _buildTeacherCard(context, teacher, ValueKey(teacher.id));
              },
            ),
          );
        },
      ),
      floatingActionButton: OpenContainer(
        transitionType: ContainerTransitionType.fadeThrough,
        openBuilder: (context, _) => const AddTeacherScreen(),
        closedElevation: 6,
        closedShape: const RoundedRectangleBorder(
          borderRadius: BorderRadius.all(Radius.circular(16)),
        ),
        closedColor: AppTheme.emeraldGreen,
        closedBuilder: (context, openContainer) => FloatingActionButton(
          onPressed: openContainer,
          backgroundColor: AppTheme.emeraldGreen,
          child: const Icon(Icons.person_add, color: Colors.white),
        ),
      ),
    );
  }

  Widget _buildTeacherCard(BuildContext context, TeacherModel teacher, Key key) {
    return Container(
      key: key,
      margin: const EdgeInsets.only(bottom: 16),
      decoration: AppTheme.neumorphicBox(context),
      child: ListTile(
        contentPadding: const EdgeInsets.all(12),
        leading: Hero(
          tag: 'teacher_${teacher.id}',
          child: CircleAvatar(
            radius: 30,
            backgroundColor: AppTheme.lightEmerald,
            backgroundImage: teacher.imagePath.isNotEmpty 
                ? NetworkImage(teacher.imagePath) 
                : null,
            child: teacher.imagePath.isEmpty
                ? const Icon(Icons.person, color: AppTheme.emeraldGreen, size: 30)
                : null,
          ),
        ),
        title: Text(
          teacher.name,
          style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
        ),
        subtitle: Padding(
          padding: const EdgeInsets.only(top: 8.0),
          child: Row(
            children: [
              const Icon(Icons.group, size: 16, color: Colors.grey),
              const SizedBox(width: 4),
              Text('${teacher.studentsCount} طالب', style: const TextStyle(fontSize: 12)),
              const SizedBox(width: 16),
              const Icon(Icons.calendar_today, size: 16, color: Colors.grey),
              const SizedBox(width: 4),
              Text(teacher.joinDate, style: const TextStyle(fontSize: 12)),
            ],
          ),
        ),
        trailing: const Icon(Icons.drag_handle, color: Colors.grey),
      ),
    );
  }
}
