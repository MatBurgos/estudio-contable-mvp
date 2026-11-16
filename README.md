# Estudio Contable MVP

Sistema web para la gestión interna de un estudio contable y portal de clientes. Permite administrar clientes, tareas, vencimientos fiscales, documentación y acceso diferenciado según el rol del usuario.

## Stack

- **Backend:** ASP.NET Core 8 Web API (arquitectura en capas: Domain, Application, Infrastructure, WebApi)
- **Base de datos:** PostgreSQL 16
- **Frontend:** React 18, TypeScript, Vite
- **Infraestructura:** Docker, Docker Compose y Nginx como reverse proxy

## Módulos principales

- Autenticación con JWT y control de roles (admin, colaborador y cliente)
- Dashboard con resumen general
- Gestión de clientes
- Seguimiento de tareas y vencimientos
- Carga y descarga de documentos
- Portal de clientes

## Cómo levantarlo con Docker

Es la forma más rápida de correr todo junto (base de datos, API y frontend).

1. Copiá el archivo de variables de entorno y completá los valores que faltan (`DB_PASSWORD` y `JWT_SECRET`):

```bash
cp .env.example .env
```

2. Levantá los contenedores:

```bash
docker compose up -d --build
```

La primera vez que arranca PostgreSQL ejecuta automáticamente el script `backend/src/EstudioContable.Infrastructure/Database/init-db.sql`.

Una vez arriba:
- Frontend: `http://localhost:3000`
- Swagger de la API: `http://localhost:5000/swagger`

## Desarrollo sin Docker

Si preferís correr cada parte por separado:

**Backend:**
- Necesitás .NET 8 SDK y una instancia de PostgreSQL corriendo.
- Configurá `ConnectionStrings:DefaultConnection` y `Jwt:Secret` (por ejemplo en un `appsettings.Development.json`, que está ignorado por git).
- Desde `backend/src/EstudioContable.WebApi`:

```bash
dotnet run
```

**Frontend:**
- Desde `frontend/`:

```bash
npm install
npm run dev
```

## Despliegue

En [DEPLOYMENT.md](DEPLOYMENT.md) están los pasos para levantarlo en un VPS con Docker o en servicios como Coolify, Railway o Render.
