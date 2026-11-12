# Account Manager — этап 1

Минимальное Windows-приложение для независимых Chromium-профилей. Реализованы создание профиля, сохранение в SQLite, запуск Chromium через Microsoft.Playwright с persistent context, корректное закрытие и повторное открытие с той же папкой профиля.

## Архитектура

- `AccountManager.Core` — доменная модель и состояния профиля.
- `AccountManager.Application` — use case-сервис и контракты репозитория, Browser Agent, прокси и расширений.
- `AccountManager.Infrastructure` — EF Core/SQLite и файловое хранилище профилей.
- `AccountManager.Agent` — реализация Browser Agent на Playwright; сейчас подключается к WPF in-process, чтобы первый этап оставался простым. Контракт позволяет вынести его в отдельный процесс на следующем этапе.
- `AccountManager.Desktop` — минимальный WPF/MVVM-клиент.
- `AccountManager.Server` — проверочный каркас будущего backend (`/health`), без управления профилями.
- `AccountManager.Web` — резерв проекта будущей React/Vite-панели.

Данные по умолчанию находятся в `%LOCALAPPDATA%\AccountManager`: база `account-manager.db`, профили — в `profiles\{uuid}`. Каждая папка запускается как отдельный persistent Chromium context.

## Запуск

1. Установите .NET 10 SDK x64 на Windows. Файл `global.json` фиксирует ветку SDK 10.0. (https://dotnet.microsoft.com/download/dotnet/10.0)
2. Откройте PowerShell в корне репозитория.
3. Восстановите зависимости: `dotnet restore AccountManager.sln`.
4. Соберите: `dotnet build AccountManager.sln -c Debug`.
5. Установите браузер Playwright: `pwsh src/AccountManager.Agent/bin/Debug/net10.0/playwright.ps1 install chromium` после первой сборки, либо запустите скрипт `playwright.ps1` из фактически созданной папки output.
6. Запустите WPF: `dotnet run --project src/AccountManager.Desktop -c Debug`.

Для запуска двойным кликом используйте `Start-AccountManager.cmd`. Если .NET SDK ещё не установлен, `scripts\Setup.ps1` автоматически установит локальную копию .NET 10 SDK, восстановит NuGet-зависимости, соберёт приложение и установит Chromium Playwright.

В окне нажмите «Создать профиль», затем выберите его и нажмите «Запустить». Авторизуйтесь вручную в открывшемся Chromium, закройте кнопкой «Закрыть», а затем запустите снова — данные persistent context будут взяты из той же папки.

Для прокси выберите профиль, нажмите «Прокси…», укажите тип и адрес (`host:port` или полный URI `http://host:port`, `https://host:port`, `socks5://host:port`), затем сохраните. Если схема не указана, она добавляется автоматически по выбранному типу. Настройка применяется при следующем запуске профиля. Пароль прокси защищается Windows DPAPI текущей учётной записи.

## Тесты

```powershell
dotnet test tests/AccountManager.UnitTests/AccountManager.UnitTests.csproj
dotnet test tests/AccountManager.IntegrationTests/AccountManager.IntegrationTests.csproj
```

Для полноценной Windows-проверки дополнительно нужно вручную проверить окно WPF, наличие Chromium, создание двух профилей, независимость cookies/local storage, закрытие и повторное открытие. В текущем окружении .NET SDK не установлен: доступен только runtime, поэтому restore/build/test здесь выполнить нельзя.

## Границы этапа

Полноценный список прокси, проверка доступности, полноценный Server API, SignalR, отдельный процесс Agent/IPC, React-панель и интеграция расширения остаются на следующих этапах. Контракты `IProxyProvider`, `IExtensionManager` и `IBrowserAgent` отделяют эти функции от UI и доменной логики.
