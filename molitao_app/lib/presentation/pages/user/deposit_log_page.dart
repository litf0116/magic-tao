import 'package:flutter/material.dart';

class DepositLogPage extends StatelessWidget {
  const DepositLogPage({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('魔力值记录'),
        backgroundColor: const Color(0xfff4835a),
        foregroundColor: Colors.white,
      ),
      body: const Center(child: Text('魔力值记录页面')),
    );
  }
}
