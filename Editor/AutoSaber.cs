﻿using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using AV3Manager;
using System;


#if VRC_SDK_VRCSDK3
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Avatars.Components;
#endif

public class NomalSaber : EditorWindow
{
    public bool isAutoSet;

    public int languageChoice;
    public string[] languageAll = { "中文", "English", "日本語", "한국어" };
    public string[] languageLag = { "语言", "Language", "言語です", "언어" };
    public int lanChange = 0;

    public GameObject userAvatar;
    public GameObject saberObjectL;
    public GameObject saberObjectR;
    public GameObject rightHandPoint;
    public GameObject leftHandPoint;

    private GameObject NewSaberObjectL;
    private GameObject NewSaberObjectR;

    private int BothHand = 0;

    private Vector3 vectorChildL = new Vector3(0, 0, 0);
    private Vector3 vectorChildR = new Vector3(0, 0, 0);

    private float GetSaberMaxSize = 0;
    private Vector3 GetMaxLengthAxis = Vector3.zero;

    [MenuItem("Qychui/NomalSaber")]
    public static void ShowWindows()
    {
        var window = GetWindow<NomalSaber>("NomalSaber");
        window.minSize = new Vector2(320, 400);
        //NomalSaber.CreateInstance<NomalSaber>().ShowUtility();
    }

    private void OnGUI()
    {
        GUIStyle titleLabelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 26,
            fontStyle = FontStyle.Normal,
        };

        EditorGUILayout.LabelField("Avatar光剑工具", titleLabelStyle, GUILayout.ExpandWidth(true), GUILayout.Height(50));

        languageChoice = EditorGUILayout.Popup(languageLag[lanChange], languageChoice, languageAll);
        EditorGUILayout.Space();
        switch (languageChoice)
        {
            case 0:
                lanChange = 0;
                break;
            case 1:
                lanChange = 1;
                break;
            case 2:
                lanChange = 2;
                break;
            case 3:
                lanChange = 3;
                break;
        }

        userAvatar = EditorGUILayout.ObjectField("模型", userAvatar, typeof(GameObject), true) as GameObject;
        EditorGUILayout.Space();

        saberObjectL = EditorGUILayout.ObjectField("左手光剑", saberObjectL, typeof(GameObject), true) as GameObject;
        EditorGUILayout.Space();
        saberObjectR = EditorGUILayout.ObjectField("右手光剑", saberObjectR, typeof(GameObject), true) as GameObject;
        EditorGUILayout.Space();


        isAutoSet = EditorGUILayout.Toggle("手动安装光剑", isAutoSet);
        if (isAutoSet == true)
        {
            EditorGUILayout.Space();
            //TODO():文字显示自适应
            EditorGUILayout.HelpBox("自动安装错误后再启用，需要选择左手和右手原点", MessageType.Info);
            //EditorGUILayout.LabelField("After an automatic installation error, when re-enabling, you need to select the origin for both the left hand and the right hand");
            EditorGUILayout.Space();
            leftHandPoint = EditorGUILayout.ObjectField("左手原点", leftHandPoint, typeof(GameObject), true) as GameObject;
            EditorGUILayout.Space();
            rightHandPoint = EditorGUILayout.ObjectField("右手原点", rightHandPoint, typeof(GameObject), true) as GameObject;
        }

        //TODO:
        EditorGUILayout.Space();

        if (GUILayout.Button("安装"))
        {
            try
            {
                var avatarForward = GetAvatarInfo();
                var saberLForward = GetSaberMaxLength(saberObjectL);
                var saberRForward = GetSaberMaxLength(saberObjectR);

                NewSaberObjectL = Instantiate(saberObjectL);
                NewSaberObjectR = Instantiate(saberObjectR);

                (var vectorL, var vectorR) = ShowAllChildObjects(userAvatar.transform); //不想改了，就这样吧

                //如果左手光剑和avatar朝向不一样
                if (saberLForward != avatarForward)
                {
                    Debug.Log("左光剑朝向不符，开始调整");

                    var rotationL = Quaternion.FromToRotation(saberLForward, avatarForward);
                    NewSaberObjectL.transform.rotation = rotationL * NewSaberObjectL.transform.rotation;

                }
                if (saberRForward != avatarForward)
                {
                    Debug.Log("右光剑朝向不符，开始调整");

                    var rotationR = Quaternion.FromToRotation(saberRForward, avatarForward);
                    NewSaberObjectR.transform.rotation = rotationR * NewSaberObjectR.transform.rotation;
                }

                EditorUtility.DisplayDialog("info", "光剑配置成功!", "确定");

            }
            catch (System.Exception)
            {
                EditorUtility.DisplayDialog("error", "请先配置模型和光剑", "确定");
            }

            #region old test Code
            //string saberNameL = saberObjectL.name;
            //GameObject newSaberObjectL = Instantiate(saberObjectL);

            //newSaberObjectL.name = saberNameL + "2424";
            //newSaberObjectL.transform.position = saberObjectL.transform.position + new Vector3(1, 0, 0); // 举例：偏移量为(1, 0, 0)

            //Debug.Log(saberRForward + ":" + avatarSaberForward);

            //float leftMaxLength =  GetSaberMaxLength(saberObjectL);
            //float rightMaxLength = GetSaberMaxLength(saberObjectR);
            //Debug.Log("左手:" + leftMaxLength);
            //Debug.Log("右手:" + rightMaxLength);

            /*
            userAvatar.GetComponent<VRCAvatarDescriptor>();
            Debug.Log("WeeeRabbi");
            Debug.Log(userAvatar.GetComponent<VRCAvatarDescriptor>().collider_fingerIndexL.transform.position);
            */
            #endregion

        }

        EditorGUILayout.Space();

        if (GUILayout.Button("合并"))
        {
            var descriptor = userAvatar.GetComponent<VRCAvatarDescriptor>();
            var expressionParameters = descriptor.expressionParameters;
            var expressionsMenu = descriptor.expressionsMenu;

            var layer = descriptor.baseAnimationLayers[4];

            AnimatorController fxController = layer.animatorController as AnimatorController;

            //var baseAnimationLayers = descriptor.baseAnimationLayers;
            //Debug.Log(fxController.name);

            var pathAnimatorController = "Assets/Qychui/VRCBS/NormalSaber/Animator/SaberAnimatorController.controller";

            var animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(pathAnimatorController);

            fxController = AnimatorCloner.MergeControllers(fxController, animatorController);

            //复制状态机
            //for (int i = 0; i < animatorController.layers.Length; i++)
            //{
            //    var sourceLayer = animatorController.layers[i];
            //    var targetLayer = new AnimatorControllerLayer()
            //    {
            //        name = sourceLayer.name,
            //        avatarMask = sourceLayer.avatarMask,
            //        blendingMode = sourceLayer.blendingMode,
            //        iKPass = sourceLayer.iKPass,
            //        syncedLayerAffectsTiming = sourceLayer.syncedLayerAffectsTiming,
            //        defaultWeight = sourceLayer.defaultWeight,
            //        stateMachine = new AnimatorStateMachine(),
            //    };
            //    fxController.AddLayer(targetLayer);
            //    CopyStates(sourceLayer.stateMachine, targetLayer.stateMachine);
            //}

            //foreach (AnimatorControllerParameter parameter in animatorController.parameters)
            //{
            //    switch (parameter.type)
            //    {
            //        case AnimatorControllerParameterType.Bool:
            //            fxController.AddParameter(parameter.name, AnimatorControllerParameterType.Bool);
            //            break;
            //        case AnimatorControllerParameterType.Float:
            //            fxController.AddParameter(parameter.name, AnimatorControllerParameterType.Float);
            //            break;
            //        case AnimatorControllerParameterType.Int:
            //            fxController.AddParameter(parameter.name, AnimatorControllerParameterType.Int);
            //            break;
            //        case AnimatorControllerParameterType.Trigger:
            //            fxController.AddParameter(parameter.name, AnimatorControllerParameterType.Trigger);
            //            break;
            //    }
            //}

            EditorUtility.SetDirty(fxController);

        }

        if (GUILayout.Button("合并VRC参数"))
        {
            MergeDemo();
        }

        if (GUILayout.Button("清除"))
        {
            Close();
        }
    }

    private void MergeDemo()
    {
        var descriptor = userAvatar.GetComponent<VRCAvatarDescriptor>();
        var expressionParameters = descriptor.expressionParameters;
        var expressionsMenu = descriptor.expressionsMenu;

        var pathAnimatorController = "Assets/Qychui/VRCBS/NormalSaber/SaberMainMenu.asset";
        var getedExpressionsMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(pathAnimatorController);

        if (expressionParameters != null && expressionsMenu !=null)
        {
            if (expressionsMenu.controls.Count < 8)
            {
                var subMeun= new VRCExpressionsMenu.Control()
                {
                    name = "SaberManager",
                    type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                };

                expressionsMenu.controls.Add(subMeun);
            }
        }
    }

    /// <summary>
    /// CopyStateMachine
    /// </summary>
    /// <param name="sourceMachine"></param>
    /// <param name="targetMachine"></param>
    private void CopyStates(AnimatorStateMachine sourceMachine, AnimatorStateMachine targetMachine)
    {
        //作者太菜了，自己写了几天写不出来
        //Copy from https://github.com/VRLabs/Avatars-3.0-Manager/tree/2.0.28

        //复制动画状态
        //foreach (ChildAnimatorState state in sourceMachine.states)
        //{
        //    var newState = new AnimatorState()
        //    {
        //        name = state.state.name,
        //        motion = state.state.motion,
        //        transitions = state.state.transitions,
        //    };



        //    foreach (var transition in state.state.transitions)
        //    {
        //        //var newTransition = new AnimatorStateTransition()
        //        //{
        //        //    name = transition.name,
        //        //    hasExitTime = transition.hasExitTime,
        //        //    exitTime = transition.exitTime,
        //        //    hasFixedDuration = transition.hasFixedDuration,
        //        //    offset = transition.offset,
        //        //    interruptionSource = transition.interruptionSource,
        //        //    orderedInterruption = transition.orderedInterruption,
        //        //    canTransitionToSelf = transition.canTransitionToSelf,
        //        //    isExit = transition.isExit,
        //        //    conditions = transition.conditions,
        //        //};

        //        newState.transitions.Append(transition);

        //    }

        //    targetMachine.AddState(newState, state.position);
        //}

        //targetMachine.anyStateTransitions = sourceMachine.anyStateTransitions;

        //for (int i = 0; i < sourceMachine.anyStateTransitions.Length; i++)
        //{
        //    targetMachine.anyStateTransitions[i] = sourceMachine.anyStateTransitions[i];
        //}
        //foreach (var item in sourceMachine.anyStateTransitions)
        //{
        //    targetMachine.anyStateTransitions.Append(item);
        //}


        //复制子状态机
        //foreach (ChildAnimatorStateMachine subMachine in sourceMachine.stateMachines)
        //{
        //    var newSubMachine = new AnimatorStateMachine();
        //    newSubMachine.name = subMachine.stateMachine.name;
        //    targetMachine.AddStateMachine(subMachine.stateMachine, subMachine.position);
        //    CopyStates(subMachine.stateMachine, newSubMachine);
        //}
    }

    // TODO():获取光剑轴前判断前面是否有一层空物体
    /// <summary>
    /// 递归获取所有的子物体，获取子物体的向量和最长的边
    /// </summary>
    /// <param name="parentTransform"></param>
    private (Vector3, Vector3) ShowAllChildObjects(Transform parentTransform)
    {
        //Debug.Log(child.name);
        foreach (Transform child in parentTransform)
        {
            if (child.name == "Hand.L")
            {
                //判断手上是否存在SaberManager的物体，没有就创建，有则添加
                if (child.transform.Find("saberManagerXL") != null && child.transform.Find("saberManagerXL/saberManagerYL") != null && child.transform.Find("saberManagerXL/saberManagerYL/saberManagerZL") != null)
                {
                    //Debug.Log("找到了子物体");
                    NewSaberObjectL.name = saberObjectL.name + "_SaberL";
                    NewSaberObjectL.transform.parent = child.transform.Find("saberManagerXL/saberManagerYL/saberManagerZL");
                    NewSaberObjectL.transform.localPosition = Vector3.zero;

                    vectorChildL = child.position;
                }
                else
                {
                    //Debug.Log("创建SaberManagerL");
                    var saberManagerXL = new GameObject("saberManagerXL");
                    var saberManagerYL = new GameObject("saberManagerYL");
                    var saberManagerZL = new GameObject("saberManagerZL");

                    saberManagerZL.transform.parent = saberManagerYL.transform;
                    saberManagerYL.transform.parent = saberManagerXL.transform;
                    saberManagerXL.transform.parent = child;

                    saberManagerXL.transform.localPosition = Vector3.zero;
                    saberManagerXL.transform.localRotation = Quaternion.identity;

                    NewSaberObjectL.name = saberObjectL.name + "_Saber.L";
                    NewSaberObjectL.transform.parent = saberManagerZL.transform;
                    NewSaberObjectL.transform.localPosition = Vector3.zero;


                    vectorChildL = child.position;
                }

                //不获取坐标，改为直接绑定光剑位置
                //NewSaberObjectL.name = saberObjectL.name + "_Saber.L";
                //NewSaberObjectL.transform.parent = child;
                //NewSaberObjectL.transform.localPosition = Vector3.zero;
                //vectorChildL = child.position;

                Debug.Log("匹配到左手");
                BothHand++;
                if (BothHand >= 2)
                {
                    Debug.Log("双手都匹配到了");
                    BothHand = 0;
                    return (vectorChildL, vectorChildR);
                }
            }
            if (child.name == "Hand.R")
            {
                if (child.transform.Find("saberManagerXR") != null && child.transform.Find("saberManagerXR/saberManagerYR") != null && child.transform.Find("saberManagerXR/saberManagerYR/saberManagerZR") != null)
                {
                    NewSaberObjectR.name = saberObjectR.name + "_SaberR";
                    NewSaberObjectR.transform.parent = child.transform.Find("saberManagerXR/saberManagerYR/saberManagerZR");
                    NewSaberObjectR.transform.localPosition = Vector3.zero;

                    vectorChildR = child.position;
                }
                else
                {
                    //Debug.Log("创建SaberManagerR");
                    var saberManagerXR = new GameObject("saberManagerXR");
                    var saberManagerYR = new GameObject("saberManagerYR");
                    var saberManagerZR = new GameObject("saberManagerZR");

                    saberManagerZR.transform.parent = saberManagerYR.transform;
                    saberManagerYR.transform.parent = saberManagerXR.transform;
                    saberManagerXR.transform.parent = child;

                    saberManagerXR.transform.localPosition = Vector3.zero;
                    saberManagerXR.transform.localRotation = Quaternion.identity;

                    NewSaberObjectR.name = saberObjectR.name + "_Saber.R";
                    NewSaberObjectR.transform.parent = saberManagerZR.transform;
                    NewSaberObjectR.transform.localPosition = Vector3.zero;

                    vectorChildR = child.position;
                }

                //NewSaberObjectR.name = saberObjectR.name + "_Saber.R";
                //NewSaberObjectR.transform.parent = child;
                //NewSaberObjectR.transform.localPosition = Vector3.zero;
                //vectorChildR = child.position;

                Debug.Log("匹配到右手");
                BothHand++;
                if (BothHand >= 2)
                {
                    Debug.Log("双手都匹配到了");
                    BothHand = 0;
                    return (vectorChildL, vectorChildR);
                }
            }
            //递归
            (Vector3, Vector3) result = ShowAllChildObjects(child);
        }

        return (Vector3.zero, Vector3.zero);
    }

    /// <summary>
    /// 返回一个（0，0，0）的方向矢量，对于一些不规则的光剑判断可能会出现问题
    /// </summary>
    /// <param name="saberObject"></param>
    /// <returns></returns>
    private Vector3 GetSaberMaxLength(GameObject saberObject)
    {

        MeshFilter meshFilter = saberObject.GetComponent<MeshFilter>();

        if (meshFilter == null || saberObject.transform.childCount > 0)
        {
            GetSaberMaxSize = 0;
            GetMaxLengthAxis = Vector3.zero;

            // TODO(自己):枚举遍历所有的物体
            GetSaberAllItems(saberObject.transform);

            //Debug.Log(GetMaxLengthAxis);
            return (GetMaxLengthAxis);
        }

        //Debug.Log("当前物体的x轴长为" + meshFilter.sharedMesh.bounds.size.x);
        //Debug.Log("当前物体的y轴长为" + meshFilter.sharedMesh.bounds.size.y);
        //Debug.Log("当前物体的z轴长为" + meshFilter.sharedMesh.bounds.size.z);

        //不想重新设计函数了，就这样吧，虽然不美观，写重复了，但是能用就行
        var saberMeshX = meshFilter.sharedMesh.bounds.size.x;
        var saberMeshY = meshFilter.sharedMesh.bounds.size.y;
        var saberMeshZ = meshFilter.sharedMesh.bounds.size.z;

        var saberMax = Mathf.Max(saberMeshX, saberMeshY, saberMeshZ);

        var maxLengthAxis = Vector3.zero;

        if (saberMeshX >= saberMeshY && saberMeshX >= saberMeshZ)
        {
            maxLengthAxis = Vector3.right;
        }
        else if (saberMeshY >= saberMeshX && saberMeshY >= saberMeshZ)
        {
            maxLengthAxis = Vector3.up;
        }
        else
        {
            maxLengthAxis = Vector3.forward;
        }

        return maxLengthAxis;
    }

    private void GetSaberAllItems(Transform saberTransform)
    {
        foreach (Transform child in saberTransform)
        {
            var meshFilter = child.GetComponent<MeshFilter>();

            if (meshFilter != null)
            {
                var saberMeshX = meshFilter.sharedMesh.bounds.size.x;
                var saberMeshY = meshFilter.sharedMesh.bounds.size.y;
                var saberMeshZ = meshFilter.sharedMesh.bounds.size.z;

                var maxLengthAxis = Vector3.zero;

                if (saberMeshX >= saberMeshY && saberMeshX >= saberMeshZ)
                {
                    maxLengthAxis = Vector3.right;
                }
                else if (saberMeshY >= saberMeshX && saberMeshY >= saberMeshZ)
                {
                    maxLengthAxis = Vector3.up;
                }
                else
                {
                    maxLengthAxis = Vector3.forward;
                }

                var saberMax = Mathf.Max(saberMeshX, saberMeshY, saberMeshZ);

                if (saberMax > GetSaberMaxSize)
                {
                    GetSaberMaxSize = saberMax;
                    GetMaxLengthAxis = maxLengthAxis;
                }
            }

            GetSaberAllItems(child);
        }

    }

    /// <summary>
    /// 获取人物的身高朝向
    /// </summary>
    /// <returns></returns>
    private Vector3 GetAvatarInfo()
    {
        //Debug.Log(userAvatar.GetComponent<VRCAvatarDescriptor>());
        var avatarTransform = userAvatar.GetComponent<Transform>();
        var avatarChildCount = avatarTransform.childCount;

        //初始化轴朝向
        var shortestAxis = Vector3.zero;

        for (int i = 0; i < avatarChildCount; i++)
        {
            var avatarChild = avatarTransform.GetChild(i);
            SkinnedMeshRenderer[] avatarMesh = avatarChild.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in avatarMesh)
            {

                var bounds = skinnedMeshRenderer.bounds;
                var localScale = skinnedMeshRenderer.transform.localScale;

                // 最小尺寸的坐标轴
                if (localScale.x < localScale.y && localScale.x < localScale.z)
                {
                    shortestAxis = Vector3.right;
                }
                else if (localScale.y < localScale.x && localScale.y < localScale.z)
                {
                    shortestAxis = Vector3.up;
                }
                else
                {
                    shortestAxis = Vector3.forward;
                }
            }
        }

        return shortestAxis;
    }

}

