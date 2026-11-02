# Contribución

Este proyecto usa [Conventional Commits](https://www.conventionalcommits.org/)
para que el historial sea legible y para generar el changelog con `git-cliff`.

## Formato

```text
<tipo>(<scope opcional>)!: <descripción en imperativo>

[cuerpo opcional]

[footer opcional]
```

Ejemplos válidos:

```text
feat: agrega búsqueda textual de tareas
fix: impide eliminar categorías con tareas asociadas
docs: actualiza la guía de despliegue en Cloud Run
test: cubre la restauración de tareas dadas de baja
refactor!: migra el proyecto a Clean Architecture
chore: actualiza dependencias de desarrollo
```

Tipos permitidos:

| Tipo | Uso |
|---|---|
| `feat` | Nueva funcionalidad |
| `fix` | Corrección de un error |
| `docs` | Cambios sólo en documentación |
| `test` | Pruebas nuevas o modificadas |
| `refactor` | Cambio interno sin alterar comportamiento |
| `perf` | Mejora de rendimiento |
| `build` | Cambios de build o dependencias |
| `ci` | Cambios en GitHub Actions u otra automatización |
| `style` | Formato, espacios, punto y coma; sin cambios de lógica |
| `chore` | Tareas de mantenimiento |
| `revert` | Reversión de un commit anterior |

Reglas:

- La descripción va en minúscula y en imperativo: `agrega`, no `agregué`.
- No terminar la descripción con punto.
- Un cambio incompatible se marca con `!` después del tipo/scope y se
  documenta en el footer con `BREAKING CHANGE:`.
- El título del PR debe seguir el mismo formato, porque `main` acepta sólo
  *Squash and merge* y usa el título del PR como mensaje final.

## Validación local

El repositorio incluye `commitlint` y `husky`. Después de clonar:

```bash
npm install
```

El hook `.husky/commit-msg` valida cada mensaje antes de crear el commit.
También se puede validar manualmente:

```bash
echo "feat: agrega un endpoint de salud" | npx commitlint
```

## Historial anterior

Los commits anteriores a la incorporación de esta política son legado y no se
reescriben: hacerlo cambiaría todos los SHA, invalidaría tags y rompería enlaces
a commits y PRs. La validación se aplica a los commits nuevos y a los rangos
que se integran mediante pull requests.
