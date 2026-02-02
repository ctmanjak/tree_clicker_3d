---
title: feat: 벌목꾼 생산성 업그레이드
type: feat
date: 2026-02-02
---

# 벌목꾼 생산성 업그레이드

벌목꾼이 벌어오는 나무 획득량을 증가시키는 새로운 업그레이드 타입 추가.

## 핵심 발견

- `LumberjackController.cs:9` - `_woodPerSecond` 필드 존재 (기본값 1)
- `LumberjackController.cs:183` - `SetStats(woodPerSecond, animationSpeed)` 메서드 이미 있음
- `LumberjackSpawner.cs:12` - `_activeLumberjacks` HashSet으로 모든 벌목꾼 접근 가능
- `UpgradeManager.cs:85-96` - `RecalculateWoodPerClick()` 패턴 참조

## Acceptance Criteria

- [x] 새 UpgradeType `LumberjackProduction` 추가
- [x] 업그레이드 구매 시 모든 활성 벌목꾼의 생산량 증가
- [x] 새로 소환되는 벌목꾼에도 현재 생산량 적용
- [x] UI에 효과 텍스트 표시 (예: "+1/벌목꾼")

## 구현 계획

### 1. UpgradeData.cs

`UpgradeType` enum에 `LumberjackProduction` 추가.

```csharp
public enum UpgradeType
{
    WoodPerClick,
    SpawnLumberjack,
    LumberjackProduction  // 추가
}
```

### 2. UpgradeManager.cs

`WoodPerClick` 패턴을 따라 구현.

```csharp
// 필드 추가
private CurrencyValue _cachedLumberjackProduction = CurrencyValue.One;

// public 메서드 추가
public CurrencyValue GetLumberjackProduction() => _cachedLumberjackProduction;

// RecalculateLumberjackProduction() 메서드 추가
private void RecalculateLumberjackProduction()
{
    CurrencyValue total = CurrencyValue.One;

    foreach (var upgrade in GetUpgradesByType(UpgradeType.LumberjackProduction))
    {
        int level = GetLevel(upgrade);
        total += upgrade.EffectAmount * level;
    }

    _cachedLumberjackProduction = total;
}

// ApplyEffect()에 케이스 추가
case UpgradeType.LumberjackProduction:
    RecalculateLumberjackProduction();
    _lumberjackSpawner?.UpdateAllLumberjackStats(_cachedLumberjackProduction);
    break;
```

### 3. LumberjackSpawner.cs

모든 벌목꾼 스탯 업데이트 메서드 추가.

```csharp
public void UpdateAllLumberjackStats(CurrencyValue woodPerSecond)
{
    foreach (var lumberjack in _activeLumberjacks)
    {
        lumberjack.SetStats(woodPerSecond, 1f);
    }
}
```

스폰 시 현재 생산량 적용:

```csharp
// SpawnLumberjack() 수정
if (obj.TryGetComponent(out LumberjackController controller))
{
    _activeLumberjacks.Add(controller);

    // 현재 생산량 적용
    if (ServiceLocator.TryGet(out UpgradeManager upgradeManager))
    {
        controller.SetStats(upgradeManager.GetLumberjackProduction(), 1f);
    }
}
```

### 4. UpgradeButtonUI.cs

`GetEffectText()`에 케이스 추가.

```csharp
private string GetEffectText()
{
    return _upgradeData.Type switch
    {
        UpgradeType.WoodPerClick => $"+{_upgradeData.EffectAmount}/클릭",
        UpgradeType.SpawnLumberjack => "벌목꾼 +1",
        UpgradeType.LumberjackProduction => $"+{_upgradeData.EffectAmount}/벌목꾼",
        _ => ""
    };
}
```

### 5. ScriptableObject 에셋 생성

Unity Editor에서:
1. Project > Create > Lumberman > Upgrade
2. 설정:
   - UpgradeName: "벌목 효율"
   - Description: "벌목꾼의 나무 수확량 증가"
   - Type: LumberjackProduction
   - BaseCost: 500
   - CostMultiplier: 1.15
   - EffectAmount: 1
   - MaxLevel: 0 (무제한)

## 파일 변경 목록

| 파일 | 변경 내용 |
|------|----------|
| `UpgradeData.cs` | enum에 `LumberjackProduction` 추가 |
| `UpgradeManager.cs` | 캐시 필드, 계산 메서드, ApplyEffect 케이스 추가 |
| `LumberjackSpawner.cs` | `UpdateAllLumberjackStats()` 추가, 스폰 시 스탯 적용 |
| `UpgradeButtonUI.cs` | 효과 텍스트 케이스 추가 |

## References

- Brainstorm: `docs/brainstorms/2026-02-02-lumberjack-production-upgrade-brainstorm.md`
- 기존 패턴: `UpgradeManager.cs:85-96` (RecalculateWoodPerClick)
