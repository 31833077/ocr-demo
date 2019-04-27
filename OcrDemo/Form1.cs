using asprise_ocr_api;
using OcrDemo.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tesseract;

namespace OcrDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Path.Combine(Application.StartupPath, "test");
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //PictureBox控件显示图片
                pictureBox1.Load(openFileDialog1.FileName);
                //获取用户选择文件的后缀名 
                string extension = Path.GetExtension(openFileDialog1.FileName);
                //声明允许的后缀名 
                string[] str = new string[] { ".jpg", ".png" };
                if (!str.Contains(extension))
                {
                    MessageBox.Show("仅能上传jpg,png格式的图片！");
                }
                else
                {
                    //识别图片文字
                    var img = new Bitmap(openFileDialog1.FileName);
                    var ocr = new TesseractEngine("./tessdata", "eng", EngineMode.Default);
                    var page = ocr.Process(img, PageSegMode.SingleLine);
                    label1.Text = page.GetText();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Path.Combine(Application.StartupPath, "test");
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            //PictureBox控件显示图片
            pictureBox1.Load(openFileDialog1.FileName);
            //获取用户选择文件的后缀名 
            string extension = Path.GetExtension(openFileDialog1.FileName);
            //声明允许的后缀名 
            string[] str = new string[] { ".jpg", ".png" };
            if (!str.Contains(extension))
            {
                MessageBox.Show("仅能上传jpg,png格式的图片！");
            }
            else
            {
                AspriseOCR.SetUp();
                AspriseOCR ocr = new AspriseOCR();
                ocr.StartEngine("eng", AspriseOCR.SPEED_FASTEST);
                string s = ocr.Recognize(openFileDialog1.FileName, -1, -1, -1, -1, -1, AspriseOCR.RECOGNIZE_TYPE_ALL, AspriseOCR.OUTPUT_FORMAT_PLAINTEXT);
                label1.Text = s;
               ocr.StopEngine();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var path = ReadImage();
            if (string.IsNullOrWhiteSpace(path))
                return;
            UnCodeBase unCodeBase = new UnCodeBase(path);

            unCodeBase.GrayByPixels(); //灰度处理
            var val = unCodeBase.GetDgGrayValue();

            unCodeBase.GetPicValidByValue(val, 4); //得到有效空间
            unCodeBase.ClearPicBorder(1);

            //Bitmap[] pics = unCodeBase.GetSplitPics(4, 1);     //分割
            //StringBuilder sb = new StringBuilder();
            //foreach(var pic in pics)
            //{
            //    string code = unCodeBase.GetSingleBmpCode(pic, 128);   //得到代码串
            //    sb.Append(code).Append(",");
            //}
            //label1.Text = sb.ToString();
            CheckCodeRecognize checkCodeRecognize = new CheckCodeRecognize();
            var img2 =  checkCodeRecognize.EZH( unCodeBase.bmpobj);
            pictureBox2.Image = checkCodeRecognize.DropDisturb(img2);
            var ocr = new TesseractEngine("./tessdata", "eng", EngineMode.Default);
            var page = ocr.Process(img2, PageSegMode.SingleLine);
            label1.Text = page.GetText();

            return;
            //---------------------------------
            //Bitmap[] bitmaps = unCodeBase.GetSplitPics(4, 1);     //分割
            //StringBuilder sb = new StringBuilder();
            //flowLayoutPanel1.Controls.Clear();
            //StringBuilder sbTxt = new StringBuilder();
            //foreach (var bitmap in bitmaps)
            //{
            //    PictureBox pic = new PictureBox();
            //    pic.Image = bitmap;
            //    pic.SizeMode = PictureBoxSizeMode.AutoSize;
            //    pic.BorderStyle = BorderStyle.Fixed3D;
            //    flowLayoutPanel1.Controls.Add(pic);
            //    var ocr = new TesseractEngine("./tessdata", "eng", EngineMode.Default);
            //    var page = ocr.Process(bitmap, PageSegMode.SingleChar);
            //    sbTxt.Append( page.GetText()).Append(",");
            //}
            //label1.Text = sbTxt.ToString();
            //  unCodeBase.bmpobj.Save("d:\\111.png");
        }


        private string ReadImage()
        {
            openFileDialog1.InitialDirectory = Path.Combine(Application.StartupPath, "test");
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return "";

            //PictureBox控件显示图片
            pictureBox1.Load(openFileDialog1.FileName);
            //获取用户选择文件的后缀名 
            string extension = Path.GetExtension(openFileDialog1.FileName);
            //声明允许的后缀名 
            string[] str = new string[] { ".jpg", ".png" };
            if (!str.Contains(extension))
            {
                MessageBox.Show("仅能上传jpg,png格式的图片！");
                return "";
            }
            return openFileDialog1.FileName;
      }

        private void button4_Click(object sender, EventArgs e)
        {
            var path = ReadImage();
            if (string.IsNullOrWhiteSpace(path))
                return;
            //Console.WriteLine("正在下载验证码......");
            //CookieContainer cc = new CookieContainer();
            //byte[] imgByte = HttpWebRequestForBPMS.GetWebResorce("http://hd.cnrds.net/hd/login.do?action=createrandimg",cc);
            //MemoryStream ms1 = new MemoryStream(imgByte);
            // Bitmap bm = (Bitmap)Image.FromStream(ms1); 
            //Bitmap img = HttpWebRequestForBPMS.GetWebImage("http://hd.cnrds.net/hd/login.do?action=createrandimg");
            //Console.WriteLine("验证码下载成功，正在识别.....");
            CheckCodeRecognize regImg = new CheckCodeRecognize();
            string regResult = regImg.RecognizeCheckCodeImg(path);
            Console.WriteLine("验证码识别成功，验证码结果为：" + regResult);

        }
    }
}
