using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Diarrhea.Scripts.Editor
{
    public static class RemoveFormerlySerializedAsTool
    {
        [MenuItem("Tools/Cleanup/Remove FormerlySerializedAs Attributes")]
        public static void RemoveAttributes()
        {
            string[] files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            int count = 0;
            var regex = new Regex(@"\[FormerlySerializedAs\s*\(\s*""[^""]+""\s*\)\s*\]\s*", RegexOptions.Compiled);

            foreach (var file in files)
            {
                string content = File.ReadAllText(file);
                if (regex.IsMatch(content))
                {
                    content = regex.Replace(content, "");
                    File.WriteAllText(file, content);
                    count++;
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"Removed FormerlySerializedAs from {count} scripts.");
        }
    }
}