# 🎬 Scene_68 — Быстрый старт

## ✅ Исправленные ошибки (последнее обновление)

- ✅ Удалён `RenderSettings.RefreshSkybox()` (не существует в Unity)
- ✅ Заменены `Color.orange`, `Color.magenta`, `Color.cyan` на `new Color()`
- ✅ Исправлены большие числа (long → int)
- ✅ Добавлено автоматическое создание префабов

## 📋 Что уже создано

✅ **Скрипты:**
- `S68_Main.cs` — Главный контроллер эпох
- `S68_Era.cs` — Управление декорациями
- `S68_Narrator.cs` — Повествование и тексты
- `S68_SceneSetup.cs` — Автоматическая настройка (Editor script)
- `S68_CreatePrefabs.cs` — **Создание префабов блоков**
- `S68_AddCamera.cs` — Добавление камеры

✅ **Сцена:**
- `Scene_68.unity` — Полностью настроенная сцена с:
  - Камерой (позиция для обзора)
  - Светом (Directional Light)
  - UI (EraTitleText, NumberText, EraDescription, ProgressBar)
  - Менеджерами (GameManager, EraManager, Narrator)
  - Точками спавна (BlockSpawnPoint, PortalSpawnPoint)
  - Землёй (Ground Plane)

✅ **Префабы:**
- Создаются автоматически через `Tools → S68 → Create Block Prefabs`
- Путь: `Assets/Prefabs/Blocks/Scene68/`

---

## 🚀 Настройка за 3 шага (полностью автоматически!)

### Шаг 1: Открыть сцену
```
В Unity Editor:
File → Open Scene → Assets/Scenes/Scene_68.unity
```

### Шаг 2: Создать префабы
```
В меню Unity:
Tools → S68 → Create Block Prefabs
```

Это создаст все префабы блоков в папке `Assets/Prefabs/Blocks/Scene68/`

### Шаг 3: Автоматическая настройка
```
В меню Unity:
Tools → Setup Scene 68
```

Это настроит:
- ✅ Все компоненты (S68_Main, S68_Era, S68_Narrator)
- ✅ Ссылки на камеру и точки спавна
- ✅ Конфигурации эпох
- ✅ Повествование для каждой эпохи
- ✅ UI тексты

### Шаг 4: Автоматическое назначение префабов
```
В меню Unity:
Tools → S68 → Auto Assign Prefabs to Scene_68
```

Это автоматически:
- ✅ Найдёт все созданные префабы
- ✅ Назначит их в соответствующие эпохи
- ✅ Заполнит списки чисел

**Всё! Теперь можно нажать Play ▶**

---

## ▶️ Запуск

После автоматической настройки:

1. Нажми **Play** ▶ в Unity Editor
2. Начнётся эпоха "Ancient World" (15 секунд)
3. Блоки будут появляться автоматически из назначенных префабов
4. Через 15 сек → портал → переход в следующую эпоху
5. И так далее до "Beyond Infinity"

**Каждая эпоха длится 15 секунд, всего ~90 секунд на всё шоу!**

---

## 🎨 Дополнительная настройка

### Добавить звуки
На **GameManager** → **S68_Main**:
- **Era Transition Sound**: `portal.mp3` или `explosion-01.wav`
- **Background Music**: создай список из 6 клипов для каждой эпохи
- **Number Sounds**: звуки для чисел

### Добавить эффекты
Создай префаб портала:
```
1. Create → 3D Object → Cylinder
2. Добавь материал с emission (светящийся)
3. Сохрани как префаб
4. Назначь в Portal Prefab на S68_Main
```

### Настроить камеру
На **GameManager** → **S68_Main**:
- **Camera Positions**: 6 позиций для каждой эпохи
- **Camera Rotations**: углы наклона камеры
- **Camera Speed**: скорость движения (по умолчанию 2)

---

## 🐛 Решение проблем

### Ошибка "Missing Script"
**Причина:** Скрипты не скомпилированы  
**Решение:**
```
Assets → Reimport All
или
Нажми Play ещё раз
```

### Текст не отображается
**Причина:** TextMeshPro не настроен  
**Решение:**
```
Window → TextMeshPro → Import TMP Essential Resources
```

### UI не виден
**Причина:** Canvas не настроен  
**Решение:**
- Убедись, что Canvas активен (галочка в инспекторе)
- Проверь, что тексты назначены в S68_Main

### Сцена пустая
**Причина:** Префабы блоков не назначены  
**Решение:**
- Следуй Шагу 3 выше
- Или используй заглушки (кубы) для теста

---

## 📊 Структура сцены

```
Hierarchy:
├── Main Camera (камера)
├── Directional Light (свет)
├── GameManager (S68_Main) ← главный контроллер
├── EraManager (S68_Era) ← декорации
├── Narrator (S68_Narrator) ← тексты
├── BlockSpawnPoint (точка спавна блоков)
├── PortalSpawnPoint (точка спавна портала)
├── Canvas (UI)
│   ├── EraTitleText (название эпохи)
│   ├── NumberText (число)
│   ├── EraDescription (описание)
│   └── ProgressBar (прогресс)
└── Ground (земля)
```

---

## 💡 Советы

1. **Тестирование:** Используй кнопку "Next Era (Debug)" в GUI для быстрого перехода
2. **Отладка:** Смотри Console (Window → General → Console)
3. **Производительность:** Уменьши decorationCount в S68_Era для слабых ПК
4. **Визуал:** Добавь Skybox из пакета "Fantasy Skybox FREE"

---

## 📞 Нужна помощь?

1. Проверь Console на ошибки
2. Убедись, что все скрипты в папке `Assets/Scripts/`
3. Открой `Scene_68_SETUP.md` для подробной документации

**Удачи в создании ролика! 🎬**
