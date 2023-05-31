<template>
  <div>
    <v-btn
      color='primary'
      @click='dialog = true'
    >
      Generuj
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
          <h2>Porovanie miest</h2>
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
        <h3>Vyber miesta na porovnanie: </h3>
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

        <h3>Vyber dni na porovnanie: </h3>
        <div class='d-flex justify-center'>
          <v-btn-toggle
            v-model='selectedDays'
            :multiple='true'
            variant='outlined'
            :divided='true'
          >
            <v-btn v-for='(day, index) in days' :key='index'>
              {{ day }}
            </v-btn>
          </v-btn-toggle>

        </div>

        <h3>Hodiny:</h3>
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

        <h3>Počet týždňov:</h3>
        <v-slider
          v-model='selectedWeeks'
          :min='1'
          :max='3'
          thumb-label
          step='1'
        >

        </v-slider>
      </div>
    </v-sheet>
  </v-dialog>
</template>

<script setup>
import { ref } from 'vue'

const selection = ref([])
const selectedDays = ref([])
const selectedHours = ref([])
const selectedWeeks = ref(1)
const dialog = ref(false)
const days = ref([
  'Po',
  'Ut',
  'St',
  'Št',
  'Pi',
  'So',
  'Ne',
])


const props = defineProps({
  availableSensors: {
    type: Array,
    required: true,
  },
})

const emit = defineEmits(['update'])

const submit = () => {
  dialog.value = false
  emit('update', {
    sensors: selection.value.map((index) => props.availableSensors[index].id).sort(),
    days: selectedDays.value.map((index) => index + 1).sort(),
    hours: selectedHours.value.map((index) => index).sort(),
    weeks: selectedWeeks.value,
  })
}


</script>

<style scoped>

</style>
