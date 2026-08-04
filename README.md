# Finance Tracker

Aplicación full-stack de finanzas personales pensada para el contexto uruguayo, donde es habitual manejar pesos y dólares al mismo tiempo.

Permite registrar ingresos y gastos en varias cuentas y monedas, y ver todo consolidado en un único panel — con los montos convertidos usando la cotización vigente **en la fecha de cada movimiento*.

> **Estado:** en desarrollo activo. El núcleo está completo y funcionando (autenticación, cuentas, categorías, movimientos, reportes y dashboard), con cobertura de tests en backend y frontend. Ver [Roadmap](#roadmap) para lo que sigue.

---

## Capturas

![Dashboard](docs/dashboard.png)

| Login | Movimientos |
|---|---|
| ![Login](docs/login.png) | ![Movimientos](docs/movimientos.png) |

<!-- TODO: agregar el link a la demo cuando esté desplegada -->
<!-- **Demo en vivo:** https://... -->

---

## Funcionalidades

**Registro de movimientos**
- Múltiples cuentas, cada una con su propia moneda
- Ingresos y gastos con **actualización atómica del saldo**: el movimiento y el balance de la cuenta se escriben dentro de una misma transacción de base de datos, con rollback si algo falla
- Categorías propias, tipadas como ingreso o gasto

**Multi-moneda**
- Cotizaciones obtenidas de dos APIs externas independientes
- Los reportes convierten cada monto a una sola moneda usando **la cotización más cercana anterior o igual a la fecha del movimiento**, de modo que un reporte pasado no cambia porque hoy se movió el dólar

**Panel de reportes**
- Resumen mensual: ingresos, gastos y ahorro neto
- Gastos por categoría (gráfico de dona, con porcentajes)
- Ingresos vs. gastos a lo largo de 12 meses (gráfico de barras)
- Cotizaciones del día

**Interfaz**
- Responsive, mobile-first: barra inferior en móvil, sidebar en escritorio
- Tema oscuro por defecto, con cambio a tema claro que se recuerda entre sesiones
- Formato numérico uruguayo (`48.240,50`) y cifras tabulares para que los montos queden alineados

---

## Stack

**Backend**
- .NET 10 (Web API)
- SQL Server 2025
- Entity Framework Core — enfoque database-first
- Autenticación JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- BCrypt para el hash de contraseñas

**Frontend**
- Angular 22 — componentes standalone, signals, nuevo control flow (`@if` / `@for`)
- SCSS con un design system basado en CSS custom properties
- Chart.js 4 (usado directamente, sin wrapper de Angular)
- Lucide para los íconos

**Tests**
- Backend: xUnit + Moq + EF Core InMemory — unitarios de servicios e integración con `WebApplicationFactory`
- Frontend: Vitest — componentes clave, interceptor y guard de autenticación

**APIs externas**
- [Frankfurter](https://frankfurter.dev) — cotizaciones de referencia del BCE, base USD
- [UruguayAPI](https://github.com/AFornio/UruguayAPI) — cotizaciones del BROU, base UYU

---

## Arquitectura

En capas, con interfaces en cada límite:

```
Controller  →  Service  →  Repository  →  DbContext  →  SQL Server
   HTTP        reglas       acceso
              de negocio    a datos
```

```
FinanceTracker/
├── FinanceTracker.API/          # Web API en .NET 10
│   ├── Controllers/             # endpoints HTTP, livianos
│   ├── Services/                # reglas de negocio y validación
│   ├── Repositories/            # acceso a datos
│   ├── DTOs/                    # contratos separados de entrada y salida
│   ├── Models/                  # entidades de EF Core (scaffolded)
│   ├── Exceptions/              # excepciones tipadas (Conflict, Unauthorized)
│   ├── Middlewares/             # manejo global de excepciones
│   └── Data/                    # DbContext
│
├── FinanceTracker.API.Tests/    # xUnit + Moq + EF InMemory
│   ├── Services/                # tests unitarios por servicio
│   └── Integration/             # tests de extremo a extremo por controller
│
├── FinanceTracker.Web/          # SPA en Angular 22
│   └── src/app/
│       ├── core/                # servicios, guards, interceptors, modelos
│       ├── features/            # auth, dashboard, cuentas, categorías, movimientos
│       ├── layout/               # sidebar, barra inferior, layout principal
│       └── shared/               # componentes reutilizables (modal, theme toggle)
│
├── docs/screenshots/            # capturas usadas en este README
└── database/                    # scripts de creación del esquema
```

**Notas de diseño**

- **Los DTOs divergen a propósito.** Los de entrada llevan IDs y se mantienen mínimos; los de salida llevan nombres para mostrar y campos calculados. Una petición de creación recibe `categoryId`; la respuesta devuelve `categoryId` y `categoryName`.
- **Borrado lógico en todo el sistema.** Los registros se marcan como inactivos en lugar de eliminarse, para que los reportes históricos no se rompan.
- **Defensa en profundidad.** El tipo de un movimiento está restringido por un `CHECK` en SQL Server, validado en la capa de servicio, y tipado como union (`'INCOME' | 'EXPENSE'`) en TypeScript.
- **Excepciones tipadas, no genéricas.** `ConflictException` y `UnauthorizedException` le permiten al middleware global traducir la intención del dominio al status HTTP correcto (409, 401) en lugar de que todo caiga en un 500 indistinguible.

---

## Decisiones técnicas

Estas fueron decisiones deliberadas, no valores por defecto. Cada una tiene una alternativa que elegiría bajo otras restricciones.

**Cotización histórica para las conversiones.**
Los reportes convierten cada movimiento con la cotización más cercana anterior o igual a su fecha, no con la actual. Un gasto de junio no debería cambiar de valor porque el dólar se movió en julio. La versión más estricta —congelar la tasa en la propia fila del movimiento al momento de guardarlo— está en el roadmap; el enfoque actual se eligió para mantener simple el módulo de reportes mientras el esquema todavía se estabiliza.

**Chart.js usado directamente, sin `ng2-charts`.**
Las librerías wrapper declaran rangos de peer dependencies que van por detrás de los lanzamientos de Angular, y Angular 22 es lo bastante nuevo como para que eso moleste. Usar Chart.js directo elimina ese acoplamiento y deja un conocimiento transferible entre frameworks. El costo son unas quince líneas de integración por gráfico: crear el canvas, instanciarlo, y destruirlo antes de redibujar para no perder memoria.

**Los colores de los gráficos se leen de las CSS custom properties en tiempo de render.**
Canvas no reacciona al CSS como sí lo hace el DOM, así que los gráficos leen su paleta con `getComputedStyle` y se redibujan cuando cambia el signal del tema. Un único origen de verdad para el color en toda la aplicación, gráficos incluidos.

**El JWT se guarda en `localStorage`.**
Es la decisión pragmática, no la más segura posible. Una cookie httpOnly es inmune al robo de token por XSS, pero introduce exposición a CSRF, requiere unos cinco cambios en el backend y complica el despliegue cross-domain. Con un token que vive 60 minutos y el escapado de HTML que Angular hace por defecto, el riesgo residual es aceptable para este alcance. La migración está en el roadmap.

**Reintentos con espera en las llamadas a APIs externas.**
Los dos proveedores de cotizaciones son servicios gratuitos que se duermen cuando están inactivos. Las llamadas reintentan tres veces con dos segundos de espera, distinguiendo fallos de conexión de timeouts, para que un arranque en frío no le llegue al usuario como un error.

**El color semántico está reservado.**
El verde y el rojo se usan únicamente sobre montos, nunca en botones ni tarjetas, de modo que siempre significan ingreso y gasto y nada más. Los gráficos de categorías usan una paleta categórica aparte. Además, cada monto lleva un `+` o `−` explícito, para que el significado sobreviva al daltonismo rojo-verde: el color refuerza, el signo informa.

**Excepciones tipadas para mapear el status HTTP.**
El middleware global de excepciones capturaba todo como un 500, incluyendo casos que en realidad son errores del cliente (email duplicado, login inválido). Se introdujeron `ConflictException` y `UnauthorizedException` para que el middleware elija el status HTTP según el tipo de excepción, no por defecto. El costo es acordarse de usar el tipo correcto en cada Service; el beneficio es que el frontend puede reaccionar distinto según el código (por ejemplo, mostrar "ya existe esa cuenta" en vez de un error genérico).

---

## Cómo correrlo localmente

**Requisitos:** .NET 10 SDK, SQL Server 2025, Node.js 20+, Angular CLI 22

**1. Base de datos**

Ejecutar `database/Scripts_Tablas.sql` sobre una base nueva para crear el esquema, y después sembrar las tablas de referencia `Roles` y `Currencies`.

**2. Backend**

Crear `FinanceTracker.API/appsettings.Development.json` (está en el `.gitignore` porque contiene secretos):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=FinanceTracker;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "una-clave-larga-y-aleatoria-de-al-menos-32-caracteres",
    "Issuer": "FinanceTrackerAPI",
    "Audience": "FinanceTrackerClient",
    "ExpirationMinutes": 60
  }
}
```

Después:

```bash
cd FinanceTracker.API
dotnet run
```

La API queda en `https://localhost:7099`, con Swagger en `/swagger`.

**3. Frontend**

```bash
cd FinanceTracker.Web
npm install
ng serve
```

La aplicación queda en `http://localhost:4200`. El CORS de la API está restringido a ese origen.

**4. Tests**

```bash
cd FinanceTracker.API.Tests
dotnet test
```

```bash
cd FinanceTracker.Web
ng test
```

---

## Endpoints

Todos los endpoints excepto `/api/Auth/*` requieren un token `Bearer`. El id del usuario se lee siempre del claim del JWT, nunca del cuerpo de la petición.

| Recurso | Endpoints |
|---|---|
| `Auth` | `POST /register`, `POST /login` |
| `Account` | `GET`, `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}` |
| `Category` | `GET`, `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}` |
| `Transaction` | `GET`, `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}` |
| `Currency` | `GET`, `GET /{id}` (datos de referencia, solo lectura) |
| `ExchangeRate` | `GET`, `POST /refresh`, `POST /refresh-uruguay` |
| `Report` | `GET /monthly-summary`, `GET /expenses-by-category`, `GET /monthly-evolution` |

---

## Seguridad

- Los secretos viven en `appsettings.Development.json`, que está en el `.gitignore`. El `appsettings.json` versionado no contiene cadena de conexión ni clave de firma.
- Contraseñas hasheadas con BCrypt.
- Los fallos de login devuelven un mensaje deliberadamente ambiguo (idéntico si el email no existe o si la contraseña es incorrecta), para que el endpoint no sirva para enumerar qué direcciones de email están registradas.
- Excepciones tipadas (`ConflictException`, `UnauthorizedException`) mapeadas a sus status HTTP correctos (409, 401) en un único middleware — ningún stack trace llega al cliente.
- El CORS está restringido a un origen específico; no se usa `AllowAnyOrigin`.
- Los guards de rutas del frontend son solo una comodidad de UX. La autorización real se aplica del lado del servidor con `[Authorize]` y acotando cada consulta al id de usuario que viene en el token.

---

## Roadmap

**Deudas conocidas**
- No hay refresh tokens: las sesiones expiran a los 60 minutos.
- Todavía no está desplegado; corre localmente.
- `AuthService` y `ExchangeRateService` acceden al `DbContext` directamente, sin pasar por un Repository, a diferencia del resto de los módulos.
- Algunos specs de Angular generados por el CLI (layout, modal, gráficos) tienen fallas preexistentes no relacionadas con la lógica de negocio.

**Planeado**
- Movimientos recurrentes (sueldo, alquiler)
- Metas de ahorro con cálculo automático del aporte necesario por período
- Presupuestos con alertas de ritmo de gasto
- Gastos compartidos (dividir la cuenta)
- Congelar la cotización en cada movimiento al momento de guardarlo
- Migrar el almacenamiento del JWT a una cookie httpOnly con protección CSRF
- Verificación de email en el registro
- Tabla de auditoría con el historial completo de cambios

**Fuera de alcance, a propósito**
- Sincronización bancaria automática: requiere servicios comerciales de agregación y una carga regulatoria que no corresponde a este proyecto. La carga manual es una decisión de diseño, y una que comparten varias apps de finanzas exitosas orientadas a la privacidad.
- Seguimiento de carteras de inversión: es otro dominio de problema.

---

## Autor

Desarrollado por **Franco** — desarrollador full-stack en Uruguay.

Trabajo con .NET, Angular, Vue y SQL Server.
