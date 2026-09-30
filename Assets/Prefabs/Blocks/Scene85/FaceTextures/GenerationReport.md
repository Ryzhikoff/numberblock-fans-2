# Scene 85 — face generation record

All final face assets are in `Assets/Prefabs/Blocks/Scene85/FaceTextures/`.

Method: built-in `image_gen` image editing with original texture references, followed only by mechanical bilinear resizing to the exact original PNG canvas and removal of alpha below 0.015. Results with a painted checkerboard were rejected and re-edited with background removal. No CLI/API fallback was used.

Original happy expressions remain on the original baked front geometry; the original transparent faces of 50 and 50,000 are switched directly. All new smug/scared layers are RGBA transparent overlays. 500,000 uses the anatomy of the original 500 face on its top 100,000 section.

| Final PNG | Original texture reference | Pixels | Method |
|---|---|---|---|
| Ten_Smug.png | Assets/Prefabs/Blocks/materials/tenFace.png | 154 × 379 | scene-local copy of existing expression |
| Ten_Scared.png | Assets/Prefabs/Blocks/materials/tenFace.png | 154 × 379 | scene-local copy of existing expression |
| Fifty_Smug.png | Assets/Prefabs/Blocks/materials/50-face.png | 1101 × 1056 | built-in image editing |
| Fifty_Scared.png | Assets/Prefabs/Blocks/materials/50-face.png | 1101 × 1056 | scene-local copy of existing expression |
| Hundred_Smug.png | Assets/Materials/11.png | 956 × 956 | built-in image editing |
| Hundred_Scared.png | Assets/Materials/11.png | 956 × 956 | scene-local copy of existing expression |
| FiveHundred_Smug.png | Assets/Prefabs/Blocks/materials/500-face.png | 521 × 521 | built-in image editing |
| FiveHundred_Scared.png | Assets/Prefabs/Blocks/materials/500-face.png | 521 × 521 | scene-local copy of existing expression |
| FiveHundred_Happy.png | Assets/Prefabs/Blocks/materials/500-face.png | 521 × 521 | built-in image editing |
| Thousand_Smug.png | Assets/Materials/oneThousandFace.png | 380 × 380 | built-in image editing |
| Thousand_Scared.png | Assets/Materials/oneThousandFace.png | 380 × 380 | built-in image editing |
| TenThousand_Smug.png | Assets/Prefabs/Blocks/materials/10000-face.png | 2018 × 5028 | built-in image editing |
| TenThousand_Scared.png | Assets/Prefabs/Blocks/materials/10000-face.png | 2018 × 5028 | built-in image editing |
| HundredThousand_Smug.png | Assets/Prefabs/Blocks/materials/1k-face.png | 3709 × 3709 | built-in image editing |
| HundredThousand_Scared.png | Assets/Prefabs/Blocks/materials/1k-face.png | 3709 × 3709 | built-in image editing |
| Million_Smug.png | Assets/Materials/oneMillionFaceTexture.png | 853 × 776 | built-in image editing |

Ten_Smug is an unused completeness layer copied from Scene83 Ten_Determined: the first character never attacks anyone. Million's scared layer is unused: the film ends with its original smile.

Final creative prompts and any final background-removal prompts:

## Fifty_Smug.png

Use case: precise-object-edit. Asset type: Unity Numberblocks transparent facial expression overlay. Input image 1 is the original character face texture and strict identity and layout reference. Create exactly ONE finished expression for this character: a mischievous smug smirk, eyes glancing toward the left at a smaller rival, subtle lowered upper eyelid(s), an asymmetric pleased grin. Exactly two upright oval eyes: the left eye has a pale orange five-point star around it with a muted grey green outline. The right eye has a dark indigo oval rim. Preserve the large curved blue hat above the eyes, exactly its original shape, colour and location. The pink mouth remains in the lower centre. Remove all body colours, all body grid lines and the original background. Return a genuinely transparent RGBA PNG: alpha MUST be exactly zero everywhere outside the eyes, rims, star/hat if present and mouth; no glow, no red residue, no shadow, no haze, no checkerboard, no background or body or words. Flat 2D texture style matching the reference, no 3D lighting or gradients added. Canvas should have aspect and layout of 1101 x 1056 pixels; preserve relative positions and dimensions of facial elements and original transparent margins. One image contains one face expression.

Final alpha correction: Use case: background-extraction. Edit the attached face expression. Remove the entire grey checkerboard BACKGROUND, including ALL small squares and all wrinkles, from this image. Output MUST be a genuine transparent RGBA PNG with a REAL alpha channel: pixel alpha equals ZERO between and outside the hat, star, eye outlines and mouth. Do NOT draw any checkerboard or black/white/grey background. Preserve the two oval eyes, orange five-point star around left eye, curved blue hat, pink mouth, smug expression, exactly their current shapes, positions and proportions. One facial expression, no body, no text, no glow, no halo. Transparent canvas aspect ratio1101:1056.

## Hundred_Smug.png

Use case: precise-object-edit. Asset type: Unity Numberblocks transparent facial expression overlay. Input image 1 is the original character face texture and strict identity and layout reference. Create exactly ONE finished expression for this character: a mischievous smug smirk, eyes glancing toward the left at a smaller rival, subtle lowered upper eyelid(s), an asymmetric pleased grin. Exactly ONE square eye with a dark red thick square rounded-corner border and square white area, ONE black rounded pupil, and a dark red smiling mouth below it. Keep original eye bounding box x=295..687 y=155..566 and mouth bounding box approximately x=315..665 y=620..798 on the 956-square canvas. Remove all body colours, all body grid lines and the original background. Return a genuinely transparent RGBA PNG: alpha MUST be exactly zero everywhere outside the eyes, rims, star/hat if present and mouth; no glow, no red residue, no shadow, no haze, no checkerboard, no background or body or words. Flat 2D texture style matching the reference, no 3D lighting or gradients added. Canvas should have aspect and layout of 956 x 956 pixels; preserve relative positions and dimensions of facial elements and original transparent margins. One image contains one face expression.

## Thousand_Smug.png

Use case: precise-object-edit. Asset type: Unity Numberblocks transparent facial expression overlay. Input image 1 is the original character face texture and strict identity and layout reference. Create exactly ONE finished expression for this character: a mischievous smug smirk, eyes glancing toward the left at a smaller rival, subtle lowered upper eyelid(s), an asymmetric pleased grin. Exactly ONE circular eye with the original brown-red round rim, white round eye, black pupil, and brown-red mouth below it. Keep original eye bbox x=105..294 y=46..246 and mouth x=105..256 y=250..333 on the 380-square canvas. Remove all body colours, all body grid lines and the original background. Return a genuinely transparent RGBA PNG: alpha MUST be exactly zero everywhere outside the eyes, rims, star/hat if present and mouth; no glow, no red residue, no shadow, no haze, no checkerboard, no background or body or words. Flat 2D texture style matching the reference, no 3D lighting or gradients added. Canvas should have aspect and layout of 380 x 380 pixels; preserve relative positions and dimensions of facial elements and original transparent margins. One image contains one face expression.

## Thousand_Scared.png

Edit this EXACT Numberblock1000 original face to STARTLED SCARED and cut out the face on a real transparent background. Preserve exactly ONE ROUND eye with original BROWN RED circular rim, round white eyeball and one small black pupil looking UP, bounding boxx=105..294 y=46..246 of a380square canvas. Below the eye, inside original mouth area x=105..256 y=250..333, make a small brown red-rimmed open O mouth. Match flat2D original shapes colours and outlines. REMOVE all red body colour and every gridline. REAL RGBA PNG with alphaZERO outside round eye and mouth, empty transparent corners. Original380x380 aspect and facial proportions. One expression, no background, no glow or shadows or words.

Final alpha correction: Remove the background from this image and make it transparent. Preserve the face unchanged, in exactly the same position, on the same canvas.

## TenThousand_Smug.png

Use case: precise-object-edit. Create a transparent facial overlay edited from this EXACT Numberblock10000 original front texture. Keep TWO upright oval eyes surrounded by TWO red five-point stars. Keep these stars and eyes at the SAME normalized locations: left star bboxx=.10.. .51 y=.075.. .27, right starx=.48.. .89 y=.075.. .27. Pink smiling mouth at x=.42.. .69 y=.285.. .38. Character's face occupies only the UPPER38% of a very tall narrow canvas; the lower62% MUST remain fully transparent and EMPTY. Change only expression to mischievous smug, glancing left with slightly lowered lids and an asymmetric pleased grin. Remove ALL body colour, white background, red border and EVERY gridline. Flat2D reference style. Canvas aspect2018:5028. REAL transparent RGBA PNG, alpha EXACTLY ZERO outside the stars eyes and mouth; NO drawn checkerboard, no background, no halo, no gradients, no words. One complete face expression.

Final alpha correction: Remove the entire grey chequered background and its wrinkles from this image. Keep ONLY the two red stars with oval white eyes and pupils, and the pink smiling mouth. Deliver a REAL transparent RGBA PNG with pixel alpha ZERO for the whole empty lower62%, all corners and EVERY space outside the facial features. Preserve the features exactly, including current smug expression, colours, sizes and locations. Preserve the tall canvas and its aspect2018:5028. Do not draw any pattern or background colour. One facial overlay.

## TenThousand_Scared.png

Use case: precise-object-edit. Create a genuinely TRANSPARENT facial expression overlay from this EXACT original Numberblock10000 texture. Character has TWO upright oval eyes surrounded by TWO red five-point stars. Preserve those stars at same locations: left star bboxx=.10.. .51 y=.075.. .27, right starx=.48.. .89 y=.075.. .27; pink mouth at x=.42.. .69 y=.285.. .38. Change only expression to startled fear: eyes wide, small pupils looking up, open pink-rimmed O-shaped mouth. All face features remain only in upper38% of tall canvas; lower62% EMPTY and alpha ZERO. Remove all white body, red perimeter and all gridlines. Match original flat2D colours and outlines. One expression, canvas aspect2018:5028, REAL RGBA PNG alphaZERO outside facial features, NO drawn checkerboard, NO background, NO body, no glow or halo or words.

Final alpha correction: Background removal only. Erase every grey square from the attached image. Keep the red star shapes, white eyes, pupils and pink mouth exactly unchanged. Save on a transparent background, as RGBA PNG with an actual empty alpha channel outside the face. All grey grid patterns must be gone. Do not draw a new background. Preserve the whole tall canvas and face positions.

## HundredThousand_Smug.png

Edit this original Numberblock100000 face texture into ONE smug mischievous facial expression on a REAL TRANSPARENT BACKGROUND. Preserve the ONE SQUARE eye with thick dark red rounded square rim and square white area at original bounding box x=.339.. .715 y=.201.. .596, and mouth x=.359.. .692 y=.65.. .82 on a square canvas. Lower its upper eyelid slightly, look LEFT with one black oval pupil, smile with an asymmetric dark red smirk and the two white tooth bars of the original. Retain the original flat2D style, shapes, colours, positions and proportions. Extract only facial features: eye, eyelid, outline, pupil, mouth. REMOVE ALL red body colour and EVERY grid line. Output RGBA PNG with pixel alpha exactly0 everywhere outside the facial features, fully transparent empty corners and spaces between eye and mouth. No drawn transparency pattern, no background, no glow, no shadows, no text. One expression, square canvas corresponding to original3709x3709.

## HundredThousand_Scared.png

Edit this EXACT original Numberblock100000 front texture into ONE STARTLED SCARED face expression on a GENUINELY transparent background. EXACTLY ONE square eye: preserve its dark red rounded SQUARE thick rim and square white region at x=.339.. .715 y=.201.. .596, with ONE small black oval pupil looking UP. Open dark red-rimmed O-shaped mouth inside original mouth area x=.359.. .692 y=.65.. .82. Match original flat2D colours and contours, unchanged feature positions. REMOVE all red body colour and every gridline. Deliver REAL RGBA PNG alpha0 everywhere outside eye and mouth, square canvas original3709x3709 aspect, no background of any colour, no patterns, no glow or shadows, no text. One complete character expression.

Final alpha correction: Remove the background from this image and make it transparent. Preserve the face unchanged, in exactly the same position, on the same canvas.

## Million_Smug.png

Edit this original Numberblock1000000 face texture into ONE mischievous smug smirk on a GENUINELY TRANSPARENT background. Preserve exactly ONE broad rounded-square white eye inside the dark red rounded-square thick rim at x=.261.. .733 y=.058.. .611, and one black oval pupil. Keep mouth x=.37.. .687 y=.682.. .989 with dark red rim and original white tooth bars. Lower upper eyelid slightly and glance LEFT, with an asymmetric pleased grin. Original flat2D texture style and colours; original feature positions and proportions. REMOVE all red body and EVERY gridline. REAL RGBA PNG, pixel alpha0 everywhere outside facial features. Empty transparent corners and space between eye and mouth. Canvas aspect853:776. No background, no patterns, no glow or shadow, no text. One complete expression.

Final alpha correction: Remove the background from this image and make it transparent. Preserve the face unchanged, in exactly the same position, on the same canvas.

## FiveHundred_Happy.png

Edit the FIRST transparent face image: change only the shocked pink mouth into a pleased happy pink smiling mouth, matching the smile in the SECOND reference image. Keep the two oval eyes, dark blue five-point star around the left eye and dark blue right-eye rim completely unchanged, exactly in place. Preserve the transparent background, actual zero-alpha pixels everywhere outside face elements. Output one face on a square transparent RGBA PNG canvas. No body and no background.

Final alpha correction: Remove the background from this image and make it transparent. Preserve the face unchanged, in exactly the same position, on the same canvas.

## FiveHundred_Smug.png

Edit the FIRST transparent facial overlay to a mischievous smug smirk. Keep the eye and star shapes and positions exactly unchanged. Preserve the dark blue FIVE-POINT STAR around the LEFT oval eye: one tip points UP, two tips extend sideways, two tips at lower left and lower right; match the star orientation in the SECOND original reference. The RIGHT eye retains the dark blue oval rim. Lower the upper eyelids gently, pupils look LEFT toward a smaller rival, and change the pink open mouth to a crooked pleased smiling grin with black interior and a small white tooth highlight. Retain the original flat 2D texture style and palette, no 3D lighting. Keep the genuinely transparent background and actual zero-alpha pixels outside facial features. One expression on a square RGBA PNG, corresponding to the source521x521 layout.

Final alpha correction: Remove the background from this image and make it transparent. Preserve the face unchanged, in exactly the same position, on the same canvas.

Existing expressions: Ten_Determined / Ten_Surprised, Fifty_Surprised, Hundred_Surprised and FiveHundred_Shocked from Scene83. All are copied to Scene85 and kept on their original corresponding face-plane transforms. FiveHundred_Smug was regenerated against the original 500 face so its star points upwards.

Validation: all 16 output PNGs passed genuine-RGBA, visible-feature, transparent-background and four-zero-alpha-corner checks. Exact dimensions, importer configuration, source planes, transparent blending and unchanged shared prefab/material hashes are checked by S85_SceneSetup.ValidateScene. Full-film Play Mode results are recorded in Scene_85_README.md.
