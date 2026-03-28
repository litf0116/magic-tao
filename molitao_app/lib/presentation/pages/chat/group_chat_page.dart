import 'package:flutter/material.dart';

class GroupChatPage extends StatelessWidget {
  final int chatId;

  const GroupChatPage({Key? key, required this.chatId}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('群聊')),
      body: const Center(child: Text('群聊页面')),
    );
  }
}
