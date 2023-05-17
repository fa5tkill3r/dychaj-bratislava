module.exports = {
  root: true,
  env: {
    browser: true,
    node: true,
  },
  plugins: ['vue', 'prettier'],
  extends: [
    'plugin:vue/vue3-essential',
    'plugin:vue/vue3-recommended',
    'eslint-config-prettier',
    'eslint:recommended',
  ],
  rules: {
    'vue/valid-v-slot': ['error', { allowModifiers: true }],
  },
}
