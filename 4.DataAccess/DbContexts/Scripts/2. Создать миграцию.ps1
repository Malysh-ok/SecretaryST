Write-Host "������� ��������" -ForegroundColor Green
                Write-Host ""
                
                $dbContextProjPath = [System.IO.Path]::Combine("D:\���������\_�������\_����������������\C#\SecretaryST\1.Presentation\Shell\", "..\..\4.DataAccess\DbContexts\DbContexts.csproj")
                $dbContextProjPath = [System.IO.Path]::GetFullPath($dbContextProjPath)
            
                $MigrationName = Read-Host "������� �������� ��������"
                dotnet ef migrations add $MigrationName -o Migrations -s "D:\���������\_�������\_����������������\C#\SecretaryST\1.Presentation\Shell\Shell.csproj" -p $dbContextProjPath -v
                pause
