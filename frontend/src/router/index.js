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
        component: () => import(/* webpackChunkName: "home" */ '@/views/Home.vue'),
      },
      {
        path: '/pm25',
        name: 'PM25',
        component: () => import(/* webpackChunkName: "pm25" */ '@/views/PM25View.vue'),
      },
      {
        path: '/temp',
        name: 'Temp',
        component: () => import(/* webpackChunkName: "pm25" */ '@/views/TempView.vue'),
      },

    ],
  },
]

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes,
})

export default router
