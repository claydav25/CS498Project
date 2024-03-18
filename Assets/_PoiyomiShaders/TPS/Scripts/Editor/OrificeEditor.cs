#if UNITY_EDITOR && UNITY_2020_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using static Thry.TPS.Orifice;
using Object = UnityEngine.Object;
#if VRC_SDK_VRCSDK3 && !UDON
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Avatars.Components;
#endif

namespace Thry.TPS.Editors
{
    [CustomEditor(typeof(Orifice))]
    public class OrificeEditor : Editor
    {
        Orifice targetOrifice;
        VisualElement root;
        MultiColumnListView blendshapeList;

        List<DropdownField> dropdownFields;

        void OnEnable()
        {
            targetOrifice = (Orifice)target;
        }

        VisualElement subSectionManualSetup;
        VisualElement subSectionShapeKeys;

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();
            Resources.Load<VisualTreeAsset>("TPS/TPS_Editor_Orifice").CloneTree(root);

            root.Q<Label>("label_version").text = "v" + TPSComponent.TPSVersion;

            subSectionShapeKeys = root.Q<VisualElement>("subsection_shapekeys");
            var toggleSetupAnimator = root.Q<Toggle>("toggle_setup-animator");
            toggleSetupAnimator.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                subSectionShapeKeys.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });
            subSectionShapeKeys.style.display = toggleSetupAnimator.value ? DisplayStyle.Flex : DisplayStyle.None;

            subSectionManualSetup = root.Q<VisualElement>("subsection_manual-setup");
            var toggleAutoSetup = root.Q<Toggle>("toggle_automatic-setup");
            toggleAutoSetup.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                subSectionManualSetup.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex;
            });
            subSectionManualSetup.style.display = targetOrifice.SetupAutomatically ? DisplayStyle.None : DisplayStyle.Flex;

#if VRC_SDK_VRCSDK3 && !UDON
            root.Q<ObjectField>("object_renderer").RegisterValueChangedCallback(HandleRendererChange);
            SetupBlendshapeList();
#endif
            return root;
        }

#if VRC_SDK_VRCSDK3 && !UDON
        void SetupBlendshapeList()
        {
            blendshapeList = root.Q<MultiColumnListView>("list_blendshapes");
            blendshapeList.itemsSource = targetOrifice.OpeningShapekeys;
            var targetList = targetOrifice.OpeningShapekeys;

            dropdownFields = new List<DropdownField>();
            SerializedProperty listProp = serializedObject.FindProperty(nameof(targetOrifice.OpeningShapekeys));

            // Name column
            blendshapeList.columns.Add(new Column
            {
                title = "Blendshape Name",
                minWidth = new Length(160, LengthUnit.Pixel),
                stretchable = true,
                resizable = false,
                makeCell = () =>
                {
                    var cellName = new TextField { name = "name", multiline = false, style = { flexGrow = 1 } };
                    var cellRoot = new VisualElement { style = { flexDirection = FlexDirection.Row } };

                    var popup = new DropdownField
                    {
                        choices = blendshapeNames,
                        style =
                        {
                            maxWidth = 60, paddingLeft = 0, marginRight = 2, paddingRight = 2,
                            display = blendshapeNames.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None
                        },
                    };
                    cellRoot.Add(popup);


                    cellRoot.Add(cellName);
                    return cellRoot;
                },
                bindCell = (element, i) =>
                {
                    serializedObject.UpdateIfRequiredOrScript();

                    var textField = element.Q<TextField>("name");
                    var nameProp = listProp.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(ShapekeyConfig.shapekeyName));
                    textField.BindProperty(nameProp);

                    var popup = element.Q<DropdownField>();
                    var cellName = element.Q<TextField>();
                    popup.RegisterCallback<ChangeEvent<string>>(evt =>
                    {
                        if(string.IsNullOrWhiteSpace(evt.newValue) || evt.newValue == evt.previousValue)
                            return;
                        cellName.value = evt.newValue;
                        popup.SetValueWithoutNotify(null);
                    });

                    if(i < dropdownFields.Count)
                        dropdownFields[i] = popup;
                    else
                        dropdownFields.Add(popup);
                },
                unbindCell = (element, i) =>
                {
                    element.Unbind();
                },
            });

            // Value column
            blendshapeList.columns.Add(new Column
            {
                title = "Normalized Value",
                width = 120,
                resizable = false,
                makeCell = () =>
                {
                    return new FloatField
                    {
                        name = "value",
                        style =
                        {
                            flexGrow = 1,
                            maxWidth = new Length { value = 90, unit = LengthUnit.Percent }
                        }
                    };
                },

                bindCell = (element, i) =>
                {
                    serializedObject.UpdateIfRequiredOrScript();
                    var floatField = (FloatField)element;
                    var floatProp = listProp.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(ShapekeyConfig.depth));
                    floatField.BindProperty(floatProp);
                },
                unbindCell = (element, i) => element.Unbind(),
            });
        }

        List<string> blendshapeNames = new List<string>();
        void HandleRendererChange(ChangeEvent<Object> evt)
        {
            var newRenderer = evt.newValue as SkinnedMeshRenderer;
            var previousRenderer = evt.previousValue as SkinnedMeshRenderer;

            if(newRenderer == previousRenderer)
                return;

            if(newRenderer == null || newRenderer.sharedMesh == null)
            {
                blendshapeNames = new List<string>();
                return;
            }

            Mesh selectedMesh = newRenderer.sharedMesh;
            blendshapeNames = new List<string>(selectedMesh.blendShapeCount);
            for(int i = 0; i < selectedMesh.blendShapeCount; i++)
                blendshapeNames.Add(selectedMesh.GetBlendShapeName(i));

            DisplayStyle displayStyle = blendshapeNames.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            foreach(var dropdown in dropdownFields)
            {
                dropdown.choices = blendshapeNames;
                dropdown.style.display = displayStyle;
            }
        }
#endif
	}
}
#endif