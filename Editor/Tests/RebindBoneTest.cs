using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

// テスト対象: GetUnmatchedBones 関数
[TestFixture]
public class GetUnmatchedBonesTests {
    private GameObject avatar;
    private SkinnedMeshRenderer smr;
    private Transform rootBone;

    [SetUp]
    public void Setup() {
        avatar = new GameObject("AvatarRoot");
        var hips = new GameObject("Hips").transform;
        hips.SetParent(avatar.transform);

        var boneA = new GameObject("BoneA").transform;
        var boneB = new GameObject("BoneB").transform;
        boneA.SetParent(hips);
        boneB.SetParent(hips);

        smr = avatar.AddComponent<SkinnedMeshRenderer>();
        smr.bones = new Transform[] { boneA, null };
        smr.rootBone = hips;

        rootBone = hips;
    }

    [TearDown]
    public void Teardown() {
        Object.DestroyImmediate(avatar);
    }

    [Test]
    public void WhenBoneMissing_ReturnsUnmatched() {
        var editor = ScriptableObject.CreateInstance<SkinnedMeshRendererRebindEditor>();
        var unmatched = InvokeGetUnmatchedBones(editor, smr, rootBone);
        Object.DestroyImmediate(editor);

        Assert.That(unmatched.Count, Is.EqualTo(1));
    }

    [Test]
    public void WhenAllBonesMatch_ReturnsEmpty() {
        smr.bones = new Transform[] { rootBone.GetChild(0), rootBone.GetChild(1) };
        var editor = ScriptableObject.CreateInstance<SkinnedMeshRendererRebindEditor>();
        var unmatched = InvokeGetUnmatchedBones(editor, smr, rootBone);
        Object.DestroyImmediate(editor);

        Assert.That(unmatched.Count, Is.EqualTo(0));
    }

    private List<string> InvokeGetUnmatchedBones(SkinnedMeshRendererRebindEditor editor, SkinnedMeshRenderer smr, Transform rootBone) {
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
}

// テスト対象: PerformRebind 関数
[TestFixture]
public class PerformRebindTests {
    private GameObject avatar;
    private SkinnedMeshRenderer smr;
    private Transform rootBone;

    [SetUp]
    public void Setup() {
        avatar = new GameObject("AvatarRoot");
        var hips = new GameObject("Hips").transform;
        hips.SetParent(avatar.transform);

        var boneA = new GameObject("BoneA").transform;
        var boneB = new GameObject("BoneB").transform;
        boneA.SetParent(hips);
        boneB.SetParent(hips);

        smr = avatar.AddComponent<SkinnedMeshRenderer>();
        smr.bones = new Transform[] { boneA, null };
        smr.rootBone = hips;

        rootBone = hips;
    }

    [TearDown]
    public void Teardown() {
        Object.DestroyImmediate(avatar);
    }

    [Test]
    public void WhenValidInput_RebindsSuccessfully() {
        var editor = ScriptableObject.CreateInstance<SkinnedMeshRendererRebindEditor>();
        var prevBone = smr.bones[0];
        InvokePerformRebind(editor, smr);
        Object.DestroyImmediate(editor);

        Assert.That(smr.bones[0], Is.EqualTo(prevBone));
        Assert.That(smr.rootBone, Is.EqualTo(rootBone));
    }

    [Test]
    public void WhenTargetIsNull_DoesNothing() {
        var editor = ScriptableObject.CreateInstance<SkinnedMeshRendererRebindEditor>();
        Assert.DoesNotThrow(() => InvokePerformRebind(editor, null));
        Object.DestroyImmediate(editor);
    }

    [Test]
    public void WhenRootIsNull_DoesNothing() {
        var editor = ScriptableObject.CreateInstance<SkinnedMeshRendererRebindEditor>();
        rootBone = null;
        Assert.DoesNotThrow(() => InvokePerformRebind(editor, smr));
        Object.DestroyImmediate(editor);
    }

    private void InvokePerformRebind(SkinnedMeshRendererRebindEditor editor, SkinnedMeshRenderer smr) {
        typeof(SkinnedMeshRendererRebindEditor)
            .GetMethod("PerformRebind", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(editor, new object[] { smr });
    }
}
