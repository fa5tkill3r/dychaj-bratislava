<template>
  <v-app-bar
    :flat='true'
  >
    <v-app-bar-nav-icon
      class='d-sm-none'
      @click.stop='drawer = !drawer'
    />
    <v-img
      class='ml-2'
      src='/favicon.svg'
      max-height='48'
      max-width='48'
      contain
    />
    <v-app-bar-title>
      {{ $t('app.title') }}
    </v-app-bar-title>

    <v-spacer />

    <div class='d-none d-sm-flex align-center'>
      <v-btn
        v-for='(item, index) in items'
        :key='index'
        :to='item.to'
      >
        {{ $t(item.title) }}
      </v-btn>

      <v-menu offset-y>
        <template #activator="{ props }">
          <v-btn
            v-bind='props'
            icon>
            <v-icon>mdi-translate</v-icon>
          </v-btn>
        </template>
        <v-list>
          <v-list-item v-for="language in languages" :key="language.code" @click="selectLanguage(language)">
            <v-list-item-title>{{ language.name }}</v-list-item-title>
          </v-list-item>
        </v-list>
      </v-menu>
    </div>

  </v-app-bar>
  <v-navigation-drawer
    v-model='drawer'
    :absolute='true'
    :temporary='true'
  >
    <v-list
      :nav='true'
    >
      <v-list-item
        v-for='(item, index) in items'
        :key='index'
        :to='item.to'
        @click='drawer = false'
      >
        <template #prepend>
          <v-icon>{{ item.icon }}</v-icon>
        </template>
        <v-list-item-title>
          {{ $t(item.title) }}
        </v-list-item-title>
      </v-list-item>
    </v-list>

  </v-navigation-drawer>
</template>

<script setup>
import { i18n } from '@/lib/i18n'
import { ref } from 'vue'

const drawer = ref(false)

const languages = [
  {
    code: 'en',
    name: 'English'
  },
  {
    code: 'sk',
    name: 'Slovenčina'
  }
]

const selectLanguage = (language) => {
  i18n.global.locale = language.code
}

const items = [
  {
    icon: 'mdi-chart-bar',
    title: 'pm25',
    to: '/pm25'
  },
  {
    icon: 'mdi-chart-bar',
    title: 'pm10',
    to: '/pm10'
  },
  {
    icon: 'mdi-thermometer',
    title: 'temperature',
    to: '/temp'
  },
  {
    icon: 'mdi-cloud-percent',
    title: 'humidity.humidity',
    to: '/humidity'
  },
  {
    icon: 'mdi-chart-bar',
    title: 'pressure.pressure',
    to: '/pressure'
  }
]

</script>


<style>
.v-toolbar-title__placeholder{
  width: 200px !important;
}
</style>

