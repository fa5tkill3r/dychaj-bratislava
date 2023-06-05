FROM node:18-alpine as builder

WORKDIR /app

COPY package.json pnpm-lock.yaml ./

COPY . .

RUN npm install -g pnpm && pnpm install

RUN pnpm build

FROM nginx

COPY --from=builder /app/dist /usr/share/nginx/html

COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
