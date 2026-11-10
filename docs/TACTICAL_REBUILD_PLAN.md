# Tactical Strike — Full gameplay and battlefield rebuild

## Direction

The current procedural Greenridge scene is a prototype only. It is not the accepted art direction and should not be polished indefinitely through random colour, tree-count or box-placement changes.

The project remains an original Unity 6.3 LTS / URP Android-first tactical infantry FPS. Squad is a reference for the quality of combined-arms battlefield readability, map composition, weapon handling, sound, cover use and player feedback. We must not copy Squad source, proprietary models, sounds, maps or branding.

Design priority is **deliberate long-range infantry combat, strong terrain identity, believable prepared positions, readable weapons and convincing audio-visual feedback**.

## Battlefield concept — first map

Working name: **Northwind Valley / Sector 01**.

World footprint: approximately 700 m × 700 m. The first 500–700 m engagement lanes must be interrupted by real terrain, woodland edges, buildings and prepared cover rather than uniformly open ground or a flat obstacle course.

Environment palette: temperate rural highland with natural muted olive vegetation, dry grass, exposed brown earth, weathered stone and darker woodland. Avoid saturated bright-green carpeting and avoid sand-dune desert styling. Use layered materials with deliberate transitions between grass, soil, rock, mud, road, trench spoil and damaged surfaces.

### Tactical layout

- Attacker staging area on the southern edge with a protected assembly position, a shallow forward trench and multiple approach choices.
- Central valley engagement lane with alternating low knolls, shallow drainage cuts and irregular ridgelines. It should offer selected long-range sightlines while preventing the entire map from being visible from one firing position.
- Western flank: broken shoulder terrain, sparse woodland bands, a concealed foot track, drainage gully, two covered firing pockets and a route that can outflank the main trench without becoming a free safe lane.
- Eastern flank: exposed service road, rockier high ground, a shallow stream/ditch crossing and a village approach bounded by compound walls, trees and rubble.
- Defender main line in the northern third: a continuous but irregular fighting system, not a single sinusoidal wall. Include firing bays, alternating traverse bays, short zig-zags, communication trenches, dugouts, reserve positions and protected exits.
- Two separated defensive compounds/houses with real door/window openings, enterable interiors, interior cover, roof variation, courtyard walls and surrounding utility structures.
- Secondary village/farm cluster with damaged outbuildings, tracks, fences, sheds, water tanks, rubble, wreckage and sightline breaks.
- Crew-served DShK/M2 positions belong on designed mounts with traverse arcs and protected crew access; do not present them as handheld rifles.
- Every route and piece of cover must be intentional: it should protect movement, create a firing angle, break line of sight or establish a recognizable objective.

## Modular world-production rules

1. Design the battlefield in plan view before placing final assets.
2. Sculpt the height field intentionally: long ridge chains, saddles, shallow gullies, sloped approaches and local micro-terrain.
3. Use modular trench and fortification segments (straight sections, elbows, T-junctions, revetments, steps, dugout entrances, fire bays) with varied heights and spacing.
4. Use genuine building kits with functional openings/interiors instead of sealed primitive boxes.
5. Use a multilayer terrain shader and authored decals for tire marks, disturbed soil, mud, rubble, road edges and blast scars.
6. Use curated props and foliage clusters. Do not scatter every prop with a single uniform random distribution.
7. Use mobile LODs, occlusion/culling, GPU instancing, baked/static lighting where practical, and distance-based foliage density.
8. Preserve a consistent scale system and document meters, player height, weapon scale, trench cover height and door dimensions.

## Combat system architecture

Replace the monolithic hard-coded AK controller with composable systems:

- WeaponDefinition ScriptableObject per weapon: identity, class, caliber label, fire modes, magazine/reserve, cadence, reload timing, recoil/sway parameters, ADS offsets, projectile profile, audio profile, VFX profile, viewmodel prefab and animation references.
- Shared WeaponController for inventory, equipping, fire/reload state, ammo, input routing and animation events.
- Per-weapon first-person rig anchors: right-hand grip, left-hand support, stock/shoulder, optic/iron-sight alignment and muzzle. Validate every animation pose rather than fitting by overall bounds alone.
- Per-weapon handling: draw/idle/walk/run/sprint/ADS/fire/reload animations, controlled recoil spring, viewmodel sway and weapon-specific sound.
- Initial roster: AK-74M, M4A1, PKM, RPG-7, SVD/Dragunov, DShK 12.7 mm and M2 Browning .50. The DShK and M2 are mounted crew-served weapons.
- Do not expose a weapon as completed until its mesh, hands, animation, textures, sound, ammo, ADS, recoil, selection and test checklist all pass.

## Projectile, impact and material response

- Separate projectile simulation from presentation.
- Tracer presentation must have an identifiable hot leading point, a compact luminous body, a continuous controlled trail and an observable decay; never rely on a pink rectangle or a hairline-only LineRenderer.
- Caliber and ammunition mode select distinct visual intensity and effect profiles.
- Use explicit surface profiles for dirt, grass, wood, stone, brick, concrete and metal.
- Terrain hits emphasize dust and disturbed earth; masonry adds chips/debris; metal adds directional sparks and ricochet audio; heavy impacts use wider debris/dust feedback.
- Penetration must alter projectile continuation only when the material/profile supports it; otherwise stop or ricochet with a clear result.
- Destructible cover should have progressive damage states and debris rather than disappearing as a single intact object.

## Player feel and sighting

- Maintain one reliable character movement model across desktop preview and Android touch controls.
- Add walk, sprint, crouch, prone and lean states only with corresponding camera, capsule, animation and weapon effects.
- ADS must align the actual iron sight/optic to a defined camera sight point. FOV reduction alone is not ADS.
- Use controlled recoil with smooth recovery, small lateral variation, weapon spring, movement sway, stance differences and stationary focus.
- Add per-weapon movement speed, aim time, reload time, recoil pattern and viewmodel settings.
- Do not tune from code guesses alone: use the browser preview to capture and compare standard screenshots.

## Audio direction

Build an AudioMixer-based layered sound system:
- Weapon-specific close report and distant tail.
- Mechanical handling, draw, magazine, bolt/charging and reload sounds.
- Directional bullet crack / pass-by and material-specific ricochet.
- Surface-specific footfalls for grass, soil, mud, stone, wood and interior floors.
- Indoor/outdoor reverb/occlusion and distance attenuation.
- Environmental beds: wind, birds/insects as appropriate, distant battle ambience and weather.
Use correctly licensed original, CC0 or explicitly redistributable assets and document provenance.

## Tactical AI and match structure

After player, map and weapon foundations are stable, build enemy squads with readable states: hold, observe, suppress, reposition, reload-under-cover, advance, fallback and regroup. AI should score cover by threat/exposure, avoid crossing open ground without reason, respond to gunfire/nearby impacts and maintain formation spacing. Use NavMesh/waypoint links and cover points authored for the map rather than random rushing.

## Preview and acceptance

The browser preview is the user's continuous design review environment. It must expose keyboard/mouse controls, display the current commit/build ID, and allow inspection of the player view, map, weapon and hit effects. Future work should include capture evidence where automated rendering is reliable.

Acceptance gates:
- WebGL: build/import, required compressed payloads and loader validation, deployed site availability, browser console/load check, interactive movement and pointer-lock test.
- Art review: map screenshots from fixed standard viewpoints, weapon/hands screenshot, ADS screenshot, tracer flight and surface-impact screenshot.
- Android: APK signature and structure, installation, 30/60 FPS performance target appropriate to hardware, touch-control and audio review on a physical phone.
- No feature is complete because CI only compiled successfully.

## Delivery order

1. Ship and validate the browser review path.
2. Establish the new top-down map layout and get visual feedback before rebuilding geometry.
3. Replace the battlefield greybox with modular terrain, trench, village and cover systems.
4. Rebuild the player viewmodel/ADS foundation and prove the AK-74M framing.
5. Build the shared weapon architecture and complete the agreed seven-weapon roster in testable increments.
6. Replace tracer, impact and audio placeholders with dedicated data-driven profiles.
7. Build movement depth and tactical AI.
8. Optimize Android and add Wi-Fi/hotspot multiplayer only after single-player is stable.

This order deliberately avoids adding many unfinished features at once. Each milestone should create something visible in the browser preview and retain an independently verifiable build.

## Actual implementation status — audited 2026-11-10

**Important: this document is a design specification, not evidence that the described map or combat systems are already implemented in the playable Unity scene.**

- The playable scene still starts through `UnityProject/Assets/TacticalStrike/Scripts/Core/TacticalStrikeBootstrap.cs`, which calls `BattlefieldGenerator.Build(worldRoot)`.
- The currently active `BattlefieldGenerator.cs` remains the prior procedural Greenridge/plateau implementation. It has not yet been replaced by the Northwind Valley authored layout described above.
- `docs/northwind-valley-layout.svg` is a top-down conceptual plan only. It is not imported into Unity as a terrain mesh, level, or playable scene.
- The current weapon implementation remains the AK-74M `WeaponController` and its existing viewmodel setup. The seven-weapon roster above is a target specification, not a completed inventory.
- The successful WebGL deployment run `42836781579` built this existing gameplay scene. Its purpose was to establish the browser build/deploy pipeline; it did not execute the battlefield or weapon redesign.
- Therefore the first deployed preview is a technical pipeline milestone only. It must not be labelled as the rebuilt game or as proof that the Northwind Valley map is in the scene.

### Required next implementation milestone

Replace the active procedural battlefield with the actual playable Northwind Valley world: create the large-scale terrain and valley sightlines, the branching northern defender trench network, the southern attacker staging and forward position, distinct western/eastern flanks, two enterable defensive compounds, the village/farm cluster, and the designed mounted heavy-MG positions. Integrate the world into the bootstrap so the browser preview and Android build use the same new scene/world implementation. Do not close this milestone based on the SVG or successful compilation alone; inspect the deployed browser build and confirm the real geometry in screenshots/video.

## Implementation status update — 2026-11-10

The playable Unity source implementation has now started; the prior section describing the gap between design and implementation remains historical context. The active `TacticalStrikeBootstrap` now calls `NorthwindValleyGenerator.Build`, not the old Greenridge generator. The 700 m x 700 m terrain mesh, two branched trench networks and earth floors/parapets, dugouts, two defensive compounds with real door/window openings and interiors, additional rural buildings, service roads, drainage channels, static heavy-MG nests, woodland belts and other battlefield props are implemented in the runtime world generator. The player now has smooth movement, stamina, crouch/prone, stationary focus and first-pass camera/weapon recoil/sway/spread integration. The AK-74M remains the only integrated first-person weapon. This milestone awaits Unity CI and actual browser/device visual verification.

The following are explicitly still unfinished: visual-quality PBR layer blending and architectural detail pass; functioning DShK/M2 interaction; the remaining six weapon models/rigs/animations/audio/behavior; AI and objectives; layered distant audio; complete caliber/surface-specific impact/penetration/destruction systems; lean/vaulting; mobile performance and device acceptance. Do not close this milestone solely because the code compiles or the conceptual SVG exists.

## First PBR art pass and integration repair — 2026-11-10

The first new world generator now has an initial PBR surface pass in source: 1K CC0 ambientCG maps are prepared for grass, field earth, brick, concrete and compacted roads, then applied to the corresponding terrain/building/trench/road materials. This replaces the earlier single-tinted grass-only terrain and flat-colour primitive surfaces at the material level. It is not yet a complete multi-layer slope/height-blended terrain shader; that remains a visual art milestone.

A compile error exposed the missing `AttachHud(MobileHud)` bridge after rewriting the player controller. The method and HUD reference have been restored; both WebGL and Android compile/build checks are being rerun from the current source. No new browser preview is considered usable until the WebGL compile, payload validation and Pages deployment all pass.

## World build milestone — new-map deployment confirmed — 2026-11-10

The first actual Northwind Valley Unity source was successfully compiled for Android and successfully deployed as WebGL. WebGL run `42852093866` deployed commit `8c4e6516abd2585a0084788359e2e3bc11382f73`; Android run `42852093702` compiled and verified the matching ARM64 APK. This is a playable-scene source milestone, not final art polish or evidence of physical-device acceptance. The map includes the active 700m terrain, connected attacker/defender trench networks, courtyards and enterable building openings/interiors, roads, drainage, woodland, two heavy-machine-gun nests, and initial PBR grass/earth/brick/concrete/road surface materials.

The follow-up source commit `30cc3a967330c362a5cde873cbc59beeded47a8d` adds functional first-pass DShK/M2 mount interaction: E or contextual touch button to mount, pivot-driven ±66° traverse / +24° elevation / -18° depression, camera/viewmodel transition, 12.7mm ballistics, 250-round belts + 750 reserve and 3.35s reload. The HMG geometry is procedural and still needs authentic mesh/texture/sound/animation refinement. The follow-up WebGL and Android CI runs `42855335693` and `42855335569` are pending validation.

## Current terrain and suppression implementation pass — 2026-11-10

The map terrain has moved beyond the prior one-material mesh: commit `1adc16e0b9ec84a62fb560dbab44595e4fec092f` builds a native Unity 700m TerrainData/Collider and blends grass, earth and rocky gravel layers using slope/elevation splat weights. Trench cuts and water drainage align to the terrain heightfield. PBR terrain appearance and phone performance still require visual/device testing.

A first-pass near-miss suppression system is also in source: unblocked projectile passes close to infantry increase suppression; the factor decays, affects viewmodel sway and shot spread, and prevents breath-hold focus temporarily. A light red edge vignette provides feedback without central blur. It is not yet AI suppression or multiplayer-authoritative suppression.

The immediately preceding CI runs for the crew-served weapons failed because the HMG pedestal referenced the undefined material name `darkMetal` rather than `metalMaterial`. This reference has been corrected. WebGL run `42857326872` and Android run `42857326877` are building the corrected layered-terrain/suppression source; this milestone is not complete until both validate and WebGL deployment succeeds.

