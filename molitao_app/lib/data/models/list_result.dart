class ListResult<T> {
  final int totalCount;
  final List<T> items;

  const ListResult({required this.totalCount, required this.items});

  factory ListResult.fromJson(
    Map<String, dynamic> json,
    T Function(Map<String, dynamic> json) fromJsonT,
  ) {
    return ListResult(
      totalCount: json['totalCount'] as int? ?? 0,
      items: (json['items'] as List<dynamic>? ?? [])
          .map((e) => fromJsonT(e as Map<String, dynamic>))
          .toList(),
    );
  }

  Map<String, dynamic> toJson(Function(T item) toJsonT) {
    return {
      'totalCount': totalCount,
      'items': items.map((e) => toJsonT(e)).toList(),
    };
  }
}
