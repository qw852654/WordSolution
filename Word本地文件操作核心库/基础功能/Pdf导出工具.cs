using System;
using Microsoft.Office.Interop.Word;

namespace Word本地文件操作核心库.工具
{
    public static class Pdf导出工具
    {
        public static void 导出(Document 文档, string pdf路径)
        {
            if (文档 == null) throw new ArgumentNullException(nameof(文档));
            if (string.IsNullOrWhiteSpace(pdf路径)) throw new ArgumentException("pdf路径不能为空。", nameof(pdf路径));

            文档.ExportAsFixedFormat(
                pdf路径,
                WdExportFormat.wdExportFormatPDF,
                OpenAfterExport: false,
                OptimizeFor: WdExportOptimizeFor.wdExportOptimizeForPrint,
                Range: WdExportRange.wdExportAllDocument,
                IncludeDocProps: true,
                KeepIRM: true,
                CreateBookmarks: WdExportCreateBookmarks.wdExportCreateHeadingBookmarks,
                DocStructureTags: true,
                BitmapMissingFonts: true,
                UseISO19005_1: true
            );
        }
    }
}

