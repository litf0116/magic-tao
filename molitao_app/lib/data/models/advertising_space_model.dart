import '../../core/utils/image_url_converter.dart';

class AdvertisingSpace {
  final int? id;
  final String? name;
  final String? title;
  final String? imageUrl;
  final String? linkUrl;
  final int? sort;
  final int? type;
  final String? description;

  AdvertisingSpace({
    this.id,
    this.name,
    this.title,
    this.imageUrl,
    this.linkUrl,
    this.sort,
    this.type,
    this.description,
  });

  factory AdvertisingSpace.fromJson(Map<String, dynamic> json) {
    return AdvertisingSpace(
      id: json['id'],
      name: json['name'],
      title: json['title'],
      imageUrl: ImageUrlConverter.convert(json['imageUrl']),
      linkUrl: json['linkUrl'],
      sort: json['sort'],
      type: json['type'],
      description: json['description'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'title': title,
      'imageUrl': imageUrl,
      'linkUrl': linkUrl,
      'sort': sort,
      'type': type,
      'description': description,
    };
  }
}
