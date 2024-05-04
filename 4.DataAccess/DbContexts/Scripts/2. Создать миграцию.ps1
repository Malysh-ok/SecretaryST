Write-Host "Создаем миграцию" -ForegroundColor Green
                Write-Host ""
                
                $dbContextProjPath = [System.IO.Path]::Combine("D:\Документы\_ПРОЕКТЫ\_Программирование\C#\SecretaryST\1.Presentation\Shell\", "..\..\4.DataAccess\DbContexts\DbContexts.csproj")
                $dbContextProjPath = [System.IO.Path]::GetFullPath($dbContextProjPath)
            
                $MigrationName = Read-Host "Введите название миграции"
                dotnet ef migrations add $MigrationName -o Migrations -s "D:\Документы\_ПРОЕКТЫ\_Программирование\C#\SecretaryST\1.Presentation\Shell\Shell.csproj" -p $dbContextProjPath -v
                pause
