<template>
  <v-container>
    <h1>PM25View</h1>
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
          label='Select'
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
            >
              <template #subtitle>
                <span>Prekročených dní</span>
                <span>za tento rok</span>
              </template>
            </SheetInfo>
          </v-col>
          <v-col
            cols='auto'
          >
            <SheetInfo
              :title='sensor.current'
              unit='µg/m³'
              subtitle='Aktuálne'
            />
          </v-col>
          <v-col
            cols='auto'
          >
            <SheetInfo
              :title='sensor.yearValueAvg'
              unit='µg/m³'
            >
              <template #subtitle>
                <span>Priemer</span>
                <span>Za posledný rok</span>
              </template>
            </SheetInfo>
          </v-col>
          <v-col
            cols='auto'
          >
            <SheetInfo
              :title='sensor.dayValueAvg'
              unit='µg/m³'
            >
              <template #subtitle>
                <span>Priemer</span>
                <span>Za den</span>
              </template>
            </SheetInfo>
          </v-col>
        </v-row>
      </div>
    </div>

    <ComparisonFilter
      :available-sensors='availableSensors'
      @update='fetchWeeklyComparison'
    />
    <div ref='lineChart' class='chart mt-12' />
    <div ref='exceedChart' class='chart mt-12' />
    <div class='d-flex justify-center flex-column align-center'>
      <h2>Porovnanie medzi týžnami</h2>
      <compare-chart-filter :available-sensors='availableSensors' @update='fetchComparisonChart' />

      <div v-if='showComparisonChart' ref='comparisonChart' class='chart mt-12' />
    </div>
    <v-divider class='mt-12' />
    <div ref='mapContainer' class='map-container mt-12' />

  </v-container>
</template>


<script setup>

import { ref, onMounted } from 'vue'
import * as echarts from 'echarts'
import mapboxgl from 'mapbox-gl'
import 'mapbox-gl/dist/mapbox-gl.css'
import { ky } from '@/lib/ky'
import CompareChartFilter from '@/components/CompareChartFilter.vue'
import { mapboxToken } from '@/lib/constants'
import SheetInfo from '@/components/SheetInfo.vue'
import ComparisonFilter from '@/components/ComparisonFilter.vue'

const lineChart = ref(null)
const exceedChart = ref(null)
const comparisonChart = ref(null)
const mapContainer = ref(null)
const stats = ref(null)
const statsSelectedSensors = ref([])
const weeklyComparisonSelectedSensors = ref([])
const compareChartDialog = ref(false)
const showComparisonChart = ref(false)
const availableSensors = ref([])

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

const fetchWeeklyComparison = async (ids) => {
  const res = await ky.post('pm25', {
    json: {
      Sensors: ids,
      From: new Date(new Date().getFullYear(), 0, 1).toISOString(),
      To: new Date().toISOString(),
      Interval: 3,
    },
  }).json()

  weeklyComparisonSelectedSensors.value = res.sensors.map((sensor) => sensor.id)

  const readingDates = res.sensors[0].readings.map((reading) => new Date(reading.dateTime).toLocaleDateString('sk'))

  const series = res.sensors.map((sensor) => {
    return {
      name: sensor.name,
      type: 'line',
      connectNulls: false,
      data: sensor.readings.map((reading) => reading.value),
    }
  })


  const chart = echarts.init(lineChart.value)
  chart.clear()


  chart.setOption({
    title: {
      text: 'Znečistenie vzduchu po týždňoch',
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
        formatter: '{value} µg/m3',
      },
    },
    series: series,
  })

  window.addEventListener('resize', () => {
    chart.resize()
  })
}

const fetchYearlyExceedances = async () => {
  const res = await ky.get('pm25/exceed').json()

  const sensors = []
  const values = []

  res.forEach(item => {
    const sensorName = item.sensor.name
    const exceedance = item.exceed

    sensors.push(sensorName)
    values.push(exceedance)
  })

  const series = [{
    name: 'Počet prekročení',
    type: 'bar',
    data: values,
  }]

  const chart = echarts.init(exceedChart.value)
  chart.clear()

  chart.setOption({
    title: {
      text: 'Prekročenia limitu PM 2.5 za posledných 365 dní',
    },
    tooltip: {
      trigger: 'axis',
    },
    legend: {
      data: ['Počet prekročení'],
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

  window.addEventListener('resize', () => {
    chart.resize()
  })
}

const fetchComparisonChart = async (options) => {
  compareChartDialog.value = false
  showComparisonChart.value = true
  const res = await ky.post('pm25/compare', {
    json: {
      Sensors: options.sensors,
      weekDays: options.days,
      hours: options.hours,
      weeks: options.weeks,
    },
  }).json()

  const categories = res[0].readings.map((reading) => new Date(reading.dateTime).toLocaleString('sk'))

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

      const categoryDate = start.toLocaleDateString('sk', { weekday: 'long' })
      if (days.indexOf(categoryDate) === -1) {
        days.push(categoryDate)
      }

      if (i === dates.length - 1)
        break

      end = dates[i - 1]


      areas.push({
        xAxis: end.toLocaleString('sk'),
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
      text: 'Porovnanie znečistenia vzduchu',
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

fetchLocations()
fetchStats()
fetchWeeklyComparison()
fetchYearlyExceedances()

onMounted(async () => {
  const mapResponse = await ky.get('pm25/map').json()

  const features = mapResponse.map((sensor) => {
    return {
      'type': 'Feature',
      'properties': {
        'description':
          `<div>
            <h3>${sensor.name}</h3>
            <h4>PM 2.5: ${sensor.readings[0].value} µg/m3</h4>
            <p>${new Date(sensor.readings[0].dateTime).toLocaleString('sk')}</p>
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

  // const firstCoordinates = features[0].geometry.coordinates

  mapboxgl.accessToken = mapboxToken
  const map = new mapboxgl.Map({
    container: mapContainer.value,
    style: 'mapbox://styles/mapbox/streets-v12',
    center: [19, 48.7],
    zoom: 7,
  })

  map.on('load', () => {
    map.addSource('places', {
      'type': 'geojson',
      'data': {
        'type': 'FeatureCollection',
        'features': features,
      },
    })

    map.addLayer({
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
    })

    map.addLayer({
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
    })

    const popup = new mapboxgl.Popup({
      closeButton: false,
      closeOnClick: false,
    })

    map.on('mouseenter', 'places', (e) => {
      map.getCanvas().style.cursor = 'pointer'

      const coordinates = e.features[0].geometry.coordinates.slice()
      const description = e.features[0].properties.description

      while (Math.abs(e.lngLat.lng - coordinates[0]) > 180) {
        coordinates[0] += e.lngLat.lng > coordinates[0] ? 360 : -360
      }

      popup.setLngLat(coordinates).setHTML(description).addTo(map)
    })

    map.on('mouseleave', 'places', () => {
      map.getCanvas().style.cursor = ''
      popup.remove()
    })

    map.addControl(
      new mapboxgl.GeolocateControl({
        positionOptions: {
          enableHighAccuracy: false,
        },
        trackUserLocation: false,
        showUserHeading: true,
      }),
    )
  })


})


</script>


<style scoped>
.title {
  font-size: 2rem;
  font-weight: 500;
  line-height: 1.2;
  margin-bottom: 0.5rem;
}

sub {
  font-size: initial;
}

.chart {
  width: 100%;
  height: 40em;
}

.map-container {
  width: 100%;
  height: 500px;
}

</style>
