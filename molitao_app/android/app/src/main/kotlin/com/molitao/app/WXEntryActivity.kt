package com.molitao.app

import android.content.Intent
import com.tencent.mm.opensdk.modelbase.BaseReq
import com.tencent.mm.opensdk.modelbase.BaseResp
import com.tencent.mm.opensdk.openapi.IWXAPIEventHandler
import io.flutter.embedding.android.FlutterActivity

class WXEntryActivity : FlutterActivity(), IWXAPIEventHandler {
    
    override fun onNewIntent(intent: Intent) {
        super.onNewIntent(intent)
        setIntent(intent)
    }
    
    override fun onReq(req: BaseReq) {}
    
    override fun onResp(resp: BaseResp) {}
}
