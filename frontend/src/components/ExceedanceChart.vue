<template>
  <div ref='exceedChart' class='chart mt-12' />
</template>

<script setup>

import { onMounted, ref, watch } from 'vue'
import * as echarts from 'echarts'
import { getLocale, t } from '@/lib/i18n'

const exceedChart = ref(null)
let chart

const props = defineProps({
  loading: {
    type: Boolean,
    required: false,
    default: false,
  },
  title: {
    type: String,
    required: true,
  },
  sensors: {
    type: Array,
    required: true,
  },
  displayTime: {
    type: Boolean,
    required: false,
    default: false,
  },
  seriesName: {
    type: String,
    required: true,
  },
})

watch(() => props.sensors, () => {
  fetchChart()
})

watch(() => getLocale(), () => {
  fetchChart()
})

watch(() => props.loading, () => {
  if (props.loading) {
    chart.showLoading()
  } else {
    chart.hideLoading()
  }
})


const fetchChart = () => {
  const sensors = []
  const values = []

  props.sensors.forEach(item => {
    const sensorName = item.sensor.name
    const exceedance = item.exceed

    sensors.push(sensorName)
    values.push(exceedance)
  })

  const series = [{
    name: t(props.seriesName),
    type: 'bar',
    data: values,
  }]

  chart.clear()


  chart.setOption({
    title: {
      text: t(props.title),
    },
    tooltip: {
      trigger: 'axis',
    },
    legend: {
      data: [t(props.seriesName)],
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
      boundaryGap: true,
      data: sensors,
      axisLabel:
        {
          rotate: 45,
        },
    },
    yAxis: {
      type: 'value',
    },
    series: series,
  })
}

onMounted(() => {
  chart = echarts.init(exceedChart.value)

  window.addEventListener('resize', () => {
    console.log('resize')
    chart.resize()
  })
})




</script>

<style scoped>
.chart {
  width: 100%;
  height: 40em;
}
</style>
