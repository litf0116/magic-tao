<template>
    <div ref="container" :class="className" :style="{ height: height, width: width }" />
</template>

<script setup lang="ts" name="LineChart">
import * as echarts from 'echarts/core'
// 引入柱状图 + 折线图 + 饼图，图表后缀都为 Chart，一般常用的就这三个，如果还需要其他的，就自行添加
import { BarChart, LineChart, LineSeriesOption, PieChart } from 'echarts/charts'
// 引入提示框，标题，直角坐标系，数据集，内置数据转换器组件，组件后缀都为 Component
import {
    TitleComponent,
    TooltipComponent,
    GridComponent,
    DatasetComponent,
    TransformComponent,
    ToolboxComponent,
    LegendComponent,
    GridComponentOption,
} from 'echarts/components'
// 标签自动布局，全局过渡动画等特性
import { LabelLayout, UniversalTransition } from 'echarts/features'
// 引入 Canvas 渲染器，注意引入 CanvasRenderer 或者 SVGRenderer 是必须的一步
import { CanvasRenderer } from 'echarts/renderers'
// 注册必须的组件
echarts.use([
    TitleComponent,
    TooltipComponent,
    GridComponent,
    DatasetComponent,
    TransformComponent,
    ToolboxComponent,
    LegendComponent,
    LabelLayout,
    UniversalTransition,
    CanvasRenderer,
    BarChart,
    LineChart,
    PieChart,
])

type EChartsOption = echarts.ComposeOption<GridComponentOption | LineSeriesOption>

const props = defineProps({
    chartData: {
        type: Object,
        required: true,
    },
    xKey: {
        type: String,
        required: true,
    },
    yKey: {
        type: String,
        required: true,
    },
    className: {
        type: String,
        default: 'chart',
    },
    height: {
        type: String,
        default: '350px',
    },
    width: {
        type: String,
        default: '100%',
    },
    title: {
        type: Array<string>,
        default: () => ['标题'],
    },
    lineColor: {
        type: Array<string>,
        default: () => ['#409EFF'],
    },
    areaColor: {
        type: Array<string>,
        default: () => ['#f3f8ff'],
    },
    smooth: {
        type: Boolean,
        default: false,
    },
})
const container = ref<HTMLElement | null>(null)
let myChart: echarts.ECharts | null = null

const resizeHandler = () => {
    myChart?.resize()
}

onMounted(() => {
    nextTick(() => {
        initChart()
    })
    window.addEventListener('resize', resizeHandler)
})

onUnmounted(() => {
    window.removeEventListener('resize', resizeHandler)
    myChart?.dispose()
})

watch(
    () => props.chartData,
    (val) => {
        setOption(val)
    }
)

function initChart() {
    myChart = echarts.init(container.value)
    setOption(props.chartData)
}

function setOption(chartData) {
    const option: EChartsOption = {
        xAxis: {
            data: chartData.map((x) => x[props.yKey]),
            boundaryGap: false,
            axisTick: {
                show: false,
            },
        },
        grid: {
            left: 20,
            right: 20,
            bottom: 20,
            top: 30,
            containLabel: true,
        },
        tooltip: {
            trigger: 'axis',
            axisPointer: {
                type: 'cross',
            },
            padding: 8,
        },
        yAxis: {
            axisTick: {
                show: true,
            },
        },
        legend: {
            data: props.title,
        },
        series: [
            {
                name: props.title[0],
                smooth: props.smooth,
                type: 'line',
                itemStyle: {
                    normal: {
                        label: {
                            show: true,
                        },
                        color: props.lineColor[0],
                        lineStyle: {
                            color: props.lineColor[0],
                            width: 2,
                        },
                        areaStyle: {
                            color: props.areaColor[0],
                        },
                    },
                },
                data: chartData.map((x) => x[props.xKey]),
                animationDuration: 1000,
                animationEasing: 'quadraticOut',
            },
        ],
    }

    myChart.setOption(option)
}
</script>
