import "package:cached_network_image/cached_network_image.dart";
import "package:flutter/material.dart";
import "package:molitao_app/data/repositories/blocked_user_repository.dart";

class BlockListPage extends StatefulWidget {
  const BlockListPage({super.key});

  @override
  State<BlockListPage> createState() => _BlockListPageState();
}

class _BlockListPageState extends State<BlockListPage> {
  final BlockedUserRepository _repository = BlockedUserRepository();
  List<Map<String, dynamic>> _blockedUsers = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _loadBlockedList();
  }

  Future<void> _loadBlockedList() async {
    try {
      final result = await _repository.getBlockedList();
      if (mounted) {
        setState(() {
          _blockedUsers = result
              .map((e) => {
                    "recordId": e.id,
                    "username": e.blockedUserName ?? "用户${e.blockedUserId}",
                    "avatar": e.blockedUserAvatar != null
                        ? (e.blockedUserAvatar!.startsWith('http')
                            ? e.blockedUserAvatar
                            : 'https://image.molitao.top/${e.blockedUserAvatar}')
                        : null,
                  })
              .toList();
          _loading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _loading = false;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text("加载失败: $e")),
        );
      }
    }
  }

  Future<void> _unblockUser(Map<String, dynamic> item) async {
    try {
      await _repository.unblockUser(item["recordId"] as int);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("已解除拉黑")),
        );
        await _loadBlockedList();
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text("操作失败: $e")),
        );
      }
    }
  }

  @override
  Widget build(final BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text(
          "黑名单管理",
          style: TextStyle(fontSize: 20, color: Colors.white),
        ),
        backgroundColor: const Color(0xfff4835a),
        foregroundColor: Colors.white,
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _blockedUsers.isEmpty
              ? const Center(
                  child: Text(
                    "暂无黑名单",
                    style: TextStyle(color: Colors.grey),
                  ),
                )
              : ListView.builder(
                  itemCount: _blockedUsers.length,
                  itemBuilder: (context, index) {
                    final item = _blockedUsers[index];
                    return ListTile(
                      leading: item["avatar"] != null
                          ? CircleAvatar(
                              backgroundColor: Colors.grey[300],
                              child: ClipOval(
                                child: CachedNetworkImage(
                                  imageUrl: item["avatar"] as String,
                                  width: 40,
                                  height: 40,
                                  fit: BoxFit.cover,
                                  placeholder: (context, url) => const Icon(
                                    Icons.person,
                                    color: Colors.grey,
                                  ),
                                  errorWidget: (context, url, error) =>
                                      const Icon(
                                    Icons.person,
                                    color: Colors.grey,
                                  ),
                                ),
                              ),
                            )
                          : CircleAvatar(
                              backgroundColor: Colors.grey[300],
                              child: const Icon(
                                Icons.person,
                                color: Colors.grey,
                              ),
                            ),
                      title: Text(item["username"] as String),
                      trailing: TextButton(
                        onPressed: () => _unblockUser(item),
                        child: const Text("解除拉黑"),
                      ),
                    );
                  },
                ),
    );
  }
}
