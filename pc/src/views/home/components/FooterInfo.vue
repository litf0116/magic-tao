<template>
    <footer class="footer-info">
        <div class="footer-content">
            <div class="footer-text">
                网站底部信息栏网站底部信息栏网站底部信息栏网站底部信息栏网站底部信息栏网站底部信息栏
            </div>

            <!-- 额外的链接信息 -->
            <div class="footer-links">
                <a href="#" class="footer-link" @click.prevent>关于我们</a>
                <span class="separator">|</span>
                <a href="#" class="footer-link" @click.prevent>联系方式</a>
                <span class="separator">|</span>
                <a href="#" class="footer-link" @click.prevent>用户协议</a>
                <span class="separator">|</span>
                <a href="#" class="footer-link" @click.prevent>隐私政策</a>
            </div>

            <!-- 版权信息 -->
            <div class="copyright">
                © 2024 魔力宝贝怀旧服交流交易平台. All rights reserved.
            </div>
        </div>
    </footer>
</template>

<script setup lang="ts">
// 底部信息栏组件无需额外逻辑
</script>

<style lang="scss" scoped>
.footer-info {
    position: relative;
    width: 100%;
    margin-top: 60px;
    padding: 30px 0;
    background: rgba(136, 106, 92, 0.1);
    border-top: 1px solid rgba(136, 106, 92, 0.3);

    .footer-content {
        max-width: 1920px;
        margin: 0 auto;
        padding: 0 40px;
        text-align: center;

        .footer-text {
            font-family: 'Source Han Sans CN', sans-serif;
            font-style: normal;
            font-weight: 400;
            font-size: 16px;
            line-height: 100%;
            color: #886A5C;
            margin-bottom: 20px;
            max-width: 672px;
            margin-left: auto;
            margin-right: auto;
        }

        .footer-links {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 16px;
            margin-bottom: 16px;
            flex-wrap: wrap;

            .footer-link {
                font-family: 'Source Han Sans CN', sans-serif;
                font-weight: 400;
                font-size: 14px;
                color: #886A5C;
                text-decoration: none;
                transition: color 0.3s ease;

                &:hover {
                    color: #5B3B2D;
                    text-decoration: underline;
                }
            }

            .separator {
                color: #886A5C;
                opacity: 0.6;
            }
        }

        .copyright {
            font-family: 'Source Han Sans CN', sans-serif;
            font-weight: 400;
            font-size: 13px;
            color: #886A5C;
            opacity: 0.8;
        }
    }
}

/* 响应式适配 */
@media (max-width: 1920px) {
    .footer-info {
        margin-top: 50px;
        padding: 25px 0;

        .footer-content {
            padding: 0 30px;

            .footer-text {
                font-size: 15px;
                margin-bottom: 18px;
            }

            .footer-links {
                gap: 14px;
                margin-bottom: 14px;

                .footer-link {
                    font-size: 13px;
                }
            }

            .copyright {
                font-size: 12px;
            }
        }
    }
}

@media (max-width: 1440px) {
    .footer-info {
        margin-top: 40px;
        padding: 20px 0;

        .footer-content {
            padding: 0 20px;

            .footer-text {
                font-size: 14px;
                margin-bottom: 15px;
            }

            .footer-links {
                gap: 12px;
                margin-bottom: 12px;

                .footer-link {
                    font-size: 12px;
                }
            }

            .copyright {
                font-size: 11px;
            }
        }
    }
}

@media (max-width: 768px) {
    .footer-info {
        margin-top: 30px;
        padding: 15px 0;

        .footer-content {
            padding: 0 15px;

            .footer-text {
                font-size: 13px;
                margin-bottom: 12px;
                line-height: 1.4;
            }

            .footer-links {
                gap: 10px;
                margin-bottom: 10px;
                flex-direction: column;
                align-items: center;

                .footer-link {
                    font-size: 12px;
                }

                .separator {
                    display: none; /* 在移动端隐藏分隔符 */
                }
            }

            .copyright {
                font-size: 10px;
                line-height: 1.4;
            }
        }
    }
}

@media (max-width: 480px) {
    .footer-info {
        margin-top: 20px;
        padding: 10px 0;

        .footer-content {
            padding: 0 10px;

            .footer-text {
                font-size: 12px;
                margin-bottom: 10px;
            }

            .footer-links {
                gap: 8px;
                margin-bottom: 8px;

                .footer-link {
                    font-size: 11px;
                }
            }

            .copyright {
                font-size: 9px;
            }
        }
    }
}
</style>