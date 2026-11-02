# Auditoría de Código y DevOps — UNIDAD 4: Prácticas Profesionales

**Repositorio:** `nicos92/PW_Tareas` (`https://github.com/nicos92/PW_Tareas`) · **Rama auditada:** `develop` (`684f88d`) · **Rama por defecto:** `main` · **Visibilidad:** pública · **Fecha de auditoría:** 2026-11-02.

> **Nota de actualización:** esta versión reemplaza la auditoría anterior (basada en `1237b04`) y refleja el estado posterior al commit `684f88d ci: incorpora validacion de commits y pipeline de despliegue (#17)`, que agregó `commitlint`, `husky`, los workflows de GitHub Actions, `CONTRIBUTING.md` y `LICENSE`.

---

## Estado de Cumplimiento - UNIDAD 4

| # | Requisito | Estado |
|---|-----------|--------|
| 1 | Historial con Conventional Commits | **CUMPLE (parcial)** — validación local y en CI; 22/96 commits no-merge conformes y 74 de legado sin prefijo |
| 2 | CI/CD con GitHub Actions (build + deploy) | **CUMPLE (parcial)** — `ci.yml` verde (build + test + commitlint); `docker-publish` y `deploy-cloud-run` definidos, pero el deploy es manual y no se ejecutó |
| 3 | README.md profesional con instalación/requisitos/ejecución | **CUMPLE** |
| 4 | Aplicación desplegada en URL pública | **NO CUMPLE** — no hay URL real; `homepage: null` y sin deployments |

---

### 1. Repositorio GitHub con historial limpio (Conventional Commits)
- **Estado: CUMPLE (parcial)**
- **Evidencia / Ausencia:**
  - **Repositorio:** remoto `origin` → `https://github.com/nicos92/PW_Tareas.git`; repo público. Ramas remotas: `main` y `develop`. Ramas locales adicionales de trabajo: `criterios`, `u2`, `u2-editorconfig`, `u2-md3`, `u2-nginx`.
  - **Estado actual:** `develop` = `origin/develop` = `684f88d` (`2026-11-01`); la rama por defecto es `main`.
  - **Commits recientes que cumplen el formato:**
    - `684f88d ci: incorpora validacion de commits y pipeline de despliegue (#17)`
    - `6c50150 docs: documenta las variables de entorno y el despliegue en GCP`
    - `acb60a8 docs: agrega la guia de despliegue en Google Cloud Run`
    - `696e55d chore: habilita el procesamiento de forwarded headers en el contenedor`
    - `5a969f0 test: agrega regresiones de la ruta de base y del almacenamiento de adjuntos`
    - `6abcac2 fix: cae a una carpeta temporal cuando la ruta de la base no admite escritura`
    - `b2e32fd feat: agrega la ruta fisica de adjuntos separada de la carpeta publica`
  - **Historial actual:** 103 commits totales (**96 no-merge** + 7 merge commits). De los no-merge, **22 cumplen Conventional Commits** (22,9 %): `fix` (7), `docs` (6), `feat` (3), `test` (2), `refactor` (2), `ci` (1), `chore` (1). Los **74 restantes (77,1 %)** son historial legado sin prefijo, por ejemplo:
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
  - **Qué falta:** el historial anterior al commit `684f88d` sigue sin cumplir (77,1 %). No se reescribe de forma deliberada porque las ramas ya están publicadas. Tampoco es verificable desde la API que la protección de rama exija los checks `commitlint`/`build-test` como obligatorios.
  - **Recomendación técnica:**
    1. Activar *branch protection* en `main` y `develop` con *Require status checks* (`commitlint`, `build-test`) y *Require pull request*.
    2. Habilitar *Squash and merge* con título de PR en formato Conventional Commits.
    3. Mantener `commitlint` + `husky`; para el historial legado, reescribir sólo ramas no compartidas con `git rebase -i` (nunca `main`/`develop` sin coordinación).

---

### 2. CI/CD con GitHub Actions: build y deploy automático
- **Estado: CUMPLE (parcial)**
- **Evidencia / Ausencia:**
  - **Workflows existentes:** `.github/workflows/` contiene `ci.yml` (86 líneas), `docker-publish.yml` (45 líneas) y `deploy-cloud-run.yml` (72 líneas). La API de GitHub reporta los **3 workflows activos**.
  - **`ci.yml` (build + test + validación):**
    - Disparadores: `push` y `pull_request` a `main`/`develop`, más `workflow_dispatch` (Líneas 3-9).
    - `permissions: contents: read`, `concurrency` con cancelación y variables `DOTNET_*` (Líneas 11-19).
    - Job `commitlint` (Líneas 22-54): instala Node 22 con caché npm, valida el título del PR y el rango de commits.
    - Job `build-test` (Líneas 56-86): .NET 8 con caché de NuGet, `dotnet restore`, `dotnet build -c Release`, `dotnet test -c Release` y publicación del artefacto TRX.
  - **`docker-publish.yml`:**
    - Disparadores: push a `main`, tags `v*` y ejecución manual (Líneas 3-8).
    - Construye y publica `docker.io/<usuario>/tareas-blazor:latest` y `:sha-<commit>` con caché de GitHub Actions (Líneas 32-45).
    - El job se omite si no existe la variable `DOCKERHUB_USERNAME` (Línea 20); la única ejecución registrada quedó en `skipped`, por lo que la imagen no se está publicando todavía.
  - **`deploy-cloud-run.yml`:**
    - Sólo se dispara manualmente con `workflow_dispatch` (Líneas 3-8) y requiere `GCP_PROJECT_ID`, `DOCKERHUB_USERNAME` y el secreto `GCP_SA_KEY` (Líneas 23-30).
    - Usa `google-github-actions/auth`, `gcloud run deploy` con `--allow-unauthenticated`, variables de entorno, `max-instances=1` y verificación final de `GET /health` con reintentos (Líneas 42-72).
    - **No registra ninguna ejecución**: `GET /actions/workflows/deploy-cloud-run.yml/runs` → `total_count: 0`.
  - **Verificación remota (GitHub API):** `GET /actions/workflows` → `total_count: 3`; `GET /actions/runs` → `total_count: 5` (CI en `main`, `develop` y PR, todas `success`); `GET /deployments` → `[]`; `GET /environments` → `total_count: 0`.
  - **Documentación:** `docs/CI_CD.md` (132 líneas) describe workflows, validación de commits, publicación en Docker Hub y despliegue; `README.md` (Líneas 101-113) resume los tres workflows.
  - **Qué falta:** el build y los tests están automatizados y verdes, pero el **deploy no es automático**: `deploy-cloud-run.yml` es manual, nunca se ejecutó y no hay deployments ni environments registrados. La publicación de la imagen tampoco ocurre porque falta configurar la variable/secreto de Docker Hub.
  - **Recomendación técnica:**
    1. Configurar `DOCKERHUB_USERNAME` (variable) y `DOCKERHUB_TOKEN` (secreto) para que `docker-publish.yml` publique la imagen.
    2. Configurar `GCP_PROJECT_ID`, `GCP_REGION`, `CLOUD_RUN_SERVICE` (variables) y `GCP_SA_KEY` (secreto), ejecutar `deploy-cloud-run.yml` y registrar la URL resultante.
    3. Opcional: encadenar el deploy a un tag `v*` o a `workflow_run` después de una publicación exitosa, y proteger el environment `production`.

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
  - **Observaciones menores:** el README ya incluye badge de CI, badge de licencia y health checks. **Sigue sin mostrar una URL pública de la aplicación**; cuando exista despliegue conviene añadirla en la cabecera y en el campo `homepage` del repositorio.
  - **Cumplimiento:** cubre descripción, requisitos, instalación, ejecución local, Docker, variables de entorno, despliegue, arquitectura, persistencia, publicación y tests.

---

### 4. Aplicación desplegada y accesible en URL pública
- **Estado: NO CUMPLE**
- **Evidencia / Ausencia:**
  - **URL pública:** **no declarada**. El `README.md` sólo contiene URLs locales (`https://localhost:7245`, `http://localhost:5017` en Línea 48; `http://localhost:8080` en Línea 66) y enlaces a GitHub/Licencia; no hay enlace a una instancia desplegada.
  - **Documentación con placeholders, no URLs reales:**
    - `docs/DESPLIEGUE_HF.md` (Líneas 34, 45 y 55) → `https://huggingface.co/spaces/TU_USUARIO/pw-tareas` (plantilla).
    - `docs/DESPLIEGUE_GCP.md` (Líneas 179, 243 y 315) → `TU_USUARIO_GITHUB` y `https://tareas-blazor-xxxxx-uc.a.run.app` (ejemplos).
    - `docs/NGINX_GUIDE.md` (Líneas 102-103) → `http://MI_IP:8081` / `http://MI_IP:8082` (ejemplo didáctico).
  - **Verificación remota:** GitHub API del repo → `"homepage": null`, `"has_pages": false`; `GET /deployments` → `[]`; `GET /environments` → `total_count: 0`. API de Hugging Face (`/api/spaces?author=nicos92`) → `[]` (sin Spaces públicos).
  - **Qué falta:** un artefacto desplegado y su enlace verificable (HTTP 200) documentado en el README. El repositorio ya está preparado:
    - `Dockerfile` (44 líneas) multi-stage listo para Cloud Run/Hugging Face.
    - `deploy-cloud-run.yml` puede desplegar y autoverificar `/health`, pero **nunca se ejecutó**.
    - Health checks implementados en `src/Tareas.Web/Program.cs` (Líneas 55-57) y documentados en `README.md` (Líneas 114-119).
  - **Recomendación técnica:**
    1. Ejecutar `deploy-cloud-run.yml` (configurando `GCP_SA_KEY`, `GCP_PROJECT_ID`, etc.) o desplegar manualmente en Cloud Run/Hugging Face Spaces usando el `Dockerfile` existente.
    2. Verificar accesibilidad (`curl -sI https://<url> | head -1` → `200`) y rutas clave (`/`, `/listaDeTareas`, `/categorias`, `/health`).
    3. Añadir la URL en la cabecera del `README.md` y en `homepage` del repositorio; agregar badge de estado.
    4. Opcional: automatizar el deploy al crear un tag `v*` para que la URL se mantenga actualizada (ver punto 2).

---

## Conclusión

- **1 — Cumple parcialmente.** La convención ya está instalada y validada de forma automática (`commitlint`, `husky`, `ci.yml`, `CONTRIBUTING.md`), pero el 77,1 % del historial (74 de 96 commits no-merge) es legado sin prefijo y no se reescribe.
- **2 — Cumple parcialmente.** Existen tres workflows activos y el CI de build/test/commitlint está verde en `main`, `develop` y PR. Sin embargo, la publicación de la imagen se omite por falta de configuración y el deploy a Cloud Run es manual y no se ejecutó (0 runs, 0 deployments).
- **3 — Cumple.** `README.md` (247 líneas) es profesional y cubre descripción, requisitos, ejecución local, Docker, variables de entorno, despliegue, CI/CD, health checks, arquitectura, migraciones, publicación, tests y licencia.
- **4 — No cumple.** No hay URL pública real ni evidencia de despliegue; sólo plantillas (`TU_USUARIO`, `xxxxx`, `MI_IP`) y guías. El repositorio está listo para desplegarse, pero falta ejecutar el despliegue y documentar la URL.
