package com.molitao;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

/**
 * 魔力淘后端 — Spring Boot 迁移版入口。
 *
 * <p>启动前请确保 {@code application.yml} 已正确配置数据库连接和 JWT 密钥。</p>
 */
@SpringBootApplication
public class MolitaoApplication {

    public static void main(String[] args) {
        SpringApplication.run(MolitaoApplication.class, args);
    }
}
