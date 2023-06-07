<template>
  <v-menu
    open-on-hover
    transition='slide-y-transition'
    :disabled='!description'
  >
    <template #activator='{ props }'>
      <v-sheet
        v-bind='props'
        elevation='4'
        rounded
        width='150'
        height='150'
        class='position-relative'
      >
        <v-btn
          v-if='help'
          class='help'
          density='compact'
          icon
          variant='text'>
          <v-icon>mdi-help-circle</v-icon>
          <v-tooltip
            activator='parent'
            max-width='300px'
          >
            <span>{{ help }}</span>
          </v-tooltip>
        </v-btn>
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

    <v-sheet
      elevation='4'
      rounded
      width='150'
      height='auto'
      class='mt-1 text-center pa-1'
    >
      <span>{{ description }}</span>
    </v-sheet>

  </v-menu>
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
  help: {
    type: String,
    required: false,
    default: null,
  },
  description: {
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

.help {
  position: absolute;
  right: 0;
}
</style>
