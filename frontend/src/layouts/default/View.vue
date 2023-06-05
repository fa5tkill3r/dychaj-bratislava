<template>
  <v-main>
    <router-view />
    <v-footer
      color='grey-lighten-4'
    >
      <div class='w-100 d-flex justify-center position-relative'>
        <span>&copy; {{ new Date().getFullYear() }} Nicolas Ondráš</span>

        <v-btn
          href='https://github.com/fa5tkill3r/bakalarska-praca'
          target='_blank'
          rel='noopener'
          class='ml-2 github'
          variant='text'
          density='compact'>
          <template #prepend>
            <v-icon>mdi-github</v-icon>
          </template>
          GitHub
        </v-btn>
      </div>

    </v-footer>
  </v-main>
</template>

<script setup>
//
</script>

<style scoped>
.github {
  position: absolute;
  right: 0;
}

</style>
