# Repository Guidelines

## Project Structure & Module Organization

This Unity 6 workspace produces animated Numberblocks videos; it is not a single application. Each file in `Assets/Scenes/` is an independent video project with its own camera, objects, timing, and behavior. Shared building blocks live in `Assets/Prefabs/`, `Assets/Materials/`, `Assets/Sound/`, `Assets/Resources/`, `Assets/Font/`, and `Assets/models/`. Scripts are in `Assets/Scripts/`; `S<number>_` files normally belong to one scene, while generic scripts are shared. Editor setup tools belong in `Assets/Scripts/Editor/`. Read `PROJECT_DOCUMENTATION.md` before changing an established scene.

## Build, Test, and Development Commands

- Open the project with Unity Hub using Unity `6000.4.3f1`, or run `Unity -projectPath .` when the editor executable is on `PATH`.
- Validate script compilation with `Unity -batchmode -quit -projectPath "$PWD" -logFile -`.

Final output is normally captured or rendered from the relevant scene in Play Mode; an application build is not the primary deliverable. Do not commit generated `Library/`, `Temp/`, `Logs/`, recordings, or render output unless explicitly requested.

## Coding Style & Naming Conventions

Use four spaces for C# indentation and one type per file. Use PascalCase for classes, methods, and public members; camelCase for locals and private fields. Keep `MonoBehaviour` filenames identical to class names. Name controllers `S<scene>_Main.cs`, supporting components `S<scene>_<Role>.cs`, and scenes `Scene_<number>.unity`. Preserve legacy names instead of casually renaming serialized assets.

Always commit the `.meta` file accompanying a new or moved Unity asset. Move assets inside Unity when possible so GUID references remain intact. Do not assume a scene change should affect other scenes; place reusable assets in shared folders, but keep one-off controllers and setup scripts scoped to their scene.

## Numberblocks Face Assets

Whenever a task creates, edits, replaces, or validates a Numberblock face or facial-expression texture, read and follow `.agents/skills/numberblocks-face-assets/SKILL.md` before inspecting or changing the related assets. Preserve each original character's anatomy and use genuine transparent PNG overlays; never infer a generic eye layout when an original prefab or texture is available.

## Testing Guidelines

No first-party automated tests are committed. Confirm a clean compile, then open and play every affected scene. Check missing references, Console errors, camera framing, animation timing, audio synchronization, prefab appearance, and the complete video sequence. For shared-resource changes, spot-check representative consuming scenes. Tests for reusable logic may go under `Assets/Tests/EditMode/` or `Assets/Tests/PlayMode/`.

## Commit & Pull Request Guidelines

Recent commits use short, imperative summaries such as `Add Numberblocks 11-99 character prefabs`. Keep each commit focused and include related assets and `.meta` files together. Pull requests should name the affected scene numbers, distinguish scene-local changes from shared-resource changes, summarize validation, and include screenshots or a short capture for visual or animation work. Call out package, rendering-pipeline, or `ProjectSettings/` changes explicitly.
