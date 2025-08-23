// using System;
// using System.Collections.Generic;
// using System.Management.Instrumentation;
// using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Security.Cryptography;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using PlasticPipe.PlasticProtocol.Messages;

public class RootZCurveGenerator : EditorWindow
{
    public UnityEngine.Object fbxAsset;
    private UnityEngine.Object _lastObj;
    private string fbxPath => AssetDatabase.GetAssetPath(fbxAsset);
    public string curveName = "RootZTransition";
    public float scale = 1f;
    public float offset = 0f;

    public int targetKeyframe = 4;
    private AnimationClip targetClip;
    private AnimationCurve rootTZCurve;
    //public string rootPath = "Root";

    // private float _minValue = 0;
    // private float _maxValue = 1;
    // private float _totalTime = 1;

    private const float _tangentErrorWeight = 0.5f;


    [MenuItem("Tools/Generate Root Z Displacement Curve")]
    public static void ShowWindow()
    {
        GetWindow<RootZCurveGenerator>("RootZ Generator");
    }

    public void OnGUI()
    {
        GUILayout.Label("Add RootZ curve to FBX clip", EditorStyles.boldLabel);
        // targetClip = EditorGUILayout.ObjectField("Target Animation Clip", targetClip, typeof(AnimationClip), false) as AnimationClip;
        // rootPath = EditorGUILayout.TextField("Root Node Path", rootPath);
        fbxAsset = EditorGUILayout.ObjectField(
            "Target Animation Clip",
            fbxAsset,
            typeof(UnityEngine.Object),
            false
        );

        if (fbxAsset != _lastObj)
        {
            ClearClip();
            Repaint();

            _lastObj = fbxAsset;
        }

        curveName = EditorGUILayout.TextField("Curve Name", curveName);
        scale = EditorGUILayout.FloatField("Scale", scale);
        offset = EditorGUILayout.FloatField("Offset", offset);


        if (GUILayout.Button("Generate"))
        {
            OnGenerateButtonClick();

            #region "save in importer"
            // ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            // // importer.clipAnimations[0].curves[0] = new ClipAnimationInfoCurve();
            // // importer.clipAnimations[0].curves[0].name = "test";
            // //importer.clipAnimations[0].curves[0]
            // //Array.Resize(ref importer.clipAnimations[0].curves, )
            // //importer.clipAnimations[0].curves

            // var clips = importer.clipAnimations;
            // var curClip = clips[0];

            // ClipAnimationInfoCurve[] addOne = new ClipAnimationInfoCurve[curClip.curves.Length + 1];
            // curClip.curves.CopyTo(addOne, 0);

            // ClipAnimationInfoCurve one = new ClipAnimationInfoCurve();
            // one.name = curveName;
            // one.curve = rootTZCurve;
            // addOne[^1] = one;

            // curClip.curves = addOne;

            // var newClips = (ModelImporterClipAnimation[])clips.Clone();
            // newClips[0] = curClip;
            // importer.clipAnimations = newClips;

            #endregion
        }

        ShowPreview();

        if (GUILayout.Button("Simplify"))
        {
            OnSimplifyButtonClick();
        }

        EditorGUI.BeginDisabledGroup(rootTZCurve == null);
        if (GUILayout.Button("Save"))
        {
            OnSaveButtonClick();
        }
        EditorGUI.EndDisabledGroup();

        // if (GUILayout.Button("Smooth"))
        // {
        //     OnSmoothButtonClick();
        // }
    }

    private void ClearClip()
    {
        targetClip = null;
        rootTZCurve = null;
    }

    private void OnGenerateButtonClick()
    {
        //string fbxPath = AssetDatabase.GetAssetPath(fbxAsset);
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);

        if (assets == null || assets.Length == 0)
        {
            Debug.LogError("FBX Load Error!");
            return;
        }

        foreach (UnityEngine.Object asset in assets)
        {
            if (asset is AnimationClip clip && !clip.name.Contains("__preview__"))
            {
                targetClip = clip;
            }
        }

        if (targetClip == null)
        {
            Debug.LogError($"Fail To Load Animation Clip in {fbxAsset}");
            return;
        }

        rootTZCurve = AnimationUtility.GetEditorCurve(targetClip, EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT.z"));
        if (rootTZCurve == null)
        {
            Debug.LogError("Fail To Get RootT.z");
            return;
        }

        var (_minValue, _maxValue, _totalTime) = GenerateRootZCurve();

        Debug.Log($"Load animation clip {targetClip.name} done, maxZtrans:{_maxValue}, minZtrans: {_minValue}, totalClipTime: {_totalTime}");
    }

    // private void OnSmoothButtonClick()
    // {
    //     for (int i = 0; i < rootTZCurve.length; i++)
    //     {
    //         rootTZCurve.SmoothTangents(i, 0);
    //     }
    //     Repaint();
    // }

    private void SmoothCurve(float factor=0)
    {
        for (int i = 0; i < rootTZCurve.length; i++)
        {
            rootTZCurve.SmoothTangents(i, factor);
        }
    }

    private void OnSimplifyButtonClick()
    {
        SmoothCurve();

        int originkeysCnt = rootTZCurve.keys.Length;
        Keyframe[] simpifiedKeys = Simplify(rootTZCurve.keys, 0.2f);
        //Debug.Log(simpifiedKeys.Length);

        rootTZCurve.keys = simpifiedKeys;
        Debug.Log($"Smooth&Simplified Done, origin keyframes: {originkeysCnt}, simiplied keyframes: {rootTZCurve.keys.Length}");
        //SmoothCurve();
        Repaint();
    }

    private void OnSaveButtonClick()
    {
        ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        // importer.clipAnimations[0].curves[0] = new ClipAnimationInfoCurve();
        // importer.clipAnimations[0].curves[0].name = "test";
        //importer.clipAnimations[0].curves[0]
        //Array.Resize(ref importer.clipAnimations[0].curves, )
        //importer.clipAnimations[0].curves

        var clips = importer.clipAnimations;
        var curClip = clips[0];

        ClipAnimationInfoCurve[] addOne = new ClipAnimationInfoCurve[curClip.curves.Length + 1];
        curClip.curves.CopyTo(addOne, 0);

        ClipAnimationInfoCurve one = new ClipAnimationInfoCurve();
        one.name = curveName;
        one.curve = rootTZCurve;
        addOne[^1] = one;

        curClip.curves = addOne;

        var newClips = (ModelImporterClipAnimation[])clips.Clone();
        newClips[0] = curClip;
        importer.clipAnimations = newClips;

        importer.SaveAndReimport();

        Debug.Log($"Save to clip {targetClip.name} as {curveName}.");
    }

    private void ShowPreview()
    {
        GUILayout.Space(10);
        GUILayout.Label("Curve Preview", EditorStyles.label);

        // 核心：使用EditorGUILayout.CurveField展示曲线
        // 第二个参数设为true允许在面板上编辑曲线
        GUI.enabled = false;
        EditorGUILayout.CurveField(
            rootTZCurve,
            Color.blue, // 曲线颜色
            new Rect(0, -1, 1, 2), // 曲线预览窗口的坐标范围（X:时间，Y:值）
            GUILayout.Height(200) // 曲线展示区域高度
        );
        GUI.enabled = true;
    }

    private (float minValue, float maxValue, float totalTime) GenerateRootZCurve()
    {
        float minValue = 0;
        float maxValue = 0;

        Keyframe[] keys = rootTZCurve.keys;
        float firstValue = keys[0].value;
        float firstTime = keys[0].time;
        float totalTime = keys[^1].time - keys[0].time;
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i].value -= firstValue;
            keys[i].time -= firstTime;
            minValue = Mathf.Min(keys[i].value, minValue);
            maxValue = Mathf.Max(keys[i].value, maxValue);
        }

        for (int i = 0; i < keys.Length; i++)
        {
            keys[i].value /= (maxValue - minValue);
            keys[i].time /= totalTime;
        }

        for (int i = 0; i < keys.Length; i++)
        {
            keys[i].value *= scale;
            keys[i].value += offset;
        }

        rootTZCurve.keys = keys;
        return (minValue, maxValue, totalTime);
    }

    private AnimationCurve SimplifyCurve(AnimationCurve curve = null)
    {
        //Debug.Log("in Simplify");
        if (targetKeyframe == -1)
        {
            return null;
        }
        //Debug.Log($"targetFrame: {targetKeyframe}");

        curve ??= rootTZCurve;

        //float totalDuration = rootTZCurve[rootTZCurve.length - 1].time - rootTZCurve[0].time;
        //float startTime = rootTZCurve[0].time;
        float startTime = curve[0].time;
        float totalDuration = curve[curve.length - 1].time - startTime;

        Keyframe[] newKeyFrames = new Keyframe[targetKeyframe];

        foreach (Keyframe k in curve.keys)
        {
            Debug.Log($"{k.time}, {k.value}, {k.inTangent}, {k.outTangent}");
        }

        float interval = totalDuration / (targetKeyframe - 1);

        for (int i = 0; i < targetKeyframe; i++)
        {
            float time = startTime + i * interval;
            float value = curve.Evaluate(time);

            float pointTangent; //outTangent;
            // if (i == 0)
            // {
            //     inTangent = curve.keys[0].inTangent;
            //     outTangent = CalculateTangent(curve, time, time + interval);
            // }
            // else if (i == targetKeyframe - 1)
            // {
            //     inTangent = CalculateTangent(curve, time - interval, time);
            //     outTangent = curve.keys[curve.length - 1].outTangent;
            // }
            // else
            // {
            //     inTangent = CalculateTangent(curve, time - interval, time);
            //     outTangent = CalculateTangent(curve, time, time + interval);
            // }
            if (i == 0) pointTangent = curve.keys[0].outTangent;
            else if (i == targetKeyframe - 1) pointTangent = curve.keys[curve.length - 1].inTangent;
            else pointTangent = CalculateTangent(curve, time);

            newKeyFrames[i] = new Keyframe(time, value, pointTangent, pointTangent);
        }

        Debug.Log(newKeyFrames.Length);

        return new AnimationCurve(newKeyFrames);
    }

    private float CalculateTangent(AnimationCurve curve, float time1, float time2)
    {
        if (Mathf.Abs(time1 - time2) < Mathf.Epsilon) return 0;

        float value1 = curve.Evaluate(time1);
        float value2 = curve.Evaluate(time2);

        return (value2 - value1) / (time2 - time1);
    }

    private float CalculateTangent(AnimationCurve curve, float time)
    {
        float delta = 0.001f;
        // float t1 = Mathf.Max(curve[0].time, time - delta);
        // float t2 = Mathf.Min(curve[curve.length-1].time, time + delta);
        float v1 = curve.Evaluate(time - delta);
        float v2 = curve.Evaluate(time + delta);
        return (v2 - v1) / (delta * 2);
    }

    private Keyframe[] Simplify(Keyframe[] originalKeys, float tolerance)
    {
        if (originalKeys.Length < 2) return originalKeys;

        System.Array.Sort(originalKeys, (a, b) => a.time.CompareTo(b.time));

        HashSet<int> keepIndices = new HashSet<int>();
        keepIndices.Add(0);
        keepIndices.Add(originalKeys.Length - 1);

        SimplifyRecursive(originalKeys, 0, originalKeys.Length - 1, tolerance, keepIndices);

        List<Keyframe> simplifiedKeys = new List<Keyframe>();
        foreach (int index in keepIndices)
            simplifiedKeys.Add(originalKeys[index]);

        simplifiedKeys.Sort((a, b) => a.time.CompareTo(b.time));

        return simplifiedKeys.ToArray();
    }

    private void SimplifyRecursive(Keyframe[] keys, int startIndex, int endIndex, float tolerance, HashSet<int> keepIndices)
    {
        if (endIndex - startIndex <= 1) return;

        float maxError = 0;
        float maxPosError = 0;
        float maxTanError = 0;
        int maxErrorIndex = -1;

        Keyframe start = keys[startIndex];
        Keyframe end = keys[endIndex];

        for (int i = startIndex + 1; i < endIndex; i++)
        {
            Keyframe current = keys[i];
            var (posError, tanError) = CalculateError(current, start, end);
            float error = posError + tanError * _tangentErrorWeight;

            if (error > maxError)
            {
                maxError = error;
                maxTanError = tanError;
                maxPosError = posError;
                maxErrorIndex = i;
            }
            /// Debug.Log($"current frame: {maxErrorIndex}, error: {posError}, {tanError}");
        }
        ///Debug.Log($"current frame: {maxErrorIndex}, error: {maxError}, {maxPosError}, {maxTanError}");
        if (maxError > tolerance && maxErrorIndex != -1)
        {
            keepIndices.Add(maxErrorIndex);
            SimplifyRecursive(keys, startIndex, maxErrorIndex, tolerance, keepIndices);
            SimplifyRecursive(keys, maxErrorIndex, endIndex, tolerance, keepIndices);
        }
    }

    private (float posError, float tangentError) CalculateError(Keyframe current, Keyframe start, Keyframe end) {
        float t = Mathf.InverseLerp(start.time, end.time, current.time);
        float predValue = EvaluateBezier(start, end, t);

        float positionError = Mathf.Abs(current.value - predValue);

        float curveTangent = CalculateBezierTangent(start, end, t);
        float tangentError = Mathf.Abs(current.outTangent - curveTangent);

        return (positionError, tangentError);
    }

    private float EvaluateBezier(Keyframe start, Keyframe end, float t) {
        AnimationCurve tmpCurve = new AnimationCurve(start, end);
        return tmpCurve.Evaluate(Mathf.Lerp(start.time, end.time, t));
    }

    private float CalculateBezierTangent(Keyframe start, Keyframe end, float t) {
        float delta = 0.001f;
        float t1 = Mathf.Clamp01(t - delta);
        float t2 = Mathf.Clamp01(t + delta);
        float v1 = EvaluateBezier(start, end, t1);
        float v2 = EvaluateBezier(start, end, t2);
        return (v2 - v1) / (2 * delta);
    }
}
