namespace QuranCenters.Core.Enums
{
    /// <summary>
    /// حالة الحضور
    /// </summary>
    public enum AttendanceStatus
    {
        حاضر = 0,
        غائب = 1,
        مستأذن = 2,
        متأخر = 3
    }

    /// <summary>
    /// تقييم الحفظ
    /// </summary>
    public enum MemorizationGrade
    {
        ممتاز = 4,
        جيد_جداً = 3,
        جيد = 2,
        مقبول = 1,
        ضعيف = 0
    }

    /// <summary>
    /// نوع التسميع
    /// </summary>
    public enum MemorizationType
    {
        حفظ_جديد = 0,
        مراجعة_قريبة = 1,
        مراجعة_بعيدة = 2
    }

    /// <summary>
    /// حالة الطالب
    /// </summary>
    public enum StudentStatus
    {
        نشط = 0,
        غير_نشط = 1,
        منسحب = 2,
        خريج = 3
    }

    /// <summary>
    /// أدوار المستخدمين
    /// </summary>
    public enum UserRole
    {
        Admin = 0,
        Teacher = 1,
        Parent = 2,
        Student = 3
    }
}
