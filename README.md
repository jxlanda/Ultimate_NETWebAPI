# WebAPI en .NET 5

Características del WebAPI:
* Log to file (nlog)
* Repository Pattern

## ¿Que es el Patrón Repositorio?

El Patrón de Diseño de Repositorio es uno de los más populares para crear una aplicación de nivel empresarial. Nos restringe a trabajar directamente con los datos de la aplicación y crea nuevas capas para las operaciones de la base de datos, la lógica de negocio y la interfaz de usuario de la aplicación.

Por qué debería usar el Patrón de Diseño de Repositorio.
- El código de acceso a los datos puede ser reutilizado.
- Es fácil de implementar la lógica del dominio.
- Nos ayuda a desacoplar la lógica.
- La lógica de negocio puede ser probada fácilmente sin acceso a los datos.
- También es una buena manera de implementar la inyección de dependencia que hace que el código sea más testeable.
