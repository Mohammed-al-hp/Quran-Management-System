import 'services/api_service.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'dart:math' as math;
import 'dart:async';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  SystemChrome.setSystemUIOverlayStyle(
    const SystemUiOverlayStyle(
      statusBarColor: Colors.transparent,
      statusBarIconBrightness: Brightness.light,
    ),
  );
  runApp(const QuranApp());
}

class QuranApp extends StatelessWidget {
  const QuranApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'نظام أهل القرآن',
      debugShowCheckedModeBanner: false,
      locale: const Locale('ar', 'SA'),
      builder: (context, child) =>
          Directionality(textDirection: TextDirection.rtl, child: child!),
      theme: ThemeData(
        fontFamily: null,
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(
          seedColor: AppColors.teal,
          brightness: Brightness.light,
        ),
      ),
      home: const SplashScreen(),
    );
  }
}

class AppColors {
  static const teal = Color(0xFF0D4A4A);
  static const tealLight = Color(0xFF1A6B6B);
  static const tealMid = Color(0xFF0F5555);
  static const gold = Color(0xFFC5A059);
  static const goldLight = Color(0xFFD4B06A);
  static const cream = Color(0xFFFDF8F0);
  static const cardBg = Color(0xFFFFFFFF);
  static const textDark = Color(0xFF1A1A2E);
  static const textMid = Color(0xFF4A5568);
  static const textLight = Color(0xFF718096);
  static const success = Color(0xFF38A169);
  static const warning = Color(0xFFD69E2E);
  static const error = Color(0xFFE53E3E);
  static const bgLight = Color(0xFFF7FAFC);

  static const tealGradient = LinearGradient(
    begin: Alignment.topRight,
    end: Alignment.bottomLeft,
    colors: [Color(0xFF0D4A4A), Color(0xFF1A7070)],
  );
  static const goldGradient = LinearGradient(
    colors: [Color(0xFFC5A059), Color(0xFFE8C47A)],
  );
}

// =====================================================================
// شاشة السبلاش
// =====================================================================
class SplashScreen extends StatefulWidget {
  const SplashScreen({super.key});
  @override
  State<SplashScreen> createState() => _SplashScreenState();
}

class _SplashScreenState extends State<SplashScreen>
    with TickerProviderStateMixin {
  late AnimationController _logoCtrl, _textCtrl, _circleCtrl;
  late Animation<double> _logoScale, _logoOpacity, _textOpacity, _circleRotate;
  late Animation<Offset> _textSlide;

  @override
  void initState() {
    super.initState();
    _circleCtrl = AnimationController(
      vsync: this,
      duration: const Duration(seconds: 8),
    )..repeat();
    _logoCtrl = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1200),
    );
    _textCtrl = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 900),
    );
    _logoScale = Tween<double>(
      begin: 0.0,
      end: 1.0,
    ).animate(CurvedAnimation(parent: _logoCtrl, curve: Curves.elasticOut));
    _logoOpacity = Tween<double>(begin: 0.0, end: 1.0).animate(
      CurvedAnimation(
        parent: _logoCtrl,
        curve: const Interval(0.0, 0.5, curve: Curves.easeIn),
      ),
    );
    _textOpacity = Tween<double>(
      begin: 0.0,
      end: 1.0,
    ).animate(CurvedAnimation(parent: _textCtrl, curve: Curves.easeIn));
    _textSlide = Tween<Offset>(
      begin: const Offset(0, 0.3),
      end: Offset.zero,
    ).animate(CurvedAnimation(parent: _textCtrl, curve: Curves.easeOut));
    _circleRotate = Tween<double>(
      begin: 0,
      end: 2 * math.pi,
    ).animate(CurvedAnimation(parent: _circleCtrl, curve: Curves.linear));
    _logoCtrl.forward().then((_) {
      _textCtrl.forward().then((_) {
        Future.delayed(const Duration(milliseconds: 800), () {
          if (mounted) {
            Navigator.pushReplacement(
              context,
              PageRouteBuilder(
                pageBuilder: (_, __, ___) => const OnboardingScreen(),
                transitionsBuilder: (_, anim, __, child) =>
                    FadeTransition(opacity: anim, child: child),
                transitionDuration: const Duration(milliseconds: 600),
              ),
            );
          }
        });
      });
    });
  }

  @override
  void dispose() {
    _logoCtrl.dispose();
    _textCtrl.dispose();
    _circleCtrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(gradient: AppColors.tealGradient),
        child: Stack(
          children: [
            AnimatedBuilder(
              animation: _circleRotate,
              builder: (_, __) => CustomPaint(
                size: Size.infinite,
                painter: _DecorativePainter(_circleRotate.value),
              ),
            ),
            Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  ScaleTransition(
                    scale: _logoScale,
                    child: FadeTransition(
                      opacity: _logoOpacity,
                      child: Container(
                        width: 120,
                        height: 120,
                        decoration: BoxDecoration(
                          color: Colors.white.withOpacity(0.15),
                          shape: BoxShape.circle,
                          border: Border.all(
                            color: AppColors.gold.withOpacity(0.5),
                            width: 2,
                          ),
                        ),
                        child: const Icon(
                          Icons.menu_book_rounded,
                          size: 65,
                          color: AppColors.gold,
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(height: 32),
                  FadeTransition(
                    opacity: _textOpacity,
                    child: SlideTransition(
                      position: _textSlide,
                      child: Column(
                        children: [
                          const Text(
                            'نظام أهل القرآن',
                            style: TextStyle(
                              color: Colors.white,
                              fontSize: 28,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                          const SizedBox(height: 8),
                          Text(
                            'بوابة الطالب وولي الأمر',
                            style: TextStyle(
                              color: AppColors.gold.withOpacity(0.9),
                              fontSize: 16,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _DecorativePainter extends CustomPainter {
  final double angle;
  _DecorativePainter(this.angle);
  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = Colors.white.withOpacity(0.04)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 1.5;
    for (int i = 0; i < 4; i++) {
      canvas.drawCircle(
        Offset(
          size.width * 0.15 + math.cos(angle + i) * 20,
          size.height * 0.15 + math.sin(angle + i) * 20,
        ),
        80.0 + i * 60,
        paint,
      );
    }
    final paint2 = Paint()
      ..color = AppColors.gold.withOpacity(0.06)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 1;
    canvas.drawCircle(
      Offset(size.width * 0.85, size.height * 0.8),
      120,
      paint2,
    );
    canvas.drawCircle(
      Offset(size.width * 0.85, size.height * 0.8),
      180,
      paint2,
    );
  }

  @override
  bool shouldRepaint(covariant _DecorativePainter old) => old.angle != angle;
}

// =====================================================================
// شاشة الترحيب
// =====================================================================
class OnboardingScreen extends StatefulWidget {
  const OnboardingScreen({super.key});
  @override
  State<OnboardingScreen> createState() => _OnboardingScreenState();
}

class _OnboardingScreenState extends State<OnboardingScreen>
    with TickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _fadeAnim;
  late Animation<Offset> _slideAnim;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 800),
    );
    _fadeAnim = Tween<double>(
      begin: 0,
      end: 1,
    ).animate(CurvedAnimation(parent: _controller, curve: Curves.easeIn));
    _slideAnim = Tween<Offset>(
      begin: const Offset(0, 0.15),
      end: Offset.zero,
    ).animate(CurvedAnimation(parent: _controller, curve: Curves.easeOut));
    _controller.forward();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _goToLogin(BuildContext context, String role) {
    Navigator.push(
      context,
      PageRouteBuilder(
        pageBuilder: (_, __, ___) => LoginScreen(role: role),
        transitionsBuilder: (_, anim, __, child) => SlideTransition(
          position: Tween<Offset>(
            begin: const Offset(0, 1),
            end: Offset.zero,
          ).animate(CurvedAnimation(parent: anim, curve: Curves.easeOutCubic)),
          child: child,
        ),
        transitionDuration: const Duration(milliseconds: 400),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Stack(
        children: [
          Container(
            decoration: const BoxDecoration(gradient: AppColors.tealGradient),
          ),
          Positioned.fill(child: CustomPaint(painter: _GeometricBgPainter())),
          SafeArea(
            child: FadeTransition(
              opacity: _fadeAnim,
              child: SlideTransition(
                position: _slideAnim,
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 28),
                  child: Column(
                    children: [
                      const Spacer(flex: 2),
                      Stack(
                        alignment: Alignment.center,
                        children: [
                          Container(
                            width: 160,
                            height: 160,
                            decoration: BoxDecoration(
                              shape: BoxShape.circle,
                              border: Border.all(
                                color: AppColors.gold.withOpacity(0.25),
                                width: 1,
                              ),
                            ),
                          ),
                          Container(
                            width: 130,
                            height: 130,
                            decoration: BoxDecoration(
                              shape: BoxShape.circle,
                              border: Border.all(
                                color: AppColors.gold.withOpacity(0.4),
                                width: 1.5,
                              ),
                            ),
                          ),
                          Container(
                            width: 100,
                            height: 100,
                            decoration: BoxDecoration(
                              shape: BoxShape.circle,
                              gradient: LinearGradient(
                                colors: [
                                  Colors.white.withOpacity(0.15),
                                  Colors.white.withOpacity(0.05),
                                ],
                              ),
                              border: Border.all(
                                color: AppColors.gold.withOpacity(0.6),
                                width: 2,
                              ),
                            ),
                            child: const Icon(
                              Icons.menu_book_rounded,
                              size: 52,
                              color: AppColors.gold,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 36),
                      const Text(
                        'نظام أهل القرآن',
                        textAlign: TextAlign.center,
                        style: TextStyle(
                          color: Colors.white,
                          fontSize: 32,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      const SizedBox(height: 12),
                      Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 20,
                          vertical: 6,
                        ),
                        decoration: BoxDecoration(
                          color: AppColors.gold.withOpacity(0.15),
                          borderRadius: BorderRadius.circular(30),
                          border: Border.all(
                            color: AppColors.gold.withOpacity(0.3),
                          ),
                        ),
                        child: const Text(
                          'بوابة الطالب وولي الأمر',
                          style: TextStyle(
                            color: AppColors.gold,
                            fontSize: 15,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                      ),
                      const SizedBox(height: 28),
                      Text(
                        'تابع تقدمك في حفظ القرآن الكريم\nومواعيد الحلقات وسجل الحضور\nكل ذلك في مكان واحد',
                        textAlign: TextAlign.center,
                        style: TextStyle(
                          color: Colors.white.withOpacity(0.75),
                          fontSize: 16,
                          height: 1.8,
                        ),
                      ),
                      const Spacer(flex: 2),
                      Row(
                        children: [
                          Expanded(
                            child: _RoleCard(
                              icon: Icons.school_rounded,
                              title: 'الطالب',
                              subtitle: 'تابع حفظك وحضورك',
                              onTap: () => _goToLogin(context, 'student'),
                            ),
                          ),
                          const SizedBox(width: 14),
                          Expanded(
                            child: _RoleCard(
                              icon: Icons.family_restroom_rounded,
                              title: 'ولي الأمر',
                              subtitle: 'تابع أبناءك',
                              onTap: () => _goToLogin(context, 'parent'),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 40),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _GeometricBgPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = Colors.white.withOpacity(0.03)
      ..style = PaintingStyle.fill;
    canvas.drawPath(
      Path()
        ..moveTo(size.width, 0)
        ..lineTo(size.width, size.height * 0.35)
        ..lineTo(size.width * 0.6, 0)
        ..close(),
      paint,
    );
    canvas.drawPath(
      Path()
        ..moveTo(0, size.height)
        ..lineTo(0, size.height * 0.7)
        ..lineTo(size.width * 0.35, size.height)
        ..close(),
      paint,
    );
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}

class _RoleCard extends StatefulWidget {
  final IconData icon;
  final String title, subtitle;
  final VoidCallback onTap;
  const _RoleCard({
    required this.icon,
    required this.title,
    required this.subtitle,
    required this.onTap,
  });
  @override
  State<_RoleCard> createState() => _RoleCardState();
}

class _RoleCardState extends State<_RoleCard>
    with SingleTickerProviderStateMixin {
  late AnimationController _ctrl;
  late Animation<double> _scale;
  @override
  void initState() {
    super.initState();
    _ctrl = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 120),
    );
    _scale = Tween<double>(
      begin: 1.0,
      end: 0.95,
    ).animate(CurvedAnimation(parent: _ctrl, curve: Curves.easeInOut));
  }

  @override
  void dispose() {
    _ctrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTapDown: (_) => _ctrl.forward(),
      onTapUp: (_) {
        _ctrl.reverse();
        widget.onTap();
      },
      onTapCancel: () => _ctrl.reverse(),
      child: ScaleTransition(
        scale: _scale,
        child: Container(
          padding: const EdgeInsets.symmetric(vertical: 24, horizontal: 16),
          decoration: BoxDecoration(
            color: Colors.white.withOpacity(0.12),
            borderRadius: BorderRadius.circular(20),
            border: Border.all(
              color: AppColors.gold.withOpacity(0.35),
              width: 1.5,
            ),
          ),
          child: Column(
            children: [
              Container(
                padding: const EdgeInsets.all(14),
                decoration: BoxDecoration(
                  color: AppColors.gold.withOpacity(0.15),
                  shape: BoxShape.circle,
                ),
                child: Icon(widget.icon, color: AppColors.gold, size: 30),
              ),
              const SizedBox(height: 12),
              Text(
                widget.title,
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 17,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 4),
              Text(
                widget.subtitle,
                textAlign: TextAlign.center,
                style: TextStyle(
                  color: Colors.white.withOpacity(0.65),
                  fontSize: 12,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

// =====================================================================
// شاشة تسجيل الدخول
// =====================================================================
class LoginScreen extends StatefulWidget {
  final String role;
  const LoginScreen({super.key, required this.role});
  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen>
    with TickerProviderStateMixin {
  final _usernameCtrl = TextEditingController();
  final _passwordCtrl = TextEditingController();
  bool _isLoading = false, _showPassword = false;
  late AnimationController _formCtrl;
  late Animation<Offset> _formSlide;
  late Animation<double> _formFade;
  bool get _isStudent => widget.role == 'student';

  @override
  void initState() {
    super.initState();
    _formCtrl = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 700),
    );
    _formSlide = Tween<Offset>(
      begin: const Offset(0, 0.2),
      end: Offset.zero,
    ).animate(CurvedAnimation(parent: _formCtrl, curve: Curves.easeOutCubic));
    _formFade = Tween<double>(
      begin: 0,
      end: 1,
    ).animate(CurvedAnimation(parent: _formCtrl, curve: Curves.easeIn));
    _formCtrl.forward();
  }

  @override
  void dispose() {
    _usernameCtrl.dispose();
    _passwordCtrl.dispose();
    _formCtrl.dispose();
    super.dispose();
  }

  Future<void> _login() async {
    if (_usernameCtrl.text.isEmpty || _passwordCtrl.text.isEmpty) {
      _showSnack('يرجى إدخال اسم المستخدم وكلمة المرور', isError: true);
      return;
    }
    setState(() => _isLoading = true);
    final apiService = ApiService();
    final result = await apiService.login(
      _usernameCtrl.text.trim(),
      _passwordCtrl.text,
    );
    if (!mounted) return;
    setState(() => _isLoading = false);
    if (result['success'] == true) {
      final role = result['role'] ?? 'Student';
      Widget target = role == 'Parent'
          ? ParentDashboard(name: _usernameCtrl.text)
          : StudentDashboard(name: _usernameCtrl.text);
      Navigator.pushReplacement(
        context,
        PageRouteBuilder(
          pageBuilder: (_, __, ___) => target,
          transitionsBuilder: (_, anim, __, child) =>
              FadeTransition(opacity: anim, child: child),
          transitionDuration: const Duration(milliseconds: 500),
        ),
      );
    } else {
      _showSnack(result['message'] ?? 'بيانات الدخول غير صحيحة', isError: true);
    }
  }

  void _showSnack(String msg, {bool isError = false}) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(msg),
        backgroundColor: isError ? AppColors.error : AppColors.success,
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        margin: const EdgeInsets.all(16),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Stack(
        children: [
          Positioned(
            top: 0,
            left: 0,
            right: 0,
            height: MediaQuery.of(context).size.height * 0.40,
            child: Container(
              decoration: const BoxDecoration(gradient: AppColors.tealGradient),
              child: SafeArea(
                child: Padding(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 24,
                    vertical: 16,
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      GestureDetector(
                        onTap: () => Navigator.pop(context),
                        child: Container(
                          padding: const EdgeInsets.all(8),
                          decoration: BoxDecoration(
                            color: Colors.white.withOpacity(0.1),
                            borderRadius: BorderRadius.circular(10),
                          ),
                          child: const Icon(
                            Icons.arrow_back_ios_new,
                            color: Colors.white,
                            size: 18,
                          ),
                        ),
                      ),
                      const Spacer(),
                      Container(
                        padding: const EdgeInsets.all(14),
                        decoration: BoxDecoration(
                          color: AppColors.gold.withOpacity(0.2),
                          shape: BoxShape.circle,
                          border: Border.all(
                            color: AppColors.gold.withOpacity(0.5),
                            width: 2,
                          ),
                        ),
                        child: Icon(
                          _isStudent
                              ? Icons.school_rounded
                              : Icons.family_restroom_rounded,
                          color: AppColors.gold,
                          size: 32,
                        ),
                      ),
                      const SizedBox(height: 12),
                      Text(
                        _isStudent ? 'دخول الطالب' : 'دخول ولي الأمر',
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 26,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      Text(
                        _isStudent
                            ? 'أدخل بياناتك لمتابعة حلقتك'
                            : 'أدخل بياناتك لمتابعة أبنائك',
                        style: TextStyle(
                          color: Colors.white.withOpacity(0.7),
                          fontSize: 14,
                        ),
                      ),
                      const SizedBox(height: 20),
                    ],
                  ),
                ),
              ),
            ),
          ),
          Positioned(
            bottom: 0,
            left: 0,
            right: 0,
            height: MediaQuery.of(context).size.height * 0.62,
            child: Container(color: AppColors.bgLight),
          ),
          Positioned.fill(
            child: SingleChildScrollView(
              child: Padding(
                padding: EdgeInsets.only(
                  top: MediaQuery.of(context).size.height * 0.30,
                  left: 20,
                  right: 20,
                  bottom: 20,
                ),
                child: FadeTransition(
                  opacity: _formFade,
                  child: SlideTransition(
                    position: _formSlide,
                    child: Container(
                      decoration: BoxDecoration(
                        color: Colors.white,
                        borderRadius: BorderRadius.circular(24),
                        boxShadow: [
                          BoxShadow(
                            color: Colors.black.withOpacity(0.08),
                            blurRadius: 30,
                            offset: const Offset(0, 10),
                          ),
                        ],
                      ),
                      padding: const EdgeInsets.all(28),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: [
                          _buildLabel('اسم المستخدم'),
                          const SizedBox(height: 8),
                          _buildTextField(
                            controller: _usernameCtrl,
                            hint: _isStudent
                                ? 'مثال: student_2026_1473'
                                : 'مثال: parent@email.com',
                            icon: Icons.person_outline_rounded,
                          ),
                          const SizedBox(height: 20),
                          _buildLabel('كلمة المرور'),
                          const SizedBox(height: 8),
                          _buildTextField(
                            controller: _passwordCtrl,
                            hint: '••••••••',
                            icon: Icons.lock_outline_rounded,
                            isPassword: true,
                            showPassword: _showPassword,
                            onTogglePassword: () =>
                                setState(() => _showPassword = !_showPassword),
                          ),
                          Align(
                            alignment: Alignment.centerLeft,
                            child: TextButton(
                              onPressed: () {},
                              child: const Text(
                                'نسيت كلمة المرور؟',
                                style: TextStyle(
                                  color: AppColors.tealLight,
                                  fontSize: 13,
                                ),
                              ),
                            ),
                          ),
                          const SizedBox(height: 8),
                          _LoginButton(
                            isLoading: _isLoading,
                            label: _isStudent
                                ? 'دخول الطالب'
                                : 'دخول ولي الأمر',
                            onPressed: _login,
                          ),
                          const SizedBox(height: 20),
                          Container(
                            padding: const EdgeInsets.all(14),
                            decoration: BoxDecoration(
                              color: AppColors.teal.withOpacity(0.05),
                              borderRadius: BorderRadius.circular(12),
                              border: Border.all(
                                color: AppColors.teal.withOpacity(0.1),
                              ),
                            ),
                            child: Row(
                              children: [
                                const Icon(
                                  Icons.info_outline,
                                  color: AppColors.teal,
                                  size: 18,
                                ),
                                const SizedBox(width: 10),
                                Expanded(
                                  child: Text(
                                    _isStudent
                                        ? 'بيانات الدخول مقدمة من مشرف المركز'
                                        : 'إذا لم تتلق بياناتك، تواصل مع المركز',
                                    style: const TextStyle(
                                      color: AppColors.teal,
                                      fontSize: 12,
                                    ),
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildLabel(String text) => Text(
    text,
    style: const TextStyle(
      fontWeight: FontWeight.w600,
      color: AppColors.textDark,
      fontSize: 14,
    ),
  );

  Widget _buildTextField({
    required TextEditingController controller,
    required String hint,
    required IconData icon,
    bool isPassword = false,
    bool showPassword = false,
    VoidCallback? onTogglePassword,
  }) {
    return Container(
      decoration: BoxDecoration(
        color: AppColors.bgLight,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: Colors.grey.shade200),
      ),
      child: TextField(
        controller: controller,
        obscureText: isPassword && !showPassword,
        textAlign: TextAlign.right,
        style: const TextStyle(fontSize: 15, color: AppColors.textDark),
        decoration: InputDecoration(
          hintText: hint,
          hintStyle: TextStyle(color: Colors.grey.shade400, fontSize: 14),
          prefixIcon: isPassword
              ? GestureDetector(
                  onTap: onTogglePassword,
                  child: Icon(
                    showPassword
                        ? Icons.visibility_off_outlined
                        : Icons.visibility_outlined,
                    color: AppColors.textLight,
                    size: 20,
                  ),
                )
              : null,
          suffixIcon: Icon(
            icon,
            color: AppColors.teal.withOpacity(0.6),
            size: 20,
          ),
          border: InputBorder.none,
          contentPadding: const EdgeInsets.symmetric(
            horizontal: 16,
            vertical: 14,
          ),
        ),
        onSubmitted: (_) => _login(),
      ),
    );
  }
}

class _LoginButton extends StatefulWidget {
  final bool isLoading;
  final String label;
  final VoidCallback onPressed;
  const _LoginButton({
    required this.isLoading,
    required this.label,
    required this.onPressed,
  });
  @override
  State<_LoginButton> createState() => _LoginButtonState();
}

class _LoginButtonState extends State<_LoginButton>
    with SingleTickerProviderStateMixin {
  late AnimationController _ctrl;
  late Animation<double> _scale;
  @override
  void initState() {
    super.initState();
    _ctrl = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 100),
    );
    _scale = Tween<double>(
      begin: 1.0,
      end: 0.97,
    ).animate(CurvedAnimation(parent: _ctrl, curve: Curves.easeInOut));
  }

  @override
  void dispose() {
    _ctrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTapDown: (_) => _ctrl.forward(),
      onTapUp: (_) {
        _ctrl.reverse();
        widget.onPressed();
      },
      onTapCancel: () => _ctrl.reverse(),
      child: ScaleTransition(
        scale: _scale,
        child: Container(
          height: 54,
          decoration: BoxDecoration(
            gradient: const LinearGradient(
              colors: [AppColors.teal, AppColors.tealLight],
            ),
            borderRadius: BorderRadius.circular(14),
            boxShadow: [
              BoxShadow(
                color: AppColors.teal.withOpacity(0.35),
                blurRadius: 15,
                offset: const Offset(0, 6),
              ),
            ],
          ),
          child: Center(
            child: widget.isLoading
                ? const SizedBox(
                    width: 24,
                    height: 24,
                    child: CircularProgressIndicator(
                      color: Colors.white,
                      strokeWidth: 2.5,
                    ),
                  )
                : Text(
                    widget.label,
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 17,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
          ),
        ),
      ),
    );
  }
}

// =====================================================================
// لوحة تحكم الطالب - مع بيانات حقيقية من API
// =====================================================================
class StudentDashboard extends StatefulWidget {
  final String name;
  const StudentDashboard({super.key, required this.name});
  @override
  State<StudentDashboard> createState() => _StudentDashboardState();
}

class _StudentDashboardState extends State<StudentDashboard>
    with TickerProviderStateMixin {
  int _currentTab = 0;
  late AnimationController _headerCtrl;
  late Animation<double> _headerOpacity;

  // بيانات API
  bool _isLoading = true;
  Map<String, dynamic> _studentData = {};
  Map<String, dynamic> _attendanceData = {};
  Map<String, dynamic> _memorizationData = {};
  String _errorMsg = '';

  @override
  void initState() {
    super.initState();
    _headerCtrl = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 600),
    );
    _headerOpacity = Tween<double>(
      begin: 0,
      end: 1,
    ).animate(CurvedAnimation(parent: _headerCtrl, curve: Curves.easeIn));
    _headerCtrl.forward();
    _loadData();
  }

  Future<void> _loadData() async {
    setState(() {
      _isLoading = true;
      _errorMsg = '';
    });
    try {
      final api = ApiService();
      final data = await api.getStudentData();
      if (data.isNotEmpty && mounted) {
        setState(() {
          _studentData = data['student'] ?? {};
          _attendanceData = data['attendance'] ?? {};
          _memorizationData = data['memorization'] ?? {};
          _isLoading = false;
        });
      } else if (mounted) {
        setState(() {
          _isLoading = false;
          _errorMsg = 'تعذر جلب البيانات';
        });
      }
    } catch (e) {
      if (mounted)
        setState(() {
          _isLoading = false;
          _errorMsg = 'خطأ في الاتصال';
        });
    }
  }

  @override
  void dispose() {
    _headerCtrl.dispose();
    super.dispose();
  }

  // استخراج بيانات الحلقة
  String get _circleName => _studentData['circle']?['name'] ?? 'غير محدد';
  String get _teacherName =>
      _studentData['circle']?['teacher']?['name'] ?? 'غير محدد';
  String get _selectedDays => _studentData['circle']?['selectedDays'] ?? '-';
  String get _startPrayer => _studentData['circle']?['startPrayer'] ?? '-';
  String get _attendanceRate => _attendanceData['attendanceRate'] ?? '0%';
  int get _presentCount => _attendanceData['present'] ?? 0;
  int get _absentCount => _attendanceData['absent'] ?? 0;
  int get _totalMem => _memorizationData['total'] ?? 0;
  List get _recentMem => _memorizationData['recent'] ?? [];
  List get _recentAttendance => _attendanceData['recent'] ?? [];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.bgLight,
      body: Column(
        children: [
          _buildHeader(),
          Expanded(
            child: _isLoading
                ? const Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        CircularProgressIndicator(color: AppColors.teal),
                        SizedBox(height: 16),
                        Text(
                          'جاري تحميل بياناتك...',
                          style: TextStyle(color: AppColors.textMid),
                        ),
                      ],
                    ),
                  )
                : _errorMsg.isNotEmpty
                ? Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const Icon(
                          Icons.wifi_off_rounded,
                          size: 60,
                          color: AppColors.textLight,
                        ),
                        const SizedBox(height: 16),
                        Text(
                          _errorMsg,
                          style: const TextStyle(
                            color: AppColors.textMid,
                            fontSize: 16,
                          ),
                        ),
                        const SizedBox(height: 20),
                        ElevatedButton(
                          onPressed: _loadData,
                          style: ElevatedButton.styleFrom(
                            backgroundColor: AppColors.teal,
                          ),
                          child: const Text(
                            'إعادة المحاولة',
                            style: TextStyle(color: Colors.white),
                          ),
                        ),
                      ],
                    ),
                  )
                : IndexedStack(
                    index: _currentTab,
                    children: [
                      _buildHomeTab(),
                      _buildAttendanceTab(),
                      _buildMemorizationTab(),
                      _buildProfileTab(),
                    ],
                  ),
          ),
          _buildBottomNav(),
        ],
      ),
    );
  }

  Widget _buildHeader() {
    return Container(
      decoration: const BoxDecoration(gradient: AppColors.tealGradient),
      child: SafeArea(
        bottom: false,
        child: FadeTransition(
          opacity: _headerOpacity,
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
            child: Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'السلام عليكم 👋',
                        style: TextStyle(
                          color: Colors.white.withOpacity(0.75),
                          fontSize: 13,
                        ),
                      ),
                      const SizedBox(height: 3),
                      Text(
                        _studentData['name'] ?? widget.name,
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 20,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ],
                  ),
                ),
                Stack(
                  children: [
                    Container(
                      padding: const EdgeInsets.all(10),
                      decoration: BoxDecoration(
                        color: Colors.white.withOpacity(0.12),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: const Icon(
                        Icons.notifications_outlined,
                        color: Colors.white,
                        size: 22,
                      ),
                    ),
                    Positioned(
                      right: 6,
                      top: 6,
                      child: Container(
                        width: 9,
                        height: 9,
                        decoration: BoxDecoration(
                          color: AppColors.gold,
                          shape: BoxShape.circle,
                          border: Border.all(color: AppColors.teal, width: 1.5),
                        ),
                      ),
                    ),
                  ],
                ),
                const SizedBox(width: 10),
                GestureDetector(
                  onTap: () async {
                    await ApiService.logout();
                    if (mounted)
                      Navigator.pushAndRemoveUntil(
                        context,
                        MaterialPageRoute(
                          builder: (_) => const OnboardingScreen(),
                        ),
                        (_) => false,
                      );
                  },
                  child: Container(
                    padding: const EdgeInsets.all(10),
                    decoration: BoxDecoration(
                      color: Colors.white.withOpacity(0.12),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: const Icon(
                      Icons.logout_rounded,
                      color: Colors.white,
                      size: 20,
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildHomeTab() {
    return RefreshIndicator(
      onRefresh: _loadData,
      color: AppColors.teal,
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(18),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // بطاقة الملخص
            _RealSummaryCard(
              attendanceRate: _attendanceRate,
              memorizedCount: _totalMem.toString(),
              circleName: _circleName,
              nextSession: '$_selectedDays - بعد $_startPrayer',
            ),
            const SizedBox(height: 22),
            const _SectionTitle('إحصائياتي'),
            const SizedBox(height: 12),
            Row(
              children: [
                Expanded(
                  child: _QuickStat(
                    label: 'أيام الحضور',
                    value: _presentCount.toString(),
                    icon: Icons.check_circle_outline,
                    color: AppColors.success,
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: _QuickStat(
                    label: 'أيام الغياب',
                    value: _absentCount.toString(),
                    icon: Icons.cancel_outlined,
                    color: AppColors.error,
                  ),
                ),
                const SizedBox(width: 10),
                Expanded(
                  child: _QuickStat(
                    label: 'سجلات الحفظ',
                    value: _totalMem.toString(),
                    icon: Icons.menu_book_rounded,
                    color: AppColors.gold,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 22),
            const _SectionTitle('آخر سجلات الحفظ'),
            const SizedBox(height: 12),
            if (_recentMem.isEmpty)
              Container(
                padding: const EdgeInsets.all(20),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(14),
                ),
                child: const Center(
                  child: Text(
                    'لا توجد سجلات حفظ بعد',
                    style: TextStyle(color: AppColors.textLight),
                  ),
                ),
              )
            else
              ..._recentMem.take(3).map((r) => _ApiMemCard(record: r)),
            if (_recentMem.isNotEmpty)
              Center(
                child: TextButton(
                  onPressed: () => setState(() => _currentTab = 2),
                  child: const Text(
                    'عرض جميع السجلات ←',
                    style: TextStyle(
                      color: AppColors.teal,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ),
              ),
            const SizedBox(height: 8),
            const _SectionTitle('الجدول الأسبوعي'),
            const SizedBox(height: 12),
            _RealScheduleCard(days: _selectedDays, startPrayer: _startPrayer),
          ],
        ),
      ),
    );
  }

  Widget _buildAttendanceTab() {
    final attendanceValue =
        _attendanceData['total'] != null && _attendanceData['total'] > 0
        ? _presentCount / (_attendanceData['total'] as int)
        : 0.0;
    return RefreshIndicator(
      onRefresh: _loadData,
      color: AppColors.teal,
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(18),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Container(
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                gradient: const LinearGradient(
                  colors: [AppColors.teal, AppColors.tealLight],
                ),
                borderRadius: BorderRadius.circular(20),
              ),
              child: Row(
                children: [
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text(
                          'نسبة الحضور الإجمالية',
                          style: TextStyle(color: Colors.white70, fontSize: 13),
                        ),
                        const SizedBox(height: 6),
                        Text(
                          _attendanceRate,
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 38,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          '$_presentCount حاضر / $_absentCount غائب',
                          style: TextStyle(
                            color: Colors.white.withOpacity(0.7),
                            fontSize: 13,
                          ),
                        ),
                      ],
                    ),
                  ),
                  _CircularProgress(value: attendanceValue.clamp(0.0, 1.0)),
                ],
              ),
            ),
            const SizedBox(height: 20),
            const _SectionTitle('سجل الحضور'),
            const SizedBox(height: 12),
            if (_recentAttendance.isEmpty)
              Container(
                padding: const EdgeInsets.all(20),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(14),
                ),
                child: const Center(
                  child: Text(
                    'لا توجد سجلات حضور بعد',
                    style: TextStyle(color: AppColors.textLight),
                  ),
                ),
              )
            else
              ..._recentAttendance.map((r) => _ApiAttendanceCard(record: r)),
          ],
        ),
      ),
    );
  }

  Widget _buildMemorizationTab() {
    final grades = _memorizationData['grades'] as List? ?? [];
    final total = _totalMem > 0 ? _totalMem : 1;
    return RefreshIndicator(
      onRefresh: _loadData,
      color: AppColors.teal,
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(18),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Container(
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(20),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.05),
                    blurRadius: 15,
                    offset: const Offset(0, 5),
                  ),
                ],
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    'توزيع تقييماتي',
                    style: TextStyle(
                      fontWeight: FontWeight.bold,
                      fontSize: 16,
                      color: AppColors.textDark,
                    ),
                  ),
                  const SizedBox(height: 16),
                  if (grades.isEmpty)
                    const Text(
                      'لا توجد تقييمات بعد',
                      style: TextStyle(color: AppColors.textLight),
                    )
                  else
                    ...grades.map((g) {
                      final grade = g['grade'] ?? '';
                      final count = g['count'] ?? 0;
                      final color = grade == 'ممتاز'
                          ? AppColors.success
                          : grade == 'جيد جداً'
                          ? Colors.blue
                          : grade == 'جيد'
                          ? AppColors.warning
                          : AppColors.error;
                      return _GradeBar(
                        label: grade,
                        count: count,
                        total: total,
                        color: color,
                      );
                    }),
                ],
              ),
            ),
            const SizedBox(height: 20),
            const _SectionTitle('سجل الحفظ التفصيلي'),
            const SizedBox(height: 12),
            if (_recentMem.isEmpty)
              Container(
                padding: const EdgeInsets.all(20),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(14),
                ),
                child: const Center(
                  child: Text(
                    'لا توجد سجلات بعد',
                    style: TextStyle(color: AppColors.textLight),
                  ),
                ),
              )
            else
              ..._recentMem.map((r) => _ApiMemCard(record: r, detailed: true)),
          ],
        ),
      ),
    );
  }

  Widget _buildProfileTab() {
    final circle = _studentData['circle'];
    return SingleChildScrollView(
      padding: const EdgeInsets.all(18),
      child: Column(
        children: [
          Container(
            padding: const EdgeInsets.all(24),
            decoration: BoxDecoration(
              gradient: const LinearGradient(
                colors: [AppColors.teal, AppColors.tealLight],
              ),
              borderRadius: BorderRadius.circular(24),
            ),
            child: Column(
              children: [
                Container(
                  width: 80,
                  height: 80,
                  decoration: BoxDecoration(
                    color: Colors.white.withOpacity(0.2),
                    shape: BoxShape.circle,
                    border: Border.all(
                      color: AppColors.gold.withOpacity(0.5),
                      width: 2,
                    ),
                  ),
                  child: const Icon(
                    Icons.person,
                    color: Colors.white,
                    size: 44,
                  ),
                ),
                const SizedBox(height: 14),
                Text(
                  _studentData['name'] ?? widget.name,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 20,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  'طالب • $_circleName',
                  style: TextStyle(
                    color: Colors.white.withOpacity(0.7),
                    fontSize: 13,
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 20),
          _ProfileInfoCard(
            items: [
              _ProfileItem(Icons.groups_rounded, 'الحلقة', _circleName),
              _ProfileItem(Icons.person_outline, 'المحفظ', _teacherName),
              _ProfileItem(
                Icons.calendar_today,
                'تاريخ الالتحاق',
                _studentData['joinDate'] ?? '-',
              ),
              _ProfileItem(
                Icons.phone_outlined,
                'رقم الهاتف',
                _studentData['phone'] ?? '-',
              ),
              if (circle != null)
                _ProfileItem(
                  Icons.access_time,
                  'مواعيد الحلقة',
                  circle['startPrayer'] ?? '-',
                ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildBottomNav() {
    final items = [
      (Icons.home_rounded, 'الرئيسية'),
      (Icons.calendar_today_rounded, 'الحضور'),
      (Icons.menu_book_rounded, 'الحفظ'),
      (Icons.person_rounded, 'ملفي'),
    ];

    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.06),
            blurRadius: 20,
            offset: const Offset(0, -5),
          ),
        ],
      ),
      child: SafeArea(
        top: false,
        child: Padding(
          padding: const EdgeInsets.symmetric(vertical: 8),
          child: Row(
            children: List.generate(
              items.length,
              (i) => Expanded(
                child: GestureDetector(
                  onTap: () => setState(() => _currentTab = i),
                  behavior: HitTestBehavior.opaque,
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      AnimatedContainer(
                        duration: const Duration(milliseconds: 200),
                        padding: const EdgeInsets.all(6),
                        decoration: BoxDecoration(
                          color: _currentTab == i
                              ? AppColors.teal.withOpacity(0.1)
                              : Colors.transparent,
                          borderRadius: BorderRadius.circular(10),
                        ),
                        child: Icon(
                          items[i].$1,
                          color: _currentTab == i
                              ? AppColors.teal
                              : AppColors.textLight,
                          size: 24,
                        ),
                      ),
                      const SizedBox(height: 3),
                      Text(
                        items[i].$2,
                        style: TextStyle(
                          fontSize: 11,
                          color: _currentTab == i
                              ? AppColors.teal
                              : AppColors.textLight,
                          fontWeight: _currentTab == i
                              ? FontWeight.w600
                              : FontWeight.normal,
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

// =====================================================================
// لوحة تحكم ولي الأمر - مع بيانات حقيقية
// =====================================================================
class ParentDashboard extends StatefulWidget {
  final String name;
  const ParentDashboard({super.key, required this.name});
  @override
  State<ParentDashboard> createState() => _ParentDashboardState();
}

class _ParentDashboardState extends State<ParentDashboard>
    with TickerProviderStateMixin {
  int _currentTab = 0, _selectedChild = 0;
  bool _isLoading = true;
  Map<String, dynamic> _parentData = {};
  List _children = [];
  String _errorMsg = '';

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  Future<void> _loadData() async {
    setState(() {
      _isLoading = true;
      _errorMsg = '';
    });
    try {
      final api = ApiService();
      final data = await api.getParentData();
      if (data.isNotEmpty && mounted) {
        setState(() {
          _parentData = data['parent'] ?? {};
          _children = data['children'] ?? [];
          _isLoading = false;
        });
      } else if (mounted) {
        setState(() {
          _isLoading = false;
          _errorMsg = 'تعذر جلب البيانات';
        });
      }
    } catch (e) {
      if (mounted)
        setState(() {
          _isLoading = false;
          _errorMsg = 'خطأ في الاتصال';
        });
    }
  }

  Map get _currentChild =>
      _children.isNotEmpty ? _children[_selectedChild] : {};

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.bgLight,
      body: Column(
        children: [
          _buildParentHeader(),
          Expanded(
            child: _isLoading
                ? const Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        CircularProgressIndicator(color: Color(0xFF2D1B69)),
                        SizedBox(height: 16),
                        Text(
                          'جاري تحميل بياناتك...',
                          style: TextStyle(color: AppColors.textMid),
                        ),
                      ],
                    ),
                  )
                : _errorMsg.isNotEmpty
                ? Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const Icon(
                          Icons.wifi_off_rounded,
                          size: 60,
                          color: AppColors.textLight,
                        ),
                        const SizedBox(height: 16),
                        Text(
                          _errorMsg,
                          style: const TextStyle(color: AppColors.textMid),
                        ),
                        const SizedBox(height: 20),
                        ElevatedButton(
                          onPressed: _loadData,
                          style: ElevatedButton.styleFrom(
                            backgroundColor: const Color(0xFF2D1B69),
                          ),
                          child: const Text(
                            'إعادة المحاولة',
                            style: TextStyle(color: Colors.white),
                          ),
                        ),
                      ],
                    ),
                  )
                : IndexedStack(
                    index: _currentTab,
                    children: [
                      _buildParentHome(),
                      _buildChildrenTab(),
                      _buildNotificationsTab(),
                      _buildParentProfile(),
                    ],
                  ),
          ),
          _buildParentBottomNav(),
        ],
      ),
    );
  }

  Widget _buildParentHeader() {
    return Container(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topRight,
          end: Alignment.bottomLeft,
          colors: [Color(0xFF2D1B69), Color(0xFF4A2D8F)],
        ),
      ),
      child: SafeArea(
        bottom: false,
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
          child: Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'مرحباً بك 👋',
                      style: TextStyle(
                        color: Colors.white.withOpacity(0.75),
                        fontSize: 13,
                      ),
                    ),
                    const SizedBox(height: 3),
                    Text(
                      _parentData['name'] ?? widget.name,
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 20,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ],
                ),
              ),
              Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 12,
                  vertical: 6,
                ),
                decoration: BoxDecoration(
                  color: Colors.white.withOpacity(0.12),
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(
                      Icons.child_care_rounded,
                      color: AppColors.gold,
                      size: 16,
                    ),
                    const SizedBox(width: 5),
                    Text(
                      '${_children.length} أبناء',
                      style: const TextStyle(color: Colors.white, fontSize: 13),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 10),
              GestureDetector(
                onTap: () async {
                  await ApiService.logout();
                  if (mounted)
                    Navigator.pushAndRemoveUntil(
                      context,
                      MaterialPageRoute(
                        builder: (_) => const OnboardingScreen(),
                      ),
                      (_) => false,
                    );
                },
                child: Container(
                  padding: const EdgeInsets.all(10),
                  decoration: BoxDecoration(
                    color: Colors.white.withOpacity(0.12),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: const Icon(
                    Icons.logout_rounded,
                    color: Colors.white,
                    size: 20,
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildParentHome() {
    final child = _currentChild;
    return RefreshIndicator(
      onRefresh: _loadData,
      color: const Color(0xFF2D1B69),
      child: SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(18),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            if (_children.length > 1) ...[
              const _SectionTitle('اختر الابن'),
              const SizedBox(height: 10),
              SizedBox(
                height: 48,
                child: ListView.separated(
                  scrollDirection: Axis.horizontal,
                  itemCount: _children.length,
                  separatorBuilder: (_, __) => const SizedBox(width: 10),
                  itemBuilder: (_, i) => GestureDetector(
                    onTap: () => setState(() => _selectedChild = i),
                    child: AnimatedContainer(
                      duration: const Duration(milliseconds: 200),
                      padding: const EdgeInsets.symmetric(
                        horizontal: 18,
                        vertical: 10,
                      ),
                      decoration: BoxDecoration(
                        color: _selectedChild == i
                            ? const Color(0xFF2D1B69)
                            : Colors.white,
                        borderRadius: BorderRadius.circular(24),
                        border: Border.all(
                          color: _selectedChild == i
                              ? const Color(0xFF2D1B69)
                              : Colors.grey.shade200,
                        ),
                      ),
                      child: Text(
                        _children[i]['name'] ?? '',
                        style: TextStyle(
                          color: _selectedChild == i
                              ? Colors.white
                              : AppColors.textMid,
                          fontWeight: FontWeight.w600,
                          fontSize: 14,
                        ),
                      ),
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 18),
            ],
            if (child.isNotEmpty) ...[
              _ApiChildSummaryCard(child: child),
              const SizedBox(height: 20),
              const _SectionTitle('ملخص الأداء'),
              const SizedBox(height: 12),
              Row(
                children: [
                  Expanded(
                    child: _QuickStat(
                      label: 'سجلات الحفظ',
                      value: '${child['totalMemorization'] ?? 0}',
                      icon: Icons.menu_book_rounded,
                      color: AppColors.teal,
                    ),
                  ),
                  const SizedBox(width: 10),
                  Expanded(
                    child: _QuickStat(
                      label: 'أيام الغياب',
                      value: '${child['totalAbsent'] ?? 0}',
                      icon: Icons.event_busy_rounded,
                      color: AppColors.error,
                    ),
                  ),
                  const SizedBox(width: 10),
                  Expanded(
                    child: _QuickStat(
                      label: 'نسبة الحضور',
                      value:
                          '${child['attendanceRate']?.toStringAsFixed(0) ?? 0}%',
                      icon: Icons.trending_up_rounded,
                      color: AppColors.success,
                    ),
                  ),
                ],
              ),
            ],
            const SizedBox(height: 20),
            const _SectionTitle('آخر الإشعارات'),
            const SizedBox(height: 12),
            _buildParentNotifList(),
          ],
        ),
      ),
    );
  }

  Widget _buildParentNotifList() {
    if (_children.isEmpty) return const SizedBox();
    final child = _currentChild;
    final childName = child['name'] ?? 'الطالب';
    final notifs = [
      (
        '📚',
        'آخر تقييم لـ $childName: ${child['lastGrade'] ?? 'غير محدد'}',
        'مؤخراً',
      ),
      (
        '📊',
        'نسبة حضور $childName: ${child['attendanceRate']?.toStringAsFixed(1) ?? 0}%',
        'هذا الشهر',
      ),
      (
        '📅',
        'حلقة: ${child['circle']?['name'] ?? 'غير محدد'}',
        'معلومات الحلقة',
      ),
    ];
    return Column(
      children: notifs
          .map(
            (n) => Container(
              margin: const EdgeInsets.only(bottom: 10),
              padding: const EdgeInsets.all(14),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(14),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.04),
                    blurRadius: 10,
                    offset: const Offset(0, 3),
                  ),
                ],
              ),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(n.$1, style: const TextStyle(fontSize: 20)),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          n.$2,
                          style: const TextStyle(
                            fontSize: 13,
                            color: AppColors.textDark,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          n.$3,
                          style: const TextStyle(
                            fontSize: 11,
                            color: AppColors.textLight,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          )
          .toList(),
    );
  }

  Widget _buildChildrenTab() {
    if (_children.isEmpty)
      return const Center(
        child: Text(
          'لا يوجد أبناء مسجلون',
          style: TextStyle(color: AppColors.textLight),
        ),
      );
    return ListView.separated(
      padding: const EdgeInsets.all(18),
      itemCount: _children.length,
      separatorBuilder: (_, __) => const SizedBox(height: 14),
      itemBuilder: (_, i) {
        final c = _children[i];
        return GestureDetector(
          onTap: () => setState(() {
            _selectedChild = i;
            _currentTab = 0;
          }),
          child: Container(
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(20),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withOpacity(0.06),
                  blurRadius: 15,
                  offset: const Offset(0, 5),
                ),
              ],
            ),
            child: Row(
              children: [
                Container(
                  width: 56,
                  height: 56,
                  decoration: const BoxDecoration(
                    gradient: LinearGradient(
                      colors: [Color(0xFF2D1B69), Color(0xFF4A2D8F)],
                    ),
                    shape: BoxShape.circle,
                  ),
                  child: const Icon(
                    Icons.person,
                    color: Colors.white,
                    size: 28,
                  ),
                ),
                const SizedBox(width: 16),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        c['name'] ?? '',
                        style: const TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 16,
                          color: AppColors.textDark,
                        ),
                      ),
                      Text(
                        c['circle']?['name'] ?? 'غير محدد',
                        style: const TextStyle(
                          color: AppColors.textLight,
                          fontSize: 13,
                        ),
                      ),
                      const SizedBox(height: 8),
                      Row(
                        children: [
                          _MiniChip(
                            label:
                                'حضور ${c['attendanceRate']?.toStringAsFixed(0) ?? 0}%',
                            color: AppColors.success,
                          ),
                          const SizedBox(width: 8),
                          _MiniChip(
                            label: '${c['totalMemorization'] ?? 0} سجل',
                            color: AppColors.teal,
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
                const Icon(
                  Icons.arrow_forward_ios,
                  color: AppColors.textLight,
                  size: 16,
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  Widget _buildNotificationsTab() {
    return ListView.builder(
      padding: const EdgeInsets.all(18),
      itemCount: _children.length,
      itemBuilder: (_, i) {
        final c = _children[i];
        return Container(
          margin: const EdgeInsets.only(bottom: 12),
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(14),
            border: Border.all(color: Colors.grey.shade100),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                c['name'] ?? '',
                style: const TextStyle(
                  fontWeight: FontWeight.bold,
                  fontSize: 15,
                  color: AppColors.textDark,
                ),
              ),
              const Divider(height: 16),
              Row(
                children: [
                  const Icon(
                    Icons.check_circle,
                    color: AppColors.success,
                    size: 16,
                  ),
                  const SizedBox(width: 8),
                  Text(
                    'حضور: ${c['totalPresent'] ?? 0} يوم',
                    style: const TextStyle(
                      fontSize: 13,
                      color: AppColors.textMid,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 6),
              Row(
                children: [
                  const Icon(Icons.cancel, color: AppColors.error, size: 16),
                  const SizedBox(width: 8),
                  Text(
                    'غياب: ${c['totalAbsent'] ?? 0} يوم',
                    style: const TextStyle(
                      fontSize: 13,
                      color: AppColors.textMid,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 6),
              Row(
                children: [
                  const Icon(Icons.star, color: AppColors.gold, size: 16),
                  const SizedBox(width: 8),
                  Text(
                    'آخر تقييم: ${c['lastGrade'] ?? 'غير محدد'}',
                    style: const TextStyle(
                      fontSize: 13,
                      color: AppColors.textMid,
                    ),
                  ),
                ],
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildParentProfile() {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(18),
      child: Column(
        children: [
          Container(
            padding: const EdgeInsets.all(24),
            decoration: BoxDecoration(
              gradient: const LinearGradient(
                colors: [Color(0xFF2D1B69), Color(0xFF4A2D8F)],
              ),
              borderRadius: BorderRadius.circular(24),
            ),
            child: Column(
              children: [
                Container(
                  width: 80,
                  height: 80,
                  decoration: BoxDecoration(
                    color: Colors.white.withOpacity(0.2),
                    shape: BoxShape.circle,
                    border: Border.all(
                      color: AppColors.gold.withOpacity(0.5),
                      width: 2,
                    ),
                  ),
                  child: const Icon(
                    Icons.family_restroom_rounded,
                    color: Colors.white,
                    size: 40,
                  ),
                ),
                const SizedBox(height: 14),
                Text(
                  _parentData['name'] ?? widget.name,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 20,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  'ولي أمر • ${_children.length} طلاب',
                  style: TextStyle(
                    color: Colors.white.withOpacity(0.7),
                    fontSize: 13,
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 20),
          _ProfileInfoCard(
            items: [
              _ProfileItem(
                Icons.child_care_rounded,
                'عدد الأبناء',
                '${_children.length}',
              ),
              _ProfileItem(
                Icons.phone_outlined,
                'رقم الهاتف',
                _parentData['phone'] ?? '-',
              ),
              _ProfileItem(
                Icons.email_outlined,
                'البريد الإلكتروني',
                _parentData['email'] ?? '-',
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildParentBottomNav() {
    final items = [
      (Icons.home_rounded, 'الرئيسية'),
      (Icons.group_rounded, 'أبنائي'),
      (Icons.notifications_rounded, 'الإشعارات'),
      (Icons.person_rounded, 'حسابي'),
    ];

    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.06),
            blurRadius: 20,
            offset: const Offset(0, -5),
          ),
        ],
      ),
      child: SafeArea(
        top: false,
        child: Padding(
          padding: const EdgeInsets.symmetric(vertical: 8),
          child: Row(
            children: List.generate(
              items.length,
              (i) => Expanded(
                child: GestureDetector(
                  onTap: () => setState(() => _currentTab = i),
                  behavior: HitTestBehavior.opaque,
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      AnimatedContainer(
                        duration: const Duration(milliseconds: 200),
                        padding: const EdgeInsets.all(6),
                        decoration: BoxDecoration(
                          color: _currentTab == i
                              ? const Color(0xFF2D1B69).withOpacity(0.1)
                              : Colors.transparent,
                          borderRadius: BorderRadius.circular(10),
                        ),
                        child: Icon(
                          items[i].$1,
                          color: _currentTab == i
                              ? const Color(0xFF2D1B69)
                              : AppColors.textLight,
                          size: 24,
                        ),
                      ),
                      const SizedBox(height: 3),
                      Text(
                        items[i].$2,
                        style: TextStyle(
                          fontSize: 11,
                          color: _currentTab == i
                              ? const Color(0xFF2D1B69)
                              : AppColors.textLight,
                          fontWeight: _currentTab == i
                              ? FontWeight.w600
                              : FontWeight.normal,
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

// =====================================================================
// مكونات API الجديدة
// =====================================================================

class _RealSummaryCard extends StatelessWidget {
  final String attendanceRate, memorizedCount, circleName, nextSession;
  const _RealSummaryCard({
    required this.attendanceRate,
    required this.memorizedCount,
    required this.circleName,
    required this.nextSession,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          begin: Alignment.topRight,
          end: Alignment.bottomLeft,
          colors: [Color(0xFF0D4A4A), Color(0xFF1A7070)],
        ),
        borderRadius: BorderRadius.circular(22),
        boxShadow: [
          BoxShadow(
            color: AppColors.teal.withOpacity(0.3),
            blurRadius: 20,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Icon(Icons.groups_rounded, color: AppColors.gold, size: 16),
              const SizedBox(width: 6),
              Text(
                circleName,
                style: TextStyle(
                  color: AppColors.gold.withOpacity(0.9),
                  fontSize: 13,
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      attendanceRate,
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 36,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const Text(
                      'نسبة الحضور',
                      style: TextStyle(color: Colors.white60, fontSize: 12),
                    ),
                  ],
                ),
              ),
              Container(
                width: 1,
                height: 50,
                color: Colors.white.withOpacity(0.15),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      memorizedCount,
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 36,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const Text(
                      'سجل حفظ',
                      style: TextStyle(color: Colors.white60, fontSize: 12),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
            decoration: BoxDecoration(
              color: Colors.white.withOpacity(0.1),
              borderRadius: BorderRadius.circular(10),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Icon(
                  Icons.access_time_rounded,
                  color: AppColors.gold,
                  size: 14,
                ),
                const SizedBox(width: 6),
                Flexible(
                  child: Text(
                    'الحلقة: $nextSession',
                    style: const TextStyle(color: Colors.white70, fontSize: 12),
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

class _ApiChildSummaryCard extends StatelessWidget {
  final Map child;
  const _ApiChildSummaryCard({required this.child});
  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [Color(0xFF2D1B69), Color(0xFF4A2D8F)],
        ),
        borderRadius: BorderRadius.circular(22),
        boxShadow: [
          BoxShadow(
            color: const Color(0xFF2D1B69).withOpacity(0.3),
            blurRadius: 20,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: Row(
        children: [
          Container(
            width: 64,
            height: 64,
            decoration: BoxDecoration(
              color: Colors.white.withOpacity(0.15),
              shape: BoxShape.circle,
              border: Border.all(
                color: AppColors.gold.withOpacity(0.4),
                width: 2,
              ),
            ),
            child: const Icon(Icons.person, color: Colors.white, size: 34),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  child['name'] ?? '',
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                Text(
                  child['circle']?['name'] ?? 'غير محدد',
                  style: TextStyle(
                    color: Colors.white.withOpacity(0.7),
                    fontSize: 13,
                  ),
                ),
                const SizedBox(height: 8),
                Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 10,
                    vertical: 4,
                  ),
                  decoration: BoxDecoration(
                    color: AppColors.gold.withOpacity(0.2),
                    borderRadius: BorderRadius.circular(20),
                    border: Border.all(color: AppColors.gold.withOpacity(0.4)),
                  ),
                  child: Text(
                    'حضور ${child['attendanceRate']?.toStringAsFixed(1) ?? 0}%',
                    style: const TextStyle(color: AppColors.gold, fontSize: 12),
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

class _ApiMemCard extends StatelessWidget {
  final dynamic record;
  final bool detailed;
  const _ApiMemCard({required this.record, this.detailed = false});

  Color get _color {
    switch (record['grade'] ?? '') {
      case 'ممتاز':
        return AppColors.success;
      case 'جيد جداً':
        return Colors.blue;
      case 'جيد':
        return AppColors.warning;
      default:
        return AppColors.error;
    }
  }

  String get _emoji {
    switch (record['grade'] ?? '') {
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

  @override
  Widget build(BuildContext context) {
    final surahStart = record['surahStart'] ?? '';
    final ayahStart = record['ayahStart'] ?? '';
    final surahEnd = record['surahEnd'] ?? '';
    final ayahEnd = record['ayahEnd'] ?? '';
    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: _color.withOpacity(0.15)),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Row(
        children: [
          Container(
            width: 42,
            height: 42,
            decoration: BoxDecoration(
              color: _color.withOpacity(0.1),
              shape: BoxShape.circle,
            ),
            child: Center(
              child: Text(_emoji, style: const TextStyle(fontSize: 18)),
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  '$surahStart → $surahEnd',
                  style: const TextStyle(
                    fontWeight: FontWeight.bold,
                    fontSize: 14,
                    color: AppColors.textDark,
                  ),
                ),
                Text(
                  'الآيات $ayahStart-$ayahEnd${detailed ? ' • ${record['date'] ?? ''}' : ''}',
                  style: const TextStyle(
                    color: AppColors.textLight,
                    fontSize: 12,
                  ),
                ),
              ],
            ),
          ),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
            decoration: BoxDecoration(
              color: _color.withOpacity(0.1),
              borderRadius: BorderRadius.circular(20),
            ),
            child: Text(
              record['grade'] ?? '',
              style: TextStyle(
                color: _color,
                fontSize: 12,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _ApiAttendanceCard extends StatelessWidget {
  final dynamic record;
  const _ApiAttendanceCard({required this.record});
  @override
  Widget build(BuildContext context) {
    final isPresent = record['status'] == 'حاضر';
    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(
          color: isPresent
              ? AppColors.success.withOpacity(0.2)
              : AppColors.error.withOpacity(0.2),
        ),
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: isPresent
                  ? AppColors.success.withOpacity(0.1)
                  : AppColors.error.withOpacity(0.1),
              shape: BoxShape.circle,
            ),
            child: Icon(
              isPresent ? Icons.check_circle_rounded : Icons.cancel_rounded,
              color: isPresent ? AppColors.success : AppColors.error,
              size: 20,
            ),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Text(
              record['date'] ?? '',
              style: const TextStyle(
                fontWeight: FontWeight.w500,
                color: AppColors.textDark,
              ),
            ),
          ),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
            decoration: BoxDecoration(
              color: isPresent
                  ? AppColors.success.withOpacity(0.1)
                  : AppColors.error.withOpacity(0.1),
              borderRadius: BorderRadius.circular(20),
            ),
            child: Text(
              record['status'] ?? '',
              style: TextStyle(
                color: isPresent ? AppColors.success : AppColors.error,
                fontSize: 12,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _RealScheduleCard extends StatelessWidget {
  final String days, startPrayer;
  const _RealScheduleCard({required this.days, required this.startPrayer});
  @override
  Widget build(BuildContext context) {
    final dayList = days.isNotEmpty ? days.split(', ') : ['غير محدد'];
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        children: dayList
            .map(
              (d) => Padding(
                padding: const EdgeInsets.symmetric(vertical: 8),
                child: Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.all(8),
                      decoration: BoxDecoration(
                        color: AppColors.teal.withOpacity(0.1),
                        shape: BoxShape.circle,
                      ),
                      child: const Icon(
                        Icons.calendar_today,
                        color: AppColors.teal,
                        size: 14,
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Text(
                        d.trim(),
                        style: const TextStyle(
                          fontWeight: FontWeight.w500,
                          color: AppColors.textDark,
                        ),
                      ),
                    ),
                    Text(
                      'بعد $startPrayer',
                      style: const TextStyle(
                        color: AppColors.textLight,
                        fontSize: 13,
                      ),
                    ),
                    const SizedBox(width: 10),
                    Container(
                      width: 8,
                      height: 8,
                      decoration: const BoxDecoration(
                        color: AppColors.success,
                        shape: BoxShape.circle,
                      ),
                    ),
                  ],
                ),
              ),
            )
            .toList(),
      ),
    );
  }
}

// =====================================================================
// مكونات مساعدة مشتركة
// =====================================================================

class _SectionTitle extends StatelessWidget {
  final String text;
  const _SectionTitle(this.text);
  @override
  Widget build(BuildContext context) => Text(
    text,
    style: const TextStyle(
      fontSize: 17,
      fontWeight: FontWeight.bold,
      color: AppColors.textDark,
    ),
  );
}

class _QuickStat extends StatelessWidget {
  final String label, value;
  final IconData icon;
  final Color color;
  const _QuickStat({
    required this.label,
    required this.value,
    required this.icon,
    required this.color,
  });
  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 10,
            offset: const Offset(0, 3),
          ),
        ],
      ),
      child: Column(
        children: [
          Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: color.withOpacity(0.1),
              shape: BoxShape.circle,
            ),
            child: Icon(icon, color: color, size: 18),
          ),
          const SizedBox(height: 8),
          Text(
            value,
            style: TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 18,
              color: color,
            ),
          ),
          Text(
            label,
            textAlign: TextAlign.center,
            style: const TextStyle(color: AppColors.textLight, fontSize: 10),
          ),
        ],
      ),
    );
  }
}

class _GradeBar extends StatefulWidget {
  final String label;
  final int count, total;
  final Color color;
  const _GradeBar({
    required this.label,
    required this.count,
    required this.total,
    required this.color,
  });
  @override
  State<_GradeBar> createState() => _GradeBarState();
}

class _GradeBarState extends State<_GradeBar>
    with SingleTickerProviderStateMixin {
  late AnimationController _ctrl;
  late Animation<double> _width;
  @override
  void initState() {
    super.initState();
    _ctrl = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1000),
    );
    _width = Tween<double>(
      begin: 0,
      end: widget.total > 0 ? widget.count / widget.total : 0,
    ).animate(CurvedAnimation(parent: _ctrl, curve: Curves.easeOutCubic));
    _ctrl.forward();
  }

  @override
  void dispose() {
    _ctrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                widget.label,
                style: const TextStyle(fontSize: 13, color: AppColors.textMid),
              ),
              Text(
                '${widget.count} مرة',
                style: TextStyle(
                  fontSize: 12,
                  color: widget.color,
                  fontWeight: FontWeight.w600,
                ),
              ),
            ],
          ),
          const SizedBox(height: 6),
          ClipRRect(
            borderRadius: BorderRadius.circular(6),
            child: AnimatedBuilder(
              animation: _width,
              builder: (_, __) => LinearProgressIndicator(
                value: _width.value,
                backgroundColor: Colors.grey.shade100,
                valueColor: AlwaysStoppedAnimation(widget.color),
                minHeight: 8,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _CircularProgress extends StatefulWidget {
  final double value;
  const _CircularProgress({required this.value});
  @override
  State<_CircularProgress> createState() => _CircularProgressState();
}

class _CircularProgressState extends State<_CircularProgress>
    with SingleTickerProviderStateMixin {
  late AnimationController _ctrl;
  late Animation<double> _anim;
  @override
  void initState() {
    super.initState();
    _ctrl = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1200),
    );
    _anim = Tween<double>(
      begin: 0,
      end: widget.value,
    ).animate(CurvedAnimation(parent: _ctrl, curve: Curves.easeOutCubic));
    _ctrl.forward();
  }

  @override
  void dispose() {
    _ctrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AnimatedBuilder(
      animation: _anim,
      builder: (_, __) => SizedBox(
        width: 80,
        height: 80,
        child: Stack(
          alignment: Alignment.center,
          children: [
            CircularProgressIndicator(
              value: _anim.value,
              backgroundColor: Colors.white.withOpacity(0.2),
              valueColor: const AlwaysStoppedAnimation(AppColors.gold),
              strokeWidth: 6,
            ),
            Text(
              '${(_anim.value * 100).round()}%',
              style: const TextStyle(
                color: Colors.white,
                fontWeight: FontWeight.bold,
                fontSize: 14,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _MiniChip extends StatelessWidget {
  final String label;
  final Color color;
  const _MiniChip({required this.label, required this.color});
  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 3),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(
        label,
        style: TextStyle(
          color: color,
          fontSize: 11,
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }
}

class _ProfileInfoCard extends StatelessWidget {
  final List<_ProfileItem> items;
  const _ProfileInfoCard({required this.items});
  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 15,
            offset: const Offset(0, 5),
          ),
        ],
      ),
      child: Column(
        children: items
            .asMap()
            .entries
            .map(
              (e) => Column(
                children: [
                  Padding(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 20,
                      vertical: 14,
                    ),
                    child: Row(
                      children: [
                        Container(
                          padding: const EdgeInsets.all(8),
                          decoration: BoxDecoration(
                            color: AppColors.teal.withOpacity(0.08),
                            shape: BoxShape.circle,
                          ),
                          child: Icon(
                            e.value.icon,
                            color: AppColors.teal,
                            size: 18,
                          ),
                        ),
                        const SizedBox(width: 14),
                        Text(
                          e.value.label,
                          style: const TextStyle(
                            color: AppColors.textLight,
                            fontSize: 13,
                          ),
                        ),
                        const Spacer(),
                        Text(
                          e.value.value,
                          style: const TextStyle(
                            fontWeight: FontWeight.w600,
                            color: AppColors.textDark,
                            fontSize: 14,
                          ),
                        ),
                      ],
                    ),
                  ),
                  if (e.key < items.length - 1)
                    Divider(
                      height: 1,
                      color: Colors.grey.shade100,
                      indent: 20,
                      endIndent: 20,
                    ),
                ],
              ),
            )
            .toList(),
      ),
    );
  }
}

class _ProfileItem {
  final IconData icon;
  final String label, value;
  _ProfileItem(this.icon, this.label, this.value);
}

class _MemRecord {
  final String surah, verses, grade, date;
  _MemRecord(this.surah, this.verses, this.grade, this.date);
}

class _ChildData {
  final String name, circleName, attendanceRate, memorizedPages, absentDays;
  _ChildData(
    this.name,
    this.circleName,
    this.attendanceRate,
    this.memorizedPages,
    this.absentDays,
  );
}
