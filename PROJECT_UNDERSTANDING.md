# GreatPioneer - 프로젝트 이해도 문서

## 📊 게임 개요

**GreatPioneer**는 **Lane 기반 턴-연속 타워 디펜스 게임**입니다.
- **3개의 Lane**: 플레이어가 생산한 엔티티가 각 Lane을 통해 적 본진(HQ)으로 진군
- **경제 시스템**: 엔티티 소환에 Gold 필요, 적 엔티티 처치 시 Gold 수익
- **실시간 게임**: 턴 기반이 아닌 연속 시간 진행, 양쪽 팀이 동시에 액션

---

## 🏗️ 전체 게임 구조

```
GreatPioneer (게임)
├── GameManager (전역 게임 상태)
│   ├── GameField (게임 필드)
│   │   ├── PlayerHq (플레이어 본진)
│   │   │   ├── EntitySpawner[0] (Lane 0)
│   │   │   ├── EntitySpawner[1] (Lane 1)
│   │   │   └── EntitySpawner[2] (Lane 2)
│   │   │
│   │   └── EnemyHq (적 본진)
│   │       ├── EntitySpawner[0] (Lane 0)
│   │       ├── EntitySpawner[1] (Lane 1)
│   │       └── EntitySpawner[2] (Lane 2)
│   │
│   └── AIScheduleHandler (적 AI 스케줄)
│       └── AIScheduleInfo (설정 데이터)
│
├── UI 시스템
│   ├── HUD (항상 표시)
│   ├── UIHqManagement (팝업)
│   │   ├── UIHqLeftPanel (좌측 정보)
│   │   └── UIHqRightPanel (네비게이션)
│   │       ├── UIHqPanelMenuSelect (메뉴)
│   │       ├── UIHqPanelLaneSelect (레인 선택)
│   │       ├── UIHqPanelSlotSelect (슬롯 선택)
│   │       └── UIHqPanelEntitySelect (엔티티 선택)
│   └── UIResult (결과 화면)
│
└── Managers (중앙 집중식 매니저)
    ├── DataManager (모든 설정 데이터)
    ├── UIManager (팝업 관리)
    ├── PoolManager (오브젝트 풀)
    ├── SaveManager (저장/로드)
    └── 기타 (String, Language, Sound 등)
```

---

## 🎮 게임 진행 흐름

### 1️⃣ 게임 시작
```
GameManager.Init()
    ↓
GameField.Init()
    ↓
CreateHqs() → Player/Enemy HQ 생성
    ↓
CreateSpawners() → 각 HQ마다 3개 EntitySpawner 생성
    ↓
GameManager.StartAIScheduleHandler()
```

### 2️⃣ 게임 진행 (루프)
```
Update() [매 프레임]
    ├── AIScheduleHandler.Update()
    │   ├── HandleTpSupply() → 주기적으로 적 TP 충전
    │   ├── HandleBurst() → 큰 파도 공격 실행
    │   └── SpendTp() → 강제 스폰 명령 (ForceSpawn)
    │
    └── GameField / EntitySpawner / AEntity
        ├── EntitySpawner.CoStartSpawn() → 자동 생산 (플레이어 Lane)
        │   └── 충분한 Gold/Mineral 대기
        │       → 생산 시간 경과
        │       → Spawn()
        │
        └── AEntity.Update() → 모든 엔티티 액션
            ├── DoAction()
            │   ├── 공격 범위 스캔
            │   ├── 타겟 선택 (거리 가깝고, 체력 낮은 순)
            │   ├── Attack()
            │   └── 공격 대상 없으면 Move()
            │
            └── CheckArrival()
                └── 적 본진 도달 시
                    ├── 피해 입음
                    ├── 소환한 HQ에 Gold 수익
                    └── 엔티티 제거
```

### 3️⃣ 게임 종료
```
HeadQuater.OnHqDamaged()
    ↓
HP ≤ 0?
    ↓ YES
GameManager.EndStage(isPlayerWin)
    ↓
UIResult 팝업 표시
    ↓
결과 저장 (UserRecord)
    ↓
Restart / Exit
```

---

## 👥 핵심 클래스 역할

### HeadQuater (본진)
**역할**: 게임의 중심 경제/상태 관리
```csharp
// 상태
- HP: 체력 (0이 되면 패배)
- Shield: 방어막 (피해 흡수)
- Gold: 경제자원 (주기적 증가)
- Mineral: 광물 (특수 사용)
- Level: 레벨 (미사용, 레벨업 시스템용)

// 주요 기능
- Init(): 초기화 (Team, 스포너 위치 설정)
- CreateSpawners(): 3개 EntitySpawner 생성
- OnHqDamaged(): 피해 처리
- EarnGold/ConsumeGold: 경제 관리
```

**중요**: HeadQuater는 EntitySpawner들의 부모 컨테이너이며, 
콜백(`_earnGold`, `_consumeGold` 등)을 통해 EntitySpawner와 통신합니다.

### EntitySpawner (레인별 소환기)
**역할**: 특정 Lane에서 엔티티 자동/강제 소환

```csharp
// 구조
- Lane: 0, 1, 2 (3개)
- Slot: 각 Lane마다 N개 슬롯 (슬롯 = 비용 할당 유닛)
- Coroutine: 슬롯별 생산 코루틴

// 생산 흐름
Slot.ChangeTarget(EntityID) 
    → OnSlotTargetChanged()
    → StartSpawn()
    → CoStartSpawn() [코루틴]
        ├── Gold 모음
        ├── 생산 시간 경과
        └── Spawn(EntityInfo)

// 강제 스폰 (AI)
ForceSpawn(List<EntityInfo>)
    → CoForceSpawn()
    → 1초 간격으로 순차 스폰
```

**중요**: 
- 플레이어: UI로 슬롯 설정 → 자동 생산
- 적: AIScheduleHandler가 `ForceSpawn()` 호출

### AEntity (엔티티/유닛)
**역할**: 개별 유닛 (전투, 이동, 생명주기)

```csharp
// 상태
- Status: HP, Shield, Armor, Attack, AttackSpeed, MoveSpeed 등
- Action: Move / Combat
- Team: Player / Enemy

// 행동 (매 프레임)
1. DoAction()
   ├── Raycast 스캔 (공격 범위 내 적군)
   ├── 타겟 선택 (가장 가깝고 체력 낮은 적)
   ├── 공격 범위 안 → Attack()
   └── 공격 범위 밖 → Move()

2. Attack()
   ├── 공격력 계산 (크리 판정)
   └── 대상.GetDamage()

3. GetDamage()
   ├── Armor 감소 적용
   ├── Shield 흡수 (먼저 차감)
   ├── HP 차감
   └── HP ≤ 0 → Destroy()

4. CheckArrival()
   └── 적 본진 도달 → 피해 입히고 제거
```

**중요**: 
- 2D Raycast를 사용하여 실시간 타겟 스캔
- 5초마다 재타게팅 (일정 거리 유지)
- 아머는 피해 감소 공식: `damage * (100 / (100 + armor))`

### AIScheduleHandler (적 AI)
**역할**: 적의 자동 스폰 스케줄 관리

```csharp
// 리소스: TP (Tactical Point)
_accumulatedTp: 누적된 TP

// 주기적 이벤트
1. HandleTpSupply() [tpInterval 주기]
   ├── tpAmountCurve 평가 (시간 기반)
   ├── spendRate 결정 (지출 vs 저축)
   └── 지출: SpendTp(), 저축: 누적

2. HandleBurst() [burstInterval 주기]
   └── 누적된 모든 TP 일괄 사용

3. Emergency() [플레이어 본진 체력 < threshold]
   └── 대량 스폰 (emergencyTpMultiplier 배수)

// SpendTp() 로직
foreach Lane (3개):
    ├── 가능한 한 많은 엔티티 선택
    │   (TP 남으면 계속 선택)
    └── ForceSpawn(Lane, EntityList)
```

**중요**: 
- 모든 값은 AnimationCurve로 시간 기반 설정
- TP는 엔티티 소환의 자원 (Gold처럼)
- AI는 Lane별로 균등하게 분배

### UserRecord (저장 데이터)
**역할**: 플레이어 진행률 저장

```csharp
_stageBestRecordDict<int, (tick, clear, clearTime, hqHpRatio)>
    └── 스테이지별 최고 기록

// 별 3개 조건
1. Clear: 승리
2. ClearInTime: 12분 이내 승리
3. ClearHqHp: 본진 체력 100% 유지
```

---

## 📁 파일 구조 (핵심)

```
Assets/Scripts/
├── Field/
│   ├── GameField.cs (필드 생성/관리)
│   └── HeadQuater.cs (본진)
│
├── Spawner/
│   ├── EntitySpawner.cs (Lane별 스포너)
│   └── EntitySpawnSlot.cs (생산 슬롯)
│
├── Characters/
│   └── AEntity.cs (엔티티 기본 클래스)
│
├── Data/
│   ├── APrefabData.cs (기본 데이터 패턴)
│   ├── HeadQuaterData.cs (본진 설정)
│   ├── EntityData.cs (엔티티 설정)
│   ├── AIScheduleData.cs (AI 스케줄 설정)
│   └── PlayerCurrencyData.cs (경제 설정)
│
├── UI/
│   ├── Hq/
│   │   ├── UIHqManagement.cs (본진 UI 팝업)
│   │   ├── UIHqRightPanel.cs (네비게이션)
│   │   ├── AUIHqPanelSelect.cs (패널 기본 클래스)
│   │   ├── UIHqPanelMenuSelect.cs (메뉴)
│   │   ├── UIHqPanelLaneSelect.cs (레인 선택)
│   │   ├── UIHqPanelEntitySelect.cs (엔티티 선택)
│   │   └── UISlotUnit.cs (슬롯 UI 표현)
│   │
│   ├── Result/
│   │   └── UIResult.cs (게임 결과)
│   │
│   └── HUDController.cs (상시 표시 HUD)
│
├── Managers/
│   ├── Managers.cs (싱글톤 진입점)
│   ├── GameManager.cs (게임 상태)
│   ├── DataManager.cs (데이터 로딩)
│   ├── UIManager.cs (UI 관리)
│   └── 기타 (Pool, Save, String, Language, Sound)
│
└── Etc/
    ├── AIScheduleHandler.cs (AI 스케줄 처리)
    ├── UserRecord.cs (저장 데이터)
    ├── PrefabID.cs (모든 리소스 ID)
    └── StringID.cs (다국어 텍스트 ID)
```

---

## 🔄 주요 데이터 흐름

### 1️⃣ 엔티티 생산 흐름 (플레이어)

```
[UI] UIHqPanelEntitySelect.OnEntitySelected()
    ↓
[Slot] EntitySpawnSlot.ChangeTarget(EntityID)
    ↓
[Spawner] EntitySpawner.OnSlotTargetChanged()
    ↓
[Spawner] EntitySpawner.StartSpawn() → CoStartSpawn() [코루틴 시작]
    ├── DataManager에서 EntityInfo 조회
    ├── Gold/Mineral 대기 (조건: >= 필요량)
    ├── 소비: HeadQuater.ConsumeGold/Mineral() 호출
    ├── 생산 시간 대기
    └── Spawn() → AEntity 생성 및 초기화
        ├── PoolManager.Instantiate(EntityID)
        ├── entity.Init(..., _earnGold) ← 콜백
        └── EntitySpawner._entityDict에 추가
```

**중요**: 
- 소비는 코루틴 내에서 발생 (즉시 X, 금액 충분 후)
- 콜백 `_earnGold`는 엔티티가 적 본진 도달 시 호출됨

### 2️⃣ 전투 흐름

```
[PlayerEntity] 적 범위 내 이동
    ↓
[PlayerEntity] DoAction() → Raycast 스캔
    ↓
[PlayerEntity] SelectTarget() → 적군 선택
    ↓
[PlayerEntity] Attack(Target)
    ├── 공격력 계산 (크리)
    └── Target.GetDamage(damage, attacker)
        ├── Armor 감소 적용
        ├── Shield 흡수
        ├── HP 차감
        └── 사망 시: attacker.OnKill(reward)
            └── _earnGold(reward) 호출 ← 처치 금액 수익
```

**중요**: 
- 처치 금액 = 소환 비용의 25% (REWARD_RATIO)
- 아머 공식: `damage * (100 / (100 + armor))`

### 3️⃣ 경제 흐름

```
[HQ] CoEarnGoldPerSecond() [주기적 1초마다]
    → HeadQuater._gold += goldPerSecond

[EntitySpawner] 엔티티 생산 시
    → HeadQuater.ConsumeGold(goldCost)

[AEntity] 적 처치 시
    → HeadQuater.EarnGold(reward = cost * 0.25)

[AEntity] 적 본진 도달 시
    → 소환한 HQ에 EarnGold(reward)
```

**설정 위치**:
- `StartGold`: PlayerCurrencyData
- `GoldPerSecond`: PlayerCurrencyData
- `EntityCost`: EntityInfo.goldCost
- `EnemyTP`: AIScheduleInfo.tpAmountCurve

---

## 🎯 게임 상태 전이

```
[Init]
    ↓
[Playing]
    ├── Update() [매 프레임]
    └── GameManager.IsInGame = true
    ↓
[GameOver 체크]
    └── AEntity.CheckArrival() 또는 OnHqDamaged()
        → HeadQuater.Hp ≤ 0?
            ↓ YES
        [EndStage]
        ├── UIResult 팝업
        ├── UserRecord 저장
        └── 선택:
            ├── [Restart] → GameField.Restart()
            └── [Exit] → GameManager.ExitStage()
```

---

## 🔐 중요 설계 원칙

### 1. 데이터 분리
- **설정 데이터**: ScriptableObject (Editor에서 수정)
- **런타임 상태**: 클래스 필드 (코드에서만 접근)
- DataManager가 모든 설정 데이터 캐싱

### 2. 콜백 기반 통신
- HeadQuater ← EntitySpawner: 경제 콜백
  ```csharp
  spawner.Init(..., earnGold, earnMineral, consumeGold, ...)
  ```
- AEntity → HeadQuater: 보상 콜백
  ```csharp
  entity.Init(..., onDie, onKill)
  ```

### 3. 1회용 리소스 풀
- PoolManager로 인스턴스 재사용
- 생성: `PoolManager.Instantiate(PrefabID)`
- 제거: `PoolManager.Destroy(obj, PrefabID)`

### 4. UI는 읽기만 (상태 변경 X)
- UI는 상태 조회 후 표시
- 상태 변경은 게임 로직에서만

### 5. 시간 기반 스케줄
- AnimationCurve로 시간 값 매핑
- AIScheduleHandler: 플레이 시간 기반

---

## ⚙️ 주요 설정 관점

### 경제 밸런싱
- **PlayerCurrencyData**:
  - `startGold`: 게임 시작 금액
  - `goldPerSecond`: 초당 수익

- **EntityInfo**:
  - `goldCost`: 소환 비용
  - `productionTime`: 생산 시간
  
- **AIScheduleInfo**:
  - `tpAmountCurve(시간)`: 적의 시간별 리소스
  - `spendRateCurve(시간)`: 적의 지출 비율

### 난이도 곡선
- **AIScheduleInfo**:
  - `tpInterval`: TP 충전 주기 (짧을수록 빈번)
  - `burstInterval`: 파도 공격 주기
  - `levelCurve(시간)`: 시간에 따른 적 레벨
  - `emergencyHpThreshold`: 발악 모드 시작 체력

### 전투 밸런싱
- **EntityInfo**:
  - `attack`: 공격력
  - `attackSpeed`: 공격 속도
  - `armor`: 방어력
  - `criticalChance`: 크리 확률 (0~1)
  - `hp`, `shield`: 생명력

---

## 🚀 게임 성능 고려사항

### 1. Raycast 최적화
- AEntity.DoAction()에서 2D Raycast 사용
- 최대 50개 결과 버퍼 (DEFAULT_RAYCAST_COUNT)
- 5초마다 재타겟팅 (계산 감소)

### 2. 코루틴 관리
- EntitySpawner: 슬롯당 1개 코루틴
- HeadQuater: 금수익 코루틴 1개
- AIScheduleHandler: 일반 Update 루프

### 3. 객체 풀링
- PoolManager로 모든 동적 객체 재사용
- 생성/제거 성능 개선

---

## 🔍 디버깅 포인트

**자주 확인할 값**:
- `HeadQuater.Hp / _maxHp`: 본진 체력
- `HeadQuater.Gold / Mineral`: 경제 상태
- `EntitySpawner._slotList[].GetProgress()`: 생산 진행률
- `AIScheduleHandler._accumulatedTp`: 적 누적 TP
- `AEntity._attackTarget`: 현재 타겟
- `GameManager.PlayTime`: 경과 시간

**로그 추가 권장**:
- `OnHqDamaged()`: 피해량 및 남은 체력
- `EntitySpawner.CoStartSpawn()`: 생산 시작/완료
- `AIScheduleHandler.SpendTp()`: 적 스폰 목록
- `AEntity.Attack()`: 크리 발생

---

이 문서는 GreatPioneer의 **실제 게임 메커니즘**을 반영하며,
새로운 기능 추가 시 이 흐름을 이해하고 있어야 합니다.

