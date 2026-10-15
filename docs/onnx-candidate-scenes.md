# ONNX 후보 테스트 씬

Unity 6000.0.32f1에서 생성·검증했다. `Assets/Scenes/ONNXCandidates`에 7개 씬이 있으며 ONNX 62개 파일을 63대 로봇에 연결했다. SQW04 한 모델은 다리 순서 두 가지를 비교하도록 두 번 배치했다. 각 로봇에는 별도의 16×16m 평평한 바닥과 목표 지점이 있다. 공통 카메라, 조명, 모델 이름/규격 표시, 선택 및 리셋 기능을 추가했다.

| 씬 | 로봇 수 | 규격 | 비고 |
|---|---:|---|---|
| [01_ForcedQuad_113_18](../Assets/Scenes/ONNXCandidates/01_ForcedQuad_113_18.unity) | 4 | 113→18 | Forced / FS_02 원본 물리 프리팹 |
| [02_Bipedal_191_22](../Assets/Scenes/ONNXCandidates/02_Bipedal_191_22.unity) | 7 | 191→22 | 4L / 512 원본 물리 프리팹 |
| [03_LegacyBipedal_175_20](../Assets/Scenes/ONNXCandidates/03_LegacyBipedal_175_20.unity) | 35 | 175→20 | 옛 관절 리스트 순서와 후보군별 프리팹 보존 |
| [04_LegacyBipedal_158_21](../Assets/Scenes/ONNXCandidates/04_LegacyBipedal_158_21.unity) | 13 | 158→21 | 실험용: 10관절 대체 프리팹에 옛 forceUsage 매핑 적용 |
| [05_LegacyQuad_126_32](../Assets/Scenes/ONNXCandidates/05_LegacyQuad_126_32.unity) | 2 | 126→32 | 실험용: 기존 사족 프리팹의 발 관절 4개를 revolute로 복원 |
| [06_SQW04_Order1234_113_32](../Assets/Scenes/ONNXCandidates/06_SQW04_Order1234_113_32.unity) | 1 | 113→32 | 부분 매칭: 다리 순서 1,2,3,4 |
| [07_SQW04_Order4123_113_32](../Assets/Scenes/ONNXCandidates/07_SQW04_Order4123_113_32.unity) | 1 | 113→32 | 부분 매칭: 다리 순서 4,1,2,3 |

## 실행

씬을 열고 Play를 누르면 첫 모델을 가까이 보여준다. 왼쪽 상단에서 모델을 선택하거나 `Overview`로 전체 배치를 확인한다.

- `Tab` / Next / Previous: 다른 모델 선택 및 카메라 이동.
- `F` / Focus: 선택한 로봇으로 카메라 이동.
- `R` / Reset all: 모든 로봇 초기화.
- 마우스 오른쪽 버튼 + WASD: 자유 카메라 이동. Q/E는 높이, Shift는 빠르게 이동.
- Pause/Resume: 일시 정지/재개.
- Target speed 슬라이더: 전체 로봇의 목표 속도 변경. 기본 2m/s.

로봇이 넘어지거나 바닥 밖으로 이동하면 에피소드를 다시 시작한다. 기본 결정 주기는 5 fixed steps이며 행동은 결정 사이에도 적용한다. Behavior Parameters는 Inference Only, CPU Burst, deterministic inference로 설정했다. 재학습 서버 없이 실행할 수 있다.

목표에 수평 거리 0.75m 이내로 도착하면 로봇은 계속 움직이고 목표만 새 무작위 위치로 이동한다. 새 목표는 각 바닥 중심의 반경 6m 안에 생성하며, 로봇에서 최소 2m 떨어뜨리고 목표 높이를 유지한다. 상단의 `Targets reached`에서 도착 횟수를 확인할 수 있다. 모든 이족·사족 후보 씬에 공통 적용된다.

## 검증 결과

7개 씬 모두 Play Mode에서 각 로봇의 행동 콜백 100회 이상을 확인했다. 모든 모델의 관측 개수가 설정값과 일치했고, 행동 출력과 검사한 물리 좌표가 유한했으며 런타임 오류가 없었다. [검증 결과](onnx-scene-validation.json), [씬별 모델 배치 목록](onnx-candidate-scenes.json), [미리보기](scene-previews/01_ForcedQuad_113_18.png)를 저장했다.

이는 초기 추론과 물리 실행 검사다. 안정적인 보행이나 목표 추종 성능을 인증하지 않는다. 특히158/21은 학습 당시 로봇 물리를 확보하지 못한 대체 구성이며,126/32의 발 관절 한계는 실험용으로 ±90도를 사용했다. SQW04는 역사 코드처럼 앞24행동만 사용하며 나머지8개는 사용하지 않는다. 화면에도 해당 제한을 표시한다.

## 구현과 재생성

- `Assets/CandidateGallery/Runtime/CandidateRobotAgent.cs`: 이전/현재 관측 순서와 행동 매핑을 담은 전용 추론 어댑터.
- `Assets/CandidateGallery/Runtime/CandidateGalleryControls.cs`: 선택·카메라·리셋·속도 조절.
- `Assets/CandidateGallery/Editor/CandidateSceneBuilder.cs`: Unity API로 씬 재생성. 에디터 메뉴 `Robot ML > Build ONNX candidate scenes`.
- `Assets/CandidateGallery/Editor/CandidateSceneValidation.cs`: 실제 Play Mode 검증 및 카메라 렌더 미리보기 생성.

원본 로봇 프리팹과 기존 게임 스크립트는 변경하지 않았다. 씬 안의 복사본에서 학습용 스크립트를 제거하고 전용 어댑터를 연결했다. 패키지 및 누락된 ProjectSettings는 복구했고7개 씬을 Build Settings에 등록했다. Unity의 최초 임포트는 기존 URP 머티리얼 업그레이드와 추적된 Library 캐시 재생성을 동반할 수 있다.

복구된 패키지: ML-Agents3.0.0, Sentis2.1.0, Input System1.11.2, uGUI2.0.0, URP17.0.3, Visual Scripting1.9.5. 패키지 목록은 임시 Unity 프로젝트에서 공식 PackageManager Client API로 해결한 결과를 복사했다. 임시 부트스트랩 프로젝트와 실행 로그/캐시는 `.gitignore`로 제외했다.
