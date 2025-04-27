using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SkinnedMeshRenderer))]
public class SkinnedMeshRendererRebindEditor : Editor
{
    private Transform customRootBone;
    private SkinnedMeshRenderer customSkinnedMesh;
    private bool isShowingSkinnedMeshBones = false;
    private bool isShowingRootBoneChildren = false;
    private bool isShowingUnmatchedPreview = true;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        SkinnedMeshRenderer smr = (SkinnedMeshRenderer)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("\uD83E\uDDB4 Bone再設定ツールくん", EditorStyles.boldLabel);

        customSkinnedMesh = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("SkinnedMeshRenderer", customSkinnedMesh ?? smr, typeof(SkinnedMeshRenderer), true);
        customRootBone = (Transform)EditorGUILayout.ObjectField("Root Bone (Armature/Hips)", customRootBone, typeof(Transform), true);

        if (customSkinnedMesh && customRootBone) {
            HandleUnmatchedBones(customSkinnedMesh, customRootBone);
        }

        DrawSkinnedMeshInfo(customSkinnedMesh);
        DrawRootBoneInfo(customRootBone);

        if (GUILayout.Button("\uD83C\uDFCB\uFE0F Boneの再設定をする")) {
            PerformRebind(customSkinnedMesh ?? smr);
        }
    }

    private void HandleUnmatchedBones(SkinnedMeshRenderer smr, Transform rootBone) {
        var unmatched = GetUnmatchedBones(smr, rootBone);
        if (unmatched.Any()) {
            EditorGUILayout.HelpBox($"⚠️ 一致しないBoneが存在します！未一致: {unmatched.Count}本", MessageType.Warning);
            isShowingUnmatchedPreview = EditorGUILayout.Foldout(isShowingUnmatchedPreview, "未一致のBone一覧");
            if (isShowingUnmatchedPreview) {
                EditorGUI.indentLevel++;
                foreach (var bone in unmatched)
                    EditorGUILayout.LabelField("• " + bone);
                EditorGUI.indentLevel--;
            }
        }
    }

    private void PerformRebind(SkinnedMeshRenderer targetSMR) {
        if (targetSMR == null || customRootBone == null) {
            Debug.LogError("SkinnedMeshRenderer または Root Bone が未設定です！");
            return;
        }

        if (targetSMR.bones == null) {
            Debug.LogError("対象のSkinnedMeshRendererにboneがありません。");
            return;
        }

        var boneNames = targetSMR.bones.Select(b => b?.name).ToArray();
        var newBones = new Transform[boneNames.Length];

        var allTransforms = customRootBone.root.GetComponentsInChildren<Transform>();
        var transformMap = allTransforms.ToDictionary(t => t.name, t => t);

        var unmatchedBones = new List<string>();
        int matchCount = 0;

        for (int i = 0; i < boneNames.Length; i++) {
            if (boneNames[i] != null && transformMap.TryGetValue(boneNames[i], out var match)) {
                newBones[i] = match;
                matchCount++;
            }
            else {
                unmatchedBones.Add(boneNames[i] ?? "(Missing Bone)");
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
        var unmatched = new List<string>();
        if (smr == null || rootBone == null) return unmatched;

        var boneNamesInHierarchy = new HashSet<string>(rootBone.GetComponentsInChildren<Transform>().Select(t => t.name));

        foreach (var bone in smr.bones) {
            if (bone == null || !boneNamesInHierarchy.Contains(bone.name)) {
                unmatched.Add(bone?.name ?? "(Missing Bone)");
            }
        }

        return unmatched;
    }

    private void DrawSkinnedMeshInfo(SkinnedMeshRenderer smr) {
        if (smr != null) {
            EditorGUILayout.LabelField("選択された SkinnedMesh:", smr.name);
            EditorGUILayout.LabelField($"\u2022 bone数: {smr.bones?.Length ?? 0}");

            isShowingSkinnedMeshBones = EditorGUILayout.Foldout(isShowingSkinnedMeshBones, "\u25BE Bone 詳細（SkinnedMesh）");
            if (isShowingSkinnedMeshBones && smr.bones != null) {
                EditorGUI.indentLevel++;
                for (int i = 0; i < smr.bones.Length; i++) {
                    var bone = smr.bones[i];
                    if (bone != null)
                        EditorGUILayout.LabelField($"[{i}] {bone.name}");
                    else
                        EditorGUILayout.LabelField($"[{i}] (Missing Bone)");
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

            isShowingRootBoneChildren = EditorGUILayout.Foldout(isShowingRootBoneChildren, "\u25BE Bone 詳細（RootBone配下）");
            if (isShowingRootBoneChildren) {
                EditorGUI.indentLevel++;
                for (int i = 0; i < allChildren.Length; i++) {
                    EditorGUILayout.LabelField($"[{i}] {allChildren[i].name}");
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}
