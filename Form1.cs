using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;

namespace Copy
{
    public partial class Form1 : Form
    {
        string verName = "HMI_ver";//版本号的名字
        string verPath = @"src\pages\0_Module\dataTbl\version.js";//版本号所在文件路径
        string verStr;//版本号
        string MD5FilePath = @"dist\output\original\MD5_Check.txt";//MD5文件所在位置
        string appFilePath = @"dist\output\original\app.prc";//程序所在位置
        string logoFilePath = @"logo";//logo目录所在位置
        string audioFilePath = @"audio";//audio目录所在位置
        string chipPath = @"..\01CHIP\";//最终文件路径

        public Form1()
        {
            InitializeComponent();
            CopyMain();
            Environment.Exit(0);
        }

        /// <summary>
        /// 主函数
        /// </summary>
        private void CopyMain()
        {
            if (GetVer())
            {
                //创建app.prc的MD5文件
                if (File.Exists(appFilePath))
                {
                    File.WriteAllText(MD5FilePath, GetFileMD5(appFilePath));
                }
                else
                {
                    MessageBox.Show("找不到该路径文件：" + appFilePath);
                    return;
                }

                //复制logo目录到指定路径
                if (Directory.Exists(logoFilePath))
                {
                    CopyDirectory(logoFilePath, chipPath + verStr + @"\" + logoFilePath);
                }
                //复制audio目录下的文件到指定路径
                if (Directory.Exists(audioFilePath))
                {
                    CopyFilesOnly(audioFilePath, chipPath + verStr);
                }

                //复制程序和其MD5值到01chip
                File.Copy(appFilePath, Path.Combine(chipPath + verStr, Path.GetFileName(appFilePath)), true);
                if (File.Exists(MD5FilePath))
                {
                    File.Copy(MD5FilePath, Path.Combine(chipPath + verStr, Path.GetFileName(MD5FilePath)), true);
                }

                //删除多余文件
                DeletePath(@"dist");
                DeletePath(@"tools\sdroot\gui");
                DeletePath(@"tools\xfel\sdroot.bin");

                MessageBox.Show("复制成功！请烧录微码到显示屏上确认\n版本号为： " + verStr);
            }
        }

        /// <summary>
        /// 获取微码版本号
        /// </summary>
        private bool GetVer()
        {
            if (!File.Exists(verPath))
            {
                MessageBox.Show("找不到下列路径：" + verPath);
                return false;
            }
            foreach (string line in File.ReadLines(verPath))
            {
                verStr = Regex.Replace(line, @"\s+", "");//清除该行文本的空白符号
                //获取版本号变量名所在行，并且该行未被注释
                if (verStr.Contains(verName) && verStr.Substring(0, 2) != "//")
                {
                    //获取双引号里面的字符串
                    string[] parts = verStr.Split('"');
                    if (parts.Length >= 3)
                    {
                        verStr = parts[1];
                        return true;
                    }
                }
            }
            MessageBox.Show("找不到版本号变量名：" + verName);
            return false;
        }

        /// <summary>
        /// 返回文件的MD5值
        /// </summary>
        public static string GetFileMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = md5.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// 复制整个目录及其所有内容到目标目录
        /// </summary>
        /// <param name="sourcePath">源目录路径</param>
        /// <param name="targetPath">目标目录路径</param>
        public static void CopyDirectory(string sourcePath, string targetPath)
        {
            try
            {
                // 检查目标目录是否以目录分隔符结束，如果不是则添加
                if (!targetPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    targetPath += Path.DirectorySeparatorChar;
                }

                // 判断目标目录是否存在，如果不存在则新建
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }

                // 获取源目录中的所有文件和子目录
                string[] fileSystemEntries = Directory.GetFileSystemEntries(sourcePath);

                // 遍历所有的文件和目录
                foreach (string entry in fileSystemEntries)
                {
                    // 如果是目录，则递归复制该目录
                    if (Directory.Exists(entry))
                    {
                        string dirName = Path.GetFileName(entry);
                        CopyDirectory(entry, targetPath + dirName);
                    }
                    // 如果是文件，则直接复制
                    else
                    {
                        string fileName = Path.GetFileName(entry);
                        string destFile = Path.Combine(targetPath, fileName);
                        File.Copy(entry, destFile, true); // true表示覆盖已存在的文件
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制目录时出错: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 复制指定目录下的文件到到目标目录
        /// </summary>
        public static void CopyFilesOnly(string sourceDir, string destDir)
        {
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            string[] fileList = Directory.GetFiles(sourceDir);
            foreach (string file in fileList)
            {
                string fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(destDir, fileName), true);
            }
        }


        /// <summary>
        /// 删除指定目录或文件
        /// </summary>
        public static string DeletePath(string path)
        {
            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Normal);
                File.Delete(path);
                return "文件删除成功";
            }
            else if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                return "目录删除成功";
            }
            else
            {
                return "路径不存在";
            }
        }

    }
}