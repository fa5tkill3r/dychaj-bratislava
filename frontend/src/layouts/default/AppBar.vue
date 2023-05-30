<template>
  <v-app-bar
    :flat='true'
  >
    <v-app-bar-title>
      <v-icon icon="mdi-circle-slice-6" />
      {{ $t('app.title') }}
    </v-app-bar-title>

    <v-spacer />

    <v-btn
      to="/pm25"
    >
      PM 2.5
    </v-btn>
    <v-btn
    <v-btn
      to="/temp"
    >
      Temperature
    </v-btn>

  </v-app-bar>
</template>

<script setup>
  //
</script>
