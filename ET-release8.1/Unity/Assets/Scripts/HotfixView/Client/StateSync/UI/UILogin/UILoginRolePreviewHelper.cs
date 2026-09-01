using UnityEngine;

namespace ET.Client
{
    public static class UILoginRolePreviewHelper
    {
        public static void ApplyPreview(FrameSheetAnimPlayer player, RoleTextureComponent roleTex, ExternalDisplayAppearance appearance)
        {
            if (player == null || roleTex == null)
            {
                return;
            }

            Texture2D body = TryGetValidPartTexture(roleTex, FrameRolePartType.Body, appearance.BodyDisplayId, appearance.Race, appearance.Gender, appearance.BodyDisplayId);
            Texture2D head = TryGetValidPartTexture(roleTex, FrameRolePartType.Head, appearance.HeadDisplayId, appearance.Race, appearance.Gender, appearance.BodyDisplayId);
            Texture2D tail = TryGetValidPartTexture(roleTex, FrameRolePartType.Tail, appearance.TailDisplayId, appearance.Race, appearance.Gender, appearance.BodyDisplayId);
            Texture2D shirt = TryGetValidPartTexture(roleTex, FrameRolePartType.Shirt, appearance.ShirtDisplayId, appearance.Race, appearance.Gender, appearance.BodyDisplayId);
            Texture2D pants = TryGetValidPartTexture(roleTex, FrameRolePartType.Pants, appearance.PantsDisplayId, appearance.Race, appearance.Gender, appearance.BodyDisplayId);

            RedressAvatar redress = player.GetComponent<RedressAvatar>();
            if (redress == null)
            {
                redress = player.gameObject.AddComponent<RedressAvatar>();
            }

            redress.ApplyTextures(body, head, tail, shirt, pants);
        }

        private static Texture2D TryGetValidPartTexture(
            RoleTextureComponent roleTex,
            FrameRolePartType part,
            int displayId,
            int race,
            int gender,
            int bodyDisplayId)
        {
            if (!roleTex.IsPartDisplayValid(part, displayId, race, gender, bodyDisplayId))
            {
                return null;
            }

            roleTex.TryGetTexture(displayId, out Texture2D texture);
            return texture;
        }
    }
}

