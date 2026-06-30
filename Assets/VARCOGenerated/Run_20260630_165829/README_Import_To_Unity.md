# VARCO Batch Unity Export

이 폴더는 08_unity_pipeline.py가 API 결과물을 Unity용으로 정리한 폴더입니다.

## Unity로 가져오기

1. unity_export 폴더 안의 Audio, Models, Manifests 폴더를 Unity Assets 안으로 드래그합니다.
2. TTS 음성은 Audio/TTS 폴더에서 확인합니다.
3. 효과음은 Audio/SFX 폴더에서 확인합니다.
4. 3D 모델은 Models 폴더의 GLB 파일을 사용합니다.
5. batch_unity_manifest.json에서 입력과 결과를 확인합니다.

## 생성 결과

- image_to_3d / sample_model / ok / outputs/batch/unity_export/Models/sample_model.glb