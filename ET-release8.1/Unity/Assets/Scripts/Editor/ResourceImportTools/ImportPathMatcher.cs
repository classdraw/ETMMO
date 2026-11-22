using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ET
{
    public static class ImportPathMatcher
    {
        public static bool IsWhitelisted(string assetPath, string pathWhitelist)
        {
            if (string.IsNullOrWhiteSpace(assetPath) || string.IsNullOrWhiteSpace(pathWhitelist))
            {
                return false;
            }

            var normalized = assetPath.Replace('\\', '/');
            var tokens = pathWhitelist.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i].Trim();
                if (string.IsNullOrEmpty(token))
                {
                    continue;
                }

                if (normalized.IndexOf("/" + token + "/", StringComparison.OrdinalIgnoreCase) >= 0
                    || normalized.StartsWith(token + "/", StringComparison.OrdinalIgnoreCase)
                    || normalized.EndsWith("/" + token, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(Path.GetFileName(normalized), token, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsIgnoredByGroup(string assetPath, ImageImportTemplate group)
        {
            if (group?.ignoreFolders == null || group.ignoreFolders.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < group.ignoreFolders.Length; i++)
            {
                var folder = ResourceImportApplier.NormalizeAssetFolder(group.ignoreFolders[i]);
                if (ResourceImportApplier.IsUnderFolder(assetPath, folder))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool RuleMatches(string assetPath, ImageImportSpecialRule rule)
        {
            if (rule == null || string.IsNullOrWhiteSpace(rule.pattern))
            {
                return false;
            }

            var normalizedPath = assetPath.Replace('\\', '/');
            var fileName = Path.GetFileName(normalizedPath);
            var pattern = rule.pattern.Trim();

            switch (rule.matchKind)
            {
                case ImportPathMatchKind.FilenameStartsWith:
                    return fileName.StartsWith(pattern, StringComparison.OrdinalIgnoreCase);
                case ImportPathMatchKind.FilenameContains:
                    return fileName.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
                case ImportPathMatchKind.FilenameEndsWith:
                    return fileName.EndsWith(pattern, StringComparison.OrdinalIgnoreCase);
                case ImportPathMatchKind.FilenameEqual:
                    return string.Equals(fileName, pattern, StringComparison.OrdinalIgnoreCase);
                case ImportPathMatchKind.DirectoryContains:
                    return normalizedPath.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
                case ImportPathMatchKind.DirectoryStartsWith:
                    return normalizedPath.StartsWith(pattern, StringComparison.OrdinalIgnoreCase);
                case ImportPathMatchKind.DirectoryEndsWith:
                    return normalizedPath.EndsWith(pattern, StringComparison.OrdinalIgnoreCase);
                case ImportPathMatchKind.DirectoryEqual:
                    return string.Equals(normalizedPath, pattern.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
                case ImportPathMatchKind.PathRegex:
                    try
                    {
                        return Regex.IsMatch(normalizedPath, pattern, RegexOptions.IgnoreCase);
                    }
                    catch (ArgumentException)
                    {
                        return false;
                    }
                default:
                    return false;
            }
        }

        public static string GetMatchKindDisplayName(ImportPathMatchKind kind)
        {
            switch (kind)
            {
                case ImportPathMatchKind.FilenameStartsWith: return "Filename Starts With";
                case ImportPathMatchKind.FilenameContains: return "Filename Contains";
                case ImportPathMatchKind.FilenameEndsWith: return "Filename Ends With";
                case ImportPathMatchKind.DirectoryContains: return "Directory Contains";
                case ImportPathMatchKind.PathRegex: return "Path Regex";
                case ImportPathMatchKind.FilenameEqual: return "Filename Equal";
                case ImportPathMatchKind.DirectoryStartsWith: return "Directory Starts With";
                case ImportPathMatchKind.DirectoryEndsWith: return "Directory Ends With";
                case ImportPathMatchKind.DirectoryEqual: return "Directory Equal";
                default: return kind.ToString();
            }
        }
    }
}