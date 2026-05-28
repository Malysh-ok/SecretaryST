Write-Host "Удаляем последнюю миграцию" -ForegroundColor Green
                Write-Host ""
                
                $dbContextProjPath = [System.IO.Path]::Combine("D:\Документы\_ПРОЕКТЫ\_Программирование\C#\SecretaryST\1.Presentation\Shell\", "..\..\4.DataAccess\DbContexts\DbContexts.csproj")
                $dbContextProjPath = [System.IO.Path]::GetFullPath($dbContextProjPath)
            
                dotnet ef migrations remove -s "D:\Документы\_ПРОЕКТЫ\_Программирование\C#\SecretaryST\1.Presentation\Shell\Shell.csproj" -p $dbContextProjPath -v
                pause
