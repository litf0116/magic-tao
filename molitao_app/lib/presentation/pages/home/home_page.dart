import 'dart:async';

import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/utils/image_url_converter.dart';
import '../../../data/models/advertising_space_model.dart';
import '../../../data/models/cms_article_model.dart';
import '../../providers/home_provider.dart';

class HomePage extends ConsumerStatefulWidget {
  const HomePage({Key? key}) : super(key: key);

  @override
  ConsumerState<HomePage> createState() => _HomePageState();
}

class _HomePageState extends ConsumerState<HomePage> {
  @override
  void initState() {
    super.initState();
    // Load data when the page is first initialized
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(homeProvider.notifier).loadHomeData();
    });
  }

  List<Widget> _buildAdRows(List<AdvertisingSpace> spaces) {
    final List<Widget> rows = [];
    for (var i = 0; i < spaces.length; i += 2) {
      final List<Widget> rowChildren = [];

      // First item
      rowChildren.add(_buildAdItem(spaces[i]));

      // Second item (if exists)
      if (i + 1 < spaces.length) {
        rowChildren.add(_buildAdItem(spaces[i + 1]));
      }

      rows.add(
        Padding(
          padding: const EdgeInsets.only(bottom: 6),
          child: Row(children: rowChildren),
        ),
      );
    }
    return rows;
  }

  Widget _buildAdItem(AdvertisingSpace item) {
    return Expanded(
      child: Container(
        height: 150,
        margin: const EdgeInsets.symmetric(horizontal: 3),
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(8),
          boxShadow: [BoxShadow(color: Colors.black.withOpacity(0.1), spreadRadius: 1, blurRadius: 5, offset: const Offset(0, 2))],
        ),
        child: Stack(
          fit: StackFit.expand,
          children: [
            ClipRRect(
              borderRadius: BorderRadius.circular(8),
              child: CachedNetworkImage(
                imageUrl: ImageUrlConverter.convert(item.imageUrl),
                fit: BoxFit.cover,
                placeholder: (context, url) => Container(color: Colors.grey[200], child: const Icon(Icons.image, size: 30)),
                errorWidget: (context, url, error) => Container(color: Colors.grey[200], child: const Icon(Icons.broken_image, size: 30)),
              ),
            ),
            Positioned(
              top: 50,
              left: 0,
              right: 0,
              child: Text(
                item.title ?? item.name ?? '',
                textAlign: TextAlign.center,
                style: const TextStyle(
                  color: Colors.white,
                  fontWeight: FontWeight.bold,
                  fontSize: 14,
                  shadows: [Shadow(color: Colors.black54, offset: Offset(1, 1), blurRadius: 2)],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final homeState = ref.watch(homeProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('魔力淘'),
        backgroundColor: const Color(0xFFf4835a),
        foregroundColor: Colors.white,
        centerTitle: true,
        elevation: 0,
      ),
      body: Container(
        decoration: const BoxDecoration(
          image: DecorationImage(
            image: NetworkImage('https://image.molitao.top/20250330/097jzbhb3jq364wcrsyjw61qb0bj9xob.png'),
            fit: BoxFit.cover,
            alignment: Alignment.topCenter,
          ),
        ),
        child: RefreshIndicator(
          onRefresh: () => ref.read(homeProvider.notifier).refreshHomeData(),
          child: SingleChildScrollView(
            child: Column(
              children: [
                // Header with background image (offset upward to match UniApp)
                Stack(
                  children: [
                    // Header 背景图片
                    Container(
                      height: 160,
                      width: double.infinity,
                      decoration: const BoxDecoration(
                        image: DecorationImage(
                          image: NetworkImage('https://image.molitao.top/20250330/04j40l4ynlbh3v3h4bgfe7j2pxiqjg8d.png'),
                          fit: BoxFit.cover,
                          alignment: Alignment(0, -0.3), // 向上偏移
                        ),
                      ),
                    ),
                    Positioned(
                      bottom: 10,
                      left: 0,
                      right: 0,
                      child: Center(
                        child: ClipRRect(
                          borderRadius: BorderRadius.circular(10),
                          child: CachedNetworkImage(
                            imageUrl: 'https://image.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png',
                            width: 231,
                            height: 106,
                            fit: BoxFit.contain,
                            placeholder: (context, url) =>
                                Container(width: 231, height: 106, color: Colors.grey[200], child: const Icon(Icons.image, size: 30)),
                            errorWidget: (context, url, error) => Container(
                              width: 231,
                              height: 106,
                              color: Colors.grey[200],
                              child: const Icon(Icons.broken_image, size: 30),
                            ),
                          ),
                        ),
                      ),
                    ),
                  ],
                ),

                // Main content
                Center(
                  child: Container(
                    margin: const EdgeInsets.only(top: 12),
                    width: MediaQuery.of(context).size.width * 0.9,
                    decoration: const BoxDecoration(
                      image: DecorationImage(
                        image: NetworkImage('https://image.molitao.top/molitao/2025-03-30/upload_qxgt8fo3iymdi0heth3rnqipc83rzawn.png'),
                        fit: BoxFit.fill,
                        repeat: ImageRepeat.repeatY,
                      ),
                    ),
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        // Trading post banner
                        GestureDetector(
                          onTap: () => context.go('/trading-post'),
                          child: ClipRRect(
                            borderRadius: BorderRadius.circular(8),
                            child: Image.asset('assets/images/jyz.png', height: 135, fit: BoxFit.cover),
                          ),
                        ),
                        const SizedBox(height: 4),
                        // Auction banner
                        GestureDetector(
                          onTap: () => context.push('/chat/auction'),
                          child: ClipRRect(
                            borderRadius: BorderRadius.circular(8),
                            child: Image.asset('assets/images/pmh.png', height: 135, fit: BoxFit.cover),
                          ),
                        ),
                        const SizedBox(height: 8),

                        // Article swiper
                        if (homeState.articles.isNotEmpty) _ArticleSwiper(articles: homeState.articles),

                        // Advertising space grid
                        if (homeState.advertisingSpaces.isNotEmpty)
                          Container(
                            margin: const EdgeInsets.symmetric(vertical: 8),
                            child: Column(children: _buildAdRows(homeState.advertisingSpaces)),
                          ),

                        if (homeState.advertisingSpaces.isEmpty && !homeState.isLoading)
                          Container(
                            padding: const EdgeInsets.symmetric(vertical: 16),
                            alignment: Alignment.center,
                            child: const Text('暂无广告位信息', style: TextStyle(color: Colors.grey, fontSize: 14)),
                          ),

                        if (homeState.isLoading && homeState.articles.isEmpty && homeState.advertisingSpaces.isEmpty)
                          const Padding(
                            padding: EdgeInsets.all(16),
                            child: Center(child: CircularProgressIndicator()),
                          ),

                        if (homeState.errorMessage != null)
                          Container(
                            padding: const EdgeInsets.all(16),
                            child: Text('加载失败: ${homeState.errorMessage}', style: const TextStyle(color: Colors.red, fontSize: 14)),
                          ),
                      ],
                    ),
                  ),
                ),

                // Bottom padding to match UniApp
                const SizedBox(height: 80),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

// Article swiper widget with auto-scroll
class _ArticleSwiper extends StatefulWidget {
  final List<CmsArticle> articles;

  const _ArticleSwiper({required this.articles});

  @override
  State<_ArticleSwiper> createState() => _ArticleSwiperState();
}

class _ArticleSwiperState extends State<_ArticleSwiper> {
  final PageController _pageController = PageController();
  int _currentPage = 0;
  late Timer _timer;

  @override
  void initState() {
    super.initState();
    _startAutoPlay();
  }

  @override
  void dispose() {
    _timer.cancel();
    _pageController.dispose();
    super.dispose();
  }

  void _startAutoPlay() {
    _timer = Timer.periodic(const Duration(seconds: 5), (timer) {
      if (widget.articles.isNotEmpty) {
        final nextPage = (_currentPage + 1) % widget.articles.length;
        _pageController.animateToPage(nextPage, duration: const Duration(milliseconds: 350), curve: Curves.easeIn);
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      height: 200,
      margin: const EdgeInsets.symmetric(vertical: 8),
      child: Stack(
        alignment: Alignment.bottomCenter,
        children: [
          PageView.builder(
            controller: _pageController,
            onPageChanged: (index) {
              setState(() => _currentPage = index);
            },
            itemCount: widget.articles.length,
            itemBuilder: (context, index) {
              final article = widget.articles[index];
              return Container(
                width: double.infinity,
                margin: const EdgeInsets.symmetric(horizontal: 4),
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(8),
                  boxShadow: [BoxShadow(color: Colors.black.withOpacity(0.1), spreadRadius: 1, blurRadius: 5, offset: const Offset(0, 2))],
                ),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(8),
                  child: CachedNetworkImage(
                    imageUrl: ImageUrlConverter.convert(article.titleImageUrl),
                    fit: BoxFit.cover,
                    placeholder: (context, url) => Container(color: Colors.grey[200], child: const Icon(Icons.image, size: 30)),
                    errorWidget: (context, url, error) =>
                        Container(color: Colors.grey[200], child: const Icon(Icons.broken_image, size: 30)),
                  ),
                ),
              );
            },
          ),
          // Page indicator (line style)
          Positioned(
            bottom: 8,
            child: Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: List.generate(
                widget.articles.length,
                (index) => Container(
                  width: _currentPage == index ? 16 : 6,
                  height: 3,
                  margin: const EdgeInsets.symmetric(horizontal: 2),
                  decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(2),
                    color: _currentPage == index ? Colors.white : Colors.white.withOpacity(0.5),
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
