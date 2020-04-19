using HEIF_Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HEIC2JPG
{
    class Program
    {
        static void Main(string[] args)
        {
            var bo = true;
            while (bo)
            {
                Console.WriteLine("请输入heic文件夹所在目录(输入y结束):");
                string strInput = Console.ReadLine();
                if (strInput != "y")
                {
                    if (Directory.Exists(strInput))
                    {
                        //Image imgSource = Image.FromFile("D:/1.heic");
                        List<FileInfo> lstFiles = getFile(strInput, ".heic");
                        Console.WriteLine("共找到" + lstFiles.Count + "个文件，开始转换...");
                        var outpath = strInput + "\\heic2jpg";
                        if (!Directory.Exists(outpath))
                            Directory.CreateDirectory(outpath);
                        var count = 1;
                        var btime_c = DateTime.Now;
                        foreach (var item in lstFiles)
                        {
                            var btime = DateTime.Now;
                            var fileext = item.Name.Substring(item.Name.LastIndexOf("."));
                            var filename = item.Name.Replace(fileext, ".jpg");
                            var heif_data = invoke_dll.read_heif(item.FullName);
                            invoke_dll.invoke_heif_to_jpg(heif_data, 80, "D:/2.jpgtmp").Save(outpath + "\\" + filename, ImageFormat.Jpeg);
                            var etime = DateTime.Now;
                            Console.WriteLine("成功:" + count + ". " + item.Name + " --> " + outpath + "\\" + filename + "  耗时：" + ExecDateDiff(btime, etime));
                            count++;
                        }
                        var etime_c = DateTime.Now;
                        Console.WriteLine("转换完成，总耗时：" + ExecDateDiff(btime_c, etime_c));
                        Console.WriteLine("继续转换请输入1(结束请按其他任意键):");
                        string jixv = Console.ReadLine();
                        if (jixv != "1")
                        {
                            bo = false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("目录不存在");
                    }
                }
                else
                {
                    bo = false;
                    Console.WriteLine("已关闭");
                }
            }
        }
        /// <summary>
        /// 程序执行时间测试
        /// </summary>
        /// <param name="dateBegin">开始时间</param>
        /// <param name="dateEnd">结束时间</param>
        /// <returns>返回(秒)单位，比如: 0.00239秒</returns>
        public static string ExecDateDiff(DateTime dateBegin, DateTime dateEnd)
        {
            TimeSpan ts1 = new TimeSpan(dateBegin.Ticks);
            TimeSpan ts2 = new TimeSpan(dateEnd.Ticks);
            TimeSpan ts3 = ts1.Subtract(ts2).Duration();
            //你想转的格式
            var dateDiff = "";
            if (ts3.Days > 0)
            {
                dateDiff += ts3.Days.ToString() + "天";
            }
            if (ts3.Hours > 0)
            {
                dateDiff += ts3.Hours.ToString() + "小时";
            }
            if (ts3.Minutes > 0)
            {
                dateDiff += ts3.Minutes.ToString() + "分钟";
            }
            if (ts3.Seconds > 0)
            {
                dateDiff += ts3.Seconds.ToString() + "秒";
            }
            if (ts3.Milliseconds > 0)
            {
                dateDiff += ts3.Milliseconds.ToString() + "毫秒";
            }
            //var dateDiff = ts3.Days.ToString() + "天" + ts3.Hours.ToString() + "小时" + ts3.Minutes.ToString() + "分钟" + ts3.Seconds.ToString() + "秒";
            return dateDiff;
            //return ts3.TotalMilliseconds.ToString();
            //return ts3.ToString("g");
        }
        /// <summary>
        /// 获得目录下所有文件或指定文件类型文件(包含所有子文件夹)
        /// </summary>
        /// <param name="path">文件夹路径</param>
        /// <param name="extName">扩展名可以多个 例如 .mp3.wma.rm</param>
        /// <returns>List<FileInfo></returns>
        public static List<FileInfo> getFile(string path, string extName, List<FileInfo> lst = null)
        {
            try
            {
                lst = lst == null ? new List<FileInfo>() : lst;
                string[] dir = Directory.GetDirectories(path); //文件夹列表  
                DirectoryInfo fdir = new DirectoryInfo(path);
                FileInfo[] file = fdir.GetFiles();
                //FileInfo[] file = Directory.GetFiles(path); //文件列表  
                if (file.Length != 0 || dir.Length != 0) //当前目录文件或文件夹不为空          
                {
                    foreach (FileInfo f in file) //显示当前目录所有文件  
                    {
                        if (extName.ToLower().IndexOf(f.Extension.ToLower()) >= 0)
                        {
                            lst.Add(f);
                        }
                    }
                    foreach (string d in dir)
                    {
                        getFile(d, extName, lst);//递归  
                    }
                }
                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="inFilePath">原图片地址</param>
        /// <param name="outFilePath">输出地址</param>
        /// <param name="size">缩放级别</param>
        /// <returns></returns>
        static bool CreateThumbnail(string inFilePath, string outFilePath, string size)
        {
            string dir = outFilePath.Remove(outFilePath.LastIndexOf('/'));
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            Image imgSource = Image.FromFile(inFilePath);
            int sWidth = imgSource.Width; // 原图片宽度
            int sHeight = imgSource.Height; // 原图片高度
            int newWidth = 0;
            int newHeight = 0;
            switch (size)
            {
                case "s120":
                    newWidth = 120;
                    break;
                case "s200":
                    newWidth = 200;
                    break;
                case "m480":
                    newWidth = 480;
                    break;
                case "m720":
                    newWidth = 512;
                    break;
                case "lg1080":
                    newWidth = 1080;
                    newHeight = 1080;
                    break;
            }

            double wScale = (double)newWidth / sWidth; // 宽比例
            double hScale = (double)newHeight / sHeight; // 高比例
            double scale = wScale > hScale ? wScale : hScale;
            if (scale > 1)
            {
                // 如果原图比要生成的图片还小，则保留原图尺寸,直接转移
                newWidth = sWidth;
                newHeight = sHeight;
                File.Copy(inFilePath, outFilePath);
            }
            else
            {
                newWidth = (int)(sWidth * scale);
                newHeight = (int)(sHeight * scale);
                Bitmap thumbnail = new Bitmap(newWidth, newHeight);
                using (Graphics tGraphic = Graphics.FromImage(thumbnail))
                {
                    tGraphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic; /* new way */
                    Rectangle rectSrc = new Rectangle(0, 0, sWidth, sHeight);
                    Rectangle rect = new Rectangle(0, 0, newWidth, newHeight);
                    tGraphic.DrawImage(imgSource, rect, rectSrc, GraphicsUnit.Pixel);
                }

                if (inFilePath.EndsWith("png"))
                {
                    thumbnail.Save(outFilePath, ImageFormat.Png);
                }
                else
                {
                    thumbnail.Save(outFilePath, ImageFormat.Jpeg);
                }
            }

            return true;
        }
    }
}
