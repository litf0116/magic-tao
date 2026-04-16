import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../data/repositories/chat_emoji_repository.dart';
import '../../data/models/chat_emoji_model.dart' as model;

/// 表情 Store - 与 UniApp chatEmojiStore 保持一致
class ChatEmojiState {
  /// 表情图片基础 URL
  final String emojiUrl =
      'https://imgcache.qq.com/open/qcloud/tim/assets/emoji/';

  /// 表情映射 - 与 UniApp emojiMap 一致
  final Map<String, String> emojiMap = const {
    '[NO]': 'emoji_0@2x.png',
    '[OK]': 'emoji_1@2x.png',
    '[下雨]': 'emoji_2@2x.png',
    '[么么哒]': 'emoji_3@2x.png',
    '[乒乓]': 'emoji_4@2x.png',
    '[便便]': 'emoji_5@2x.png',
    '[信封]': 'emoji_6@2x.png',
    '[偷笑]': 'emoji_7@2x.png',
    '[傲慢]': 'emoji_8@2x.png',
    '[再见]': 'emoji_9@2x.png',
    '[冷汗]': 'emoji_10@2x.png',
    '[凋谢]': 'emoji_11@2x.png',
    '[刀]': 'emoji_12@2x.png',
    '[删除]': 'emoji_13@2x.png',
    '[勾引]': 'emoji_14@2x.png',
    '[发呆]': 'emoji_15@2x.png',
    '[发抖]': 'emoji_16@2x.png',
    '[可怜]': 'emoji_17@2x.png',
    '[可爱]': 'emoji_18@2x.png',
    '[右哼哼]': 'emoji_19@2x.png',
    '[右太极]': 'emoji_20@2x.png',
    '[右车头]': 'emoji_21@2x.png',
    '[吐]': 'emoji_22@2x.png',
    '[吓]': 'emoji_23@2x.png',
    '[咒骂]': 'emoji_24@2x.png',
    '[咖啡]': 'emoji_25@2x.png',
    '[啤酒]': 'emoji_26@2x.png',
    '[嘘]': 'emoji_27@2x.png',
    '[回头]': 'emoji_28@2x.png',
    '[困]': 'emoji_29@2x.png',
    '[坏笑]': 'emoji_30@2x.png',
    '[多云]': 'emoji_31@2x.png',
    '[大兵]': 'emoji_32@2x.png',
    '[大哭]': 'emoji_33@2x.png',
    '[太阳]': 'emoji_34@2x.png',
    '[奋斗]': 'emoji_35@2x.png',
    '[奶瓶]': 'emoji_36@2x.png',
    '[委屈]': 'emoji_37@2x.png',
    '[害羞]': 'emoji_38@2x.png',
    '[尴尬]': 'emoji_39@2x.png',
    '[左哼哼]': 'emoji_40@2x.png',
    '[左太极]': 'emoji_41@2x.png',
    '[左车头]': 'emoji_42@2x.png',
    '[差劲]': 'emoji_43@2x.png',
    '[弱]': 'emoji_44@2x.png',
    '[强]': 'emoji_45@2x.png',
    '[彩带]': 'emoji_46@2x.png',
    '[彩球]': 'emoji_47@2x.png',
    '[得意]': 'emoji_48@2x.png',
    '[微笑]': 'emoji_49@2x.png',
    '[心碎了]': 'emoji_50@2x.png',
    '[快哭了]': 'emoji_51@2x.png',
    '[怄火]': 'emoji_52@2x.png',
    '[怒]': 'emoji_53@2x.png',
    '[惊恐]': 'emoji_54@2x.png',
    '[惊讶]': 'emoji_55@2x.png',
    '[憨笑]': 'emoji_56@2x.png',
    '[手枪]': 'emoji_57@2x.png',
    '[打哈欠]': 'emoji_58@2x.png',
    '[抓狂]': 'emoji_59@2x.png',
    '[折磨]': 'emoji_60@2x.png',
    '[抠鼻]': 'emoji_61@2x.png',
    '[抱抱]': 'emoji_62@2x.png',
    '[抱拳]': 'emoji_63@2x.png',
    '[拳头]': 'emoji_64@2x.png',
    '[挥手]': 'emoji_65@2x.png',
    '[握手]': 'emoji_66@2x.png',
    '[撇嘴]': 'emoji_67@2x.png',
    '[擦汗]': 'emoji_68@2x.png',
    '[敲打]': 'emoji_69@2x.png',
    '[晕]': 'emoji_70@2x.png',
    '[月亮]': 'emoji_71@2x.png',
    '[棒棒糖]': 'emoji_72@2x.png',
    '[汽车]': 'emoji_73@2x.png',
    '[沙发]': 'emoji_74@2x.png',
    '[流汗]': 'emoji_75@2x.png',
    '[流泪]': 'emoji_76@2x.png',
    '[激动]': 'emoji_77@2x.png',
    '[灯泡]': 'emoji_78@2x.png',
    '[炸弹]': 'emoji_79@2x.png',
    '[熊猫]': 'emoji_80@2x.png',
    '[爆筋]': 'emoji_81@2x.png',
    '[爱你]': 'emoji_82@2x.png',
    '[爱心]': 'emoji_83@2x.png',
    '[爱情]': 'emoji_84@2x.png',
    '[猪头]': 'emoji_85@2x.png',
    '[猫咪]': 'emoji_86@2x.png',
    '[献吻]': 'emoji_87@2x.png',
    '[玫瑰]': 'emoji_88@2x.png',
    '[瓢虫]': 'emoji_89@2x.png',
    '[疑问]': 'emoji_90@2x.png',
    '[白眼]': 'emoji_91@2x.png',
    '[皮球]': 'emoji_92@2x.png',
    '[睡觉]': 'emoji_93@2x.png',
    '[磕头]': 'emoji_94@2x.png',
    '[示爱]': 'emoji_95@2x.png',
    '[礼品袋]': 'emoji_96@2x.png',
    '[礼物]': 'emoji_97@2x.png',
    '[篮球]': 'emoji_98@2x.png',
    '[米饭]': 'emoji_99@2x.png',
    '[糗大了]': 'emoji_100@2x.png',
    '[红双喜]': 'emoji_101@2x.png',
    '[红灯笼]': 'emoji_102@2x.png',
    '[纸巾]': 'emoji_103@2x.png',
    '[胜利]': 'emoji_104@2x.png',
    '[色]': 'emoji_105@2x.png',
    '[药]': 'emoji_106@2x.png',
    '[菜刀]': 'emoji_107@2x.png',
    '[蛋糕]': 'emoji_108@2x.png',
    '[蜡烛]': 'emoji_109@2x.png',
    '[街舞]': 'emoji_110@2x.png',
    '[衰]': 'emoji_111@2x.png',
    '[西瓜]': 'emoji_112@2x.png',
    '[调皮]': 'emoji_113@2x.png',
    '[象棋]': 'emoji_114@2x.png',
    '[跳绳]': 'emoji_115@2x.png',
    '[跳跳]': 'emoji_116@2x.png',
    '[车厢]': 'emoji_117@2x.png',
    '[转圈]': 'emoji_118@2x.png',
    '[鄙视]': 'emoji_119@2x.png',
    '[酷]': 'emoji_120@2x.png',
    '[钞票]': 'emoji_121@2x.png',
    '[钻戒]': 'emoji_122@2x.png',
    '[闪电]': 'emoji_123@2x.png',
    '[闭嘴]': 'emoji_124@2x.png',
    '[闹钟]': 'emoji_125@2x.png',
    '[阴险]': 'emoji_126@2x.png',
    '[难过]': 'emoji_127@2x.png',
    '[雨伞]': 'emoji_128@2x.png',
    '[青蛙]': 'emoji_129@2x.png',
    '[面条]': 'emoji_130@2x.png',
    '[鞭炮]': 'emoji_131@2x.png',
    '[风车]': 'emoji_132@2x.png',
    '[飞吻]': 'emoji_133@2x.png',
    '[飞机]': 'emoji_134@2x.png',
    '[饥饿]': 'emoji_135@2x.png',
    '[香蕉]': 'emoji_136@2x.png',
    '[骷髅]': 'emoji_137@2x.png',
    '[麦克风]': 'emoji_138@2x.png',
    '[麻将]': 'emoji_139@2x.png',
    '[鼓掌]': 'emoji_140@2x.png',
    '[龇牙]': 'emoji_141@2x.png',
  };

  const ChatEmojiState();

  /// 获取表情图片完整 URL
  String? getEmojiUrl(String code) {
    final fileName = emojiMap[code];
    if (fileName != null) {
      return '$emojiUrl$fileName';
    }
    return null;
  }

  /// 解析文本中的表情代码，返回富文本片段
  List<EmojiTextSegment> parseText(String text) {
    final segments = <EmojiTextSegment>[];
    int currentIndex = 0;

    for (final entry in emojiMap.entries) {
      final code = entry.key;
      int startIndex = text.indexOf(code, currentIndex);

      while (startIndex != -1) {
        // 添加表情前的文本
        if (startIndex > currentIndex) {
          segments.add(
            EmojiTextSegment(
              type: EmojiSegmentType.text,
              text: text.substring(currentIndex, startIndex),
            ),
          );
        }

        // 添加表情
        segments.add(
          EmojiTextSegment(
            type: EmojiSegmentType.emoji,
            text: code,
            emojiUrl: '$emojiUrl${entry.value}',
          ),
        );

        currentIndex = startIndex + code.length;
        startIndex = text.indexOf(code, currentIndex);
      }
    }

    // 添加剩余文本
    if (currentIndex < text.length) {
      segments.add(
        EmojiTextSegment(
          type: EmojiSegmentType.text,
          text: text.substring(currentIndex),
        ),
      );
    }

    return segments.isEmpty
        ? [EmojiTextSegment(type: EmojiSegmentType.text, text: text)]
        : segments;
  }
}

/// 表情文本片段类型
enum EmojiSegmentType { text, emoji }

/// 表情文本片段
class EmojiTextSegment {
  final EmojiSegmentType type;
  final String text;
  final String? emojiUrl;

  const EmojiTextSegment({
    required this.type,
    required this.text,
    this.emojiUrl,
  });
}

/// 表情 Store Provider - 简化版本，不包含收藏功能
final chatEmojiStoreProvider = Provider<ChatEmojiState>((ref) {
  return const ChatEmojiState();
});

/// 收藏表情状态
class UserEmojiState {
  final List<model.ChatEmojiDto> userEmoji;
  final bool isLoading;

  const UserEmojiState({this.userEmoji = const [], this.isLoading = false});

  UserEmojiState copyWith({
    List<model.ChatEmojiDto>? userEmoji,
    bool? isLoading,
  }) {
    return UserEmojiState(
      userEmoji: userEmoji ?? this.userEmoji,
      isLoading: isLoading ?? this.isLoading,
    );
  }
}

/// 收藏表情 Notifier
class UserEmojiNotifier extends StateNotifier<UserEmojiState> {
  final ChatEmojiRepository _repository = ChatEmojiRepository();
  bool _initialized = false;

  UserEmojiNotifier() : super(const UserEmojiState());

  /// 初始化获取收藏表情（首次访问时调用）
  Future<void> ensureInitialized() async {
    if (!_initialized) {
      _initialized = true;
      await fetchUserEmoji();
    }
  }

  /// 获取用户收藏的表情列表
  Future<void> fetchUserEmoji() async {
    state = state.copyWith(isLoading: true);
    try {
      final emojis = await _repository.getAll();
      state = UserEmojiState(userEmoji: emojis, isLoading: false);
    } catch (e) {
      debugPrint('[UserEmojiNotifier] 获取收藏表情失败: $e');
      state = state.copyWith(isLoading: false);
    }
  }

  /// 添加收藏表情
  Future<bool> addToEmoji(String url) async {
    try {
      final result = await _repository.create(url);
      if (result != null) {
        await fetchUserEmoji();
        return true;
      }
      return false;
    } catch (e) {
      debugPrint('[UserEmojiNotifier] 添加收藏表情失败: $e');
      return false;
    }
  }

  /// 删除收藏表情
  Future<bool> removeEmoji(int id) async {
    try {
      final success = await _repository.delete(id);
      if (success) {
        await fetchUserEmoji();
        return true;
      }
      return false;
    } catch (e) {
      debugPrint('[UserEmojiNotifier] 删除收藏表情失败: $e');
      return false;
    }
  }

  /// 重新加载
  Future<void> reload() async {
    await fetchUserEmoji();
  }
}

/// 收藏表情 Provider
final userEmojiProvider =
    StateNotifierProvider<UserEmojiNotifier, UserEmojiState>((ref) {
      return UserEmojiNotifier();
    });
