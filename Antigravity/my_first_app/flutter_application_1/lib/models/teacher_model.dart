class TeacherModel {
  final String id;
  final String name;
  final String imagePath; // Dummy image path or url
  final String joinDate;
  final int studentsCount;
  final String circleId;

  TeacherModel({
    required this.id,
    required this.name,
    this.imagePath = '',
    required this.joinDate,
    required this.studentsCount,
    required this.circleId,
  });
}
