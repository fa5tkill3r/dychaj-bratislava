<template>
  <v-sheet
    elevation='4'
    rounded
    width='150'
    height='150'
  >
    <div class='d-flex flex-column justify-center align-center fill-height text-center pa-1'>
      <span class='title'>{{ title }}
        <slot name='unit'>
          <sub>{{ unit }}</sub>
        </slot>
      </span>
      <slot name='subtitle'>
        <span>{{ subtitle }}</span>
      </slot>
    </div>
  </v-sheet>
</template>

<script setup>

defineProps({
  title: {
    type: [String, Number],
    required: true,
  },
  subtitle: {
    type: String,
    required: false,
    default: null,
  },
  unit: {
    type: String,
    required: false,
    default: null,
  },
})

</script>

<style scoped>
.title {
  font-size: 2rem;
  font-weight: 500;
  line-height: 1.2;
  margin-bottom: 0.5rem;
}

sub {
  font-size: initial;
}
</style>
