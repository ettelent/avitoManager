# Архитектура этапа 1

`Core` не знает о WPF, EF Core или Playwright. `Application` содержит сценарии и интерфейсы. `Infrastructure` реализует SQLite и файловое хранилище. `Agent` реализует Playwright. `Desktop` только связывает DI, ViewModel и Views. `Server` и `Web` присутствуют как точки расширения и не участвуют в запуске WPF.

Путь запуска профиля: `MainViewModel -> AccountManagerService -> IBrowserAgent -> PlaywrightBrowserAgent -> Chromium persistent context`.
