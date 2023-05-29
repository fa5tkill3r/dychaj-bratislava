<template>
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
          @click='() => $emit("update:tags", selection.map((index) => tags[index].id))'
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
          v-for='module in availableModules'
          :key='module.id'
        >
          {{ module.name }}
        </v-chip>
      </v-chip-group>
    </div>

    <div class='d-flex justify-center pa-4'>
      <v-btn-toggle
        v-model='days'
        multiple='true'
        variant='outlined'
        divided='true'
      >
        <v-btn v-for='(day, index) in days' :key='index'>
          {{ day }}
        </v-btn>
      </v-btn-toggle>

    </div>

    <div class='d-flex justify-center pa-4'>
      <v-item-group
        v-model="selectedHours"
        class="d-flex justify-sm-space-between px-6 pt-2 pb-6"
      >
        <v-item
          v-for="n in 23"
          :key="n"
        >
          <template #default="{ toggle }">
            <v-btn
              :icon="`mdi-numeric-${n}`"
              border
              height="40"
              variant="text"
              width="40"
              @click="toggle"
            ></v-btn>
          </template>
        </v-item>
      </v-item-group>

    </div>

  </v-sheet>
</template>

<script setup>
import { ref } from 'vue'

const selection = ref([])
const selectedDays = ref([])
const selectedHours = ref([])
const days = ref([
  'Po',
  'Ut',
  'St',
  'Št',
  'Pi',
  'So',
  'Ne',
])
const hours = [
  '00:00',
  '01:00',
  '02:00',
  '03:00',
  '04:00',
  '05:00',
  '06:00',
  '07:00',
  '08:00',
  '09:00',
  '10:00',
  '11:00',
  '12:00',
  '13:00',
  '14:00',
  '15:00',
  '16:00',
  '17:00',
  '18:00',
  '19:00',
  '20:00',
  '21:00',
  '22:00',
  '23:00',
]





const props = defineProps({
  availableModules: {
    type: Array,
    required: true,
  },
  selectedModules: {
    type: Array,
    required: true,
  },
})


</script>

<style scoped>

</style>
