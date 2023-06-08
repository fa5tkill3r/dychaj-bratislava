<template>
  <v-main>
    <router-view />
    <v-footer
      color='grey-lighten-4'
    >
      <div class='w-100 d-flex justify-space-between'>
        <div></div>

        <span v-if='showVersion'>APP: {{ version }} / API: {{ apiVersion }}</span>
        <span
          v-else
          @click='handleClick'
        >&copy; {{ new Date().getFullYear() }} Nicolas Ondráš</span>

        <v-btn
          href='https://github.com/fa5tkill3r/dychaj-bratislava'
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
import { version as packageVersion } from '@/../package.json'
import { ref } from 'vue'
import { ky } from '@/lib/ky'

const version = ref(packageVersion)
const apiVersion = ref('')
const clickCount = ref(0)
const showVersion = ref(false)

const handleClick = () => {
  clickCount.value++
  if (clickCount.value === 10) {
    showVersion.value = true
    setTimeout(() => {
      showVersion.value = false
    }, 3000)
  }
  setTimeout(() => {
    clickCount.value = 0
  }, 2000)
}

const getVersion = async () => {
  apiVersion.value = await ky.get('').json()
}

getVersion()

</script>

