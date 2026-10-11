using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ET
{
    /// <summary>
    /// ExternalDisplayConfig.xlsx 一行：Id / PartSlotKey / SpriteName / desc（C~F）。
    /// </summary>
    public readonly struct UnitPartSlotExportRow
    {
        public int Id { get; }
        public int PartSlotKey { get; }
        public string SpriteName { get; }
        public string Desc { get; }

        public UnitPartSlotExportRow(int id, int partSlotKey, string spriteName, string desc)
        {
            Id = id;
            PartSlotKey = partSlotKey;
            SpriteName = spriteName ?? string.Empty;
            Desc = desc ?? string.Empty;
        }
    }

    public readonly struct ExternalDisplayExcelWriteResult
    {
        public int WrittenCount { get; }

        public ExternalDisplayExcelWriteResult(int writtenCount)
        {
            WrittenCount = writtenCount;
        }
    }

    /// <summary>
    /// 写入 ExternalDisplayConfig.xlsx（仅 C~F 四列，每次全量覆盖数据区）。
    /// </summary>
    public static class ExternalDisplayExcelWriter
    {
        public const string DefaultExcelAssetPath = "Assets/Config/Excel/ExternalDisplayConfig.xlsx";

        private const int DataStartRow = 6;
        private const int ColId = 3;
        private const int ColPartSlotKey = 4;
        private const int ColSpriteName = 5;
        private const int ColDesc = 6;
        private const int LastCol = ColDesc;

        private static readonly XNamespace Ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private const string SheetEntryPath = "xl/worksheets/sheet1.xml";
        private const string SharedStringsEntryPath = "xl/sharedStrings.xml";

        public static ExternalDisplayExcelWriteResult WriteUnitPartSlotRows(
            string xlsxPath,
            IReadOnlyList<UnitPartSlotExportRow> rows)
        {
            if (string.IsNullOrWhiteSpace(xlsxPath) || !File.Exists(xlsxPath))
            {
                throw new FileNotFoundException("找不到 Excel 文件。", xlsxPath);
            }

            rows ??= Array.Empty<UnitPartSlotExportRow>();

            byte[] sheetBytes = ReadZipEntry(xlsxPath, SheetEntryPath);
            byte[] sharedStringBytes = ReadZipEntry(xlsxPath, SharedStringsEntryPath);

            SharedStringTable sharedStrings = SharedStringTable.Load(sharedStringBytes);
            XDocument sheetDoc = XDocument.Load(new MemoryStream(sheetBytes), LoadOptions.PreserveWhitespace);
            XElement sheetData = sheetDoc.Root?.Element(Ns + "sheetData")
                ?? throw new InvalidOperationException("sheet1.xml 缺少 sheetData。");

            ClearDataRows(sheetData);

            int maxUsedRow = DataStartRow - 1;
            for (int i = 0; i < rows.Count; i++)
            {
                UnitPartSlotExportRow row = rows[i];
                int writeRow = DataStartRow + i;
                XElement rowElement = CreateDataRowElement(writeRow);
                sheetData.Add(rowElement);
                WriteRow(rowElement, writeRow, row, sharedStrings);
                maxUsedRow = writeRow;
            }

            UpdateDimension(sheetDoc, maxUsedRow);
            sheetBytes = SaveDocument(sheetDoc);
            sharedStringBytes = sharedStrings.Save();

            WriteZipEntry(xlsxPath, SheetEntryPath, sheetBytes);
            WriteZipEntry(xlsxPath, SharedStringsEntryPath, sharedStringBytes);

            return new ExternalDisplayExcelWriteResult(rows.Count);
        }

        private static void ClearDataRows(XElement sheetData)
        {
            List<XElement> dataRows = sheetData.Elements(Ns + "row")
                .Where(row => ReadRowNumber(row) >= DataStartRow)
                .ToList();

            for (int i = 0; i < dataRows.Count; i++)
            {
                dataRows[i].Remove();
            }
        }

        private static void WriteRow(
            XElement rowElement,
            int rowNumber,
            UnitPartSlotExportRow row,
            SharedStringTable sharedStrings)
        {
            WriteNumberCell(rowElement, ColId, rowNumber, row.Id);
            WriteNumberCell(rowElement, ColPartSlotKey, rowNumber, row.PartSlotKey);
            WriteStringCell(rowElement, ColSpriteName, rowNumber, row.SpriteName, sharedStrings);
            WriteStringCell(rowElement, ColDesc, rowNumber, row.Desc, sharedStrings);
            RemoveCellsAfterColumn(rowElement, LastCol);
            rowElement.SetAttributeValue("spans", "3:6");
            SortRowCells(rowElement);
        }

        private static void RemoveCellsAfterColumn(XElement rowElement, int lastCol)
        {
            List<XElement> toRemove = rowElement.Elements(Ns + "c")
                .Where(cell => GetColumnIndex((string)cell.Attribute("r")) > lastCol)
                .ToList();

            for (int i = 0; i < toRemove.Count; i++)
            {
                toRemove[i].Remove();
            }
        }

        private static XElement CreateDataRowElement(int rowNumber)
        {
            return new XElement(
                Ns + "row",
                new XAttribute("r", rowNumber),
                new XAttribute("s", "1"),
                new XAttribute("customFormat", "1"),
                new XAttribute("spans", "3:6"));
        }

        private static void UpdateDimension(XDocument sheetDoc, int maxRow)
        {
            XElement dimension = sheetDoc.Root?.Element(Ns + "dimension");
            if (dimension == null)
            {
                return;
            }

            dimension.SetAttributeValue("ref", $"C2:F{Math.Max(maxRow, DataStartRow)}");
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

        private static void SortRowCells(XElement rowElement)
        {
            List<XElement> cells = rowElement.Elements(Ns + "c").ToList();
            if (cells.Count <= 1)
            {
                return;
            }

            foreach (XElement cell in cells)
            {
                cell.Remove();
            }

            foreach (XElement cell in cells.OrderBy(c => GetColumnIndex((string)c.Attribute("r"))))
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

        private static int ReadRowNumber(XElement rowElement)
        {
            return int.TryParse((string)rowElement.Attribute("r"), out int rowNumber) ? rowNumber : 0;
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
