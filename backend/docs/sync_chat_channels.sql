-- =====================================================
-- 聊天频道数据同步 SQL
-- 功能：根据 T_Message 表数据同步 T_ChatChannel 表
-- 执行顺序：1 → 2 → 3
-- =====================================================

-- =====================================================
-- 1. 同步系统频道（To IS NULL，Chan 有值）
-- 系统频道如：大厅、拍卖频道
-- =====================================================
INSERT INTO T_ChatChannel
    (ChannelId, ChannelType, ChannelName, LastMessageId, LastMessageContent,
     LastMessageFromName, LastMessageFromAvatar, LastMessageTime, SortOrder, IsActive, CreationTime)
SELECT
    m.Chan AS ChannelId,
    0 AS ChannelType,  -- 0 = System
    CASE m.Chan
        WHEN '-1_auction' THEN '拍卖频道'
        WHEN '0_lobby' THEN '大厅'
        ELSE m.Chan
    END AS ChannelName,
    m.Id AS LastMessageId,
    m.Msg AS LastMessageContent,
    m.FromName AS LastMessageFromName,
    m.Avatar AS LastMessageFromAvatar,
    m.Time AS LastMessageTime,
    CASE m.Chan WHEN '-1_auction' THEN 99 WHEN '0_lobby' THEN 100 ELSE 0 END AS SortOrder,
    1 AS IsActive,
    NOW() AS CreationTime
FROM T_Message m
INNER JOIN (
    SELECT Chan, MAX(Time) AS MaxTime
    FROM T_Message
    WHERE Chan IS NOT NULL
      AND Chan != ''
      AND To IS NULL  -- 系统消息：To 为空
    GROUP BY Chan
) t ON m.Chan = t.Chan AND m.Time = t.MaxTime
ON DUPLICATE KEY UPDATE
    LastMessageId = m.Id,
    LastMessageContent = m.Msg,
    LastMessageFromName = m.FromName,
    LastMessageFromAvatar = m.Avatar,
    LastMessageTime = m.Time;


-- =====================================================
-- 2. 同步私聊频道（To IS NOT NULL，Chan 为 NULL）
-- 根据 From 和 To 字段生成频道
-- =====================================================
INSERT INTO T_ChatChannel
    (ChannelId, ChannelType, User1Id, User2Id, User1Status, User2Status,
     LastMessageId, LastMessageContent, LastMessageFromName, LastMessageFromAvatar,
     LastMessageTime, SortOrder, IsActive, CreationTime)
SELECT
    CONCAT('private_', LEAST(m.From, m.To), '_', GREATEST(m.From, m.To)) AS ChannelId,
    1 AS ChannelType,  -- 1 = Private
    LEAST(m.From, m.To) AS User1Id,
    GREATEST(m.From, m.To) AS User2Id,
    0 AS User1Status,  -- 0 = Normal
    0 AS User2Status,
    m.Id AS LastMessageId,
    m.Msg AS LastMessageContent,
    m.FromName AS LastMessageFromName,
    m.Avatar AS LastMessageFromAvatar,
    m.Time AS LastMessageTime,
    0 AS SortOrder,
    1 AS IsActive,
    NOW() AS CreationTime
FROM T_Message m
INNER JOIN (
    SELECT
        LEAST(`From`, `To`) AS user1,
        GREATEST(`From`, `To`) AS user2,
        MAX(Time) AS MaxTime
    FROM T_Message
    WHERE Chan IS NULL AND `To` IS NOT NULL
    GROUP BY user1, user2
) t ON ((m.From = t.user1 AND m.To = t.user2) OR (m.From = t.user2 AND m.To = t.user1))
    AND m.Time = t.MaxTime
ON DUPLICATE KEY UPDATE
    LastMessageId = m.Id,
    LastMessageContent = m.Msg,
    LastMessageFromName = m.FromName,
    LastMessageFromAvatar = m.Avatar,
    LastMessageTime = m.Time;


-- =====================================================
-- 3. 验证数据同步结果
-- =====================================================

-- 3.1 查看 Message 表数据分布
SELECT
    CASE WHEN To IS NULL THEN '系统频道' ELSE '私聊' END AS MessageType,
    COUNT(*) AS MessageCount,
    COUNT(DISTINCT CASE WHEN To IS NULL THEN Chan END) AS SystemChannelCount,
    COUNT(DISTINCT CASE WHEN To IS NOT NULL THEN CONCAT(LEAST(From, To), '_', GREATEST(From, To)) END) AS PrivateChatCount
FROM T_Message;

-- 3.2 查看已同步的频道数量
SELECT
    ChannelType,
    CASE ChannelType WHEN 0 THEN '系统频道' WHEN 1 THEN '私聊频道' END AS TypeName,
    COUNT(*) AS ChannelCount
FROM T_ChatChannel
WHERE IsActive = 1
GROUP BY ChannelType;

-- 3.3 查看系统频道详情
SELECT
    ChannelId,
    ChannelName,
    LastMessageContent,
    FROM_UNIXTIME(LastMessageTime/1000) AS LastMessageTime
FROM T_ChatChannel
WHERE ChannelType = 0 AND IsActive = 1
ORDER BY LastMessageTime DESC;

-- 3.4 查看最近活跃的私聊频道（Top 20）
SELECT
    ChannelId,
    User1Id,
    User2Id,
    LastMessageContent,
    FROM_UNIXTIME(LastMessageTime/1000) AS LastMessageTime
FROM T_ChatChannel
WHERE ChannelType = 1 AND IsActive = 1
ORDER BY LastMessageTime DESC
LIMIT 20;

-- 3.5 检查是否有频道数据异常
SELECT
    COUNT(*) AS TotalChannels,
    SUM(CASE WHEN LastMessageId IS NULL THEN 1 ELSE 0 END) AS ChannelsWithoutLastMessage
FROM T_ChatChannel
WHERE IsActive = 1;


-- =====================================================
-- 附录：Message 表结构参考
-- =====================================================
-- | 字段   | 系统频道 (Chan 有值) | 私聊 (To 有值) |
-- |--------|---------------------|---------------|
-- | Chan   | 有值 (-1_auction等) | NULL          |
-- | From   | 发送者ID            | 发送者ID      |
-- | To     | NULL                | 接收者ID      |
-- =====================================================
