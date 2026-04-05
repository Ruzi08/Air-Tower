# 📜 Соглашение о коммитах для Unity-проекта

Мы используем **Conventional Commits** с расширениями для геймдева. Это позволяет автоматически генерировать changelog, понимать историю изменений и упрощает код-ревью.

## 🔤 Формат коммита

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Примеры:

```
feat(player): add double jump mechanic

fix(ui): health bar not updating after damage

perf(rendering): optimize occlusion culling in Forest scene

docs(readme): update installation instructions
```

## 🏷️ Type (обязательный)

| Type | Описание | Пример |
|------|----------|--------|
| **feat** | Новая функциональность | `feat(inventory): add item stacking` |
| **fix** | Исправление бага | `fix(physics): player falling through floor` |
| **docs** | Изменение документации | `docs: update setup guide` |
| **style** | Форматирование, пробелы, точки с запятой (не влияет на логику) | `style: remove unused using statements` |
| **refactor** | Рефакторинг без изменения функциональности | `refactor(player): extract movement logic` |
| **perf** | Улучшение производительности | `perf(shaders): reduce draw calls` |
| **test** | Добавление или исправление тестов | `test(combat): add damage calculation tests` |
| **build** | Изменения в сборке, зависимостях, CI/CD | `build: update Unity version to 2022.3.10` |
| **ci** | Настройка CI/CD | `ci: add automatic builds for PR` |
| **chore** | Обслуживание (обновление пакетов, миграция ассетов) | `chore: update TextMeshPro to 3.2.0` |
| **revert** | Отмена предыдущего коммита | `revert: undo commit abc123` |

## 🎯 Scope (рекомендуемый)

Для Unity-проектов используйте одну из категорий:

| Scope | Описание |
|-------|----------|
| `player` | Механики игрока, управление, анимации |
| `ui` | Интерфейс, меню, HUD |
| `combat` | Боевая система, урон, оружие |
| `inventory` | Инвентарь, предметы, экипировка |
| `physics` | Физика, коллизии, рейкасты |
| `audio` | Звуки, музыка, микшер |
| `ai` | ИИ врагов, навигация, поведение |
| `save` | Сохранения, загрузки, PlayerPrefs |
| `netcode` | Мультиплеер, синхронизация |
| `scene` | Изменения в сценах (префабы, расположение объектов) |
| `shaders` | Шейдеры, материалы, визуальные эффекты |
| `editor` | Редактор Unity, кастомные окна, тулзы |
| `assets` | Импорт новых ассетов (текстуры, модели, звуки) |

**Без scope** можно, если изменение затрагивает много областей сразу:

```
feat: add main menu with settings and credits
```

## 📝 Subject

- Используйте **повелительное наклонение** (present tense): "add" вместо "added", "fix" вместо "fixes"
- **Не более 72 символов**
- **Без точки в конце**
- **С маленькой буквы** (в английском)
- На русском можно, но лучше на английском для международной команды

❌ Плохо: `Fixed bug where the player can't jump when crouching.`  
✅ Хорошо: `fix(player): allow jump from crouch state`

## 📄 Body (опционально)

- Пояснение **почему** сделано изменение, а не **что** (это видно из diff)
- Можно разбить на несколько строк
- Используйте пустую строку между subject и body

```
feat(combat): add critical hit system

Implement random crit chance based on player's luck stat.
Crit deals 2x damage and plays a special VFX.

Closes #42
```

## 🔚 Footer (опционально)

### Breaking changes
Для изменений, ломающих обратную совместимость:

```
feat(controls): migrate to new input system

BREAKING CHANGE: old Input Manager settings are no longer used.
All prefabs need to be updated to Input Actions.
```

### Закрытие issue

```
fix(ui): resolve health bar flickering

Closes #117, #119
```

---

## 🎮 Специфичные для геймдева рекомендации

### Коммитьте часто и атомарно
Один коммит = одна логическая единица работы.

✅ **Хорошо:**
```
feat(player): add dash ability
feat(player): add dash cooldown UI
fix(player): dash direction relative to camera
```

❌ **Плохо:**
```
feat(player): add dash, fix jump, refactor movement, update prefab
```

### Что коммитить в Unity

| Делайте коммит | Не коммитьте |
|----------------|---------------|
| `.cs` скрипты | `Library/` |
| `.unity` сцены | `Temp/` |
| `.prefab` префабы | `obj/` |
| `.asset` ассеты (если это данные) | `Builds/` |
| `.meta` файлы **обязательно** | `.userprefs` |
| Shaders (`.shader`) | `.csproj`, `.sln` |
| `.gitattributes` | логи, дампы |

**Золотое правило:** если изменился ассет — коммитьте вместе с его `.meta` файлом.

### Коммиты для LFS-файлов

При добавлении больших файлов (текстуры, модели, звуки) явно указывайте:

```
feat(assets): add environment textures (LFS)

- 4k skybox textures (2 files, ~45 MB each via LFS)
- ground materials with normal maps
```

---

### Шаблон сообщения коммита

Создайте `~/.gitcommit_template.txt`:

```
<type>(<scope>): <subject>

# Types: feat, fix, docs, style, refactor, perf, test, build, ci, chore
# Scopes: player, ui, combat, inventory, physics, audio, ai, save, netcode, scene, shaders, editor, assets
# Subject: present tense, no dot at end, max 72 chars
```

И настройте Git:

```bash
git config --global commit.template ~/.gitcommit_template.txt
```

---

## 📋 Чек-лист перед коммитом

- [ ] Собрал ли я проект без ошибок?
- [ ] Закоммитил ли я все `.meta` файлы вместе с ассетами?
- [ ] Не попали ли в коммит временные файлы (`Library/`, `Temp/`)?
- [ ] Использовал ли я правильный type?
- [ ] Кратко и понятно ли описал изменение в subject?
- [ ] Большие файлы (текстуры, модели, звуки) через LFS?

---

## 🚀 Примеры хороших коммитов для реальной игры

```
feat(combat): add headshot multiplier for ranged weapons

Headshots deal 3x damage and trigger a unique sound effect.
Currently only implemented for bow and rifle.

feat(ui): add XP bar with level-up animation

feat(save): implement autosave every 5 minutes

fix(player): prevent movement while inventory is open

perf(rendering): enable GPU instancing for grass prefabs

refactor(ai): extract patrol behavior into separate class

docs: update CONTRIBUTING.md with commit guidelines

chore: update Cinemachine from 2.9.5 to 2.9.7

revert: undo health regen mechanic (caused balance issues)

ci: add automated build for Windows and macOS on push to main
```

---

## ❓ Частые вопросы

**Q: Можно ли коммитить на русском?**  
A: Нет

**Q: Что делать, если я забыл LFS-файлы закоммитить через LFS?**  
A: Удалите их из истории, добавьте в `.gitattributes` и сделайте новый коммит. Используйте `git lfs migrate` если история уже запущена.

**Q: Нужно ли коммитить изменения в `.gitattributes`?**  
A: Да, и сразу же после добавления новых LFS-типов.

**Q: Как откатить плохой коммит?**  
A: `git revert <hash>` — создаст новый коммит с отменой изменений, не переписывая историю.

---

