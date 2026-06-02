import "package:flutter/material.dart";
import "package:molitao_app/data/repositories/report_repository.dart";

class ReportPage extends StatefulWidget {
  const ReportPage({
    super.key,
    required this.messageId,
    required this.reportedUserId,
    required this.chan,
  });

  final int messageId;
  final int reportedUserId;
  final String chan;

  @override
  State<ReportPage> createState() => _ReportPageState();
}

class _ReportPageState extends State<ReportPage> {
  final _formKey = GlobalKey<FormState>();
  final _evidenceController = TextEditingController();

  String? _selectedReason;
  bool _isLoading = false;

  final List<String> _reasonOptions = [
    "垃圾广告",
    "恶意骚扰",
    "违法内容",
    "其他",
  ];

  @override
  void dispose() {
    _evidenceController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _isLoading = true);

    try {
      final repository = ReportRepository();
      await repository.createReport(
        messageId: widget.messageId,
        reportedUserId: widget.reportedUserId,
        chan: widget.chan,
        reason: _selectedReason!,
        evidence: _evidenceController.text.isNotEmpty
            ? _evidenceController.text
            : null,
      );

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text("举报成功")),
        );
        Navigator.pop(context, true);
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text("举报失败: $e")),
        );
      }
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  @override
  Widget build(final BuildContext context) => Scaffold(
        appBar: AppBar(
          title: const Text(
            "举报消息",
            style: TextStyle(fontSize: 20, color: Colors.white),
          ),
          backgroundColor: const Color(0xfff4835a),
          foregroundColor: Colors.white,
        ),
        body: _isLoading
            ? const Center(child: CircularProgressIndicator())
            : _buildForm(),
      );

  Widget _buildForm() => Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            // 消息上下文提示
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: const Color(0xfff6f6f6),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Text(
                "举报消息: ${widget.chan}",
                style: const TextStyle(
                  fontSize: 14,
                  color: Color(0xff666666),
                ),
              ),
            ),
            const SizedBox(height: 24),

            // 举报原因
            const Text(
              "举报原因",
              style: TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.w500,
              ),
            ),
            const SizedBox(height: 8),
            DropdownButtonFormField<String>(
              value: _selectedReason,
              decoration: const InputDecoration(
                border: OutlineInputBorder(),
                hintText: "请选择举报原因",
              ),
              items: _reasonOptions.map((final reason) {
                return DropdownMenuItem(
                  value: reason,
                  child: Text(reason),
                );
              }).toList(),
              onChanged: (final value) {
                setState(() {
                  _selectedReason = value;
                });
              },
              validator: (final value) {
                if (value == null || value.isEmpty) {
                  return "请选择举报原因";
                }
                return null;
              },
            ),
            const SizedBox(height: 24),

            // 补充说明
            const Text(
              "补充说明（可选）",
              style: TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.w500,
              ),
            ),
            const SizedBox(height: 8),
            TextFormField(
              controller: _evidenceController,
              decoration: const InputDecoration(
                border: OutlineInputBorder(),
                hintText: "请补充相关证据或说明（最多500字）",
              ),
              maxLines: 5,
              maxLength: 500,
            ),
            const SizedBox(height: 32),

            // 提交按钮
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: _isLoading ? null : _submit,
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xfff4835a),
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
                child: const Text(
                  "提交举报",
                  style: TextStyle(fontSize: 16),
                ),
              ),
            ),
          ],
        ),
      );
}