using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ET
{
    public readonly struct ExternalDisplayExportRow
    {
        public int DisplayId { get; }
        public int Gender { get; }
        public int Race { get; }
        public string Name { get; }
        public string Desc { get; }
        public int PartKey { get; }
        public int BodyType { get; }
        public int NeedBodyType { get; }

        public ExternalDisplayExportRow(
            int displayId,
            int gender,
            int race,
            string name,
            string desc = "",
            int partKey = 0,
            int bodyType = 0,
            int needBodyType = 0)
        {
            DisplayId = displayId;
            Gender = gender;
            Race = race;
            Name = name ?? string.Empty;
            Desc = desc ?? string.Empty;
            PartKey = partKey;
            BodyType = bodyType;
            NeedBodyType = needBodyType;
        }
    }

    public readonly struct ExternalDisplayExcelAppendResult
    {
        public int AddedCount { get; }
        public int UpdatedCount { get; }
        public int NextIdStart { get; }

        public ExternalDisplayExcelAppendResult(int addedCount, int updatedCount, int nextIdStart)
        {
            AddedCount = addedCount;
            UpdatedCount = updatedCount;
            NextIdStart = nextIdStart;
        }
    }

    /// <summary>
    /// 向 ExternalDisplayConfig.xlsx 追加数据行，不改动表头与列结构。
    /// </summary>
    public static class ExternalDisplayExcelWriter
    {
        private const int DataStartRow = 6;
        private const int ColId = 3;
        private const int ColDisplayId = 4;
        private const int ColGender = 5;
        private const int ColRace = 6;
        private const int ColName = 7;
        private const int ColPartKey = 8;
        private const int ColBodyType = 9;
        private const int ColNeedBodyType = 10;
        private const int ColDesc = 11;

        private static readonly XNamespace Ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private const string SheetEntryPath = "xl/worksheets/sheet1.xml";
        private const string SharedStringsEntryPath = "xl/sharedStrings.xml";

        public static ExternalDisplayExcelAppendResult AppendRows(string xlsxPath, IReadOnlyList<ExternalDisplayExportRow> rows)
        {
            if (string.IsNullOrWhiteSpace(xlsxPath) || !File.Exists(xlsxPath))
            {
                throw new FileNotFoundException("找不到 Excel 文件。", xlsxPath);
            }

            if (rows == null || rows.Count == 0)
            {
                return new ExternalDisplayExcelAppendResult(0, 0, ReadNextId(xlsxPath));
            }

            byte[] sheetBytes = ReadZipEntry(xlsxPath, SheetEntryPath);
            byte[] sharedStringBytes = ReadZipEntry(xlsxPath, SharedStringsEntryPath);

            SharedStringTable sharedStrings = SharedStringTable.Load(sharedStringBytes);
            XDocument sheetDoc = XDocument.Load(new MemoryStream(sheetBytes), LoadOptions.PreserveWhitespace);
            XElement sheetData = sheetDoc.Root?.Element(Ns + "sheetData")
                ?? throw new InvalidOperationException("sheet1.xml 缺少 sheetData。");

            Dictionary<int, XElement> rowMap = sheetData.Elements(Ns + "row")
                .Select(row => new KeyValuePair<int, XElement>(ReadRowNumber(row), row))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            EnsurePartKeyBodyTypeLayout(sheetData, rowMap, sharedStrings);

            Dictionary<int, List<int>> displayIdToRows = new Dictionary<int, List<int>>();
            int maxId = 1000;
            int maxUsedRow = DataStartRow - 1;

            foreach (KeyValuePair<int, XElement> pair in rowMap.OrderBy(pair => pair.Key))
            {
                if (pair.Key < DataStartRow)
                {
                    continue;
                }

                string idText = ReadCellText(pair.Value, ColId, pair.Key, sharedStrings);
                if (int.TryParse(idText, out int id) && id > maxId)
                {
                    maxId = id;
                }

                string displayText = ReadCellText(pair.Value, ColDisplayId, pair.Key, sharedStrings);
                if (int.TryParse(displayText, out int displayId))
                {
                    if (!displayIdToRows.TryGetValue(displayId, out List<int> rowNumbers))
                    {
                        rowNumbers = new List<int>();
                        displayIdToRows[displayId] = rowNumbers;
                    }

                    rowNumbers.Add(pair.Key);
                    maxUsedRow = Math.Max(maxUsedRow, pair.Key);
                }
            }

            int nextId = maxId + 1;
            int added = 0;
            int updated = 0;
            int writeRow = FindFirstEmptyDataRow(rowMap, maxUsedRow);

            for (int i = 0; i < rows.Count; i++)
            {
                ExternalDisplayExportRow row = rows[i];
                if (displayIdToRows.TryGetValue(row.DisplayId, out List<int> existingRows))
                {
                    for (int r = 0; r < existingRows.Count; r++)
                    {
                        int existingRow = existingRows[r];
                        if (!rowMap.TryGetValue(existingRow, out XElement rowElement))
                        {
                            continue;
                        }

                        WriteRowFields(rowElement, existingRow, row, sharedStrings, assignId: null);
                        updated++;
                    }

                    continue;
                }

                if (!rowMap.TryGetValue(writeRow, out XElement newRowElement))
                {
                    newRowElement = CreateRowElement(writeRow);
                    sheetData.Add(newRowElement);
                    rowMap[writeRow] = newRowElement;
                }

                WriteRowFields(newRowElement, writeRow, row, sharedStrings, nextId);

                displayIdToRows[row.DisplayId] = new List<int> { writeRow };
                nextId++;
                added++;
                writeRow++;
                maxUsedRow = Math.Max(maxUsedRow, writeRow - 1);
            }

            UpdateDimension(sheetDoc, maxUsedRow);
            sheetBytes = SaveDocument(sheetDoc);
            sharedStringBytes = sharedStrings.Save();

            WriteZipEntry(xlsxPath, SheetEntryPath, sheetBytes);
            WriteZipEntry(xlsxPath, SharedStringsEntryPath, sharedStringBytes);

            return new ExternalDisplayExcelAppendResult(added, updated, nextId);
        }

        private static void WriteRowFields(
            XElement rowElement,
            int rowNumber,
            ExternalDisplayExportRow row,
            SharedStringTable sharedStrings,
            int? assignId)
        {
            if (assignId.HasValue)
            {
                WriteNumberCell(rowElement, ColId, rowNumber, assignId.Value);
            }

            WriteNumberCell(rowElement, ColDisplayId, rowNumber, row.DisplayId);
            WriteNumberCell(rowElement, ColGender, rowNumber, row.Gender);
            WriteNumberCell(rowElement, ColRace, rowNumber, row.Race);
            WriteStringCell(rowElement, ColName, rowNumber, row.Name, sharedStrings);
            WriteNumberCell(rowElement, ColPartKey, rowNumber, row.PartKey);
            WriteNumberCell(rowElement, ColBodyType, rowNumber, row.BodyType);
            WriteNumberCell(rowElement, ColNeedBodyType, rowNumber, row.NeedBodyType);
            WriteDescCell(rowElement, ColDesc, rowNumber, row.Desc, sharedStrings);

            EnsureRowSpans(rowElement);
            SortRowCells(rowElement);
        }

        private static void EnsurePartKeyBodyTypeLayout(
            XElement sheetData,
            Dictionary<int, XElement> rowMap,
            SharedStringTable sharedStrings)
        {
            if (!rowMap.TryGetValue(4, out XElement headerRow))
            {
                return;
            }

            string colHName = ReadCellText(headerRow, ColPartKey, 4, sharedStrings);
            if (colHName == "Desc")
            {
                WritePartKeyBodyTypeHeaders(sheetData, rowMap, sharedStrings);
                MigrateLegacyDescColumn(rowMap, sharedStrings);
            }
            else
            {
                string colJName = ReadCellText(headerRow, ColNeedBodyType, 4, sharedStrings);
                if (colJName == "Desc")
                {
                    MigrateDescToNeedBodyTypeColumn(rowMap, sharedStrings);
                }

                if (colHName != "PartKey" && colHName != "Part")
                {
                    WritePartKeyBodyTypeHeaders(sheetData, rowMap, sharedStrings);
                }
                else if (colJName != "NeedBodyType")
                {
                    WriteNeedBodyTypeHeaders(sheetData, rowMap, sharedStrings);
                }
            }

            FixNumericColumnStyles(rowMap);
        }

        private static void WriteNeedBodyTypeHeaders(
            XElement sheetData,
            Dictionary<int, XElement> rowMap,
            SharedStringTable sharedStrings)
        {
            WriteHeaderStringCell(sheetData, rowMap, 3, ColNeedBodyType, "是否限制体型", sharedStrings);
            WriteHeaderStringCell(sheetData, rowMap, 4, ColNeedBodyType, "NeedBodyType", sharedStrings);
            WriteHeaderStringCell(sheetData, rowMap, 5, ColNeedBodyType, "int", sharedStrings);
        }

        private static void MigrateDescToNeedBodyTypeColumn(
            Dictionary<int, XElement> rowMap,
            SharedStringTable sharedStrings)
        {
            foreach (KeyValuePair<int, XElement> pair in rowMap)
            {
                if (pair.Key < DataStartRow)
                {
                    continue;
                }

                string desc = ReadCellText(pair.Value, ColNeedBodyType, pair.Key, sharedStrings);
                WriteNumberCell(pair.Value, ColNeedBodyType, pair.Key, 0);
                WriteDescCell(pair.Value, ColDesc, pair.Key, desc, sharedStrings);
                EnsureRowSpans(pair.Value);
                SortRowCells(pair.Value);
            }
        }

        private static void WritePartKeyBodyTypeHeaders(
            XElement sheetData,
            Dictionary<int, XElement> rowMap,
            SharedStringTable sharedStrings)
        {
            WriteHeaderStringCell(sheetData, rowMap, 3, ColPartKey, "部位", sharedStrings);
            WriteHeaderStringCell(sheetData, rowMap, 3, ColBodyType, "体型", sharedStrings);
            WriteHeaderStringCell(sheetData, rowMap, 3, ColNeedBodyType, "是否限制体型", sharedStrings);
            WriteHeaderStringCell(sheetData, rowMap, 3, ColDesc, "描述", sharedStrings);
            WriteHeaderStringCell(sheetData, rowMap, 4, ColPartKey, "PartKey", sharedStrings);
            WriteHeaderStringCell(sheetData, rowMap, 4, ColBodyType, "BodyType", sharedStrings);
            WriteHeaderStringCell(sheetData, rowMap, 4, ColNeedBodyType, "NeedBodyType", sharedStrings);
            WriteHeaderStringCell(sheetData, rowMap, 4, ColDesc, "Desc", sharedStrings);
            WriteHeaderStringCell(sheetData, rowMap, 5, ColPartKey, "int", sharedStrings);
            WriteHeaderStringCell(sheetData, rowMap, 5, ColBodyType, "int", sharedStrings);
            WriteHeaderStringCell(sheetData, rowMap, 5, ColNeedBodyType, "int", sharedStrings);
            WriteHeaderStringCell(sheetData, rowMap, 5, ColDesc, "string", sharedStrings);

            if (rowMap.TryGetValue(3, out XElement row3))
            {
                EnsureRowSpans(row3);
            }

            if (rowMap.TryGetValue(4, out XElement row4))
            {
                EnsureRowSpans(row4);
            }

            if (rowMap.TryGetValue(5, out XElement row5))
            {
                EnsureRowSpans(row5);
            }
        }

        private static void MigrateLegacyDescColumn(Dictionary<int, XElement> rowMap, SharedStringTable sharedStrings)
        {
            foreach (KeyValuePair<int, XElement> pair in rowMap)
            {
                if (pair.Key < DataStartRow)
                {
                    continue;
                }

                string desc = ReadCellText(pair.Value, ColPartKey, pair.Key, sharedStrings);
                string displayText = ReadCellText(pair.Value, ColDisplayId, pair.Key, sharedStrings);
                int partKey = 0;
                if (int.TryParse(displayText, out int displayId))
                {
                    partKey = displayId / 10000000;
                }

                WriteNumberCell(pair.Value, ColPartKey, pair.Key, partKey);
                WriteNumberCell(pair.Value, ColBodyType, pair.Key, 0);
                WriteNumberCell(pair.Value, ColNeedBodyType, pair.Key, 0);
                WriteDescCell(pair.Value, ColDesc, pair.Key, desc, sharedStrings);
                EnsureRowSpans(pair.Value);
                SortRowCells(pair.Value);
            }
        }

        private static void WriteHeaderStringCell(
            XElement sheetData,
            Dictionary<int, XElement> rowMap,
            int rowNumber,
            int col,
            string value,
            SharedStringTable sharedStrings)
        {
            if (!rowMap.TryGetValue(rowNumber, out XElement rowElement))
            {
                rowElement = new XElement(Ns + "row", new XAttribute("r", rowNumber), new XAttribute("s", "3"), new XAttribute("spans", "3:11"));
                sheetData.Add(rowElement);
                rowMap[rowNumber] = rowElement;
            }

            WriteStringCell(rowElement, col, rowNumber, value, sharedStrings);
        }

        private static void FixNumericColumnStyles(Dictionary<int, XElement> rowMap)
        {
            foreach (KeyValuePair<int, XElement> pair in rowMap)
            {
                if (pair.Key < DataStartRow)
                {
                    continue;
                }

                FixNumericCellStyle(pair.Value, ColPartKey, pair.Key);
                FixNumericCellStyle(pair.Value, ColBodyType, pair.Key);
                FixNumericCellStyle(pair.Value, ColNeedBodyType, pair.Key);
            }
        }

        private static void FixNumericCellStyle(XElement rowElement, int col, int rowNumber)
        {
            string cellRef = CellRef(col, rowNumber);
            XElement cell = rowElement.Elements(Ns + "c").FirstOrDefault(element => (string)element.Attribute("r") == cellRef);
            if (cell == null || cell.Attribute("t") != null)
            {
                return;
            }

            cell.SetAttributeValue("s", "1");
        }

        private static int ReadNextId(string xlsxPath)
        {
            byte[] sheetBytes = ReadZipEntry(xlsxPath, SheetEntryPath);
            byte[] sharedStringBytes = ReadZipEntry(xlsxPath, SharedStringsEntryPath);
            SharedStringTable sharedStrings = SharedStringTable.Load(sharedStringBytes);
            XDocument sheetDoc = XDocument.Load(new MemoryStream(sheetBytes));
            XElement sheetData = sheetDoc.Root?.Element(Ns + "sheetData");
            if (sheetData == null)
            {
                return 1001;
            }

            int maxId = 1000;
            foreach (XElement row in sheetData.Elements(Ns + "row"))
            {
                int rowNumber = ReadRowNumber(row);
                if (rowNumber < DataStartRow)
                {
                    continue;
                }

                string idText = ReadCellText(row, ColId, rowNumber, sharedStrings);
                if (int.TryParse(idText, out int id) && id > maxId)
                {
                    maxId = id;
                }
            }

            return maxId + 1;
        }

        private static int FindFirstEmptyDataRow(Dictionary<int, XElement> rowMap, int maxUsedRow)
        {
            for (int row = DataStartRow; row <= maxUsedRow; row++)
            {
                if (!rowMap.TryGetValue(row, out XElement rowElement))
                {
                    return row;
                }

                string idText = ReadCellText(rowElement, ColId, row, SharedStringTable.Empty);
                if (string.IsNullOrWhiteSpace(idText))
                {
                    return row;
                }
            }

            return maxUsedRow + 1;
        }

        private static XElement CreateRowElement(int rowNumber)
        {
            return new XElement(
                Ns + "row",
                new XAttribute("r", rowNumber),
                new XAttribute("s", "1"),
                new XAttribute("customFormat", "1"),
                new XAttribute("spans", "3:11"));
        }

        private static void WriteNumberCell(XElement rowElement, int col, int rowNumber, int value)
        {
            XElement cell = GetOrCreateCell(rowElement, col, rowNumber, "1");
            cell.SetAttributeValue("r", CellRef(col, rowNumber));
            cell.SetAttributeValue("s", "1");
            cell.Attributes("t").Remove();
            cell.Elements(Ns + "v").Remove();
            cell.Add(new XElement(Ns + "v", value.ToString()));
        }

        private static void WriteStringCell(XElement rowElement, int col, int rowNumber, string value, SharedStringTable sharedStrings)
        {
            XElement cell = GetOrCreateCell(rowElement, col, rowNumber, "1");
            cell.SetAttributeValue("r", CellRef(col, rowNumber));
            cell.SetAttributeValue("s", "1");
            cell.SetAttributeValue("t", "s");
            cell.Elements(Ns + "v").Remove();
            cell.Add(new XElement(Ns + "v", sharedStrings.Add(value ?? string.Empty).ToString()));
        }

        private static void WriteDescCell(XElement rowElement, int col, int rowNumber, string value, SharedStringTable sharedStrings)
        {
            XElement cell = GetOrCreateCell(rowElement, col, rowNumber, "4");
            cell.SetAttributeValue("r", CellRef(col, rowNumber));
            cell.SetAttributeValue("s", "4");
            cell.Elements(Ns + "v").Remove();
            cell.Attributes("t").Remove();

            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            cell.SetAttributeValue("t", "s");
            cell.Add(new XElement(Ns + "v", sharedStrings.Add(value).ToString()));
        }

        private static void EnsureRowSpans(XElement rowElement)
        {
            rowElement.SetAttributeValue("spans", "3:11");
        }

        private static void SortRowCells(XElement rowElement)
        {
            List<XElement> cells = rowElement.Elements(Ns + "c").ToList();
            if (cells.Count <= 1)
            {
                return;
            }

            List<XElement> sorted = cells
                .OrderBy(cell => GetColumnIndex((string)cell.Attribute("r")))
                .ToList();

            foreach (XElement cell in cells)
            {
                cell.Remove();
            }

            foreach (XElement cell in sorted)
            {
                rowElement.Add(cell);
            }
        }

        private static int GetColumnIndex(string cellRef)
        {
            if (string.IsNullOrEmpty(cellRef))
            {
                return int.MaxValue;
            }

            int rowStart = 0;
            while (rowStart < cellRef.Length && char.IsLetter(cellRef[rowStart]))
            {
                rowStart++;
            }

            string letters = cellRef.Substring(0, rowStart);
            int col = 0;
            for (int i = 0; i < letters.Length; i++)
            {
                col = col * 26 + letters[i] - 'A' + 1;
            }

            return col;
        }

        private static XElement GetOrCreateCell(XElement rowElement, int col, int rowNumber, string style)
        {
            string cellRef = CellRef(col, rowNumber);
            XElement cell = rowElement.Elements(Ns + "c").FirstOrDefault(element => (string)element.Attribute("r") == cellRef);
            if (cell != null)
            {
                return cell;
            }

            cell = new XElement(Ns + "c", new XAttribute("r", cellRef), new XAttribute("s", style));
            rowElement.Add(cell);
            return cell;
        }

        private static string ReadCellText(XElement rowElement, int col, int rowNumber, SharedStringTable sharedStrings)
        {
            string cellRef = CellRef(col, rowNumber);
            XElement cell = rowElement.Elements(Ns + "c").FirstOrDefault(element => (string)element.Attribute("r") == cellRef);
            if (cell == null)
            {
                return string.Empty;
            }

            XElement valueElement = cell.Element(Ns + "v");
            if (valueElement == null)
            {
                return string.Empty;
            }

            string raw = valueElement.Value ?? string.Empty;
            if ((string)cell.Attribute("t") == "s" && int.TryParse(raw, out int index))
            {
                return sharedStrings.Get(index);
            }

            return raw;
        }

        private static int ReadRowNumber(XElement rowElement)
        {
            return int.TryParse((string)rowElement.Attribute("r"), out int rowNumber) ? rowNumber : 0;
        }

        private static void UpdateDimension(XDocument sheetDoc, int maxRow)
        {
            XElement dimension = sheetDoc.Root?.Element(Ns + "dimension");
            if (dimension == null)
            {
                return;
            }

            dimension.SetAttributeValue("ref", $"C2:K{Math.Max(maxRow, DataStartRow)}");
        }

        private static string CellRef(int col, int row)
        {
            return $"{ColumnLetters(col)}{row}";
        }

        private static string ColumnLetters(int col)
        {
            StringBuilder builder = new StringBuilder();
            while (col > 0)
            {
                col--;
                builder.Insert(0, (char)('A' + col % 26));
                col /= 26;
            }

            return builder.ToString();
        }

        private static byte[] ReadZipEntry(string zipPath, string entryPath)
        {
            using FileStream fileStream = File.OpenRead(zipPath);
            using ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
            ZipArchiveEntry entry = archive.GetEntry(entryPath) ?? throw new FileNotFoundException($"xlsx 内缺少 {entryPath}");
            using Stream stream = entry.Open();
            using MemoryStream memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        private static void WriteZipEntry(string zipPath, string entryPath, byte[] content)
        {
            using FileStream fileStream = File.Open(zipPath, FileMode.Open, FileAccess.ReadWrite);
            using ZipArchive archive = new ZipArchive(fileStream, ZipArchiveMode.Update);
            archive.GetEntry(entryPath)?.Delete();
            ZipArchiveEntry entry = archive.CreateEntry(entryPath, CompressionLevel.Optimal);
            using Stream stream = entry.Open();
            stream.Write(content, 0, content.Length);
        }

        private static byte[] SaveDocument(XDocument document)
        {
            using MemoryStream memoryStream = new MemoryStream();
            document.Save(memoryStream, SaveOptions.DisableFormatting);
            return memoryStream.ToArray();
        }

        private sealed class SharedStringTable
        {
            public static readonly SharedStringTable Empty = new SharedStringTable(new XDocument(new XElement(Ns + "sst")));

            private readonly XDocument document;
            private readonly XElement root;
            private readonly List<string> values = new List<string>();
            private readonly Dictionary<string, int> indexMap = new Dictionary<string, int>(StringComparer.Ordinal);

            private SharedStringTable(XDocument document)
            {
                this.document = document;
                root = document.Root ?? throw new InvalidOperationException("sharedStrings.xml 无效。");
            }

            public static SharedStringTable Load(byte[] bytes)
            {
                XDocument doc = XDocument.Load(new MemoryStream(bytes), LoadOptions.PreserveWhitespace);
                SharedStringTable table = new SharedStringTable(doc);
                foreach (XElement item in doc.Root.Elements(Ns + "si"))
                {
                    string text = string.Concat(item.Descendants(Ns + "t").Select(element => element.Value));
                    table.values.Add(text);
                    if (!table.indexMap.ContainsKey(text))
                    {
                        table.indexMap[text] = table.values.Count - 1;
                    }
                }

                return table;
            }

            public int Add(string value)
            {
                value ??= string.Empty;
                if (indexMap.TryGetValue(value, out int index))
                {
                    return index;
                }

                index = values.Count;
                values.Add(value);
                indexMap[value] = index;
                root.Add(new XElement(Ns + "si", new XElement(Ns + "t", value)));
                root.SetAttributeValue("count", values.Count);
                root.SetAttributeValue("uniqueCount", values.Count);
                return index;
            }

            public string Get(int index)
            {
                if (index < 0 || index >= values.Count)
                {
                    return string.Empty;
                }

                return values[index];
            }

            public byte[] Save()
            {
                root.SetAttributeValue("count", values.Count);
                root.SetAttributeValue("uniqueCount", values.Count);
                using MemoryStream memoryStream = new MemoryStream();
                document.Save(memoryStream, SaveOptions.DisableFormatting);
                return memoryStream.ToArray();
            }
        }
    }
}
