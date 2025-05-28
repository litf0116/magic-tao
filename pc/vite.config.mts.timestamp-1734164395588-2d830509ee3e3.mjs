// vite.config.mts
import { resolve } from "node:path";
import { defineConfig } from "file:///E:/DotNet/%E9%AD%94%E5%8A%9B%E6%B7%98/pc/node_modules/vite/dist/node/index.js";
import vue from "file:///E:/DotNet/%E9%AD%94%E5%8A%9B%E6%B7%98/pc/node_modules/@vitejs/plugin-vue/dist/index.mjs";
import vueJsx from "file:///E:/DotNet/%E9%AD%94%E5%8A%9B%E6%B7%98/pc/node_modules/@vitejs/plugin-vue-jsx/dist/index.mjs";
import UnoCss from "file:///E:/DotNet/%E9%AD%94%E5%8A%9B%E6%B7%98/pc/node_modules/unocss/dist/vite.mjs";
import AutoImport from "file:///E:/DotNet/%E9%AD%94%E5%8A%9B%E6%B7%98/pc/node_modules/unplugin-auto-import/dist/vite.js";
import Components from "file:///E:/DotNet/%E9%AD%94%E5%8A%9B%E6%B7%98/pc/node_modules/unplugin-vue-components/dist/vite.mjs";
var __vite_injected_original_dirname = "E:\\DotNet\\\u9B54\u529B\u6DD8\\pc";
var iconDirectory = resolve(__vite_injected_original_dirname, "icons");
var vite_config_default = defineConfig(({ command, mode }) => {
  return {
    server: {
      host: "0.0.0.0",
      port: 4200
    },
    resolve: {
      alias: {
        "@": resolve(__vite_injected_original_dirname, "src"),
        "#": resolve(__vite_injected_original_dirname, "types")
      }
    },
    build: {
      // sourcemap: true,
      outDir: mode === "production" ? "dist" : "dist_staging",
      // chunkSizeWarningLimit: 3 * 1024,
      rollupOptions: {
        manualChunks(id) {
          if (id.includes("echarts")) {
            return "echarts";
          }
          if (id.includes("node_modules")) {
            return "vendor";
          }
        }
      }
    },
    plugins: [
      vue(),
      vueJsx(),
      UnoCss(),
      AutoImport({
        // targets to transform
        include: [
          /\.[tj]sx?$/,
          // .ts, .tsx, .js, .jsx
          /\.vue$/,
          /\.vue\?vue/
          // .vue
        ],
        // global imports to register
        dirs: ["src/composables/**/*", "./src/stores/**/*"],
        imports: ["vue", "vue-router"],
        // Enable auto import by filename for default module exports under directories
        defaultExportByFilename: true,
        dts: "./auto-imports.d.ts"
      }),
      Components({
        // dirs: ['src/components/**/*'],
        // include: [/\.vue$/, /\.tsx$/, /\.jsx$/, /\.md$/],
      })
    ],
    css: {
      preprocessorOptions: {
        scss: {
          additionalData: `@import "@/_variables.scss";`
        }
      }
    }
  };
});
export {
  vite_config_default as default
};
//# sourceMappingURL=data:application/json;base64,ewogICJ2ZXJzaW9uIjogMywKICAic291cmNlcyI6IFsidml0ZS5jb25maWcubXRzIl0sCiAgInNvdXJjZXNDb250ZW50IjogWyJjb25zdCBfX3ZpdGVfaW5qZWN0ZWRfb3JpZ2luYWxfZGlybmFtZSA9IFwiRTpcXFxcRG90TmV0XFxcXFx1OUI1NFx1NTI5Qlx1NkREOFxcXFxwY1wiO2NvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9maWxlbmFtZSA9IFwiRTpcXFxcRG90TmV0XFxcXFx1OUI1NFx1NTI5Qlx1NkREOFxcXFxwY1xcXFx2aXRlLmNvbmZpZy5tdHNcIjtjb25zdCBfX3ZpdGVfaW5qZWN0ZWRfb3JpZ2luYWxfaW1wb3J0X21ldGFfdXJsID0gXCJmaWxlOi8vL0U6L0RvdE5ldC8lRTklQUQlOTQlRTUlOEElOUIlRTYlQjclOTgvcGMvdml0ZS5jb25maWcubXRzXCI7Ly8gdml0ZS5jb25maWcudHNcclxuaW1wb3J0IHsgcmVzb2x2ZSB9IGZyb20gJ25vZGU6cGF0aCdcclxuaW1wb3J0IHsgZGVmaW5lQ29uZmlnIH0gZnJvbSAndml0ZSdcclxuaW1wb3J0IHZ1ZSBmcm9tICdAdml0ZWpzL3BsdWdpbi12dWUnXHJcbmltcG9ydCB2dWVKc3ggZnJvbSAnQHZpdGVqcy9wbHVnaW4tdnVlLWpzeCdcclxuaW1wb3J0IFVub0NzcyBmcm9tICd1bm9jc3Mvdml0ZSdcclxuaW1wb3J0IEF1dG9JbXBvcnQgZnJvbSAndW5wbHVnaW4tYXV0by1pbXBvcnQvdml0ZSdcclxuaW1wb3J0IENvbXBvbmVudHMgZnJvbSAndW5wbHVnaW4tdnVlLWNvbXBvbmVudHMvdml0ZSdcclxuXHJcbmNvbnN0IGljb25EaXJlY3RvcnkgPSByZXNvbHZlKF9fZGlybmFtZSwgJ2ljb25zJylcclxuLy8gaHR0cHM6Ly92aXRlanMuZGV2L2NvbmZpZy9cclxuZXhwb3J0IGRlZmF1bHQgZGVmaW5lQ29uZmlnKCh7IGNvbW1hbmQsIG1vZGUgfSkgPT4ge1xyXG4gICAgcmV0dXJuIHtcclxuICAgICAgICBzZXJ2ZXI6IHtcclxuICAgICAgICAgICAgaG9zdDogJzAuMC4wLjAnLFxyXG4gICAgICAgICAgICBwb3J0OiA0MjAwLFxyXG4gICAgICAgIH0sXHJcbiAgICAgICAgcmVzb2x2ZToge1xyXG4gICAgICAgICAgICBhbGlhczoge1xyXG4gICAgICAgICAgICAgICAgJ0AnOiByZXNvbHZlKF9fZGlybmFtZSwgJ3NyYycpLFxyXG4gICAgICAgICAgICAgICAgJyMnOiByZXNvbHZlKF9fZGlybmFtZSwgJ3R5cGVzJyksXHJcbiAgICAgICAgICAgIH0sXHJcbiAgICAgICAgfSxcclxuICAgICAgICBidWlsZDoge1xyXG4gICAgICAgICAgICAvLyBzb3VyY2VtYXA6IHRydWUsXHJcbiAgICAgICAgICAgIG91dERpcjogbW9kZSA9PT0gJ3Byb2R1Y3Rpb24nID8gJ2Rpc3QnIDogJ2Rpc3Rfc3RhZ2luZycsXHJcbiAgICAgICAgICAgIC8vIGNodW5rU2l6ZVdhcm5pbmdMaW1pdDogMyAqIDEwMjQsXHJcbiAgICAgICAgICAgIHJvbGx1cE9wdGlvbnM6IHtcclxuICAgICAgICAgICAgICAgIG1hbnVhbENodW5rcyhpZCkge1xyXG4gICAgICAgICAgICAgICAgICAgIGlmIChpZC5pbmNsdWRlcygnZWNoYXJ0cycpKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIHJldHVybiAnZWNoYXJ0cydcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKGlkLmluY2x1ZGVzKCdub2RlX21vZHVsZXMnKSkge1xyXG4gICAgICAgICAgICAgICAgICAgICAgICByZXR1cm4gJ3ZlbmRvcidcclxuICAgICAgICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgIH0sXHJcbiAgICAgICAgcGx1Z2luczogW1xyXG4gICAgICAgICAgICB2dWUoKSxcclxuICAgICAgICAgICAgdnVlSnN4KCksXHJcbiAgICAgICAgICAgIFVub0NzcygpLFxyXG4gICAgICAgICAgICBBdXRvSW1wb3J0KHtcclxuICAgICAgICAgICAgICAgIC8vIHRhcmdldHMgdG8gdHJhbnNmb3JtXHJcbiAgICAgICAgICAgICAgICBpbmNsdWRlOiBbXHJcbiAgICAgICAgICAgICAgICAgICAgL1xcLlt0al1zeD8kLywgLy8gLnRzLCAudHN4LCAuanMsIC5qc3hcclxuICAgICAgICAgICAgICAgICAgICAvXFwudnVlJC8sXHJcbiAgICAgICAgICAgICAgICAgICAgL1xcLnZ1ZVxcP3Z1ZS8sIC8vIC52dWVcclxuICAgICAgICAgICAgICAgIF0sXHJcbiAgICAgICAgICAgICAgICAvLyBnbG9iYWwgaW1wb3J0cyB0byByZWdpc3RlclxyXG4gICAgICAgICAgICAgICAgZGlyczogWydzcmMvY29tcG9zYWJsZXMvKiovKicsICcuL3NyYy9zdG9yZXMvKiovKiddLFxyXG4gICAgICAgICAgICAgICAgaW1wb3J0czogWyd2dWUnLCAndnVlLXJvdXRlciddLFxyXG4gICAgICAgICAgICAgICAgLy8gRW5hYmxlIGF1dG8gaW1wb3J0IGJ5IGZpbGVuYW1lIGZvciBkZWZhdWx0IG1vZHVsZSBleHBvcnRzIHVuZGVyIGRpcmVjdG9yaWVzXHJcbiAgICAgICAgICAgICAgICBkZWZhdWx0RXhwb3J0QnlGaWxlbmFtZTogdHJ1ZSxcclxuICAgICAgICAgICAgICAgIGR0czogJy4vYXV0by1pbXBvcnRzLmQudHMnLFxyXG4gICAgICAgICAgICB9KSxcclxuICAgICAgICAgICAgQ29tcG9uZW50cyh7XHJcbiAgICAgICAgICAgICAgICAvLyBkaXJzOiBbJ3NyYy9jb21wb25lbnRzLyoqLyonXSxcclxuICAgICAgICAgICAgICAgIC8vIGluY2x1ZGU6IFsvXFwudnVlJC8sIC9cXC50c3gkLywgL1xcLmpzeCQvLCAvXFwubWQkL10sXHJcbiAgICAgICAgICAgIH0pLFxyXG4gICAgICAgIF0sXHJcbiAgICAgICAgY3NzOiB7XHJcbiAgICAgICAgICAgIHByZXByb2Nlc3Nvck9wdGlvbnM6IHtcclxuICAgICAgICAgICAgICAgIHNjc3M6IHtcclxuICAgICAgICAgICAgICAgICAgICBhZGRpdGlvbmFsRGF0YTogYEBpbXBvcnQgXCJAL192YXJpYWJsZXMuc2Nzc1wiO2AsXHJcbiAgICAgICAgICAgICAgICB9LFxyXG4gICAgICAgICAgICB9LFxyXG4gICAgICAgIH0sXHJcbiAgICB9XHJcbn0pXHJcbiJdLAogICJtYXBwaW5ncyI6ICI7QUFDQSxTQUFTLGVBQWU7QUFDeEIsU0FBUyxvQkFBb0I7QUFDN0IsT0FBTyxTQUFTO0FBQ2hCLE9BQU8sWUFBWTtBQUNuQixPQUFPLFlBQVk7QUFDbkIsT0FBTyxnQkFBZ0I7QUFDdkIsT0FBTyxnQkFBZ0I7QUFQdkIsSUFBTSxtQ0FBbUM7QUFTekMsSUFBTSxnQkFBZ0IsUUFBUSxrQ0FBVyxPQUFPO0FBRWhELElBQU8sc0JBQVEsYUFBYSxDQUFDLEVBQUUsU0FBUyxLQUFLLE1BQU07QUFDL0MsU0FBTztBQUFBLElBQ0gsUUFBUTtBQUFBLE1BQ0osTUFBTTtBQUFBLE1BQ04sTUFBTTtBQUFBLElBQ1Y7QUFBQSxJQUNBLFNBQVM7QUFBQSxNQUNMLE9BQU87QUFBQSxRQUNILEtBQUssUUFBUSxrQ0FBVyxLQUFLO0FBQUEsUUFDN0IsS0FBSyxRQUFRLGtDQUFXLE9BQU87QUFBQSxNQUNuQztBQUFBLElBQ0o7QUFBQSxJQUNBLE9BQU87QUFBQTtBQUFBLE1BRUgsUUFBUSxTQUFTLGVBQWUsU0FBUztBQUFBO0FBQUEsTUFFekMsZUFBZTtBQUFBLFFBQ1gsYUFBYSxJQUFJO0FBQ2IsY0FBSSxHQUFHLFNBQVMsU0FBUyxHQUFHO0FBQ3hCLG1CQUFPO0FBQUEsVUFDWDtBQUNBLGNBQUksR0FBRyxTQUFTLGNBQWMsR0FBRztBQUM3QixtQkFBTztBQUFBLFVBQ1g7QUFBQSxRQUNKO0FBQUEsTUFDSjtBQUFBLElBQ0o7QUFBQSxJQUNBLFNBQVM7QUFBQSxNQUNMLElBQUk7QUFBQSxNQUNKLE9BQU87QUFBQSxNQUNQLE9BQU87QUFBQSxNQUNQLFdBQVc7QUFBQTtBQUFBLFFBRVAsU0FBUztBQUFBLFVBQ0w7QUFBQTtBQUFBLFVBQ0E7QUFBQSxVQUNBO0FBQUE7QUFBQSxRQUNKO0FBQUE7QUFBQSxRQUVBLE1BQU0sQ0FBQyx3QkFBd0IsbUJBQW1CO0FBQUEsUUFDbEQsU0FBUyxDQUFDLE9BQU8sWUFBWTtBQUFBO0FBQUEsUUFFN0IseUJBQXlCO0FBQUEsUUFDekIsS0FBSztBQUFBLE1BQ1QsQ0FBQztBQUFBLE1BQ0QsV0FBVztBQUFBO0FBQUE7QUFBQSxNQUdYLENBQUM7QUFBQSxJQUNMO0FBQUEsSUFDQSxLQUFLO0FBQUEsTUFDRCxxQkFBcUI7QUFBQSxRQUNqQixNQUFNO0FBQUEsVUFDRixnQkFBZ0I7QUFBQSxRQUNwQjtBQUFBLE1BQ0o7QUFBQSxJQUNKO0FBQUEsRUFDSjtBQUNKLENBQUM7IiwKICAibmFtZXMiOiBbXQp9Cg==
