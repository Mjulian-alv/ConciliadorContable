# ArcaAutomatizacion — Guía de Integración

ArcaAutomatizacion es una API REST que automatiza la obtención de comprobantes recibidos desde el portal de ARCA (ex-AFIP). Permite a sistemas externos obtener esos comprobantes en formato estructurado enviando simplemente las credenciales fiscales y un rango de fechas.

**URL base:** `https://tu-servidor/`

---

## Autenticación

Todos los endpoints bajo `/api/` (excepto `/api/auth`) requieren un **token de API** en el header de cada request. El token se obtiene mediante registro o login.

El token se puede enviar de dos formas:

```
X-Api-Token: <token>
```
o
```
Authorization: Bearer <token>
```

### Registrar un usuario nuevo

**`POST /api/auth/register`**

Crea un usuario y devuelve su token de API.

**Body (JSON):**

```json
{
  "email": "empresa@ejemplo.com",
  "password": "miPassword123",
  "companyName": "Mi Empresa S.A.",
  "requestsLimit": 100
}
```

| Campo | Tipo | Requerido | Descripción |
|---|---|---|---|
| `email` | string | ? | Email único del usuario |
| `password` | string | ? | Contraseña |
| `companyName` | string | ? | Nombre de la empresa |
| `requestsLimit` | int | ? | Límite de requests permitidos (`null` = ilimitado) |

**Respuesta exitosa (`200`):**

```json
{
  "ok": true,
  "token": "BASE64_TOKEN_AQUI",
  "userId": 1
}
```

**Respuesta de error (`400`):**

```json
{
  "ok": false,
  "message": "El email ya está registrado"
}
```

---

### Iniciar sesión (obtener token)

**`POST /api/auth/login`**

Autentica al usuario y devuelve un nuevo token de API.

**Body (JSON):**

```json
{
  "email": "empresa@ejemplo.com",
  "password": "miPassword123",
  "requestsLimit": 50
}
```

| Campo | Tipo | Requerido | Descripción |
|---|---|---|---|
| `email` | string | ? | Email del usuario |
| `password` | string | ? | Contraseña |
| `requestsLimit` | int | ? | Límite de requests para este token (`null` = ilimitado) |

**Respuesta exitosa (`200`):**

```json
{
  "ok": true,
  "token": "BASE64_TOKEN_AQUI",
  "userId": 1
}
```

**Respuesta de error (`401`):**

```json
{
  "ok": false,
  "message": "Credenciales inválidas"
}
```

---

## Endpoints disponibles

### Exportar comprobantes recibidos

**`POST /api/MisComprobantes/exportar`**

Automatiza el ingreso a ARCA con las credenciales del contribuyente, navega a "Mis Comprobantes", filtra por rango de fechas y devuelve los comprobantes recibidos parseados desde el CSV exportado.

> ?? Este endpoint puede demorar entre **30 y 90 segundos** según la respuesta del portal.

**Headers requeridos:**

```
X-Api-Token: <token>
Content-Type: application/json
```

**Body (JSON):**

```json
{
  "username": "20123456789",
  "password": "clave_afip",
  "cuit": "20-12345678-9",
  "fechaInicio": "01/01/2024",
  "fechaFin": "31/01/2024"
}
```

| Campo | Tipo | Requerido | Descripción |
|---|---|---|---|
| `username` | string | ? | CUIL/CUIT del contribuyente (sin guiones) |
| `password` | string | ? | Clave fiscal de ARCA |
| `cuit` | string | ? | CUIT con guiones para seleccionar la representada (ej: `20-12345678-9`) |
| `fechaInicio` | string | ? | Fecha de inicio en formato `DD/MM/YYYY` |
| `fechaFin` | string | ? | Fecha de fin en formato `DD/MM/YYYY` |

**Respuesta exitosa (`200`):**

```json
{
  "ok": true,
  "message": "Comprobantes exportados y parseados correctamente",
  "downloadedFiles": ["comprobantes_2024.csv"],
  "totalRegistros": 42,
  "comprobantes": [
    {
      "fechaEmision": "15/01/2024",
      "tipoComprobante": "FACTURA B",
      "puntoVenta": "00001",
      "numeroDesde": "00000123",
      "numeroHasta": "00000123",
      "codAutorizacion": "12345678901234",
      "tipoDocEmisor": "CUIT",
      "nroDocEmisor": "30123456789",
      "denominacionEmisor": "PROVEEDOR S.R.L.",
      "tipoDocReceptor": "CUIT",
      "nroDocReceptor": "20123456789",
      "tipoCambio": "1",
      "moneda": "PES",
      "impNetoGravadoIVA0": "0",
      "iva25": "0",
      "impNetoGravadoIVA25": "0",
      "iva5": "0",
      "impNetoGravadoIVA5": "0",
      "iva105": "0",
      "impNetoGravadoIVA105": "1000",
      "iva21": "0",
      "impNetoGravadoIVA21": "0",
      "iva27": "0",
      "impNetoGravadoIVA27": "0",
      "impNetoGravadoTotal": "1000",
      "impNetoNoGravado": "0",
      "impOpExentas": "0",
      "otrosTributos": "0",
      "totalIVA": "105",
      "impTotal": "1105"
    }
  ]
}
```

#### Campos del objeto `ComprobanteCsv`

| Campo | Descripción |
|---|---|
| `fechaEmision` | Fecha de emisión del comprobante |
| `tipoComprobante` | Tipo (FACTURA A, FACTURA B, NOTA DE CRÉDITO, etc.) |
| `puntoVenta` | Punto de venta |
| `numeroDesde` | Número desde |
| `numeroHasta` | Número hasta |
| `codAutorizacion` | CAE / CAI / CAEA |
| `tipoDocEmisor` | Tipo de documento del emisor |
| `nroDocEmisor` | Número de documento del emisor |
| `denominacionEmisor` | Razón social del emisor |
| `tipoDocReceptor` | Tipo de documento del receptor |
| `nroDocReceptor` | Número de documento del receptor |
| `tipoCambio` | Tipo de cambio aplicado |
| `moneda` | Código de moneda (PES, DOL, etc.) |
| `impNetoGravadoIVA0` | Importe neto gravado al 0% |
| `iva25` | IVA al 2.5% |
| `impNetoGravadoIVA25` | Neto gravado al 2.5% |
| `iva5` | IVA al 5% |
| `impNetoGravadoIVA5` | Neto gravado al 5% |
| `iva105` | IVA al 10.5% |
| `impNetoGravadoIVA105` | Neto gravado al 10.5% |
| `iva21` | IVA al 21% |
| `impNetoGravadoIVA21` | Neto gravado al 21% |
| `iva27` | IVA al 27% |
| `impNetoGravadoIVA27` | Neto gravado al 27% |
| `impNetoGravadoTotal` | Total neto gravado |
| `impNetoNoGravado` | Importe neto no gravado |
| `impOpExentas` | Operaciones exentas |
| `otrosTributos` | Otros tributos |
| `totalIVA` | Total de IVA |
| `impTotal` | Importe total del comprobante |

---

## Manejo de errores

Todos los errores devuelven `ok: false` con un mensaje descriptivo. En casos de error durante la automatización, la respuesta puede incluir un arreglo `screenshots` con nombres de archivos de captura de pantalla generados en el servidor para diagnóstico.

| HTTP Status | Descripción |
|---|---|
| `400` | Parámetros faltantes o error durante la automatización |
| `401` | Token ausente, inválido o expirado |
| `404` | No se encontró un elemento esperado en el portal |
| `429` | Límite de requests del token alcanzado |

**Ejemplo de error `401`:**

```json
{
  "ok": false,
  "message": "Token expirado"
}
```

**Ejemplo de error `404` con screenshot:**

```json
{
  "ok": false,
  "message": "No se pudo acceder a Mis Comprobantes o CUIT no encontrado",
  "screenshots": ["error_navigation_20240115_143022.png"]
}
```

---

## Consideraciones importantes

- **Tiempo de respuesta:** Configurar un timeout de al menos **120 segundos** en el cliente HTTP.
- **Credenciales:** Las credenciales de ARCA (`username`/`password`) se usan solo en tiempo de ejecución y no son almacenadas.
- **Límite de requests:** Una vez alcanzado `requestsLimit`, el token devuelve `429` hasta obtener uno nuevo vía `/api/auth/login`.
- **Formato de fechas:** Usar siempre `DD/MM/YYYY` para los campos de fecha.
