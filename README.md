Service Discovery

Add Adventure.ServiceDefaults reference to targeted api projects
Then, in the api project, do the following
builder.AddServiceDefaults();
app.MapDefaultEndPoints();
----------------------------------------------------------------------------------------------------
Person
Production
Purchase
Sales
Warehouse
Finance
HumanResource

----------------------------------------------------------------------------------------------------
Microsoft.EntityFrameworkCore.Design
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.EntityFrameworkCore.Tools
Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite

dotnet ef dbcontext scaffold "data source=CIQLAP139\SQL2022_INSTANCE;initial catalog=AdventureWorks2022;user id=sa;password=cloudiq@123;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --output-dir Database/AdventureWorks/Entities --context-dir Database/AdventureWorks/DBContext --context AdventureWorksDbContext --no-onconfiguring --no-pluralize
----------------------------------------------------------------------------------------------------
Setting up certificate
dotnet dev-certs https -ep ./aspnetapp.pfx -p cloudiq@123
----------------------------------------------------------------------------------------------------
docker-compose up --build -d
docker-compose down
----------------------------------------------------------------------------------------------------