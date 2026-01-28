# VR Body Tracking App - Deployment Guide

## Сборка для Oculus Quest 3

### Требования
- Unity 2022.3 LTS или выше
- Android Build Support
- Oculus Integration Package
- MediaPipe Unity Plugin

### Шаги сборки

1. **Настройка Build Settings**
   - File → Build Settings
   - Выбрать Android платформу
   - Switch Platform
   - Add Open Scenes (добавить MainScene)

2. **Настройка Player Settings**
   - Player Settings → Other Settings
   - Company Name: YourCompany
   - Product Name: VR Body Tracking
   - Package Name: com.yourcompany.vrbodytracking
   - Minimum API Level: Android 7.0 (API level 24)
   - Target API Level: Android 12.0 (API level 31)
   - Scripting Backend: IL2CPP
   - Target Architectures: ARM64

3. **VR Настройки**
   - XR Plug-in Management → Oculus
   - Включить Oculus XR Plugin
   - Virtual Reality Supported: true

4. **Разрешения**
   - В Player Settings → Other Settings → Required permissions:
   - Camera
   - Microphone
   - Internet

5. **Настройка качества**
   - Quality Settings → Very Low (для оптимизации)
   - Включить Dynamic Batching
   - Включify GPU Instancing

### Установка на Quest 3

1. **Включение Developer Mode**
   - Включить Developer Mode в Oculus Quest 3
   - Настройки → Developer → Developer Mode

2. **Подключение к ПК**
   - Подключить Quest 3 через USB-C
   - Разрешить USB debugging

3. **Установка через adb**
   ```bash
   adb install -r VRBodyTracking.apk
   ```

4. **Запуск приложения**
   - Найти приложение в библиотеке Quest
   - Запустить и предоставить разрешения на камеру

## Оптимизация производительности

### Рекомендуемые настройки
- Target Frame Rate: 90 FPS
- Resolution Scale: 80%
- Fixed Foveated Rendering: High
- Occlusion: Enabled
- Shadows: Disabled или Hard Only

### Мониторинг производительности
- Использовать Oculus Performance HUD
- Следить за FPS и температурой устройства
- Оптимизировать при FPS < 80

## Тестирование

### Обязательные тесты
1. **VR функциональность**
   - Правильная работа VR рендеринга
   - Отслеживание контроллеров
   - Hand tracking

2. **Трекинг тела**
   - Захват видео с вебкамеры
   - Распознавание поз
   - Синхронизация с аватаром

3. **Производительность**
   - Стабильный FPS
   - Отсутствие лагов
   - Температура устройства

### Известные проблемы
- Вебкамера может работать с задержкой
- При низкой освещенности трекинг ухудшается
- Высокая нагрузка на процессор

## Поддержка устройств

### Поддерживаемые устройства
- Oculus Quest 3 (основная цель)
- Oculus Quest 2 (ограниченная поддержка)
- Oculus Quest Pro (полная поддержка)

### Минимальные требования
- Snapdragon XR2 Gen 2
- 8GB RAM
- Вебкамера с поддержкой 1280x720 @ 30fps

## Обновления

### Процесс обновления
1. Тестирование новой версии
2. Сборка APK
3. Загрузка в Oculus Store
4. Публикация обновления

### Версионирование
- Major.Minor.Patch формат
- Обратная совместимость
- Миграция настроек
