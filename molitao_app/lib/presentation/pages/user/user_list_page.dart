import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../providers/user_provider.dart';
import '../../../data/models/user_model.dart';

class UserListPage extends ConsumerStatefulWidget {
  const UserListPage({super.key});

  @override
  ConsumerState<UserListPage> createState() => _UserListPageState();
}

class _UserListPageState extends ConsumerState<UserListPage> {
  final TextEditingController _searchController = TextEditingController();
  final ScrollController _scrollController = ScrollController();

  @override
  void initState() {
    super.initState();
    // 初始加载用户列表
    Future.microtask(() {
      ref.read(userListProvider.notifier).loadUsers();
    });
  }

  @override
  void dispose() {
    _searchController.dispose();
    _scrollController.dispose();
    super.dispose();
  }

  void _onSearch(String keyword) {
    ref.read(userListProvider.notifier).searchUsers(keyword);
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(userListProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('用户列表'),
        backgroundColor: const Color(0xFFf4835a),
        foregroundColor: Colors.white,
      ),
      body: Column(
        children: [
          // 搜索框
          Container(
            padding: const EdgeInsets.all(16),
            color: Colors.white,
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: '请输入姓名或账号',
                prefixIcon: const Icon(Icons.search),
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(8),
                ),
                filled: true,
                fillColor: Colors.grey[100],
              ),
              onChanged: _onSearch,
            ),
          ),
          // 用户列表
          Expanded(
            child: state.isLoading && state.users.isEmpty
                ? const Center(child: CircularProgressIndicator())
                : state.error != null && state.users.isEmpty
                ? Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Text('加载失败: ${state.error}'),
                        const SizedBox(height: 16),
                        ElevatedButton(
                          onPressed: () =>
                              ref.read(userListProvider.notifier).loadUsers(),
                          child: const Text('重试'),
                        ),
                      ],
                    ),
                  )
                : state.users.isEmpty
                ? const Center(child: Text('暂无数据'))
                : RefreshIndicator(
                    onRefresh: () =>
                        ref.read(userListProvider.notifier).loadUsers(),
                    child: ListView.builder(
                      controller: _scrollController,
                      itemCount: state.users.length,
                      itemBuilder: (context, index) {
                        final user = state.users[index];
                        return _buildUserItem(user);
                      },
                    ),
                  ),
          ),
          // 底部统计
          if (state.users.isNotEmpty)
            Container(
              padding: const EdgeInsets.all(12),
              color: Colors.grey[100],
              child: Center(
                child: Text(
                  '共 ${state.totalCount} 条数据',
                  style: const TextStyle(color: Colors.grey),
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildUserItem(UserDto user) {
    return InkWell(
      onTap: () {
        // TODO: 跳转到用户详情/编辑页面
      },
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
        decoration: const BoxDecoration(
          color: Colors.white,
          border: Border(
            bottom: BorderSide(color: Color(0xFFEEEEEE), width: 1),
          ),
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Row(
              children: [
                Text(
                  user.name ?? '',
                  style: const TextStyle(
                    fontWeight: FontWeight.w600,
                    fontSize: 16,
                  ),
                ),
                if (user.isActive == false)
                  Container(
                    margin: const EdgeInsets.only(left: 8),
                    padding: const EdgeInsets.symmetric(
                      horizontal: 6,
                      vertical: 2,
                    ),
                    decoration: BoxDecoration(
                      color: Colors.red[50],
                      borderRadius: BorderRadius.circular(4),
                    ),
                    child: const Text(
                      '已禁用',
                      style: TextStyle(fontSize: 12, color: Colors.red),
                    ),
                  ),
              ],
            ),
            Row(
              children: [
                Text(
                  '账号: ${user.userName ?? ''}',
                  style: const TextStyle(color: Colors.grey),
                ),
                const SizedBox(width: 8),
                const Icon(
                  Icons.chevron_right,
                  color: Color(0xFFf4835a),
                  size: 20,
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
