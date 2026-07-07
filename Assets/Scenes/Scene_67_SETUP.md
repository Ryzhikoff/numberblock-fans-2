# 🎬 Настройка Scene_67 - Битва Блок-Чисел

## 📋 Шаг за шагом

### 1. Открыть сцену
- В Unity: `File → Open Scene` → выбери `Assets/Scenes/Scene_67.unity`

### 2. Создать GameManager
```
Hierarchy (правая панель):
  → Правый клик → Create Empty
  → Переименуй в "GameManager"
  → Добавь компонент: Add Component → S67_Main
```

### 3. Создать точки спавна
В Hierarchy создай 3 пустых объекта:

**LeftSpawnPoint:**
- Position: `X=-15, Y=2, Z=0`

**RightSpawnPoint:**
- Position: `X=15, Y=2, Z=0`

**CenterMergePoint:**
- Position: `X=0, Y=2, Z=0`

### 4. Настроить S67_Main компонент
На GameManager в инспекторе заполни:

**Block Prefabs** (List, размер 6):
```
[0] = 50.prefab
[1] = 100.prefab
[2] = 200.prefab
[3] = 400.prefab
[4] = 800.prefab
[5] = 1000.prefab
```

**Spawn Points:**
- Left Spawn Point → перетащи LeftSpawnPoint
- Right Spawn Point → перетащи RightSpawnPoint
- Center Merge Point → перетащи CenterMergePoint

**Camera:**
- Battle Camera → перетащи Main Camera

**UI Texts:**
- Round Text → создай TextMeshPro (см. шаг 5)
- Result Text → создай TextMeshPro (см. шаг 5)

### 5. Создать UI (TextMeshPro)
```
Hierarchy → Right Click → UI → Text - TextMeshPro
```

**RoundText:**
- Anchor: Top Center
- Anchored Position: `X=0, Y=-50`
- Size: `Width=400, Height=100`
- Text: `ROUND 1`
- Font Size: `48`
- Color: Белый

**ResultText:**
- Anchor: Center
- Anchored Position: `X=0, Y=0`
- Size: `Width=600, Height=150`
- Text: `50 + 50 = 100`
- Font Size: `64`
- Color: Жёлтый
- GameObject: выключить (галочку снять)

### 6. Настроить префабы блоков
Для каждого префаба (50, 100, 200, 400, 800, 1000):
- Убедись, что есть компонент **Scale** с правильными значениями
- Добавь **Rigidbody** (если нет):
  - `isKinematic = true`
  - `useGravity = false`

### 7. (Опционально) Добавить звуки
В S67_Main компоненте:
- **Round Start Sound** — звук начала раунда
- **Victory Sound** — звук победы
- **Background Music** — фоновая музыка

### 8. (Опционально) Добавить эффекты
Создай префаб эффекта:
```
Create → 3D Object → Sphere
→ Добавь материал (жёлтый/оранжевый)
→ Добавь компонент S67_MergeEffect
→ Сохрани как префаб
→ Назначь в Merge Effect Prefab
```

### 9. Запуск
- Нажми **Play** ▶
- Блоки 50+50 сольются в 100
- Затем 100+100 → 200 → 400 → 800 → 1000

---

## 🔧 Структура сцены

```
Hierarchy:
├── Main Camera
├── Directional Light
├── GameManager (S67_Main)
├── SpawnPoints
│   ├── LeftSpawnPoint (-15, 2, 0)
│   ├── RightSpawnPoint (15, 2, 0)
│   └── CenterMergePoint (0, 2, 0)
├── Canvas (UI)
│   ├── RoundText
│   └── ResultText
└── Environment (опционально)
```

---

## ⚠️ Возможные проблемы

### Ошибка "Missing Script"
- Убедись, что скрипты S67_Main.cs, S67_Fighter.cs в папке Assets/Scripts

### Блоки не появляются
- Проверь, что префабы назначены в Block Prefabs
- Проверь, что точки спавна имеют правильные координаты

### Текст не отображается
- Убедись, что TextMeshPro установлен (Window → Package Manager → TextMeshPro)

---

## 📝 Примечания

- Раунды: 50+50=100, 100+100=200, 200+200=400, 400+400=800, 800+800=1000
- Время раунда: ~5 секунд
- Камера двигается автоматически к центру после каждого раунда
