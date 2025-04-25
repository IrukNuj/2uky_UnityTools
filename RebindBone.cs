using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkinnedMeshRenderer))]
public class SkinnedMeshRendererRebindEditor : Editor
{
    private Transform customRootBone;
    private SkinnedMeshRenderer customSkinnedMesh;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SkinnedMeshRenderer smr = (SkinnedMeshRenderer)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("\uD83E\uDDB4 ---Bone Rebinder chinpo--", EditorStyles.boldLabel);

        customSkinnedMesh = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Target SkinnedMeshRenderer", customSkinnedMesh ? customSkinnedMesh : smr, typeof(SkinnedMeshRenderer), true);
        customRootBone = (Transform)EditorGUILayout.ObjectField("Root Bone (Armature/Hips)", customRootBone, typeof(Transform), true);

        EditorGUILayout.LabelField("\uD83E\uDDB4 --- ---", EditorStyles.boldLabel);


        if (GUILayout.Button("\uD83C\uDFCB\uFE0F Rebind Bones to Avatar"))
        {
            var targetSMR = customSkinnedMesh ? customSkinnedMesh : smr;

            if (targetSMR == null)
            {
                Debug.LogWarning("SkinnedMeshRenderer が選択されていません！");
                return;
            }

            if (customRootBone == null)
            {
                Debug.LogWarning("Root Bone (Armature/Hips) を設定してください！");
                return;
            }

            var boneNames = new string[targetSMR.bones.Length];
            for (int i = 0; i < boneNames.Length; i++)
                boneNames[i] = targetSMR.bones[i]?.name;

            var newBones = new Transform[boneNames.Length];
            foreach (var t in customRootBone.root.GetComponentsInChildren<Transform>())
            {
                for (int i = 0; i < boneNames.Length; i++)
                {
                    if (t.name == boneNames[i])
                        newBones[i] = t;
                }
            }

            targetSMR.bones = newBones;
            targetSMR.rootBone = customRootBone;

            Debug.Log($"\u2705 {targetSMR.name} に対して Rebind 完了しました！");
        }
    }
}

[InitializeOnLoad]
public static class AutoRebindOnLoad
{
    static AutoRebindOnLoad()
    {
        EditorApplication.delayCall += RunAutoRebind;
    }

    static void RunAutoRebind()
    {
        var smr = GameObject.Find("Item_Boots")?.GetComponent<SkinnedMeshRenderer>();
        var avatar = GameObject.FindWithTag("Player");
        var hips = avatar?.transform.Find("Armature/Hips");

        if (smr == null || hips == null)
        {
            Debug.Log("⚠️ 自動Rebind対象が見つかりませんでした（スキップ）");
            return;
        }

        var boneNames = new string[smr.bones.Length];
        for (int i = 0; i < boneNames.Length; i++)
            boneNames[i] = smr.bones[i]?.name;

        var newBones = new Transform[boneNames.Length];
        foreach (var t in hips.root.GetComponentsInChildren<Transform>())
        {
            for (int i = 0; i < boneNames.Length; i++)
            {
                if (t.name == boneNames[i])
                    newBones[i] = t;
            }
        }

        smr.bones = newBones;
        smr.rootBone = hips;

        Debug.Log("✅ 自動Rebind完了！（起動時）");
    }
}
