<template>
  <div class='d-flex justify-end mt-12'>
    <v-btn
      icon
      @click='dialog = !dialog'
    >
      <v-icon>mdi-filter</v-icon>
    </v-btn>
  </div>

  <v-dialog
    v-model='dialog'
    width='auto'
  >
    <v-sheet
      style='max-width: 50em;'
      elevation='10'
      rounded='xl'
    >
      <v-sheet
        class='pa-3 bg-primary text-right'
        rounded='t-xl'
      >

        <div class='d-flex align-center justify-space-between'>
          <h2>Filter miest</h2>
          <v-btn
            class='ms-2'
            icon
            @click='submit'
          >
            <v-icon>mdi-check-bold</v-icon>
          </v-btn>
        </div>
      </v-sheet>

      <div class='pa-4'>
        <v-chip-group
          v-model='selection'
          selected-class='text-primary'
          :column='true'
          :multiple='true'
          :filter='true'
        >
          <v-chip
            v-for='tag in availableSensors'
            :key='tag'
          >
            {{ tag.name }}
          </v-chip>
        </v-chip-group>
      </div>
    </v-sheet>
  </v-dialog>
</template>

<script setup>

import { ref } from 'vue'

const selection = ref([])
const dialog = ref(false)

const props = defineProps({
  availableSensors: {
    type: Array,
    required: true,
  },
})

const emit = defineEmits(['update'])

const submit = () => {
  emit('update', selection.value.map((index) => props.availableSensors[index].id))
  dialog.value = false
}

// onMounted(() => {
//   selection.value = props.availableSensors.map((tag) => {
//     return props.selectedSensors.findIndex((t) => t.id === tag)
//   })
// })
</script>

<style scoped>

</style>
