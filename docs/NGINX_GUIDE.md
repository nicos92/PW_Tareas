# Guía de despliegue con Nginx (Linux) — Blazor Server

## Requisitos

```bash
# Instalar Nginx
sudo dnf install -y nginx

# Habilitar Nginx en el firewall (puertos publicos)
sudo firewall-cmd --permanent --add-port=8081/tcp
sudo firewall-cmd --permanent --add-port=8082/tcp
sudo firewall-cmd --reload

# Instalar ASP.NET Core Runtime 8.0
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
sudo ./dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet
sudo ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet

# Iniciar Nginx y habilitarlo al arranque
sudo systemctl enable --now nginx
```

---

## 1. Publicar la aplicación

```bash
dotnet publish -c Release -o /var/www/tareasapp
```

Esto genera los archivos en `/var/www/tareasapp`.

---

## 2. Crear un servicio systemd por cada aplicación

Cada aplicación Blazor se ejecuta como un proceso Kestrel independiente con su propio puerto.

Crear un usuario dedicado (no usar `root`):

```bash
sudo useradd -r -s /bin/false -m -d /var/www/tareasapp tareasapp
```

```bash
sudo nano /etc/systemd/system/tareasapp.service
```

```ini
[Unit]
Description=Aplicacion Tareas Blazor
After=network.target

[Service]
WorkingDirectory=/var/www/tareasapp
ExecStart=/usr/bin/dotnet /var/www/tareasapp/TareasBlazor.dll --urls "http://localhost:5001"
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=tareasapp
User=tareasapp
Group=tareasapp
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

> `--urls "http://localhost:5001"` define el puerto interno donde Kestrel escucha. Nginx se conectará a este puerto.

Habilitar e iniciar:

```bash
sudo systemctl enable tareasapp
sudo systemctl start tareasapp
sudo systemctl status tareasapp
```

### Segunda aplicación (ejemplo)

Publicas otra app en `/var/www/otraapp` y creas otro servicio:

```ini
ExecStart=/usr/bin/dotnet /var/www/otraapp/OtraApp.dll --urls "http://localhost:5002"
```

```bash
sudo systemctl enable otraapp
sudo systemctl start otraapp
```

---

## 3. Configurar Nginx como proxy inverso

En IIS cambiar el puerto equivale en Nginx a cambiar el `listen` del `server block`.

### Ejemplo: dos aplicaciones en diferentes puertos Nginx

```
http://MI_IP:8081  →  Kestrel :5001  (TareasApp)
http://MI_IP:8082  →  Kestrel :5002  (OtraApp)
```

> **Nota para Fedora/Alma:** El directorio `sites-available` no existe por defecto. Tienes dos opciones:
> - Crearlo manualmente: `sudo mkdir -p /etc/nginx/sites-available /etc/nginx/sites-enabled` y agregar `include /etc/nginx/sites-enabled/*;` dentro del bloque `http` en `/etc/nginx/nginx.conf`
> - O usar directamente `/etc/nginx/conf.d/tareasapp.conf` (recomendado, ya está incluido automáticamente)

Crear archivo de configuración:

```bash
sudo nano /etc/nginx/conf.d/tareasapp.conf
```

```nginx
server {
    listen 8081;
    server_name _;

    location / {
        proxy_pass http://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

Activar (con `conf.d` los archivos se cargan automáticamente):

```bash
sudo nginx -t
sudo systemctl reload nginx
```

Para la segunda app, creas otro archivo con distinto `listen`:

```nginx
server {
    listen 8082;
    server_name _;

    location / {
        proxy_pass http://localhost:5002;
        # ... mismos headers que arriba
    }
}
```

### Múltiples apps en un solo archivo

También puedes poner todo en un mismo archivo:

```nginx
server {
    listen 8081;
    server_name _;
    location / {
        proxy_pass http://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}

server {
    listen 8082;
    server_name _;
    location / {
        proxy_pass http://localhost:5002;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

---

## 4. WebSockets / SignalR (importante para Blazor Server)

Blazor Server usa SignalR (WebSockets) para la comunicación en tiempo real. Sin los headers `Upgrade` y `Connection` la app falla al intentar reconectar o interactuar.

La configuración de arriba ya incluye las líneas necesarias:

```nginx
proxy_http_version 1.1;
proxy_set_header Upgrade $http_upgrade;
proxy_set_header Connection "upgrade";
```

Si ves errores de reconexión en el browser, verifica que estos headers estén presentes.

---

## 5. SELinux (Fedora / Alma Linux)

SELinux está en modo `Enforcing` por defecto en Fedora y Alma. Puede bloquear Nginx y Kestrel.

### 5.1 Verificar el estado

```bash
getenforce
# Enforcing
```

### 5.2 Permitir a Nginx hacer conexiones de red (proxy_pass)

```bash
# Permitir que Nginx actúe como proxy inverso
sudo setsebool -P httpd_can_network_connect 1
```

Sin esto, Nginx devolverá **502 Bad Gateway** aunque Kestrel esté corriendo.

### 5.3 Contexto de seguridad para los archivos de la aplicación

```bash
# Aplicar contexto httpd_sys_content_t a los archivos estaticos
sudo semanage fcontext -a -t httpd_sys_content_t "/var/www/tareasapp(/.*)?"
sudo restorecon -Rv /var/www/tareasapp

# Si la app escribe archivos (uploads, BD SQLite), necesitas contexto httpd_sys_rw_content_t
sudo semanage fcontext -a -t httpd_sys_rw_content_t "/var/www/tareasapp/uploads(/.*)?"
sudo semanage fcontext -a -t httpd_sys_rw_content_t "/var/www/tareasapp/tareas\\.db"
sudo restorecon -Rv /var/www/tareasapp
```

> **Nota:** Esto aplica cuando Nginx sirve archivos estáticos directamente. Si solo usas `proxy_pass`, el contexto del binario de Kestrel (dotnet) no necesita cambio, pero los archivos SQLite o `uploads/` que la app escribe en tiempo de ejecución sí pueden requerir contexto adecuado para el usuario `tareasapp`.

### 5.4 Permitir que dotnet escuche en puertos específicos

Si SELinux bloquea a dotnet para escuchar en un puerto:

```bash
# Verificar si hay denegaciones
sudo ausearch -m avc -ts recent
# O en tiempo real
sudo tail -f /var/log/audit/audit.log | grep denied

# Si dotnet necesita escuchar en un puerto no estandar
sudo semanage port -a -t http_port_t -p tcp 5001
```

### 5.5 Deshabilitar SELinux temporalmente (solo para pruebas)

```bash
sudo setenforce 0   # Permissive mode
# ... probar la app ...
sudo setenforce 1   # Volver a Enforcing
```

### 5.6 Solución completa: generar politicas desde las denegaciones

```bash
# Instalar herramientas de auditoria
sudo dnf install -y setroubleshoot setools-console

# Ver sugerencias de solución
sudo sealert -a /var/log/audit/audit.log

# O crear un modulo desde las denegaciones
sudo ausearch -m avc -ts recent | sudo audit2allow -M tareasapp
sudo semodule -i tareasapp.pp
```

---

## 6. Puerto por defecto (80) y Forwarded Headers

Si una app escucha en el puerto 80 (por defecto), basta con el bloque más simple:

```nginx
server {
    listen 80;
    server_name _;

    location / {
        proxy_pass http://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

---

## 7. Esquema visual

```
                         ┌─────────────────┐
                         │    Nginx         │
                         │  (proxy inverso) │
                         │                  │
  Browser ──► http://IP:8081  ──► Kestrel :5001  (TareasApp)
  Browser ──► http://IP:8082  ──► Kestrel :5002  (OtraApp)
  Browser ──► http://IP:8083  ──► Kestrel :5003  (OtraApp2)
                         └─────────────────┘
```

Cada app Blazor corre como un servicio systemd independiente en su propio puerto Kestrel, y Nginx expone cada una en un puerto público diferente.

---

## 8. Comandos útiles

```bash
# Ver logs de Nginx
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log

# Ver logs de la aplicación (journald)
sudo journalctl -u tareasapp -f

# Ver logs de SELinux en tiempo real
sudo tail -f /var/log/audit/audit.log | grep denied

# Ver denegaciones de SELinux
sudo ausearch -m avc -ts recent

# Ver estado del servicio
sudo systemctl status tareasapp

# Recargar Nginx sin cortar conexiones
sudo systemctl reload nginx

# Reiniciar Nginx
sudo systemctl restart nginx

# Verificar sintaxis de configuracion
sudo nginx -t

# Probar que el puerto esta escuchando
sudo ss -tlnp | grep 8081

# Listar servicios systemd de la app
sudo systemctl list-units | grep tareas

# Ver puertos Kestrel activos
sudo ss -tlnp | grep dotnet

# Ver contexto SELinux de los archivos
ls -laZ /var/www/tareasapp/
```

---

## 9. Troubleshooting común

### Error 502 Bad Gateway
- Kestrel no está corriendo: `sudo systemctl restart tareasapp`
- Puerto incorrecto en `proxy_pass`: verificar que coincida con `--urls` del servicio

### La página carga pero las interacciones no responden
- Faltan headers de WebSocket: verificar `proxy_set_header Upgrade` y `Connection`
- Ver consola del browser: errores de SignalR/negotiate

### Puerto ocupado
```bash
sudo ss -tlnp | grep 5001
```

### Firewall bloqueando puertos (firewalld)
```bash
sudo firewall-cmd --permanent --add-port=8081/tcp
sudo firewall-cmd --permanent --add-port=8082/tcp
sudo firewall-cmd --reload
```

### SELinux bloqueando (502 Bad Gateway)
```bash
# Ver denegaciones de SELinux
sudo ausearch -m avc -ts recent | grep nginx

# Solucion rapida
sudo setsebool -P httpd_can_network_connect 1

# O modo permisivo para identificar el problema
sudo setenforce 0
# ... probar ...
sudo setenforce 1
```

### dotnet no puede leer/escribir archivos
```bash
# Corregir contexto SELinux
sudo restorecon -Rv /var/www/tareasapp

# Si persiste, revisar permisos del usuario
sudo ls -laZ /var/www/tareasapp
```
