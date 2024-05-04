********************************************
********** ДЛЯ EF Core И МИГРАЦИЙ **********
********************************************

Для установки dotnet-ef (.NET Core CLI) набрать в терминале:
    dotnet tool install --global dotnet-ef
Для обновления:
    ...можно с версией:
    dotnet tool update --global dotnet-ef --version X.Y.Z

В проекте должны быть установлены NuGet-пакеты:
    - Microsoft.EntityFrameworkCore.Design
    - Microsoft.EntityFrameworkCore


Работа с миграциями (все делаем в терминале):
1) Задаем текущую директорию (проект, где находится класс DbContextFactory, в нашем случае Shell):
	cd 1.Presentation\Shell
2) Создаем миграцию (в качестве примера указано имя Initial - для первой миграции):
	dotnet ef migrations add Initial -o Migrations -p <Абсолютный_путь_к_файлу_проекта_с_контекстом_БД>
3) Применяем миграцию (обновляем БД):
	dotnet ef database update -p <Абсолютный_путь_к_файлу_проекта_с_контекстом_БД>
3) Удаляем последнюю миграцию (если нужно):
	dotnet ef migrations remove -p <Абсолютный_путь_к_файлу_проекта_с_контекстом_БД>
	
UPD (01.06.2023)
Абзац выше устарел. Теперь создаются скрипты - см. Shell.csproj (в нашем случае).