# ==============================================================================
# Multi-stage Dockerfile para el Frontend React + Vite
# ==============================================================================

# Etapa 1: Build de la aplicación con Node.js
FROM node:20-alpine AS build
WORKDIR /app

# Instalar dependencias
COPY package*.json ./
RUN npm ci

# Copiar código fuente y compilar bundle de producción
COPY . .
RUN npm run build

# Etapa 2: Servir archivos estáticos y reverse-proxy con Nginx
FROM nginx:alpine AS runtime

# Copiar bundle compilado al directorio de Nginx
COPY --from=build /app/dist /usr/share/nginx/html

# Copiar configuración personalizada de Nginx
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
