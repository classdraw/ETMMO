using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace ET
{
    public static class UnitPartSlotCollectionExcelExporter
    {
        public static bool ExportToExternalDisplay(out string message)
        {
            string collectionPath = UnitPartSlotCollectionBuilder.OutputAssetPath;
            UnitPartSlotCollectionConfig config = AssetDatabase.LoadAssetAtPath<UnitPartSlotCollectionConfig>(collectionPath);
            if (config == null)
            {
                message = "请先收集并生成：" + collectionPath;
                return false;
            }

            config.RebuildLookup();

            string excelPath = Path.GetFullPath(ExternalDisplayExcelWriter.DefaultExcelAssetPath);
            if (!File.Exists(excelPath))
            {
                message = "找不到 Excel：" + ExternalDisplayExcelWriter.DefaultExcelAssetPath;
                return false;
            }

            List<UnitPartSlotExportRow> rows = BuildRows(config);
            ExternalDisplayExcelWriteResult result = ExternalDisplayExcelWriter.WriteUnitPartSlotRows(excelPath, rows);
            AssetDatabase.Refresh();

            message = rows.Count == 0
                ? $"导出完成：已清空 Excel 旧数据，当前无 Item 可写入。\n{ExternalDisplayExcelWriter.DefaultExcelAssetPath}"
                : $"导出完成：已清空旧数据并写入 {result.WrittenCount} 条。\n{ExternalDisplayExcelWriter.DefaultExcelAssetPath}";
            return true;
        }

        private static List<UnitPartSlotExportRow> BuildRows(UnitPartSlotCollectionConfig config)
        {
            var rows = new List<UnitPartSlotExportRow>();
            if (config.groups == null)
            {
                return rows;
            }

            for (int g = 0; g < config.groups.Count; g++)
            {
                UnitPartSlotGroup group = config.groups[g];
                if (group?.items == null)
                {
                    continue;
                }

                int partSlotKey = group.partSlotKey;
                string slotChinese = UnitPartSlotNames.GetChineseName(partSlotKey);

                for (int i = 0; i < group.items.Count; i++)
                {
                    UnitPartSlotItem item = group.items[i];
                    if (item == null || item.id <= 0)
                    {
                        continue;
                    }

                    string spriteName = string.IsNullOrEmpty(item.spriteName) ? string.Empty : item.spriteName;
                    string desc = slotChinese + item.id;
                    rows.Add(new UnitPartSlotExportRow(item.id, partSlotKey, spriteName, desc));
                }
            }

            rows.Sort((a, b) => a.Id.CompareTo(b.Id));
            return rows;
        }
    }
}
