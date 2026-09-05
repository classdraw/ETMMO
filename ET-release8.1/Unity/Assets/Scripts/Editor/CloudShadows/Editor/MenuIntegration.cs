using UnityEngine;
using UnityEditor;

namespace SSCS {

    public class CloudShadowsEditorIntegration : MonoBehaviour {

        [MenuItem("GameObject/Effects/Cloud Shadows", false, 120)]
        public static void CreateFogVolume(MenuCommand menuCommand) {
            GameObject go = Resources.Load<GameObject>("SSCS/Prefabs/CloudShadows");
            go = Instantiate(go);
            go.name = "Cloud Shadows";
            Undo.RegisterCreatedObjectUndo(go, "Create Cloud Shadows");
            Selection.activeObject = go;
        }
    }
}