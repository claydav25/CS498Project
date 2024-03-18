using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Poiyomi.ModularShaderSystem;

namespace Poi.Tools
{
    public class ModuleOrderListing : EditorWindow
    {
        [MenuItem("Poi/Tools/Modular Shader/Module Order Listing")]
        public static void mol()
        {
            GetWindow<ModuleOrderListing>().Show();
        }

        ModularShader modularShader;
        List<ShaderModule> modules = new List<ShaderModule>();
        private string[] keywords;
        private string[] visualKeywords;
        private Vector2 scrollPosition;
        private int selectedIndex;
        private bool showTemplates;
        private Color grey = new Color(0.85f, 0.85f, 0.85f, 1.0f);

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            modularShader = EditorGUILayout.ObjectField(modularShader, typeof(ModularShader), false, null) as ModularShader;
            if (EditorGUI.EndChangeCheck())
            {
                modules = new List<ShaderModule>();
                if (modularShader != null)
                {
                    scrollPosition = Vector2.zero;
                    modules = ShaderGenerator.FindAllModules(modularShader);
                    keywords = modules.SelectMany(module => module.Templates.SelectMany(mtemplate => mtemplate.Keywords)).Distinct().OrderBy(x => x).ToArray();
                    visualKeywords = keywords.Select(x => x.Replace("_", "/")).ToArray();
                    selectedIndex = 0;
                }
            }
            if (modules != null && modules.Count != 0)
            {
                showTemplates = EditorGUILayout.ToggleLeft("Show Template", showTemplates);
                selectedIndex = EditorGUILayout.Popup(selectedIndex, visualKeywords, GUILayout.ExpandWidth(true));
                using (new EditorGUILayout.ScrollViewScope(scrollPosition, GUILayout.ExpandWidth(true)))
                {
                    bool odd = false;
                    foreach (var module in modules)
                    {
                        foreach (var mtemplate in module.Templates)
                        {
                            if (mtemplate.Keywords.Contains(keywords[selectedIndex]))
                            {
                                GUI.color = odd ? grey : Color.white;
                                odd = !odd;
                                EditorGUILayout.BeginHorizontal();
                                GUI.enabled = false;
                                EditorGUILayout.ObjectField(module, typeof(ShaderModule), false, null);
                                if (showTemplates) EditorGUILayout.ObjectField(mtemplate.Template, typeof(TemplateAsset), false, null);
                                GUI.enabled = true;
                                EditorGUI.BeginChangeCheck();
                                int q = EditorGUILayout.IntField(mtemplate.Queue);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    mtemplate.Queue = q;
                                    EditorUtility.SetDirty(module);
                                }
                                EditorGUILayout.EndHorizontal();
                                // EditorGUILayout.Separator();
                            }
                        }
                    }
                }
            }
        }
    }
}
