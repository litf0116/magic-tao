<template>
    <div class="background-container">
        <!-- 基础背景 -->
        <div class="base-background"></div>

        <!-- 花瓣纹理背景 -->
        <div class="texture-layer flower-texture"></div>

        <!-- 牛皮纸纹理背景 -->
        <div class="texture-layer paper-texture"></div>

        <!-- 卡通场景背景 -->
        <div class="scene-background">
            <div class="scene-gradient"></div>
            <div class="scene-image"></div>
        </div>

        <!-- 顶部装饰图层 -->
        <div class="top-decoration-layer">
            <div class="top-decoration"></div>
        </div>
    </div>
</template>

<script setup lang="ts">
// 背景组件无需额外逻辑
</script>

<style lang="scss" scoped>
.background-container {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: -1;
    overflow: hidden;

    .base-background {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-color: #84D6FE;
    }

    .texture-layer {
        position: absolute;
        background-size: cover;
        background-repeat: no-repeat;

        &.flower-texture {
            width: 1124.9px;
            height: 1999.83px;
            right: 875.19px;
            bottom: -908.35px;
            background-image: url('@/assets/flower-texture.png');
            transform: rotate(90.34deg);
            opacity: 0.6;
        }

        &.paper-texture {
            width: 2511px;
            height: 1412px;
            left: -228px;
            top: -139px;
            background-image: url('@/assets/paper-texture.png');
            opacity: 0.4;
        }
    }

    .scene-background {
        position: absolute;
        width: 2016px;
        height: 720px;
        left: -48px;
        top: -237px;

        .scene-gradient {
            position: absolute;
            width: 100%;
            height: 100%;
            background: linear-gradient(180deg, #C4C4C4 87.57%, rgba(94, 94, 94, 0) 97.92%);
            opacity: 0.3;
        }

        .scene-image {
            position: absolute;
            width: 100%;
            height: 100%;
            background-image: url('@/assets/cartoon-scene.png');
            background-size: cover;
            background-position: center;
            opacity: 0.8;
        }
    }

    .top-decoration-layer {
        position: absolute;
        width: 1203px;
        height: 500px;
        left: 336px;
        top: -17px;

        .top-decoration {
            position: absolute;
            width: 100%;
            height: 100%;
            background-image: url('@/assets/top-decoration.png');
            background-size: cover;
            background-position: center;
        }
    }
}

/* 响应式适配 */
@media (max-width: 1920px) {
    .background-container {
        .texture-layer {
            &.flower-texture {
                width: 900px;
                height: 1600px;
                right: 700px;
                bottom: -700px;
            }

            &.paper-texture {
                width: 2000px;
                height: 1120px;
                left: -180px;
                top: -110px;
            }
        }

        .scene-background {
            width: 1600px;
            height: 576px;
            left: -38px;
            top: -190px;
        }

        .top-decoration-layer {
            width: 962px;
            height: 400px;
            left: 270px;
            top: -14px;
        }
    }
}

@media (max-width: 1440px) {
    .background-container {
        .scene-background {
            width: 1200px;
            height: 432px;
            left: -28px;
            top: -142px;
        }

        .top-decoration-layer {
            width: 722px;
            height: 300px;
            left: 202px;
            top: -10px;
        }
    }
}

@media (max-width: 768px) {
    .background-container {
        .texture-layer {
            &.flower-texture {
                width: 600px;
                height: 1067px;
                right: 400px;
                bottom: -400px;
            }

            &.paper-texture {
                width: 1400px;
                height: 784px;
                left: -126px;
                top: -77px;
            }
        }

        .scene-background {
            width: 900px;
            height: 324px;
            left: -21px;
            top: -107px;
        }

        .top-decoration-layer {
            width: 541px;
            height: 225px;
            left: 152px;
            top: -8px;
        }
    }
}
</style>