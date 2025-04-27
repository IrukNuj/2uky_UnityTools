using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[CustomEditor(typeof(SkinnedMeshRenderer))]
public class SkinnedMeshRendererRebindEditor : Editor
{
    private Editor defaultEditor;
    private Transform customRootBone;
    private SkinnedMeshRenderer customSkinnedMesh;
    private bool isShowSkinnedMeshBones = false;
    private bool isShowRootBoneChildren = false;
    private bool isShowUnmatchedPreview = true;

    private void OnEnable() {
        var editorType = typeof(Editor).Assembly.GetType("UnityEditor.SkinnedMeshRendererEditor");
        if (editorType != null) {
            defaultEditor = CreateEditor(targets, editorType);
        }
    }

    private void OnDisable() {
        if (defaultEditor != null)
            DestroyImmediate(defaultEditor);
    }

    public override void OnInspectorGUI() {
        if (defaultEditor != null) {
            defaultEditor.OnInspectorGUI();
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("\uD83E\uDDB4  Bone再設定ツールくん", EditorStyles.boldLabel);

        SkinnedMeshRenderer smr = (SkinnedMeshRenderer)target;

        customSkinnedMesh = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("SkinnedMeshRenderer", customSkinnedMesh ? customSkinnedMesh : smr, typeof(SkinnedMeshRenderer), true);
        customRootBone = (Transform)EditorGUILayout.ObjectField("Root Bone (Armature/Hips)", customRootBone, typeof(Transform), true);

        if (customSkinnedMesh && customRootBone) {
            HandleUnmatchedBones(customSkinnedMesh, customRootBone);
        }

        DrawSkinnedMeshInfo(customSkinnedMesh);
        DrawRootBoneInfo(customRootBone);

        if (GUILayout.Button("\uD83C\uDFCB\uFE0F Boneの再設定をする")) {
            PerformRebind(smr);
        }
    }

    private void HandleUnmatchedBones(SkinnedMeshRenderer smr, Transform rootBone) {
        var unmatched = GetUnmatchedBones(smr, rootBone);
        if (unmatched.Any()) {
            EditorGUILayout.HelpBox($"⚠️ 一致しないBoneが存在します！未一致: {unmatched.Count}本", MessageType.Warning);
            isShowUnmatchedPreview = EditorGUILayout.Foldout(isShowUnmatchedPreview, "未一致のBone一覧");
            if (isShowUnmatchedPreview) {
                EditorGUI.indentLevel++;
                foreach (var bone in unmatched)
                    EditorGUILayout.LabelField("• " + bone);
                EditorGUI.indentLevel--;
            }
        }
    }

    private void PerformRebind(SkinnedMeshRenderer fallbackSMR) {
        var targetSMR = customSkinnedMesh ? customSkinnedMesh : fallbackSMR;

        if (targetSMR == null) {
            Debug.LogError("SkinnedMeshRenderer が選択されていません！");
            return;
        }

        if (customRootBone == null) {
            Debug.LogError("Root Bone (Armature/Hips) を設定してください！");
            return;
        }

        var boneNames = new string[targetSMR.bones.Length];
        for (int i = 0; i < boneNames.Length; i++)
            boneNames[i] = targetSMR.bones[i]?.name;

        var newBones = new Transform[boneNames.Length];
        var allTransforms = customRootBone.root.GetComponentsInChildren<Transform>();
        var unmatchedBones = new List<string>();
        int matchCount = 0;

        for (int i = 0; i < boneNames.Length; i++) {
            var match = allTransforms.FirstOrDefault(t => t.name == boneNames[i]);
            if (match != null) {
                newBones[i] = match;
                matchCount++;
            }
            else {
                unmatchedBones.Add(boneNames[i]);
            }
        }

        targetSMR.bones = newBones;
        targetSMR.rootBone = customRootBone;

        Debug.Log($"✅ {targetSMR.name} に対して Boneの再設定がかんりょうしました！一致: {matchCount}/{boneNames.Length}本");

        if (unmatchedBones.Count > 0) {
            Debug.LogWarning($"❌ 一致しなかった Bone名一覧:\n- {string.Join("\n- ", unmatchedBones)}");
        }
    }

    private List<string> GetUnmatchedBones(SkinnedMeshRenderer smr, Transform rootBone) {
        var boneNames = smr.bones.Where(b => b != null).Select(b => b.name).ToList();
        var rootBoneNames = rootBone.GetComponentsInChildren<Transform>().Select(t => t.name).ToHashSet();
        return boneNames.Where(name => !rootBoneNames.Contains(name)).ToList();
    }

    private void DrawSkinnedMeshInfo(SkinnedMeshRenderer smr) {
        if (smr != null) {
            EditorGUILayout.LabelField("選択された SkinnedMesh:", smr.name);
            EditorGUILayout.LabelField($"\u2022 bone数: {smr.bones?.Length ?? 0}");

            isShowSkinnedMeshBones = EditorGUILayout.Foldout(isShowSkinnedMeshBones, "\u25BE Bone 詳細（SkinnedMesh）");
            if (isShowSkinnedMeshBones && smr.bones != null) {
                EditorGUI.indentLevel++;
                for (int i = 0; i < smr.bones.Length; i++) {
                    var bone = smr.bones[i];
                    if (bone != null)
                        EditorGUILayout.LabelField($"[{i}] {bone.name}");
                }
                EditorGUI.indentLevel--;
            }
        }
    }

    private void DrawRootBoneInfo(Transform rootBone) {
        if (rootBone != null) {
            EditorGUILayout.LabelField("選択された Root Bone:", rootBone.name);
            var allChildren = rootBone.GetComponentsInChildren<Transform>();
            EditorGUILayout.LabelField($"\u2022 子Transform数: {allChildren.Length}");

            isShowRootBoneChildren = EditorGUILayout.Foldout(isShowRootBoneChildren, "\u25BE Bone 詳細（RootBone配下）");
            if (isShowRootBoneChildren) {
                EditorGUI.indentLevel++;
                for (int i = 0; i < allChildren.Length; i++) {
                    EditorGUILayout.LabelField($"[{i}] {allChildren[i].name}");
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}
