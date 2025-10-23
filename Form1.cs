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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Copy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string target = "HMI_ver";
            string lines;

            //foreach (string line in File.ReadLines(@"src\pages\0_Module\dataTbl\version.js"))
                foreach (string line in File.ReadLines(@"version.js"))
                {
                    // 逐行处理，内存效率更高
                    lines = Regex.Replace(line, @"\s+", "");

                    if (lines.Contains(target) && lines.Substring(0, 2) != "//")
                    {
                        string[] parts = lines.Split('"');
                        if(parts.Length >= 3)
                        {
                            //richTextBox1.AppendText(parts[1]);
                        }
                    }
                }
       
            File.WriteAllText(@"MD5_Check.txt", GetFileMD5(@"app.prc"));

            CopyAndDelete(@"MD5_Check.txt", @"../01chip/MD5_Check.txt");

            DeletePath(@"tools\sdroot\gui");
            DeletePath(@"tools\xfel\sdroot.bin");
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
        /// 复制文件到新位置后删除原文件
        /// </summary>
        public static bool CopyAndDelete(string sourcePath, string destinationPath)
        {
            try
            {
                // 参数验证
                if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(destinationPath))
                    return false;

                if (!File.Exists(sourcePath))
                    throw new FileNotFoundException($"源文件不存在: {sourcePath}");

                // 确保目标目录存在
                string destinationDir = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(destinationDir))
                    Directory.CreateDirectory(destinationDir);

                // 复制文件
                File.Copy(sourcePath, destinationPath, true);
                Console.WriteLine($"文件复制成功: {destinationPath}");

                // 验证目标文件
                if (File.Exists(destinationPath))
                {
                    // 删除原文件
                    File.Delete(sourcePath);
                    Console.WriteLine($"原文件删除成功: {sourcePath}");

                    return true;
                }
                else
                {
                    Console.WriteLine("错误：复制后目标文件不存在");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"复制删除操作失败: {ex.Message}");
                return false;
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
                // 检查目标目录是否以目录分隔符结束，如果不是则添加:ml-citation{ref="1" data="citationList"}
                if (!targetPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    targetPath += Path.DirectorySeparatorChar;
                }

                // 判断目标目录是否存在，如果不存在则新建:ml-citation{ref="1,3" data="citationList"}
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }

                // 获取源目录中的所有文件和子目录:ml-citation{ref="1,5" data="citationList"}
                string[] fileSystemEntries = Directory.GetFileSystemEntries(sourcePath);

                // 遍历所有的文件和目录
                foreach (string entry in fileSystemEntries)
                {
                    // 如果是目录，则递归复制该目录:ml-citation{ref="1,4" data="citationList"}
                    if (Directory.Exists(entry))
                    {
                        string dirName = Path.GetFileName(entry);
                        CopyDirectory(entry, targetPath + dirName);
                    }
                    // 如果是文件，则直接复制:ml-citation{ref="1,3" data="citationList"}
                    else
                    {
                        string fileName = Path.GetFileName(entry);
                        string destFile = Path.Combine(targetPath, fileName);
                        File.Copy(entry, destFile, true); // true表示覆盖已存在的文件:ml-citation{ref="3,6" data="citationList"}
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"复制目录时出错: {ex.Message}");
                throw;
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



/*
 * @echo off

set filepath1=..\01CHIP\X7.GD028A.F51.002-1.V100AXX
set filepath2=..\01CHIP\X7.GD028A.F51.022-1.V100AXX

echo 1:%filepath1:~10,19%
echo 2:%filepath2:~10,19%
set /p sel=选择微码号：
if %sel% == 1 (
	set filepath=%filepath1%
) else (
	set filepath=%filepath2%
)

echo 正在打包的是：%filepath%

CALL MD5.bat

if not exist %filepath%\logo (
    MD %filepath%\logo
)

xcopy logo %filepath%\logo /e /y
xcopy audio\* %filepath%\ /y

if not exist dist\output\original\app.prc (
    echo dist\output\original\app.prc不存在
    goto :exit
)
copy dist\output\original\app.prc %filepath%
copy dist\output\original\MD5_Check.txt %filepath%

:exit
if not "%1"=="-b" (
rd dist /s /q
rd tools\sdroot\gui /s /q
del /f tools\xfel\sdroot.bin
)

ping 127.0.0.1 -n 3 >nul
echo.
*/