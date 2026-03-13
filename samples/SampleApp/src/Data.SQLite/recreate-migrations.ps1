Remove-Item Migrations -Force -Confirm:$false -Recurse
dotnet ef migrations add Initial
