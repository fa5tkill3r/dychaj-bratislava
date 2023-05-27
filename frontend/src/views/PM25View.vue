<template>
  <v-container>
    <h1>PM25View</h1>
    <div v-if='stats'>
      <div>
        <v-autocomplete
          v-model='selectedModules'
          chips
          multiple
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
                <span class='title'>10</span>
                <span>prekročených hodín</span>
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


    <div ref='lineChart' class='chart mt-12'></div>
    <div ref='mapContainer' class='map-container mt-12'></div>
  </v-container>
</template>


<script setup>

import { ref, onMounted, computed, watch } from 'vue'
import * as echarts from 'echarts'
import mapboxgl from 'mapbox-gl'
import 'mapbox-gl/dist/mapbox-gl.css'
import { ky } from '@/lib/ky'

const lineChart = ref(null)
const mapContainer = ref(null)
const stats = ref(null)
const selectedModules = ref([])

const fetchStats = async (ids) => {
  stats.value = await ky.post('pm25/stats', {
    json: {
      modules: ids,
    },
  }).json()

  selectedModules.value = stats.value.modules.map((module) => module.module.id)
}

fetchStats()


const initChart = () => {
  const chart = echarts.init(lineChart.value)
  chart.resize()


  chart.setOption({
    title: {
      text: 'Stacked Line',
    },
    tooltip: {
      trigger: 'axis',
    },
    legend: {
      data: ['Email', 'Union Ads', 'Video Ads', 'Direct', 'Search Engine'],
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
      data: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
    },
    yAxis: {
      type: 'value',
    },
    series: [
      {
        name: 'Email',
        type: 'line',
        stack: 'Total',
        connectNulls: true,
        data: [120, null, 101, 134, 90, 230, 210],
      },
      {
        name: 'Union Ads',
        type: 'line',
        stack: 'Total',
        data: [220, 182, 191, 234, 290, 330, 310],
      },
      {
        name: 'Video Ads',
        type: 'line',
        stack: 'Total',
        data: [150, 232, 201, 154, 190, 330, 410],
      },
      {
        name: 'Direct',
        type: 'line',
        stack: 'Total',
        data: [320, 332, 301, 334, 390, 330, 320],
      },
      {
        name: 'Search Engine',
        type: 'line',
        stack: 'Total',
        data: [820, 932, 901, 934, 1290, 1330, 1320],
      },
    ],
  })

  window.addEventListener('resize', () => {
    chart.resize()
  })
}

onMounted(() => {
  initChart()
  mapboxgl.accessToken = 'pk.eyJ1IjoiZmFzdGtpbGxlciIsImEiOiJjbGI0YW5hbnYwbWVmM3BweGdudTAxb2FpIn0.wwCSm3SksqjjGOwYiqS-jQ'
  const map = new mapboxgl.Map({
    container: mapContainer.value,
// Choose from Mapbox's core styles, or make your own style with Mapbox Studio
    style: 'mapbox://styles/mapbox/streets-v12',
    center: [-77.04, 38.907],
    zoom: 11.15,
  })

  map.on('load', () => {
    map.addSource('places', {
// This GeoJSON contains features that include an "icon"
// property. The value of the "icon" property corresponds
// to an image in the Mapbox Streets style's sprite.
      'type': 'geojson',
      'data': {
        'type': 'FeatureCollection',
        'features': [
          {
            'type': 'Feature',
            'properties': {
              'description':
                '<strong>Make it Mount Pleasant</strong><p><a href="http://www.mtpleasantdc.com/makeitmtpleasant" target="_blank" title="Opens in a new window">Make it Mount Pleasant</a> is a handmade and vintage market and afternoon of live entertainment and kids activities. 12:00-6:00 p.m.</p>',
              'icon': 'theatre',
            },
            'geometry': {
              'type': 'Point',
              'coordinates': [-77.038659, 38.931567],
            },
          },
          {
            'type': 'Feature',
            'properties': {
              'description':
                '<strong>Mad Men Season Five Finale Watch Party</strong><p>Head to Lounge 201 (201 Massachusetts Avenue NE) Sunday for a <a href="http://madmens5finale.eventbrite.com/" target="_blank" title="Opens in a new window">Mad Men Season Five Finale Watch Party</a>, complete with 60s costume contest, Mad Men trivia, and retro food and drink. 8:00-11:00 p.m. $10 general admission, $20 admission and two hour open bar.</p>',
              'icon': 'theatre',
            },
            'geometry': {
              'type': 'Point',
              'coordinates': [-77.003168, 38.894651],
            },
          },
          {
            'type': 'Feature',
            'properties': {
              'description':
                '<strong>Big Backyard Beach Bash and Wine Fest</strong><p>EatBar (2761 Washington Boulevard Arlington VA) is throwing a <a href="http://tallulaeatbar.ticketleap.com/2012beachblanket/" target="_blank" title="Opens in a new window">Big Backyard Beach Bash and Wine Fest</a> on Saturday, serving up conch fritters, fish tacos and crab sliders, and Red Apron hot dogs. 12:00-3:00 p.m. $25.grill hot dogs.</p>',
              'icon': 'bar',
            },
            'geometry': {
              'type': 'Point',
              'coordinates': [-77.090372, 38.881189],
            },
          },
          {
            'type': 'Feature',
            'properties': {
              'description':
                '<strong>Ballston Arts & Crafts Market</strong><p>The <a href="http://ballstonarts-craftsmarket.blogspot.com/" target="_blank" title="Opens in a new window">Ballston Arts & Crafts Market</a> sets up shop next to the Ballston metro this Saturday for the first of five dates this summer. Nearly 35 artists and crafters will be on hand selling their wares. 10:00-4:00 p.m.</p>',
              'icon': 'art-gallery',
            },
            'geometry': {
              'type': 'Point',
              'coordinates': [-77.111561, 38.882342],
            },
          },
          {
            'type': 'Feature',
            'properties': {
              'description':
                '<strong>Seersucker Bike Ride and Social</strong><p>Feeling dandy? Get fancy, grab your bike, and take part in this year\'s <a href="http://dandiesandquaintrelles.com/2012/04/the-seersucker-social-is-set-for-june-9th-save-the-date-and-start-planning-your-look/" target="_blank" title="Opens in a new window">Seersucker Social</a> bike ride from Dandies and Quaintrelles. After the ride enjoy a lawn party at Hillwood with jazz, cocktails, paper hat-making, and more. 11:00-7:00 p.m.</p>',
              'icon': 'bicycle',
            },
            'geometry': {
              'type': 'Point',
              'coordinates': [-77.052477, 38.943951],
            },
          },
          {
            'type': 'Feature',
            'properties': {
              'description':
                '<strong>Capital Pride Parade</strong><p>The annual <a href="http://www.capitalpride.org/parade" target="_blank" title="Opens in a new window">Capital Pride Parade</a> makes its way through Dupont this Saturday. 4:30 p.m. Free.</p>',
              'icon': 'rocket',
            },
            'geometry': {
              'type': 'Point',
              'coordinates': [-77.043444, 38.909664],
            },
          },
          {
            'type': 'Feature',
            'properties': {
              'description':
                '<strong>Muhsinah</strong><p>Jazz-influenced hip hop artist <a href="http://www.muhsinah.com" target="_blank" title="Opens in a new window">Muhsinah</a> plays the <a href="http://www.blackcatdc.com">Black Cat</a> (1811 14th Street NW) tonight with <a href="http://www.exitclov.com" target="_blank" title="Opens in a new window">Exit Clov</a> and <a href="http://godsilla.bandcamp.com" target="_blank" title="Opens in a new window">Gods’illa</a>. 9:00 p.m. $12.</p>',
              'icon': 'music',
            },
            'geometry': {
              'type': 'Point',
              'coordinates': [-77.031706, 38.914581],
            },
          },
          {
            'type': 'Feature',
            'properties': {
              'description':
                '<strong>A Little Night Music</strong><p>The Arlington Players\' production of Stephen Sondheim\'s  <a href="http://www.thearlingtonplayers.org/drupal-6.20/node/4661/show" target="_blank" title="Opens in a new window"><em>A Little Night Music</em></a> comes to the Kogod Cradle at The Mead Center for American Theater (1101 6th Street SW) this weekend and next. 8:00 p.m.</p>',
              'icon': 'music',
            },
            'geometry': {
              'type': 'Point',
              'coordinates': [-77.020945, 38.878241],
            },
          },
          {
            'type': 'Feature',
            'properties': {
              'description':
                '<strong>Truckeroo</strong><p><a href="http://www.truckeroodc.com/www/" target="_blank">Truckeroo</a> brings dozens of food trucks, live music, and games to half and M Street SE (across from Navy Yard Metro Station) today from 11:00 a.m. to 11:00 p.m.</p>',
              'icon': 'music',
            },
            'geometry': {
              'type': 'Point',
              'coordinates': [-77.007481, 38.876516],
            },
          },
        ],
      },
    })
// Add a layer showing the places.
    map.addLayer({
      'id': 'places',
      'type': 'symbol',
      'source': 'places',
      'layout': {
        'icon-image': ['get', 'icon'],
        'icon-allow-overlap': true,
      },
    })

// When a click event occurs on a feature in the places layer, open a popup at the
// location of the feature, with description HTML from its properties.
    map.on('click', 'places', (e) => {
// Copy coordinates array.
      const coordinates = e.features[0].geometry.coordinates.slice()
      const description = e.features[0].properties.description

// Ensure that if the map is zoomed out such that multiple
// copies of the feature are visible, the popup appears
// over the copy being pointed to.
      while (Math.abs(e.lngLat.lng - coordinates[0]) > 180) {
        coordinates[0] += e.lngLat.lng > coordinates[0] ? 360 : -360
      }

      new mapboxgl.Popup()
        .setLngLat(coordinates)
        .setHTML(description)
        .addTo(map)
    })

// Change the cursor to a pointer when the mouse is over the places layer.
    map.on('mouseenter', 'places', () => {
      map.getCanvas().style.cursor = 'pointer'
    })

// Change it back to a pointer when it leaves.
    map.on('mouseleave', 'places', () => {
      map.getCanvas().style.cursor = ''
    })
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
