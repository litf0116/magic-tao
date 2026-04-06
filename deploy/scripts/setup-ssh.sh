#!/usr/bin/expect

set timeout 30
set password "jDmeSH4ffFj8wZiy"

spawn ssh-copy-id -i /Users/mac/.ssh/id_rsa.pub -o StrictHostKeyChecking=no root@8.130.178.251

expect {
    "password:" {
        send "$password\r"
        expect eof
    }
    "already exist" {
        puts "密钥已存在"
        expect eof
    }
    timeout {
        puts "超时"
        exit 1
    }
}

spawn ssh -o StrictHostKeyChecking=no -o ConnectTimeout=5 root@8.130.178.251 "echo '免密登录成功'"
expect {
    "免密登录成功" {
        puts "\n✓ 免密登录配置成功！"
    }
    "password:" {
        puts "\n✗ 免密登录失败，仍需密码"
    }
    timeout {
        puts "\n✗ 连接超时"
    }
}