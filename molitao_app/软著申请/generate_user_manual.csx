#r "nuget: DocumentFormat.OpenXml, 3.2.0"

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;

var outputPath = "/Users/mac/workspace/magic-tao/molitao_app/软著申请/用户手册.docx";
var screenshotDir = "/Users/mac/workspace/magic-tao/molitao_app/软著申请/截图";

// 确保输出目录存在
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

// 创建文档
using var doc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);
var mainPart = doc.AddMainDocumentPart();
mainPart.Document = new Document(new Body());

var body = mainPart.Document.Body!;

// 设置页面属性 (A4, 边距)
var sectPr = new SectionProperties(
    new PageSize { Width = 11906, Height = 16838 },
    new PageMargin { Top = 1440, Right = 1440, Bottom = 1440, Left = 1440, Header = 720, Footer = 720 }
);

// 创建样式
var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
stylesPart.Styles = new Styles();
stylesPart.Styles.Save();

// 添加页眉页脚
var headerPart = mainPart.AddNewPart<HeaderPart>();
headerPart.Header = new Header(
    new Paragraph(
        new ParagraphProperties(
            new Justification { Val = JustificationValues.Right },
            new SpacingBetweenLines { After = "0" }
        ),
        new Run(
            new RunProperties(
                new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" },
                new FontSize { Val = "18" }
            ),
            new Text("魔力淘 V1.0.0")
        )
    )
);
headerPart.Header.Save();

var footerPart = mainPart.AddNewPart<FooterPart>();
footerPart.Footer = new Footer(
    new Paragraph(
        new ParagraphProperties(
            new Justification { Val = JustificationValues.Center }
        ),
        new Run(
            new RunProperties(
                new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" },
                new FontSize { Val = "18" }
            ),
            new Text("黑龙江省魔淡网络科技有限公司")
        )
    )
);
footerPart.Footer.Save();

// 创建标题样式函数
void AddHeading(Body b, string text, int level) {
    var size = level == 1 ? "36" : level == 2 ? "28" : "24";
    var spacing = level == 1 ? new SpacingBetweenLines { Before = "400", After = "200" } : new SpacingBetweenLines { Before = "300", After = "150" };
    var p = new Paragraph(
        new ParagraphProperties(
            new ParagraphStyleId { Val = $"Heading{level}" },
            spacing,
            new KeepNext()
        ),
        new Run(
            new RunProperties(
                new RunFonts { Ascii = "SimHei", HighAnsi = "SimHei", EastAsia = "SimHei" },
                new Bold(),
                new FontSize { Val = size },
                new Color { Val = "f4835a" }
            ),
            new Text(text)
        )
    );
    b.Append(p);
}

// 创建正文段落函数
void AddParagraph(Body b, string text, bool indent = true) {
    var pPr = new ParagraphProperties(
        new SpacingBetweenLines { After = "160", Line = "276", LineRule = LineSpacingRuleValues.Auto }
    );
    if (indent) pPr.Append(new Indentation { FirstLine = "480" }); // 首行缩进2字符

    var rPr = new RunProperties(
        new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" },
        new FontSize { Val = "21" }
    );

    var p = new Paragraph(pPr, new Run(rPr, new Text(text)));
    b.Append(p);
}

// 创建图片函数
void AddImage(Body b, string imagePath, string caption) {
    if (!File.Exists(imagePath)) {
        // 添加占位符
        var placeholder = new Paragraph(
            new ParagraphProperties(
                new Justification { Val = JustificationValues.Center },
                new SpacingBetweenLines { Before = "200", After = "100" }
            ),
            new Run(
                new RunProperties(
                    new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" },
                    new FontSize { Val = "21" },
                    new Color { Val = "999999" }
                ),
                new Text($"[图片: {Path.GetFileName(imagePath)} - 文件不存在]")
            )
        );
        b.Append(placeholder);

        placeholder = new Paragraph(
            new ParagraphProperties(
                new Justification { Val = JustificationValues.Center },
                new SpacingBetweenLines { After = "200" }
            ),
            new Run(
                new RunProperties(
                    new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" },
                    new FontSize { Val = "18" },
                    new Italic(),
                    new Color { Val = "999999" }
                ),
                new Text(caption)
            )
        );
        b.Append(placeholder);
        return;
    }

    // 添加图片
    var imagePart = mainPart.AddImagePart(ImagePartType.Png);
    using (var fs = new FileStream(imagePath, FileMode.Open)) {
        imagePart.feedData(fs);
    }

    var imageId = mainPart.GetIdOfPart(imagePart);
    var imageWidth = 4500000; // 约5英寸
    var imageHeight = 3000000; // 按比例

    var pict = new Picture(
        new ShapeProperties(
            new Transform2D(
                new Offset { X = 0, Y = 0 },
                new Extents { Cx = imageWidth, Cy = imageHeight }
            ),
            new PresetGeometry(new AdjustValueList()) { Preset = ShapeTypeValues.Rectangle }
        ),
        new ImageData { Id = imageId }
    );

    var drawing = new Drawing(pict);

    var imgPara = new Paragraph(
        new ParagraphProperties(
            new Justification { Val = JustificationValues.Center },
            new SpacingBetweenLines { Before = "200", After = "100" }
        ),
        new Run(drawing)
    );
    b.Append(imgPara);

    // 图片说明
    var capPara = new Paragraph(
        new ParagraphProperties(
            new Justification { Val = JustificationValues.Center },
            new SpacingBetweenLines { After = "300" }
        ),
        new Run(
            new RunProperties(
                new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" },
                new FontSize { Val = "18" },
                new Italic(),
                new Color { Val = "666666" }
            ),
            new Text(caption)
        )
    );
    b.Append(capPara);
}

// 创建无序列表项
void AddListItem(Body b, string text) {
    var p = new Paragraph(
        new ParagraphProperties(
            new SpacingBetweenLines { After = "80", Line = "276", LineRule = LineSpacingRuleValues.Auto },
            new Indentation { Left = "360", Hanging = "360" }
        ),
        new Run(
            new RunProperties(
                new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" },
                new FontSize { Val = "21" }
            ),
            new Text("• " + text)
        )
    );
    b.Append(p);
}

// 创建有序列表项
void AddNumberItem(Body b, string text, int num) {
    var p = new Paragraph(
        new ParagraphProperties(
            new SpacingBetweenLines { After = "80", Line = "276", LineRule = LineSpacingRuleValues.Auto },
            new Indentation { Left = "360", Hanging = "360" }
        ),
        new Run(
            new RunProperties(
                new RunFonts { Ascii = "SimSun", HighAnsi = "SimSun", EastAsia = "SimSun" },
                new FontSize { Val = "21" }
            ),
            new Text($"{num}. {text}")
        )
    );
    b.Append(p);
}

// ========== 封面 ==========
AddHeading(body, "魔力淘用户手册", 1);

var coverInfo = new[] {
    "软件名称：魔力淘",
    "版本号：V1.0.0",
    "著作权人：黑龙江省魔淡网络科技有限公司"
};
foreach (var info in coverInfo) {
    AddParagraph(body, info, false);
}

AddParagraph(body, "", false);
AddParagraph(body, "", false);
AddParagraph(body, "版权所有 © 黑龙江省魔淡网络科技有限公司", false);

// 分页
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));

// ========== 目录 ==========
AddHeading(body, "目  录", 1);

var tocItems = new[] {
    "1. 软件概述",
    "2. 运行环境",
    "3. 安装说明",
    "4. 功能模块说明",
    "    4.1 首页模块",
    "    4.2 认证登录模块",
    "    4.3 交易帖子模块",
    "    4.4 即时通讯模块",
    "    4.5 通讯录模块",
    "    4.6 个人中心模块",
    "    4.7 公告模块",
    "    4.8 设置模块",
    "    4.9 账号安全模块",
    "    4.10 关于模块",
    "5. 操作流程说明",
    "6. 常见问题"
};
foreach (var item in tocItems) {
    AddParagraph(body, item, false);
}

// 分页
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));

// ========== 1. 软件概述 ==========
AddHeading(body, "1. 软件概述", 1);
AddParagraph(body, "魔力淘是一款二手物品交易平台移动应用，为用户提供便捷的二手商品浏览、交易、支付等功能。用户可以通过本应用发布闲置物品、浏览他人发布的商品、参与交易，完成支付等操作。");

AddHeading(body, "主要功能", 2);
AddListItem(body, "商品浏览与搜索");
AddListItem(body, "交易帖子发布与管理");
AddListItem(body, "即时消息通讯");
AddListItem(body, "好友关系管理");
AddListItem(body, "在线支付");
AddListItem(body, "个人信息管理");
AddListItem(body, "系统公告查看");

AddHeading(body, "技术架构", 2);
AddListItem(body, "前端：Flutter 跨平台移动应用框架");
AddListItem(body, "后端：C# .NET 8 + ABP Framework");
AddListItem(body, "数据库：MySQL");
AddListItem(body, "即时通讯：WebSocket");

// ========== 2. 运行环境 ==========
AddHeading(body, "2. 运行环境", 1);

AddHeading(body, "2.1 Android系统要求", 2);
AddListItem(body, "操作系统：Android 8.0 及以上版本");
AddListItem(body, "存储空间：至少100MB可用空间");
AddListItem(body, "网络：需要网络连接（Wi-Fi或移动数据）");

AddHeading(body, "2.2 iOS系统要求", 2);
AddListItem(body, "操作系统：iOS 12.0 及以上版本");
AddListItem(body, "存储空间：至少100MB可用空间");
AddListItem(body, "网络：需要网络连接（Wi-Fi或移动数据）");

// ========== 3. 安装说明 ==========
AddHeading(body, "3. 安装说明", 1);

AddHeading(body, "3.1 Android安装", 2);
AddNumberItem(body, "从应用市场下载安装包", 1);
AddNumberItem(body, "点击安装包进行安装", 2);
AddNumberItem(body, "安装完成后点击图标启动应用", 3);
AddNumberItem(body, "首次启动需要授予网络权限", 4);

AddHeading(body, "3.2 iOS安装", 2);
AddNumberItem(body, "从App Store搜索"魔力淘"", 1);
AddNumberItem(body, "点击"获取"按钮下载安装", 2);
AddNumberItem(body, "安装完成后点击图标启动应用", 3);

// ========== 4. 功能模块说明 ==========
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
AddHeading(body, "4. 功能模块说明", 1);

// 4.1 首页模块
AddHeading(body, "4.1 首页模块", 2);
AddParagraph(body, "首页是用户进入应用后的第一个界面，展示平台品牌形象、功能入口、内容资讯等。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "平台品牌展示与 Banner 广告");
AddListItem(body, "交易站快速入口（跳转交易帖子列表）");
AddListItem(body, "秒杀场快速入口（跳转秒杀聊天页面）");
AddListItem(body, "CMS 文章轮播展示");
AddListItem(body, "广告位网格展示");
AddListItem(body, "APP 版本更新检测与弹窗提示");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "点击 Banner 广告查看推广内容", 1);
AddNumberItem(body, "点击交易站入口浏览商品帖子", 2);
AddNumberItem(body, "点击秒杀场入口参与实时交易", 3);
AddNumberItem(body, "滑动查看轮播文章内容", 4);
AddNumberItem(body, "系统自动检测版本更新", 5);

AddImage(body, $"{screenshotDir}/01_首页.png", "图 4-1 首页界面");

// 4.2 认证登录模块
AddHeading(body, "4.2 认证登录模块", 2);
AddParagraph(body, "认证登录模块提供多种登录方式，支持账号密码、短信验证码、微信一键登录、二维码登录等。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "用户名密码输入框");
AddListItem(body, "手机号码输入框");
AddListItem(body, "验证码输入框");
AddListItem(body, "微信登录按钮");
AddListItem(body, "记住账号选项");
AddListItem(body, "二维码扫码登录入口");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "账号密码登录：输入用户名和密码，点击登录按钮", 1);
AddNumberItem(body, "短信验证码登录：输入手机号，点击获取验证码，输入收到的验证码登录", 2);
AddNumberItem(body, "微信一键登录：点击微信图标，授权后自动登录", 3);
AddNumberItem(body, "二维码登录：点击扫码图标，用微信扫描二维码确认登录", 4);
AddNumberItem(body, "登录成功后自动跳转目标页面", 5);

AddImage(body, $"{screenshotDir}/02_登录页.png", "图 4-2 登录界面");

// 4.3 交易帖子模块
AddHeading(body, "4.3 交易帖子模块", 2);
AddHeading(body, "4.3.1 交易站（帖子列表）", 3);
AddParagraph(body, "交易站是平台的核心功能区，用户可浏览、搜索、筛选各类交易帖子。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "分类标签筛选（交易/求购/问答/分享/其他）");
AddListItem(body, "搜索框 + 热搜词快捷搜索");
AddListItem(body, "置顶帖子区域展示");
AddListItem(body, "帖子卡片列表");
AddListItem(body, "发布帖子按钮");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "点击分类标签筛选帖子类型", 1);
AddNumberItem(body, "在搜索框输入关键词搜索", 2);
AddNumberItem(body, "点击热搜词快速搜索热门内容", 3);
AddNumberItem(body, "下拉刷新获取最新帖子", 4);
AddNumberItem(body, "上拉加载更多帖子", 5);
AddNumberItem(body, "点击发布按钮创建新帖子", 6);

AddImage(body, $"{screenshotDir}/04_交易站.png", "图 4-3 交易站界面");

AddHeading(body, "4.3.2 帖子详情页", 3);
AddParagraph(body, "帖子详情页展示用户发布的交易帖子完整信息，包括帖子内容、发布者信息、联系方式、分类标签等。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "帖子标题");
AddListItem(body, "发布者信息（头像、昵称、发布时间）");
AddListItem(body, "联系方式（微信号、QQ号）");
AddListItem(body, "分类标签（彩色标签展示，如交易/求购/问答/分享/其他）");
AddListItem(body, "帖子内容（支持富文本，包含文字和图片）");
AddListItem(body, "点击留言按钮");
AddListItem(body, "修改/删除菜单（仅作者可见）");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "浏览帖子标题、内容和图片", 1);
AddNumberItem(body, "查看发布者信息和联系方式", 2);
AddNumberItem(body, "点击分类标签查看同类型帖子", 3);
AddNumberItem(body, "点击内容中的图片可放大全屏预览", 4);
AddNumberItem(body, "点击"点击留言"按钮跳转至私聊页面", 5);
AddNumberItem(body, "作者点击右上角菜单可修改或删除帖子", 6);

AddImage(body, $"{screenshotDir}/10_商品详情.png", "图 4-4 帖子详情界面");

AddHeading(body, "4.3.3 发布帖子页", 3);
AddParagraph(body, "发布帖子页用于创建新的交易帖子，用户可以填写标题、选择分类、编辑内容、添加联系方式后发布。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "标题输入框（最多100字）");
AddListItem(body, "分类选择器（交易/求购/问答/分享/其他，单选）");
AddListItem(body, "内容编辑器（支持富文本，最多5000字）");
AddListItem(body, "插入图片按钮");
AddListItem(body, "微信输入框");
AddListItem(body, "QQ号输入框");
AddListItem(body, "发布按钮");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "输入帖子标题（必填）", 1);
AddNumberItem(body, "选择帖子分类标签", 2);
AddNumberItem(body, "编辑帖子内容，可输入文字描述", 3);
AddNumberItem(body, "点击图片图标从相册选择图片插入内容", 4);
AddNumberItem(body, "填写微信号和QQ号（可选）", 5);
AddNumberItem(body, "点击发布按钮提交帖子", 6);
AddNumberItem(body, "发布成功后自动返回上一页", 7);

AddImage(body, $"{screenshotDir}/12_发布帖子.png", "图 4-5 发布帖子界面");

// 4.4 即时通讯模块
AddHeading(body, "4.4 即时通讯模块", 2);
AddHeading(body, "4.4.1 会话列表", 3);
AddParagraph(body, "会话列表展示所有聊天会话，包括私聊、群聊、特殊频道等。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "会话列表（头像、名称、最新消息、时间）");
AddListItem(body, "未读消息红点提示");
AddListItem(body, "特殊频道入口（系统公告、新手群、秒杀场）");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "点击会话进入聊天界面", 1);
AddNumberItem(body, "查看未读消息数量", 2);
AddNumberItem(body, "长按会话可删除聊天记录", 3);
AddNumberItem(body, "点击特殊频道进入群聊", 4);

AddImage(body, $"{screenshotDir}/03_会话列表.png", "图 4-6 会话列表界面");

AddHeading(body, "4.4.2 私聊界面", 3);
AddParagraph(body, "私聊界面提供一对一实时聊天功能，支持文字、图片、表情等消息类型。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "聊天消息气泡");
AddListItem(body, "消息输入框");
AddListItem(body, "表情选择器");
AddListItem(body, "图片发送按钮");
AddListItem(body, "对方头像和昵称");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "输入文字消息发送", 1);
AddNumberItem(body, "点击图片按钮发送图片", 2);
AddNumberItem(body, "点击表情选择器发送表情", 3);
AddNumberItem(body, "上拉加载历史消息", 4);
AddNumberItem(body, "点击对方头像查看资料", 5);

AddImage(body, $"{screenshotDir}/13_私聊界面.png", "图 4-7 私聊界面");

AddHeading(body, "4.4.3 秒杀场", 3);
AddParagraph(body, "秒杀场是平台的限时特惠活动专区，用户可以参与秒杀商品的超值优惠活动。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "限时秒杀商品展示");
AddListItem(body, "当前秒杀状态（进行中/已结束）");
AddListItem(body, "出价/下单区域");
AddListItem(body, "秒杀动态实时滚动");
AddListItem(body, "公告通知栏");
AddListItem(body, "商品详情弹窗");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "查看当前秒杀商品信息和价格", 1);
AddNumberItem(body, "点击"立即秒杀"参与活动", 2);
AddNumberItem(body, "查看实时出价/下单动态", 3);
AddNumberItem(body, "点击商品查看详情", 4);
AddNumberItem(body, "秒杀成功跳转支付页面", 5);

AddImage(body, $"{screenshotDir}/09_秒杀场.png", "图 4-8 秒杀场界面");

// 4.5 通讯录模块
AddHeading(body, "4.5 通讯录模块", 2);
AddParagraph(body, "通讯录模块管理好友关系，支持添加好友、处理好友申请等操作。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "好友搜索框");
AddListItem(body, "好友申请列表");
AddListItem(body, "好友列表");
AddListItem(body, "好友状态标识");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "搜索框输入昵称查找好友", 1);
AddNumberItem(body, "查看好友申请列表", 2);
AddNumberItem(body, "同意或拒绝好友申请", 3);
AddNumberItem(body, "点击好友发起私聊", 4);

AddImage(body, $"{screenshotDir}/05_通讯录.png", "图 4-9 通讯录界面");

// 4.6 个人中心模块
AddHeading(body, "4.6 个人中心模块", 2);
AddHeading(body, "4.6.1 个人中心主页", 3);
AddParagraph(body, "个人中心是用户管理个人信息、资产、订单的核心页面，包含工作台、买家功能区、卖家功能区等入口。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "用户信息卡片（头像、昵称）");
AddListItem(body, "统计数据（好友数、魔力值余额）");
AddListItem(body, "工作台（魔力值增加/减少）");
AddListItem(body, "买家功能区（出价中秒杀、待收货、已成交）");
AddListItem(body, "卖家功能区（我要卖、待发货、订单）");
AddListItem(body, "退出登录按钮");
AddListItem(body, "APP 版本号");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "点击头像跳转至个人信息编辑页面", 1);
AddNumberItem(body, "点击右上角设置图标跳转至设置页面", 2);
AddNumberItem(body, "点击工作台功能管理魔力值（充值/提现）", 3);
AddNumberItem(body, "点击"已成交"查看成交记录", 4);
AddNumberItem(body, "点击"退出登录"安全退出账号", 5);

AddImage(body, $"{screenshotDir}/06_个人中心.png", "图 4-10 个人中心界面");

AddHeading(body, "4.6.2 个人信息编辑", 3);
AddParagraph(body, "个人信息编辑页面用于修改用户头像、昵称、联系方式等信息。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "头像上传/更换区域");
AddListItem(body, "昵称输入框");
AddListItem(body, "QQ 号输入框");
AddListItem(body, "微信号输入框");
AddListItem(body, "保存按钮");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "点击头像从相册选择图片上传", 1);
AddNumberItem(body, "修改昵称", 2);
AddNumberItem(body, "填写或修改 QQ 号", 3);
AddNumberItem(body, "填写或修改微信号", 4);
AddNumberItem(body, "点击保存按钮提交修改", 5);

AddImage(body, $"{screenshotDir}/07_个人信息修改.png", "图 4-11 个人信息修改界面");

AddHeading(body, "4.6.3 魔力值充值", 3);
AddParagraph(body, "魔力值充值页面用于将账户余额转换为魔力值，充值后可用于参与秒杀活动。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "当前魔力值展示");
AddListItem(body, "充值金额输入");
AddListItem(body, "充值说明");
AddListItem(body, "确认充值按钮");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "查看当前魔力值余额", 1);
AddNumberItem(body, "输入充值金额", 2);
AddNumberItem(body, "点击确认充值跳转微信支付", 3);
AddNumberItem(body, "支付成功后魔力值自动增加", 4);

AddImage(body, $"{screenshotDir}/14_魔力值充值.png", "图 4-12 魔力值充值界面");

AddHeading(body, "4.6.4 魔力值提现", 3);
AddParagraph(body, "魔力值提现页面用于将魔力值转换为账户余额，可提现到绑定账户。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "当前魔力值展示");
AddListItem(body, "可提现金额展示");
AddListItem(body, "提现金额输入");
AddListItem(body, "确认提现按钮");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "查看当前魔力值和可提现金额", 1);
AddNumberItem(body, "输入提现金额", 2);
AddNumberItem(body, "点击确认提现申请", 3);
AddNumberItem(body, "等待审核后金额转入账户余额", 4);

AddImage(body, $"{screenshotDir}/15_魔力值提现.png", "图 4-13 魔力值提现界面");

AddHeading(body, "4.6.5 已成交", 3);
AddParagraph(body, "已成交页面展示用户参与的交易成交记录，包括成交时间、商品信息和成交状态。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "成交记录列表");
AddListItem(body, "商品图片和名称");
AddListItem(body, "成交时间");
AddListItem(body, "成交状态");
AddListItem(body, "下拉刷新");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "浏览成交记录列表", 1);
AddNumberItem(body, "下拉刷新获取最新记录", 2);
AddNumberItem(body, "点击记录查看详情", 3);

AddImage(body, $"{screenshotDir}/18_已成交商品列表.png", "图 4-14 已成交商品列表界面");

// 4.7 公告模块
AddHeading(body, "4.7 公告模块", 2);
AddHeading(body, "4.7.1 公告列表", 3);
AddParagraph(body, "公告列表展示平台所有公告和通知，支持分类筛选。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "公告列表");
AddListItem(body, "分类筛选（系统公告、活动通知）");
AddListItem(body, "公告图片预览");
AddListItem(body, "发布时间展示");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "浏览公告列表", 1);
AddNumberItem(body, "点击分类筛选公告", 2);
AddNumberItem(body, "点击公告查看详情", 3);
AddNumberItem(body, "下拉刷新获取最新公告", 4);

AddImage(body, $"{screenshotDir}/11_公告列表.png", "图 4-15 公告列表界面");

AddHeading(body, "4.7.2 公告详情", 3);
AddParagraph(body, "公告详情页展示单条公告的完整内容。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "公告标题");
AddListItem(body, "发布时间");
AddListItem(body, "公告正文内容");
AddListItem(body, "相关图片展示");

AddImage(body, $"{screenshotDir}/08_公告展示.png", "图 4-16 公告展示界面");

// 4.8 设置模块
AddHeading(body, "4.8 设置模块", 2);
AddParagraph(body, "设置模块提供应用基础配置功能，包括账户管理、消息通知、通用设置等。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "个人信息入口");
AddListItem(body, "账号安全入口");
AddListItem(body, "修改密码入口");
AddListItem(body, "推送通知开关");
AddListItem(body, "清除缓存按钮和缓存大小显示");
AddListItem(body, "关于我们入口");
AddListItem(body, "用户协议入口");
AddListItem(body, "隐私政策入口");
AddListItem(body, "退出登录按钮");
AddListItem(body, "版本号显示");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "点击"个人信息"跳转至个人信息编辑页面", 1);
AddNumberItem(body, "点击"账号安全"跳转至账号安全页面", 2);
AddNumberItem(body, "点击"修改密码"弹出修改密码对话框", 3);
AddNumberItem(body, "开启/关闭推送通知开关", 4);
AddNumberItem(body, "点击"清除缓存"弹出确认对话框", 5);
AddNumberItem(body, "点击"关于我们"跳转至关于页面", 6);
AddNumberItem(body, "点击"用户协议"弹出用户协议内容", 7);
AddNumberItem(body, "点击"隐私政策"弹出隐私政策内容", 8);
AddNumberItem(body, "点击"退出登录"弹出确认对话框", 9);

// 4.9 账号安全模块
AddHeading(body, "4.9 账号安全模块", 2);
AddParagraph(body, "账号安全模块提供账号安全相关设置，用于管理账号的密码和安全信息。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "个人信息入口");
AddListItem(body, "账号安全入口");
AddListItem(body, "修改密码入口");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "点击"个人信息"跳转至个人信息编辑页面", 1);
AddNumberItem(body, "点击"账号安全"查看账号安全设置", 2);
AddNumberItem(body, "点击"修改密码"弹出修改密码对话框", 3);

// 4.10 关于模块
AddHeading(body, "4.10 关于模块", 2);
AddParagraph(body, "关于模块展示应用版本信息，提供检查更新、用户协议、隐私政策等功能。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "当前版本号显示");
AddListItem(body, "构建编号显示");
AddListItem(body, "检查更新按钮");
AddListItem(body, "关于我们入口");
AddListItem(body, "用户协议入口");
AddListItem(body, "隐私政策入口");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "查看当前应用版本号和构建编号", 1);
AddNumberItem(body, "点击"检查更新"检测是否有新版本", 2);
AddNumberItem(body, "有新版本时弹出更新对话框，可选择"立即更新"或"稍后更新"", 3);
AddNumberItem(body, "点击"关于我们"跳转至关于页面", 4);
AddNumberItem(body, "点击"用户协议"弹出用户协议内容", 5);
AddNumberItem(body, "点击"隐私政策"弹出隐私政策内容", 6);

// 4.11 私聊模块
AddHeading(body, "4.11 私聊模块", 2);
AddParagraph(body, "私聊页面提供一对一实时聊天功能，支持发送文字消息、图片消息和表情。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "聊天消息气泡（发送/接收区分）");
AddListItem(body, "消息输入框");
AddListItem(body, "发送按钮");
AddListItem(body, "表情选择器");
AddListItem(body, "图片发送按钮");
AddListItem(body, "聊天记录列表");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "在消息输入框输入文字", 1);
AddNumberItem(body, "点击发送按钮发送消息", 2);
AddNumberItem(body, "点击表情按钮选择收藏表情发送", 3);
AddNumberItem(body, "点击图片按钮从相册选择图片发送", 4);
AddNumberItem(body, "上拉加载更多历史消息", 5);
AddNumberItem(body, "自动滚动到最新消息", 6);

AddImage(body, $"{screenshotDir}/13_私聊界面.png", "图 4-17 私聊界面");

// 4.12 群聊模块
AddHeading(body, "4.12 群聊模块", 2);
AddParagraph(body, "群聊页面提供群组实时聊天功能，支持文字消息、图片消息和表情，与群成员进行群体交流。");

AddHeading(body, "界面元素", 3);
AddListItem(body, "聊天消息气泡（发送/接收区分，显示发送者昵称）");
AddListItem(body, "消息输入框");
AddListItem(body, "发送按钮");
AddListItem(body, "表情选择器");
AddListItem(body, "图片发送按钮");
AddListItem(body, "群成员入口按钮");
AddListItem(body, "聊天记录列表");

AddHeading(body, "操作说明", 3);
AddNumberItem(body, "在消息输入框输入文字", 1);
AddNumberItem(body, "点击发送按钮发送群聊消息", 2);
AddNumberItem(body, "点击表情按钮选择收藏表情发送", 3);
AddNumberItem(body, "点击图片按钮从相册选择图片发送", 4);
AddNumberItem(body, "点击群成员按钮查看群成员列表", 5);
AddNumberItem(body, "上拉加载更多历史消息", 6);

AddImage(body, $"{screenshotDir}/16_群聊.png", "图 4-18 群聊界面");

// ========== 5. 操作流程说明 ==========
body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
AddHeading(body, "5. 操作流程说明", 1);

AddHeading(body, "5.1 用户注册登录流程", 2);
AddNumberItem(body, "下载安装：从应用市场下载并安装应用", 1);
AddNumberItem(body, "启动应用：点击图标启动应用", 2);
AddNumberItem(body, "选择登录方式：账号密码/短信验证码/微信登录/二维码登录", 3);
AddNumberItem(body, "完成登录：输入凭证或授权后进入首页", 4);
AddNumberItem(body, "完善信息：首次登录可完善个人资料", 5);

AddHeading(body, "5.2 发布商品流程", 2);
AddNumberItem(body, "进入发布页：点击首页或个人中心发布按钮", 1);
AddNumberItem(body, "填写信息：输入标题、选择分类、编辑内容", 2);
AddNumberItem(body, "上传图片：选择或拍摄商品图片", 3);
AddNumberItem(body, "填写联系方式：输入微信或 QQ 号（可选）", 4);
AddNumberItem(body, "提交发布：点击发布按钮完成发布", 5);

AddHeading(body, "5.3 交易沟通流程", 2);
AddNumberItem(body, "浏览商品：在帖子列表浏览商品", 1);
AddNumberItem(body, "查看详情：点击帖子查看详细信息", 2);
AddNumberItem(body, "联系卖家：点击"联系对方"发起私聊", 3);
AddNumberItem(body, "在线沟通：通过聊天协商交易细节", 4);
AddNumberItem(body, "达成交易：双方确认后完成交易", 5);

AddHeading(body, "5.4 充值提现流程", 2);
AddNumberItem(body, "进入个人中心：点击底部导航个人中心", 1);
AddNumberItem(body, "进入工作台：点击工作台入口", 2);
AddNumberItem(body, "选择操作：点击充值或提现", 3);
AddNumberItem(body, "输入金额：填写充值或提现金额", 4);
AddNumberItem(body, "完成支付：跳转微信完成支付操作", 5);

AddHeading(body, "5.5 账号安全设置流程", 2);
AddNumberItem(body, "进入设置：从个人中心点击设置入口", 1);
AddNumberItem(body, "进入账号安全：点击账号安全选项", 2);
AddNumberItem(body, "选择操作：修改密码/绑定微信/绑定手机", 3);
AddNumberItem(body, "完成验证：根据提示完成安全验证", 4);
AddNumberItem(body, "确认成功：操作完成后显示成功提示", 5);

// ========== 6. 常见问题 ==========
AddHeading(body, "6. 常见问题", 1);

var qas = new[] {
    ("Q1：如何修改个人信息？", "A：进入个人中心，点击头像进入个人信息编辑页面进行修改。"),
    ("Q2：如何联系卖家？", "A：在商品详情页点击"联系对方"按钮，进入聊天界面发送消息。"),
    ("Q3：支付失败怎么办？", "A：请检查网络连接和支付账户余额，重新尝试支付。如问题持续，请联系客服。"),
    ("Q4：如何添加好友？", "A：在通讯录页面搜索用户昵称，找到后点击添加好友按钮发送申请。"),
    ("Q5：如何查看历史订单？", "A：进入个人中心，在买家/卖家功能区可查看对应订单记录。"),
    ("Q6：如何退出登录？", "A：进入个人中心，点击设置，点击退出登录按钮。"),
    ("Q7：忘记密码怎么办？", "A：在登录页面选择短信验证码登录，登录后在账号安全中设置新密码。"),
    ("Q8：如何绑定微信？", "A：进入个人中心 > 设置 > 账号安全，点击绑定微信。"),
    ("Q9：如何充值余额？", "A：进入个人中心 > 工作台，点击充值，选择金额后跳转微信完成支付。"),
    ("Q10：如何查看版本更新？", "A：进入设置 > 关于，点击检查更新，系统会提示是否有新版本。")
};

foreach (var (q, a) in qas) {
    AddParagraph(body, q, false);
    AddParagraph(body, a);
}

AddParagraph(body, "", false);
AddParagraph(body, "魔力淘 V1.0.0 | 黑龙江省魔淡网络科技有限公司", false);

// 添加节属性
body.Append(sectPr);

// 保存文档
mainPart.Document.Save();

Console.WriteLine($"用户手册Word文档已生成: {outputPath}");