<template>
  <v-app-bar
    :flat='true'
  >
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

    <v-btn
      to="/pm25"
    >
      {{ $t('pm25') }}
    </v-btn>
    <v-btn
      to="/pm10"
    >
      {{ $t('pm10') }}
    </v-btn>
    <v-btn
      to="/temp"
    >
      {{ $t('temperature') }}
    </v-btn>
    <v-btn
      to="/humidity"
    >
      {{ $t('humidity.humidity') }}
    </v-btn>
    <v-btn
      to="/pressure"
    >
      {{ $t('pressure.pressure') }}
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


  </v-app-bar>
</template>

<script setup>
import { i18n } from '@/lib/i18n'

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


</script>
