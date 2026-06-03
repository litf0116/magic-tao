package com.molitao.common.config;

import com.fasterxml.jackson.databind.PropertyNamingStrategies;
import com.fasterxml.jackson.databind.SerializationFeature;
import org.springframework.boot.autoconfigure.jackson.Jackson2ObjectMapperBuilderCustomizer;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

import java.util.TimeZone;

/**
 * Jackson 序列化配置。
 *
 * <p>保持与 C# ABP Framework 的 JSON 输出风格一致：</p>
 * <ul>
 *   <li>{@code LOWER_CAMEL_CASE} — C# {@code AccessToken} → JSON {@code accessToken}</li>
 *   <li>{@code Asia/Shanghai} — 时区</li>
 *   <li>{@code yyyy-MM-dd HH:mm:ss} — 日期时间格式</li>
 *   <li>{@code NON_NULL} — 不输出 null 字段（可选，保持响应简洁）</li>
 * </ul>
 */
@Configuration
public class JacksonConfig {

    @Bean
    public Jackson2ObjectMapperBuilderCustomizer customizer() {
        return builder -> {
            // CamelCase 命名 — 与 ABP 默认序列化一致
            builder.propertyNamingStrategy(PropertyNamingStrategies.LOWER_CAMEL_CASE);

            // 时区
            builder.timeZone(TimeZone.getTimeZone("Asia/Shanghai"));

            // 日期格式
            builder.simpleDateFormat("yyyy-MM-dd HH:mm:ss");

            // 不序列化 null 字段
            builder.serializationInclusion(com.fasterxml.jackson.annotation.JsonInclude.Include.NON_NULL);

            // Date 类型不写时间戳
            builder.featuresToDisable(SerializationFeature.WRITE_DATES_AS_TIMESTAMPS);
        };
    }
}
