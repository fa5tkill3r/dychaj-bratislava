<template>
  <v-container>
    <h1>{{ $t('pressure.pressure') }}</h1>
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
              :title='Math.round(sensor.current/100) + " hPa"'
              :subtitle='$t("current")'
            />
          </v-col>
          <v-col
            cols='auto'
          >
            <SheetInfo
              icon='mdi-thermometer-high'
              :title='Math.round(sensor.max/100) + " hPa"'
              :subtitle='$t("pressure.maxToday")'
            />
          </v-col>
          <v-col
            cols='auto'
          >
            <SheetInfo
              icon='mdi-thermometer-low'
              :title='Math.round(sensor.min/100) + " hPa"'
              :subtitle='$t("pressure.minToday")'
            />
          </v-col>
        </v-row>
      </div>
    </div>
    <v-divider class='my-5' />

    <div class='d-flex align-center justify-space-between'>
      <h3>{{ $t('pressure.comparison') }}</h3>
      <comparison-settings :available-sensors='availableSensors' @update='fetchComparisonChart' />
    </div>

    <ComparisonChart
      :sensors='comparisonChart.sensors'
      :loading='comparisonChart.loading'
      unit='pa'
      :zoom='true'
      :display-time='true'
      :connect-nulls='true'
      :scale='{
        scale: "auto",
      }'
    />

    <v-divider class='my-5'/>

    <MapComponent
      :layers='map.layers'
      :features='map.features'
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

const appStore = useAppStore()
const availableSensors = ref([])
const statsSelectedSensors = ref([])
const stats = ref(null)
const comparisonChart = ref({
  sensors: [],
  loading: false,
})
const map = ref({
  layers: [],
  features: [],
})

const fetchStats = async (ids) => {
  stats.value = await ky.post('pressure/stats', {
    json: {
      sensors: ids,
    },
  }).json()

  statsSelectedSensors.value = stats.value.sensors.map((sensor) => sensor.sensor.id)
}

const fetchLocations = async () => {
  availableSensors.value = await ky.get('pressure').json()
}

const fetchComparisonChart = async (configure) => {
  let json = {}

  if (configure) {
    json = {
      ...configure,
    }
  }

  comparisonChart.value.loading = true

  const res = await ky.post('pressure', {
    json,
  }).json()

  comparisonChart.value = {
    sensors: res.sensors,
    loading: false,
  }
}

const fetchMap = async () => {
  const mapResponse = await ky.get('pressure/map').json()

  const features = mapResponse.map((sensor) => {
    return {
      'type': 'Feature',
      'properties': {
        'description':
          `<div>
            <h3>${sensor.name}</h3>
            <h4>${t('pressure.pressure')}: ${Math.round(sensor.readings[0].value / 100)} hPa</h4>
            <p>${new Date(sensor.readings[0].dateTime).toLocaleString(getLocale())}</p>
            <p>${sensor.location?.address}</p>
          </div>`,
        'value': Math.round(sensor.readings[0].value / 100),
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
              '#00ff00',
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
fetchStats(appStore.favorites.pressure)

onMounted(() => {
  fetchComparisonChart({
    sensors: appStore.favorites.pressure,
  })
  fetchMap()
})

</script>


<style scoped>


</style>
