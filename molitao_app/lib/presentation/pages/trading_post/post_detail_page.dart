import 'package:flutter/material.dart';

class PostDetailPage extends StatelessWidget {
  final int postId;

  const PostDetailPage({Key? key, required this.postId}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('帖子详情'),
        backgroundColor: const Color(0xfff4835a), // Primary color #f4835a
        foregroundColor: Colors.white,
      ),
      body: const Center(child: Text('帖子详情页面 - 帖子ID: ')),
    );
  }
}
