dotnet add package LinqToTwitter  
dotnet add package Newtonsoft.Json    
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet build --runtime linux-x64
dotnet publish -o /app --runtime linux-x64 --self-contained true