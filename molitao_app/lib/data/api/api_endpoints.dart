class ApiEndpoints {
  // TokenAuth
  static const String authenticate = '/api/TokenAuth/Authenticate';
  static const String weixinMiniAuthenticate =
      '/api/TokenAuth/WeixinMiniAuthenticate';
  static const String authenticateWeixinApp =
      '/api/TokenAuth/AuthenticateWeixinApp';
  static const String weixinMiniPhoneAuthenticate =
      '/api/TokenAuth/WeixinMiniPhoneAuthenticate';
  static const String logout = '/api/TokenAuth/Logout';
  static const String pubQrLogin = '/api/TokenAuth/PubQrLogin';
  static const String qrToken = '/api/TokenAuth/QrToken';

  // User
  static const String getUser = '/api/services/app/User/Get';
  static const String updateUser = '/api/services/app/User/Update';
  static const String getAllUsers = '/api/services/app/User/GetAll';

  // Account
  static const String canUsePasswordLogin =
      '/api/services/app/Account/CanUsePasswordLogin';
  static const String enablePasswordLogin =
      '/api/services/app/Account/EnablePasswordLogin';
  static const String changePassword =
      '/api/services/app/Account/ChangePassword';
  static const String disablePasswordLogin =
      '/api/services/app/Account/DisablePasswordLogin';

  // Session
  static const String getCurrentLoginInformation =
      '/api/services/app/Session/GetCurrentLoginInformations';

  // WebSocket
  static const String sendChannelMsg = '/ws/SendChannelMsg';
  static const String sendMsg = '/ws/send-msg';
  static const String preConnect = '/ws/pre-connect';
  static const String offline = '/ws/offline';
  static const String getChannels = '/ws/get-channels';
  static const String backout = '/ws/backout';
  static const String leaveChannel = '/ws/leave-channel';
  static const String subChannel = '/ws/sub-channel';
  static const String delChannel = '/ws/del-channel';
  static const String banUser = '/ws/ban-user';

  // AuctionItem
  static const String getPublicAuctionList =
      '/api/services/app/AuctionItem/GetPublicList';
  static const String startAuction =
      '/api/services/app/AuctionItem/StartAuction';
  static const String endAuction = '/api/services/app/AuctionItem/EndAuction';
  static const String bid = '/api/services/app/AuctionItem/Bid';
  static const String getMySuccessList =
      '/api/services/app/AuctionItem/GetMySuccessList';
  static const String subStartNotify =
      '/api/services/app/AuctionItem/SubStartNotify';
  static const String getAuctionMidList =
      '/api/services/app/AuctionItem/GetAuctionMidList';
  static const String getKasecStatus =
      '/api/services/app/AuctionItem/GetKasecStatus';
  static const String getAuctionDetail = '/api/AuctionItem/GetDetail';

  // Message
  static const String getPrivateHistory =
      '/api/services/app/Message/getPrivateHistory';
  static const String getChanHistory =
      '/api/services/app/Message/getChanHistory';
  static const String getChanLastId = '/api/services/app/Message/getChanLastId';
  static const String getPrivateLastId =
      '/api/services/app/Message/getPrivateLastId';

  // Post
  static const String getPostList = '/api/Post/GetList';
  static const String getLatestBulletin = '/api/PostBulletin/GetLatestBulletin';
  static const String getCategoryList = '/api/PostCategory/GetCategoryList';
  static const String getPostDetail = '/api/Post/PostDetail/';
  static const String deletePost = '/api/Post/Delete/';
  static const String addPost = '/api/Post/Add';
  static const String editPost = '/api/Post/Edit';

  // UserFriend
  static const String addFriend = '/api/services/app/UserFriend/AddFriend';
  static const String getUserFriends =
      '/api/services/app/UserFriend/GetUserFriends';
  static const String agreeFriend = '/api/services/app/UserFriend/Agree';
  static const String getUserFriendCount =
      '/api/services/app/UserFriend/GetUserFriendCount';

  // Announce
  static const String getLatestAnnounce =
      '/api/services/app/Announce/GetLatest';
  static const String getAllPublicAnnounce =
      '/api/services/app/Announce/GetAllPublic';

  // ChatEmoji
  static const String getAllChatEmoji = '/api/services/app/ChatEmoji/GetAll';
  static const String deleteChatEmoji = '/api/services/app/ChatEmoji/Delete';
  static const String createChatEmoji = '/api/services/app/ChatEmoji/Create';

  // Client
  static const String payDeposit = '/api/services/app/Client/PayDeposit';
  static const String topUp = '/api/services/app/Client/TopUp';
  static const String getMyCount = '/api/services/app/Client/GetMyCount';
  static const String getChatList = '/api/services/app/Client/GetChatList';
  static const String deleteChatList =
      '/api/services/app/Client/DeleteChatList';
  static const String payWithdrawal = '/api/services/app/Client/PayWithdrawal';

  // UserBalanceLog
  static const String getUserBalanceLog =
      '/api/services/app/UserBalanceLog/GetMyAll';

  // UserDepositLog
  static const String getUserDepositLog =
      '/api/services/app/UserDepositLog/GetMyAll';

  // WeChat
  static const String getWechatPhone = '/api/app/weixin/getPhone';

  // AppRelease
  static const String checkUpdate = '/api/services/app/AppRelease/CheckUpdate';

  // Upload
  static const String getUploadSignature =
      '/api/services/app/Upload/GetSignature';

  // HotWords
  static const String getHotWordsList = '/api/HotWords/GetList';

  // AdvertisingSpace
  static const String getAdvertisingSpaceTypeList =
      '/api/AdvertisingSpace/GetTypeList/';

  // GroupChatLevelSettings
  static const String getGroupChatLevelSettingsList =
      '/api/GroupChatLevelSettings/GetList';
  static const String getUserLevelInfo =
      '/api/GroupChatLevelSettings/GetUserLevelInfo/';
  static const String getUserGroupLevel =
      '/api/GroupChatLevelSettings/GetUserGroupLevel/';

  // CmsArticle
  static const String getAllPublicCmsArticles =
      '/api/services/app/CmsArticle/GetAllPublic';

  // ContentSecurity
  static const String checkMedia = '/api/ContentSecurity/CheckMedia';
}
