# Auditoría de Código y DevOps — UNIDAD 4: Prácticas Profesionales

**Repositorio:** `nicos92/PW_Tareas` (`https://github.com/nicos92/PW_Tareas`) · **Rama auditada:** `main` (`30fa5af`) · **Rama por defecto:** `main` · **Visibilidad:** pública · **Fecha de auditoría:** 2026-11-02.

> **Nota de actualización:** esta versión reemplaza la auditoría anterior (basada en `1237b04`) y refleja el estado posterior al despliegue exitoso en Cloud Run (revisión `tareas-blazor-00005-f5l`, `https://tareas-blazor-3ziforwb7a-uc.a.run.app/`) sobre `main` (`30fa5af`). Los workflows de GitHub Actions quedaron operativos con Workload Identity Federation, tags inmutables y verificación de salud.

---

## Estado de Cumplimiento - UNIDAD 4

| # | Requisito | Estado |
|---|-----------|--------|
| 1 | Historial con Conventional Commits | **CUMPLE (parcial)** — validación local y en CI; 26/100 commits no-merge conformes y 74 de legado sin prefijo |
| 2 | CI/CD con GitHub Actions (build + deploy) | **CUMPLE** — `ci.yml` verde (build + test + commitlint); `docker-publish` publica la imagen y `deploy-cloud-run` despliega en Cloud Run con éxito (manual mediante `workflow_dispatch`) |
| 3 | README.md profesional con instalación/requisitos/ejecución | **CUMPLE** |
| 4 | Aplicación desplegada en URL pública | **CUMPLE** — `https://tareas-blazor-3ziforwb7a-uc.a.run.app/` responde 200; revisión `tareas-blazor-00005-f5l` al 100 % |

---

### 1. Repositorio GitHub con historial limpio (Conventional Commits)
- **Estado: CUMPLE (parcial)**
- **Evidencia / Ausencia:**
  - **Repositorio:** remoto `origin` → `https://github.com/nicos92/PW_Tareas.git`; repo público. Rama remota: `main`; `develop` se integró mediante PR y se eliminó del remoto. Ramas locales de trabajo: `criterios`, `develop`, `u2`, `u2-editorconfig`, `u2-md3`, `u2-nginx`.
  - **Estado actual:** `main` = `30fa5af` (`ci: Develop (#22)`, `2026-11-02`); la rama por defecto es `main`.
  - **Commits recientes que cumplen el formato:**
    - `30fa5af ci: Develop (#22)`
    - `826745b ci: espera la sincronizacion del mirror antes de desplegar`
    - `2000c81 ci: despliega cloud run con un tag inmutable`
    - `304405c ci: migra el despliegue en cloud run a workload identity federation`
    - `684f88d ci: incorpora validacion de commits y pipeline de despliegue (#17)`
    - `6c50150 docs: documenta las variables de entorno y el despliegue en GCP`
    - `acb60a8 docs: agrega la guia de despliegue en Google Cloud Run`
    - `696e55d chore: habilita el procesamiento de forwarded headers en el contenedor`
  - **Historial actual:** 107 commits totales (**100 no-merge** + 7 merge commits). De los no-merge, **26 cumplen Conventional Commits** (26,0 %): `fix` (7), `docs` (7), `ci` (4), `feat` (3), `test` (2), `refactor` (2), `chore` (1). Los **74 restantes (74,0 %)** son historial legado sin prefijo, por ejemplo:
    - `agrega documentacion para el despligue en hugging face spaces`
    - `actualiza readme`, `agrega docker file`, `agrega docker ignore`
    - `agrega configrraciones para github`
    - `agrega comentario para simulacion`
    - `agrega mcp de drawio para opencode`
  - **Fortalezas incorporadas:**
    - `package.json` con `@commitlint/cli`, `@commitlint/config-conventional` y `husky`; script `prepare` para instalar hooks.
    - `commitlint.config.mjs` (Líneas 1-14): extiende la configuración convencional, ignora `Merge ...`/`Revert ...` y fija `header-max-length` en 100.
    - `.husky/commit-msg` (Línea 1): ejecuta `npx --no -- commitlint --edit "$1"` en cada commit local.
    - Workflow `ci.yml` (Líneas 22-54): job `commitlint` que valida el título del PR y el rango de commits del push/PR.
    - `CONTRIBUTING.md` (Líneas 1-72): documenta formato, tipos permitidos y ejemplos.
    - `.github/pull_request_template.md`: checklist que exige título y commits conformes.
    - `cliff.toml` (Líneas 1-16) y `CHANGELOG.md` (Líneas 1-25) para generar el changelog; tags semánticos `v0.1.0`, `v1.0.0`, `v1.0.1`.
  - **Qué falta:** el historial anterior al commit `684f88d` sigue sin cumplir (74,0 %). No se reescribe de forma deliberada porque las ramas ya están publicadas. Tampoco es verificable desde la API que la protección de rama exija los checks `commitlint`/`build-test` como obligatorios.
  - **Recomendación técnica:**
    1. Activar *branch protection* en `main` (y en `develop` si se vuelve a crear) con *Require status checks* (`commitlint`, `build-test`) y *Require pull request*.
    2. Habilitar *Squash and merge* con título de PR en formato Conventional Commits.
    3. Mantener `commitlint` + `husky`; para el historial legado, reescribir sólo ramas no compartidas con `git rebase -i` (nunca `main` sin coordinación).

---

### 2. CI/CD con GitHub Actions: build y deploy automático
- **Estado: CUMPLE**
- **Evidencia / Ausencia:**
  - **Workflows existentes:** `.github/workflows/` contiene `ci.yml` (86 líneas), `docker-publish.yml` (45 líneas) y `deploy-cloud-run.yml` (98 líneas). La API de GitHub reporta los **3 workflows activos**.
  - **`ci.yml` (build + test + validación):**
    - Disparadores: `push` y `pull_request` a `main`/`develop`, más `workflow_dispatch` (Líneas 3-9).
    - `permissions: contents: read`, `concurrency` con cancelación y variables `DOTNET_*` (Líneas 11-19).
    - Job `commitlint` (Líneas 22-54): instala Node 22 con caché npm, valida el título del PR y el rango de commits.
    - Job `build-test` (Líneas 56-86): .NET 8 con caché de NuGet, `dotnet restore`, `dotnet build -c Release`, `dotnet test -c Release` y publicación del artefacto TRX.
  - **`docker-publish.yml`:**
    - Disparadores: push a `main`, tags `v*` y ejecución manual (Líneas 3-8).
    - Construye y publica `docker.io/<usuario>/tareas-blazor:latest` y `:sha-<commit>` con caché de GitHub Actions (Líneas 32-45).
    - El job se omite si no existe la variable `DOCKERHUB_USERNAME` (Línea 20). Tras configurarla, la ejecución del `2026-11-02` publicó `latest` y `sha-<commit>` en Docker Hub correctamente.
  - **`deploy-cloud-run.yml`:**
    - Sólo se dispara manualmente con `workflow_dispatch` (Líneas 3-8) y requiere `GCP_PROJECT_ID`, `GCP_WORKLOAD_IDENTITY_PROVIDER`, `GCP_SERVICE_ACCOUNT` y `DOCKERHUB_USERNAME` (Líneas 23-35); se autentica con **Workload Identity Federation**, sin clave JSON.
    - Por defecto despliega el tag inmutable `sha-<commit>` (Líneas 46-52), espera hasta 5 minutos a que el mirror de Cloud Run sincronice la imagen (Líneas 54-67) y ejecuta `gcloud run deploy` con `--allow-unauthenticated`, variables de entorno y `max-instances=1` (Líneas 69-90).
    - Verifica `GET /health` con reintentos después del despliegue (Líneas 92-98).
    - **Ejecución exitosa:** run `41466777264` (`2026-11-02T14:56:51Z`) sobre `main` (`30fa5af`); desplegó la revisión `tareas-blazor-00005-f5l` al 100 % del tráfico con URL `https://tareas-blazor-3ziforwb7a-uc.a.run.app/`. Los 8 runs fallidos anteriores corresponden a la puesta a punto de credenciales WIF y a la sincronización inicial del mirror, no a fallos de la aplicación.
  - **Verificación remota (GitHub API):** `GET /actions/workflows` → `total_count: 3`; `GET /actions/runs` → `total_count: 35` (CI: 14 `success` y 2 `failure`; `docker-publish`: 4 `success`; `deploy-cloud-run`: 2 `success` y 8 `failure` de configuración); `GET /deployments` → 10 registros en el environment `production`; `GET /environments` → `total_count: 1` (`production`). El repo mantiene `homepage: null` y `has_pages: false`.
  - **Documentación:** `docs/CI_CD.md` (163 líneas) describe workflows, validación de commits, publicación en Docker Hub, Workload Identity Federation, tags inmutables y despliegue; `README.md` (Líneas 101-113) resume los tres workflows.
  - **Qué falta:** el build, los tests, la publicación de la imagen y el deploy están operativos, pero el **deploy sigue siendo manual** (`workflow_dispatch`), no se dispara automáticamente al publicar una imagen. Además, la URL pública todavía no está en el `README.md` ni en el campo `homepage` del repositorio.
  - **Recomendación técnica:**
    1. Añadir la URL `https://tareas-blazor-3ziforwb7a-uc.a.run.app/` al `README.md` y al campo `homepage` del repositorio.
    2. Proteger el environment `production` con reglas de aprobación si se requiere intervención manual.
    3. Opcional: encadenar el deploy a un tag `v*` o a `workflow_run` después de una publicación exitosa.

---

### 3. README.md profesional con instrucciones de instalación
- **Estado: CUMPLE**
- **Evidencia / Ausencia:**
  - **Ubicación:** `README.md` (247 líneas). Secciones evaluadas:
    - **Descripción y badges:** Líneas 1-18 (front matter de Hugging Face, badge de CI, badge de licencia GPL-3.0, stack Blazor Server .NET 8 y Clean Architecture).
    - **Características:** Líneas 20-28.
    - **Requisitos previos:** Líneas 30-40 (`.NET 8 SDK`, navegador, Node.js 22 para el hook de commits y `npm install`).
    - **Ejecución local:** Líneas 42-49 (`dotnet run --project src/Tareas.Web` y URLs locales).
    - **Docker/Podman:** Líneas 50-76 (`build`/`run` y tabla de variables de entorno).
    - **Despliegue:** Líneas 77-100 (Hugging Face Spaces y Cloud Run desde Docker Hub).
    - **CI/CD:** Líneas 101-113 (los tres workflows y enlace a `docs/CI_CD.md`).
    - **Salud del servicio:** Líneas 114-119 (`/health` y `/health/ready`).
    - **Arquitectura y estructura:** Líneas 120-154 (regla de dependencia, árbol y front end) y Líneas 155-166 (componentes reutilizables).
    - **Persistencia y migraciones:** Líneas 167-201 (SQLite, EF Core y comandos `dotnet ef`).
    - **Publicación y paquetes:** Líneas 202-218.
    - **Tests y validaciones:** Líneas 219-237.
    - **Licencia:** Líneas 238-247 (GPL-3.0, con `LICENSE` en la raíz).
  - **Observaciones menores:** el README ya incluye badge de CI, badge de licencia y health checks. La aplicación ya está desplegada en `https://tareas-blazor-3ziforwb7a-uc.a.run.app/`, pero **el README todavía no muestra esa URL**; conviene añadirla en la cabecera y en el campo `homepage` del repositorio.
  - **Cumplimiento:** cubre descripción, requisitos, instalación, ejecución local, Docker, variables de entorno, despliegue, arquitectura, persistencia, publicación y tests.

---

### 4. Aplicación desplegada y accesible en URL pública
- **Estado: CUMPLE**
- **Evidencia:**
  - **URL pública:** `https://tareas-blazor-3ziforwb7a-uc.a.run.app/` (también responde `https://tareas-blazor-907898926068.us-central1.run.app/`), en la región `us-central1`.
  - **Verificación HTTP (2026-11-02):** `/` → `200`, `/listaDeTareas` → `200`, `/categorias` → `200`, `/health` → `200` con `{"status":"ok"}` y `/health/ready` → `200`.
  - **Despliegue:** run `41466777264` (`2026-11-02T14:56:51Z`) sobre `main` (`30fa5af`); revisión `tareas-blazor-00005-f5l` sirviendo el 100 % del tráfico.
  - **Automatización:** `deploy-cloud-run.yml` se autentica con Workload Identity Federation, despliega el tag inmutable `sha-<commit>`, espera la sincronización del mirror y verifica `/health`. `docker-publish.yml` publica `latest` y `sha-<commit>` en Docker Hub.
  - **Verificación remota:** GitHub API del repo → `"homepage": null`, `"has_pages": false`; `GET /deployments` → 10 registros; `GET /environments` → `total_count: 1` (`production`). API de Hugging Face (`/api/spaces?author=nicos92`) → `[]` (sin Spaces públicos).
  - **Documentación pendiente:** el `README.md` todavía no incluye la URL pública y `docs/DESPLIEGUE_GCP.md` conserva placeholders (`TU_USUARIO_GITHUB`, `https://tareas-blazor-xxxxx-uc.a.run.app`) en sus ejemplos.
  - **Qué falta:** documentar la URL en el `README.md` y en el campo `homepage` del repositorio; opcionalmente agregar un badge de estado del servicio.
  - **Recomendación técnica:**
    1. Añadir `https://tareas-blazor-3ziforwb7a-uc.a.run.app/` en la cabecera del `README.md` y en `homepage` del repositorio.
    2. Actualizar `docs/DESPLIEGUE_GCP.md` con la URL real como ejemplo.
    3. Opcional: automatizar el deploy al crear un tag `v*` para que la URL se mantenga actualizada (ver punto 2).

---

## Conclusión

- **1 — Cumple parcialmente.** La convención ya está instalada y validada de forma automática (`commitlint`, `husky`, `ci.yml`, `CONTRIBUTING.md`), pero el 74,0 % del historial (74 de 100 commits no-merge) es legado sin prefijo y no se reescribe.
- **2 — Cumple.** Existen tres workflows activos: el CI de build/test/commitlint está verde, `docker-publish` publica la imagen en Docker Hub y `deploy-cloud-run` desplegó correctamente en Cloud Run con Workload Identity Federation. El deploy es manual (`workflow_dispatch`), pero ejecutable y verificado.
- **3 — Cumple.** `README.md` (247 líneas) es profesional y cubre descripción, requisitos, ejecución local, Docker, variables de entorno, despliegue, CI/CD, health checks, arquitectura, migraciones, publicación, tests y licencia.
- **4 — Cumple.** La aplicación está desplegada en `https://tareas-blazor-3ziforwb7a-uc.a.run.app/` y las rutas principales y de salud responden `200`. Falta sólo documentar la URL en el `README.md` y en `homepage`.
