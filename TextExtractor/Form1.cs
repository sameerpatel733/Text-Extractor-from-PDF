using System;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using OfficeOpenXml;
using PdfiumViewer;
using Tesseract;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace TextExtractor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ExcelPackage.License.SetNonCommercialOrganization("Your Org Name");
        }

        private string selectedFile = "";
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectedFile = ofd.FileName;
                tbxpath.Text = selectedFile;
            }
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFile))
            {
                MessageBox.Show("Please select PDF first ");
            }

            string result = ExtractTextFromFile(selectedFile);
            tbxOutput.Clear();
            tbxOutput.Visible = true;
            tbxOutput.Text = result;
        }

        private string ExtractTextFromFile(string path) 
        {
            string extention = Path.GetExtension(path).ToLower();

            switch (extention) 
            {
                case ".pdf":
                return ExtractPDF(path);

                case ".jpg":
                case ".png":
                    return ExtractImage(path);

                case ".docx":
                    return Extractdocx(path);

                case ".xlsx":
                    return ExtractExcel(path);

                case ".txt":
                case ".csv":
                    return ExtractFromtxt(path);

                default:
                    return "Files not support !!!";
            }
        }

        /*private string ExtractPDF(string path) 
        {
            StringBuilder sb = new StringBuilder();
            
            using (UglyToad.PdfPig.PdfDocument document = UglyToad.PdfPig.PdfDocument.Open(selectedFile))
            {
                foreach (UglyToad.PdfPig.Content.Page page in document.GetPages())
                {
                    tbxOutput.AppendText($"--- Page {page.Number} --- ");
                    if (string.IsNullOrWhiteSpace(page.Text))
                    {
                        sb.AppendLine("Scanned Page Detected... Running OCR...");
                        string ocrText = ExtractImage(selectedFile);
                        sb.AppendLine(ocrText);                        
                    }
                    else
                    {
                        sb.AppendLine(page.Text);
                    }
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }*/

        private string ExtractPDF(string path)
        {
            StringBuilder sb = new StringBuilder();

            using (var document = UglyToad.PdfPig.PdfDocument.Open(path))
            {
                foreach (var page in document.GetPages())
                {
                    sb.AppendLine($"---------------- Page {page.Number} ----------------");
                    sb.AppendLine();

                    var words = page.GetWords()
                         .OrderByDescending(w => w.BoundingBox.Bottom)
                         .ThenBy(w => w.BoundingBox.Left)
                         .ToList();


                    double lastY = double.MaxValue;

                    foreach (var word in words)
                    {
                        if (Math.Abs(word.BoundingBox.Bottom - lastY) > 3)
                        {
                            sb.AppendLine();
                            lastY = word.BoundingBox.Bottom;
                        }

                        sb.Append(word.Text + " ");
                    }

                    sb.AppendLine();
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private string Extractdocx(string path) 
        {
            StringBuilder sb = new StringBuilder();
            using (var doc = WordprocessingDocument.Open(path, false)) 
            {
                var body = doc.MainDocumentPart.Document.Body;
                sb.AppendLine(body.InnerText);
            }
            return sb.ToString();
        }

        private string ExtractExcel(string path) 
        {
            StringBuilder sb = new StringBuilder();
            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                var sheet = package.Workbook.Worksheets[0];
                for (int row = sheet.Dimension.Start.Row; row <= sheet.Dimension.End.Row; row++) 
                {
                    for(int column = sheet.Dimension.Start.Column;column <= sheet.Dimension.End.Column; column++) 
                    {
                        sb.Append(sheet.Cells[row, column].Text + " ");
                    }
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        private string ExtractFromtxt(string path) 
        {
            StringBuilder sb = new StringBuilder();
            var lines = File.ReadAllLines(path);

            foreach (var line in lines) 
            {
                var columns = line.Split(',');
                sb.AppendLine(string.Join("|", columns));
            }
            return sb.ToString();
        }

        private string ExtractImage(string path)
        {
            StringBuilder sb = new StringBuilder();
            string extention = Path.GetExtension(path).ToLower();
            string tessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

            using (var engine = new TesseractEngine(tessPath, "eng", EngineMode.LstmOnly))
            {
                engine.DefaultPageSegMode = PageSegMode.Auto;

                if (extention == ".pdf")
                {
                    using (var document = PdfiumViewer.PdfDocument.Load(path))
                    {
                        int pageCount = document.PageCount;
                        for (int i = 0; i < pageCount; i++)
                        {
                            sb.AppendLine($"--- Page {i + 1} --- ");
                            using (var image = document.Render(i, 600, 600, true))
                            {
                                sb.AppendLine(ProcesImageWithOCR(image, engine));
                            }
                            sb.AppendLine();
                        }
                    }
                }
                else if (extention == ".jpg" || extention == ".png" || extention == ".jpeg")
                {
                    using (Bitmap image = new Bitmap(path))
                    {
                        sb.AppendLine(ProcesImageWithOCR(image, engine));
                    }
                }
            }
            return sb.ToString();
        }


        private string ProcesImageWithOCR(Image image ,TesseractEngine engine) 
        {
            using (var ms = new MemoryStream())
            {
                Bitmap bitmap = new Bitmap(image);
                Bitmap gray = MakeGrayscale(bitmap);
                gray.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                ms.Position = 0;

                using (var img = Pix.LoadFromMemory(ms.ToArray()))
                using (var page = engine.Process(img))
                {
                     return page.GetText();
                }
            }
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
