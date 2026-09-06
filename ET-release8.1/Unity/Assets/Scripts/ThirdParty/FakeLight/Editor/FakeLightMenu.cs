using UnityEditor;
using UnityEngine;

namespace CenturyGameEditor.FakeLight
{
    using CenturyGame.FakeLight;

    public class FakeLightMenu
    {
        static Vector3 GetSpawnPosition()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                Camera camera = sceneView.camera;
                return camera.transform.position + camera.transform.forward * 5f;
            }
            return Vector3.zero;
        }
        
        [MenuItem("GameObject/Light/FakePointLight", false, 10)]
        static void CreatePointFakeLight()
        {
            GameObject go = new GameObject("FakePointLight");
            
            go.transform.position = GetSpawnPosition();
            go.AddComponent<FakeLight>();

            Selection.activeGameObject = go;

            if (Selection.activeTransform != null)
            {
                go.transform.parent = Selection.activeTransform;
            }

            Undo.RegisterCreatedObjectUndo(go, "Create Fake Point Light");
        }

        [MenuItem("GameObject/Light/FakeSpotLight", false, 10)]
        static void CreateSpotFakeLight()
        {
            GameObject go = new GameObject("FakeSpotLight");

            go.transform.position = GetSpawnPosition();
            go.AddComponent<CenturyGame.FakeLight.FakeLight>();
            go.GetComponent<FakeLight>().lightType = FakeLight.FakeLightType.Spot;
            Selection.activeGameObject = go;

            if (Selection.activeTransform != null)
            {
                go.transform.parent = Selection.activeTransform;
                go.transform.localRotation = Quaternion.Euler(90, 0, 0);
            }

            Undo.RegisterCreatedObjectUndo(go, "Create Fake Spot Light");
        }
    }
}
