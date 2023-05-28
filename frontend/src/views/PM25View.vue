<template>
  <v-container>
    <h1>PM25View</h1>
    <div v-if='stats'>
      <div>
        <v-autocomplete
          v-model='selectedModules'
          :chips='true'
          :multiple='true'
          name='id'
          item-title='name'
          item-value='id'
          class='ml-auto mr-0'
          label='Select'
          :items='stats.availableModules'
          @update:model-value='fetchStats'
        ></v-autocomplete>
      </div>
      <div
        v-for='module in stats.modules'
        :key='module.module.id'
      >
        <h3>{{ module.module.name }}</h3>
        <v-row
          justify='center'
        >
          <v-col
            cols='auto'
          >
            <v-sheet
              elevation='4'
              rounded
              width='150'
              height='150'
            >
              <div class='d-flex flex-column justify-center align-center fill-height'>
                <span class='title'>{{ module.daysAboveThreshold }}</span>
                <span>prekročených dní</span>
                <span>Za tento rok</span>
              </div>
            </v-sheet>
          </v-col>
          <v-col
            cols='auto'
          >
            <v-sheet
              elevation='4'
              rounded
              width='150'
              height='150'
            >
              <div class='d-flex flex-column justify-center align-center fill-height'>
                <span class='title'>{{ module.current }} <sub>µg/m3</sub> </span>
                <span>Aktualne</span>
              </div>
            </v-sheet>
          </v-col>
          <v-col
            cols='auto'
          >
            <v-sheet
              elevation='4'
              rounded
              width='150'
              height='150'
            >
              <div class='d-flex flex-column justify-center align-center fill-height'>
                <span class='title'>{{ module.yearValueAvg }} <sub>µg/m3</sub> </span>
                <span>Priemer</span>
                <span>Za posledný rok</span>
              </div>
            </v-sheet>
          </v-col>
          <v-col
            cols='auto'
          >
            <v-sheet
              elevation='4'
              rounded
              width='150'
              height='150'
            >
              <div class='d-flex flex-column justify-center align-center fill-height'>
                <span class='title'>{{ module.dayValueAvg }} <sub>µg/m3</sub> </span>
                <span>Priemer</span>
                <span>Za den</span>
              </div>
            </v-sheet>
          </v-col>
        </v-row>
      </div>
    </div>


    <div class='d-flex'>
      <v-btn
        class='ml-auto mr-0'
        color='primary'
        @click='dialog = true'
      >
        <v-icon>mdi-filter</v-icon>
      </v-btn>
    </div>

    <div ref='lineChart' class='chart mt-12' />
    <div ref='exceedChart' class='chart mt-12'/>
    <div ref='mapContainer' class='map-container mt-12' />

  </v-container>
  <v-dialog
    v-model='dialog'
    width='auto'
  >
    <filter-component
      :tags='stats?.availableModules' :selected-tags='weeklyComparisonSelectedModules'
      @update:tags='fetchWeeklyComparison'></filter-component>

  </v-dialog>
</template>


<script setup>

import { ref, onMounted, computed, watch } from 'vue'
import * as echarts from 'echarts'
import mapboxgl from 'mapbox-gl'
import 'mapbox-gl/dist/mapbox-gl.css'
import { ky } from '@/lib/ky'
import FilterComponent from '@/components/FilterComponent.vue'

const lineChart = ref(null)
const exceedChart = ref(null)
const mapContainer = ref(null)
const stats = ref(null)
const selectedModules = ref([])
const weeklyComparisonSelectedModules = ref([])
const dialog = ref(false)

const fetchStats = async (ids) => {
  stats.value = await ky.post('pm25/stats', {
    json: {
      modules: ids,
    },
  }).json()

  selectedModules.value = stats.value.modules.map((module) => module.module.id)
}

const fetchWeeklyComparison = async (ids) => {
  dialog.value = false

  const res = await ky.post('pm25/weekly', {
    json: {
      modules: ids,
    },
  }).json()

  weeklyComparisonSelectedModules.value = res.modules.map((module) => module.id)

  const readingDates = res.modules[0].readings.map((reading) => new Date(reading.dateTime).toLocaleDateString('sk'))

  const series = res.modules.map((module) => {
    return {
      name: module.name,
      type: 'line',
      connectNulls: false,
      data: module.readings.map((reading) => reading.value),
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
    const sensorName = item.module.name
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


fetchStats()
fetchWeeklyComparison()
fetchYearlyExceedances()

onMounted(async () => {
  const mapResponse = await ky.get('pm25/map').json()

  const features = mapResponse.map((module) => {
    return {
      'type': 'Feature',
      'properties': {
        'description':
          `<div>
            <h3>${module.name}</h3>
            <h4>PM 2.5: ${module.readings[0].value} µg/m3</h4>
            <p>${module.location?.address}</p>
          </div>`,
        'value': module.readings[0].value,
      },
      'geometry': {
        'type': 'Point',
        'coordinates': [module.location?.longitude, module.location?.latitude],
      },
    }
  })

  // const firstCoordinates = features[0].geometry.coordinates

  mapboxgl.accessToken = 'pk.eyJ1IjoiZmFzdGtpbGxlciIsImEiOiJjbGI0YW5hbnYwbWVmM3BweGdudTAxb2FpIn0.wwCSm3SksqjjGOwYiqS-jQ'
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
