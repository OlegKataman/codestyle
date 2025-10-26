using UnityEditor;
using UnityEngine;

namespace Diarrhea.Scripts.Editor
{
    public abstract class AnimationPathFixer
    {
        [MenuItem("Tools/Fix Animation Paths")]
        public static void FixPaths()
        {
            var clip = Selection.activeObject as AnimationClip;
            if (clip == null)
            {
                Debug.LogError("Select an AnimationClip first!");
                return;
            }

            var bindings = AnimationUtility.GetCurveBindings(clip);

            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);

                // Пример: добавляем новый root-префикс
                var newPath = "Root/" + binding.path;

                var newBinding = binding;
                newBinding.path = newPath;

                AnimationUtility.SetEditorCurve(clip, binding, null); // удалить старый
                AnimationUtility.SetEditorCurve(clip, newBinding, curve); // добавить новый
            }

            Debug.Log($"Fixed paths in {clip.name}");
        }
    }
}