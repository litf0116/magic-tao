import 'package:flutter/material.dart';

class AddPostPage extends StatelessWidget {
  const AddPostPage({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('发布帖子'),
        backgroundColor: const Color(0xfff4835a), // Primary color #f4835a
        foregroundColor: Colors.white,
      ),
      body: const Center(child: Text('发布帖子页面')),
    );
  }
}
