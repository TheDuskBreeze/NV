using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExcelDataReader;
using UnityEngine;

public class ExcelReader : MonoBehaviour
{
    public struct ExcelData {
        public string speaker;
        public string content;
        public string avatarImageFileName;
        public string vocalAudioFileName;
        public string backgroundImageFileName;
        public string backgroundMusicFileName;
        public string character1ImageFileName;
        public string character2ImageFileName;
        public string character1Action;
        public string character2Action;
        public string coordinateX1;
        public string coordinateX2;
    }

    public static List<ExcelData> ReadExcel(string filepath) {
        List<ExcelData> excelData = new List<ExcelData>();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using (var stream = File.Open(filepath, FileMode.Open, FileAccess.Read)) {
            using (var reader = ExcelReaderFactory.CreateReader(stream)) {
                do {
                    while (reader.Read()) {
                        ExcelData data = new ExcelData();
                        data.speaker = GetCellString(reader, 0);
                        data.content = GetCellString(reader, 1);
                        data.avatarImageFileName = GetCellString(reader, 2);
                        data.vocalAudioFileName = GetCellString(reader, 3);
                        data.backgroundImageFileName = GetCellString(reader, 4);
                        data.backgroundMusicFileName = GetCellString(reader, 5);
                        data.character1Action = GetCellString(reader, 6);
                        data.coordinateX1 = GetCellString(reader, 7);
                        data.character1ImageFileName = GetCellString(reader, 8);
                        data.character2Action = GetCellString(reader, 9);
                        data.coordinateX2 = GetCellString(reader, 10);
                        data.character2ImageFileName = GetCellString(reader, 11);
                        excelData.Add(data);
                    }
                } while (reader.NextResult());
            }
        }
        return excelData;
    }

    private static string GetCellString(IExcelDataReader reader, int index)
    {
        return reader.IsDBNull(index) ? string.Empty : reader.GetValue(index)?.ToString();
    }
}
