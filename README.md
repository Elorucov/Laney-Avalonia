# Laney-Avalonia
Кросс-платформенная версия Laney на UI-фреймворке Avalonia

## Перед началом работы
Сначала установите [.NET SDK 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) и [PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell).

Для проверки корректности установки: `dotnet --version` и `pwsh -v`.

## Клонирование и запуск
```
git clone https://github.com/Elorucov/Laney-Avalonia.git
cd Laney-Avalonia/L2
dotnet run
```

## Сборка
Запустите скрипт **build.ps1**, находящийся в папке **L2**: `pwsh .\build.ps1` на Windows или `pwsh ./build.ps1` на Linux/macOS.
Скрипт скомпилирует релизную версию программы (т. е. выполнит `dotnet publish -c Release`), автоматически увеличивая номер сборки после компиляции. На Windows скомпилируется только версия для этой ОС на трёх архитектурах (x86, x86-64, arm64).
А на Linux и macOS скрипт скомпилирует сборки сразу для обеих ОС (Linux только на x86-64, macOS на x86-64, arm64 для 11 и 12 версии отдельно).

Скрипт в конце выведет путь к скомпилированной программе.
