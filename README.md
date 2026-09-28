# Расписание работников — Anime Shop Simulator

![Anime Shop Simulator](https://img.shields.io/badge/Anime%20Shop%20Simulator-1.0.6-f6a800)
![Version](https://img.shields.io/badge/version-0.1.2-1685d1)
![WolfCore](https://img.shields.io/badge/requires-WolfCore-1685d1)
![MelonLoader](https://img.shields.io/badge/MelonLoader-0.7.3-7952b3)
![License](https://img.shields.io/badge/license-MIT-2ea44f)

**Расписание работников** добавляет отдельные правила рабочего времени для каждой профессии в **Anime Shop Simulator**.

Например, кассир сможет обслужить оставшуюся после закрытия очередь, а выбранные сотрудники — начать работу ещё до открытия магазина.

## Скачать

Готовая сборка находится в **[последнем выпуске](https://github.com/Midfr0st/AnimeShopSimulator-EmployeeSchedules/releases/latest)**.

Для работы необходим [WolfCore](https://github.com/Midfr0st/AnimeShopSimulator-WolfCore).

## Возможности

Для каждой поддерживаемой профессии можно независимо разрешить:

- начинать работу до открытия магазина;
- продолжать работу после закрытия магазина.

Доступны кассир, выкладчик, кладовщик, уборщик и распаковщик. Если обе настройки выключены, профессия полностью следует обычному расписанию игры.

По умолчанию кассир продолжает работу после закрытия, чтобы обслужить оставшихся покупателей.

## Установка

1. Полностью закройте игру.
2. Установите [MelonLoader](https://github.com/LavaGang/MelonLoader).
3. Установите [WolfCore](https://github.com/Midfr0st/AnimeShopSimulator-WolfCore/releases/latest).
4. Скачайте `WolfEmployeeSchedules.dll` из [Releases](https://github.com/Midfr0st/AnimeShopSimulator-EmployeeSchedules/releases/latest).
5. Поместите обе DLL в папку `Anime Shop Simulator\Mods`.
6. Запустите игру.

Подробности: [INSTALLATION.ru.md](docs/INSTALLATION.ru.md).

## Настройки

Откройте `Esc` → `Моды` → `Расписание работников`. Изменения сохраняются в:

```text
Anime Shop Simulator\UserData\WolfEmployeeSchedules.settings.json
```

Мод меняет только границы рабочего времени. Он не заменяет штатный выбор коробок, поиск полок или логику профессий.

## Совместимость

- Anime Shop Simulator `1.0.6`;
- MelonLoader `0.7.3`;
- WolfCore `0.2.5`;
- Windows x64, Unity IL2CPP.

## Если что-то не работает

См. [решение проблем](docs/TROUBLESHOOTING.ru.md). Для отчёта приложите `MelonLoader\Latest.log` и создайте обращение в [GitHub Issues](https://github.com/Midfr0st/AnimeShopSimulator-EmployeeSchedules/issues).

## Связанные проекты

- [WolfCore](https://github.com/Midfr0st/AnimeShopSimulator-WolfCore) — обязательное ядро и меню настроек;
- [Фильтры полок](https://github.com/Midfr0st/AnimeShopSimulator-ShelfFilters);
- [Статистика товаров](https://github.com/Midfr0st/AnimeShopSimulator-ProductStatistics);
- [Отзывы о магазине](https://github.com/Midfr0st/AnimeShopSimulator-ShopReviews).

## Лицензия

Проект распространяется по условиям [MIT License](LICENSE). Это неофициальная пользовательская модификация. Мод предоставляется «как есть» и используется на свой риск.
