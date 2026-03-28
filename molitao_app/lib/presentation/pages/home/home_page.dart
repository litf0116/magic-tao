import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:cached_network_image/cached_network_image.dart';
import 'package:carousel_slider/carousel_slider.dart';
import 'package:go_router/go_router.dart';
import '../../providers/home_provider.dart';
import '../../../data/models/cms_article_model.dart';
import '../../../data/models/advertising_space_model.dart';

class HomePage extends ConsumerWidget {
  const HomePage({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final homeState = ref.watch(homeProvider);

    // Load data when the page is first built
    ref.read(homeProvider.notifier).loadHomeData();

    return Scaffold(
      backgroundColor: Colors.grey[50],
      body: RefreshIndicator(
        onRefresh: () => ref.read(homeProvider.notifier).refreshHomeData(),
        child: CustomScrollView(
          slivers: [
            // Header with logo
            SliverAppBar(
              backgroundColor: const Color(0xFFf4835a),
              expandedHeight: 160,
              pinned: true,
              flexibleSpace: FlexibleSpaceBar(
                background: Container(
                  decoration: const BoxDecoration(
                    image: DecorationImage(
                      image: CachedNetworkImageProvider(
                        'https://cdn.molitao.top/20250330/04j40l4ynlbh3v3h4bgfe7j2pxiqjg8d.png',
                      ),
                      fit: BoxFit.cover,
                    ),
                  ),
                  child: Center(
                    child: Padding(
                      padding: const EdgeInsets.only(bottom: 20),
                      child: ClipRRect(
                        borderRadius: BorderRadius.circular(10),
                        child: CachedNetworkImage(
                          imageUrl:
                              'https://cdn.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png',
                          width: 231, // 462rpx converted to dp
                          height: 106, // 212rpx converted to dp
                          fit: BoxFit.contain,
                          placeholder: (context, url) => Container(
                            width: 231,
                            height: 106,
                            color: Colors.grey[200],
                            child: const Icon(Icons.image, size: 30),
                          ),
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
                ),
              ),
            ),

            // Main content
            SliverToBoxAdapter(
              child: Container(
                margin: const EdgeInsets.only(top: 12),
                width: MediaQuery.of(context).size.width * 0.9, // 90vw
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    // Trading post and auction banners
                    Column(
                      children: [
                        GestureDetector(
                          onTap: () {
                            // Navigate to trading post
                            context.push('/trading-post');
                          },
                          child: ClipRRect(
                            borderRadius: BorderRadius.circular(8),
                            child: Image.asset(
                              'assets/images/jyz.png',
                              width: double.infinity,
                              height: 135, // 270rpx converted to dp
                              fit: BoxFit.cover,
                            ),
                          ),
                        ),
                        const SizedBox(height: 4),
                        GestureDetector(
                          onTap: () {
                            // Navigate to auction
                            context.push('/auction');
                          },
                          child: ClipRRect(
                            borderRadius: BorderRadius.circular(8),
                            child: Image.asset(
                              'assets/images/pmh.png',
                              width: double.infinity,
                              height: 135, // 270rpx converted to dp
                              fit: BoxFit.cover,
                            ),
                          ),
                        ),
                      ],
                    ),

                    const SizedBox(height: 8),

                    // Article swiper
                    if (homeState.articles.isNotEmpty)
                      Container(
                        width: double.infinity,
                        height: 200,
                        margin: const EdgeInsets.symmetric(vertical: 8),
                        child: CarouselSlider(
                          options: CarouselOptions(
                            height: 200,
                            autoPlay: true,
                            autoPlayInterval: const Duration(seconds: 5),
                            viewportFraction: 1.0,
                            onPageChanged: (index, reason) {},
                          ),
                          items: homeState.articles.map((article) {
                            return Builder(
                              builder: (BuildContext context) {
                                return Container(
                                  width: double.infinity,
                                  decoration: BoxDecoration(
                                    borderRadius: BorderRadius.circular(8),
                                    boxShadow: [
                                      BoxShadow(
                                        color: Colors.black.withOpacity(0.1),
                                        spreadRadius: 1,
                                        blurRadius: 5,
                                        offset: const Offset(0, 2),
                                      ),
                                    ],
                                  ),
                                  child: ClipRRect(
                                    borderRadius: BorderRadius.circular(8),
                                    child: CachedNetworkImage(
                                      imageUrl: article.titleImageUrl ?? '',
                                      fit: BoxFit.cover,
                                      placeholder: (context, url) => Container(
                                        color: Colors.grey[200],
                                        child: const Icon(
                                          Icons.image,
                                          size: 30,
                                        ),
                                      ),
                                      errorWidget: (context, url, error) =>
                                          Container(
                                            color: Colors.grey[200],
                                            child: const Icon(
                                              Icons.broken_image,
                                              size: 30,
                                            ),
                                          ),
                                    ),
                                  ),
                                );
                              },
                            );
                          }).toList(),
                        ),
                      ),

                    // Advertising space grid
                    if (homeState.advertisingSpaces.isNotEmpty)
                      Container(
                        margin: const EdgeInsets.symmetric(vertical: 8),
                        child: Wrap(
                          spacing: 6,
                          runSpacing: 6,
                          children: homeState.advertisingSpaces
                              .asMap()
                              .entries
                              .map((entry) {
                                final index = entry.key;
                                final item = entry.value;
                                return Expanded(
                                  flex: 1,
                                  child: Container(
                                    width:
                                        (MediaQuery.of(context).size.width *
                                                    0.9 -
                                                24) /
                                            2 -
                                        3, // Approximate calculation for 2 columns with spacing
                                    height: 150,
                                    decoration: BoxDecoration(
                                      borderRadius: BorderRadius.circular(8),
                                      boxShadow: [
                                        BoxShadow(
                                          color: Colors.black.withOpacity(0.1),
                                          spreadRadius: 1,
                                          blurRadius: 5,
                                          offset: const Offset(0, 2),
                                        ),
                                      ],
                                    ),
                                    child: Stack(
                                      fit: StackFit.expand,
                                      children: [
                                        ClipRRect(
                                          borderRadius: BorderRadius.circular(
                                            8,
                                          ),
                                          child: CachedNetworkImage(
                                            imageUrl: item.imageUrl ?? '',
                                            fit: BoxFit.cover,
                                            placeholder: (context, url) =>
                                                Container(
                                                  color: Colors.grey[200],
                                                  child: const Icon(
                                                    Icons.image,
                                                    size: 30,
                                                  ),
                                                ),
                                            errorWidget:
                                                (context, url, error) =>
                                                    Container(
                                                      color: Colors.grey[200],
                                                      child: const Icon(
                                                        Icons.broken_image,
                                                        size: 30,
                                                      ),
                                                    ),
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
                                            ),
                                          ),
                                        ),
                                      ],
                                    ),
                                  ),
                                );
                              })
                              .toList(),
                        ),
                      ),

                    if (homeState.advertisingSpaces.isEmpty &&
                        !homeState.isLoading)
                      Container(
                        padding: const EdgeInsets.symmetric(vertical: 16),
                        alignment: Alignment.center,
                        child: const Text(
                          '暂无广告位信息',
                          style: TextStyle(color: Colors.grey, fontSize: 14),
                        ),
                      ),

                    // Loading indicator when data is loading
                    if (homeState.isLoading &&
                        homeState.articles.isEmpty &&
                        homeState.advertisingSpaces.isEmpty)
                      const Padding(
                        padding: EdgeInsets.all(16),
                        child: Center(child: CircularProgressIndicator()),
                      ),

                    // Error message if there's an error
                    if (homeState.errorMessage != null)
                      Container(
                        padding: const EdgeInsets.all(16),
                        child: Text(
                          '加载失败: ${homeState.errorMessage}',
                          style: const TextStyle(
                            color: Colors.red,
                            fontSize: 14,
                          ),
                        ),
                      ),
                  ],
                ),
              ),
            ),

            // Bottom padding to match UniApp
            const SliverToBoxAdapter(child: SizedBox(height: 80)),
          ],
        ),
      ),
    );
  }
}
