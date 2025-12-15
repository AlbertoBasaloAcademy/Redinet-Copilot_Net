# Astro Bookings Briefing

AstroBookings será un API para una aplicación de reservas para viajes espaciales. Gestiona cohetes, vuelos y reservas con control de capacidad, precios y estados.

## Funcionalidades clave

- Crear `Rocket` con validación
- Listar todos o buscar `Rocket` por id
- Crear `Flight` con fecha de lanzamiento futura y precio > 0 → `SCHEDULED`
- Listar `Flight` futuros (filtro estado)
- Crear `Booking` con descuentos, al llegar al mínimo → `CONFIRMED` 
- Consultar `Booking` por `Flight`
- Cancelar `Flight` → `CANCELLED` 

### Entidades

- `Rocket`
    - Nombre (obligatorio), 
    - Capacidad (máx. 10), 
    - Velocidad (opcional), 
    - Alcance (opcional: `LEO`, `MOON`, `MARS`)

- `Flight`
    - Fecha de lanzamiento futura, 
    - Precio base, 
    - Mínimo de pasajeros (5 default)
    - Estados: `SCHEDULED`, `CONFIRMED`, `SOLD_OUT`, `CANCELLED` , `DONE` , cambio automático según reservas

- `Booking`
    - Pasajero (nombre, email),  
    - Precio final, cálculo con descuentos

### Lógica

- Alcanza mínimo pasajeros → `CONFIRMED` + notificar
- No superar capacidad cohete → `SOLD_OUT` al límite
- Falta 1 semana sin mínimo → `CANCELLED` + notificar + devolver pago
- Reservar solo si vuelo no lleno `SOLD_OUT` ni cancelado `CANCELLED`
- Realizar el vuelo → `DONE`

- Descuentos (precedencia, solo uno)
    1. Última plaza → 0%
    2. Falta 1 para completar mínimo → 30%
    3. Resto → 10%

## Requisitos técnicos

- Se usará como demostración y ejercicio en talleres de formación
  - No se necesita persistencia en base de datos, ni seguridad
- Es una aplicación backend con API RESTful
  - C#, .NET recomendado
  - Usar las mínimas dependencias posibles  

