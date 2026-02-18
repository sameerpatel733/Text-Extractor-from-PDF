using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Tesseract;
using System.Drawing;
using System;
using PdfiumViewer;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;


namespace TextExtractor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string selectedFile = "";
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "PDF Files (*.pdf)|*.pdf";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                selectedFile = openFileDialog1.FileName;
                tbxpath.Text = selectedFile;
            }
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFile))
            {
                MessageBox.Show("Please select PDF first ");
                return;
            }

            tbxOutput.Clear();
            tbxOutput.Visible = true;

            using (UglyToad.PdfPig.PdfDocument document = UglyToad.PdfPig.PdfDocument.Open(selectedFile))
            {
                foreach (UglyToad.PdfPig.Content.Page page in document.GetPages())
                {
                    tbxOutput.AppendText($"--- Page {page.Number} --- \r\n");
                    if (string.IsNullOrWhiteSpace(page.Text))
                    {
                        tbxOutput.AppendText("Scanned Page Detected... Running OCR...\r\n");

                        string ocrText = RunOCR(selectedFile, page.Number);
                        tbxOutput.AppendText(ocrText);
                    }
                    else
                    {
                        tbxOutput.AppendText(page.Text);
                    }
                    tbxOutput.AppendText("\r\n\r\n");
                }
            }

            MessageBox.Show("Text Extract Complate");
        }

        private string RunOCR(string path, int pageNumber)
        {
            string extractedText = "";
            using (var document = PdfiumViewer.PdfDocument.Load(path))
            {
                using (var image = document.Render(pageNumber - 1, 300, 300, true))
                {
                    using (var ms = new MemoryStream())
                    {
                        Bitmap bitmap = new Bitmap(image);
                        Bitmap gray = MakeGrayscale(bitmap);
                        gray.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        
                        ms.Position = 0;

                        using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.LstmOnly))
                        {
                            engine.DefaultPageSegMode = PageSegMode.Auto;
                            using (var img = Pix.LoadFromMemory(ms.ToArray()))
                            {
                                using (var page = engine.Process(img))
                                {
                                    extractedText = page.GetText();
                                }
                            }
                        }
                    }
                }
            }
            return extractedText;
        }

        private Bitmap MakeGrayscale(Bitmap original) 
        {
            Bitmap newbitmap = new Bitmap(original.Width, original.Height);

            using (Graphics g = Graphics.FromImage(newbitmap)) 
            {
                ColorMatrix colorMatrix = new ColorMatrix(
                    new float[][] 
                    {
                        new float[] {.3f, .3f, .3f, 0, 0},
                        new float[] {.59f, .59f, .59f, 0, 0},
                        new float[] {.11f, .11f, .11f, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0, 0, 1}
                    }
                    );

                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);

                g.DrawImage(original,
                    new Rectangle(0, 0, original.Width, original.Height),
                    0, 0, original.Width, original.Height,
                    GraphicsUnit.Pixel, attributes);
            }
            return newbitmap;
        }
    }
}
