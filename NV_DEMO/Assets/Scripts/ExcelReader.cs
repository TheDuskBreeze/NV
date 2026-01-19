using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExcelDataReader;
using UnityEngine;

public class ExcelReader : MonoBehaviour
{
    public struct ExcelData
    {
        public string speaker;
        public string content;
        public string avatarImageFileName;
        public string vocalAudioFileName;
        public string backgroundImageFileName;
        public string backgroundMusicFileName;
        public string character1Action;
        public string coordinateX1;
        public string character1ImageFileName;
        public string character2Action;
        public string coordinateX2;
        public string character2ImageFileName;
    }

    public static List<ExcelData> ReadExcel(string filepath)
    {
        List<ExcelData> excelData = new List<ExcelData>();

        // 确保支持 GB2312 等编码
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        if (!File.Exists(filepath))
        {
            Debug.LogError($"找不到Excel文件: {filepath}");
            return excelData;
        }

        using (var stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                // 如果你的表格有表头，取消下面这行的注释来跳过第一行
                // reader.Read(); 

                while (reader.Read())
                {
                    // 检查这一行是否完全为空（至少判断前两列）
                    if (reader.GetValue(0) == null && reader.GetValue(1) == null)
                    {
                        continue;
                    }

                    try
                    {
                        ExcelData data = new ExcelData();

                        // 按照你表格的 0-11 列索引进行安全读取
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
                    catch (System.Exception e)
                    {
                        // 即使某一行解析出问题，也记录错误并继续读取下一行，不让整个程序崩溃
                        Debug.LogError($"第 {excelData.Count + 1} 行数据解析异常: {e.Message}");
                    }
                }
            }
        }

        Debug.Log($"Excel读取完成，共加载 {excelData.Count} 条对话数据。");
        return excelData;
    }

    /// <summary>
    /// 核心防御方案：安全获取单元格内容
    /// </summary>
    private static string GetCellString(IExcelDataReader reader, int index)
    {
        // 关键点：即使Excel插件认为这一行只有5列，当你访问第11列时，
        // 这里会直接返回空，而不会触发 IndexOutOfRangeException
        if (reader == null || index < 0 || index >= reader.FieldCount)
        {
            return string.Empty;
        }

        if (reader.IsDBNull(index))
        {
            return string.Empty;
        }

        // 获取值并去掉首尾空格
        object value = reader.GetValue(index);
        return value == null ? string.Empty : value.ToString().Trim();
    }
}
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using ExcelDataReader;
//using UnityEngine;

//public class ExcelReader : MonoBehaviour
//{
//    public struct ExcelData
//    {
//        public string speaker;
//        public string content;
//        public string avatarImageFileName;
//        public string vocalAudioFileName;
//        public string backgroundImageFileName;
//        public string backgroundMusicFileName;
//        public string character1ImageFileName;
//        public string character2ImageFileName;
//        public string character1Action;
//        public string character2Action;
//        public string coordinateX1;
//        public string coordinateX2;
//    }

//    public static List<ExcelData> ReadExcel(string filepath)
//    {
//        List<ExcelData> excelData = new List<ExcelData>();
//        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
//        using (var stream = File.Open(filepath, FileMode.Open, FileAccess.Read))
//        {
//            using (var reader = ExcelReaderFactory.CreateReader(stream))
//            {
//                do
//                {
//                    //while (reader.Read()) {
//                    //    ExcelData data = new ExcelData();
//                    //    if (reader.GetValue(0) == null && reader.GetValue(1) == null)
//                    //    {
//                    //        continue;
//                    //    }
//                    //    data.speaker = GetCellString(reader, 0);
//                    //    data.content = GetCellString(reader, 1);
//                    //    data.avatarImageFileName = GetCellString(reader, 2);
//                    //    data.vocalAudioFileName = GetCellString(reader, 3);
//                    //    data.backgroundImageFileName = GetCellString(reader, 4);
//                    //    data.backgroundMusicFileName = GetCellString(reader, 5);
//                    //    data.character1Action = GetCellString(reader, 6);
//                    //    data.coordinateX1 = GetCellString(reader, 7);
//                    //    data.character1ImageFileName = GetCellString(reader, 8);
//                    //    data.character2Action = GetCellString(reader, 9);
//                    //    data.coordinateX2 = GetCellString(reader, 10);
//                    //    data.character2ImageFileName = GetCellString(reader, 11);
//                    //    excelData.Add(data);
//                    //}
//                    while (reader.Read())
//                    {
//                        try
//                        {
//                            Debug.Log($"正在读取第 {excelData.Count + 1} 行，实际列数: {reader.FieldCount}");
//                            // 如果前两列（姓名和对话）都是空的，说明是无效行，直接跳过
//                            if (reader.FieldCount < 2 || reader.IsDBNull(0) && reader.IsDBNull(1)) continue;

//                            ExcelData data = new ExcelData();
//                            data.speaker = GetCellString(reader, 0);
//                            data.content = GetCellString(reader, 1);
//                            data.avatarImageFileName = GetCellString(reader, 2);
//                            data.vocalAudioFileName = GetCellString(reader, 3);
//                            data.backgroundImageFileName = GetCellString(reader, 4);
//                            data.backgroundMusicFileName = GetCellString(reader, 5);
//                            data.character1Action = GetCellString(reader, 6);
//                            data.coordinateX1 = GetCellString(reader, 7);
//                            data.character1ImageFileName = GetCellString(reader, 8);
//                            data.character2Action = GetCellString(reader, 9);
//                            data.coordinateX2 = GetCellString(reader, 10);
//                            data.character2ImageFileName = GetCellString(reader, 11);
//                            excelData.Add(data);
//                        }
//                        catch (System.Exception e)
//                        {
//                            Debug.LogError($"第 {excelData.Count + 1} 行解析失败！原因: {e.Message}");
//                            continue;
//                        }
//                    }
//                } while (reader.NextResult());
//            }
//        }
//        return excelData;
//    }

//    private static string GetCellString(IExcelDataReader reader, int index)
//    {
//        // 关键：首先判断要求的索引是否超过了当前行实际读到的列数 (FieldCount)
//        if (reader == null || index < 0 || index >= reader.FieldCount)
//        {
//            return string.Empty;
//        }

//        // 确保不为 Null 后再转为字符串
//        return reader.IsDBNull(index) ? string.Empty : reader.GetValue(index)?.ToString().Trim();
//    }
//}
