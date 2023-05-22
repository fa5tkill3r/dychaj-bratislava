<template>
  <v-main>
    <router-view />
    <v-footer
      color='grey-lighten-4'
    >
      <div class='d-flex justify-center w-100 '>
        &copy; {{ new Date().getFullYear() }} Nicolas Ondráš
      </div>

    </v-footer>
  </v-main>
</template>

<script setup>
  //
</script>
