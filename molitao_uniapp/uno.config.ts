import presetWeapp from 'unocss-preset-weapp'
import { extractorAttributify, transformerClass } from 'unocss-preset-weapp/transformer'
import presetIcons from '@unocss/preset-icons'
import transformerDirectives from '@unocss/transformer-directives'
const { presetWeappAttributify, transformerAttributify } = extractorAttributify()
export default {
    presets: [
        presetWeapp({ whRpx: false }),
        // attributify autocomplete
        presetWeappAttributify(),
        presetIcons(),
    ],
    shortcuts: [
        {
            'switch-animation': 'transition duration-300',
            'flex-center': 'flex justify-center items-center',
            'border-base': 'border-gray-200 dark:border-dark-200',
            'bg-base': 'bg-white dark:bg-dark-100',
            'color-base': 'text-gray-900 dark:text-gray-300',
            'color-fade': 'text-gray-900:50 dark:text-gray-300:50',
        },
        // dynamic shortcuts
        [/^btn-(.*)$/, ([, c]) => `border-0 bg-${c}-500 hover:bg-${c}-400 text-${c}-100 rounded`],
        {
            'flex-center': 'flex justify-center items-center',
        },
    ],
    transformers: [
        transformerDirectives({
            enforce: 'pre',
        }),
        transformerAttributify(),
        transformerClass(),
    ],
    theme: {},
}
