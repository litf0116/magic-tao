import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../providers/announce_provider.dart';

class AnnounceListPage extends ConsumerWidget {
  final int? categoryId;

  const AnnounceListPage({super.key, this.categoryId});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(announceProvider);

    // 初始加载
    ref.listen<AnnounceState>(announceProvider, (prev, next) {
      if (prev?.announces.isEmpty ?? true) {
        ref
            .read(announceProvider.notifier)
            .loadAnnounces(categoryId: categoryId);
      }
    });

    return Scaffold(
      appBar: AppBar(
        title: const Text('公告'),
        backgroundColor: const Color(0xFFf4835a),
        foregroundColor: Colors.white,
      ),
      body: RefreshIndicator(
        onRefresh: () =>
            ref.read(announceProvider.notifier).refresh(categoryId: categoryId),
        child: state.isLoading && state.announces.isEmpty
            ? const Center(child: CircularProgressIndicator())
            : state.error != null && state.announces.isEmpty
            ? Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Text('加载失败: ${state.error}'),
                    const SizedBox(height: 16),
                    ElevatedButton(
                      onPressed: () => ref
                          .read(announceProvider.notifier)
                          .loadAnnounces(categoryId: categoryId),
                      child: const Text('重试'),
                    ),
                  ],
                ),
              )
            : state.announces.isEmpty
            ? ListView(
                children: const [
                  SizedBox(height: 200),
                  Center(child: Text('暂无公告')),
                ],
              )
            : ListView.builder(
                padding: const EdgeInsets.all(16),
                itemCount: state.announces.length,
                itemBuilder: (context, index) {
                  final announce = state.announces[index];

                  return Container(
                    margin: const EdgeInsets.only(bottom: 16),
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(12),
                      boxShadow: [
                        BoxShadow(
                          color: Colors.black.withOpacity(0.05),
                          blurRadius: 10,
                          offset: const Offset(0, 2),
                        ),
                      ],
                    ),
                    child: Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Expanded(
                          child: Text(
                            announce.content?.replaceAll('\n', '\n') ?? '',
                            style: const TextStyle(
                              fontSize: 14,
                              color: Color(0xFF666666),
                            ),
                          ),
                        ),
                        if (announce.imageUrl != null &&
                            announce.imageUrl!.isNotEmpty)
                          GestureDetector(
                            onTap: () {
                              // TODO: 图片预览
                            },
                            child: Container(
                              width: 80,
                              height: 80,
                              margin: const EdgeInsets.only(left: 8),
                              decoration: BoxDecoration(
                                borderRadius: BorderRadius.circular(8),
                                color: Colors.grey[200],
                              ),
                              child: ClipRRect(
                                borderRadius: BorderRadius.circular(8),
                                child: Image.network(
                                  announce.imageUrl!,
                                  fit: BoxFit.cover,
                                  errorBuilder: (context, error, stackTrace) =>
                                      const Icon(Icons.broken_image),
                                ),
                              ),
                            ),
                          ),
                      ],
                    ),
                  );
                },
              ),
      ),
    );
  }
}
