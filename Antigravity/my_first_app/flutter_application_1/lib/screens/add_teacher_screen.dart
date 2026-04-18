import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'dart:math';
import '../providers/quran_center_provider.dart';
import '../models/teacher_model.dart';
import '../theme/app_theme.dart';

class AddTeacherScreen extends StatefulWidget {
  const AddTeacherScreen({super.key});

  @override
  State<AddTeacherScreen> createState() => _AddTeacherScreenState();
}

class _AddTeacherScreenState extends State<AddTeacherScreen>
    with SingleTickerProviderStateMixin {
  final _formKey = GlobalKey<FormState>();
  String _name = '';
  int _studentsCount = 0;
  String? _selectedCircleId;

  late AnimationController _pulseController;
  late Animation<double> _pulseAnimation;

  @override
  void initState() {
    super.initState();
    _pulseController = AnimationController(
      vsync: this,
      duration: const Duration(seconds: 2),
    )..repeat(reverse: true);

    _pulseAnimation = Tween<double>(begin: 1.0, end: 1.15).animate(
      CurvedAnimation(parent: _pulseController, curve: Curves.easeInOut),
    );
  }

  @override
  void dispose() {
    _pulseController.dispose();
    super.dispose();
  }

  void _saveTeacher() {
    if (_formKey.currentState!.validate()) {
      if (_selectedCircleId == null) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('يرجى اختيار الحلقة أولاً')),
        );
        return;
      }

      _formKey.currentState!.save();

      final provider = Provider.of<QuranCenterProvider>(context, listen: false);
      final newTeacher = TeacherModel(
        id: DateTime.now().millisecondsSinceEpoch.toString(),
        name: _name,
        joinDate:
            '${DateTime.now().year}-${DateTime.now().month.toString().padLeft(2, '0')}-${DateTime.now().day.toString().padLeft(2, '0')}',
        studentsCount: _studentsCount,
        circleId: _selectedCircleId!,
      );

      provider.addTeacher(newTeacher);
      Navigator.pop(context); // Go back with animation handled by OpenContainer
    }
  }

  @override
  Widget build(BuildContext context) {
    final provider = Provider.of<QuranCenterProvider>(context);
    final circles = provider.circles;

    return Scaffold(
      appBar: AppBar(
        title: const Text(
          'إضافة محفظ جديد',
          style: TextStyle(fontWeight: FontWeight.bold),
        ),
      ),
      body: Form(
        key: _formKey,
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // صوره المحفظ مع تأثير النبض
              Center(
                child: AnimatedBuilder(
                  animation: _pulseAnimation,
                  builder: (context, child) {
                    return Transform.scale(
                      scale: _pulseAnimation.value,
                      child: Container(
                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          boxShadow: [
                            BoxShadow(
                              color: AppTheme.emeraldGreen.withOpacity(0.3),
                              blurRadius: 15,
                              spreadRadius: 5,
                            ),
                          ],
                        ),
                        child: const CircleAvatar(
                          radius: 50,
                          backgroundColor: Colors.white,
                          child: Icon(
                            Icons.add_a_photo,
                            size: 40,
                            color: AppTheme.emeraldGreen,
                          ),
                        ),
                      ),
                    );
                  },
                ),
              ),
              const SizedBox(height: 32),

              // حقول الإدخال
              TextFormField(
                decoration: InputDecoration(
                  labelText: 'اسم المحفظ',
                  prefixIcon: const Icon(Icons.person),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(16),
                  ),
                  filled: true,
                  fillColor: Theme.of(context).brightness == Brightness.dark
                      ? AppTheme.cardDark
                      : Colors.white,
                ),
                validator: (value) =>
                    value!.isEmpty ? 'يرجى إدخال الاسم' : null,
                onSaved: (value) => _name = value!,
              ),
              const SizedBox(height: 16),

              TextFormField(
                decoration: InputDecoration(
                  labelText: 'عدد الطلاب المبدئي',
                  prefixIcon: const Icon(Icons.group),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(16),
                  ),
                  filled: true,
                  fillColor: Theme.of(context).brightness == Brightness.dark
                      ? AppTheme.cardDark
                      : Colors.white,
                ),
                keyboardType: TextInputType.number,
                validator: (value) {
                  if (value == null || value.isEmpty) return 'يرجى إدخال العدد';
                  if (int.tryParse(value) == null) return 'قيمة غير صحيحة';
                  return null;
                },
                onSaved: (value) => _studentsCount = int.parse(value!),
              ),
              const SizedBox(height: 32),

              // اختيار الحلقة - Chip Selection
              const Text(
                'اختر حلقة التحفيظ:',
                style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
              ),
              const SizedBox(height: 12),
              Wrap(
                spacing: 8,
                runSpacing: 12,
                children: circles.map((circle) {
                  final isSelected = _selectedCircleId == circle.id;
                  return ChoiceChip(
                    label: Text(circle.name),
                    selected: isSelected,
                    onSelected: (selected) {
                      setState(() {
                        _selectedCircleId = selected ? circle.id : null;
                      });
                    },
                    selectedColor: AppTheme.emeraldGreen.withOpacity(0.2),
                    labelStyle: TextStyle(
                      color: isSelected
                          ? AppTheme.emeraldGreen
                          : (Theme.of(context).brightness == Brightness.dark
                                ? Colors.white
                                : Colors.black87),
                      fontWeight: isSelected
                          ? FontWeight.bold
                          : FontWeight.normal,
                    ),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                      side: BorderSide(
                        color: isSelected
                            ? AppTheme.emeraldGreen
                            : Colors.grey.withOpacity(0.3),
                      ),
                    ),
                  );
                }).toList(),
              ),

              const SizedBox(height: 48),

              ElevatedButton(
                onPressed: _saveTeacher,
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppTheme.emeraldGreen,
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(16),
                  ),
                  elevation: 2,
                ),
                child: const Text(
                  'إضافة المحفظ',
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
