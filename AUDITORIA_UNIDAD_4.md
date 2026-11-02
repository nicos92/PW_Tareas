# Auditoría de Código y DevOps — UNIDAD 4: Prácticas Profesionales

**Repositorio:** `nicos92/PW_Tareas` (`https://github.com/nicos92/PW_Tareas`) · **Rama auditada:** `develop` (`1237b04`) · **Rama por defecto:** `main` · **Visibilidad:** pública · **Fecha de auditoría:** 2026-11-01.

---

## Estado de Cumplimiento - UNIDAD 4

| # | Requisito | Estado |
|---|-----------|--------|
| 1 | Historial con Conventional Commits | **NO CUMPLE (parcial)** — 30/111 commits conformes; 75 sin prefijo |
| 2 | CI/CD con GitHub Actions (build + deploy) | **NO CUMPLE** — no existe `.github/workflows/` |
| 3 | README.md profesional con instalación/requisitos/ejecución | **CUMPLE** |
| 4 | Aplicación desplegada en URL pública | **NO CUMPLE** — sólo placeholders, sin URL real |

---

### 1. Repositorio GitHub con historial limpio (Conventional Commits)
- **Estado: NO CUMPLE (parcial)**
- **Evidencia / Ausencia:**
  - **Repositorio:** remoto `origin` → `https://github.com/nicos92/PW_Tareas.git`; repo público, 3 ramas remotas principales (`main`, `develop`, `criterios`) y varias de trabajo (`u2`, `u2-md3`, `u2-migration`, `u2-nginx`).
  - **Estructura de commits:** se observan prefijos válidos en la etapa reciente:
    - `1237b04 test: cubre restriccion de borrado de categorias`
    - `04e03d8 fix: impide eliminar categorias con tareas asociadas`
    - `190c353 feat: agrega gestion de categorias y relacion opcional con tareas`
    - `b67f0fa feat: agrega filtro y restauracion de tareas dadas de baja`
    - `6e743f0 refactor!: migra el proyecto a Clean Architecture por capas` (breaking change correctamente marcado con `!`)
    - Tipos usados: `fix` (9), `docs` (8), `feat` (6), `test` (4), `refactor` (2), `chore` (1) → **30 commits conformes**.
  - **Historial global:** 111 commits totales; **75 no siguen Conventional Commits** (≈68 %), por ejemplo:
    - `agrega documentacion para el despligue en hugging face spaces`
    - `actualiza readme`, `agrega docker file`, `agrega docker ignore`
    - `arregla responsive en lista de tareas`
    - `agrega mcp de drawio para opencode`
    - `agrega configrraciones para github`
  - Además hay **6 merge commits** (`Merge branch ...`, `Merge pull request ...`) y una rama con commits de prueba (`7fde3c7 agrega comentario para simulacion`).
  - **Fortalezas:** existen `cliff.toml` (Líneas 1-16) y `CHANGELOG.md` (Líneas 1-24) que evidencian intención de automatizar el changelog por tipo de commit, y tags semánticos `v0.1.0`, `v1.0.0`, `v1.0.1`.
  - **Qué falta:** no hay validación automática del formato (no existe `commitlint`, `husky`, hook `commit-msg` ni workflow de lint de PR). El historial antiguo no es reescribible sin riesgo porque las ramas ya están publicadas y compartidas.
  - **Recomendación técnica:**
    1. Agregar `commitlint` + `husky` (`.husky/commit-msg` → `npx commitlint --edit $1`) con `@commitlint/config-conventional`, o un workflow que valide el título del PR.
    2. Habilitar en GitHub la opción *Squash and merge* con título de PR en formato Conventional Commits y *Require status checks*.
    3. Documentar la convención en `CONTRIBUTING.md`; para el historial existente, opcionalmente reescribir sólo ramas no compartidas con `git rebase -i` (nunca `main`/`develop` sin coordinación).

---

### 2. CI/CD con GitHub Actions: build y deploy automático
- **Estado: NO CUMPLE**
- **Evidencia / Ausencia:**
  - **Archivo de workflow:** **no existe** `.github/workflows/` (ni `.yaml`/`.yml` de Actions) en ninguna rama. `git log --all -- .github/workflows` no devuelve resultados.
  - Contenido actual de `.github/`: sólo `CODEOWNERS` (Líneas 1-9), `ISSUE_TEMPLATE/user_story.md` y `pull_request_template.md`. No hay pipelines.
  - Verificación remota (GitHub API): `GET /repos/nicos92/PW_Tareas/actions/workflows` → `total_count: 0`; `GET /actions/runs` → `total_count: 0`; `GET /deployments` → `[]`; `GET /environments` → `total_count: 0`.
  - Lo que sí existe es **build y despliegue manual**:
    - `Dockerfile` (Líneas 1-44) con build multi-stage y runtime, pero se ejecuta a mano con `docker build`.
    - `docs/DESPLIEGUE_GCP.md` (Líneas 138-181) **propone** un `cloudbuild.yaml` y un trigger de Cloud Build, pero ese archivo **no está commiteado** en la raíz (`ls cloudbuild.yaml` → no existe).
    - `docs/DESPLIEGUE_HF.md` (Líneas 48-58) depende del build automático de Hugging Face al hacer push, no de GitHub Actions.
  - **Recomendación técnica:** crear `.github/workflows/ci.yml` (en `push`/`pull_request` a `main`/`develop`) con:
    ```yaml
    name: ci
    on:
      push: { branches: [main, develop] }
      pull_request: { branches: [main, develop] }
    jobs:
      build:
        runs-on: ubuntu-latest
        steps:
          - uses: actions/checkout@v4
          - uses: actions/setup-dotnet@v4
            with: { dotnet-version: '8.0.x' }
          - run: dotnet restore
          - run: dotnet build --no-restore -c Release
          - run: dotnet test --no-build -c Release
    ```
    y un `deploy.yml` (o un job `deploy` condicionado a `main`) con `google-github-actions/auth`, `docker build/push` a Artifact Registry y `google-github-actions/deploy-cloudrun`, o bien push al Space de Hugging Face con `huggingface_hub`. Añadir `concurrency`, caché de NuGet y *environments* con secretos (`GCP_SA_KEY`, `HF_TOKEN`).

---

### 3. README.md profesional con instrucciones de instalación
- **Estado: CUMPLE**
- **Evidencia / Ausencia:**
  - **Ubicación:** `README.md` (180 líneas). Secciones evaluadas:
    - **Descripción:** Líneas 11-14 (propósito, stack Blazor Server .NET 8, Clean Architecture) y Líneas 16-24 (características).
    - **Requisitos previos:** Líneas 26-29 (`.NET 8 SDK`, navegador moderno).
    - **Instalación / ejecución:** Líneas 31-37 (`dotnet run --project src/Tareas.Web` y URLs locales), Líneas 39-46 (Docker `build`/`run`), Líneas 48-55 (variables de entorno del contenedor).
    - **Despliegue:** Líneas 57-69 con enlaces a `docs/DESPLIEGUE_HF.md` y `docs/DESPLIEGUE_GCP.md`.
    - **Arquitectura y estructura:** Líneas 71-104 (regla de dependencia y árbol de carpetas).
    - **Persistencia y migraciones:** Líneas 118-144 (ruta de SQLite, EF Core y comandos `dotnet ef`).
    - **Publicación y tests:** Líneas 146-152 (`dotnet publish`) y Líneas 163-167 (`dotnet test`).
    - **Validaciones:** Líneas 169-180 (tabla de reglas de negocio).
  - **Observaciones menores:** el README incluye front matter de Hugging Face (Líneas 1-9), pero no muestra una URL pública, un badge de CI ni una licencia. No impide el cumplimiento del requisito (instalación, requisitos y ejecución están claros), pero conviene añadir esos elementos cuando exista despliegue.

---

### 4. Aplicación desplegada y accesible en URL pública
- **Estado: NO CUMPLE**
- **Evidencia / Ausencia:**
  - **URL pública:** **no declarada**. El `README.md` no contiene ningún enlace de aplicación desplegada; sólo URLs locales (`http://localhost:8080`, Líneas 37 y 46).
  - **Documentación con placeholders, no URLs reales:**
    - `docs/DESPLIEGUE_HF.md` (Líneas 54-56) → `https://huggingface.co/spaces/TU_USUARIO/pw-tareas` (plantilla).
    - `docs/DESPLIEGUE_GCP.md` (Líneas 236-240 y 306-315) → `https://tareas-blazor-xxxxx-uc.a.run.app` (ejemplo).
    - `docs/NGINX_GUIDE.md` (Líneas 102-103, 317-319) → `http://MI_IP:8081` (ejemplo didáctico).
  - **Verificación remota:** GitHub API del repo → `"homepage": null`, `"has_pages": false`; no hay deployments ni environments. API de Hugging Face (`/api/spaces?author=nicos92`) → `[]` (sin Spaces públicos). Búsqueda de URLs reales (`*.run.app`, `*.github.io`, `huggingface.co/spaces/<usuario>`, `*.azurewebsites.net`, `*.vercel.app`) en todo el historial → sin resultados distintos de placeholders.
  - **Qué falta:** un artefacto desplegado y su enlace verificable (HTTP 200) documentado en el README. Existe Dockerfile y guías de despliegue, lo que demuestra preparación, pero no despliegue activo.
  - **Recomendación técnica:**
    1. Desplegar en Cloud Run (`gcloud run deploy ... --allow-unauthenticated`), Hugging Face Spaces (SDK Docker) o Azure App Service usando el `Dockerfile` existente.
    2. Verificar accesibilidad (`curl -sI https://<url> | head -1` → `200`) y rutas clave (`/`, `/listaDeTareas`, `/categorias`, `/configuracion`).
    3. Añadir la URL en la cabecera del `README.md` y en `homepage` del repositorio; agregar badge de estado y un health check (`/health` o equivalente) para monitoreo.
    4. Integrar el deploy en el workflow de GitHub Actions para que la URL se mantenga actualizada (ver punto 2).

---

## Conclusión

- **1 — Parcial.** La práctica de Conventional Commits está instalada en la etapa reciente y respaldada por `cliff.toml`/`CHANGELOG.md`, pero el 68 % del historial (75 de 111 commits) no cumple el formato y no hay validación automatizada.
- **2 — No cumple.** No existe ningún workflow de GitHub Actions ni ejecución registrada; el build/deploy es manual (Docker/`gcloud`) o delegado a Hugging Face, con `cloudbuild.yaml` sólo como ejemplo documentado.
- **3 — Cumple.** `README.md` es profesional y cubre descripción, requisitos (.NET 8), ejecución local, Docker, variables de entorno, arquitectura, migraciones, publicación y tests.
- **4 — No cumple.** No hay URL pública real ni evidencia de despliegue; sólo plantillas (`TU_USUARIO`, `xxxxx`) y guías de despliegue. El repositorio está listo para desplegarse (Dockerfile + documentación), pero el requisito exige una instancia accesible.
