using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;

            string[] docs = Directory.GetFiles(textBox1.Text, "*.pdf");
            int rec = 0;
            foreach (var item in docs)
            {
                rec += 1;
                Rasterizer r = new Rasterizer();
                r.totalDocs = docs.Length;
                r.currentIndex = rec;
                r.Form = this;
                r.pageFrom = 1;
                r.pageTo = 100;
                r.inputFile = item;
                r.outputFile = Path.Combine(Path.GetDirectoryName(item) , "page-%03d.png");
                r.FileName = Path.GetFileNameWithoutExtension(item);
                r.Start();
            }


            button1.Enabled = true;
            MessageBox.Show("Done!");

        }

        private void button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            textBox1.Text = folderBrowserDialog1.SelectedPath;
        }
    }
}
