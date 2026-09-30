# Scene 87 — face generation record

All final face assets are stored in `Assets/Prefabs/Blocks/Scene87/FaceTextures/`.
The built-in `image_gen` editor was used; no CLI/API fallback was used. Original
Numberblock textures were inspected first and supplied as strict anatomy references.
The generated files were only resized to the exact required canvas and stripped of
erroneous low-alpha edge haze. Results containing a painted checkerboard were rejected.

| Final PNG | Strict source reference | Pixels | Expression |
|---|---|---:|---|
| `Fifty_Startled.png` | `Assets/Prefabs/Blocks/materials/50-face.png` | 1101 × 1056 | wide eyes looking down, open O mouth |
| `Hundred_Startled.png` | `Assets/Materials/11.png`; transparent anatomy reference `Assets/Prefabs/Blocks/Scene83/FaceTextures/Hundred_Surprised.png` | 956 × 956 | one square eye looking down, open O mouth |
| `OneFifty_Delighted.png` | the 50 source above plus the transparent 100 reference | 1536 × 1024 | hybrid star/square eyes, blue hat, joyful mouth |

## Final prompts

### Fifty_Startled.png

`Use case: precise-object-edit. Unity transparent facial-expression overlay. Image 1
is the strict original identity, anatomy, palette, proportions and layout reference for
Numberblock 50. Change only the expression to sudden startled fear at the instant of
tripping: two upright oval eyes opened wide, small pupils looking down and forward, and
a compact pink-rimmed open O mouth. Preserve the pale orange five-point star, muted
grey-green outline, dark indigo right-eye rim and large curved blue hat exactly. Genuine
RGBA transparency with alpha zero outside facial elements; no body, grid, glow, halo,
checkerboard, text or watermark.`

### Hundred_Startled.png

`Use case: precise-object-edit. Unity transparent facial-expression overlay. Preserve
exactly one dark-red rounded square eye rim, square white eye and one dark-red-rimmed open
O mouth from the validated transparent Numberblock 100 reference. Change only the pupil
to look down and slightly forward and give the mouth a slight anxious tilt. Genuine RGBA
transparency with alpha zero outside eye and mouth; no body, grid, glow, halo,
checkerboard, text or watermark.`

Two earlier attempts from the baked body texture produced a painted checkerboard and were
not imported.

### OneFifty_Delighted.png

`Use case: stylized-concept. Clean Unity transparent facial overlay for a new composite
Numberblock 150 on a 15-by-10 body. Use Numberblock 50 for the exact left star eye, curved
blue hat, pink mouth palette and flat 2D outlines; use Numberblock 100 for the exact right
square eye anatomy and dark-red palette. Create one delighted relieved face on a 3:2
canvas. Genuine RGBA transparency with alpha zero outside solid facial elements; no body,
grid, glow, halo, backdrop, checkerboard, number, text or watermark.`

An earlier composite inherited glow from an old reference and was rejected.

## Validation

All final PNGs are RGBA, have exact required dimensions, contain both visible and
zero-alpha pixels, and have four fully transparent corners. Contrast-background review
showed no checkerboard, rectangle or edge halo. Scene-local overlay materials use
transparent blending, `ZWrite = 0`, no shadows, clamp/bilinear import, no mipmaps and no
compression. The common source prefabs and materials are not modified.
