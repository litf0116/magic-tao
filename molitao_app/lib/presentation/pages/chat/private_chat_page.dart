import 'package:flutter/material.dart';

class PrivateChatPage extends StatelessWidget {
  final int chatId;

  const PrivateChatPage({Key? key, required this.chatId}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('私聊')),
      body: const Center(child: Text('私聊页面')),
    );
  }
}
