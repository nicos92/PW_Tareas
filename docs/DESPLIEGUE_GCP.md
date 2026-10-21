# Despliegue en Google Cloud (Cloud Run + Artifact Registry)

Esta aplicación Blazor Server se dockeriza con el `Dockerfile` de la raíz del
proyecto y sirve en el **puerto 8080**. Para publicarla en Google Cloud el
flujo es de tres pasos:

```
Dockerfile  ──docker build──►  imagen local  ──docker push──►  Artifact Registry
                                                                    │
                                                            gcloud run deploy
                                                                    ▼
                                                          Cloud Run (URL https)
```

| Pieza | Qué es |
|---|---|
| **Artifact Registry** | Repositorio privado de imágenes Docker dentro del proyecto de GCP. Reemplaza a Docker Hub. |
| **Cloud Run** | Runtime serverless: ejecuta el contenedor, escala a cero y expone una URL HTTPS sin administrar servidores. |
| **Cloud Build** *(opcional)* | Compila y publica la imagen automáticamente, sin Docker en la máquina local. |

> **Resumen del flujo completo**
> 1. Crear el repositorio de imágenes en Artifact Registry.
> 2. `docker build` + `docker push` de la imagen.
> 3. `gcloud run deploy` de esa imagen en un servicio público de Cloud Run.

---

## 1. Requisitos previos

### 1.1 En Google Cloud

- Un **proyecto de GCP con billing habilitado** (Cloud Run y Artifact Registry
  cobran por uso).
- La **Google Cloud CLI** (`gcloud`) instalada y autenticada:

  ```bash
  gcloud auth login
  gcloud auth application-default login
  gcloud config set project TU_PROJECT_ID
  gcloud config set run/region us-central1
  gcloud config set artifacts/location us-central1
  ```

  Verificá el proyecto activo con `gcloud config list`.

- **Docker** instalado y corriendo en la máquina donde hacés el build.

### 1.2 APIs a habilitar

```bash
gcloud services enable \
  run.googleapis.com \
  artifactregistry.googleapis.com \
  cloudbuild.googleapis.com \
  logging.googleapis.com
```

### 1.3 Permisos de la cuenta con la que operás

| Permiso | Para qué |
|---|---|
| `roles/artifactregistry.admin` | Crear el repositorio y subir imágenes. |
| `roles/run.admin` | Crear y actualizar el servicio de Cloud Run. |
| `roles/storage.admin` | Crear el bucket de imágenes (sólo si usás el Plan B del punto 4.3). |
| `roles/logging.viewer` | Ver logs con `gcloud run logs read`. |

---

## 2. Publicar la imagen en Docker

### 2.1 Crear el repositorio en Artifact Registry

```bash
gcloud artifacts repositories create tareas-docker \
  --repository-format=docker \
  --location=us-central1 \
  --description="Imagen de PW Tareas (Blazor Server)"

gcloud artifacts repositories list
```

> La **región** del repositorio y la del servicio de Cloud Run son independientes,
> pero conviene usar la misma (`us-central1` en este documento) para que la
> imagen viaje por la red interna y no por internet.

### 2.2 Autenticar a Docker contra Artifact Registry

```bash
gcloud auth configure-docker us-central1-docker.pkg.dev
```

Esto agrega el host de Artifact Registry a `~/.docker/config.json`. No hace
falta `docker login` con usuario y contraseña.

### 2.3 Construir la imagen

```bash
export PROJECT_ID=$(gcloud config get-value project)
export REGION=us-central1
export IMAGE=us-central1-docker.pkg.dev/$PROJECT_ID/tareas-docker/tareas-blazor:1.0.0

docker build -t $IMAGE .
```

El `Dockerfile` ya resuelve todo lo necesario: build en etapas con el SDK de .NET 8,
runtime con el ASP.NET Core 8, usuario sin privilegios (uid 1000), escucha en el
puerto 8080 y compatibilidad con proxies que terminan TLS.

**Probá la imagen localmente antes de publicarla:**

```bash
docker run --rm -p 8080:8080 $IMAGE
```

Abrí `http://localhost:8080`. La base SQLite se crea sola en `/app/data` y las
migraciones de EF Core se aplican al arrancar.

> **El contenedor no expone `/app/data` al host.** Si necesitás conservar la base
> entre ejecuciones locales, montá un volumen:
> `docker run --rm -p 8080:8080 -v tareas-data:/app/data $IMAGE`.

### 2.4 Publicar (push) la imagen

```bash
docker push $IMAGE
```

Verificá que quedó:

```bash
gcloud artifacts docker images list \
  us-central1-docker.pkg.dev/$PROJECT_ID/tareas-docker/tareas-blazor \
  --include-tags
```

---

## 3. Alternativa: Cloud Build (build y push automático)

Si preferís no tener Docker en la máquina local, Cloud Build puede compilar la
imagen desde el repositorio de GitHub y publicarla en Artifact Registry.

Guardá este contenido como `cloudbuild.yaml` en la raíz del repositorio:

```yaml
steps:
  - name: gcr.io/cloud-builders/docker
    args: ['build', '-t', '${_IMAGE}', '.']
  - name: gcr.io/cloud-builders/docker
    args: ['push', '${_IMAGE}']

images:
  - '${_IMAGE}'

substitutions:
  _IMAGE: 'us-central1-docker.pkg.dev/$PROJECT_ID/tareas-docker/tareas-blazor:1.0.0'

options:
  logging: CLOUD_LOGGING_ONLY
```

Ejecución manual desde la consola:

```bash
gcloud builds submit --config cloudbuild.yaml
```

Disparo automático en cada `push` a `main` (creá el trigger desde **Cloud Build →
Triggers → Create trigger → GitHub**, conectando el repositorio `PW_Tareas` con
una conexión de repositorio de 2ª generación):

```bash
gcloud builds triggers create github \
  --repo-name=PW_Tareas \
  --repo-owner=TU_USUARIO_GITHUB \
  --branch-pattern=main \
  --build-config=cloudbuild.yaml
```

Luego, el despliegue a Cloud Run se hace igual, apuntando a la etiqueta publicada
(punto 4).

---

## 4. Despliegue en Cloud Run

### 4.1 Qué cambia respecto de correr el contenedor en un servidor común

Cloud Run no es "un servidor con Docker": hay tres diferencias que condicionan
la configuración. Las tres están resueltas en la imagen o en el comando de
despliegue de este documento.

| Diferencia | Efecto en esta app | Cómo se resuelve |
|---|---|---|
| **El proxy termina el TLS.** Cloud Run (Google Front End) recibe HTTPS y le habla HTTP al contenedor. | Kestrel ve `http` y no sabe que el cliente está en HTTPS. Si además se define un puerto HTTPS (`ASPNETCORE_HTTPS_PORT`), `UseHttpsRedirection()` genera un loop de 307. | El `Dockerfile` define `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`: la app procesa `X-Forwarded-Proto` e `X-Forwarded-For` y conoce el esquema real. |
| **El sistema de archivos es efímero.** Es en memoria y se pierde cuando la instancia se detiene. | La base SQLite y los adjuntos desaparecen en cada arranque en frío. | `DATA_DIR=/tmp/data` y `Storage__UploadsPhysicalPath=/tmp/uploads` (carpetas escribibles y efímeras). Ver el punto 7. |
| **Cada instancia es independiente.** No hay disco compartido entre réplicas. | Con más de una instancia, cada una tiene **su propia** base SQLite y dos visitantes ven tareas distintas. | `--max-instances=1` (obligatorio mientras la base sea SQLite). |

> **Verificado en esta aplicación:** con el contenedor tal como está, una
> petición a `/` detrás de un proxy que envía `X-Forwarded-Proto: https`
> responde `200` (no hay loop) y la ruta de la base se informa en el log al
> arrancar. El loop de redirección sólo aparece si además se configura
> `ASPNETCORE_HTTPS_PORT`, y con los *forwarded headers* activados ni siquiera
> en ese caso.

### 4.2 Deploy mínimo (todo efímero)

```bash
gcloud run deploy tareas-blazor \
  --image=$IMAGE \
  --region=us-central1 \
  --platform=managed \
  --port=8080 \
  --allow-unauthenticated \
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Production;DATA_DIR=/tmp/data;Storage__UploadsPhysicalPath=/tmp/uploads" \
  --cpu=1 \
  --memory=512Mi \
  --concurrency=80 \
  --min-instances=0 \
  --max-instances=1 \
  --timeout=3600
```

Qué hace cada parte y por qué está así:

| Flag | Motivo |
|---|---|
| `--port=8080` | Cloud Run espera que el contenedor escuche en `$PORT` (8080 por defecto). El `Dockerfile` ya fija `ASPNETCORE_URLS=http://+:8080`: si cambiás el puerto acá, cambiá también la variable. |
| `--allow-unauthenticated` | Deja el servicio público (sin login de Google). Es lo que se quiere en una demo. |
| `--set-env-vars` | Ver la tabla del punto 5. |
| `--max-instances=1` | Crítico: con más de una instancia **cada una tiene su propia base SQLite**, así que dos visitantes verían tareas distintas. |
| `--min-instances=0` | Escala a cero cuando no hay tráfico (se paga sólo por lo que se usa). Ver el costo en el punto 9. |
| `--timeout=3600` | Las conexiones WebSocket de Blazor Server (circuitos interactivos) son requests de larga duración. 3600 s es el máximo permitido en Cloud Run. |
| `--memory=512Mi` | Es el default de Cloud Run. Bajarlo a 256Mi es posible para una app tan chica, pero 512Mi deja margen para el filesystem en memoria. |

Al terminar, `gcloud` imprime la URL del servicio:

```
Service URL: https://tareas-blazor-xxxxx-uc.a.run.app
```

### 4.3 Deploy con imágenes persistentes (Plan B)

Si querés que **las imágenes adjuntas sobrevivan** a los reinicios de la
instancia, montá un bucket de Cloud Storage en la carpeta de uploads:

```bash
export BUCKET=pw-tareas-uploads
export REGION=us-central1

gcloud storage buckets create gs://$BUCKET --location=$REGION --uniform-bucket-level-access

gcloud run deploy tareas-blazor \
  --image=$IMAGE \
  --region=$REGION \
  --platform=managed \
  --port=8080 \
  --allow-unauthenticated \
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Production;DATA_DIR=/tmp/data" \
  --add-volume=name=uploads,type=cloud-storage,bucket=$BUCKET,readonly=false \
  --add-volume-mount=volume=uploads,mount-path=/app/wwwroot/uploads \
  --cpu=1 \
  --memory=512Mi \
  --min-instances=0 \
  --max-instances=1 \
  --timeout=3600
```

Detalles del Plan B:

- El montaje **reemplaza** la carpeta `/app/wwwroot/uploads` de la imagen. Como
  queda dentro del `wwwroot`, el middleware de archivos estáticos sigue sirviendo
  los adjuntos sin tocar una línea de código.
- La cuenta de servicio del runtime necesita permiso de escritura sobre el
  bucket (`roles/storage.objectAdmin`).
- **La base de datos NO va al bucket.** SQLite necesita *file locking*, que
  Cloud Storage FUSE no provee: la base se corrompe. `DATA_DIR` sigue siendo
  `/tmp/data` (efímera).

---

## 5. Variables de entorno

Todas se setean con `--set-env-vars` (separadas por `;`). Los nombres usan
doble guion bajo para los `:` de la configuración de .NET.

| Variable | Valor en Cloud Run | Por qué |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Cloud Run ya lo define por defecto; se explicita por claridad. Activa `UseExceptionHandler`, `UseHsts` y `UseHttpsRedirection` en `Program.cs`. |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | `true` | Ya viene en el `Dockerfile`. Hace que la app conozca el esquema real (`X-Forwarded-Proto`) y la IP real del cliente, y evita el loop de redirección si se define un puerto HTTPS. |
| `DATA_DIR` | `/tmp/data` | Carpeta escribible y efímera donde vive `tareas.db`. Si se omite o no admite escritura, la app cae sola a `/tmp/PW_Tareas` y lo avisa en el log, pero conviene setearla para que la ruta sea predecible. |
| `Storage__UploadsPhysicalPath` | `/tmp/uploads` | Carpeta física de los adjuntos. Al estar fuera del `wwwroot`, la app la publica en la URL `/uploads/...` por su cuenta. Vaciar la variable deja los adjuntos en `wwwroot/uploads` (comportamiento por defecto). |
| `Storage__UploadsPath` | `uploads` (default) | Carpeta **pública** de los adjuntos: es el prefijo de la URL (`/uploads/...`). |
| `ASPNETCORE_URLS` | `http://+:8080` | Ya viene fijada en el `Dockerfile`. Si se cambia `--port`, hay que cambiar esta variable en el mismo deploy. |
| `ConnectionStrings__Tareas` | *(vacío)* | Si se define, gana sobre `DATA_DIR` (ver `RutaBaseDatos.ResolverConexion`). Se deja vacío para usar el path por defecto. |

> **Las variables de entorno no persisten entre despliegues si cambiás el
> servicio con `gcloud run services update`.** Después de cada `gcloud run deploy` o
> `update`, verificá la configuración con:
> `gcloud run services describe tareas-blazor --region=us-central1 --format='value(spec.template.spec.containers[0].env)'`

---

## 6. Verificar el despliegue

```bash
# 1. URL del servicio
gcloud run services describe tareas-blazor --region=us-central1 --format='value(status.url)'

# 2. Que responde (esperar 200, no 30x en bucle)
curl -sI https://tareas-blazor-xxxxx-uc.a.run.app | head -1

# 3. Logs de la última ejecución
gcloud run logs read tareas-blazor --region=us-central1 --limit=50
```

En los logs del arranque tienen que aparecer estas dos líneas:

```
Base de datos SQLite en /tmp/data/tareas.db
```

y, **sólo si** el directorio configurado no admite escritura:

```
El directorio /app/data no admite escritura: la base se guardó en /tmp/PW_Tareas/tareas.db. En Cloud Run usá DATA_DIR=/tmp/data.
```

Checklist funcional en el navegador:

- [ ] La home carga y el menú de navegación funciona.
- [ ] Se puede **crear una tarea** y aparece en el listado.
- [ ] Se puede **subir una imagen** y se ve en la tarjeta de la tarea.
- [ ] Al recargar, la tarea sigue ahí (mientras la instancia no se haya parado).
- [ ] El panel de estadísticas muestra los valores correctos.

> **Importante sobre el escalado a cero:** con `--min-instances=0`, si la
> instancia está detenida, el primer request tarda unos segundos (arranque en
> frío) y **la base de datos arranca vacía**. Es el comportamiento esperado en
> este modo, no un error.

---

## 7. Persistencia: qué se pierde y cómo resolverlo

| | Plan A (mínimo) | Plan B (recomendado para demo) |
|---|---|---|
| Base de datos SQLite | Efímera (`/tmp/data`) | Efímera (`/tmp/data`) |
| Imágenes adjuntas | Efímeras (`/tmp/uploads`) | Persistentes (bucket de Cloud Storage) |
| Configuración extra | Sólo variables de entorno | `--add-volume` + `--add-volume-mount` |

Con `--max-instances=1` la experiencia es consistente (siempre la misma
instancia y la misma base), pero el contenido se reinicia cada vez que Cloud Run
apaga la instancia. Para una presentación o una demo online alcanza.

Si en el futuro hace falta persistencia real, las salidas son:

| Opción | Qué implica |
|---|---|
| **Cloud SQL (PostgreSQL)** | Es la opción correcta en producción. Requiere cambiar el proveedor de EF Core (`UseSqlite` → `UseNpgsql`) y regenerar las migraciones. |
| **GKE + PersistentVolumeClaim** | Mantiene SQLite tal cual, con un disco persistente (Filestore o PD). Requiere cluster + StorageClass. |
| **Compute Engine + disco persistente** | La más simple: una VM con `docker run` de la imagen y un volumen montado en `/app/data`. |
| **Cloud Run con `--min-instances=1`** | No persiste datos, pero evita el arranque en frío y hace que la base sobreviva más tiempo entre requests. |

---

## 8. Publicar una nueva versión y hacer rollback

Versioná siempre con etiquetas distintas (nunca reutilices `1.0.0`):

```bash
export IMAGE=us-central1-docker.pkg.dev/$PROJECT_ID/tareas-docker/tareas-blazor:1.0.1

docker build -t $IMAGE .
docker push $IMAGE

gcloud run deploy tareas-blazor --image=$IMAGE --region=us-central1
```

Para publicar **sin** cortar el tráfico en vivo (revisión nueva, 0% de tráfico):

```bash
gcloud run deploy tareas-blazor --image=$IMAGE --region=us-central1 --no-traffic --tag=1.0.1
gcloud run services update-traffic tareas-blazor --to-tags=1.0.1=100
```

Rollback a la versión anterior:

```bash
gcloud run services update-traffic tareas-blazor --to-revisions=1.0.0=100
```

Listado de revisiones:

```bash
gcloud run revisions list --service=tareas-blazor --region=us-central1
```

---

## 9. Costos y limpieza

Qué se factura:

| Recurso | Cobro |
|---|---|
| Cloud Run | Requests, CPU y memoria mientras hay instancias activas. Con `--min-instances=0` no se cobra cuando está en cero. |
| Artifact Registry | Almacenamiento de las imágenes (~$0.10/GiB/mes) y operaciones de red. |
| Cloud Storage (Plan B) | Almacenamiento y solicitudes sobre el bucket. |

Limpiar todo:

```bash
# Servicio
gcloud run services delete tareas-blazor --region=us-central1

# Repositorio de imágenes (borra también las imágenes que contiene)
gcloud artifacts repositories delete tareas-docker --location=us-central1 --quiet

# Bucket (Plan B)
gcloud storage rm -r gs://$BUCKET
```

---

## 10. Problemas frecuentes

| Síntoma | Causa | Solución |
|---|---|---|
| `ERR_TOO_MANY_REDIRECTS` / loop 307 | Se definió un puerto HTTPS (`ASPNETCORE_HTTPS_PORT`) y la app no conoce el `X-Forwarded-Proto` del proxy | Verificá que `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` (ya viene en el `Dockerfile`) |
| La app no arranca: `SqliteException: unable to open database file` | `DATA_DIR` apunta a un directorio que no admite escritura (por ejemplo `/app/data` en una imagen de sólo lectura) | Setear `DATA_DIR=/tmp/data`. La app tiene un respaldo: cae a `/tmp/PW_Tareas` y lo avisa en el log, pero conviene corregir la variable. |
| `UnauthorizedAccessException` al subir una imagen | La carpeta de adjuntos no es escribible | Setear `Storage__UploadsPhysicalPath=/tmp/uploads`, o montar el volumen del Plan B |
| La imagen se sube pero no se ve (404 en `/uploads/...`) | La ruta física quedó fuera del `wwwroot` y no se configuró `Storage__UploadsPhysicalPath` | Setear `Storage__UploadsPhysicalPath` con la ruta real de los adjuntos |
| `403` al abrir la URL | El servicio no es público | `gcloud run services add-iam-policy-binding tareas-blazor --region=us-central1 --member="allUsers" --role="roles/run.invoker"` |
| La lista de tareas aparece vacía | Se arrancó una instancia nueva (base efímera) o hay más de una instancia con bases distintas | Revisar `--max-instances=1` y la fecha de la última instancia en la consola de Cloud Run |
| Fallo del health check | El contenedor no escucha en el puerto que declaraste | El puerto de `--port` debe coincidir con `ASPNETCORE_URLS` |
| La interacción de Blazor se desconecta a los pocos minutos | Timeout de request | `--timeout=3600` |
| `Cloud Run service has been deployed with --allow-unauthenticated` en la consola | Es informational, no un error | Ignorar |

---

## 11. Checklist final

- [ ] Proyecto de GCP con billing habilitado.
- [ ] `gcloud` instalado, autenticado y con el proyecto correcto.
- [ ] APIs `run`, `artifactregistry`, `cloudbuild` y `logging` habilitadas.
- [ ] Repositorio `tareas-docker` creado en Artifact Registry.
- [ ] `gcloud auth configure-docker us-central1-docker.pkg.dev` ejecutado.
- [ ] `docker build` + `docker run` local verificado.
- [ ] `docker push` correcto.
- [ ] `gcloud run deploy` con `DATA_DIR=/tmp/data` y `Storage__UploadsPhysicalPath=/tmp/uploads`.
- [ ] `--max-instances=1` para no fragmentar la base SQLite.
- [ ] URL pública respondiendo 200 (sin loop de redirección).
- [ ] Log de arranque con la ruta real de la base de datos.
- [ ] Crear tarea y subir imagen verificados en el navegador.
- [ ] Etiqueta de versión registrada para poder hacer rollback.

---

## Ver también

- [DESPLIEGUE_HF.md](DESPLIEGUE_HF.md) — despliegue en Hugging Face Spaces.
- [NGINX_GUIDE.md](NGINX_GUIDE.md) — despliegue con Nginx sobre un servidor Linux.
- [README.md](../README.md) — variables del contenedor y estructura del proyecto.
