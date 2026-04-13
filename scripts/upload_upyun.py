#!/usr/bin/env python3
import base64
import hashlib
import hmac
import json
import os
import sys
import time
import urllib.request
import urllib.parse

API_BASE = "http://localhost:5000"
BUCKET = "molitao"
OPERATOR = "molitao"
PASSWORD = "cHILcN0KfdVwcCkQz0Kjou7utChvtkpv"
DOMAIN = "http://image.molitao.top"


def upload_to_upyun(local_file, remote_path=None):
    if not os.path.exists(local_file):
        print(f"File not found: {local_file}")
        return None

    file_size = os.path.getsize(local_file)
    filename = os.path.basename(local_file)
    remote_path = remote_path or f"apps/{filename}"

    print(f"Uploading {local_file} ({file_size} bytes) to {remote_path}")

    deadline = int(time.time()) + 3600

    policy_dict = {
        "bucket": BUCKET,
        "save-key": f"/{remote_path}",
        "expiration": deadline,
        "file-size": {"maximum": file_size},
    }

    policy_json = json.dumps(policy_dict, separators=(",", ":"))
    policy_base64 = base64.b64encode(policy_json.encode()).decode().rstrip("=\n")

    password_md5 = hashlib.md5(PASSWORD.encode()).hexdigest()
    signature = hmac.new(
        password_md5.encode(), policy_base64.encode(), hashlib.sha1
    ).digest()
    signature_b64 = base64.b64encode(signature).decode().rstrip("=+")

    print(f"Policy: {policy_base64}")
    print(f"Signature: {signature_b64}")

    url = f"http://v0.api.upyun.com/{BUCKET}"

    boundary = "----FormBoundary7MA4YWxkTrZu0gW"

    with open(local_file, "rb") as f:
        file_data = f.read()

    body = f"--{boundary}\r\n"
    body += f'Content-Disposition: form-data; name="policy"\r\n\r\n'
    body += f"{policy_base64}\r\n"
    body += f"--{boundary}\r\n"
    body += f'Content-Disposition: form-data; name="signature"\r\n\r\n'
    body += f"{signature_b64}\r\n"
    body += f"--{boundary}\r\n"
    body += f'Content-Disposition: form-data; name="file"; filename="{filename}"\r\n'
    body += "Content-Type: application/octet-stream\r\n\r\n"

    req = urllib.request.Request(
        url,
        data=body.encode() + file_data + f"\r\n--{boundary}--\r\n".encode(),
        method="POST",
    )
    req.add_header("Content-Type", f"multipart/form-data; boundary={boundary}")

    try:
        with urllib.request.urlopen(req, timeout=300) as response:
            result = response.read().decode()
            print(f"Response: {response.status} - {result}")
            if response.status == 200:
                return f"{DOMAIN}/{remote_path}"
    except urllib.error.HTTPError as e:
        print(f"HTTP Error: {e.code} - {e.read().decode()}")
    except Exception as e:
        print(f"Error: {e}")

    if response.status_code == 200:
        return f"{DOMAIN}/{remote_path}"
    return None


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python upload_upyun.py <local-file> [remote-path]")
        sys.exit(1)

    local_file = sys.argv[1]
    remote_path = sys.argv[2] if len(sys.argv) > 2 else None

    result = upload_to_upyun(local_file, remote_path)
    if result:
        print(f"\nUpload successful!")
        print(f"URL: {result}")
    else:
        print("\nUpload failed!")
        sys.exit(1)
