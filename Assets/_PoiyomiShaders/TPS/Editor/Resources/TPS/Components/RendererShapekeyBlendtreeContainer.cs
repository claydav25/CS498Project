using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Thry.TPS.Editors
{
    public class RendererShapekeyBlendtreeContainer : BindableElement
    {
        #region Uxml Stuff
        public new class UxmlFactory : UxmlFactory<RendererShapekeyBlendtreeContainer, UxmlTraits> { }


        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription label = new UxmlStringAttributeDescription() { name = "label", defaultValue = "Renderer" };
            UxmlStringAttributeDescription bindingPath = new UxmlStringAttributeDescription() { name = "binding-path" };
            UxmlStringAttributeDescription bindingPathList = new UxmlStringAttributeDescription() { name = "binding-path-list" };

            public override void Init(VisualElement visualElement, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(visualElement, bag, cc);
                var baseField = (RendererShapekeyBlendtreeContainer)visualElement;

                string labelValue = label.GetValueFromBag(bag, cc);
                baseField.RendererField.label = labelValue;
                baseField.label = labelValue;

                string bindingPathValue = bindingPath.GetValueFromBag(bag, cc);
                baseField.RendererField.bindingPath = bindingPathValue;
                baseField.bindingPath = bindingPathValue;

                string bindingPathListValue = bindingPathList.GetValueFromBag(bag, cc);
                baseField.BlendshapeList.bindingPath = bindingPathListValue;
                baseField.bindingPathList = bindingPathListValue;
            }
        }        
        
        public string bindingPathList { get; set; }
        public string label { get; set; }

        public RendererShapekeyBlendtreeContainer()
        {
            VisualTreeAsset asset = Resources.Load<VisualTreeAsset>("TPS/Components/RendererShapekeyBlendtreeContainer");
            asset.CloneTree(this);

            RendererField.label = label;
            RendererField.RegisterCallback<ChangeEvent<SkinnedMeshRenderer>>(HandleRendererChange);

            BlendshapeList.makeItem += () =>
            {
                var root = new VisualElement
                {
                    style = { flexDirection = FlexDirection.Row }
                };
                var nameLabel = new Label { name = "name" };
                var valueLabel = new Label { name = "value" };

                root.Add(nameLabel);
                root.Add(valueLabel);

                return root;
            };

            BlendshapeList.bindItem += (element, i) =>
            {
                ShapekeyConfig config = (ShapekeyConfig)BlendshapeList.itemsSource[i];
                element.Q<Label>("name").text = config.shapekeyName;
                element.Q<Label>("value").text = config.depth.ToString("G");
            };
        }
        #endregion

        ObjectField RendererField => this.Q<ObjectField>("renderer");
        ListView BlendshapeList => this.Q<ListView>("list");

        List<string> blendshapeNames;

        void HandleRendererChange(ChangeEvent<SkinnedMeshRenderer> evt)
        {
            if(evt.newValue == evt.previousValue)
                return;

            SkinnedMeshRenderer smr = evt.newValue;
            if(smr == null || smr.sharedMesh == null)
            {
                blendshapeNames = new List<string>();
                return;
            }

            Mesh selectedMesh = smr.sharedMesh;

            blendshapeNames = new List<string>(selectedMesh.blendShapeCount);
            for(int i = 0; i < selectedMesh.blendShapeCount; i++)
                blendshapeNames.Add(selectedMesh.GetBlendShapeName(i));
        }

        void SetupBlendshapeList(ListView list, SerializedProperty shapeKeyListProp)
        {
            /*
            list.itemsSource = shapeKeys;
            list.bindItem = (element, index) =>
            {
                ((Label)element).text = shapeKeys[index].shapekeyName;
            };
            list.makeItem = () => new Label();
            list.RegisterCallback<ChangeEvent<PropertyField>>(evt =>
            {
                shapeKeys[BlendshapeList.selectedIndex].shapekeyName = ((TextField)BlendshapeList.selectedItem).value;
            });
            */
        }
    }
}