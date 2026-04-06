import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';
import '../../../data/repositories/post_repository.dart';
import '../../../data/services/upload_service.dart';

/// 发布/编辑帖子页面
class AddPostPage extends ConsumerStatefulWidget {
  final int? postId;

  const AddPostPage({super.key, this.postId});

  @override
  ConsumerState<AddPostPage> createState() => _AddPostPageState();
}

class _AddPostPageState extends ConsumerState<AddPostPage> {
  final _formKey = GlobalKey<FormState>();
  final _titleController = TextEditingController();
  final _contentController = TextEditingController();
  final _wechatController = TextEditingController();
  final _qqController = TextEditingController();
  final ImagePicker _imagePicker = ImagePicker();
  final UploadService _uploadService = UploadService();

  bool _isLoading = false;
  bool _isUploadingImage = false;
  bool _isEditing = false;
  List<String> _selectedCategories = [];

  final List<Map<String, dynamic>> _categories = [
    {'id': 1, 'name': '交易'},
    {'id': 2, 'name': '求购'},
    {'id': 3, 'name': '问答'},
    {'id': 4, 'name': '分享'},
    {'id': 5, 'name': '其他'},
  ];

  @override
  void initState() {
    super.initState();
    if (widget.postId != null) {
      _isEditing = true;
      _loadPostData();
    }
  }

  @override
  void dispose() {
    _titleController.dispose();
    _contentController.dispose();
    _wechatController.dispose();
    _qqController.dispose();
    super.dispose();
  }

  Future<void> _loadPostData() async {
    setState(() => _isLoading = true);

    try {
      final post = await PostRepository().getPostDetail(widget.postId!);
      if (post != null && mounted) {
        _titleController.text = post.title ?? '';
        _contentController.text = post.content ?? '';
        _wechatController.text = post.wechat ?? '';
        _qqController.text = post.qq ?? '';

        if (post.categoryName != null) {
          _selectedCategories = post.categoryName!
              .split(',')
              .where((t) => t.isNotEmpty)
              .toList();
        }
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('加载帖子失败: $e')));
      }
    }

    setState(() => _isLoading = false);
  }

  Future<void> _pickImage() async {
    try {
      final XFile? image = await _imagePicker.pickImage(
        source: ImageSource.gallery,
        maxWidth: 1024,
        maxHeight: 1024,
        imageQuality: 85,
      );

      if (image != null) {
        // 上传图片
        setState(() => _isUploadingImage = true);

        try {
          final imageUrl = await _uploadService.uploadImage(image.path);

          if (imageUrl != null) {
            // 插入图片 HTML 到内容中
            final imageHtml = '<img src="$imageUrl" />';
            _contentController.text += imageHtml;

            if (mounted) {
              ScaffoldMessenger.of(
                context,
              ).showSnackBar(const SnackBar(content: Text('图片上传成功')));
            }
          } else {
            if (mounted) {
              ScaffoldMessenger.of(
                context,
              ).showSnackBar(const SnackBar(content: Text('图片上传失败，请重试')));
            }
          }
        } catch (uploadError) {
          if (mounted) {
            ScaffoldMessenger.of(
              context,
            ).showSnackBar(SnackBar(content: Text('图片上传失败：$uploadError')));
          }
        } finally {
          if (mounted) {
            setState(() => _isUploadingImage = false);
          }
        }
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('选择图片失败：$e')));
      }
    }
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _isLoading = true);

    try {
      final repository = PostRepository();
      final categoryIds = _selectedCategories.isNotEmpty
          ? _categories
                .where((c) => _selectedCategories.contains(c['name']))
                .map((c) => c['id'] as int)
                .toList()
          : null;

      if (_isEditing) {
        await repository.updatePost(
          id: widget.postId!,
          title: _titleController.text,
          content: _contentController.text,
          categoryId: categoryIds?.first,
        );
      } else {
        await repository.createPost(
          title: _titleController.text,
          content: _contentController.text,
          categoryId: categoryIds?.first,
        );
      }

      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text(_isEditing ? '修改成功' : '发布成功')));
        Navigator.pop(context, true);
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text('操作失败: $e')));
      }
    }

    setState(() => _isLoading = false);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(_isEditing ? '修改帖子' : '发布帖子'),
        backgroundColor: const Color(0xFFf4835a),
        foregroundColor: Colors.white,
        actions: [
          TextButton(
            onPressed: _isLoading ? null : _submit,
            child: const Text(
              '发布',
              style: TextStyle(color: Colors.white, fontSize: 16),
            ),
          ),
        ],
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _buildForm(),
    );
  }

  Widget _buildForm() {
    return Form(
      key: _formKey,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          // 标题输入
          TextFormField(
            controller: _titleController,
            decoration: const InputDecoration(
              labelText: '标题',
              hintText: '请输入标题',
              border: OutlineInputBorder(),
            ),
            maxLength: 100,
            validator: (value) {
              if (value == null || value.trim().isEmpty) {
                return '请输入标题';
              }
              return null;
            },
          ),
          const SizedBox(height: 16),

          // 分类选择
          _buildCategorySelector(),
          const SizedBox(height: 16),

          // 内容输入
          TextFormField(
            controller: _contentController,
            decoration: InputDecoration(
              labelText: '内容',
              hintText: '请输入内容',
              border: const OutlineInputBorder(),
              suffixIcon: _isUploadingImage
                  ? const Padding(
                      padding: EdgeInsets.all(8.0),
                      child: SizedBox(
                        width: 20,
                        height: 20,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      ),
                    )
                  : IconButton(
                      icon: const Icon(Icons.image),
                      onPressed: _pickImage,
                      tooltip: '插入图片',
                    ),
            ),
            maxLines: 8,
            maxLength: 5000,
            validator: (value) {
              if (value == null || value.trim().isEmpty) {
                return '请输入内容';
              }
              return null;
            },
          ),
          const SizedBox(height: 16),

          // 联系方式
          _buildContactSection(),
        ],
      ),
    );
  }

  Widget _buildCategorySelector() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          '分类',
          style: TextStyle(fontSize: 16, fontWeight: FontWeight.w500),
        ),
        const SizedBox(height: 8),
        Wrap(
          spacing: 8,
          runSpacing: 8,
          children: _categories.map((category) {
            final isSelected = _selectedCategories.contains(category['name']);
            return FilterChip(
              label: Text(category['name']),
              selected: isSelected,
              onSelected: (selected) {
                setState(() {
                  if (selected) {
                    _selectedCategories.add(category['name']);
                  } else {
                    _selectedCategories.remove(category['name']);
                  }
                });
              },
              selectedColor: const Color(0xFFf4835a).withOpacity(0.2),
              checkmarkColor: const Color(0xFFf4835a),
            );
          }).toList(),
        ),
      ],
    );
  }

  Widget _buildContactSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          '联系方式（可选）',
          style: TextStyle(fontSize: 16, fontWeight: FontWeight.w500),
        ),
        const SizedBox(height: 12),
        Row(
          children: [
            Icon(Icons.wechat, color: Colors.green.shade600, size: 20),
            const SizedBox(width: 8),
            Expanded(
              child: TextFormField(
                controller: _wechatController,
                decoration: const InputDecoration(
                  labelText: '微信号',
                  border: OutlineInputBorder(),
                  contentPadding: EdgeInsets.symmetric(
                    horizontal: 12,
                    vertical: 12,
                  ),
                ),
              ),
            ),
          ],
        ),
        const SizedBox(height: 12),
        Row(
          children: [
            Icon(Icons.chat, color: Colors.blue.shade600, size: 20),
            const SizedBox(width: 8),
            Expanded(
              child: TextFormField(
                controller: _qqController,
                decoration: const InputDecoration(
                  labelText: 'QQ号',
                  border: OutlineInputBorder(),
                  contentPadding: EdgeInsets.symmetric(
                    horizontal: 12,
                    vertical: 12,
                  ),
                ),
                keyboardType: TextInputType.number,
              ),
            ),
          ],
        ),
      ],
    );
  }
}
