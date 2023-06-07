<template>
  <div
    class='my-7'
  >
    <div
      class='gauge position-relative'
      :style='{
      height: height,
      borderRadius: height,
      background: gaugeColor,
    }'
    >
      <div
        v-if='value'
        class='position-absolute'
        :style='{
          left: `${position}%`,
        }'
      >
        <div
          class='value d-flex'
        >
          <span>{{ value }}</span>
          <span>{{ unit }}</span>
        </div>
        <div
          class='pointer'
          :style='{
            height: height,
          }'
        >
        </div>
      </div>

      <div
        class='limit'
      >
        <span>{{ limits[0].value }}</span>
        <span>{{ unit }}</span>
      </div>

      <div
        class='limit'
      >
        <span>{{ max }}</span>
        <span>{{ unit }}</span>
      </div>
    </div>
  </div>
</template>

<script setup>

import { computed } from 'vue'

const props = defineProps({
  limits: {
    type: Array,
    required: true,
  },
  unit: {
    type: String,
    required: true,
  },
  height: {
    type: String,
    default: '2em',
  },
  value: {
    type: Number,
    default: null,
  },
})

const gaugeColor = computed(() => {
  let gradient = `linear-gradient(90deg, `
  for (let i = 0; i < props.limits.length; i++) {
    const limit = props.limits[i]
    if (i === 0) {
      gradient += `${limit.color} 0%, `
    } else if (i === props.limits.length - 1) {
      gradient += `${limit.color} 100%`
    } else {
      gradient += `${limit.color} ${i / (props.limits.length - 1) * 100}%, `
    }
  }
  return `${gradient}`
})

const position = computed(() => {
  let value = props.value / props.limits.at(-1).value * 100

  if (value > 98) {
    value = 98
  } else if (value < 2) {
    value = 2
  }

  return value
})

const max = computed(() => {
  let max = props.limits.at(-1).value
  if (props.value > max) {
    max = props.value
  }
  return max
})


</script>

<style scoped>
.gauge {
  width: 100%;
  border-radius: 2em;
  height: 80%;
}

.limit {
  position: absolute;
  bottom: -2em;
}

.limit:last-child {
  right: 0;
}

.pointer {
  width: 2px;
  background: #fff;
  border: 1px solid #000;
}

.value {
  position: absolute;
  top: -2em;
  left: 50%;
  transform: translateX(-50%);
}

</style>
