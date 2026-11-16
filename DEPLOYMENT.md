# Guía de despliegue

Notas para levantar el proyecto en un servidor con Docker Compose o en plataformas PaaS.

## Servicios y puertos

| Servicio | Contenedor | Tecnología | Puerto interno | Puerto host (default) |
|---|---|---|---|---|
| Base de datos | `estudio_contable_db` | PostgreSQL 16 Alpine | `5432` | `5432` |
| Backend API | `estudio_contable_api` | ASP.NET Core 8 WebApi | `8080` | `5000` |
| Frontend Web | `estudio_contable_web` | React + TypeScript + Nginx | `80` | `3000` |

Nginx sirve los archivos estáticos de la SPA (con fallback a `index.html` para React Router) y redirige `/api/` hacia `http://backend:8080/api/`, así todo queda bajo un mismo dominio sin problemas de CORS.

En el primer arranque, PostgreSQL ejecuta `backend/src/EstudioContable.Infrastructure/Database/init-db.sql`.

## Comandos útiles de Docker

```bash
# Ver estado de los contenedores
docker compose ps

# Ver logs en tiempo real
docker compose logs -f

# Ver logs solo del backend
docker compose logs -f backend

# Detener los contenedores
docker compose down

# Detener y borrar volúmenes de datos (resetea la base)
docker compose down -v

# Reiniciar un servicio puntual
docker compose restart backend
```

## Despliegue en un VPS (Ubuntu / Debian)

1. Instalar Docker y Docker Compose en el servidor:

```bash
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker $USER
```

2. Clonar el repositorio y crear el `.env` con credenciales de producción:

```bash
git clone <URL_DEL_REPOSITORIO> /opt/estudio-contable
cd /opt/estudio-contable
cp .env.example .env
nano .env
```

Ejemplo de configuración para producción:

```env
DB_NAME=estudio_contable_prod
DB_USER=postgres
DB_PASSWORD=tu_password_segura
DB_PORT=5432

BACKEND_PORT=5000
ASPNETCORE_ENVIRONMENT=Production
JWT_SECRET=clave_jwt_aleatoria_de_al_menos_64_caracteres

FRONTEND_PORT=80
```

3. Levantar los servicios:

```bash
docker compose up -d --build
```

4. Configurar HTTPS (por ejemplo con Nginx Proxy Manager, Traefik o Certbot en el host):

```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d tudominio.com
```

## Despliegue en PaaS (Coolify / Railway / Render)

### Coolify
1. Conectar el repositorio de GitHub en Coolify.
2. Elegir **Docker Compose Deployment** y cargar las variables del `.env`.

### Railway / Render
1. **Base de datos:** crear un PostgreSQL gestionado y correr el script `backend/src/EstudioContable.Infrastructure/Database/init-db.sql`.
2. **Backend:**
   - Dockerfile: `backend/Dockerfile`
   - Context: `backend`
   - Variable de entorno: `ConnectionStrings__DefaultConnection` apuntando a la base gestionada y `Jwt__Secret`.
3. **Frontend:**
   - Dockerfile: `frontend/Dockerfile`
   - Context: `frontend`
