// vite.config.ts
import { resolve } from 'node:path'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import UnoCss from 'unocss/vite'
import AutoImport from 'unplugin-auto-import/vite'
import Components from 'unplugin-vue-components/vite'

const iconDirectory = resolve(__dirname, 'icons')
// https://vitejs.dev/config/
export default defineConfig(({ command, mode }) => {
    return {
        server: {
            host: '0.0.0.0',
            port: 4200,
        },
        resolve: {
            alias: {
                '@': resolve(__dirname, 'src'),
                '#': resolve(__dirname, 'types'),
            },
        },
        build: {
            // sourcemap: true,
            outDir: mode === 'production' ? 'dist' : 'dist_staging',
            // chunkSizeWarningLimit: 3 * 1024,
            rollupOptions: {
                manualChunks(id) {
                    if (id.includes('echarts')) {
                        return 'echarts'
                    }
                    if (id.includes('node_modules')) {
                        return 'vendor'
                    }
                },
            },
            // 生产环境移除console.log
            minify: 'terser',
            terserOptions: {
                compress: {
                    drop_console: mode === 'production',
                    drop_debugger: mode === 'production',
                },
            },
        },
        plugins: [
            vue(),
            vueJsx(),
            UnoCss(),
            AutoImport({
                // targets to transform
                include: [
                    /\.[tj]sx?$/, // .ts, .tsx, .js, .jsx
                    /\.vue$/,
                    /\.vue\?vue/, // .vue
                ],
                // global imports to register
                dirs: ['src/composables/**/*', './src/stores/**/*'],
                imports: ['vue', 'vue-router'],
                // Enable auto import by filename for default module exports under directories
                defaultExportByFilename: true,
                dts: './auto-imports.d.ts',
            }),
            Components({
                // dirs: ['src/components/**/*'],
                // include: [/\.vue$/, /\.tsx$/, /\.jsx$/, /\.md$/],
            }),
        ],
        css: {
            preprocessorOptions: {
                scss: {
                    additionalData: `@import "@/_variables.scss";`,
                },
            },
        },
    }
})
