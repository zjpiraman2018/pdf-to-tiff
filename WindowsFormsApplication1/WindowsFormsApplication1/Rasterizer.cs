using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ghostscript.NET.Processor;
using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using System.Net;

namespace WindowsFormsApplication1
{
    class Rasterizer
    {
        public Form1 Form { get; set; }
        public string FileName { get; set; }
        public string inputFile { get; set; }
        public string outputFile { get; set; }
        public int pageFrom { get; set; }
        public int pageTo { get; set; }
        private int totalPage { get; set; }
        public int currentIndex { get; set; }
        public int totalDocs { get; set; }


        public void Start()
        {

            this.totalPage = GetNoOfPagesPDF(inputFile);

            using (GhostscriptProcessor ghostscript = new GhostscriptProcessor())
            {
                ghostscript.Processing += new GhostscriptProcessorProcessingEventHandler(ghostscript_Processing);
                ghostscript.Completed += Ghostscript_Completed;
                List<string> switches = new List<string>();
                switches.Add("-empty");
                switches.Add("-dSAFER");
                switches.Add("-dBATCH");
                switches.Add("-dNOPAUSE");
                switches.Add("-dNOPROMPT");
                switches.Add("-dFirstPage=" + pageFrom.ToString());
                switches.Add("-dLastPage=" + pageTo.ToString());
                switches.Add("-sDEVICE=png16m");
                switches.Add("-r150");
                switches.Add("-dTextAlphaBits=4");
                switches.Add("-dGraphicsAlphaBits=4");
                switches.Add(@"-sOutputFile=" + outputFile);
                switches.Add(@"-f");
                switches.Add(inputFile);

                ghostscript.Process(switches.ToArray());
            }
        }

        private void Ghostscript_Completed(object sender, GhostscriptProcessorEventArgs e)
        {
            MergeImage(Path.GetDirectoryName(inputFile));
        }

        void ghostscript_Processing(object sender, GhostscriptProcessorProcessingEventArgs e)
        {
            this.Form.Text = "Converted: " + currentIndex.ToString() + " / Total Document: "  + totalDocs.ToString() + " / Processing: [" + this.FileName +"  ("+ ((e.CurrentPage.ToString() + "/" + totalPage.ToString())).ToString() + ")]" ;
        }


        public static int GetNoOfPagesPDF(string FileName)
        {
            int result = 0;
            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            StreamReader r = new StreamReader(fs);
            string pdfText = r.ReadToEnd();

            System.Text.RegularExpressions.Regex regx = new Regex(@"/Type\s*/Page[^s]");
            System.Text.RegularExpressions.MatchCollection matches = regx.Matches(pdfText);
            result = matches.Count;
            return result;

        }

        public bool isNumeric(string text)
        {
            Regex reNum = new Regex(@"^\d+$");
            return reNum.Match(text).Success;
        }

        public string ConvertToTiff(string dir, string name) {
            string tif = Path.Combine(dir,  name + ".tif");
            string pngFile = Path.Combine(dir, "page-"+ ("000" + name).Substring(("000" + name).Length - 3, 3) + ".png");
            var png = Image.FromFile(pngFile);
            png.Save(tif, ImageFormat.Tiff);
            png.Dispose();
            png = null;
            File.Delete(pngFile);
            return tif;
        }

        public bool MergeImage(string Folder)
        {
            bool bRetval = false;
            for (int i = 1; i <= this.totalPage; i++)
            {
                ConvertToTiff(Folder, i.ToString());
            }

            string[] aImg2 = Directory.GetFiles(Folder, "*.tif");
            TiffBitmapEncoder encoder = new TiffBitmapEncoder();

            using (FileStream stream = new FileStream(Path.Combine(Folder, this.FileName +  ".tif"), FileMode.OpenOrCreate))
            {
                foreach (var img in aImg2.Where(a => isNumeric(Path.GetFileNameWithoutExtension(a))))
                {
                    BitmapFrame frame = BitmapFrame.Create(new System.Uri(img), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    encoder.Compression = TiffCompressOption.Lzw; // compress to 
                    encoder.Frames.Add(frame);
                }
                encoder.Save(stream);
            }

            foreach (var img in aImg2.Where(a =>  isNumeric(Path.GetFileNameWithoutExtension(a))))
            {
                File.Delete(img);
            }




            return bRetval;

        }


    }



}
