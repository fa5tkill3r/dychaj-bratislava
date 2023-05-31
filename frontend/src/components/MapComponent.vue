<template>
  <div ref='mapContainer' class='map-container mt-12' />
</template>


<script setup>
import { onMounted, ref, watch } from 'vue'
import mapboxgl from 'mapbox-gl'
import { mapboxToken } from '@/lib/constants'
import { getLocale } from '@/lib/i18n'

const mapContainer = ref(null)
let map

const props = defineProps({
  features: {
    type: Array,
    required: true,
  },
  layers: {
    type: Array,
    required: true,
  },
})

watch(() => props.features, () => {
  if (map) {
    map.remove()
  }

  fetchMap()
})

watch(() => props.layers, () => {
  if (map) {
    map.remove()
  }

  fetchMap()
})

watch(() => getLocale(), () => {
  if (map) {
    map.remove()
  }

  fetchMap()
})

const fetchMap = () => {
  mapboxgl.accessToken = mapboxToken
  map = new mapboxgl.Map({
    container: mapContainer.value,
    style: 'mapbox://styles/mapbox/streets-v12',
    center: [19, 48.7],
    zoom: 7,
  })

  if (props.layers.length === 0 || props.features.length === 0) {
    return
  }

  map.on('load', () => {
    map.addSource('places', {
      type: 'geojson',
      data: {
        type: 'FeatureCollection',
        features: props.features,
      },
    })

    props.layers.forEach((layer) => {
      map.addLayer(layer)
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


onMounted(() => {
  fetchMap()
})


</script>

<style scoped>
.map-container {
  width: 100%;
  height: 500px;
}
</style>
