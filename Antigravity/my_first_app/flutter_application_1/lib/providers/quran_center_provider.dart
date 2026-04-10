import 'package:flutter/material.dart';
import '../models/teacher_model.dart';
import '../models/circle_model.dart';
import 'dart:math';

class QuranCenterProvider with ChangeNotifier {
  List<TeacherModel> _teachers = [];
  List<CircleModel> _circles = [];

  List<TeacherModel> get teachers => _teachers;
  List<CircleModel> get circles => _circles;

  QuranCenterProvider() {
    _loadDummyData();
  }

  void _loadDummyData() {
    _circles = [
      CircleModel(id: 'c1', name: 'حلقة الإمام الشافعي', teacherId: 't1', currentStudents: 15, maxCapacity: 20),
      CircleModel(id: 'c2', name: 'حلقة الإمام مالك', teacherId: 't2', currentStudents: 22, maxCapacity: 25),
      CircleModel(id: 'c3', name: 'حلقة الإمام أحمد', teacherId: 't3', currentStudents: 10, maxCapacity: 15),
      CircleModel(id: 'c4', name: 'حلقة الإمام أبو حنيفة', teacherId: '', currentStudents: 5, maxCapacity: 15),
    ];

    _teachers = [
      TeacherModel(id: 't1', name: 'الشيخ أحمد محمود', joinDate: '2023-01-15', studentsCount: 15, circleId: 'c1'),
      TeacherModel(id: 't2', name: 'الشيخ محمد علي', joinDate: '2022-11-01', studentsCount: 22, circleId: 'c2'),
      TeacherModel(id: 't3', name: 'الشيخ عبد الله حسن', joinDate: '2024-02-10', studentsCount: 10, circleId: 'c3'),
    ];
  }

  void addTeacher(TeacherModel teacher) {
    _teachers.add(teacher);
    
    // Update circle's student count logic if needed for demo
    int circleIndex = _circles.indexWhere((c) => c.id == teacher.circleId);
    if(circleIndex != -1) {
      final oldCircle = _circles[circleIndex];
      _circles[circleIndex] = CircleModel(
        id: oldCircle.id, 
        name: oldCircle.name, 
        teacherId: teacher.id, 
        currentStudents: min(oldCircle.currentStudents + teacher.studentsCount, oldCircle.maxCapacity), 
        maxCapacity: oldCircle.maxCapacity
      );
    }
    notifyListeners();
  }

  void reorderTeachers(int oldIndex, int newIndex) {
    if (newIndex > oldIndex) {
      newIndex -= 1;
    }
    final TeacherModel item = _teachers.removeAt(oldIndex);
    _teachers.insert(newIndex, item);
    notifyListeners();
  }
}
