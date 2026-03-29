import 'package:flutter/material.dart';

class UserInfoPage extends StatelessWidget {
  const UserInfoPage({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('个人信息'),
        backgroundColor: const Color(0xfff4835a),
        foregroundColor: Colors.white,
      ),
      body: const Center(child: Text('用户信息页面')),
    );
  }
}
