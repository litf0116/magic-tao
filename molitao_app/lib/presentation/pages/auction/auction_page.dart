import 'package:flutter/material.dart';

class AuctionPage extends StatelessWidget {
  const AuctionPage({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('拍卖')),
      body: const Center(child: Text('拍卖页面')),
    );
  }
}
