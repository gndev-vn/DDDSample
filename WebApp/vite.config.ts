import vue from '@vitejs/plugin-vue';
import tailwindcss from '@tailwindcss/vite';
import { defineConfig } from 'vite';
import { z } from 'zod';

const envSchema = z.object({
  PORT: z.coerce.number().int().positive().default(5173),
  CATALOG_API_URL: z.string().min(1).default('http://localhost:5000'),
  ORDERING_API_URL: z.string().min(1).default('http://localhost:5004'),
  PAYMENT_API_URL: z.string().min(1).default('http://localhost:5012'),
  IDENTITY_API_URL: z.string().min(1).default('http://localhost:5008'),
});

const env = envSchema.parse(process.env);

function createProxy(target: string, prefix: string) {
  return {
    target,
    changeOrigin: true,
    secure: false,
    rewrite: (path: string) => path.replace(new RegExp(`^${prefix}`), ''),
  };
}

export default defineConfig({
  plugins: [vue(), tailwindcss()],
  server: {
    host: '0.0.0.0',
    port: env.PORT,
    strictPort: true,
    proxy: {
      '/backend/catalog': createProxy(env.CATALOG_API_URL, '/backend/catalog'),
      '/backend/ordering': createProxy(env.ORDERING_API_URL, '/backend/ordering'),
      '/backend/payment': createProxy(env.PAYMENT_API_URL, '/backend/payment'),
      '/backend/identity': createProxy(env.IDENTITY_API_URL, '/backend/identity'),
    },
  },
  preview: {
    host: '0.0.0.0',
    port: env.PORT,
    strictPort: true,
  },
});
