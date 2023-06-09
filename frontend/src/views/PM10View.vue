<template>
  <v-container>
    <h1>{{ $t('pm10') }}</h1>
    <b>{{ $t('pmSubtitle') }}</b>
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
        <div class='d-flex'>
          <v-btn
            :icon='appStore.isFavorite(sensor.sensor) ? "mdi-heart" : "mdi-heart-outline"'
            density='compact'
            variant='text'
            class='mr-3'
            :color='appStore.isFavorite(sensor.sensor) ? "red" : ""'
            @click='appStore.toggleFavorite(sensor.sensor)'
          />

          <h3>{{ sensor.sensor.name }}</h3>
        </div>
        <v-row
          justify='center'
        >
          <v-col
            cols='auto'
          >
            <SheetInfo
              :title='sensor.daysAboveThreshold'
              :subtitle='$t("daysAboveThreshold")'
              :help='$t("pm10daysAboveThresholdHelp")'
            />
          </v-col>
          <v-col
            cols='auto'
          >
            <SheetInfo
              :title='sensor.current'
              :subtitle='$t("current")'
              unit='µg/m³'
            />
          </v-col>
          <v-col
            cols='auto'
          >
            <SheetInfo
              :title='sensor.yearValueAvg'
              unit='µg/m³'
              :subtitle='$t("yearValueAvg")'
              :description='$t("yearAvgDescription", [new Date().getFullYear()])'
            />
          </v-col>
          <v-col
            cols='auto'
          >
            <SheetInfo
              :title='sensor.dayValueAvg'
              unit='µg/m³'
              :subtitle='$t("dayValueAvg")'
              :description='$t("dayAvgDescription")'
            />
          </v-col>
        </v-row>
      </div>
    </div>

    <v-divider class='my-5' />

    <div class='d-flex justify-space-between align-center'>
      <h3>{{ $t("airPollutionInWeeks") }}</h3>

      <ComparisonFilter
        :available-sensors='availableSensors'
        @update='fetchWeeklyChart'
      />
    </div>
    <ComparisonChart
      :sensors='weeklyComparison.sensors'
      :loading='weeklyComparison.loading'
      unit='µg/m³'
      :limit='50'
      :scale='{
        min: 60,
        max: 0
      }'
    />

    <v-divider class='my-5' />

    <ExceedanceChart
      title='yearlyExceedances'
      series-name='exceedancesCount'
      :sensors='exceedChart.sensors'
      :loading='exceedChart.loading'
    />

    <v-divider class='my-5' />

    <div class='d-flex justify-space-between align-center'>
      <h3>{{ $t('airPollutionWeekComparison') }}</h3>
      <compare-chart-filter :available-sensors='availableSensors' @update='fetchComparisonChart' />
    </div>

    <div v-if='showComparisonChart' ref='comparisonChart' class='chart mt-12' />

    <v-divider class='mt-12 mb-5' />

    <h3>{{ $t('mapHeadlinePM') }}</h3>

    <LegendComponent
      :limits='[
        {
          value: 0,
          color: "#00e400",
        },
        {
          value: 25,
          color: "#ffff00",
        },
        {
          value: 50,
          color: "#ff0000",
        },
      ]'
      unit='µg/m³'
      :value='gaugeValue'
    />

    <MapComponent
      :layers='map.layers'
      :features='map.features'
      class='mt-8'
      @enter='gaugeValue = $event'
      @leave='gaugeValue = null'
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
import MapComponent from '@/components/MapComponent.vue'
import ExceedanceChart from '@/components/ExceedanceChart.vue'
import { useAppStore } from '@/store/app'
import LegendComponent from '@/components/LegendComponent.vue'

const appStore = useAppStore()

const comparisonChart = ref(null)
const gaugeValue = ref(null)
const stats = ref(null)
const statsSelectedSensors = ref([])
const showComparisonChart = ref(false)
const availableSensors = ref([])
const weeklyComparison = ref({
  sensors: [],
  loading: false,
})
const map = ref({
  layers: [],
  features: [],
})
const exceedChart = ref({
  sensors: [],
  loading: false,
})

const fetchStats = async (ids) => {
  stats.value = await ky.post('pm10/stats', {
    json: {
      sensors: ids,
    },
  }).json()

  statsSelectedSensors.value = stats.value.sensors.map((sensor) => sensor.sensor.id)
}

const fetchLocations = async () => {
  availableSensors.value = await ky.get('pm10').json()
}

const fetchWeeklyChart = async (ids) => {
  weeklyComparison.value.loading = true
  const res = await ky.post('pm10', {
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

  const res = await ky.get('pm10/exceed').json()

  exceedChart.value = {
    sensors: res,
    loading: false,
  }
}

const fetchComparisonChart = async (options) => {
  showComparisonChart.value = true
  const res = await ky.post('pm10/compare', {
    json: {
      Sensors: options.sensors,
      weekDays: options.days,
      hours: options.hours,
      weeks: options.weeks,
    },
  }).json()

  const categories = res[0].readings.map((reading) => new Date(reading.dateTime).toLocaleString(getLocale()))

  let fetchDays = true
  let days = []
  const series = res.map((sensor) => {
    const dates = res[0].readings.map((reading) => new Date(reading.dateTime))

    const areas = []
    let start = dates[0]
    let end = null

    for (let i = 0; i < dates.length; i++) {
      const date = dates[i]

      if (date.toDateString() === start.toDateString()) {
        continue
      }
      end = dates[i - 1]


      const categoryDate = start.toLocaleDateString(getLocale(), { weekday: 'long' })


      if (fetchDays)
        days.push(categoryDate)


      areas.push({
        xAxis: end.toLocaleString(getLocale()),
        name: '',
      })

      start = date
    }

    if (fetchDays)
      days.push(start.toLocaleDateString(getLocale(), { weekday: 'long' }))

    fetchDays = false

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
            <h4>${t('pm10')}: ${sensor.readings[0].value} µg/m3</h4>
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
            'text-offset': [0, 1],
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
              25,
              '#ffff00',
              50,
              '#ff0000',
            ],
            'circle-radius': 10,
            'circle-stroke-width': 2,
            'circle-stroke-color': '#ffffff',
          },
        },
      ],
    }
  }

  reload()
  watch(getLocale, reload)
}


fetchLocations()
fetchStats(appStore.favorites.pm10)
fetchWeeklyChart(appStore.favorites.pm10)
fetchYearlyExceedances()
fetchMap()


</script>


<style scoped>

.chart {
  width: 100%;
  height: 40em;
}
</style>
