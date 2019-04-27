using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Net;
using System.Collections;
using System.IO;

namespace OcrDemo.Util
{
    public class CheckCodeRecognize
    {
        #region 成员变量
        //色差阀值 越小消除的杂色越多
        private double threshold = 150;
        //二值阀值 越大效果越不明显
        private double ezFZ = 0.42;
        //背景近似度阀值
        private double bjfz = 80;
        //图片路径
        private string imgPath = string.Empty;
        //每个字符最小宽度
        public int MinWidthPerChar = 7;
        //每个字符最大宽度
        public int MaxWidthPerChar = 18;
        //每个字符最小高度
        public int MinHeightPerChar = 10;
        //学习库保存的路径
        private readonly string samplePath = AppDomain.CurrentDomain.BaseDirectory + "Sample\\";
        #endregion

        #region 图片处理
        /// <summary>
        /// 对传入的图片二值化
        /// </summary>
        /// <param name="bitmap">传入的原图片</param>
        /// <returns>处理过后的图片</returns>
        public Bitmap EZH(Bitmap bitmap)
        {
            if (bitmap != null)
            {
                var img = new Bitmap(bitmap);
                for (var x = 0; x < img.Width; x++)
                {
                    for (var y = 0; y < img.Height; y++)
                    {
                        Color color = img.GetPixel(x, y);
                        if (color.GetBrightness() < ezFZ)
                        {
                            img.SetPixel(x, y, Color.Black);
                        }
                        else
                        {
                            img.SetPixel(x, y, Color.White);
                        }
                    }
                }
                return img;
            }
            return null;

        }
        /// <summary>
        /// 去背景
        /// 把图片中最多的一部分颜色视为背景色 选出来后替换为白色
        /// </summary>
        /// <param name="bitmapImg">将要处理的图片</param>
        /// <returns>返回去过背景的图片</returns>
        public Bitmap RemoveBackGround(Bitmap bitmapImg)
        {
            if (bitmapImg == null)
            {
                return null;
            }
            //key 颜色  value颜色对应的数量
            Dictionary<Color, int> colorDic = new Dictionary<Color, int>();
            //获取图片中每个颜色的数量
            for (var x = 0; x < bitmapImg.Width; x++)
            {
                for (var y = 0; y < bitmapImg.Height; y++)
                {
                    //删除边框
                    if (y == 0 || y == bitmapImg.Height)
                    {
                        bitmapImg.SetPixel(x, y, Color.White);
                    }

                    var color = bitmapImg.GetPixel(x, y);
                    var colorRGB = color.ToArgb();

                    if (colorDic.ContainsKey(color))
                    {
                        colorDic[color] = colorDic[color] + 1;
                    }
                    else
                    {
                        colorDic[color] = 1;
                    }
                }
            }
            //图片中最多的颜色
            Color maxColor = colorDic.OrderByDescending(o => o.Value).FirstOrDefault().Key;
            //图片中最少的颜色
            Color minColor = colorDic.OrderBy(o => o.Value).FirstOrDefault().Key;

            Dictionary<int[], double> maxColorDifDic = new Dictionary<int[], double>();
            //查找 maxColor 最接近颜色
            for (var x = 0; x < bitmapImg.Width; x++)
            {
                for (var y = 0; y < bitmapImg.Height; y++)
                {
                    maxColorDifDic.Add(new int[] { x, y }, GetColorDif(bitmapImg.GetPixel(x, y), maxColor));
                }
            }
            //去掉和maxColor接近的颜色 即 替换成白色
            var maxColorDifList = maxColorDifDic.OrderBy(o => o.Value).Where(o => o.Value < bjfz).ToArray();
            foreach (var kv in maxColorDifList)
            {
                bitmapImg.SetPixel(kv.Key[0], kv.Key[1], Color.White);
            }
            return bitmapImg;

        }
        /// <summary>
        /// 获取色差
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <returns></returns>
        private double GetColorDif(Color color1, Color color2)
        {
            return Math.Sqrt((Math.Pow((color1.R - color2.R), 2) +
                Math.Pow((color1.G - color2.G), 2) +
                Math.Pow((color1.B - color2.B), 2)));
        }
        /// <summary>
        /// 去掉目标干扰线
        /// </summary>
        /// <param name="img">将要处理的图片</param>
        /// <returns>去掉干干扰线处理过的图片</returns>  
        public Bitmap DropDisturb(Bitmap img)
        {
            if (img == null)
            {
                return null;
            }
            byte[] p = new byte[9]; //最小处理窗口3*3
            //去干扰线
            for (var x = 0; x < img.Width; x++)
            {
                for (var y = 0; y < img.Height; y++)
                {
                    Color currentColor = img.GetPixel(x, y);
                    int color = currentColor.ToArgb();

                    if (x > 0 && y > 0 && x < img.Width - 1 && y < img.Height - 1)
                    {
                        #region 中值滤波效果不好
                        ////取9个点的值
                        //p[0] = img.GetPixel(x - 1, y - 1).R;
                        //p[1] = img.GetPixel(x, y - 1).R;
                        //p[2] = img.GetPixel(x + 1, y - 1).R;
                        //p[3] = img.GetPixel(x - 1, y).R;
                        //p[4] = img.GetPixel(x, y).R;
                        //p[5] = img.GetPixel(x + 1, y).R;
                        //p[6] = img.GetPixel(x - 1, y + 1).R;
                        //p[7] = img.GetPixel(x, y + 1).R;
                        //p[8] = img.GetPixel(x + 1, y + 1).R;
                        ////计算中值
                        //for (int j = 0; j < 5; j++)
                        //{
                        //    for (int i = j + 1; i < 9; i++)
                        //    {
                        //        if (p[j] > p[i])
                        //        {
                        //            s = p[j];
                        //            p[j] = p[i];
                        //            p[i] = s;
                        //        }
                        //    }
                        //}
                        ////      if (img.GetPixel(x, y).R < dgGrayValue)
                        //img.SetPixel(x, y, Color.FromArgb(p[4], p[4], p[4]));    //给有效值付中值
                        #endregion

                        //上 x y+1
                        double upDif = GetColorDif(currentColor, img.GetPixel(x, y + 1));
                        //下 x y-1
                        double downDif = GetColorDif(currentColor, img.GetPixel(x, y - 1));
                        //左 x-1 y
                        double leftDif = GetColorDif(currentColor, img.GetPixel(x - 1, y));
                        //右 x+1 y
                        double rightDif = GetColorDif(currentColor, img.GetPixel(x + 1, y));
                        //左上
                        double upLeftDif = GetColorDif(currentColor, img.GetPixel(x - 1, y + 1));
                        //右上
                        double upRightDif = GetColorDif(currentColor, img.GetPixel(x + 1, y + 1));
                        //左下
                        double downLeftDif = GetColorDif(currentColor, img.GetPixel(x - 1, y - 1));
                        //右下
                        double downRightDif = GetColorDif(currentColor, img.GetPixel(x + 1, y - 1));

                        ////四面色差较大
                        //if (upDif > threshold && downDif > threshold && leftDif > threshold && rightDif > threshold)
                        //{
                        //    img.SetPixel(x, y, Color.White);
                        //}
                        //三面色差较大
                        if ((upDif > threshold && downDif > threshold && leftDif > threshold)
                            || (downDif > threshold && leftDif > threshold && rightDif > threshold)
                            || (upDif > threshold && leftDif > threshold && rightDif > threshold)
                            || (upDif > threshold && downDif > threshold && rightDif > threshold))
                        {
                            img.SetPixel(x, y, Color.White);
                        }

                        List<int[]> xLine = new List<int[]>();
                        //去横向干扰线  原理 如果这个点上下有很多白色像素则认为是干扰
                        for (var x1 = x + 1; x1 < x + 10; x1++)
                        {
                            if (x1 >= img.Width)
                            {
                                break;
                            }

                            if (img.GetPixel(x1, y + 1).ToArgb() == Color.White.ToArgb()
                                && img.GetPixel(x1, y - 1).ToArgb() == Color.White.ToArgb())
                            {
                                xLine.Add(new int[] { x1, y });
                            }
                        }
                        if (xLine.Count() >= 4)
                        {
                            foreach (var xpoint in xLine)
                            {
                                img.SetPixel(xpoint[0], xpoint[1], Color.White);
                            }
                        }

                        //去竖向干扰线

                    }
                }
            }
            return img;
        }
        /// <summary>
        /// 对图片先竖向分割，再横向分割
        /// </summary>
        /// <param name="img">将要分割的图片</param>
        /// <returns>所有分割后的字符图片</returns>
        private Bitmap[] SplitImage(Bitmap img)
        {
            if (img == null)
            {
                return null;
            }
            List<int[]> xCutPointList = GetXCutPointList(img);
            List<int[]> yCutPointList = GetYCutPointList(xCutPointList, img);
            Bitmap[] bitmapArr = new Bitmap[5];
            //对分割的部分划线
            for (int i = 0; i < xCutPointList.Count(); i++)
            {
                int xStart = xCutPointList[i][0];
                int xEnd = xCutPointList[i][1];
                int yStart = yCutPointList[i][0];
                int yEnd = yCutPointList[i][1];
                if (i >= 4) break;
                bitmapArr[i] = (Bitmap)AcquireRectangleImage(img,
                    new Rectangle(xStart, yStart, xEnd - xStart + 1, yEnd - yStart + 1));
            }
            return bitmapArr;
        }
        /// <summary>
        /// 分别从图片的上下寻找像素点大于阙值的地方，然后获取有黑色像素的有效区域
        /// </summary>
        /// <param name="xCutPointList">x轴范围的x坐标集合</param>
        /// <param name="img">目标图片</param>
        /// <returns>y轴坐标开始和结束点，其实就是黑色像素图片的有效区域</returns>
        private List<int[]> GetYCutPointList(List<int[]> xCutPointList, Bitmap img)
        {
            List<int[]> list = new List<int[]>();
            //获取图像最上面Y值
            int topY = 0;
            //获取图像最下面的Y值
            int bottomY = 0;
            foreach (var xPoint in xCutPointList)
            {
                for (int ty = 1; ty < img.Height; ty++)
                {
                    int xStart = xPoint[0];
                    int xEnd = xPoint[1];
                    int blackCount = GetBlackPXCountInY(ty, 2, xStart, xEnd, img);
                    if (blackCount > 3)
                    {
                        topY = ty;
                        break;
                    }
                }
                for (int by = img.Height; by > 1; by--)
                {
                    int xStart = xPoint[0];
                    int xEnd = xPoint[1];
                    int blackCount = GetBlackPXCountInY(by, -2, xStart, xEnd, img);
                    if (blackCount > 3)
                    {
                        bottomY = by;
                        break;
                    }
                }
                list.Add(new int[] { topY, bottomY });

            }
            return list;
        }
        /// <summary>
        /// 获取分割后某区域的黑色像素
        /// </summary>
        /// <param name="startY"></param>
        /// <param name="offset"></param>
        /// <param name="startX"></param>
        /// <param name="endX"></param>
        /// <param name="img"></param>
        /// <returns></returns>
        private int GetBlackPXCountInY(int startY, int offset, int startX, int endX, Bitmap img)
        {
            int blackPXCount = 0;
            int startY1 = offset > 0 ? startY : startY + offset;
            int offset1 = offset > 0 ? startY + offset : startY;
            for (var x = startX; x <= endX; x++)
            {
                for (var y = startY1; y < offset1; y++)
                {
                    if (y >= img.Height)
                    {
                        continue;
                    }
                    if (img.GetPixel(x, y).ToArgb() == Color.Black.ToArgb())
                    {
                        blackPXCount++;
                    }
                }
            }
            return blackPXCount;
        }
        /// <summary>
        /// 获取一个垂直区域内的黑色像素
        /// </summary>
        /// <param name="startX">开始x</param>
        /// <param name="offset">左偏移像素</param>
        /// <returns></returns>
        private int GetBlackPXCountInX(int startX, int offset, Bitmap img)
        {
            int blackPXCount = 0;
            for (int x = startX; x < startX + offset; x++)
            {
                if (x >= img.Width)
                {
                    continue;
                }
                for (var y = 0; y < img.Height; y++)
                {
                    if (img.GetPixel(x, y).ToArgb() == Color.Black.ToArgb())
                    {
                        blackPXCount++;
                    }
                }
            }
            return blackPXCount;
        }
        /// <summary>
        /// 获取竖向分割点
        /// </summary>
        /// <param name="img"></param>
        /// <returns>List int[xstart xend]</returns>
        private List<int[]> GetXCutPointList(Bitmap img)
        {
            //分割点  List<int[xstart xend]>
            List<int[]> xCutList = new List<int[]>();
            int startX = -1;//-1表示在寻找开始节点
            for (var x = 0; x < img.Width; x++)
            {
                if (startX == -1)//开始点
                {
                    int blackPXCount = GetBlackPXCountInX(x, 2, img);
                    //如果大于有效像素则是开始节点 ,0-x的矩形区域大于3像素，认为是字母，防止一些噪点被切割           
                    if (blackPXCount > 5)
                    {
                        startX = x;

                    }
                }
                else//结束点
                {
                    if (x == img.Width - 1)//判断是否最后一列
                    {
                        xCutList.Add(new int[] { startX, x });
                        break;
                    }
                    else if (x >= startX + MinWidthPerChar)//隔开一定距离才能结束分割
                    {
                        int blackPXCount = GetBlackPXCountInX(x, 2, img);//判断后面区域黑色像素点的个数
                        //小于等于阀值则是结束节点                       
                        if (blackPXCount < 2)
                        {

                            if (x > startX + MaxWidthPerChar)//尽量控制不执行
                            {
                                //大于最大字符的宽度应该是两个字符粘连到一块了 从中间分开
                                int middleX = startX + (x - startX) / 2;
                                xCutList.Add(new int[] { startX, middleX });
                                xCutList.Add(new int[] { middleX + 1, x });
                            }
                            else
                            {
                                //验证黑色像素是否太少
                                blackPXCount = GetBlackPXCountInX(startX, x - startX, img);
                                if (blackPXCount <= 10)
                                {
                                    startX = -1;//重置开始点
                                }
                                else
                                {
                                    xCutList.Add(new int[] { startX, x });
                                }
                            }
                            startX = -1;//重置开始点
                        }
                    }
                }
            }
            return xCutList;
        }
        /// <summary>
        /// 截取图像的矩形区域
        /// </summary>
        /// <param name="source">源图像对应picturebox1</param>
        /// <param name="rect">矩形区域，如上初始化的rect</param>
        /// <returns>矩形区域的图像</returns>
        private Image AcquireRectangleImage(Image source, Rectangle rect)
        {
            if (source == null || rect.IsEmpty) return null;
            //Bitmap bmSmall = new Bitmap(rect.Width, rect.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Bitmap bmSmall = new Bitmap(rect.Width, rect.Height, source.PixelFormat);

            using (Graphics grSmall = Graphics.FromImage(bmSmall))
            {
                grSmall.DrawImage(source,
                                  new System.Drawing.Rectangle(0, 0, bmSmall.Width, bmSmall.Height),
                                  rect,
                                  GraphicsUnit.Pixel);
                grSmall.Dispose();
            }
            return bmSmall;
        }
        #endregion

        #region 图片识别
        /// <summary>
        /// 返回两图比较的相似度 最大1
        /// </summary>
        /// <param name="compareImg">对比图</param>
        /// <param name="mainImg">要识别的图</param>
        /// <returns></returns>
        private double CompareImg(Bitmap compareImg, Bitmap mainImg)
        {
            int img1x = compareImg.Width;
            int img1y = compareImg.Height;
            int img2x = mainImg.Width;
            int img2y = mainImg.Height;
            //最小宽度
            double min_x = img1x > img2x ? img2x : img1x;
            //最小高度
            double min_y = img1y > img2y ? img2y : img1y;

            double score = 0;
            //重叠的黑色像素
            for (var x = 0; x < min_x; x++)
            {
                for (var y = 0; y < min_y; y++)
                {
                    if (compareImg.GetPixel(x, y).ToArgb() == Color.Black.ToArgb()
                        && compareImg.GetPixel(x, y).ToArgb() == mainImg.GetPixel(x, y).ToArgb())
                    {
                        score++;
                    }
                }
            }
            double originalBlackCount = 0;
            //对比图片的黑色像素
            for (var x = 0; x < img1x; x++)
            {
                for (var y = 0; y < img1y; y++)
                {
                    if (Color.Black.ToArgb() == compareImg.GetPixel(x, y).ToArgb())
                    {
                        originalBlackCount++;
                    }
                }
            }
            return score / originalBlackCount;
        }
        public string RecognizeCheckCodeImg(string path)
        {
            Bitmap bitImg = new Bitmap(path);
            return RecognizeCheckCodeImg(bitImg);
        }

        /// <summary>
        /// 用所有的学习的图片对比当前图，通过黑色和图片比率获取最大相似度的字符图片，从而识别
        /// </summary>
        /// <param name="imgArr">要识别图片的数组</param>
        /// <returns>识别后的字符串</returns>
        public string RecognizeCheckCodeImg(Bitmap bitImg)
        {
            Bitmap EZHimg = EZH(bitImg);
            Bitmap[] imgArr = SplitImage(EZHimg);
            string returnString = string.Empty;
            for (int i = 0; i < imgArr.Length; i++)
            {
                if (imgArr[i] == null)
                {
                    continue;
                }
                var img = imgArr[i];
                if (img == null)
                {
                    continue;
                }
                string[] detailPathList = Directory.GetDirectories(samplePath);
                if (detailPathList == null || detailPathList.Length == 0)
                {
                    continue;
                }
                string resultString = string.Empty;
                //config.txt 文件中指定了识别字母的顺序
                string configPath = samplePath + "config.txt";
                if (!File.Exists(configPath))
                {
                    Console.WriteLine("config.txt文件不存在,无法识别");
                    return null;
                }
                string configString = File.ReadAllText(configPath);
                double maxRate = 0;//相似度  最大1
                foreach (char resultChar in configString)
                {
                    string charPath = samplePath + resultChar.ToString();//特征目录存储路径
                    if (!Directory.Exists(charPath))
                    {
                        continue;
                    }
                    string[] fileNameList = Directory.GetFiles(charPath);
                    if (fileNameList == null || fileNameList.Length == 0)
                    {
                        continue;
                    }


                    foreach (string filename in fileNameList)
                    {
                        Bitmap imgSample = new Bitmap(filename);
                        //过滤宽高相差太大的
                        if (Math.Abs(imgSample.Width - img.Width) >= 2
                            || Math.Abs(imgSample.Height - img.Height) >= 3)
                        {
                            continue;
                        }
                        //当前相似度                       
                        double currentRate = CompareImg(imgSample, img);
                        if (currentRate > maxRate)
                        {
                            maxRate = currentRate;
                            resultString = resultChar.ToString();
                        }
                        imgSample.Dispose();
                    }
                }
                returnString = returnString + resultString;
            }
            return returnString;
        }
        #endregion

    }
}
