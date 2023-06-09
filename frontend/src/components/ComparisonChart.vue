<template>
  <div ref='chartContainer' class='chart mt-12' />
</template>

<script setup>
import * as echarts from 'echarts'
import { onMounted, ref, watch } from 'vue'
import { getLocale } from '@/lib/i18n'

const props = defineProps({
  loading: {
    type: Boolean,
    required: false,
    default: false,
  },
  unit: {
    type: String,
    required: true,
  },
  title: {
    type: String,
    required: false,
    default: '',
  },
  sensors: {
    type: Array,
    required: true,
  },
  zoom: {
    type: Boolean,
    required: false,
    default: false,
  },
  displayTime: {
    type: Boolean,
    required: false,
    default: false,
  },
  limit: {
    type: Number,
    required: false,
    default: 0,
  },
  scale: {
    type: Object,
    required: false,
    default: () => ({
      min: null,
      max: null,
      scale: null,
    }),
  },
  connectNulls: {
    type: Boolean,
    required: false,
    default: false,
  },
})

const chartContainer = ref(null)
let chart


onMounted(() => {
  chart = echarts.init(chartContainer.value)

  window.addEventListener('resize', () => {
    chart.resize()
  })
})

watch(() => props.sensors, () => {
  fetchChart()
})

watch(() => props.loading, () => {
  if (props.loading) {
    chart.showLoading()
  } else {
    chart.hideLoading()
  }
})

watch(() => props.title, () => {
  fetchChart()
})

const fetchChart = async () => {
  const readingDates = props.sensors[0].readings.map((reading) => {
    if (props.displayTime) {
      return new Date(reading.dateTime).toLocaleString(getLocale())
    }
    return new Date(reading.dateTime).toLocaleDateString(getLocale())
  })

  const markline = props.limit ?
    {
      markLine: {
        label: {
          show: true,
        },
        data: [
          {
            yAxis: props.limit,
            name: 'Limit',
          }
        ]
      },
    }
    :
    null

  const series = props.sensors.map((sensor) => {
    return {
      name: sensor.name,
      type: 'line',
      connectNulls: props.connectNulls,
      data: sensor.readings.map((reading) => reading.value),
      ...markline,
    }
  })


  chart.clear()

  const zoom = props.zoom ?
    [
      {
        type: 'inside',
      },
      {
        type: 'slider',
      },
    ]
    :
    null

  const options = {
    title: {
      text: props.title,
      top: '4%',
    },
    tooltip: {
      trigger: 'axis',
    },
    legend: {
      data: series.map((series) => series.name),
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      containLabel: true,
    },
    toolbox: {
      feature: {
        saveAsImage: {},
      },
    },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: readingDates,
    },
    yAxis: {
      type: 'value',
      axisLabel: {
        formatter: `{value} ${props.unit}`,
      },
      ...props.scale,
    },
    dataZoom: zoom,
    series: series,
  }


  chart.setOption(options)
}

</script>

<style scoped>
.chart {
  width: 100%;
  height: 40em;
}
</style>
