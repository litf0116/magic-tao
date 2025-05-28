/// <reference types="vite/client" />
declare module '*.vue' {
    import { DefineComponent } from 'vue'
    // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/ban-types
    const component: DefineComponent<{}, {}, any>
    export default component
}

// declare module 'perfect-scrollbar'
declare module 'js-cookie'
declare module 'lodash'
declare module 'cash-dom'
// declare module "element-plus";
declare module '@jsdawn/vue3-tinymce'
declare interface Window {
    TMap: any
}
