# Face Contour Geometric Consistency: Research Plan

## Goal

Make the game character card's frontal face contour geometrically consistent
with the original JPG's FACE_OVAL from MediaPipe.

---

## Core Concept: Contour Width Profile

Define a geometric quantity x that is computed identically on both sides:

    Given a closed 2D contour (FACE_OVAL or game face edge),
    at normalized height h in [0, 1] (0=chin, 1=forehead):
    x(h) = w(h) / face_h
    where w(h) = horizontal span of contour at height y(h)

- Source (photo): compute from MediaPipe FACE_OVAL 36 points
- Game (screenshot): run same MediaPipe, compute from same FACE_OVAL 36 points
- Both sides use identical definition and algorithm = geometric consistency

---

## Why We Don't Need Mesh Extraction

The audit doc says we need ShapeAnime to get precise x definitions.
But we have a more practical path: use the game itself as oracle.

1. Set slider value -> write card -> game loads -> take frontal screenshot
2. Run MediaPipe on screenshot -> get FACE_OVAL 36 points
3. Compute contour_width_profile with same function as source

This gives us:
- Identical x definition on both sides (same function, same FACE_OVAL, same algorithm)
- No need to know game internals (mesh/bones/skinning)
- Only need: game can run, take screenshots, MediaPipe can detect face

---

## Implementation Phases

### Phase 1: Implement Contour Width Profile

1. Write contour_width_profile(face_oval_xy, K=20) function
2. Integrate into extract_face_ratios.py
3. Test on existing source images

### Phase 2: Empirical Slider Sweep

For each of the 13 2D-visible contour sliders (0,2,4,5,6,8,9,10,11,13,15,16,18):
- Sweep from -100 to 200, step 30 (or finer)
- Each value: write card -> screenshot -> MediaPipe -> contour_profile
- Output: slider_value -> profile JSON tables

### Phase 3: Contour Optimizer

- Given source profile, find slider combo minimizing profile difference
- Loss = sum of (profile_source[k] - profile_game[k])^2
- Can use existing Optuna/one-dim framework

### Phase 4: End-to-End Verification

Source JPG -> profile(source) -> optimize -> best sliders -> card -> screenshot -> profile(game) -> compare

---

## Slider Classification

| Category | Indices | Strategy |
|----------|---------|----------|
| 2D visible, affects width | 0, 4, 5, 10, 15, 18 | Sweep + width optimization |
| 2D visible, affects height | 2, 6, 9, 11, 13, 16 | Same, height features from profile |
| 2D visible, angle | 8 | From chin segment slope |
| Depth (never possible) | 1, 3, 7, 12, 14, 17 | from_card, don't touch |

---

## Relationship to Existing System

- Does NOT replace existing 16 ratio -> 16 slider mapping for features (eyes, nose, mouth)
- ADDS contour-based alignment for face edge sliders
- Final 59 shapeValueFace = features(from_landmarks) + contour(from_profile) + depth(from_card)
