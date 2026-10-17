# -*- coding: utf-8 -*-
"""Fix JobConfig.xlsx: I4 field Name -> Model; job 1002 display Name 法师 -> 巫师."""
import zipfile
import re
import os
import shutil
import tempfile
from xml.etree import ElementTree as ET

XLSX = r"E:\Git\FAE\ETMMO\ET-release8.1\Unity\Assets\Config\Excel\JobConfig.xlsx"
OUT = r"E:\Git\FAE\ETMMO\ET-release8.1\Unity\Assets\Config\Excel\JobConfig_patched.xlsx"
NS = "{http://schemas.openxmlformats.org/spreadsheetml/2006/main}"


def main():
    tmp = tempfile.mkdtemp()
    try:
        with zipfile.ZipFile(XLSX, "r") as zin:
            zin.extractall(tmp)

        ss_path = os.path.join(tmp, "xl", "sharedStrings.xml")
        tree = ET.parse(ss_path)
        root = tree.getroot()
        si_nodes = root.findall(".//" + NS + "si")

        def add_string(text):
            si = ET.Element(NS + "si")
            t = ET.SubElement(si, NS + "t")
            t.text = text
            root.append(si)
            return len(si_nodes) - 1 + len(root.findall(".//" + NS + "si")) - len(si_nodes)

        # Re-count: append adds at end
        wizard_idx = None
        for i, si in enumerate(root.findall(".//" + NS + "si")):
            texts = []
            for t in si.findall(".//" + NS + "t"):
                if t.text:
                    texts.append(t.text)
            if u"".join(texts) == u"巫师":
                wizard_idx = i
                break
        if wizard_idx is None:
            si = ET.Element(NS + "si")
            t = ET.SubElement(si, NS + "t")
            t.text = u"巫师"
            root.append(si)
            wizard_idx = len(root.findall(".//" + NS + "si")) - 1

        model_idx = None
        for i, si in enumerate(root.findall(".//" + NS + "si")):
            texts = []
            for t in si.findall(".//" + NS + "t"):
                if t.text:
                    texts.append(t.text)
            if u"".join(texts) == u"Model":
                model_idx = i
                break
        if model_idx is None:
            raise RuntimeError("Model string not found in sharedStrings")

        tree.write(ss_path, encoding="UTF-8", xml_declaration=True)

        sheet_path = os.path.join(tmp, "xl", "worksheets", "sheet1.xml")
        sheet = open(sheet_path, "rb").read().decode("utf-8")

        # I4: field name must be Model (shared string index model_idx)
        sheet = re.sub(
            r'(<c r="I4"[^>]*t="s"[^>]*><v>)\d+(</v></c>)',
            r"\g<1>%d\g<2>" % model_idx,
            sheet,
            count=1,
        )
        if 'r="I4"' in sheet and str(model_idx) not in sheet.split('r="I4"')[1][:80]:
            # inline string form
            sheet = sheet.replace(
                '<c r="I4" t="s"><v>10</v></c>',
                '<c r="I4" t="s"><v>%d</v></c>' % model_idx,
            )

        # G7: job 1002 display name
        sheet = re.sub(
            r'(<c r="G7"[^>]*t="s"[^>]*><v>)\d+(</v></c>)',
            r"\g<1>%d\g<2>" % wizard_idx,
            sheet,
            count=1,
        )

        open(sheet_path, "wb").write(sheet.encode("utf-8"))

        src = XLSX if os.path.exists(XLSX) else OUT
        with zipfile.ZipFile(OUT, "w", zipfile.ZIP_DEFLATED) as zout:
            for folder, _, files in os.walk(tmp):
                for name in files:
                    path = os.path.join(folder, name)
                    arc = os.path.relpath(path, tmp).replace("\\", "/")
                    zout.write(path, arc)
        print("OK: wrote", OUT)
    finally:
        shutil.rmtree(tmp, ignore_errors=True)


if __name__ == "__main__":
    main()
