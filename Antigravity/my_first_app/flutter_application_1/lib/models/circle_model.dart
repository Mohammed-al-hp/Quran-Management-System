class CircleModel {
  final String id;
  final String name;
  final String teacherId;
  final int currentStudents;
  final int maxCapacity;
  
  CircleModel({
    required this.id,
    required this.name,
    required this.teacherId,
    required this.currentStudents,
    required this.maxCapacity,
  });

  double get capacityPercentage {
    return maxCapacity == 0 ? 0 : currentStudents / maxCapacity;
  }
}
