import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import '../../../data/models/auction_item_model.dart';
import '../../../data/repositories/auction_repository.dart';
import '../../../core/utils/image_url_converter.dart';
import '../../../core/widgets/app_bottom_sheet.dart';

class AuctionSuccessListPage extends ConsumerStatefulWidget {
  const AuctionSuccessListPage({super.key});

  @override
  ConsumerState<AuctionSuccessListPage> createState() =>
      _AuctionSuccessListPageState();
}

class _AuctionSuccessListPageState
    extends ConsumerState<AuctionSuccessListPage> {
  final AuctionRepository _repository = AuctionRepository();
  List<AuctionItemDto> _list = [];
  bool _isLoading = false;
  String? _errorMessage;

  @override
  void initState() {
    super.initState();
    _loadSuccessList();
  }

  Future<void> _loadSuccessList() async {
    if (_isLoading) return;

    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    try {
      final result = await _repository.getMySuccessList(
        skipCount: 0,
        maxResultCount: 50,
      );
      if (mounted) {
        setState(() {
          _list = result.items;
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _errorMessage = e.toString();
          _isLoading = false;
        });
      }
    }
  }

  String _formatTime(DateTime? dealTime) {
    if (dealTime == null) return '';
    return DateFormat('MM-dd HH:mm').format(dealTime);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text(
          '已成交',
          style: TextStyle(fontSize: 20, color: Colors.white),
        ),
        backgroundColor: const Color(0xfff4835a),
        foregroundColor: Colors.white,
      ),
      body: RefreshIndicator(
        onRefresh: _loadSuccessList,
        child: Container(
          color: const Color(0xfff6f6f6),
          child: _isLoading && _list.isEmpty
              ? const Center(child: CircularProgressIndicator())
              : _errorMessage != null
              ? Center(
                  child: Text(
                    '加载失败：$_errorMessage',
                    style: const TextStyle(color: Colors.red),
                  ),
                )
              : _list.isEmpty
              ? const Center(
                  child: Text('暂无成交记录', style: TextStyle(color: Colors.grey)),
                )
              : ListView.builder(
                  padding: const EdgeInsets.all(8),
                  itemCount: _list.length,
                  itemBuilder: (context, index) {
                    final item = _list[index];
                    return _buildListItem(item);
                  },
                ),
        ),
      ),
    );
  }

  Widget _buildListItem(AuctionItemDto item) {
    return GestureDetector(
      onTap: () => _showDetail(item),
      child: Container(
        margin: const EdgeInsets.only(bottom: 8),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(8),
          boxShadow: [
            BoxShadow(
              color: Colors.grey.withOpacity(0.2),
              spreadRadius: 1,
              blurRadius: 5,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Row(
          children: [
            ClipRRect(
              borderRadius: const BorderRadius.all(Radius.circular(8)),
              child: Image.network(
                ImageUrlConverter.convert(item.imageUrl ?? ''),
                width: 64,
                height: 64,
                fit: BoxFit.cover,
                errorBuilder: (context, error, stackTrace) {
                  return Container(
                    width: 64,
                    height: 64,
                    color: Colors.grey[300],
                    child: const Icon(Icons.broken_image, color: Colors.grey),
                  );
                },
              ),
            ),
            Expanded(
              child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 8),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      item.name ?? '',
                      style: const TextStyle(
                        fontSize: 14,
                        color: Color(0xff935F4E),
                      ),
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: 4),
                    if (item.finalPrice != null)
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          RichText(
                            text: TextSpan(
                              children: [
                                const TextSpan(
                                  text: '成交价：',
                                  style: TextStyle(
                                    fontSize: 12,
                                    color: Colors.grey,
                                  ),
                                ),
                                TextSpan(
                                  text: '￥${item.finalPrice}',
                                  style: const TextStyle(
                                    fontSize: 14,
                                    fontWeight: FontWeight.bold,
                                    color: Colors.red,
                                  ),
                                ),
                              ],
                            ),
                          ),
                          if (item.dealTime != null)
                            Text(
                              _formatTime(item.dealTime),
                              style: const TextStyle(
                                fontSize: 12,
                                color: Colors.grey,
                              ),
                            ),
                        ],
                      ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _showDetail(AuctionItemDto item) {
    showAppBottomSheet(
      context: context,
      builder: (context) => Container(
        padding: const EdgeInsets.all(16),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              '商品名称：${item.name ?? ''}',
              style: const TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              item.description ?? '',
              style: const TextStyle(fontSize: 14),
            ),
            const SizedBox(height: 16),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: () => Navigator.pop(context),
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xfff4835a),
                  foregroundColor: Colors.white,
                ),
                child: const Text('关闭'),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
