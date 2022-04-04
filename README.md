# .NET 6 Web API

## :star: Features
    * Log (nlog)
    * Repository Pattern
    * Global Exception Middleware
    * Bulk Operations
    * AutoMapper
    * Pagination
    * Filter
    * Sort
    * Data Shaping
    * HATEOAS 
    * JWT Authorization

## :page_with_curl: NLog
NLog is a free logging platform for .NET with rich log routing and management capabilities. It makes it easy to produce and manage high-quality logs for your application regardless of its size or complexity. [Nlog Repository.](https://github.com/NLog/NLog)

## :page_with_curl: Repository Pattern

Repositories are classes or components that encapsulate the logic required to access data sources. They centralize common data access functionality, providing better maintainability and decoupling the infrastructure or technology used to access databases from the domain model layer. If you use an Object-Relational Mapper (ORM) like Entity Framework, the code that must be implemented is simplified, thanks to LINQ and strong typing. This lets you focus on the data persistence logic rather than on data access plumbing. [More Info.](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design#:~:text=of%20Work%20patterns.-,The%20Repository%20pattern,from%20the%20domain%20model%20layer.)

## :page_with_curl: HATEOAS
HATEOAS (Hypermedia as the Engine of Application State) is a constraint of the REST application architecture. HATEOAS keeps the REST style architecture unique from most other network application architectures. REST architectural style lets us use the hypermedia links in the API response contents. It allows the client to dynamically navigate to the appropriate resources by traversing the hypermedia links.
Navigating hypermedia links is conceptually the same as browsing through web pages by clicking the relevant hyperlinks to achieve a final goal.

For example, the given below JSON response may be from an API like HTTP GET http://api.domain.com/management/departments/10
~~~
{
    "departmentId": 10,
    "departmentName": "Administration",
    "locationId": 1700,
    "managerId": 200,
    "links": [
        {
            "href": "10/employees",
            "rel": "employees",
            "type" : "GET"
        }
    ]
}
~~~
In the preceding example, the response returned by the server contains hypermedia links to employee resources 10/employees which can be traversed by the client to read employees belonging to the department. [More info.](https://restfulapi.net/hateoas/)

### :pushpin: How to active HATEOAS:
    1. Need to add Header "Accept" to the request with value "application/vnd.example.hateoas+json"


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

CREATE TABLE Users (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(255),
    FirstLastName NVARCHAR(255),
    SecondLastName NVARCHAR(255),
    AvatarUrl NVARCHAR(320),
    Email NVARCHAR(320) NOT NULL UNIQUE,
	[Password] NVARCHAR(255) NOT NULL
);
