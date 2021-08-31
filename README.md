# WebAPI en .NET 5

Características del WebAPI:
* Log (nlog)
* Patrón Repositorio
* Global Exception Middleware
* Bulk Operations
* AutoMapper
* Paginación
* Filtrado
* Ordenamiento
* Data Shaping
* HATEOAS 

## ¿Que es el Patrón Repositorio?

El Patrón de Diseño de Repositorio es uno de los más populares para crear una aplicación de nivel empresarial. Nos restringe a trabajar directamente con los datos de la aplicación y crea nuevas capas para las operaciones de la base de datos, la lógica de negocio y la interfaz de usuario de la aplicación.

Por qué debería usar el Patrón de Diseño de Repositorio.
- El código de acceso a los datos puede ser reutilizado.
- Es fácil de implementar la lógica del dominio.
- Nos ayuda a desacoplar la lógica.
- La lógica de negocio puede ser probada fácilmente sin acceso a los datos.
- También es una buena manera de implementar la inyección de dependencia que hace que el código sea más testeable.

## ¿Que es el HATEOAS?
Hypermedia as the Engine of Application State (HATEOAS), en español, hipermedia como motor del estado de la aplicación, es un componente de la arquitectura de aplicación REST que lo distingue de otras arquitecturas.

Con HATEOAS, un cliente interacciona con una aplicación de red cuyos servidores de aplicación proporcionan información dinámicamente a través de hipermedia. Un cliente REST necesita poco o ningún conocimiento previo sobre cómo interactuar con una aplicación o servidor más allá de un conocimiento genérico de los hipermedia.

[Ver en Wikipedia](https://es.wikipedia.org/wiki/HATEOAS)

Para ver los enlaces dínamicos en API:
- Agregar o editar header "Accept" a la petición
- Colocar el valor "application/vnd.example.hateoas+json" 


## SQL SCRIPT 

CREATE TABLE Account (
AccountId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
DateCreated Date NOT NULL,
AccountType NVARCHAR(45) NOT NULL,
OwnerId UNIQUEIDENTIFIER,
FOREIGN KEY (OwnerId) REFERENCES Owner(OwnerId)
);

CREATE TABLE Owner(
	OwnerId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
	Name NVARCHAR(60) NOT NULL,
	DateOfBirth DATE NOT NULL,
	Address NVARCHAR(100) NOT NULL
);

INSERT INTO owner
VALUES ('24fd81f8-d58a-4bcc-9f35-dc6cd5641906','John Keen','1980-12-05','61 Wellfield Road'), 
('261e1685-cf26-494c-b17c-3546e65f5620','Anna Bosh','1974-11-14','27 Colored Row'),
('a3c1880c-674c-4d18-8f91-5d3608a2c937','Sam Query','1990-04-22','91 Western Roads'),
('f98e4d74-0f68-4aac-89fd-047f1aaca6b6','Martin Miller','1983-05-21','3 Edgar Buildings');

INSERT INTO account
VALUES ('03e91478-5608-4132-a753-d494dafce00b','2003-12-15','Domestic','f98e4d74-0f68-4aac-89fd-047f1aaca6b6'),
('356a5a9b-64bf-4de0-bc84-5395a1fdc9c4','1996-02-15','Domestic','261e1685-cf26-494c-b17c-3546e65f5620'), 
('371b93f2-f8c5-4a32-894a-fc672741aa5b','1999-05-04','Domestic','24fd81f8-d58a-4bcc-9f35-dc6cd5641906'), 
('670775db-ecc0-4b90-a9ab-37cd0d8e2801','1999-12-21','Savings','24fd81f8-d58a-4bcc-9f35-dc6cd5641906'), 
('a3fbad0b-7f48-4feb-8ac0-6d3bbc997bfc','2010-05-28','Domestic','a3c1880c-674c-4d18-8f91-5d3608a2c937'), 
('aa15f658-04bb-4f73-82af-82db49d0fbef','1999-05-12','Foreign','24fd81f8-d58a-4bcc-9f35-dc6cd5641906'), 
('c6066eb0-53ca-43e1-97aa-3c2169eec659','1996-02-16','Foreign','261e1685-cf26-494c-b17c-3546e65f5620'), 
('eccadf79-85fe-402f-893c-32d3f03ed9b1','2010-06-20','Foreign','a3c1880c-674c-4d18-8f91-5d3608a2c937');