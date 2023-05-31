// Composables
import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    component: () => import('@/layouts/default/Default.vue'),
    children: [
      {
        path: '',
        name: 'Home',
        // route level code-splitting
        // this generates a separate chunk (about.[hash].js) for this route
        // which is lazy-loaded when the route is visited.
        component: () => import(/* webpackChunkName: "home" */ '@/views/HomeView.vue'),
      },
      {
        path: '/pm25',
        name: 'PM25',
        component: () => import(/* webpackChunkName: "pm25" */ '@/views/PM25View.vue'),
      },
      {
        path: '/pm10',
        name: 'PM10',
        component: () => import(/* webpackChunkName: "pm10" */ '@/views/PM10View'),
      },
      {
        path: '/temp',
        name: 'Temp',
        component: () => import(/* webpackChunkName: "temp" */ '@/views/TempView.vue'),
      },
      {
        path: '/humidity',
        name: 'Humidity',
        component: () => import(/* webpackChunkName: "humidity" */ '@/views/HumidityView'),
      },
      {
        path: '/pressure',
        name: 'Pressure',
        component: () => import(/* webpackChunkName: "pressure" */ '@/views/PressureView'),
      },
    ],
  },
]

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes,
})

export default router
