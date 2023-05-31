<template>
  <v-container>
    <h1>{{ $t('pm25Heading') }}</h1>
    <div v-if='stats'>
      <div>
        <v-autocomplete
          v-model='statsSelectedSensors'
          :chips='true'
          :multiple='true'
          name='id'
          item-title='name'
          item-value='id'
          class='ml-auto mr-0'
          :label='$t("select")'
          :items='availableSensors'
          @update:model-value='fetchStats'
        ></v-autocomplete>
      </div>
      <div
        v-for='sensor in stats.sensors'
        :key='sensor.sensor.id'
      >
        <h3>{{ sensor.sensor.name }}</h3>
        <v-row
          justify='center'
        >
          <v-col
            cols='auto'
          >
            <SheetInfo
              :title='sensor.daysAboveThreshold'
              :subtitle='$t("daysAboveThreshold")'
            />
          </v-col>
          <v-col
            cols='auto'
          >
            <SheetInfo
              :title='sensor.current'
              unit='µg/m³'
              :subtitle='$t("current")'
            />
          </v-col>
          <v-col
            cols='auto'
          >
            <SheetInfo
              :title='sensor.yearValueAvg'
              unit='µg/m³'
              :subtitle='$t("yearValueAvg")'
            />
          </v-col>
          <v-col
            cols='auto'
          >
            <SheetInfo
              :title='sensor.dayValueAvg'
              unit='µg/m³'
              :subtitle='$t("dayValueAvg")'
            />
          </v-col>
        </v-row>
      </div>
    </div>

    <ComparisonFilter
      :available-sensors='availableSensors'
      @update='fetchWeeklyChart'
    />
    <ComparisonChart
      :title='$t("airPollutionInWeeks")'
      :sensors='weeklyComparison.sensors'
      :loading='weeklyComparison.loading'
      unit='µg/m³' />

    <ExceedanceChart
      title='yearlyExceedances'
      series-name='exceedancesCount'
      :sensors='exceedChart.sensors'
      :loading='exceedChart.loading'
    />
    <div class='d-flex justify-center flex-column align-center'>
      <h2>{{ $t('airPollutionWeekComparison') }}</h2>
      <compare-chart-filter :available-sensors='availableSensors' @update='fetchComparisonChart' />

      <div v-if='showComparisonChart' ref='comparisonChart' class='chart mt-12' />
    </div>
    <v-divider class='mt-12' />
    <MapComponent
      :layers='map.layers'
      :features='map.features'
    />
  </v-container>
</template>


<script setup>

import { ref, watch } from 'vue'
import * as echarts from 'echarts'
import 'mapbox-gl/dist/mapbox-gl.css'
import { ky } from '@/lib/ky'
import CompareChartFilter from '@/components/CompareChartFilter.vue'
import SheetInfo from '@/components/SheetInfo.vue'
import ComparisonFilter from '@/components/ComparisonFilter.vue'
import ComparisonChart from '@/components/ComparisonChart.vue'
import { getLocale, t } from '@/lib/i18n'
import ExceedanceChart from '@/components/ExceedanceChart.vue'
import MapComponent from '@/components/MapComponent.vue'


const comparisonChart = ref(null)
const stats = ref(null)
const statsSelectedSensors = ref([])
const showComparisonChart = ref(false)
const availableSensors = ref([])
const weeklyComparison = ref({
  sensors: [],
  loading: false,
})
const exceedChart = ref({
  sensors: [],
  loading: false,
})
const map = ref({
  layers: [],
  features: [],
})

const fetchStats = async (ids) => {
  stats.value = await ky.post('pm25/stats', {
    json: {
      sensors: ids,
    },
  }).json()

  statsSelectedSensors.value = stats.value.sensors.map((sensor) => sensor.sensor.id)
}

const fetchLocations = async () => {
  availableSensors.value = await ky.get('pm25').json()
}

const fetchWeeklyChart = async (ids) => {
  weeklyComparison.value.loading = true
  const res = await ky.post('pm25', {
    json: {
      Sensors: ids,
      From: new Date(new Date().getFullYear(), 0, 1).toISOString(),
      To: new Date().toISOString(),
      Interval: 3,
    },
  }).json()

  weeklyComparison.value.sensors = res.sensors
  weeklyComparison.value.loading = false
}

const fetchYearlyExceedances = async () => {
  exceedChart.value.loading = true

  const res = await ky.get('pm25/exceed').json()

  exceedChart.value = {
    sensors: res,
    loading: false,
  }
}

const fetchComparisonChart = async (options) => {
  showComparisonChart.value = true
  const res = await ky.post('pm25/compare', {
    json: {
      Sensors: options.sensors,
      weekDays: options.days,
      hours: options.hours,
      weeks: options.weeks,
    },
  }).json()

  const categories = res[0].readings.map((reading) => new Date(reading.dateTime).toLocaleString(getLocale()))

  let days = []
  const series = res.map((sensor) => {
    const dates = res[0].readings.map((reading) => new Date(reading.dateTime))

    const areas = []
    let start = dates[0]
    let end = null

    for (let i = 0; i < dates.length; i++) {
      const date = dates[i]


      if (date.getDay() === start.getDay() && i !== dates.length - 1)
        continue

      const categoryDate = start.toLocaleDateString(getLocale(), { weekday: 'long' })
      if (days.indexOf(categoryDate) === -1) {
        days.push(categoryDate)
      }

      if (i === dates.length - 1)
        break

      end = dates[i - 1]


      areas.push({
        xAxis: end.toLocaleString(getLocale()),
        name: '',
      })

      start = date
      end = null
    }

    return {
      name: sensor.name,
      type: 'line',
      connectNulls: false,
      data: sensor.readings.map((reading) => reading.value),
      markLine: {
        symbol: ['none', 'none'],
        data: areas,
        label: {
          show: false,
        },
      },
    }
  })

  const chart = echarts.init(comparisonChart.value)
  chart.clear()

  chart.setOption({
    title: {
      text: t('airPollutionComparison'),
    },
    tooltip: {
      trigger: 'axis',
    },
    legend: {
      data: series.map((serie) => serie.name),
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '8%',
      containLabel: true,
    },
    toolbox: {
      feature: {
        saveAsImage: {},
      },
    },
    dataZoom: [
      {
        type: 'inside',
      },
      {
        type: 'slider',
      },
    ],
    xAxis: [
      {
        type: 'category',
        boundaryGap: true,
        data: categories,
        axisLabel: {
          rotate: 45,
        },
      },
      {
        position: 'bottom',
        offset: 90,
        axisLine: {
          show: false,
        },
        axisTick: {
          show: false,
        },
        // hide axis pointer
        axisPointer: {
          show: false,
        },
        data: days,
      },
    ],
    yAxis: {
      type: 'value',
      axisLabel: {
        formatter: '{value} µg/m3',
      },
    },
    series: series,
  })

  window.addEventListener('resize', () => {
    chart.resize()
  })
}

const fetchMap = async () => {
  const mapResponse = await ky.get('pm25/map').json()

  const reload = () => {
    const features = mapResponse.map((sensor) => {
      return {
        'type': 'Feature',
        'properties': {
          'description':
            `<div>
            <h3>${sensor.name}</h3>
            <h4>${t('pm25')}: ${sensor.readings[0].value} µg/m3</h4>
            <p>${new Date(sensor.readings[0].dateTime).toLocaleString(getLocale())}</p>
            <p>${sensor.location?.address}</p>
          </div>`,
          'value': sensor.readings[0].value,
        },
        'geometry': {
          'type': 'Point',
          'coordinates': [sensor.location?.longitude, sensor.location?.latitude],
        },
      }
    })

    map.value = {
      features: features,
      layers: [
        {
          'id': 'places',
          'type': 'symbol',
          'source': 'places',
          'layout': {
            'text-field': ['get', 'value'],
            'text-font': ['Open Sans Regular'],
            'text-size': 12,
            'text-offset': [0, 0.5],
            'text-anchor': 'top',
          },
          'paint': {
            'text-color': '#000',
          },
        },
        {
          'id': 'places-circle',
          'type': 'circle',
          'source': 'places',
          'paint': {
            'circle-color': [
              'interpolate',
              ['linear'],
              ['get', 'value'],
              0,
              '#00ff00',
              10,
              '#ffff00',
              20,
              '#ff0000',
            ],
            'circle-radius': 6,
            'circle-stroke-width': 2,
            'circle-stroke-color': '#ffffff',
          },
        }
      ]
    }
  }

  reload()
  watch(getLocale, reload)
}


fetchLocations()
fetchStats()
fetchWeeklyChart()
fetchYearlyExceedances()
fetchMap()



</script>


<style scoped>

.chart {
  width: 100%;
  height: 40em;
}
</style>
