using UnityEngine;

namespace ET.Client
{
    public static class LoginRolePreviewAnchorHelper
    {
        public static Transform FindLeftAnchor(GameObject uiLoginRoot, GameObject boundAnchor)
        {
            return FindAnchor(uiLoginRoot, boundAnchor, "RoleLeft", "m_goUIRoleLeft");
        }

        public static Transform FindRightAnchor(GameObject uiLoginRoot, GameObject boundAnchor)
        {
            return FindAnchor(uiLoginRoot, boundAnchor, "RoleRight", "m_goUIRoleRight");
        }

        private static Transform FindAnchor(GameObject uiLoginRoot, GameObject boundAnchor, params string[] names)
        {
            if (boundAnchor != null)
            {
                return boundAnchor.transform;
            }

            if (uiLoginRoot == null)
            {
                return null;
            }

            Transform[] transforms = uiLoginRoot.GetComponentsInChildren<Transform>(true);
            for (int n = 0; n < names.Length; n++)
            {
                string targetName = names[n];
                for (int i = 0; i < transforms.Length; i++)
                {
                    Transform t = transforms[i];
                    if (t != null && t.name == targetName)
                    {
                        return t;
                    }
                }
            }

            return null;
        }
    }
}
