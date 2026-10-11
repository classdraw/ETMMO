using UnityEditor;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;
using UnityEngine.Rendering;

public class SB_SimpleLitShaderGUI : ShaderGUI
{
    static readonly int SpecularHighlightsId = Shader.PropertyToID("_SpecularHighlights");
    static readonly int EnvironmentReflectionsId = Shader.PropertyToID("_EnvironmentReflections");

    MaterialProperty _specularHighlights;
    MaterialProperty _environmentReflections;
    MaterialProperty _baseMap;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        FindProperties(properties);

        EditorGUI.BeginChangeCheck();

        foreach (var prop in properties)
        {
            if (prop.name is "_SpecularHighlights" or "_EnvironmentReflections" or "_MainTex")
                continue;

            materialEditor.ShaderProperty(prop, prop.displayName);
        }

        if (_baseMap != null)
            materialEditor.TextureScaleOffsetProperty(_baseMap);

        EditorGUILayout.Space();
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
            if (_specularHighlights != null)
                materialEditor.ShaderProperty(_specularHighlights, LitGUI.Styles.highlightsText);
            if (_environmentReflections != null)
                materialEditor.ShaderProperty(_environmentReflections, LitGUI.Styles.reflectionsText);
        }

        if (EditorGUI.EndChangeCheck())
        {
            foreach (Material material in materialEditor.targets)
                SetMaterialKeywords(material);
        }
    }

    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
    {
        base.AssignNewShaderToMaterial(material, oldShader, newShader);
        SetMaterialKeywords(material);
    }

    public override void ValidateMaterial(Material material)
    {
        SetMaterialKeywords(material);
    }

    void FindProperties(MaterialProperty[] properties)
    {
        _specularHighlights = FindProperty("_SpecularHighlights", properties, false);
        _environmentReflections = FindProperty("_EnvironmentReflections", properties, false);
        _baseMap = FindProperty("_BaseMap", properties, false);
    }

    static void SetMaterialKeywords(Material material)
    {
        if (material.HasProperty(SpecularHighlightsId))
        {
            CoreUtils.SetKeyword(material, "_SPECULARHIGHLIGHTS_OFF",
                material.GetFloat("_SpecularHighlights") == 0.0f);
        }

        if (material.HasProperty(EnvironmentReflectionsId))
        {
            CoreUtils.SetKeyword(material, "_ENVIRONMENTREFLECTIONS_OFF",
                material.GetFloat("_EnvironmentReflections") == 0.0f);
        }

        if (material.HasProperty("_MainTex") && material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_MainTex", material.GetTexture("_BaseMap"));
            material.SetTextureScale("_MainTex", material.GetTextureScale("_BaseMap"));
            material.SetTextureOffset("_MainTex", material.GetTextureOffset("_BaseMap"));
        }
    }
}
