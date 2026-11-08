# Kaluz – Sistema Web

Sistema de gestión para tienda de ropa femenina desarrollado con ASP.NET Core MVC (.NET 8) y SQL Server.

## Ejecución en Visual Studio

1. Abrir `Kaluz.sln` con Visual Studio y tener instalado el SDK de .NET 8 y la carga de desarrollo ASP.NET.
2. Crear la base de datos ejecutando `KaluzDB_FINAL.sql` **solo en una instancia de prueba sin datos que deban preservarse** o restaurar un respaldo de la base original.
3. Ajustar la cadena `ConnectionStrings:Kaluz` en `Kaluz.Web/appsettings.json` para el servidor SQL de la computadora.
4. Restaurar los paquetes NuGet y ejecutar con F5.

El script SQL instala la estructura inicial y no incluye los registros de ventas y clientes creados posteriormente. Para migrar los datos utilizar un respaldo aparte; no subir respaldos ni secretos a repositorios públicos.
