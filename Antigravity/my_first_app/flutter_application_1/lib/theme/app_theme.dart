import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

class AppTheme {
  // الألوان الأساسية مستوحاة من الأخضر الزمردي والذهبي المطفي
  static const Color emeraldGreen = Color(0xFF0D6B5A);
  static const Color lightEmerald = Color(0xFFE8F3F1);
  static const Color matteGold = Color(0xFFD4AF37);
  static const Color darkBackground = Color(0xFF121212);
  static const Color cardDark = Color(0xFF1E1E1E);

  static ThemeData get lightTheme {
    return ThemeData(
      useMaterial3: true,
      colorScheme: ColorScheme.fromSeed(
        seedColor: emeraldGreen,
        primary: emeraldGreen,
        secondary: matteGold,
        surface: Colors.white,
      ),
      scaffoldBackgroundColor: const Color(0xFFF8F9FA),
      textTheme: GoogleFonts.tajawalTextTheme(ThemeData.light().textTheme),
      appBarTheme: const AppBarTheme(
        backgroundColor: emeraldGreen,
        foregroundColor: Colors.white,
        centerTitle: true,
        elevation: 0,
      ),
      cardTheme: CardThemeData(
        elevation: 4,
        shadowColor: emeraldGreen.withOpacity(0.1),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(16),
        ),
      ),
    );
  }

  static ThemeData get darkTheme {
    return ThemeData(
      useMaterial3: true,
      brightness: Brightness.dark,
      colorScheme: ColorScheme.fromSeed(
        brightness: Brightness.dark,
        seedColor: emeraldGreen,
        primary: emeraldGreen,
        secondary: matteGold,
        surface: cardDark,
      ),
      scaffoldBackgroundColor: darkBackground,
      textTheme: GoogleFonts.tajawalTextTheme(ThemeData.dark().textTheme),
      appBarTheme: const AppBarTheme(
        backgroundColor: cardDark,
        foregroundColor: matteGold,
        centerTitle: true,
        elevation: 0,
      ),
      cardTheme: CardThemeData(
        elevation: 4,
        shadowColor: Colors.black.withOpacity(0.4),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(16),
        ),
        color: cardDark,
      ),
    );
  }

  static BoxDecoration neumorphicBox(BuildContext context, {double radius = 16, bool isPressed = false}) {
    bool isDark = Theme.of(context).brightness == Brightness.dark;
    
    return BoxDecoration(
      color: isDark ? cardDark : Colors.white,
      borderRadius: BorderRadius.circular(radius),
      boxShadow: isPressed ? null : [
        BoxShadow(
          color: isDark ? Colors.black54 : Colors.grey.withOpacity(0.15),
          offset: const Offset(4, 4),
          blurRadius: 10,
        ),
        BoxShadow(
          color: isDark ? Colors.white.withOpacity(0.05) : Colors.white,
          offset: const Offset(-4, -4),
          blurRadius: 10,
        ),
      ],
    );
  }
}
