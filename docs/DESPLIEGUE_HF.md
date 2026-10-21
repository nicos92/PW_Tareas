# Despliegue en Hugging Face Spaces (SDK Docker)

Esta aplicación Blazor Server se dockeriza con el `Dockerfile` de la raíz del
proyecto y sirve en el **puerto 8080**. En Hugging Face Spaces se sirve en el
mismo puerto gracias a `app_port: 8080` en el front matter del `README.md`.

## 1. Crear la cuenta y el token

1. Regístrate en <https://huggingface.co> (es gratis).
2. Ve a <https://huggingface.co/settings/tokens>
3. Crea un token con permisos **write** (los tokens `read` no permiten hacer push).

> **El token es una credencial:** guardalo fuera del repositorio (gestor de
> contraseñas o variable de entorno). Nunca lo escribas en un archivo versionado,
> porque queda accesible para cualquiera que lea el repositorio y Hugging Face
> puede revocarlo sin avisar. Si se filtró, revocalo y creá uno nuevo.

## 2. Crear el Space

1. Ve a <https://huggingface.co/new-space>
2. Completa:
   - **Owner**: tu usuario.
   - **Space name**: p. ej. `pw-tareas`.
   - **License**: la que prefieras (p. ej. MIT).
   - **SDK**: **Docker** (el SDK "blank" / Docker).
3. Crea el Space. **No** subas el README de ejemplo: este repositorio ya trae
   el `README.md` correcto con el front matter (`sdk: docker`, `app_port: 8080`).

## 3. Hacer push del proyecto al Space

En la carpeta del proyecto (una sola vez):

```bash
git remote add space https://huggingface.co/spaces/TU_USUARIO/pw-tareas
git push space main
```

Si tu rama local se llama `master`, usa `git push space master:main`.

Alternativa con `huggingface-cli`:

```bash
pip install -U "huggingface_hub[cli]"
huggingface-cli login   # pega el token
huggingface-cli upload TU_USUARIO/pw-tareas . --repo-type=space
```

## 4. Build automático

Una vez hecho el push, Hugging Face construye la imagen con el `Dockerfile`
del repositorio. El estado del build se ve en la pestaña **Logs** del Space.
Cuando termine, la app queda disponible en:

```
https://huggingface.co/spaces/TU_USUARIO/pw-tareas
```

En cada push posterior, el Space se reconstruye automáticamente.

## Notas importantes

- **Puerto**: el front matter del `README.md` define `app_port: 8080`, por lo
  que el contenedor NO necesita cambiar de puerto; la app ya escucha en `8080`
  (`ASPNETCORE_URLS=http://+:8080` en el Dockerfile).
- **Datos efímeros**: la base de datos SQLite vive en `/app/data` dentro del
  contenedor. Al reconstruir el Space (cada push) la BD y las imágenes
  subidas se pierden. Suficiente para una demo/presentación.
- **HTTPS**: el proxy de Hugging Face termina TLS; dentro del contenedor sólo
  hay HTTP. El `Dockerfile` define `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`
  para que la app procese `X-Forwarded-Proto` y sepa que el esquema real es
  HTTPS. `UseHttpsRedirection` sólo redirige si se define un puerto HTTPS
  (`ASPNETCORE_HTTPS_PORT`), que en este despliegue no ocurre.
- **Migraciones**: EF Core aplica las migraciones automáticamente al arrancar
  (`db.Database.Migrate()` en `Program.cs`), no hay pasos manuales.
- **Usuario no root**: el contenedor corre como el usuario `user` (uid 1000),
  como recomienda Hugging Face.

## Presentación local (sin Hugging Face)

Para exponer la app en el puerto 8080 en la máquina de la presentación:

```bash
docker build -t tareas-blazor .
docker run --rm -p 8080:8080 tareas-blazor
```

Abrir `http://localhost:8080` en el navegador. Para que otros equipos de la
misma red local puedan acceder, abrir el puerto en el firewall y usar la IP
de la máquina (p. ej. `http://192.168.x.x:8080`).
