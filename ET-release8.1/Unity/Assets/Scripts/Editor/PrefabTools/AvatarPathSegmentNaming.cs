using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ET
{
    /// <summary>
    /// Avatar 预制体工具路径段规范化（数字前缀、Armor 1、纯数字段等）。
    /// </summary>
    public static class AvatarPathSegmentNaming
    {
        private static readonly Regex OrderedFolderPrefix = new Regex(@"^\d+_(.+)$", RegexOptions.Compiled);

        private static readonly Regex TrailingSpaceDigitsSuffix = new Regex(@"\s+\d+$", RegexOptions.Compiled);

        private static readonly Regex SingleWordUnderscoreDigitsSuffix = new Regex(@"^(\p{L}+)_\d+$", RegexOptions.Compiled);

        public static string StripOrderedFolderPrefix(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                return folderName;
            }

            Match m = OrderedFolderPrefix.Match(folderName);
            return m.Success ? m.Groups[1].Value : folderName;
        }

        public static string NormalizePathSegmentForAvatar(string segment)
        {
            if (string.IsNullOrEmpty(segment))
            {
                return segment;
            }

            segment = TrailingSpaceDigitsSuffix.Replace(segment, string.Empty);
            Match m = SingleWordUnderscoreDigitsSuffix.Match(segment);
            if (m.Success)
            {
                segment = m.Groups[1].Value;
            }

            return segment;
        }

        public static List<string> ApplyAvatarStylePathNormalization(IReadOnlyList<string> filteredSegments)
        {
            var normalized = new List<string>(filteredSegments.Count);
            foreach (string s in filteredSegments)
            {
                normalized.Add(NormalizePathSegmentForAvatar(s));
            }

            normalized = RemoveNumericOnlyPathTokens(normalized);
            return CollapseConsecutiveDuplicateSegments(normalized);
        }

        private static List<string> CollapseConsecutiveDuplicateSegments(List<string> segments)
        {
            if (segments.Count <= 1)
            {
                return segments;
            }

            var list = new List<string>(segments.Count);
            foreach (string s in segments)
            {
                if (list.Count > 0 && list[list.Count - 1].Equals(s, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                list.Add(s);
            }

            return list;
        }

        private static bool IsNumericOnlyFolderToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            for (int i = 0; i < token.Length; i++)
            {
                if (!char.IsDigit(token[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static List<string> RemoveNumericOnlyPathTokens(List<string> segments)
        {
            var list = new List<string>(segments.Count);
            foreach (string s in segments)
            {
                if (IsNumericOnlyFolderToken(s))
                {
                    continue;
                }

                list.Add(s);
            }

            return list;
        }
    }
}
