import { presetUno, presetAttributify, presetIcons } from 'unocss'
import transformerDirectives from '@unocss/transformer-directives'
import { FileSystemIconLoader } from '@iconify/utils/lib/loader/node-loaders'
import { resolve } from 'node:path'
const iconDirectory = resolve(__dirname, 'icons')

import { asyncRouter } from './src/routes/index'
function getIcons(routes: any[]): string[] {
    console.log(routes)
    let icons: string[] = []
    routes.forEach((route) => {
        if (route.meta && route.meta.icon) {
            icons.push(route.meta.icon)
        }

        if (route.children) {
            icons = icons.concat(getIcons(route.children))
        }
    })
    return icons
}

// console.log(getIcons(asyncRouter))

export default {
    presets: [
        presetUno(),
        presetAttributify(),
        presetIcons({
            extraProperties: {
                display: 'inline-block',
                'vertical-align': 'middle',
            },
            collections: {
                mdi: () => import('@iconify-json/mdi/icons.json').then((i) => i.default),
                carbon: () => import('@iconify-json/carbon/icons.json').then((i) => i.default),
                custom: FileSystemIconLoader(iconDirectory),
            },
        }),
        // presetTagify()
    ],

    shortcuts: [
        // { logo: 'i-logos-vue w-6em h-6em transform transition-800' },
        // you could still have object style
        { btn: 'py-2 px-4 font-semibold rounded-lg shadow-md' },
        // dynamic shortcuts
        [/^btn-(.*)$/, ([, c]) => `border-0 bg-${c}-500 hover:bg-${c}-400 text-${c}-100 py-2 px-4 rounded-lg`],
        {
            'flex-center': 'flex justify-center items-center',
        },
    ],
    transformers: [transformerDirectives()],
}
