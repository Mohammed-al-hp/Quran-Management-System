import 'package:flutter/material.dart';
import '../services/api_service.dart';

/// شاشة الإشعارات - تعرض جميع إشعارات المستخدم
/// تدعم التحديد كمقروء بشكل فردي وجماعي
class NotificationsScreen extends StatefulWidget {
  const NotificationsScreen({Key? key}) : super(key: key);

  @override
  State<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends State<NotificationsScreen> {
  final ApiService _apiService = ApiService();
  List<dynamic> _notifications = [];
  int _unreadCount = 0;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadNotifications();
  }

  Future<void> _loadNotifications() async {
    setState(() => _isLoading = true);
    final data = await _apiService.getNotifications();
    if (mounted) {
      setState(() {
        _notifications = data['notifications'] ?? [];
        _unreadCount = data['unreadCount'] ?? 0;
        _isLoading = false;
      });
    }
  }

  Future<void> _markAsRead(int id, int index) async {
    final success = await _apiService.markNotificationRead(id);
    if (success && mounted) {
      setState(() {
        _notifications[index]['isRead'] = true;
        _unreadCount = (_unreadCount - 1).clamp(0, _unreadCount);
      });
    }
  }

  Future<void> _markAllRead() async {
    final success = await _apiService.markAllNotificationsRead();
    if (success && mounted) {
      setState(() {
        for (var n in _notifications) {
          n['isRead'] = true;
        }
        _unreadCount = 0;
      });
    }
  }

  IconData _getNotificationIcon(String? type) {
    switch (type) {
      case 'Attendance':
        return Icons.calendar_today;
      case 'Task':
        return Icons.assignment;
      case 'Grade':
        return Icons.star;
      default:
        return Icons.notifications;
    }
  }

  Color _getNotificationColor(String? type) {
    switch (type) {
      case 'Attendance':
        return const Color(0xFF28a745);
      case 'Task':
        return const Color(0xFF007bff);
      case 'Grade':
        return const Color(0xFFC5A059);
      default:
        return const Color(0xFF1D5D5D);
    }
  }

  @override
  Widget build(BuildContext context) {
    const Color primaryTeal = Color(0xFF1D5D5D);

    return Scaffold(
      appBar: AppBar(
        title: Row(
          children: [
            const Text('الإشعارات', style: TextStyle(fontWeight: FontWeight.bold)),
            if (_unreadCount > 0) ...[
              const SizedBox(width: 8),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                decoration: BoxDecoration(
                  color: Colors.red,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Text(
                  '$_unreadCount',
                  style: const TextStyle(color: Colors.white, fontSize: 12),
                ),
              ),
            ],
          ],
        ),
        backgroundColor: primaryTeal,
        foregroundColor: Colors.white,
        actions: [
          if (_unreadCount > 0)
            TextButton(
              onPressed: _markAllRead,
              child: const Text(
                'قراءة الكل',
                style: TextStyle(color: Colors.white70),
              ),
            ),
        ],
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _notifications.isEmpty
              ? Center(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Icon(Icons.notifications_off, size: 80, color: Colors.grey[300]),
                      const SizedBox(height: 16),
                      Text(
                        'لا توجد إشعارات',
                        style: TextStyle(fontSize: 18, color: Colors.grey[500]),
                      ),
                    ],
                  ),
                )
              : RefreshIndicator(
                  onRefresh: _loadNotifications,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(8),
                    itemCount: _notifications.length,
                    itemBuilder: (context, index) {
                      final notification = _notifications[index];
                      final isRead = notification['isRead'] == true;
                      final type = notification['type'] as String?;

                      return Card(
                        elevation: isRead ? 0 : 2,
                        color: isRead ? Colors.grey[50] : Colors.white,
                        margin: const EdgeInsets.symmetric(vertical: 4),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(12),
                          side: isRead
                              ? BorderSide.none
                              : BorderSide(color: _getNotificationColor(type).withOpacity(0.3)),
                        ),
                        child: ListTile(
                          contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                          leading: CircleAvatar(
                            backgroundColor: _getNotificationColor(type).withOpacity(0.15),
                            child: Icon(
                              _getNotificationIcon(type),
                              color: _getNotificationColor(type),
                            ),
                          ),
                          title: Text(
                            notification['title'] ?? '',
                            style: TextStyle(
                              fontWeight: isRead ? FontWeight.normal : FontWeight.bold,
                              fontSize: 15,
                            ),
                          ),
                          subtitle: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              const SizedBox(height: 4),
                              Text(
                                notification['message'] ?? '',
                                style: TextStyle(fontSize: 13, color: Colors.grey[600]),
                              ),
                              const SizedBox(height: 6),
                              Text(
                                notification['createdAt'] ?? '',
                                style: TextStyle(fontSize: 11, color: Colors.grey[400]),
                              ),
                            ],
                          ),
                          trailing: !isRead
                              ? Container(
                                  width: 10,
                                  height: 10,
                                  decoration: BoxDecoration(
                                    color: _getNotificationColor(type),
                                    shape: BoxShape.circle,
                                  ),
                                )
                              : null,
                          onTap: () {
                            if (!isRead) {
                              _markAsRead(notification['id'], index);
                            }
                          },
                        ),
                      );
                    },
                  ),
                ),
    );
  }
}
