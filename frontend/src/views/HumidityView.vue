<template>
  <v-container>
    <h1>{{ $t('humidity.humidity') }}</h1>
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
              :title='sensor.current + " %"'
              :subtitle='$t("current")'
            />
          </v-col>
          <v-col
            cols='auto'
          >
            <SheetInfo
              icon='mdi-thermometer-high'
              :title='sensor.max + " %"'
              :subtitle='$t("humidity.maxToday")'
            />
          </v-col>
          <v-col
            cols='auto'
          >
            <SheetInfo
              icon='mdi-thermometer-low'
              :title='sensor.min + " %"'
              :subtitle='$t("humidity.minToday")'
            />
          </v-col>
        </v-row>
      </div>
    </div>

    <v-divider class='my-5' />

    <div class='d-flex justify-space-between align-center'>
      <h3>{{ $t("humidity.comparison") }}</h3>
      <comparison-settings :available-sensors='availableSensors' @update='fetchComparisonChart' />
    </div>


    <ComparisonChart
      :sensors='comparisonChart.sensors'
      :loading='comparisonChart.loading'
      unit='%'
      :zoom='true'
      :display-time='true'
    />

    <v-divider class='my-5' />

    <LegendComponent
      :limits='[
        {
          value: 0,
          color: "#ffffff",
        },
        {
          value: 100,
          color: "#002dff",
        },
      ]'
      unit='%'
      :value='gaugeValue'
    />

    <MapComponent
      :layers='map.layers'
      :features='map.features'
      @enter='gaugeValue = $event'
      @leave='gaugeValue = null'
    />
  </v-container>
</template>

<script setup>
import { ky } from '@/lib/ky'
import { onMounted, ref, watch } from 'vue'
import SheetInfo from '@/components/SheetInfo.vue'
import ComparisonSettings from '@/components/ComparisonSettings.vue'
import 'mapbox-gl/dist/mapbox-gl.css'
import ComparisonChart from '@/components/ComparisonChart.vue'
import { getLocale, t } from '@/lib/i18n'
import MapComponent from '@/components/MapComponent.vue'
import { useAppStore } from '@/store/app'
import LegendComponent from '@/components/LegendComponent.vue'

const appStore = useAppStore()
const availableSensors = ref([])
const statsSelectedSensors = ref([])
const stats = ref(null)
const gaugeValue = ref(null)
const comparisonChart = ref({
  sensors: [],
  loading: false,
})
const map = ref({
  layers: [],
  features: [],
})

const fetchStats = async (ids) => {
  stats.value = await ky.post('humidity/stats', {
    json: {
      sensors: ids,
    },
  }).json()

  statsSelectedSensors.value = stats.value.sensors.map((sensor) => sensor.sensor.id)
}

const fetchLocations = async () => {
  availableSensors.value = await ky.get('humidity').json()
}

const fetchComparisonChart = async (configure) => {
  let json = {}

  if (configure) {
    json = {
      ...configure,
    }
  }

  comparisonChart.value.loading = true

  const res = await ky.post('humidity', {
    json,
  }).json()

  comparisonChart.value = {
    sensors: res.sensors,
    loading: false,
  }
}

const fetchMap = async () => {
  const mapResponse = await ky.get('humidity/map').json()

  const features = mapResponse.map((sensor) => {
    return {
      'type': 'Feature',
      'properties': {
        'description':
          `<div>
            <h3>${sensor.name}</h3>
            <h4>${t('humidity.humidity')}: ${sensor.readings[0].value} %</h4>
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

  const reload = () => {
    map.value = {
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
              '#ffffff',
              100,
              '#002dff',
            ],
            'circle-radius': 10,
            'circle-stroke-width': 2,
            'circle-stroke-color': '#ffffff',
          },
        },
      ],
      features,
    }
  }
  reload()

  watch(getLocale(), reload)
}

fetchLocations()
fetchStats(appStore.favorites.humidity)

onMounted(() => {
  fetchComparisonChart({
    sensors: appStore.favorites.humidity,
  })
  fetchMap()
})

</script>


<style scoped>


</style>
