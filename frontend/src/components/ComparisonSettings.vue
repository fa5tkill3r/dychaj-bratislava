<template>
  <div class='d-flex justify-end mt-12'>
    <v-btn
      icon
      @click='dialog = !dialog'
    >
      <v-icon>mdi-wrench</v-icon>
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
          <h2>{{ $t('placeComparison') }}</h2>
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
        <h3>{{ $t('choosePlacesToCompare') }} </h3>
        <v-chip-group
          v-model='selection'
          selected-class='text-primary'
          :column='true'
          :multiple='true'
          :filter='true'
        >
          <v-chip
            v-for='sensor in availableSensors'
            :key='sensor.id'
          >
            {{ sensor.name }}
          </v-chip>
        </v-chip-group>

        <h3>{{ $t('chooseAccuracy') }}</h3>
        <div class='d-flex justify-center'>
          <v-btn-toggle
            v-model='interval'
            :multiple='false'
            mandatory
          >
            <v-btn>
              {{ $t('tenmin') }}
            </v-btn>

            <v-btn>
              {{ $t('onehour') }}
            </v-btn>

            <v-btn>
              {{ $t('oneday') }}
            </v-btn>

            <v-btn>
              {{ $t('oneweek') }}
            </v-btn>
          </v-btn-toggle>
        </div>

        <h3>{{ $t('daysToCompare', [range]) }}</h3>
        <VueDatePicker
          v-model='date'
          range
          :max-range='range'
          position='center'
          auto-apply
          :max-date='to'
          inline />
      </div>

    </v-sheet>
  </v-dialog>
</template>

<script setup>

import { ref, watch } from 'vue'

const dialog = ref(false)
const selection = ref([])
const date = ref([])
const interval = ref(1)
const range = 7

let from = new Date()
from.setHours(0, 0, 0, 0)

let to = new Date()
to.setHours(23, 59, 59, 999)

date.value = [from.toISOString(), to.toISOString()]


const props = defineProps({
  availableSensors: {
    type: Array,
    required: true,
  },
})
const emit = defineEmits(['update'])

const submit = () => {
  emit('update', {
    Sensors: selection.value.map((index) => props.availableSensors[index].id).sort(),
    Interval: interval.value,
    From: date.value[0],
    To: date.value[1],
  })
  dialog.value = false
}
</script>

<style scoped>
.dp__flex_display {
  justify-content: center;
}
</style>
