<template>
  <v-container>
    <h1>Temp View</h1>
    <div v-if='stats'>
      <div>
        <v-autocomplete
          v-model='statsSelectedModules'
          :chips='true'
          :multiple='true'
          name='id'
          item-title='name'
          item-value='id'
          class='ml-auto mr-0'
          label='Select'
          :items='availableModules'
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
            <SheetInfo
              :title='module.current + " °C"'
              subtitle='Aktualne'
            />
          </v-col>
          <v-col
            cols='auto'
          >
            <SheetInfo
              icon='mdi-thermometer-high'
              :title='module.max + " °C"'>
              <template #subtitle>
                <span>Maximálna</span>
                <span>teplota dnes</span>
              </template>
            </SheetInfo>
          </v-col>
          <v-col
            cols='auto'
          >
            <SheetInfo
              icon='mdi-thermometer-low'
              :title='module.min + " °C"'>
              <template #subtitle>
                <span>Minimálna</span>
                <span>teplota dnes</span>
              </template>
            </SheetInfo>
          </v-col>
        </v-row>
      </div>
    </div>

    <comparison-settings :available-modules='availableModules' @update='fetchComparisonChart'/>
    <div ref='comparisonChart' class='chart mt-12' />
    <div ref='mapContainer' class='map-container mt-12' />
  </v-container>
</template>

<script setup>
import { ky } from '@/lib/ky'
import { onMounted, ref } from 'vue'
import SheetInfo from '@/components/SheetInfo.vue'
import * as echarts from 'echarts'
import ComparisonSettings from '@/components/ComparisonSettings.vue'
import mapboxgl from 'mapbox-gl'
import 'mapbox-gl/dist/mapbox-gl.css'
import { mapboxToken } from '@/lib/constants'

const availableModules = ref([])
const statsSelectedModules = ref([])
const stats = ref(null)
const comparisonChart = ref(null)
const mapContainer = ref(null)

const fetchStats = async (ids) => {
  stats.value = await ky.post('temp/stats', {
    json: {
      modules: ids,
    },
  }).json()

  statsSelectedModules.value = stats.value.modules.map((module) => module.module.id)
}

const fetchLocations = async () => {
  availableModules.value = await ky.get('temp/locations').json()
}

const fetchComparisonChart = async (configure) => {
  let json = {}

  if (configure)
  {
    json = {
      ...configure,
    }
  }

  const chart = echarts.init(comparisonChart.value)
  chart.clear()

  chart.showLoading()

  const res = await ky.post('temp', {
    json
  }).json()

  // weeklyComparisonSelectedModules.value = res.modules.map((module) => module.id)

  const readingDates = res.modules[0].readings.map((reading) => new Date(reading.dateTime).toLocaleString('sk'))

  const series = res.modules.map((module) => {
    return {
      name: module.name,
      type: 'line',
      connectNulls: false,
      data: module.readings.map((reading) => reading.value),
    }
  })

  chart.hideLoading()


  chart.setOption({
    title: {
      text: 'Porovnanie teplôt',
      top: 10
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
    dataZoom: [
      {
        type: 'inside',
      },
      {
        type: 'slider',
      },
    ],
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: readingDates,
    },
    yAxis: {
      type: 'value',
      axisLabel: {
        formatter: '{value} °C',
      },
    },
    series: series,
  })

  window.addEventListener('resize', () => {
    chart.resize()
  })
}

const fetchMap = async () => {
  const mapResponse = await ky.get('temp/map').json()

  const toDayString = (date) => {
    return new Date(date.getTime() - (date.getTimezoneOffset() * 60000)).toLocaleString('sk')
  }

  const features = mapResponse.map((module) => {
    return {
      'type': 'Feature',
      'properties': {
        'description':
          `<div>
            <h3>${module.name}</h3>
            <h4>Teplota: ${module.readings[0].value} °C</h4>
            <p>${toDayString(new Date(module.readings[0].dateTime))}</p>
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
          -20,
          '#00f',
          0,
          '#07a7e7',
          10,
          '#c3f800',
          20,
          '#f8a600',
          30,
          '#e01919',
          35,
          '#930202',
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
}

fetchLocations()
fetchStats()

onMounted(() => {
  fetchComparisonChart()
  fetchMap()
})

</script>


<style scoped>
.chart {
  width: 100%;
  height: 40em;
}

.map-container {
  width: 100%;
  height: 500px;
}
</style>
