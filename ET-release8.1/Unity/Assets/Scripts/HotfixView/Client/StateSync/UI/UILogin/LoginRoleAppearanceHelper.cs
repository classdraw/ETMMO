using System.Collections.Generic;

namespace ET.Client
{
    public static class LoginRoleAppearanceHelper
    {
        public const int DefaultRace = (int)FrameRoleRaceType.Human;
        public const int DefaultGender = (int)FrameRoleGenderType.Male;

        public static void InitDefault(RoleTextureComponent roleTex, ref LoginRoleAppearance appearance)
        {
            appearance.Race = DefaultRace;
            appearance.Gender = DefaultGender;
            ValidatePartSelections(roleTex, ref appearance);
        }

        public static void ValidatePartSelections(RoleTextureComponent roleTex, ref LoginRoleAppearance appearance)
        {
            appearance.BodyDisplayId = FixPartDisplayId(roleTex, FrameRolePartType.Body, appearance.Race, appearance.Gender, appearance.BodyDisplayId);
            appearance.HeadDisplayId = FixPartDisplayId(roleTex, FrameRolePartType.Head, appearance.Race, appearance.Gender, appearance.HeadDisplayId);
            appearance.TailDisplayId = FixPartDisplayId(roleTex, FrameRolePartType.Tail, appearance.Race, appearance.Gender, appearance.TailDisplayId);
            appearance.ShirtDisplayId = FixPartDisplayId(roleTex, FrameRolePartType.Shirt, appearance.Race, appearance.Gender, appearance.ShirtDisplayId);
            appearance.PantsDisplayId = FixPartDisplayId(roleTex, FrameRolePartType.Pants, appearance.Race, appearance.Gender, appearance.PantsDisplayId);
        }

        public static void CycleRace(RoleTextureComponent roleTex, ref LoginRoleAppearance appearance, int delta)
        {
            List<int> races = roleTex.GetAvailableRaces();
            if (races.Count == 0)
            {
                return;
            }

            appearance.Race = CycleValue(races, appearance.Race, delta);
            List<int> genders = roleTex.GetAvailableGenders(appearance.Race);
            if (genders.Count > 0 && !genders.Contains(appearance.Gender))
            {
                appearance.Gender = genders[0];
            }

            ValidatePartSelections(roleTex, ref appearance);
        }

        public static void CycleGender(RoleTextureComponent roleTex, ref LoginRoleAppearance appearance, int delta)
        {
            List<int> genders = roleTex.GetAvailableGenders(appearance.Race);
            if (genders.Count == 0)
            {
                return;
            }

            appearance.Gender = CycleValue(genders, appearance.Gender, delta);
            ValidatePartSelections(roleTex, ref appearance);
        }

        public static void CyclePart(RoleTextureComponent roleTex, ref LoginRoleAppearance appearance, FrameRolePartType part, int delta)
        {
            int current = GetPartDisplayId(ref appearance, part);
            List<int> ids = roleTex.GetPartDisplayIds(part, appearance.Race, appearance.Gender);
            if (ids.Count == 0)
            {
                SetPartDisplayId(ref appearance, part, 0);
                return;
            }

            SetPartDisplayId(ref appearance, part, CycleValue(ids, current, delta));
        }

        private static int FixPartDisplayId(RoleTextureComponent roleTex, FrameRolePartType part, int race, int gender, int currentDisplayId)
        {
            List<int> ids = roleTex.GetPartDisplayIds(part, race, gender);
            if (ids.Count == 0)
            {
                return 0;
            }

            if (currentDisplayId > 0 && ids.Contains(currentDisplayId))
            {
                return currentDisplayId;
            }

            if (currentDisplayId > 0 && FrameRoleTextureId.TryDecode(currentDisplayId, out _, out _, out _, out int index))
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    if (FrameRoleTextureId.TryDecode(ids[i], out _, out _, out _, out int candidateIndex) && candidateIndex == index)
                    {
                        return ids[i];
                    }
                }
            }

            return ids[0];
        }

        private static int CycleValue(List<int> values, int current, int delta)
        {
            if (values == null || values.Count == 0)
            {
                return current;
            }

            int index = values.IndexOf(current);
            if (index < 0)
            {
                index = 0;
            }
            else
            {
                index = (index + delta) % values.Count;
                if (index < 0)
                {
                    index += values.Count;
                }
            }

            return values[index];
        }

        private static int GetPartDisplayId(ref LoginRoleAppearance appearance, FrameRolePartType part)
        {
            return part switch
            {
                FrameRolePartType.Body => appearance.BodyDisplayId,
                FrameRolePartType.Head => appearance.HeadDisplayId,
                FrameRolePartType.Tail => appearance.TailDisplayId,
                FrameRolePartType.Shirt => appearance.ShirtDisplayId,
                FrameRolePartType.Pants => appearance.PantsDisplayId,
                _ => 0,
            };
        }

        private static void SetPartDisplayId(ref LoginRoleAppearance appearance, FrameRolePartType part, int displayId)
        {
            switch (part)
            {
                case FrameRolePartType.Body:
                    appearance.BodyDisplayId = displayId;
                    break;
                case FrameRolePartType.Head:
                    appearance.HeadDisplayId = displayId;
                    break;
                case FrameRolePartType.Tail:
                    appearance.TailDisplayId = displayId;
                    break;
                case FrameRolePartType.Shirt:
                    appearance.ShirtDisplayId = displayId;
                    break;
                case FrameRolePartType.Pants:
                    appearance.PantsDisplayId = displayId;
                    break;
            }
        }
    }
}
