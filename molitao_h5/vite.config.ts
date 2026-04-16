import { defineConfig } from 'vite'
import path from 'path'
import uni from '@dcloudio/vite-plugin-uni'
import AutoImport from 'unplugin-auto-import/vite'
import UnoCSS from 'unocss/vite'
import UniLayouts from '@uni-helper/vite-plugin-uni-layouts'

export default defineConfig(async () => ({
    optimizeDeps: {
        include: [
            'z-paging',
            'dayjs',
            'lodash-es',
            '@vueuse/core',
            'pinia'
        ],
        exclude: ['@dcloudio/uni-ui'],
        esbuildOptions: {
            resolveExtensions: ['.mjs', '.js', '.ts', '.jsx', '.tsx', '.json', '.vue']
        }
    },
    publicDir: 'public',
    base: './',
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
        // 修复 Service Worker MIME 类型问题
        headers: {
            'Content-Type': 'application/javascript; charset=utf-8',
        },
    },
    build: {
        sourcemap: false,
        watch: {
            exclude: ['node_modules/**', '/__uno.css'],
        },
        rollupOptions: {
            output: {
                manualChunks: {
                    'vendor-core': ['vue', 'pinia', '@vueuse/core'],
                    'vendor-ui': ['@climblee/uv-ui', '@dcloudio/uni-ui'],
                    'vendor-utils': ['dayjs', 'lodash-es'],
                    'vendor-chat': ['z-paging'],
                },
            },
        },
    },
    plugins: [
        // UniPages({
        //     exclude: ['**/components/**/**.*'], // 过滤掉pages里面的components文件夹
        // }),
        UniLayouts(),
        uni(),
        //     {
        //       name: "test",
        //       configResolved(config) {
        //         console.log("config.resolve.alias", config.resolve.alias);
        //       },
        //     },
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
        // Bundle analyzer - only enable when needed (ESM dynamic import)
        process.env.ANALYZE === 'true' && (await import('rollup-plugin-visualizer')).visualizer({
            filename: './dist/stats.html',
            open: true,
            gzipSize: true,
            brotliSize: true,
        }),
    ].filter(Boolean),
    css: {
        preprocessorOptions: {
            scss: {
                quietDeps: true,
                silenceDeprecations: ['import', 'global-builtin', 'legacy-js-api'],
            },
        },
    },
    resolve: {
        alias: {
            '@': path.resolve(__dirname, './src'),
        },
        mainFields: ['module', 'jsnext:main', 'jsnext', 'main'],
        extensions: ['.mjs', '.js', '.ts', '.jsx', '.tsx', '.json', '.vue'],
    },
    esbuild: {
        drop: ['console', 'debugger'],
    },
}))