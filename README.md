# Better Build Settings

Better Build Settings is a modular Unity build configuration tool designed for managing repeatable build setups inside the Unity Editor and CI/CD pipelines.

The tool provides:

- multiple build profiles;
- modular build pipeline customization;
- Addressables group control;
- scripting define symbol control;
- automatic restore of project state after build;
- JSON-based configuration storage.

---

# Features

## Build Profiles

Create and manage multiple build configurations.

Examples:

- `default`
- `android_dev`
- `android_prod`
- `steam_demo`

Each profile stores:

- enabled modules;
- module settings;
- build-related configuration state.

Profiles are persisted as JSON files.

---

## Modular Architecture

The build pipeline is split into modules.

Each module can:

- be enabled/disabled;
- serialize its own config;
- apply temporary project changes before build;
- restore previous state after build.

Current modules:

- Addressables Module
- Defines Module

---

# Addressables Module

Controls which Addressables groups are included in the build.

Features:

- select enabled Addressables groups from dropdown;
- automatically disable all other groups during build;
- restore original `Include In Build` values after build;
- validation for missing Addressables groups.

Example config:

```json
{
  "restoreAfterBuild": true,
  "enabledGroups": [
    "Base",
    "Gameplay",
    "UI"
  ]
}
```

---

# Defines Module

Controls scripting define symbols.

Features:

- select defines from project define list;
- override defines during build;
- restore original defines after build.

Example config:

```json
{
  "restoreAfterBuild": true,
  "enabledDefines": [
    "PRODUCTION",
    "STEAM_BUILD"
  ]
}
```

---

# Build Flow

When pressing `BUILD`:

1. Current profile is saved;
2. Enabled modules are collected;
3. Modules apply temporary project changes;
4. Unity `BuildPipeline.BuildPlayer` is executed;
5. Modules restore original project state.

Restore happens even if the build fails.

---

# UI Overview

Open tool from:

```text
Tools / Build Settings
```

Window contains:

## Left Panel

- list of build modules;
- module enable/disable toggles.

## Right Panel

- selected module configuration.

## Top Toolbar

- profile selection;
- create new profile;
- save config;
- revert changes.

## Bottom Toolbar

- build button.

---

# Configuration Storage

Profiles are stored as JSON.

Example:

```json
{
  "Modules": [
    {
      "ModuleId": "addressables",
      "Enabled": true,
      "JsonPayload": {
        "restoreAfterBuild": true,
        "enabledGroups": [
          "Gameplay",
          "UI"
        ]
      }
    }
  ]
}
```

---

# Goals

This tool is designed for:

- repeatable builds;
- build reproducibility;
- reducing manual Unity configuration changes;
- CI/CD integration;
- modular build customization.

---

# Current Limitations

Current implementation:

- targets `StandaloneWindows64`;
- uses local JSON configuration;
- does not yet provide command-line CI entry points;
- does not yet support preset inheritance.

---

# Planned Features

Potential future improvements:

- CLI build entry point;
- build target selection;
- platform-specific profiles;
- preset system;
- profile inheritance;
- validation pipeline;
- build reports;
- custom module registration.

---

# Design Principles

The tool follows several principles:

- build profiles should be self-contained;
- temporary build changes should be reversible;
- modules should own their own serialization;
- configuration should remain human-readable;
- CI and local builds should behave identically.

---

# Requirements

- Unity Editor
- Odin Inspector
- Newtonsoft Json
- Unity Addressables

---

# License

Internal tool / work in progress.

# Better Build Settings

Better Build Settings — это модульный инструмент для Unity, предназначенный для управления build-конфигурациями внутри Unity Editor и CI/CD пайплайнов.

Инструмент предоставляет:

- несколько build-профилей;
- модульную систему настройки билда;
- управление Addressables groups;
- управление scripting define symbols;
- автоматическое восстановление состояния проекта после билда;
- хранение конфигураций в JSON.

---

# Возможности

## Build Profiles

Инструмент поддерживает несколько build-конфигураций.

Примеры:

- `default`
- `android_dev`
- `android_prod`
- `steam_demo`

Каждый профиль хранит:

- список включённых модулей;
- настройки модулей;
- состояние build-конфигурации.

Профили сохраняются в JSON-файлы.

---

## Модульная архитектура

Build pipeline разделён на независимые модули.

Каждый модуль может:

- включаться/выключаться;
- сериализовать собственную конфигурацию;
- временно изменять настройки проекта перед билдом;
- восстанавливать исходное состояние после билда.

Текущие модули:

- Addressables Module
- Defines Module

---

# Addressables Module

Модуль управляет тем, какие Addressables groups будут включены в билд.

Возможности:

- выбор Addressables groups через dropdown;
- автоматическое отключение остальных групп перед билдом;
- восстановление исходных `Include In Build` после билда;
- валидация отсутствующих групп.

Пример конфигурации:

```json
{
  "restoreAfterBuild": true,
  "enabledGroups": [
    "Base",
    "Gameplay",
    "UI"
  ]
}
```

---

# Defines Module

Модуль управляет scripting define symbols.

Возможности:

- выбор define symbols из проекта;
- переопределение define symbols перед билдом;
- восстановление исходных define symbols после билда.

Пример конфигурации:

```json
{
  "restoreAfterBuild": true,
  "enabledDefines": [
    "PRODUCTION",
    "STEAM_BUILD"
  ]
}
```

---

# Процесс билда

При нажатии кнопки `BUILD`:

1. Сохраняется текущий build profile;
2. Собирается список активных модулей;
3. Модули применяют временные изменения;
4. Вызывается `Unity BuildPipeline.BuildPlayer`;
5. Модули восстанавливают исходное состояние проекта.

Восстановление происходит даже при ошибке билда.

---

# Интерфейс

Открытие инструмента:

```text
Tools / Build Settings
```

Окно состоит из:

## Левая панель

- список build-модулей;
- включение/выключение модулей.

## Правая панель

- настройки выбранного модуля.

## Верхняя панель

- выбор build profile;
- создание нового profile;
- сохранение конфигурации;
- откат изменений.

## Нижняя панель

- кнопка запуска билда.

---

# Хранение конфигураций

Профили сохраняются в JSON.

Пример:

```json
{
  "Modules": [
    {
      "ModuleId": "addressables",
      "Enabled": true,
      "JsonPayload": {
        "restoreAfterBuild": true,
        "enabledGroups": [
          "Gameplay",
          "UI"
        ]
      }
    }
  ]
}
```

---

# Назначение инструмента

Инструмент предназначен для:

- воспроизводимых билдов;
- уменьшения ручных изменений в Unity;
- интеграции с CI/CD;
- модульной настройки build pipeline;
- централизованного управления build-конфигурациями.

---

# Текущие ограничения

Текущая версия:

- ориентирована на `StandaloneWindows64`;
- использует локальные JSON-конфиги;
- пока не содержит CLI entry point для CI;
- пока не поддерживает inheritance или preset system.

---

# Планируемые улучшения

Потенциальные улучшения:

- CLI build entry point;
- выбор build target;
- platform-specific profiles;
- система пресетов;
- inheritance конфигураций;
- pipeline валидации;
- build reports;
- регистрация пользовательских модулей.

---

# Принципы архитектуры

Инструмент строится на следующих принципах:

- build profile должен быть самодостаточным;
- временные изменения должны быть обратимыми;
- каждый модуль отвечает за собственную сериализацию;
- конфигурация должна оставаться читаемой;
- CI и локальные билды должны работать одинаково.

---

# Требования

- Unity Editor
- Odin Inspector
- Newtonsoft Json
- Unity Addressables

---

# License

Internal tool / work in progress.
