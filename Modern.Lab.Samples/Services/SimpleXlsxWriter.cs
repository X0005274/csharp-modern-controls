using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Text;

namespace Modern.Lab.Samples.Services
{
    /// <summary>
    /// 외부 라이브러리 없이 최소 구성의 진짜 .xlsx(Open XML) 파일을 쓰는 헬퍼.
    /// WindowsBase의 System.IO.Packaging(ZIP 컨테이너)으로 워크북/시트/스타일
    /// 파트를 직접 기록한다. 모든 셀은 인라인 문자열이고 헤더 행만 굵게 —
    /// 조회 결과 내보내기 용도에 충분한 범위다. Excel이 더블클릭으로 바로 연다.
    /// </summary>
    internal static class SimpleXlsxWriter
    {
        private const string MainNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private const string RelNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        /// <summary>헤더 1행 + 데이터 행들을 단일 시트 워크북으로 저장한다.</summary>
        internal static void Write(string path, string sheetName, string[] headers, IEnumerable<string[]> rows)
        {
            using (Package package = Package.Open(path, FileMode.Create))
            {
                Uri workbookUri = new Uri("/xl/workbook.xml", UriKind.Relative);
                Uri sheetUri = new Uri("/xl/worksheets/sheet1.xml", UriKind.Relative);
                Uri stylesUri = new Uri("/xl/styles.xml", UriKind.Relative);

                PackagePart workbookPart = package.CreatePart(
                    workbookUri,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml",
                    CompressionOption.Normal);
                PackagePart sheetPart = package.CreatePart(
                    sheetUri,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml",
                    CompressionOption.Normal);
                PackagePart stylesPart = package.CreatePart(
                    stylesUri,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml",
                    CompressionOption.Normal);

                // 패키지 → 워크북, 워크북 → 시트/스타일 관계 연결.
                package.CreateRelationship(workbookUri, TargetMode.Internal,
                    RelNamespace + "/officeDocument", "rId1");
                workbookPart.CreateRelationship(new Uri("worksheets/sheet1.xml", UriKind.Relative),
                    TargetMode.Internal, RelNamespace + "/worksheet", "rId1");
                workbookPart.CreateRelationship(new Uri("styles.xml", UriKind.Relative),
                    TargetMode.Internal, RelNamespace + "/styles", "rId2");

                WriteText(workbookPart, BuildWorkbookXml(sheetName));
                WriteText(stylesPart, BuildStylesXml());
                WriteText(sheetPart, BuildSheetXml(headers, rows));
            }
        }

        private static void WriteText(PackagePart part, string xml)
        {
            using (Stream stream = part.GetStream(FileMode.Create, FileAccess.Write))
            {
                byte[] bytes = new UTF8Encoding(false).GetBytes(xml);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        private static string BuildWorkbookXml(string sheetName)
        {
            StringBuilder xml = new StringBuilder();
            xml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            xml.Append("<workbook xmlns=\"").Append(MainNamespace).Append("\"");
            xml.Append(" xmlns:r=\"").Append(RelNamespace).Append("\">");
            xml.Append("<sheets><sheet name=\"").Append(Escape(sheetName));
            xml.Append("\" sheetId=\"1\" r:id=\"rId1\"/></sheets>");
            xml.Append("</workbook>");
            return xml.ToString();
        }

        // 최소 유효 스타일: 폰트 0(일반)/1(굵게), Excel이 요구하는 기본 fill 2종,
        // 셀 서식 0(일반)/1(헤더 굵게).
        private static string BuildStylesXml()
        {
            StringBuilder xml = new StringBuilder();
            xml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            xml.Append("<styleSheet xmlns=\"").Append(MainNamespace).Append("\">");
            xml.Append("<fonts count=\"2\">");
            xml.Append("<font><sz val=\"11\"/><name val=\"Calibri\"/></font>");
            xml.Append("<font><b/><sz val=\"11\"/><name val=\"Calibri\"/></font>");
            xml.Append("</fonts>");
            xml.Append("<fills count=\"2\">");
            xml.Append("<fill><patternFill patternType=\"none\"/></fill>");
            xml.Append("<fill><patternFill patternType=\"gray125\"/></fill>");
            xml.Append("</fills>");
            xml.Append("<borders count=\"1\"><border/></borders>");
            xml.Append("<cellStyleXfs count=\"1\"><xf/></cellStyleXfs>");
            xml.Append("<cellXfs count=\"2\">");
            xml.Append("<xf xfId=\"0\"/>");
            xml.Append("<xf xfId=\"0\" fontId=\"1\" applyFont=\"1\"/>");
            xml.Append("</cellXfs>");
            xml.Append("</styleSheet>");
            return xml.ToString();
        }

        private static string BuildSheetXml(string[] headers, IEnumerable<string[]> rows)
        {
            StringBuilder xml = new StringBuilder();
            xml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            xml.Append("<worksheet xmlns=\"").Append(MainNamespace).Append("\">");
            xml.Append("<sheetData>");

            AppendRow(xml, headers, true);

            foreach (string[] row in rows)
            {
                AppendRow(xml, row, false);
            }

            xml.Append("</sheetData>");
            xml.Append("</worksheet>");
            return xml.ToString();
        }

        private static void AppendRow(StringBuilder xml, string[] cells, bool header)
        {
            xml.Append("<row>");

            foreach (string cell in cells)
            {
                xml.Append(header ? "<c t=\"inlineStr\" s=\"1\">" : "<c t=\"inlineStr\">");
                xml.Append("<is><t xml:space=\"preserve\">").Append(Escape(cell)).Append("</t></is>");
                xml.Append("</c>");
            }

            xml.Append("</row>");
        }

        private static string Escape(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            StringBuilder escaped = new StringBuilder(text.Length);

            foreach (char ch in text)
            {
                switch (ch)
                {
                    case '&': escaped.Append("&amp;"); break;
                    case '<': escaped.Append("&lt;"); break;
                    case '>': escaped.Append("&gt;"); break;
                    case '"': escaped.Append("&quot;"); break;
                    default:
                        // 제어 문자는 XML 1.0에서 불법 — 공백으로 완화한다.
                        if (ch < 0x20 && ch != '\t' && ch != '\n' && ch != '\r')
                        {
                            escaped.Append(' ');
                        }
                        else
                        {
                            escaped.Append(ch);
                        }
                        break;
                }
            }

            return escaped.ToString();
        }
    }
}
