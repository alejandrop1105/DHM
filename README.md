# 🩺 Data Health Monitor (DHM)

**Data Health Monitor (DHM)** es una solución construida en **.NET 10** bajo el paradigma de **Clean Architecture**, diseñada para supervisar y asegurar la integridad de múltiples bases de datos de diferentes clientes / proyectos (Tenants) en tiempo real. 

Proporciona tanto una visualización rápida mediante cuadros de mando (Dashboards) interactivos como la capacidad de configurar sistemas de "autocuración" (Self-Healing) que reaccionan de inmediato cuando una prueba obligatoria falla.

---

## 🚀 Funcionalidades del Sistema

- **Arquitectura Multitenant:** Configuración y persistencia segmentada. Puedes añadir múltiples clientes y gestionar a qué bases de datos remotas se conectará el sistema para hacer validaciones cruzadas. Soporta **SQLite, SQL Server, MySQL y PostgreSQL**.
- **Dashboard en Tiempo Real:** Interfaz viva construida en **Blazor Server** con componentes limpios y modernos (**MudBlazor**). Implementa alertas mediante semáforos, notificaciones y conexión en tiempo real vía **SignalR**.
- **Motor de Pruebas Totalmente Automático:** Un proceso en segundo plano *(`HealthCheckWorker`)* ejecuta las rutinas diseñadas a la frecuencia especificada.
- **Tipos de Evaluaciones (Health Tests):** Configuración de múltiples tipos de resultados para una consulta: Verificar que exista la data, contar filas, o buscar un valor único específico.
- **Mecanismos de Self-Healing (Autocuración):** En caso de que una prueba lógica no se cumpla, el sistema puede disparar automáticamente una consulta de remediación preconfigurada para corregir el entorno afectado de inmediato.
- **Logs e Historiales:** Registro meticuloso de latencia (en ms), estado de ejecución de los checks históricos, y excepciones.

---

## 📖 Manual de Usuario

### 1. Panel de Inicio (Dashboard)
Apenas ingreses al sistema, se presentará el cuadro de mandos principal donde encontrarás cuadros por cada *Tenant* registrado, revelando su porcentaje de salud (establecido según la cantidad de pruebas que actualmente están aprobadas o falladas).

### 2. Gestión de Tenants (Clientes / Proyectos)
En la sección **"Tenants"** (menú lateral), podrás administrar los endpoints o bases de datos a monitorear.
- Haz clic en **Crear** para añadir un nuevo elemento.
- Se te pedirá ingresar información como el **Nombre**, la **Cadena de Conexión (Connection String)** y seleccionar cuál es el **Proveedor** (SQL Server, SQLite, MySQL o PostgreSQL).
- Podrás editar o eliminar *tenants* en cualquier momento.

### 3. Gestión de Pruebas de Salud (Tests)
En la pestaña de **"Tests"** podrás configurar los chequeos para cada *Tenant*:
- **Frecuencia en Segundos:** Cada cuánto el worker en segundo plano realizará esta prueba.
- **Query / Comando SQL:** La instrucción que el sistema consultará para el chequeo.
- **Resultado Esperado:** Puedes configurar qué espera el servicio para "pasar" el test. (Ej. `Exists`, `Count = 5`, etc.)
- **Auto Remediación:** De forma opcional, si la prueba resulta en error, introduce una consulta SQL alternativa en este campo que se disparará como mecanismo de resolución de emergencia.

---

## ⚙️ Configuración e Instalación (Despliegue)

El proyecto está diseñado de forma modular (*Clean Architecture*) y requiere contar con el **SDK de .NET 10** instalado en tu máquina o servidor destino.

### Prerrequisitos
- [.NET 10.0 SDK](https://dotnet.microsoft.com/)
- Un navegador web moderno.

### Pasos de compilación local

1. Clona el repositorio desde GitHub:
   ```bash
   git clone [URL-DEL-REPOSITORIO]
   cd Data-Health-Monitor
   ```

2. (Opcional) Restaura las dependencias por defecto:
   ```bash
   dotnet restore DHM.slnx
   ```

3. Inicia la aplicación web de manera local. La base de datos interna de operaciones de la aplicación (SQLite local) se generará sola mediante el inicializador incrustado si no la tienes la primera vez:
   ```bash
   dotnet run --project src/DHM.Web/DHM.Web.csproj
   ```

4. Abre tu navegador y dirígete a `http://localhost:5090`. Al primer inicio, un entorno simulado "Demo - Tienda Online" con una base de datos Mock será configurado automáticamente en tu entorno para que experimentes de primera mano. 

> **Nota para Entornos de Producción:** Si planeas subir DHM a un servicio de alojamiento, recuerda no publicar la carpeta con el Mock demolete y ajustar la cadena de conexiones del repositorio interno que utiliza EntityFramework/Dapper a conveniencia.
