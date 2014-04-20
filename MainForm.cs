﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShutdownPlannerGUI {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();
            this.Icon = Properties.Resources.Sign_06;
        }

        private void buttonShutdownSubmit_Click(object sender, EventArgs e) {
            if (this.backgroundWorker.IsBusy) return;
            this.enableGuiRec(this.groupBox1.Controls, false);
            this.enableGuiRec(this.groupBox2.Controls, false);
            Application.DoEvents();

            System.Threading.Thread.Sleep(1000);

            this.enableGuiRec(this.groupBox1.Controls, true);
            this.enableGuiRec(this.groupBox2.Controls, true);
        }

        private void buttonAbortSubmit_Click(object sender, EventArgs e) {
            if (this.backgroundWorker.IsBusy) return;

            this.enableGuiRec(this.groupBox1.Controls, false);
            this.enableGuiRec(this.groupBox2.Controls, false);
            Application.DoEvents();
            this.textBoxOutput.ResetText();
            this.ConsoleWriteLine("shutdown /a");
            this.ConsoleWriteLine("");
            this.backgroundWorker.RunWorkerAsync("/a");
        }

        private void enableGuiRec(Control.ControlCollection controlCollection, bool p) {
            foreach (Control c in controlCollection) {
                c.Enabled = p;
                if ((c.Controls != null) && (c.Controls.Count > 0)) {
                    this.enableGuiRec(c.Controls, p);
                }
            }
        }

        private void proc_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            try {
                if (e.Data == null) return;
                this.ConsoleWriteLine(e.Data);
                //if (!String.IsNullOrEmpty(e.Data)) {
                //    string line = ((enc_out != null) && (enc_in != null))
                //        ? enc_out.GetString(enc_in.GetBytes(e.Data)) : e.Data;
                //    Console.Error.WriteLine(line);
                //} else Console.Error.WriteLine();
            } catch { }
        }

        private void ConsoleWriteLine(string p) {
            if (this.InvokeRequired) {
                this.Invoke(new Action<string>(this.ConsoleWriteLine), new object[] { p });
                return;
            }
            if (String.IsNullOrEmpty(p)) p = "\r\n";
            else if (!p.EndsWith("\r\n")) p += "\r\n";
            this.textBoxOutput.AppendText(p);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            string arguments = (string)e.Argument;

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.Arguments = arguments;
            psi.CreateNoWindow = true;
            psi.FileName = "shutdown";
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.WorkingDirectory = Environment.CurrentDirectory;

            Process proc = new Process();
            proc.StartInfo = psi;
            proc.OutputDataReceived += proc_OutputDataReceived;
            proc.ErrorDataReceived += proc_OutputDataReceived;
            proc.EnableRaisingEvents = true;
            proc.Start();

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            proc.WaitForExit();

            e.Result = proc.ExitCode;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            this.enableGuiRec(this.groupBox1.Controls, true);
            this.enableGuiRec(this.groupBox2.Controls, true);
            this.ConsoleWriteLine("");
            this.ConsoleWriteLine("Process completed (" + e.Result.ToString() + ")");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = this.backgroundWorker.IsBusy;
        }

    }
}
