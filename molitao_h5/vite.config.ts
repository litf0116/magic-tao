import { defineConfig } from 'vite'
import uni from '@dcloudio/vite-plugin-uni'
import AutoImport from 'unplugin-auto-import/vite'
import UnoCSS from 'unocss/vite'
import UniLayouts from '@uni-helper/vite-plugin-uni-layouts'

export default defineConfig({
    publicDir: 'public',
    base: '/h5/',
    define: {
        __APP_VERSION__: '"20260407@1.2.0"',
    },
    server: {
        port: 5175,
        host: '0.0.0.0',
        proxy: {
            '/api': {
                target: 'http://127.0.0.1:12580',
                changeOrigin: true,
                secure: true,
            },
            '/ws': {
                target: 'ws://127.0.0.1:12580',
                ws: true,
                changeOrigin: true,
            },
        },
    },
    build: {
        sourcemap: false,
        watch: {
            exclude: ['node_modules/**', '/__uno.css'],
        },
        rollupOptions: {
            output: {
                manualChunks: undefined,
            },
        },
    },
    plugins: [
        // UniPages({
        //     exclude: ['**/components/**/**.*'], // 过滤掉pages里面的components文件夹
        // }),
        UniLayouts(),
        uni(),
        //     {
        //       name: "test",
        //       configResolved(config) {
        //         console.log("config.resolve.alias", config.resolve.alias);
        //       },
        //     },
        UnoCSS(),
        AutoImport({
            // targets to transform
            include: [
                /\.[tj]sx?$/, // .ts, .tsx, .js, .jsx
                /\.vue$/,
                /\.vue\?vue/, // .vue
            ],
            // global imports to register
            dirs: ['src/composables/**/*', './src/stores/**/*'],
            imports: ['vue'],
            // Enable auto import by filename for default module exports under directories
            // defaultExportByFilename: true,
            dts: './auto-imports.d.ts',
        }),
    ],
    css: {
        preprocessorOptions: {
            scss: {
                quietDeps: true,
                silenceDeprecations: ['import', 'global-builtin', 'legacy-js-api'],
            },
        },
    },
    esbuild: {
        // drop: ['console', 'debugger'],
    },
})