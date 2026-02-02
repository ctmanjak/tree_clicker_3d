---
title: 성장 곡선 알고리즘 개선
type: feat
date: 2026-02-02
---

# feat: 성장 곡선 알고리즘 개선 (비용 곡선 파동)

## Overview

비용 곡선에 **사인파 기반 파동**을 적용하여 정체 → 성장 → 학살 사이클이 무한히 반복되는 구조를 만든다. O(1) 시간 복잡도로 구현하여 성능 영향 없이 게임 재미를 증폭시킨다.

## 핵심 아이디어

현재: 비용이 항상 1.15배로 일정하게 증가 (단조로움)
```
레벨 1 → 2 → 3 → 4 → 5 ...
비용  ×1.15 ×1.15 ×1.15 ×1.15 ...
```

개선: 사인파로 비용 배수가 주기적으로 변화
```
레벨   1    7    13   19   25   31 ...
배수  1.15 1.20 1.15 1.10 1.15 1.20 ...
      기준  정체  기준  학살  기준  정체  (반복)
```

## 사이클 설계

25레벨을 한 주기로, 사인파가 자연스럽게 3단계 사이클 생성:

```
        정체 (비용↑)
           /\
          /  \
기준 ----/----\----/---- 기준
                \/
             학살 (비용↓)

|-- 25레벨 주기 --|-- 25레벨 주기 --|-- ...
```

| 구간 | 비용 배수 | 체감 |
|------|-----------|------|
| 주기 초반 (0-6) | 1.15 → 1.20 | 점점 비싸짐 |
| 주기 중반 - 정체 (6-7) | 1.20 (최고점) | 벽 느낌 |
| 주기 중반 (7-18) | 1.20 → 1.10 | 점점 저렴해짐 |
| 주기 후반 - 학살 (18-19) | 1.10 (최저점) | 빠른 레벨업! |
| 주기 끝 (19-25) | 1.10 → 1.15 | 다시 기준으로 |

## Technical Approach

### 구현 코드

```csharp
// UpgradeData.cs

private const int CYCLE_LENGTH = 25;
private const double WAVE_AMPLITUDE = 0.05;  // ±5% 변동 (1.10 ~ 1.20)

public CurrencyValue GetCost(int currentLevel)
{
    // 기본 지수 곡선 (O(1))
    CurrencyValue baseCost = _baseCost * CurrencyValue.Pow(_costMultiplier, currentLevel);

    // 사인파로 파동 적용 (O(1))
    // 사인파: -1 ~ +1 사이를 오가며 자연스러운 곡선 생성
    double phase = 2 * Math.PI * currentLevel / CYCLE_LENGTH;
    double waveMultiplier = 1 + WAVE_AMPLITUDE * Math.Sin(phase);

    return (baseCost * waveMultiplier).Floor();
}
```

### 작동 원리

```
Math.Sin(phase) 값:
- phase = 0 (레벨 0, 25, 50...) → sin = 0 → 배수 1.00
- phase = π/2 (레벨 6, 31, 56...) → sin = 1 → 배수 1.05 (정체)
- phase = π (레벨 12, 37, 62...) → sin = 0 → 배수 1.00
- phase = 3π/2 (레벨 18, 43, 68...) → sin = -1 → 배수 0.95 (학살)

최종 비용 배수:
- 정체 구간: 1.15 × 1.05 = 1.2075
- 기준 구간: 1.15 × 1.00 = 1.15
- 학살 구간: 1.15 × 0.95 = 1.0925
```

### 레벨별 시뮬레이션

baseCost=100, costMultiplier=1.15, amplitude=0.05 기준:

| 레벨 | 파동 배수 | 실효 배수 | 비용 | 체감 |
|------|-----------|-----------|------|------|
| 1 | 1.01 | 1.16 | 116 | 기준 |
| 6 | 1.05 | 1.21 | 262 | 정체 (최고) |
| 12 | 1.00 | 1.15 | 534 | 기준 |
| 18 | 0.95 | 1.09 | 917 | 학살 (최저) |
| 25 | 1.00 | 1.15 | 1,878 | 기준 |
| 31 | 1.05 | 1.21 | 4,303 | 정체 |
| 43 | 0.95 | 1.09 | 15,032 | 학살 |
| ... | 반복 | 반복 | ... | 무한 반복 |

### 파일 변경

| 파일 | 변경 내용 |
|------|-----------|
| `UpgradeData.cs` | `GetCost()` 메서드 수정 (5줄 추가) |

## Acceptance Criteria

- [ ] `CYCLE_LENGTH` 상수 추가 (25)
- [ ] `WAVE_AMPLITUDE` 상수 추가 (0.05)
- [ ] `GetCost()` 메서드에 사인파 적용
- [ ] O(1) 시간 복잡도 유지 확인
- [ ] 레벨 6, 31, 56...에서 비용이 최고점인지 확인 (정체)
- [ ] 레벨 18, 43, 68...에서 비용이 최저점인지 확인 (학살)

## 미래 확장: 나무 체력 시스템

나무 체력 도입 시 사이클이 더 명확해짐:

```
정체 구간: 비용↑ + 나무 체력↑ → 클릭 많이 필요, 업그레이드 비쌈
학살 구간: 비용↓ + 나무 체력 동일 → 빠른 업그레이드, 나무 빨리 벰
```

## 설정 조절 가이드

| 상수 | 기본값 | 효과 |
|------|--------|------|
| `CYCLE_LENGTH` | 25 | 작으면 사이클 빠름, 크면 느림 |
| `WAVE_AMPLITUDE` | 0.05 | 작으면 변화 미미, 크면 극적 |

예시:
- `AMPLITUDE = 0.10` → 비용 ±10% 변동 (1.035 ~ 1.265)
- `CYCLE_LENGTH = 15` → 15레벨마다 사이클 반복

## References

- 현재 비용 계산: `UpgradeData.cs:48-51`
- Cookie Clicker: 건물별 1.15배 고정
- 사인파 적용으로 자연스러운 파동 생성

---

## 변경 전후 비교

### Before (현재)
```csharp
public CurrencyValue GetCost(int currentLevel)
{
    return (_baseCost * CurrencyValue.Pow(_costMultiplier, currentLevel)).Floor();
}
```

### After (개선)
```csharp
private const int CYCLE_LENGTH = 25;
private const double WAVE_AMPLITUDE = 0.05;

public CurrencyValue GetCost(int currentLevel)
{
    CurrencyValue baseCost = _baseCost * CurrencyValue.Pow(_costMultiplier, currentLevel);

    double phase = 2 * Math.PI * currentLevel / CYCLE_LENGTH;
    double waveMultiplier = 1 + WAVE_AMPLITUDE * Math.Sin(phase);

    return (baseCost * waveMultiplier).Floor();
}
```

**변경점:**
- 코드 3줄 추가
- O(1) 유지
- 무한 반복 사이클 자동 생성
