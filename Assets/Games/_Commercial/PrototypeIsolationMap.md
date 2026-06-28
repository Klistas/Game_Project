# Prototype Isolation Map

2026-06-28 기준 세 프로토타입은 씬과 에셋 경로를 분리한다.

## Body Rebels

- Scene: `Assets/Games/BodyRebels/Scenes/BodyRebelsPrototype.unity`
- Script: `Assets/Games/BodyRebels/Scripts/BodyRebelsPrototype.cs`
- Runtime sprites: `Assets/Games/BodyRebels/Resources/BodyRebelsSprites`
- AI prompts: `Assets/Games/BodyRebels/Art/AIAssetPrompts.md`
- Editor menu: `Game Prototypes/Open Scene/Body Rebels`

## Everyone Innocent

- Scene: `Assets/Games/EveryoneInnocent/Scenes/EveryoneInnocentPrototype.unity`
- Script: `Assets/Games/EveryoneInnocent/Scripts/EveryoneInnocentPrototype.cs`
- Runtime sprites: `Assets/Games/EveryoneInnocent/Resources/EveryoneInnocentSprites`
- AI prompts: `Assets/Games/EveryoneInnocent/Art/AIAssetPrompts.md`
- Editor menu: `Game Prototypes/Open Scene/Everyone Innocent`

## Intended Feature

- Scene: `Assets/Games/IntendedFeature/Scenes/IntendedFeaturePrototype.unity`
- Script: `Assets/Games/IntendedFeature/Scripts/IntendedFeaturePrototype.cs`
- Runtime sprites: `Assets/Games/IntendedFeature/Resources/IntendedFeatureSprites`
- AI prompts: `Assets/Games/IntendedFeature/Art/AIAssetPrompts.md`
- Editor menu: `Game Prototypes/Open Scene/Intended Feature`

## Isolation Rule

각 독립 씬은 자기 프로토타입 컴포넌트가 붙은 `*_RuntimeRoot` 하나와, 다른 프로토타입의 자동 부트를 막기 위한 빈 `*_RuntimeRoot`를 함께 가진다. 이렇게 하면 `PrototypeRuntimeDefaults`나 PlayerPrefs가 다른 게임을 가리켜도 현재 씬의 게임만 실행된다.

새 이미지 에셋은 반드시 해당 게임의 `Resources/<Game>Sprites` 폴더에만 넣는다. 같은 역할의 이미지라도 게임 간 공유 폴더에 두지 않는다.
