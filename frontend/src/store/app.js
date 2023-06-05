// Utilities
import { defineStore } from 'pinia'

export const useAppStore = defineStore('app', {
  state: () => ({
    favorites: JSON.parse(localStorage.getItem('favorites')) || {
      temperature: [],
      humidity: [],
      pressure: [],
      pm25: [],
      pm10: [],
    }
  }),
  actions: {
    toggleFavorite(sensor) {
      const favorites = this.favorites[sensor.type.toLowerCase()]
      const index = favorites.findIndex((id) => id === sensor.id)
      if (index === -1) {
        favorites.push(sensor.id)
      } else {
        favorites.splice(index, 1)
      }

      localStorage.setItem('favorites', JSON.stringify(this.favorites))
    },
    isFavorite(sensor) {
      const favorites = this.favorites[sensor.type.toLowerCase()]
      return favorites.findIndex((id) => id === sensor.id) !== -1
    }
  },

})
