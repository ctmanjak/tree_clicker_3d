# Brainstorm: 벌목꾼 생산성 업그레이드

**Date**: 2026-02-02
**Status**: Approved

## What We're Building

벌목꾼이 벌어오는 나무 획득량을 증가시키는 새로운 업그레이드 타입.

업그레이드하면 모든 벌목꾼이 나무를 벨 때 획득하는 나무량이 증가한다.

## Why This Approach

### 선택한 방법: 벌목꾼당 생산량 증가

**이유:**
- 기존 `WoodPerClick` 패턴과 일관성 유지
- 구현 복잡도 낮음
- 업그레이드 효과가 직관적으로 이해됨

### 고려했던 대안들

| 방법 | 설명 | 기각 이유 |
|------|------|----------|
| 작업 속도 증가 | 벌목 주기 단축 | 애니메이션 동기화 복잡 |
| 전체 보너스 배수 | % 배수 적용 | 현재 시스템에 퍼센트 계산 없음 |

## Key Decisions

1. **새 UpgradeType 추가**: `LumberjackProduction`
2. **효과 계산 방식**: 가산형 (base + upgrade effect * level)
3. **적용 대상**: 모든 벌목꾼에 동일하게 적용
4. **비용 재화**: Wood (기존 업그레이드와 동일)

## Implementation Points

1. `UpgradeData.cs` - `UpgradeType` enum에 `LumberjackProduction` 추가
2. `UpgradeManager.cs` - `ApplyEffect()`에 케이스 추가, 생산량 계산 메서드 추가
3. 벌목꾼 스크립트 - `UpgradeManager`에서 생산량 값 조회
4. `UpgradeButtonUI.cs` - 효과 텍스트 포맷 추가

## Open Questions

- 벌목꾼 기본 생산량은 얼마인가? → `_woodPerSecond = CurrencyValue.One` (1)
- 업그레이드 아이템 이름/아이콘은? → "벌목 효율", 아이콘 추후 결정

## Next Steps

`/workflows:plan` 실행하여 상세 구현 계획 수립
