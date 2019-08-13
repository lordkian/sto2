using System;
using System.Threading;
using System.Windows.Forms;

namespace sto
{
    public partial class Setting : Form
    {
        public Main Main { get; set; }
        public Setting(Main main)
        {
            InitializeComponent();
            Main = main;
            numericUpDown1.Value = Process.DownloadThreadNumber;
            numericUpDown2.Value = Process.ProcessThreadNumber;
            numericUpDown3.Value = Captcha.CaptchaThreadNumber;
            numericUpDown5.Value = Captcha.CaptchaNumberToSave;
            numericUpDown4.Value = Main.KeepingHost;
            checkBox1.Checked = main.Proxy;
            textBox1.Text = main.ProxyString;
        }

        private void Setting_FormClosing(object sender, FormClosingEventArgs e)
        {
            Main.Enabled = true;
            new Thread(() => { Main.SaveSetting(); Main.RefeshHosts(); }).Start();
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Process.DownloadThreadNumber = (int)numericUpDown1.Value;
        }

        private void NumericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            Process.ProcessThreadNumber = (int)numericUpDown2.Value;

        }

        private void NumericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            Captcha.CaptchaThreadNumber = (int)numericUpDown3.Value;
        }

        private void NumericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            Main.KeepingHost = (int)numericUpDown4.Value;
        }

        private void NumericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            Captcha.CaptchaNumberToSave = (int)numericUpDown5.Value;
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            Main.Proxy = checkBox1.Checked;
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            Main.ProxyString = textBox1.Text;
        }
    }
}
