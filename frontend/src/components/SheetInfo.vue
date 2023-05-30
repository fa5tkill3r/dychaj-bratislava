<template>
  <v-sheet
    elevation='4'
    rounded
    width='150'
    height='150'
  >
    <div class='d-flex flex-column justify-center align-center fill-height'>
      <span class='title'>{{ title }} </span>
      <slot name='subtitle'>
        <sub>{{ subtitle }}</sub>
      </slot>
    </div>
  </v-sheet>
</template>

<script setup>

defineProps({
  title: {
    type: String,
    required: true,
  },
  subtitle: {
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
