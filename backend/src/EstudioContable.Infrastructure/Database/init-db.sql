-- =============================================================================
-- BASE DE DATOS: SISTEMA DE GESTIÓN PARA ESTUDIOS CONTABLES (ARGENTINA)
-- Script DDL & Seed Inicial para PostgreSQL 14+
-- =============================================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 1. ESTUDIOS (Tenants)
CREATE TABLE IF NOT EXISTS "Estudios" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Nombre" VARCHAR(150) NOT NULL,
    "Cuit" VARCHAR(11) NOT NULL,
    "Email" VARCHAR(150) NOT NULL,
    "Telefono" VARCHAR(50),
    "Plan" VARCHAR(30) DEFAULT 'Trial',
    "Activo" BOOLEAN DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ
);
CREATE INDEX IF NOT EXISTS "IX_Estudios_Cuit" ON "Estudios"("Cuit");

-- 2. OBLIGACIONES (Catálogo Maestro Nacional de Impuestos y Regímenes)
CREATE TABLE IF NOT EXISTS "Obligaciones" (
    "Id" VARCHAR(50) PRIMARY KEY,
    "Nombre" VARCHAR(150) NOT NULL,
    "Organismo" VARCHAR(50) NOT NULL,
    "Periodicidad" VARCHAR(20) NOT NULL,
    "RequiereJurisdiccion" BOOLEAN DEFAULT FALSE,
    "EsFrecuente" BOOLEAN DEFAULT TRUE,
    "Descripcion" TEXT
);

-- 3. CLIENTES
CREATE TABLE IF NOT EXISTS "Clientes" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "EstudioId" UUID NOT NULL REFERENCES "Estudios"("Id") ON DELETE RESTRICT,
    "Cuit" VARCHAR(11) NOT NULL,
    "RazonSocial" VARCHAR(200) NOT NULL,
    "NombreFantasia" VARCHAR(150),
    "TipoPersona" VARCHAR(20) NOT NULL,
    "RegimenTributario" VARCHAR(30) NOT NULL,
    "CierreEjercicioMes" INT,
    "EmailContacto" VARCHAR(150),
    "TelefonoContacto" VARCHAR(50),
    "ResponsableAsignadoId" UUID,
    "Notas" TEXT,
    "Activo" BOOLEAN DEFAULT TRUE,
    "CreatedAt" TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ
);
CREATE INDEX IF NOT EXISTS "IX_Clientes_EstudioId_Cuit" ON "Clientes"("EstudioId", "Cuit");
CREATE INDEX IF NOT EXISTS "IX_Clientes_EstudioId_Activo" ON "Clientes"("EstudioId", "Activo");

-- 4. USUARIOS
CREATE TABLE IF NOT EXISTS "Usuarios" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "EstudioId" UUID NOT NULL REFERENCES "Estudios"("Id") ON DELETE RESTRICT,
    "Email" VARCHAR(150) NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "Nombre" VARCHAR(100) NOT NULL,
    "Apellido" VARCHAR(100) NOT NULL,
    "Rol" VARCHAR(30) NOT NULL, -- 'AdminEstudio', 'Colaborador', 'ClientePortal'
    "ClienteId" UUID REFERENCES "Clientes"("Id") ON DELETE SET NULL,
    "Activo" BOOLEAN DEFAULT TRUE,
    "UltimoAccesoAt" TIMESTAMPTZ,
    "CreatedAt" TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ,
    CONSTRAINT "UQ_Usuarios_EstudioId_Email" UNIQUE ("EstudioId", "Email")
);
ALTER TABLE "Clientes" ADD CONSTRAINT "FK_Clientes_Responsable" 
    FOREIGN KEY ("ResponsableAsignadoId") REFERENCES "Usuarios"("Id") ON DELETE SET NULL;

-- 5. CLIENTE_OBLIGACIONES
CREATE TABLE IF NOT EXISTS "ClienteObligaciones" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "EstudioId" UUID NOT NULL REFERENCES "Estudios"("Id") ON DELETE RESTRICT,
    "ClienteId" UUID NOT NULL REFERENCES "Clientes"("Id") ON DELETE CASCADE,
    "ObligacionId" VARCHAR(50) NOT NULL REFERENCES "Obligaciones"("Id") ON DELETE RESTRICT,
    "Jurisdiccion" VARCHAR(100),
    "DiaVencimientoPersonalizado" INT,
    "Activo" BOOLEAN DEFAULT TRUE,
    "Notas" TEXT,
    "CreatedAt" TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ
);
CREATE INDEX IF NOT EXISTS "IX_ClienteObligaciones_Cliente_Obligacion" ON "ClienteObligaciones"("ClienteId", "ObligacionId");

-- 6. VENCIMIENTOS
CREATE TABLE IF NOT EXISTS "Vencimientos" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "EstudioId" UUID NOT NULL REFERENCES "Estudios"("Id") ON DELETE RESTRICT,
    "ClienteId" UUID NOT NULL REFERENCES "Clientes"("Id") ON DELETE CASCADE,
    "ClienteObligacionId" UUID NOT NULL REFERENCES "ClienteObligaciones"("Id") ON DELETE CASCADE,
    "PeriodoFiscal" VARCHAR(7) NOT NULL, -- ej: '2026-09'
    "FechaVencimientoOriginal" DATE NOT NULL,
    "FechaVencimientoEfectiva" DATE NOT NULL,
    "TieneProrroga" BOOLEAN DEFAULT FALSE,
    "MotivoProrroga" TEXT,
    "Estado" VARCHAR(20) DEFAULT 'Pendiente',
    "MontoAPagar" NUMERIC(14,2),
    "FechaPresentacion" TIMESTAMPTZ,
    "FechaPago" TIMESTAMPTZ,
    "ResponsableId" UUID REFERENCES "Usuarios"("Id") ON DELETE SET NULL,
    "Notas" TEXT,
    "CreatedAt" TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ
);
CREATE INDEX IF NOT EXISTS "IX_Vencimientos_EstudioId_Periodo" ON "Vencimientos"("EstudioId", "PeriodoFiscal");
CREATE INDEX IF NOT EXISTS "IX_Vencimientos_EstudioId_FechaEfectiva" ON "Vencimientos"("EstudioId", "FechaVencimientoEfectiva");
CREATE INDEX IF NOT EXISTS "IX_Vencimientos_EstudioId_Estado" ON "Vencimientos"("EstudioId", "Estado");

-- 7. TAREAS
CREATE TABLE IF NOT EXISTS "Tareas" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "EstudioId" UUID NOT NULL REFERENCES "Estudios"("Id") ON DELETE RESTRICT,
    "ClienteId" UUID NOT NULL REFERENCES "Clientes"("Id") ON DELETE CASCADE,
    "VencimientoId" UUID REFERENCES "Vencimientos"("Id") ON DELETE CASCADE,
    "Titulo" VARCHAR(200) NOT NULL,
    "Descripcion" TEXT,
    "AsignadoAUsuarioId" UUID REFERENCES "Usuarios"("Id") ON DELETE SET NULL,
    "Estado" VARCHAR(20) DEFAULT 'Pendiente',
    "Prioridad" VARCHAR(15) DEFAULT 'Media',
    "FechaLimite" DATE,
    "Orden" INT DEFAULT 0,
    "CreatedAt" TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ
);
CREATE INDEX IF NOT EXISTS "IX_Tareas_EstudioId_Estado" ON "Tareas"("EstudioId", "Estado");
CREATE INDEX IF NOT EXISTS "IX_Tareas_EstudioId_Asignado" ON "Tareas"("EstudioId", "AsignadoAUsuarioId");

-- 8. DOCUMENTOS
CREATE TABLE IF NOT EXISTS "Documentos" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "EstudioId" UUID NOT NULL REFERENCES "Estudios"("Id") ON DELETE RESTRICT,
    "ClienteId" UUID NOT NULL REFERENCES "Clientes"("Id") ON DELETE CASCADE,
    "VencimientoId" UUID REFERENCES "Vencimientos"("Id") ON DELETE SET NULL,
    "NombreOriginal" VARCHAR(255) NOT NULL,
    "NombreAlmacenado" VARCHAR(255) NOT NULL,
    "RutaAlmacenamiento" VARCHAR(500) NOT NULL,
    "ContentType" VARCHAR(100) DEFAULT 'application/pdf',
    "TamanoBytes" BIGINT NOT NULL,
    "Categoria" VARCHAR(30) DEFAULT 'Otro',
    "VisibleParaCliente" BOOLEAN DEFAULT TRUE,
    "SubidoPorUsuarioId" UUID NOT NULL REFERENCES "Usuarios"("Id") ON DELETE RESTRICT,
    "CreatedAt" TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMPTZ
);
CREATE INDEX IF NOT EXISTS "IX_Documentos_EstudioId_Cliente" ON "Documentos"("EstudioId", "ClienteId");
CREATE INDEX IF NOT EXISTS "IX_Documentos_EstudioId_Vencimiento" ON "Documentos"("EstudioId", "VencimientoId");

-- =============================================================================
-- SEED INICIAL: OBLIGACIONES TRIBUTARIAS EN ARGENTINA
-- =============================================================================
INSERT INTO "Obligaciones" ("Id", "Nombre", "Organismo", "Periodicidad", "RequiereJurisdiccion", "EsFrecuente", "Descripcion")
VALUES 
('IVA', 'Impuesto al Valor Agregado (F.2002)', 'ARCA/AFIP', 'Mensual', FALSE, TRUE, 'Declaración Jurada mensual según terminación de CUIT (días 18 a 22).'),
('F931_LSD', 'Cargas Sociales / Libro de Sueldos Digital (F.931)', 'ARCA/AFIP', 'Mensual', FALSE, TRUE, 'Aportes y contribuciones patronales según CUIT (días 9 a 12).'),
('MONOTRIBUTO', 'Régimen Simplificado (Monotributo)', 'ARCA/AFIP', 'Mensual', FALSE, TRUE, 'Cuota fija mensual integrada. Vence el día 20 de cada mes.'),
('IIBB_CM', 'Ingresos Brutos - Convenio Multilateral (CM03)', 'COMARB', 'Mensual', TRUE, TRUE, 'Anticipo mensual de Ingresos Brutos interjurisdiccional (días 15 a 19).'),
('IIBB_LOCAL', 'Ingresos Brutos - Régimen General Local', 'Rentas Provincial', 'Mensual', TRUE, TRUE, 'Declaración mensual en jurisdicción sede (ARBA, AGIP, etc.).'),
('GANANCIAS_PJ', 'Impuesto a las Ganancias - Sociedades (F.713)', 'ARCA/AFIP', 'Anual', FALSE, TRUE, 'Declaración Jurada anual al 5to mes posterior al cierre de ejercicio.'),
('GANANCIAS_PH', 'Impuesto a las Ganancias - Personas Humanas', 'ARCA/AFIP', 'Anual', FALSE, TRUE, 'Declaración anual período fiscal anterior (habitualmente junio).'),
('BIENES_PERSONALES', 'Impuesto sobre los Bienes Personales', 'ARCA/AFIP', 'Anual', FALSE, TRUE, 'Declaración anual sobre bienes al 31 de diciembre.'),
('SICORE', 'Sistema de Control de Retenciones (SICORE)', 'ARCA/AFIP', 'Quincenal', FALSE, FALSE, 'Retenciones y percepciones impositivas practicadas.')
ON CONFLICT ("Id") DO NOTHING;

-- =============================================================================
-- SEED DE DEMOSTRACIÓN: ESTUDIO PILOTO Y USUARIOS
-- =============================================================================
DO $$
DECLARE
    v_estudio_id UUID := 'a0000000-0000-0000-0000-000000000001';
    v_admin_id UUID := 'b0000000-0000-0000-0000-000000000001';
    v_colab_id UUID := 'b0000000-0000-0000-0000-000000000002';
    v_cliente_1_id UUID := 'c0000000-0000-0000-0000-000000000001';
    v_cliente_2_id UUID := 'c0000000-0000-0000-0000-000000000002';
    v_cliente_3_id UUID := 'c0000000-0000-0000-0000-000000000003';
    v_portal_user_id UUID := 'b0000000-0000-0000-0000-000000000003';
BEGIN
    -- Estudio
    INSERT INTO "Estudios" ("Id", "Nombre", "Cuit", "Email", "Telefono", "Plan")
    VALUES (v_estudio_id, 'Estudio Perez & Asociados', '30712345678', 'contacto@estudioperez.com.ar', '+54 11 4321-8765', 'Pro')
    ON CONFLICT ("Id") DO NOTHING;

    -- Usuarios del Estudio (Password en claro para demo dev: 'Admin123!' / 'Colab123!')
    INSERT INTO "Usuarios" ("Id", "EstudioId", "Email", "PasswordHash", "Nombre", "Apellido", "Rol")
    VALUES 
    (v_admin_id, v_estudio_id, 'admin@estudioperez.com.ar', 'Admin123!', 'Martín', 'Pérez', 'AdminEstudio'),
    (v_colab_id, v_estudio_id, 'lucia@estudioperez.com.ar', 'Colab123!', 'Lucía', 'Gómez', 'Colaborador')
    ON CONFLICT ("EstudioId", "Email") DO NOTHING;

    -- Clientes de prueba
    -- Cliente 1: Responsable Inscripto (CUIT terminado en 0 -> IVA día 18)
    INSERT INTO "Clientes" ("Id", "EstudioId", "Cuit", "RazonSocial", "NombreFantasia", "TipoPersona", "RegimenTributario", "ResponsableAsignadoId", "EmailContacto", "TelefonoContacto")
    VALUES (v_cliente_1_id, v_estudio_id, '30714455660', 'Distribuidora del Centro S.R.L.', 'Distribuidora Centro', 'Juridica', 'ResponsableInscripto', v_colab_id, 'administracion@distribuidoracentro.com.ar', '11 5555-0101')
    ON CONFLICT DO NOTHING;

    -- Cliente 2: Monotributista (CUIT terminado en 5 -> Monotributo día 20)
    INSERT INTO "Clientes" ("Id", "EstudioId", "Cuit", "RazonSocial", "TipoPersona", "RegimenTributario", "ResponsableAsignadoId", "EmailContacto")
    VALUES (v_cliente_2_id, v_estudio_id, '20349876545', 'Carlos Alberto Fernández', 'Fisica', 'Monotributo', v_admin_id, 'carlosfernandez@gmail.com')
    ON CONFLICT DO NOTHING;

    -- Cliente 3: Empresa con Convenio Multilateral (CUIT terminado en 9 -> IVA día 22, CM día 18)
    INSERT INTO "Clientes" ("Id", "EstudioId", "Cuit", "RazonSocial", "TipoPersona", "RegimenTributario", "ResponsableAsignadoId", "EmailContacto")
    VALUES (v_cliente_3_id, v_estudio_id, '33681234569', 'Logística y Transportes Austral S.A.', 'Juridica', 'ResponsableInscripto', v_colab_id, 'finanzas@transportesaustral.com.ar')
    ON CONFLICT DO NOTHING;

    -- Usuario del portal del cliente 1
    INSERT INTO "Usuarios" ("Id", "EstudioId", "Email", "PasswordHash", "Nombre", "Apellido", "Rol", "ClienteId")
    VALUES (v_portal_user_id, v_estudio_id, 'cliente@distribuidoracentro.com.ar', 'Cliente123!', 'Roberto', 'González', 'ClientePortal', v_cliente_1_id)
    ON CONFLICT ("EstudioId", "Email") DO NOTHING;

    -- Asociar obligaciones
    -- Cliente 1: IVA y F931
    INSERT INTO "ClienteObligaciones" ("EstudioId", "ClienteId", "ObligacionId") VALUES
    (v_estudio_id, v_cliente_1_id, 'IVA'),
    (v_estudio_id, v_cliente_1_id, 'F931_LSD')
    ON CONFLICT DO NOTHING;

    -- Cliente 2: Monotributo
    INSERT INTO "ClienteObligaciones" ("EstudioId", "ClienteId", "ObligacionId") VALUES
    (v_estudio_id, v_cliente_2_id, 'MONOTRIBUTO')
    ON CONFLICT DO NOTHING;

    -- Cliente 3: IVA y Convenio Multilateral
    INSERT INTO "ClienteObligaciones" ("EstudioId", "ClienteId", "ObligacionId", "Jurisdiccion") VALUES
    (v_estudio_id, v_cliente_3_id, 'IVA', NULL),
    (v_estudio_id, v_cliente_3_id, 'IIBB_CM', 'Sede CABA (901) + Buenos Aires (902)')
    ON CONFLICT DO NOTHING;

END $$;
