<template>
  <div>
    <v-btn
      color='primary'
      @click='dialog = true'
    >
      {{ $t('generate') }}
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
          <h2>{{ $t('airPollutionWeekComparison') }}</h2>
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
        <h3>{{ $t('placeSelect') }}</h3>
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

        <h3>{{ $t('daysSelect') }}</h3>
        <div>
          <v-btn-toggle
            v-model='selectedDays'
            :multiple='true'
            class='d-flex justify-center flex-wrap align-content-stretch'
          >
            <v-btn v-for='day in days' :key='day'>
              {{ day }}
            </v-btn>
          </v-btn-toggle>

        </div>

        <h3>{{ $t('hours') }} </h3>
        <div class='d-flex justify-center'>
          <v-item-group
            v-model='selectedHours'
            :multiple='true'
            class='d-flex justify-sm-space-between px-6 pt-2 pb-6'
          >
            <v-row
              justify='space-evenly'
              :dense='true'
            >
              <v-col
                v-for='n in 24'
                :key='n'
                cols='auto'
              >
                <v-item>
                  <template #default='{ toggle, isSelected }'>
                    <v-btn
                      icon
                      :active='isSelected'
                      color='primary'
                      border
                      height='40'
                      variant='text'
                      width='40'
                      @click='toggle'
                    >{{ n - 1 }}
                    </v-btn>

                  </template>
                </v-item>
              </v-col>
            </v-row>
          </v-item-group>
        </div>

        <h3>{{ $t('weekCount') }}</h3>
        <v-slider
          v-model='selectedWeeks'
          :min='1'
          :max='3'
          step='1'
          show-ticks='always'
          :ticks='[1, 2, 3]'
        >

        </v-slider>
      </div>
    </v-sheet>
  </v-dialog>
</template>

<script setup>
import { ref } from 'vue'
import { t } from '@/lib/i18n'

const selection = ref([])
const selectedDays = ref([])
const selectedHours = ref([])
const selectedWeeks = ref(1)
const dialog = ref(false)


const days = [
  t('days.monday'),
  t('days.tuesday'),
  t('days.wednesday'),
  t('days.thursday'),
  t('days.friday'),
  t('days.saturday'),
  t('days.sunday'),
]


const props = defineProps({
  availableSensors: {
    type: Array,
    required: true,
  },
})

const emit = defineEmits(['update'])

const submit = () => {
  dialog.value = false
  let days = selectedDays.value.map((index) => (index + 1) % 7).sort()
  if (days.length === 0) {
    days = [new Date().getDay()]
  }
  let hours = selectedHours.value.map((index) => index).sort()
  if (hours.length === 0) {
    hours = [new Date().getHours()]
  }

  emit('update', {
    sensors: selection.value.map((index) => props.availableSensors[index].id).sort(),
    days: days,
    hours: hours,
    weeks: selectedWeeks.value,
  })
}


</script>

<style scoped>

</style>
