class ChatEmojiDto {
  final String? url;
  final String? payload;
  final int? id;

  const ChatEmojiDto({this.url, this.payload, this.id});

  factory ChatEmojiDto.fromJson(Map<String, dynamic> json) {
    return ChatEmojiDto(
      url: json['url'],
      payload: json['payload'],
      id: json['id'],
    );
  }

  Map<String, dynamic> toJson() {
    return {'url': url, 'payload': payload, 'id': id};
  }
}
