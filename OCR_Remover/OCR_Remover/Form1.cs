using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PdfOcrRemover
{
    public partial class Form1 : Form
    {
        private string rootFolder = "";
        private bool isProcessing = false;

        // Change Ghostscript path if needed
        private string gsPath = @"C:\Program Files\gs\gs10.06.0\bin\gswin64c.exe";

        public Form1()
        {
            InitializeComponent();
        }

        // SELECT FOLDER
        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    rootFolder = dialog.SelectedPath;
                    txtFolder.Text = rootFolder;
                    Log("Selected Folder: " + rootFolder);
                }
            }
        }

        // START PROCESS
        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(rootFolder))
            {
                MessageBox.Show("Please select folder first.");
                return;
            }

            if (!File.Exists(gsPath))
            {
                MessageBox.Show("Ghostscript not found.\nCheck path:\n" + gsPath);
                return;
            }

            if (isProcessing) return;

            isProcessing = true;
            btnStart.Enabled = false;
            progressBar1.Value = 0;
            lblPercent.Text = "0%";

            await Task.Run(ProcessFolder);

            isProcessing = false;
            btnStart.Enabled = true;

            MessageBox.Show("Task Completed Successfully!", "Done",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            Application.Exit();
        }

        // PROCESS ALL PDFs
        private void ProcessFolder()
        {
            var pdfFiles = Directory.GetFiles(rootFolder, "*.pdf", SearchOption.AllDirectories)
                                    .Where(f => !f.Contains(Path.Combine(rootFolder, "Output")))
                                    .ToArray();

            int total = pdfFiles.Length;
            int done = 0;

            foreach (var file in pdfFiles)
            {
                try
                {
                    FlattenPdf(file);
                    done++;

                    int percent = total == 0 ? 0 : (done * 100) / total;

                    UpdateUI(percent, $"Processed: {file}");
                }
                catch (Exception ex)
                {
                    UpdateUI(null, $"Skipped: {Path.GetFileName(file)} → {ex.Message}");
                }
            }
        }

        // SAFE UI UPDATE
        private void UpdateUI(int? percent, string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateUI(percent, message)));
                return;
            }

            if (percent.HasValue)
            {
                progressBar1.Value = Math.Min(percent.Value, 100);
                lblPercent.Text = percent + "%";
            }

            Log(message);
        }

        // FLATTEN PDF
        private void FlattenPdf(string pdfPath)
        {
            string relativePath = GetRelativePath(rootFolder, pdfPath);
            string outputPath = Path.Combine(rootFolder, "Output", relativePath);

            string dir = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string args = $"-dNOPAUSE -dBATCH -sDEVICE=pdfwrite " +
                          $"-dFILTERTEXT -dFILTERVECTOR " +
                          $"-sOutputFile=\"{outputPath}\" \"{pdfPath}\"";

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = gsPath,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using (Process proc = Process.Start(psi) ?? throw new Exception("Failed to start Ghostscript"))
            {
                proc.WaitForExit();

                if (proc.ExitCode != 0)
                    throw new Exception("Ghostscript failed");
            }
        }

        // RELATIVE PATH HELPER
        private string GetRelativePath(string basePath, string fullPath)
        {
            Uri baseUri = new Uri(basePath.EndsWith("\\") ? basePath : basePath + "\\");
            Uri fullUri = new Uri(fullPath);
            return Uri.UnescapeDataString(
                baseUri.MakeRelativeUri(fullUri)
                       .ToString()
                       .Replace('/', '\\'));
        }

        // THREAD-SAFE LOG
        private void Log(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() =>
                    txtLog.AppendText(message + Environment.NewLine)));
            }
            else
            {
                txtLog.AppendText(message + Environment.NewLine);
            }
        }
    }
}
