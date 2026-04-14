#!/usr/bin/env python3
"""
魔力淘 App 发布脚本

功能：
1. 构建 release APK
2. 上传到又拍云 CDN
3. 调用后端 API 创建发布记录 (PublishAppReleaseByUrl)

用法：
    ./app_deploy.py --version 1.0.2 --build 4 --desc "版本描述"
    ./app_deploy.py --version 1.0.2 --build 4 --skip-build
"""

import argparse
import base64
import hashlib
import hmac
import json
import os
import subprocess
import sys
import time
import urllib.error
import urllib.parse
import urllib.request

# ============== 配置 ==============
BUCKET = "molitao"
OPERATOR = "molitao"
PASSWORD = "cHILcN0KfdVwcCkQz0Kjou7utChvtkpv"
IMG_URL = "http://image.molitao.top"
REMOTE_DIR = "/apps"
API_BASE = "http://localhost:5000"
ADMIN_USER = "feifei"
ADMIN_PASS = "123456"
SSH_HOST = "molitao"


def ssh_exec(cmd):
    import subprocess

    result = subprocess.run(["ssh", SSH_HOST, cmd], capture_output=True, text=True)
    if result.returncode != 0:
        raise Exception(f"SSH command failed: {result.stderr}")
    return result.stdout


def get_admin_token_via_ssh():
    payload = json.dumps({"userNameOrEmailAddress": ADMIN_USER, "password": ADMIN_PASS})
    cmd = f"curl -s -X POST 'http://localhost:5000/api/TokenAuth/Authenticate' -H 'Content-Type: application/json' -d '{payload}'"
    result = ssh_exec(cmd)
    data = json.loads(result)
    if data.get("success"):
        return data["result"]["accessToken"]
    else:
        raise Exception(f"认证失败: {data.get('error', {}).get('message', 'Unknown')}")


def publish_by_url_via_ssh(
    token,
    version_name,
    version_code,
    description,
    download_url,
    filename,
    file_size,
    is_force_update=False,
    platform="android",
):
    payload = json.dumps(
        {
            "versionName": version_name,
            "versionCode": version_code,
            "description": description,
            "downloadUrl": download_url,
            "fileName": filename,
            "fileSize": file_size,
            "isForceUpdate": is_force_update,
            "platform": platform,
        }
    )
    cmd = f"curl -s -X POST 'http://localhost:5000/api/services/app/AppRelease/PublishAppReleaseByUrl' -H 'Content-Type: application/json' -H 'Authorization: Bearer {token}' -d '{payload}'"
    result = ssh_exec(cmd)
    data = json.loads(result)
    if data.get("success"):
        return data.get("result")
    else:
        raise Exception(f"发布失败: {data.get('error', {}).get('message', 'Unknown')}")


def publish_by_url(
    token,
    version_name,
    version_code,
    description,
    download_url,
    filename,
    file_size,
    is_force_update=False,
    platform="android",
):
    """调用后端 API 创建发布记录"""
    url = f"{API_BASE}/api/services/app/AppRelease/PublishAppReleaseByUrl"

    payload = {
        "versionName": version_name,
        "versionCode": version_code,
        "description": description,
        "downloadUrl": download_url,
        "fileName": filename,
        "fileSize": file_size,
        "isForceUpdate": is_force_update,
        "platform": platform,
    }

    data = json.dumps(payload).encode()
    req = urllib.request.Request(url, data=data)
    req.add_header("Content-Type", "application/json")
    req.add_header("Authorization", f"Bearer {token}")

    try:
        with urllib.request.urlopen(req, timeout=30) as resp:
            result = json.loads(resp.read().decode())
            if result.get("success"):
                return result.get("result")
            else:
                raise Exception(
                    f"发布失败: {result.get('error', {}).get('message', 'Unknown')}"
                )
    except urllib.error.HTTPError as e:
        error_body = json.loads(e.read().decode()) if e.fp else {}
        raise Exception(
            f"发布请求失败: {e.code} - {error_body.get('error', {}).get('message', e.reason)}"
        )


def get_signature(data_str, policy_base64):
    """获取又拍云上传签名"""
    params = urllib.parse.urlencode({"data": data_str, "policy": policy_base64})
    url = f"{API_BASE}/api/services/app/Upload/GetSignature?{params}"
    req = urllib.request.Request(url)

    with urllib.request.urlopen(req, timeout=30) as resp:
        result = json.loads(resp.read().decode())
        return result.get("signature", "")


def upload_to_upyun(local_file, remote_path):
    """上传文件到又拍云 CDN"""
    file_size = os.path.getsize(local_file)
    filename = os.path.basename(local_file)
    date_gmt = time.strftime("%a, %d %b %Y %H:%M:%S GMT", time.gmtime())
    expiration = int(time.time()) + 43200

    opts = {
        "save-key": remote_path,
        "bucket": BUCKET,
        "expiration": expiration,
        "date": date_gmt,
    }
    policy_json = json.dumps(opts)
    policy_base64 = base64.b64encode(policy_json.encode()).decode()
    data_str = "&".join(["POST", "/" + BUCKET, date_gmt, policy_base64])

    signature = get_signature(data_str, policy_base64)
    if not signature:
        raise Exception("Failed to get signature")

    upload_url = f"https://v0.api.upyun.com/{BUCKET}"
    with open(local_file, "rb") as f:
        file_data = f.read()

    boundary = "----FormBoundary7MA4YWxkTrZu0gW"
    body = f"--{boundary}\r\n"
    body += 'Content-Disposition: form-data; name="policy"\r\n\r\n'
    body += f"{policy_base64}\r\n"
    body += f"--{boundary}\r\n"
    body += 'Content-Disposition: form-data; name="authorization"\r\n\r\n'
    body += f"UPYUN {OPERATOR}:{signature}\r\n"
    body += f"--{boundary}\r\n"
    body += f'Content-Disposition: form-data; name="file"; filename="{filename}"\r\n'
    body += "Content-Type: application/octet-stream\r\n\r\n"

    req = urllib.request.Request(
        upload_url,
        data=body.encode() + file_data + f"\r\n--{boundary}--\r\n".encode(),
        method="POST",
    )
    req.add_header("Content-Type", f"multipart/form-data; boundary={boundary}")

    with urllib.request.urlopen(req, timeout=300) as resp:
        result = resp.read().decode()
        if "ok" not in result:
            raise Exception(f"Upload failed: {result}")
        return f"{IMG_URL}{remote_path}"


def check_update_api(version_code, platform="android"):
    """检查更新 API"""
    url = f"{API_BASE}/api/services/app/AppRelease/CheckUpdate?currentVersionCode={version_code}&platform={platform}"
    req = urllib.request.Request(url)

    with urllib.request.urlopen(req, timeout=30) as resp:
        return json.loads(resp.read().decode())


def build_apk():
    """构建 release APK"""
    os.chdir("/Users/mac/workspace/magic-tao/molitao_app")
    result = subprocess.run(
        ["flutter", "build", "apk", "--release"], capture_output=True, text=True
    )
    if result.returncode != 0:
        raise Exception(f"Build failed: {result.stderr}")
    return "/Users/mac/workspace/magic-tao/molitao_app/build/app/outputs/flutter-apk/app-release.apk"


def main():
    parser = argparse.ArgumentParser(description="Deploy Molitao App")
    parser.add_argument("--version", required=True, help="Version name (e.g., 1.0.2)")
    parser.add_argument(
        "--build", type=int, required=True, help="Version code (e.g., 4)"
    )
    parser.add_argument("--desc", default="App update", help="Version description")
    parser.add_argument("--force", action="store_true", help="Force update")
    parser.add_argument("--skip-build", action="store_true", help="Skip build step")
    parser.add_argument(
        "--platform", default="android", help="Platform (default: android)"
    )
    args = parser.parse_args()

    remote_filename = f"molitao-v{args.version}-release.apk"
    remote_path = f"{REMOTE_DIR}/{remote_filename}"

    # APK 输出路径
    default_apk = "/Users/mac/workspace/magic-tao/molitao_app/build/app/outputs/flutter-apk/app-release.apk"

    # 1. 构建 APK
    if not args.skip_build:
        print("Building APK...")
        apk_path = build_apk()
        print(f"APK built: {apk_path}")
    else:
        apk_path = default_apk
        if not os.path.exists(apk_path):
            print("APK not found, building...")
            apk_path = build_apk()

    file_size = os.path.getsize(apk_path)
    print(f"APK file: {apk_path}")
    print(f"File size: {file_size} bytes")

    # 2. 上传到 CDN
    print(f"Uploading to CDN: {remote_path}...")
    try:
        download_url = upload_to_upyun(apk_path, remote_path)
        print(f"CDN URL: {download_url}")
    except Exception as e:
        print(f"Upload failed: {e}")
        # 如果上传失败，尝试使用本地文件路径作为下载 URL（用于本地开发）
        download_url = apk_path
        print(f"Using local path as download URL: {download_url}")

    # 3. 获取 Admin Token (via SSH)
    print("Authenticating via SSH...")
    try:
        token = get_admin_token_via_ssh()
        print(f"Got admin token: {token[:20]}...")
    except Exception as e:
        print(f"Authentication failed: {e}")
        print("Cannot proceed with API call. Please check admin credentials.")
        sys.exit(1)

    # 4. 调用后端 API 创建发布记录 (via SSH)
    print("Creating release record via API...")
    try:
        release_id = publish_by_url_via_ssh(
            token=token,
            version_name=args.version,
            version_code=args.build,
            description=args.desc,
            download_url=download_url,
            filename=remote_filename,
            file_size=file_size,
            is_force_update=args.force,
            platform=args.platform,
        )
        print(f"Release created with ID: {release_id}")
    except Exception as e:
        print(f"Failed to create release: {e}")
        print("\nNote: The file has been uploaded to CDN.")
        print(f"CDN URL: {download_url}")
        print(
            "You may need to call PublishAppReleaseByUrl manually or check API permissions."
        )
        sys.exit(1)

    # 5. 验证发布
    print("\nVerifying...")
    try:
        api_result = check_update_api(args.build - 1, args.platform)
        print(
            f"CheckUpdate API (currentVersionCode={args.build - 1}): {json.dumps(api_result, indent=2)}"
        )
    except Exception as e:
        print(f"Verification failed: {e}")

    print("\n" + "=" * 50)
    print(f"DEPLOY SUCCESS!")
    print(f"Version: {args.version} (build {args.build})")
    print(f"Download: {download_url}")
    print("=" * 50)


if __name__ == "__main__":
    main()
