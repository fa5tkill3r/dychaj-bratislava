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
          v-for='tag in tags'
          :key='tag'
        >
          {{ tag.name }}
        </v-chip>
      </v-chip-group>
    </div>
  </v-sheet>
</template>

<script setup>

import { onMounted, ref, watch } from 'vue'

const selection = ref([])

const props = defineProps({
  tags: {
    type: Array,
    required: true,
  },
  selectedTags: {
    type: Array,
    required: true,
  },
})

defineEmits(['update:tags'])

onMounted(() => {
  selection.value = props.selectedTags.map((tag) => {
    return props.tags.findIndex((t) => t.id === tag)
  })


})
</script>

<style scoped>

</style>
