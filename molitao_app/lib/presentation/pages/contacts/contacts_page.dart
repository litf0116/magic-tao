import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../../providers/contacts_provider.dart';
import '../../mixins/auth_guard_mixin.dart';

class ContactsPage extends ConsumerStatefulWidget {
  const ContactsPage({super.key});

  @override
  ConsumerState<ContactsPage> createState() => _ContactsPageState();
}

class _ContactsPageState extends ConsumerState<ContactsPage>
    with AuthGuardMixin {
  final TextEditingController _searchController = TextEditingController();

  @override
  void initState() {
    super.initState();
    // AuthGuardMixin will check login status in initState
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(contactsProvider.notifier).loadFriends();
    });
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(contactsProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('通讯录'),
        backgroundColor: const Color(0xFFf4835a),
        foregroundColor: Colors.white,
      ),
      body: RefreshIndicator(
        onRefresh: () => ref.read(contactsProvider.notifier).loadFriends(),
        child: Column(
          children: [
            // Search bar
            Container(
              padding: const EdgeInsets.all(12),
              color: Colors.white,
              child: TextField(
                controller: _searchController,
                onChanged: (value) {
                  ref.read(contactsProvider.notifier).setFilterText(value);
                },
                decoration: InputDecoration(
                  hintText: '搜索联系人',
                  prefixIcon: const Icon(Icons.search, color: Colors.grey),
                  filled: true,
                  fillColor: Colors.grey[100],
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(24),
                    borderSide: BorderSide.none,
                  ),
                  contentPadding: const EdgeInsets.symmetric(vertical: 0),
                ),
              ),
            ),

            // Content
            Expanded(
              child: state.isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : _buildContent(state),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildContent(ContactsState state) {
    return ListView(
      children: [
        // Friend requests section
        if (state.friendRequests.isNotEmpty) ...[
          _buildSectionTitle('好友申请'),
          ...state.friendRequests.map(
            (friend) => _buildFriendRequestItem(friend),
          ),
        ],

        // Friends section
        _buildSectionTitle('好友'),
        if (state.filteredFriends.isEmpty)
          const Padding(
            padding: EdgeInsets.all(32),
            child: Center(
              child: Text('暂无好友', style: TextStyle(color: Colors.grey)),
            ),
          )
        else
          ...state.filteredFriends.map((friend) => _buildFriendItem(friend)),

        const SizedBox(height: 80), // Bottom padding for tab bar
      ],
    );
  }

  Widget _buildSectionTitle(String title) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      color: const Color(0xFFf3f4f7),
      child: Text(
        title,
        style: const TextStyle(fontSize: 15, color: Color(0xFF666666)),
      ),
    );
  }

  Widget _buildFriendRequestItem(FriendItem friend) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      decoration: const BoxDecoration(
        color: Colors.white,
        border: Border(bottom: BorderSide(color: Color(0xFFefefef))),
      ),
      child: Row(
        children: [
          // Avatar
          _buildAvatar(friend.avatar, 48),

          const SizedBox(width: 16),

          // Name
          Expanded(
            child: Text(
              friend.name,
              style: const TextStyle(fontSize: 15, fontWeight: FontWeight.bold),
            ),
          ),

          // Action buttons
          Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              GestureDetector(
                onTap: () => _handleFriendRequest(friend.id, true),
                child: Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 12,
                    vertical: 6,
                  ),
                  decoration: BoxDecoration(
                    color: const Color(0xFF07c160),
                    borderRadius: BorderRadius.circular(4),
                  ),
                  child: const Text(
                    '同意',
                    style: TextStyle(color: Colors.white, fontSize: 13),
                  ),
                ),
              ),
              const SizedBox(width: 8),
              GestureDetector(
                onTap: () => _handleFriendRequest(friend.id, false),
                child: Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 12,
                    vertical: 6,
                  ),
                  decoration: BoxDecoration(
                    color: Colors.grey[300],
                    borderRadius: BorderRadius.circular(4),
                  ),
                  child: const Text(
                    '拒绝',
                    style: TextStyle(color: Colors.black87, fontSize: 13),
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildFriendItem(FriendItem friend) {
    return GestureDetector(
      onTap: () {
        context.push(
          '/chat/private/${friend.id}',
          extra: {'name': friend.name, 'avatar': friend.avatar},
        );
      },
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        decoration: const BoxDecoration(
          color: Colors.white,
          border: Border(bottom: BorderSide(color: Color(0xFFefefef))),
        ),
        child: Row(
          children: [
            // Avatar
            _buildAvatar(friend.avatar, 48),

            const SizedBox(width: 16),

            // Name
            Expanded(
              child: Text(
                friend.name,
                style: const TextStyle(
                  fontSize: 15,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildAvatar(String? url, double size) {
    if (url != null && url.isNotEmpty) {
      return ClipRRect(
        borderRadius: BorderRadius.circular(size / 2),
        child: CachedNetworkImage(
          imageUrl: url,
          width: size,
          height: size,
          fit: BoxFit.cover,
          placeholder: (context, url) => Container(
            color: Colors.grey[200],
            child: Icon(Icons.person, size: size / 2, color: Colors.grey),
          ),
          errorWidget: (context, url, error) => Container(
            color: Colors.grey[200],
            child: Icon(Icons.person, size: size / 2, color: Colors.grey),
          ),
        ),
      );
    }

    return Container(
      width: size,
      height: size,
      decoration: BoxDecoration(
        color: Colors.grey[300],
        shape: BoxShape.circle,
      ),
      child: Icon(Icons.person, size: size / 2, color: Colors.white),
    );
  }

  void _handleFriendRequest(int friendId, bool accept) async {
    try {
      await ref
          .read(contactsProvider.notifier)
          .handleFriendRequest(friendId, accept);
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text(accept ? '已同意好友申请' : '已拒绝好友申请')));
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('操作失败: $e')));
      }
    }
  }
}
