# ── Build stage ──
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npx ng build --configuration=production

# ── Serve stage ──
FROM nginx:alpine AS final
# Remove default nginx config
RUN rm /etc/nginx/conf.d/default.conf
# Copy our custom config
COPY nginx.conf /etc/nginx/conf.d/default.conf
# Copy built Angular app
COPY --from=build /app/dist/auction/browser /usr/share/nginx/html
EXPOSE 443 80
