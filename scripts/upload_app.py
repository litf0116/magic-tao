#!/usr/bin/env python3
import hashlib
import hmac
import base64
import json
import time
import urllib.request
import urllib.error
import sys

BUCKET = "molitao"
OPERATOR = "molitao"
PASSWORD = "cHILcN0KfdVwcCkQz0Kjou7utChvtkpv"
IMG_URL = "http://image.molitao.top"
REMOTE_PATH = "/apps/molitao-v1.0.2-release.apk"
LOCAL_FILE = "/www/wwwroot/api/wwwroot/uploads/apps/molitao-v1.0.2-release.apk"

# Build policy
date_gmt = time.strftime("%a, %d %b %Y %H:%M:%S GMT", time.gmtime())
expiration = int(time.time()) + 43200

opts = {
    "save-key": REMOTE_PATH,
    "bucket": BUCKET,
    "expiration": expiration,
    "date": date_gmt,
}

policy_json = json.dumps(opts)
policy_base64 = base64.b64encode(policy_json.encode()).decode()

# Build signature data
data_str = "&".join(["POST", "/" + BUCKET, date_gmt, policy_base64])
print(f"Data for signature: {data_str}")

# Get signature from API
api_url = "http://localhost:5000/api/services/app/Upload/GetSignature"
import urllib.parse

params = urllib.parse.urlencode({"data": data_str, "policy": policy_base64})
req = urllib.request.Request(api_url + "?" + params)
try:
    with urllib.request.urlopen(req, timeout=30) as resp:
        res = json.loads(resp.read().decode())
        print(f"API Response: {res}")
        signature = res.get("signature", "")
except Exception as e:
    print(f"API Error: {e}")
    sys.exit(1)

# Upload file
upload_url = f"https://v0.api.upyun.com/{BUCKET}"

with open(LOCAL_FILE, "rb") as f:
    file_data = f.read()

boundary = "----FormBoundary7MA4YWxkTrZu0gW"

body = f"--{boundary}\r\n"
body += 'Content-Disposition: form-data; name="policy"\r\n\r\n'
body += f"{policy_base64}\r\n"
body += f"--{boundary}\r\n"
body += 'Content-Disposition: form-data; name="authorization"\r\n\r\n'
body += f"UPYUN {OPERATOR}:{signature}\r\n"
body += f"--{boundary}\r\n"
body += 'Content-Disposition: form-data; name="file"; filename="molitao-v1.0.2-release.apk"\r\n'
body += "Content-Type: application/octet-stream\r\n\r\n"

req = urllib.request.Request(
    upload_url,
    data=body.encode() + file_data + f"\r\n--{boundary}--\r\n".encode(),
    method="POST",
)
req.add_header("Content-Type", f"multipart/form-data; boundary={boundary}")

try:
    with urllib.request.urlopen(req, timeout=300) as resp:
        result = resp.read().decode()
        print(f"Upload Response: {resp.status} - {result}")
        if "ok" in result:
            print(f"\nFile URL: {IMG_URL}{REMOTE_PATH}")
except urllib.error.HTTPError as e:
    print(f"Upload HTTP Error: {e.code} - {e.read().decode()}")
except Exception as e:
    print(f"Upload Error: {e}")
