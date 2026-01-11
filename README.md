시연 영상입니다.
https://youtu.be/jXrVMk5Fqaw
# Dodged (닷지드)

Unity 기반 3D 액션 게임 - 적응형 AI 보스 전투 시스템

## 프로젝트 개요

**장르**: 3D 액션 게임  
**플랫폼**: PC  
**개발 툴**: Unity  
**개발 기간**: 3개월

플레이어의 회피 패턴을 학습하여 2차 전투에서 맞춤형 난이도를 제공하는 보스 전투 게임입니다.

## 핵심 특징

### 1. 적응형 AI 시스템
- **1차 전투**: 플레이어의 회피 패턴, 취약한 공격 유형 데이터 수집
- **인터미션**: 수집된 데이터를 시각적으로 분석 및 표시
- **2차 전투**: 
  - 보스 체력 1.5배 증가
  - 플레이어가 취약한 공격 패턴 집중 사용
  - 선호 회피 방향의 십자 장판 범위 1.3배 확장

### 2. 전투 시스템

#### 보스 공격 패턴
- **근접 공격**: 애니메이션 이벤트 기반 무기 콜라이더 제어
- **십자 장판 공격**: 
  - 2초 예고 후 0.5초 데미지 판정
  - 회피 방향 추적 (좌우 4방향)
  - 적응형 범위 조정

#### 특수 공격 (2차 전투 전용)
- **잔상 공격**: 
  - 선호 방향 회피 시 발동
  - 보스 모델 3개 복제 (삼각형 배치)
  - 애니메이션 동기화 및 반투명 효과
  - 3개 원형 범위 동시 판정

- **슬로우모션 돌진**: 
  - 비선호 방향 회피 시 발동
  - 슬로우모션 중 플레이어 추적 돌진
  - 도달 후 즉시 근접 공격
  - Trail Renderer 시각 효과

### 3. 시각 효과 시스템

#### 피격 피드백
- **비네트 효과**: 피격 시 화면 가장자리 붉은 테두리
- **카메라 쉐이크**: Cinemachine Impulse Source 활용

#### 2차 전투 분위기 연출
- **Color Adjustment**: 어두운 톤, 채도 감소
- **Film Grain**: 거친 필름 질감
- **전용 Ground**: 1차/2차 전투별 다른 배경

#### 슬로우모션 효과
- **Chromatic Aberration**: 색수차 효과
- **Motion Blur**: 모션 블러
- **페이드 인/아웃**: 부드러운 전환

### 4. 튜토리얼 시스템
- 게임 조작 설명 UI
- 십자 공격 8회 회피 연습
- 공격 트리거 기반 자동 진행
- 연습 완료 후 실전 전투 진입

### 5. 슬로우모션 시스템
- **전역 매니저**: 싱글톤 패턴으로 통합 관리
- **플레이어 속도 조정**: 이동 속도 10%로 감소 (대시는 정상 속도 유지)
- **애니메이션 속도 동기화**: 슬로우모션 중 애니메이션도 느려짐

## 기술 스택

### Unity 시스템
- **Input System**: 새로운 Unity Input System 사용
- **Cinemachine**: 카메라 제어 및 쉐이크 효과
- **URP Post-Processing**: 
  - Vignette
  - Color Adjustments
  - Chromatic Aberration
  - Motion Blur
  - Film Grain

### 아키텍처
- **EventBus 패턴**: 느슨한 결합의 이벤트 관리
- **Static DataStore**: 전투 세션 데이터 영구 저장
- **Singleton 패턴**: 전역 매니저 (SlowMotionManager, EventBus)

### 최적화
- **Material Property Block**: 잔상 효과 최적화
- **오브젝트 풀링 미적용**: Unity Profiler 분석 결과 불필요 판정 (GC 12.1KB)

## 게임 플로우
메인 메뉴 → 튜토리얼 → 1차 전투 → 인터미션 (분석 결과) → 2차 전투 (적응형) → 엔딩

## 회피 시스템

### 판정 로직
1. 보스 공격 시작 시 플레이어 위치 저장
2. 공격 범위 내 여부로 위험 구역 판정
3. 공격 종료 시 피격 여부로 회피 성공/실패 기록
4. 십자 장판 회피 시 방향 계산 (좌우만 추적)

### 회피 방향 계산
- 십자 장판 회전 각도 고려
- 로컬 좌표계 변환
- 좌우 이동 성분 추출 (앞뒤 회피 제외)
- 가장 많이 사용한 방향을 선호 방향으로 저장

## 수집 데이터

### 1차 전투에서 수집
- 총 회피 시도 횟수
- 회피 성공/실패 횟수
- 근접 공격 피격률
- 십자 장판 피격률
- 선호 회피 방향 (좌/우)

### 2차 전투 적용
- 높은 피격률 패턴의 가중치 증가
- 선호 방향의 십자 장판 범위 확장
- 특수 공격 발동 조건으로 활용

## 개발 과정

### 주요 해결 과제

#### UI 초기화 문제
- **문제**: EventBus 구독 전 초기 상태 발행으로 체력바 미표시
- **해결**: DelayedInitialize 코루틴으로 한 프레임 지연 후 초기화

#### 애니메이션 동기화
- **문제**: 잔상 공격 시 보스 애니메이션 복제 필요
- **해결**: AnimatorStateInfo로 현재 상태 복사, speed=0으로 정지

#### 슬로우모션 적용 범위
- **문제**: 대시까지 느려지면 플레이어 불리
- **해결**: 이동만 속도 감소, 대시는 정상 속도 유지

## 조작법

- **WASD**: 이동
- **Space**: 점프
- **Shift**: 대시
- **좌클릭**: 공격

## 프로젝트 구조
Scripts/
├── Player/
│   ├── PlayerInputManager.cs
│   ├── PlayerMovement.cs
│   ├── PlayerHealth.cs
│   └── PlayerBodyRotation.cs
├── Boss/
│   ├── BossAI.cs
│   ├── BossAttackSystem.cs
│   ├── BossHealth.cs
│   ├── AfterimageAttackSystem.cs
│   └── SlowMotionChargeSystem.cs
├── Systems/
│   ├── DodgeSystem.cs
│   ├── DodgeCounter.cs
│   ├── AdaptiveDifficultySystem.cs
│   ├── CombatVisualSystem.cs
│   └── SlowMotionManager.cs
├── Data/
│   ├── CombatSessionDataStore.cs
│   └── CombatSessionEventBus.cs
├── UI/
│   ├── CombatUIManager.cs
│   ├── IntermissionUI.cs
│   ├── MainMenuManager.cs
│   └── EndingUI.cs
├── Tutorial/
│   ├── TutorialManager.cs
│   ├── TutorialAttackTrigger.cs
│   └── TutorialExitTrigger.cs
└── Core/
    ├── CombatManager.cs
    └── SceneTransitionManager.cs

## 개발 환경

- Unity 2022.3 LTS 이상
- Universal Render Pipeline (URP)
- Cinemachine 2.9.0 이상
- TextMeshPro

## 졸업 작품 목표

이 프로젝트는 다음을 증명하기 위해 제작되었습니다:
1. **데이터 기반 게임 디자인**: 플레이어 행동 분석 및 활용
2. **적응형 AI 구현**: 학습하는 보스 시스템
3. **시각적 피드백**: Post-Processing을 활용한 몰입감
4. **완성도 있는 게임 루프**: 튜토리얼부터 엔딩까지
