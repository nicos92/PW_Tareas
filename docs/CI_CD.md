# Integración y entrega continua

Este documento describe los workflows de GitHub Actions y el flujo de publicación
de la imagen en Docker Hub para desplegarla en Google Cloud Run.

## Workflows

| Archivo | Cuándo se ejecuta | Qué hace |
|---|---|---|
| `.github/workflows/ci.yml` | Push y pull request a `main`/`develop`; ejecución manual | Valida Conventional Commits, restaura, compila en Release y ejecuta las 76 pruebas. Publica el resultado TRX como artefacto. |
| `.github/workflows/docker-publish.yml` | Push a `main`, tags `v*`; ejecución manual | Compila el `Dockerfile` y publica `latest` y `sha-<commit>` en Docker Hub. |
| `.github/workflows/deploy-cloud-run.yml` | Ejecución manual | Despliegue opcional en Cloud Run usando una cuenta de servicio de GCP. |

## Validación de commits

El workflow `ci.yml` valida:

- el título del pull request;
- el rango de commits del pull request;
- el rango de commits nuevo en cada push (no todo el historial).

Los mensajes de merge se ignoran. El historial anterior a la incorporación de
esta política es legado y no se reescribe. La configuración local está en
`commitlint.config.mjs` y el hook en `.husky/commit-msg`.

Para habilitar el hook después de clonar:

```bash
npm install
```

## Publicación de la imagen

Imagen pública:

```text
docker.io/nicos92/tareas-blazor:latest
docker.io/nicos92/tareas-blazor:sha-<commit>
```

Para que el workflow publique automáticamente hacen falta:

| Tipo | Nombre | Valor |
|---|---|---|
| Variable | `DOCKERHUB_USERNAME` | `nicos92` |
| Secreto | `DOCKERHUB_TOKEN` | Token de Docker Hub con permiso de escritura |

Mientras no exista la variable `DOCKERHUB_USERNAME`, el job de publicación se
omite y el workflow de CI termina correctamente.

También se puede publicar localmente con Podman:

```bash
podman build -t docker.io/nicos92/tareas-blazor:latest .
podman push docker.io/nicos92/tareas-blazor:latest
```

## Despliegue en Cloud Run desde la consola

El despliegue habitual se hace desde el navegador:

1. Abrir Cloud Run en Google Cloud Console.
2. Crear un servicio o editar `tareas-blazor`.
3. Elegir **Imagen de contenedor** y usar el tag inmutable
   `docker.io/nicos92/tareas-blazor:sha-<commit>`. Evitá `latest`: Cloud Run
   cachea las imágenes públicas de Docker Hub hasta una hora y puede desplegar
   una versión anterior.
4. Configurar:
   - puerto `8080`;
   - invocaciones sin autenticar (`--allow-unauthenticated`);
   - CPU 1, memoria 512 MiB;
   - mínimo de instancias 0 y máximo 1;
   - timeout 3600 s;
   - variables:
     - `ASPNETCORE_ENVIRONMENT=Production`
     - `DATA_DIR=/tmp/data`
     - `Storage__UploadsPhysicalPath=/tmp/uploads`
5. Desplegar y copiar la URL del servicio.

Verificación:

```bash
curl -fsS https://<url>/health
curl -s -o /dev/null -w '%{http_code}\n' https://<url>/
curl -s -o /dev/null -w '%{http_code}\n' https://<url>/listaDeTareas
```

`/health` debe responder `200` con `{"status":"ok"}`. Para comprobar la base de
datos, usar `/health/ready`.

## Despliegue automático opcional

El workflow `deploy-cloud-run.yml` permite desplegar desde GitHub Actions. No se
ejecuta automáticamente; hay que lanzarlo a mano y configurar previamente. Por
defecto usa el tag inmutable `sha-<commit>` del commit que ejecuta el workflow;
evitá `latest` porque Cloud Run cachea las imágenes públicas de Docker Hub hasta
una hora y puede desplegar una versión anterior.

| Tipo | Nombre | Valor |
|---|---|---|
| Variable | `GCP_PROJECT_ID` | ID del proyecto de GCP |
| Variable | `GCP_WORKLOAD_IDENTITY_PROVIDER` | Nombre completo del proveedor WIF |
| Variable | `GCP_SERVICE_ACCOUNT` | Email de la cuenta de servicio |
| Variable | `GCP_REGION` | `us-central1` (o la región elegida) |
| Variable | `CLOUD_RUN_SERVICE` | `tareas-blazor` (opcional) |
| Variable | `DOCKERHUB_USERNAME` | `nicos92` |

El workflow se autentica con **Workload Identity Federation** (OIDC), sin claves
JSON. Antes de ejecutarlo hay que:

1. Habilitar las APIs `iam.googleapis.com`, `iamcredentials.googleapis.com`,
   `sts.googleapis.com` y `run.googleapis.com`.
2. Crear un pool y un proveedor OIDC para GitHub Actions con issuer
   `https://token.actions.githubusercontent.com` y los mapeos
   `google.subject=assertion.sub`, `attribute.repository=assertion.repository` y
   `attribute.repository_owner=assertion.repository_owner`.
3. Restringir el proveedor con una condición como
   `assertion.repository == 'nicos92/PW_Tareas'`.
4. Otorgar `roles/iam.workloadIdentityUser` al principal set del repositorio
   sobre la cuenta de servicio.
5. Dar a la cuenta de servicio los roles `roles/run.admin` y
   `roles/iam.serviceAccountUser`.

Ejemplo de valores para este repositorio:

| Variable | Valor |
|---|---|
| `GCP_PROJECT_ID` | `project-f8477ab4-3866-4a40-82c` |
| `GCP_WORKLOAD_IDENTITY_PROVIDER` | `projects/907898926068/locations/global/workloadIdentityPools/github-pool/providers/github` |
| `GCP_SERVICE_ACCOUNT` | `gh-deployer@project-f8477ab4-3866-4a40-82c.iam.gserviceaccount.com` |

Mientras falte alguna de las variables `GCP_PROJECT_ID`,
`GCP_WORKLOAD_IDENTITY_PROVIDER`, `GCP_SERVICE_ACCOUNT` o
`DOCKERHUB_USERNAME`, el job se omite.

Comando equivalente al que ejecuta el workflow:

```bash
gcloud run deploy tareas-blazor \
  --image=docker.io/nicos92/tareas-blazor:sha-<commit> \
  --region=us-central1 \
  --platform=managed \
  --port=8080 \
  --allow-unauthenticated \
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Production,DATA_DIR=/tmp/data,Storage__UploadsPhysicalPath=/tmp/uploads" \
  --cpu=1 \
  --memory=512Mi \
  --concurrency=80 \
  --min-instances=0 \
  --max-instances=1 \
  --timeout=3600
```

## Notas

- Cloud Run puede bajar imágenes públicas de Docker Hub sin credenciales de
  registry.
- La base SQLite y los adjuntos son efímeros en Cloud Run. Con
  `max-instances=1` todos los visitantes comparten la misma instancia y la misma
  base mientras la instancia esté viva.
- Para persistir adjuntos, usar el montaje de Cloud Storage descrito en
  `docs/DESPLIEGUE_GCP.md`.
