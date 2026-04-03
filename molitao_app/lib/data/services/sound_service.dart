import 'package:audioplayers/audioplayers.dart';

class SoundService {
  static final SoundService _instance = SoundService._internal();
  factory SoundService() => _instance;
  SoundService._internal();

  final AudioPlayer _player = AudioPlayer();

  Future<void> playMessageSound() async {
    await _player.play(AssetSource('sounds/cgsys11.mp3'));
  }

  Future<void> playWelcomeSound() async {
    await _player.play(AssetSource('sounds/cgsys17.mp3'));
  }

  Future<void> playAuctionEndSound() async {
    await _player.play(AssetSource('sounds/cgsys17.mp3'));
  }

  void dispose() {
    _player.dispose();
  }
}
