Write-Host "������� ������ ��������" -ForegroundColor Green
                Write-Host ""
                
                $dbContextProjPath = [System.IO.Path]::Combine("D:\���������\_�������\_����������������\C#\SecretaryST\1.Presentation\Shell\", "..\..\4.DataAccess\DbContexts\DbContexts.csproj")
                $dbContextProjPath = [System.IO.Path]::GetFullPath($dbContextProjPath)
            
                dotnet ef migrations add Initial -o Migrations -s "D:\���������\_�������\_����������������\C#\SecretaryST\1.Presentation\Shell\Shell.csproj" -p $dbContextProjPath -v
                pause
