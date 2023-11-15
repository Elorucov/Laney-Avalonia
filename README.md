# Laney-Avalonia
Кросс-платформенная версия Laney на UI-фреймворке Avalonia

## Перед началом работы
Сначала установите [.NET SDK 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) и [PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell).

Для проверки корректности установки: `dotnet --version` и `pwsh -v`.

## Клонирование и запуск
```
git clone https://github.com/Elorucov/Laney-Avalonia.git
cd Laney-Avalonia/L2
dotnet run
```

## Сборка
Запустите скрипт **build_aot.ps1**, находящийся в папке **L2**: `pwsh .\build_aot.ps1` на Windows или `pwsh ./build_aot.ps1` на Linux/macOS.
Скрипт скомпилирует релизную версию программы (т. е. выполнит `dotnet publish -c Release`), автоматически увеличивая номер сборки после компиляции. Для Windows и macOS будут скомпилированы x86-64 и arm64-версии, для Linux — только x86-64. 32-битные версии не поддерживаются.

Скрипт в конце выведет путь к скомпилированной программе.

**Обратите внимание:** скрипт скомпилирует бинарник только для той ОС, на которой запущен скрипт.

Скрипту можно передавать аргумент `channel`, доступны два значения: `BETA` и `RELEASE`.
```
pwsh ./build_aot.ps1 -channel RELEASE
```
