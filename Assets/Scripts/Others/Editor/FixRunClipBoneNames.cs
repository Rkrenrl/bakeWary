using UnityEditor;
using UnityEngine;

public class FixRunClipBoneNames
{
    [MenuItem("Tools/Fix Run Clip Bone Names")]
    static void Fix()
    {
        AnimationClip original = Selection.activeObject as AnimationClip;
        if (original == null)
        {
            Debug.LogError("Run 애니메이션 클립을 Project 창에서 선택한 상태로 실행하세요.");
            return;
        }

        AnimationClip fixedClip = Object.Instantiate(original);
        fixedClip.name = original.name + "_Fixed";

        var bindings = AnimationUtility.GetCurveBindings(original);
        foreach (var binding in bindings)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(original, binding);
            EditorCurveBinding newBinding = binding;
            newBinding.path = binding.path.Replace("Armature.001", "Armature"); // 경로 안의 접미사 제거
            AnimationUtility.SetEditorCurve(fixedClip, newBinding, curve);
        }

        string path = "Assets/" + fixedClip.name + ".anim";
        AssetDatabase.CreateAsset(fixedClip, path);
        AssetDatabase.SaveAssets();
        Debug.Log("저장됨: " + path);
    }
}