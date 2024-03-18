using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
#if VRC_SDK_VRCSDK3 && !UDON
using VRC.SDK3.Avatars.Components;
using static VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
#endif

namespace Thry.TPS
{
    public enum CompareType { EQUAL, NOT_EQUAL, LESS, GREATER }

    public class AnimatorHelper
    {  
        
        #if VRC_SDK_VRCSDK3 && !UDON    
        public static AnimatorController GetFXController(VRCAvatarDescriptor descriptor, bool doGenerate = true)
        {
            if(!descriptor.customizeAnimationLayers)
            {
                descriptor.customizeAnimationLayers = true;
                Debug.LogWarning("[TPS] Customize Animation Layers was disabled, enabling it now.");
                EditorUtility.SetDirty(descriptor);
            }

            CustomAnimLayer[] layers = descriptor.baseAnimationLayers;
            if (layers.Length < 5)
            {
                Array.Resize<CustomAnimLayer>(ref layers, 5);
                descriptor.baseAnimationLayers = layers;
            } 

            int fxIndex = layers.ToList().FindIndex(l => l.type == VRCAvatarDescriptor.AnimLayerType.FX);
            if(fxIndex == -1)
            {
                fxIndex = 4;
                CustomAnimLayer customAnimLayer = layers[fxIndex];
                customAnimLayer.type = VRCAvatarDescriptor.AnimLayerType.FX;
                customAnimLayer.isEnabled = true;
                customAnimLayer.isDefault = false;
                layers[4] = customAnimLayer;
                Debug.LogWarning("[TPS] FX Layer not correctly marked. Assigning FX to layer 4.");
                EditorUtility.SetDirty(descriptor);
            }

            CustomAnimLayer fxLayer = layers[fxIndex];
            if(fxLayer.animatorController == null)
            {
                if(!doGenerate) return null;
                fxLayer.animatorController = AnimatorHelper.CreateAnimatorController(Helper.FindAvatarDirectory(descriptor.transform), "FX_" + descriptor.name);
                fxLayer.isEnabled = true;
                fxLayer.isDefault = false;
                Debug.LogWarning("[TPS] FX Layer Animator Controller was not found, creating it now.");
                EditorUtility.SetDirty(descriptor);
            }
            if(fxLayer.isEnabled == false)
            {
                fxLayer.isEnabled = true;
                Debug.LogWarning("[TPS] FX Layer was disabled, enabling it now.");
                EditorUtility.SetDirty(descriptor);
            }
            if(fxLayer.isDefault == true)
            {
                fxLayer.isDefault = false;
                Debug.LogWarning("[TPS] FX Layer was set to default, fixing now.");
                EditorUtility.SetDirty(descriptor);
            }

            layers[fxIndex] = fxLayer;
            descriptor.baseAnimationLayers = layers;

            return AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(fxLayer.animatorController));
        }

        public static AnimatorController CreateAnimatorController(string directory, string name)
        {
            string path = directory + "/" + name;
            AnimatorController animator = AnimatorController.CreateAnimatorControllerAtPathWithClip(UniquePath(path, ".asset"), EmptyClip);
            animator.layers[0].stateMachine.states[0].state.writeDefaultValues = false;
            return animator;
        }
        #endif

        public static AnimatorControllerLayer GetLayer(AnimatorController animator, string searchName, string createName, AnimatorControllerLayer needsToBeAboveLayer = null, bool createIfMissing = true)
        {
            AnimatorControllerLayer layer = null;
            int layerIndex = -1;

            for (int i = 0; i < animator.layers.Length; i++)
            {
                if (animator.layers[i].name == searchName)
                {
                    layer = animator.layers[i];
                    layerIndex = i;
                    break;
                }
            }

            if(layer == null && createIfMissing)
            {
                layer = new AnimatorControllerLayer();
                layer.name = createName;
                layer.defaultWeight = 1;
                layer.stateMachine = new AnimatorStateMachine();
                layer.stateMachine.name = layer.name;
                layer.stateMachine.hideFlags = HideFlags.HideInHierarchy;
                if (AssetDatabase.GetAssetPath(animator) != "")
                {
                    AssetDatabase.AddObjectToAsset(layer.stateMachine,
                        AssetDatabase.GetAssetPath(animator));
                }
                layerIndex = animator.layers.Length;
                animator.AddLayer(layer);
            }

            if (layer != null && needsToBeAboveLayer != null)
            {
                AnimatorControllerLayer[] layers = animator.layers;
                for (int i = 0; i < layers.Length; i++)
                {
                    if (layers[i].name == layer.name)
                    {
                        break;
                    }
                    if (layers[i].name == needsToBeAboveLayer.name)
                    {
                        needsToBeAboveLayer = layers[i];
                        layers[i] = layer;
                        for (int j = layerIndex; j > i; j--)
                            layers[j] = layers[j - 1];
                        layers[i + 1] = needsToBeAboveLayer;
                        break;
                    }
                }
                animator.layers = layers;
            }
            return layer;
        }

        public static void ClearLayer(AnimatorControllerLayer layer)
        {
            if(layer == null) return;
            layer.stateMachine.states = new ChildAnimatorState[0];
            layer.stateMachine.stateMachines = new ChildAnimatorStateMachine[0];
            layer.stateMachine.anyStateTransitions = new AnimatorStateTransition[0];
            layer.stateMachine.entryTransitions = new AnimatorTransition[0];
            layer.stateMachine.behaviours = new StateMachineBehaviour[0];
        }

        public static void RemoveLayer(AnimatorController animator, AnimatorControllerLayer layer)
        {
            if(layer == null) return;
            AnimatorControllerLayer[] layers = animator.layers;
            animator.layers = layers.Where(l => l.name != layer.name).ToArray();
        }

        public static void AddParameter(AnimatorController animator, string param, AnimatorControllerParameterType type, object defaultValue = null)
        {
            if (animator.parameters.Where(p => p.name == param).Count() == 0)
            {
                animator.AddParameter(param, type);
            }
            if (defaultValue != null)
            {
                AnimatorControllerParameter[] parameters = animator.parameters;
                if (defaultValue.GetType() == typeof(bool)) parameters.First(p => p.name == param).defaultBool = (bool)defaultValue;
                if (defaultValue.GetType() == typeof(int)) parameters.First(p => p.name == param).defaultInt = (int)defaultValue;
                if (defaultValue.GetType() == typeof(float)) parameters.First(p => p.name == param).defaultFloat = (float)defaultValue;
                animator.parameters = parameters;
            }
        }

        public static void RemoveParameter(AnimatorController animator, string param)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;
            animator.parameters = parameters.Where(p => p.name != param).ToArray();
        }

        #if VRC_SDK_VRCSDK3 && !UDON
        public static VRCAvatarParameterDriver AddParameterDriver(AnimatorState state, params (string, VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType, float)[] drive)
        {
            if (drive.Count() == 0) return null;
            VRCAvatarParameterDriver driver = state.behaviours.FirstOrDefault(b => b.GetType() == typeof(VRCAvatarParameterDriver)) as VRCAvatarParameterDriver;
            if (driver == null)
                driver = state.AddStateMachineBehaviour(typeof(VRCAvatarParameterDriver)) as VRCAvatarParameterDriver;
            foreach ((string, VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType, float) d in drive)
            {
                VRC.SDKBase.VRC_AvatarParameterDriver.Parameter driverParameter = new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter();
                driverParameter.name = d.Item1;
                driverParameter.value = d.Item3;
                driverParameter.type = d.Item2;
                driver.parameters.Add(driverParameter);
            }
            return driver;
        }
        #endif

        static string UniquePath(string path, string postFix)
        {
            if (File.Exists(path + postFix))
            {
                int i = 0;
                while (File.Exists(path + i + postFix)) i++;
                path = path + i;
            }
            return path + postFix;
        }

        private static AnimationClip _empty;
        public static AnimationClip EmptyClip
        {
            get
            {
                if (_empty == null) _empty = LoadEmptyClipOrCreateClip("Empty_Clip", "Assets", 1f / 60);
                return _empty;
            }
        }

        private static AnimationClip LoadEmptyClipOrCreateClip(string name, string directory, float length)
        {
            string[] guids = AssetDatabase.FindAssets(name + " t:animationclip");
            if (guids.Length > 0) return AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return CreateClip("Assets", "Empty_Clip", new CurveConfig("NanObject", "m_IsActive", typeof(GameObject), FloatCurve(1, length)));
        }

        public static AnimationCurve OnCurve => new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 1) });
        public static AnimationCurve OnCurveOneFrame => new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1f / 60, 1) });
        public static AnimationCurve OffCurve => new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 0) });
        public static AnimationCurve OffCurveOneFrame => new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1f / 60, 0) });
        public static AnimationCurve PositiveOneCurveOneFrame => new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1f / 60, 1) });
        public static AnimationCurve NegativeOneCurveOneFrame => new AnimationCurve(new Keyframe[] { new Keyframe(0, -1), new Keyframe(1f / 60, -1) });
        public static AnimationCurve IntCurve(int value, float time) { return new AnimationCurve(new Keyframe[] { new Keyframe(0, value), new Keyframe(time, value) }); }
        public static AnimationCurve FloatCurve(float value, float time) { return new AnimationCurve(new Keyframe[] { new Keyframe(0, value), new Keyframe(time, value) }); }
        public static AnimationCurve CustomCurve(params (float, float)[] keys) { return new AnimationCurve(keys.Select(tv => new Keyframe(tv.Item1, tv.Item2)).ToArray()); }

        public struct CurveConfig
        {
            public string Path;
            public string Attribute;
            public Type Type;
            public AnimationCurve Curve;
            public CurveConfig(string path, string at, Type type, AnimationCurve curve)
            {
                this.Path = path;
                this.Attribute = at;
                this.Type = type;
                this.Curve = curve;
            }
        }

        public static AnimationClip CreateClip(string directory, string name, params (string, Type, string, AnimationCurve)[] curves)
        {
            AnimationClip clip = new AnimationClip();
            foreach ((string, Type, string, AnimationCurve) curve in curves)
            {
                clip.SetCurve(curve.Item1, curve.Item2, curve.Item3, curve.Item4);
            }
            AssetDatabase.CreateAsset(clip, directory + "/" + name + ".anim");
            return clip;
        }

        public static AnimationClip CreateClip(string directory, string name, params CurveConfig[] curves)
        {
            AnimationClip clip = new AnimationClip();
            foreach (CurveConfig c in curves)
            {
                clip.SetCurve(c.Path, c.Type, c.Attribute, c.Curve);
            }
            AssetDatabase.CreateAsset(clip, directory + "/" + name + ".anim");
            return clip;
        }

        public static AnimationClip CreateClip(string name, params (string, Type, string, AnimationCurve)[] curves)
        {
            AnimationClip clip = new AnimationClip();
            clip.name = name;
            foreach ((string, Type, string, AnimationCurve) curve in curves)
            {
                clip.SetCurve(curve.Item1, curve.Item2, curve.Item3, curve.Item4);
            }
            return clip;
        }

        public static AnimationClip CreateClip(string name, params CurveConfig[] curves)
        {
            AnimationClip clip = new AnimationClip();
            clip.name = name;
            foreach (CurveConfig c in curves)
            {
                clip.SetCurve(c.Path, c.Type, c.Attribute, c.Curve);
            }
            return clip;
        }

        public struct ThryCurveData
        {
            public string path;
            public string propertyName;
            public AnimationCurve curve;
        }

        public static ThryCurveData[] GetAllCurves(AnimationClip clip)
        {
            if (clip == null) return new ThryCurveData[0];
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            ThryCurveData[] curves = new ThryCurveData[bindings.Length];
            for (int i = 0; i < curves.Length; i++)
            {
                ThryCurveData data = new ThryCurveData();
                data.path = bindings[i].path;
                data.propertyName = bindings[i].propertyName;
                data.curve = AnimationUtility.GetEditorCurve(clip, bindings[i]);
                curves[i] = data;
            }
            return curves;
        }

        public static AnimatorState CreateState(string name, AnimatorControllerLayer layer, Motion motion, bool setAsDefaultState = false)
        {
            AnimatorState state = layer.stateMachine.AddState(name);
            state.motion = motion;
            state.writeDefaultValues = false;
            if (setAsDefaultState) layer.stateMachine.defaultState = state;
            return state;
        }

        public static AnimatorState CreateState(string name, AnimatorStateMachine stateMachine, Motion motion, bool setAsDefaultState = false)
        {
            AnimatorState state = stateMachine.AddState(name);
            state.motion = motion;
            state.writeDefaultValues = false;
            if (setAsDefaultState) stateMachine.defaultState = state;
            return state;
        }

        public static AnimatorStateTransition CreateTransition(AnimatorState from, AnimatorState to, float duration = 0.01f, bool hasExitTime = true, float exitTime = 0.1f)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = hasExitTime;
            transition.duration = duration;
            transition.exitTime = exitTime;
            return transition;
        }

        public static AnimatorStateTransition CreateTransition(AnimatorState from, AnimatorStateMachine to, float duration = 0.01f, bool hasExitTime = true, float exitTime = 0.1f)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = hasExitTime;
            transition.duration = duration;
            transition.exitTime = exitTime;
            return transition;
        }

        public static AnimatorStateTransition CreateAnyStateTransition(AnimatorControllerLayer l, AnimatorState to, float duration = 0.01f, bool hasExitTime = false, float exitTime = 0.1f)
        {
            AnimatorStateTransition transition = l.stateMachine.AddAnyStateTransition(to);
            transition.hasExitTime = hasExitTime;
            transition.duration = duration;
            transition.exitTime = exitTime;
            transition.canTransitionToSelf = false;
            return transition;
        }

        public class Condition
        {
            public string ParamName;
            public CompareType CompareType;
            public object Value;
            public Condition(string n, CompareType t, object v)
            {
                this.ParamName = n;
                this.CompareType = t;
                this.Value = v;
            }
        }

        public static AnimatorStateTransition CreateTransition(AnimatorState from, AnimatorState to, params Condition[] conditions)
        {
            AnimatorStateTransition transition = CreateTransition(from, to, 0.00f, false, 0.0f);
            AddTransitionConditions(transition, conditions);
            return transition;
        }

        public static AnimatorStateTransition CreateTransition(AnimatorState from, AnimatorStateMachine to, params Condition[] conditions)
        {
            AnimatorStateTransition transition = CreateTransition(from, to, 0.01f, false, 0.0f);
            AddTransitionConditions(transition, conditions);
            return transition;
        }

        public static AnimatorTransition CreateTransition(AnimatorStateMachine from, AnimatorState to, AnimatorStateMachine parent, params Condition[] conditions)
        {
            AnimatorTransition newT = new AnimatorTransition();
            newT.destinationState = to;
            AddTransitionConditions(newT, conditions);
            AnimatorTransition[] transitions = parent.GetStateMachineTransitions(from);
            transitions = transitions.Append(newT).ToArray();
            parent.SetStateMachineTransitions(from, transitions);
            return newT;
        }

        public static AnimatorTransition CreateTransition(AnimatorStateMachine from, AnimatorStateMachine to, AnimatorStateMachine parent, params Condition[] conditions)
        {
            AnimatorTransition newT = new AnimatorTransition();
            newT.destinationStateMachine = to;
            AddTransitionConditions(newT, conditions);
            AnimatorTransition[] transitions = parent.GetStateMachineTransitions(from);
            transitions = transitions.Append(newT).ToArray();
            parent.SetStateMachineTransitions(from, transitions);
            return newT;
        }

        public static AnimatorStateTransition CreateTransition(AnimatorState from, AnimatorState to, float duration = 0.01f, bool hasExitTime = false, float exitTime = 0.1f, params Condition[] conditions)
        {
            AnimatorStateTransition transition = CreateTransition(from, to, duration, hasExitTime, exitTime);
            AddTransitionConditions(transition, conditions);
            return transition;
        }

        public static void AddTransitionConditions(AnimatorStateTransition transition, params Condition[] conditions)
        {
            foreach (Condition c in conditions)
            {
                if (c.Value.GetType() == typeof(float))
                {
                    if (c.CompareType == CompareType.LESS) transition.AddCondition(AnimatorConditionMode.Less, (float)c.Value, c.ParamName);
                    if (c.CompareType == CompareType.GREATER) transition.AddCondition(AnimatorConditionMode.Greater, (float)c.Value, c.ParamName);
                }
                else if (c.Value.GetType() == typeof(int))
                {
                    if (c.CompareType == CompareType.LESS) transition.AddCondition(AnimatorConditionMode.Less, (int)c.Value, c.ParamName);
                    if (c.CompareType == CompareType.GREATER) transition.AddCondition(AnimatorConditionMode.Greater, (int)c.Value, c.ParamName);
                    if (c.CompareType == CompareType.EQUAL) transition.AddCondition(AnimatorConditionMode.Equals, (int)c.Value, c.ParamName);
                    if (c.CompareType == CompareType.NOT_EQUAL) transition.AddCondition(AnimatorConditionMode.NotEqual, (int)c.Value, c.ParamName);
                }
                else if (c.Value.GetType() == typeof(bool))
                {
                    if ((bool)c.Value) transition.AddCondition(AnimatorConditionMode.If, 0, c.ParamName);
                    else transition.AddCondition(AnimatorConditionMode.IfNot, 0, c.ParamName);
                }
            }
        }

        public static void AddTransitionConditions(AnimatorTransition transition, params Condition[] conditions)
        {
            foreach (Condition c in conditions)
            {
                if (c.Value.GetType() == typeof(float))
                {
                    if (c.CompareType == CompareType.LESS) transition.AddCondition(AnimatorConditionMode.Less, (float)c.Value, c.ParamName);
                    if (c.CompareType == CompareType.GREATER) transition.AddCondition(AnimatorConditionMode.Greater, (float)c.Value, c.ParamName);
                }
                else if (c.Value.GetType() == typeof(int))
                {
                    if (c.CompareType == CompareType.LESS) transition.AddCondition(AnimatorConditionMode.Less, (int)c.Value, c.ParamName);
                    if (c.CompareType == CompareType.GREATER) transition.AddCondition(AnimatorConditionMode.Greater, (int)c.Value, c.ParamName);
                    if (c.CompareType == CompareType.EQUAL) transition.AddCondition(AnimatorConditionMode.Equals, (int)c.Value, c.ParamName);
                    if (c.CompareType == CompareType.NOT_EQUAL) transition.AddCondition(AnimatorConditionMode.NotEqual, (int)c.Value, c.ParamName);
                }
                else if (c.Value.GetType() == typeof(bool))
                {
                    if ((bool)c.Value) transition.AddCondition(AnimatorConditionMode.If, 0, c.ParamName);
                    else transition.AddCondition(AnimatorConditionMode.IfNot, 0, c.ParamName);
                }
            }
        }

        public static BlendTree CreateFloatCopyBlendTree(string directory, string name, string fromParamter, string toParameter)
        {
            AnimationClip neg = CreateClip(name + "_neg", ("", typeof(Animator), toParameter, NegativeOneCurveOneFrame));
            AnimationClip pos = CreateClip(name + "_pos", ("", typeof(Animator), toParameter, PositiveOneCurveOneFrame));
            BlendTree tree = new BlendTree();
            tree.useAutomaticThresholds = false;
            tree.AddChild(neg, -1);
            tree.AddChild(pos, 1);
            tree.blendParameter = fromParamter;
            string path = directory + "/" + name + ".asset";
            AssetDatabase.CreateAsset(tree, path);
            AssetDatabase.AddObjectToAsset(neg, path);
            AssetDatabase.AddObjectToAsset(pos, path);
            return tree;
        }

        // public static Dictionary<(GameObject, GameObject), string> savedPaths = new Dictionary<(GameObject, GameObject), string>();
        // public static string GetPath(GameObject sensor, GameObject avatar)
        // {
        //     if (savedPaths.ContainsKey((sensor, avatar))) return savedPaths[(sensor, avatar)];
        //     Transform o = sensor.transform.parent;
        //     List<Transform> path = new List<Transform>();
        //     while (o != avatar.transform && o != null)
        //     {
        //         path.Add(o);
        //         o = o.parent;
        //     }
        //     path.Reverse();
        //     System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //     foreach (Transform t in path)
        //     {
        //         sb.Append(t.name + "/");
        //     }
        //     sb.Append(sensor.name);
        //     string finalpath = sb.ToString();
        //     savedPaths.Add((sensor, avatar), finalpath);
        //     return finalpath;
        // }

        public static void SaveBlendTree(BlendTree tree, string directory, string name, bool deepTrees = false, params AnimationClip[] additionalClips)
        {
            string path = directory + "/" + name + ".asset";
            Helper.AssertFolderExists(directory);
            AssetDatabase.CreateAsset(tree, path);

            List<AnimationClip> allClips = new List<AnimationClip>();
            if (additionalClips != null && additionalClips.Length > 0)
            {
                allClips.AddRange(additionalClips);
            }
            allClips.AddRange(tree.children.Select(c => c.motion).Where(m => m is AnimationClip && m != null).Select(m => m as AnimationClip));
            if (deepTrees)
            {
                List<BlendTree> allTrees = new List<BlendTree>();
                GatherAllSubtrees(tree, allTrees);
                foreach (BlendTree t in allTrees.Distinct())
                {
                    AssetDatabase.AddObjectToAsset(t, path);
                    allClips.AddRange(t.children.Select(c => c.motion).Where(m => m is AnimationClip && m != null).Select(m => m as AnimationClip));
                }
            }

            foreach (AnimationClip c in allClips.Distinct())
                AssetDatabase.AddObjectToAsset(c, path);

            AssetDatabase.ImportAsset(path);
        }

        static void GatherAllSubtrees(BlendTree tree, List<BlendTree> trees)
        {
            foreach (BlendTree t in tree.children.Select(c => c.motion).Where(c => c is BlendTree && c != null))
            {
                trees.Add(t);
                GatherAllSubtrees(t, trees);
            }
        }
    }
}