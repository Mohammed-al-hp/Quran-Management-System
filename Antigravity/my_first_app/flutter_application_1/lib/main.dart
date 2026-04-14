import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'dart:convert'; // لتحويل البيانات لـ JSON
import 'package:http/http.dart' as http; // للاتصال بالـ API
import 'theme/app_theme.dart';
import 'providers/quran_center_provider.dart';
import 'screens/dashboard_screen.dart';
import 'screens/circles_screen.dart';
import 'screens/teachers_list_screen.dart';
import 'screens/qr_scanner_screen.dart';
import 'screens/notifications_screen.dart';
import 'screens/student_progress_screen.dart';
import 'services/api_service.dart';

void main() {
  runApp(const QuranCenterApp());
}

class QuranCenterApp extends StatelessWidget {
  const QuranCenterApp({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [ChangeNotifierProvider(create: (_) => QuranCenterProvider())],
      child: MaterialApp(
        title: 'نظام إدارة مراكز التحفيظ',
        debugShowCheckedModeBanner: false,
        theme: AppTheme.lightTheme.copyWith(
          textTheme: ThemeData.light().textTheme.apply(fontFamily: null),
        ),
        darkTheme: AppTheme.darkTheme.copyWith(
          textTheme: ThemeData.dark().textTheme.apply(fontFamily: null),
        ),
        themeMode: ThemeMode.system,
        locale: const Locale('ar', 'SA'),
        builder: (context, child) {
          return Directionality(
            textDirection: TextDirection.rtl,
            child: child!,
          );
        },
        home: const OnboardingScreen(),
      ),
    );
  }
}

// --- واجهة الترحيب ---
class OnboardingScreen extends StatelessWidget {
  const OnboardingScreen({super.key});

  @override
  Widget build(BuildContext context) {
    const Color primaryTeal = Color(0xFF1D5D5D);
    const Color goldAccent = Color(0xFFC5A059);

    return Scaffold(
      backgroundColor: Colors.white,
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 30),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Spacer(),
              const Icon(
                Icons.menu_book_rounded,
                size: 140,
                color: primaryTeal,
              ),
              const SizedBox(height: 24),
              const Text(
                'نظام أهل القرآن',
                style: TextStyle(
                  fontSize: 30,
                  fontWeight: FontWeight.bold,
                  color: primaryTeal,
                ),
              ),
              const Text(
                'تسهيل و تسيير التعليم القرآني',
                style: TextStyle(fontSize: 18, color: goldAccent),
              ),
              const SizedBox(height: 40),
              const Text(
                'نظام أهل القرآن هو نظام سحابي متكامل يهدف لتوظيف آخر ما توصلت إليه التقنية في خدمة تعليم القرآن الكريم',
                textAlign: TextAlign.center,
                style: TextStyle(
                  fontSize: 16,
                  height: 1.6,
                  color: Colors.black54,
                ),
              ),
              const Spacer(),
              SizedBox(
                width: double.infinity,
                height: 55,
                child: ElevatedButton(
                  onPressed: () => Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (context) => const LoginScreen(),
                    ),
                  ),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: primaryTeal,
                    foregroundColor: Colors.white,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                  child: const Text(
                    'ابدأ الآن',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                ),
              ),
              const SizedBox(height: 40),
            ],
          ),
        ),
      ),
    );
  }
}

// --- واجهة تسجيل الدخول مع JWT ---
class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final TextEditingController _usernameController = TextEditingController();
  final TextEditingController _passwordController = TextEditingController();
  final ApiService _apiService = ApiService();
  bool _isLoading = false;

  Future<void> _login() async {
    if (_usernameController.text.isEmpty || _passwordController.text.isEmpty) {
      _showError('يرجى إدخال البريد الإلكتروني وكلمة المرور');
      return;
    }

    setState(() => _isLoading = true);
    try {
      final result = await _apiService.login(
        _usernameController.text.trim(),
        _passwordController.text,
      );

      if (result['success'] == true && mounted) {
        final role = result['role'] ?? 'Student';

        // التوجيه حسب الدور
        Widget targetScreen;
        switch (role) {
          case 'Admin':
            targetScreen = const MainScreen();
            break;
          case 'Teacher':
            targetScreen = const TeacherMainScreen();
            break;
          case 'Parent':
            targetScreen = ParentMainScreen(userId: result['userId'] ?? '');
            break;
          default:
            targetScreen = const MainScreen();
        }

        Navigator.pushReplacement(
          context,
          MaterialPageRoute(builder: (context) => targetScreen),
        );
      } else {
        _showError(result['message'] ?? 'بيانات الدخول غير صحيحة');
      }
    } catch (e) {
      _showError('تعذر الاتصال بالسيرفر. تأكد من تشغيله');
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  void _showError(String msg) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(msg),
        backgroundColor: Colors.red[700],
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    const Color primaryTeal = Color(0xFF1D5D5D);
    return Scaffold(
      backgroundColor: Colors.white,
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        iconTheme: const IconThemeData(color: primaryTeal),
      ),
      body: SingleChildScrollView(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 25),
          child: Column(
            children: [
              const SizedBox(height: 20),
              const Icon(
                Icons.menu_book_rounded,
                size: 100,
                color: primaryTeal,
              ),
              const SizedBox(height: 40),
              TextFormField(
                controller: _usernameController,
                textAlign: TextAlign.right,
                keyboardType: TextInputType.emailAddress,
                decoration: InputDecoration(
                  hintText: 'البريد الإلكتروني',
                  prefixIcon: const Icon(
                    Icons.person_outline,
                    color: primaryTeal,
                  ),
                  filled: true,
                  fillColor: Colors.grey[50],
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: BorderSide.none,
                  ),
                ),
              ),
              const SizedBox(height: 20),
              TextFormField(
                controller: _passwordController,
                textAlign: TextAlign.right,
                obscureText: true,
                decoration: InputDecoration(
                  hintText: 'كلمة المرور',
                  prefixIcon: const Icon(
                    Icons.lock_outline,
                    color: primaryTeal,
                  ),
                  filled: true,
                  fillColor: Colors.grey[50],
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: BorderSide.none,
                  ),
                ),
                onFieldSubmitted: (_) => _login(),
              ),
              const SizedBox(height: 35),
              SizedBox(
                width: double.infinity,
                height: 55,
                child: ElevatedButton(
                  onPressed: _isLoading ? null : _login,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: primaryTeal,
                    foregroundColor: Colors.white,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                  child: _isLoading
                      ? const CircularProgressIndicator(color: Colors.white)
                      : const Text(
                          'تسجيل الدخول',
                          style: TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

// --- الشاشة الرئيسية للمدير ---
class MainScreen extends StatefulWidget {
  const MainScreen({Key? key}) : super(key: key);
  @override
  State<MainScreen> createState() => _MainScreenState();
}

class _MainScreenState extends State<MainScreen> {
  int _currentIndex = 0;
  final List<Widget> _screens = [
    const DashboardScreen(),
    const QrScannerScreen(),
    const CirclesScreen(),
    const TeachersListScreen(),
    const NotificationsScreen(),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: AnimatedSwitcher(
        duration: const Duration(milliseconds: 300),
        child: _screens[_currentIndex],
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _currentIndex,
        onTap: (index) => setState(() => _currentIndex = index),
        selectedItemColor: const Color(0xFF1D5D5D),
        unselectedItemColor: Colors.grey,
        type: BottomNavigationBarType.fixed,
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.dashboard_rounded),
            label: 'الرئيسية',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.qr_code_scanner_rounded),
            label: 'مسح QR',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.group_work_rounded),
            label: 'الحلقات',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.people_alt_rounded),
            label: 'المحفظين',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.notifications_rounded),
            label: 'الإشعارات',
          ),
        ],
      ),
    );
  }
}

// --- الشاشة الرئيسية للمحفظ ---
class TeacherMainScreen extends StatefulWidget {
  const TeacherMainScreen({Key? key}) : super(key: key);
  @override
  State<TeacherMainScreen> createState() => _TeacherMainScreenState();
}

class _TeacherMainScreenState extends State<TeacherMainScreen> {
  int _currentIndex = 0;
  final List<Widget> _screens = [
    const DashboardScreen(),
    const QrScannerScreen(),
    const CirclesScreen(),
    const NotificationsScreen(),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: AnimatedSwitcher(
        duration: const Duration(milliseconds: 300),
        child: _screens[_currentIndex],
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _currentIndex,
        onTap: (index) => setState(() => _currentIndex = index),
        selectedItemColor: const Color(0xFF1D5D5D),
        unselectedItemColor: Colors.grey,
        type: BottomNavigationBarType.fixed,
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.dashboard_rounded),
            label: 'الرئيسية',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.qr_code_scanner_rounded),
            label: 'مسح QR',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.group_work_rounded),
            label: 'الحلقات',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.notifications_rounded),
            label: 'الإشعارات',
          ),
        ],
      ),
    );
  }
}

// --- الشاشة الرئيسية لولي الأمر ---
class ParentMainScreen extends StatelessWidget {
  final String userId;
  const ParentMainScreen({Key? key, required this.userId}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    const Color primaryTeal = Color(0xFF1D5D5D);

    return Scaffold(
      appBar: AppBar(
        title: const Text('متابعة أبنائي', style: TextStyle(fontWeight: FontWeight.bold)),
        backgroundColor: primaryTeal,
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.notifications),
            onPressed: () => Navigator.push(
              context,
              MaterialPageRoute(builder: (_) => const NotificationsScreen()),
            ),
          ),
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () async {
              await ApiService.logout();
              if (context.mounted) {
                Navigator.pushAndRemoveUntil(
                  context,
                  MaterialPageRoute(builder: (_) => const OnboardingScreen()),
                  (route) => false,
                );
              }
            },
          ),
        ],
      ),
      body: FutureBuilder<List<dynamic>>(
        future: ApiService().getStudents(),
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }

          final students = snapshot.data ?? [];
          if (students.isEmpty) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(Icons.child_care, size: 80, color: Colors.grey[300]),
                  const SizedBox(height: 16),
                  Text(
                    'لم يتم ربط أي طالب بحسابك بعد',
                    style: TextStyle(fontSize: 16, color: Colors.grey[500]),
                  ),
                ],
              ),
            );
          }

          return ListView.builder(
            padding: const EdgeInsets.all(16),
            itemCount: students.length,
            itemBuilder: (context, index) {
              final student = students[index];
              return Card(
                elevation: 2,
                margin: const EdgeInsets.only(bottom: 12),
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                child: ListTile(
                  contentPadding: const EdgeInsets.all(16),
                  leading: CircleAvatar(
                    radius: 28,
                    backgroundColor: primaryTeal.withOpacity(0.1),
                    child: const Icon(Icons.person, color: primaryTeal, size: 30),
                  ),
                  title: Text(
                    student['name'] ?? '',
                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 17),
                  ),
                  subtitle: Text(
                    student['circleName'] ?? 'غير محدد',
                    style: TextStyle(color: Colors.grey[600]),
                  ),
                  trailing: const Icon(Icons.arrow_forward_ios, color: primaryTeal),
                  onTap: () => Navigator.push(
                    context,
                    MaterialPageRoute(
                      builder: (_) => StudentProgressScreen(studentId: student['id']),
                    ),
                  ),
                ),
              );
            },
          );
        },
      ),
    );
  }
}
