import { fileURLToPath, URL } from 'node:url';
import path from 'node:path';
import fs from 'fs';
import os from 'node:os';

import { defineConfig, loadEnv } from 'vite';
import vue from '@vitejs/plugin-vue';
import VueDevTools from 'vite-plugin-vue-devtools';
import tailwindcss from '@tailwindcss/vite';

import Inspect from 'vite-plugin-inspect';

import AutoImport from 'unplugin-auto-import/vite';
import Components from 'unplugin-vue-components/vite';
import { ElementPlusResolver } from 'unplugin-vue-components/resolvers';

import svgSpritePlugin from '@pivanov/vite-plugin-svg-sprite';

import { VitePWA } from 'vite-plugin-pwa';

export default defineConfig(({ command, mode }) => {

  const env = loadEnv(mode, process.cwd(), '');

  const optimizeDepsElementPlusIncludes = ['element-plus/es'];
  fs.readdirSync('node_modules/element-plus/es/components').map((dirname) => {
    fs.access(
      `node_modules/element-plus/es/components/${dirname}/style/css.mjs`,
      (err) => {
        if (!err) {
          optimizeDepsElementPlusIncludes.push(
            `element-plus/es/components/${dirname}/style/css`
          );
        }
      }
    );
  });

  return {
    base: '/', 
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url))
      },
      extensions: ['.mjs', '.js', '.ts', '.jsx', '.tsx', '.json', '.vue']
    },
    css: {
      preprocessorOptions: {
        scss: {
          silenceDeprecations: ['import']
        }
      }
    },
    build: {
      chunkSizeWarningLimit: 2500,
      rollupOptions: {
        onwarn(warning, defaultHandler) {
          if (warning.code === 'INVALID_ANNOTATION') return;
          defaultHandler(warning);
        }
      }
    },
    optimizeDeps: {
      include: optimizeDepsElementPlusIncludes
    },
    plugins: [
      vue(),
      tailwindcss(),
      VueDevTools({

        launchEditor: 'code'
      }),
      Inspect(),
      AutoImport({
        resolvers: [ElementPlusResolver()]
      }),
      Components({
        resolvers: [ElementPlusResolver()]
      }),
      svgSpritePlugin({
        iconDirs: [path.resolve(process.cwd(), 'src/icons/svg')],

        symbolId: 'icon-[name]',

        inject: 'body-last' 
      }),
      VitePWA({
        registerType: 'autoUpdate',
        includeAssets: ['favicon.ico', 'icon/apple-touch-icon.png'],
        workbox: {
          maximumFileSizeToCacheInBytes: 3000000
        },
        manifest: {
          name: 'GOLDB v3',
          short_name: 'GOLDB',
          description: 'Next-Generation B2B Jewelry Supply Chain Platform',
          theme_color: '#1a1a1a',
          background_color: '#ffffff',
          display: 'standalone',
          icons: [
            {
              src: 'icon/android-192x192.png',
              sizes: '192x192',
              type: 'image/png'
            },
            {
              src: 'icon/android-512x512.png',
              sizes: '512x512',
              type: 'image/png'
            },
            {
              src: 'icon/android-512x512.png',
              sizes: '512x512',
              type: 'image/png',
              purpose: 'any maskable'
            }
          ]
        }
      })
    ],
    server: {
      host: 'localhost',
      port: 5252,
      proxy: {
        '/api': {
          target: 'http://localhost:5077', 
          changeOrigin: true,
          rewrite: (path) => path.replace(/^\/api/, ''),
          headers: {
            Cookie: env.VITE_COOKIE
          }
        },
        '/uploads': {
          target: 'http://localhost:5077', 
          changeOrigin: true
        }
      }
    }
  };
});
