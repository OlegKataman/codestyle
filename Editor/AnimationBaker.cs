using UnityEngine;
using UnityEditor;
using System.IO;

public sealed class AnimationBaker : EditorWindow
{
    private Animator _animator;
    private Camera _renderCamera;
    private int _frameRate = 30;
    private int _outputResolution = 512;
    private string _outputFolder = "Assets/BakedAnimations";

    [MenuItem("Tools/Animation Baker")]
    public static void ShowWindow()
    {
        GetWindow<AnimationBaker>("Animation Baker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Bake bone animations to sprite frames", EditorStyles.boldLabel);

        _animator = (Animator)EditorGUILayout.ObjectField("Animator", _animator, typeof(Animator), true);
        _renderCamera = (Camera)EditorGUILayout.ObjectField("Render Camera", _renderCamera, typeof(Camera), true);
        _frameRate = EditorGUILayout.IntField("Frame Rate", _frameRate);
        _outputResolution = EditorGUILayout.IntField("Output Resolution", _outputResolution);
        _outputFolder = EditorGUILayout.TextField("Output Folder", _outputFolder);

        if (GUILayout.Button("Bake All Clips"))
        {
            if (_animator == null || _renderCamera == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign both Animator and Camera!", "OK");
                return;
            }

            BakeAllClips();
        }
    }

    private void BakeAllClips()
    {
        var clips = _animator.runtimeAnimatorController.animationClips;
        if (clips.Length == 0)
        {
            EditorUtility.DisplayDialog("No Clips", "Animator has no AnimationClips!", "OK");
            return;
        }

        Directory.CreateDirectory(_outputFolder);
        
        // --- Настраиваем рендер ---
        var rt = new RenderTexture(_outputResolution, _outputResolution, 24, RenderTextureFormat.ARGB32);
        var tex = new Texture2D(_outputResolution, _outputResolution, TextureFormat.RGBA32, false);
        _renderCamera.targetTexture = rt;
        _renderCamera.clearFlags = CameraClearFlags.SolidColor;
        _renderCamera.backgroundColor = new Color(0, 0, 0, 0); // прозрачный фон
        _renderCamera.cullingMask = LayerMask.GetMask("Enemy"); // рендерим только слой персонажа

        foreach (var clip in clips)
        {
            string clipFolder = Path.Combine(_outputFolder, clip.name);
            Directory.CreateDirectory(clipFolder);

            int frameCount = Mathf.CeilToInt(clip.length * _frameRate);
            Debug.Log($"Baking '{clip.name}' with {frameCount} frames...");

            for (int i = 0; i < frameCount; i++)
            {
                float time = i / (float)_frameRate;
                clip.SampleAnimation(_animator.gameObject, time);

                _renderCamera.Render();
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();

                byte[] bytes = tex.EncodeToPNG();
                File.WriteAllBytes(Path.Combine(clipFolder, $"frame_{i:D4}.png"), bytes);

                EditorUtility.DisplayProgressBar("Baking Animation", $"{clip.name}: frame {i}/{frameCount}", (float)i / frameCount);
            }

            EditorUtility.ClearProgressBar();
        }
        
        RenderTexture.active = null;
        AssetDatabase.Refresh();
        Debug.Log($"✅ Bake complete! Frames saved to: {_outputFolder}");
    }
}
