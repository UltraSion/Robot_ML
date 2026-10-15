# Robot_ML · 물리 기반 보행 학습

- **개발 기간:** 2025.01 ~ 2025.08
- **사용 기술:** Unity, C#, ML-Agents, 물리 기반 관절 제어
- **구현 범위:** 프로젝트 설계 및 구현 전반

## 이족 보행

AI가 관절을 직접 제어하는 방식과, 삼각함수로 만든 기본 동작을 AI가 보조하는 방식을 시도했습니다.

### AI 관절 제어

관절별 힘과 목표 각도를 AI가 출력해 보행을 학습합니다.

https://github.com/user-attachments/assets/01a5eda0-b6df-4255-9db5-0326ca65b995

### 삼각함수 기반 동작 + AI 보조

삼각함수로 다리의 기본 움직임을 만들고, AI가 그 움직임을 보조하도록 구성했습니다.

https://github.com/user-attachments/assets/ca71e81f-0e5e-4719-b27f-9a1cc304687f

## 사족 보행

대칭형 몸체에서 다리의 역할과 관절 출력 구조를 바꾸며 보행을 학습했습니다.

### 관절별 독립 제어

다리 순서를 재배치하지 않고, 각 관절의 힘과 목표 각도를 독립적으로 출력합니다.

https://github.com/user-attachments/assets/adc75e24-d0dd-4ac6-8810-3dd11440df89

### 목표 방향 기준 다리 역할 구분

목표를 바라보는 방향을 기준으로 다리 순서를 맞춥니다. 몸체가 돌아가도 같은 순서의 다리가 같은 방향의 역할을 학습하도록 했습니다.

https://github.com/user-attachments/assets/cdf8ae27-8f7b-48c1-9cd0-60731ed68c2c

### 다리 역할 구분 + 기준값·차이값 제어

다리 순서를 맞추고, 기준값과 차이값의 조합으로 12개 관절의 목표 각도를 계산합니다. 관절의 힘은 개별적으로 제어합니다.

https://github.com/user-attachments/assets/01710a98-8db7-46b3-9d7d-c1c26d7f0347

## 모델 모음 씬 실행

학습한 ONNX 모델 62개를 7개 씬에 모았습니다. 각 로봇을 선택해 움직임을 확인할 수 있으며, 다리 순서 비교용 중복 배치를 포함해 총 63대가 배치되어 있습니다.

1. 저장소를 내려받아 Unity Hub에서 프로젝트 폴더를 추가합니다.
2. **Unity 6000.0.32f1**로 열고 패키지 설치와 에셋 임포트가 끝날 때까지 기다립니다.
3. 아래 씬 중 하나를 열고 **Play**를 누릅니다. 학습 서버나 Python 실행 없이 저장된 모델로 추론합니다.

| 씬 | 구성 | 로봇 수 |
|---|---|---:|
| [01 · 사족 기준값·차이값 제어](Assets/Scenes/ONNXCandidates/01_ForcedQuad_113_18.unity) | 113 관측 / 18 행동 | 4 |
| [02 · 이족 보행](Assets/Scenes/ONNXCandidates/02_Bipedal_191_22.unity) | 191 관측 / 22 행동 | 7 |
| [03 · 초기 이족 보행](Assets/Scenes/ONNXCandidates/03_LegacyBipedal_175_20.unity) | 175 관측 / 20 행동 | 35 |
| [04 · 초기 이족 보행 · 대체 물리 구성](Assets/Scenes/ONNXCandidates/04_LegacyBipedal_158_21.unity) | 158 관측 / 21 행동 | 13 |
| [05 · 사족 관절별 독립 제어](Assets/Scenes/ONNXCandidates/05_LegacyQuad_126_32.unity) | 126 관측 / 32 행동 | 2 |
| [06 · 목표 방향 기준 다리 순서 1·2·3·4](Assets/Scenes/ONNXCandidates/06_SQW04_Order1234_113_32.unity) | 113 관측 / 32 행동 | 1 |
| [07 · 목표 방향 기준 다리 순서 4·1·2·3](Assets/Scenes/ONNXCandidates/07_SQW04_Order4123_113_32.unity) | 113 관측 / 32 행동 | 1 |

- **모델 선택:** `Tab` 또는 화면의 `Previous` / `Next`
- **카메라:** `F`로 선택 모델에 포커스, `Overview`로 전체 보기
- **자유 이동:** 마우스 오른쪽 버튼을 누른 채 `WASD`, `Q/E`로 높이 조절, `Shift`로 빠르게 이동
- **초기화·정지:** `R`로 전체 초기화, `Pause` / `Resume`으로 일시 정지·재개
- **목표 속도:** 화면의 `Target speed` 슬라이더로 조절

초기 모델 일부는 학습 당시의 물리 설정을 완전히 확보하지 못해 대체 구성으로 실행합니다. 씬별 적용 범위와 재생성 방법은 [모델 모음 씬 안내](docs/onnx-candidate-scenes.md)에 정리했습니다.
