import kyclient from 'ky'

const getApiUrl = () => {
  if (import.meta.env.MODE === 'production') {
    return window.location.origin + '/api'
  } else {
    return import.meta.env.VITE_API_URL
  }
}

const defaultOptions = {
  prefixUrl: getApiUrl(),
  headers: {
    accept: 'application/json',
  },
  hooks: {
    beforeRequest: [
      request => {
        const token = sessionStorage.getItem('token')
        if (token) {
          request.headers.set('authorization', `Bearer ${token}`)
        }
      },
    ],

  },
}

export let ky = kyclient.create({ ...defaultOptions })

export function setDefaults(options) {
  ky = ky.extend(options)
}
