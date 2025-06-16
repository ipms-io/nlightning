import {defineConfig} from 'vite';
import {resolve} from 'path';

export default defineConfig({
    build: {
        target: 'esnext',
        minify: 'esbuild',
        lib: {
            entry: resolve(__dirname, 'blazorSodium.js'),
            formats: ['es'],
            fileName: 'blazorSodium.bundle'
        },
        outDir: '../../../wwwroot',
        sourcemap: false,
        emptyOutDir: true
    }
});