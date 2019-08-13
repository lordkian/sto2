using Newtonsoft.Json;
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

namespace List_to_SH
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            var d = new OpenFileDialog() { Filter = "JSON file|*.json" };
            var res = d.ShowDialog();
            if (res == DialogResult.Cancel || res == DialogResult.Abort || res == DialogResult.No)
                return;
            var sr = new StreamReader(d.FileName);
            var list = JsonConvert.DeserializeObject<List<List<string>>>(sr.ReadToEnd());

            var all = new List<string>();
            foreach (var item in list)
            {
                var fileName = item[0];
                item.Remove(fileName);
                foreach (var item2 in item)
                    all.Add(textBox1.Text.Replace("<FileName>", fileName).Replace("<Link>", item2));
            }

            var sw = new List<StreamWriter>();
            for (int i = 0; i < numericUpDown1.Value; i++)
                sw.Add(new StreamWriter(d.FileName.Replace(".json", i + ".sh")));
            while (all.Count > 0)
                foreach (var item in sw)
                {
                    if (all.Count == 0)
                        break;
                    var tmp = all.First();
                    all.Remove(tmp);
                    item.WriteLine(tmp);
                    item.Flush();
                }
            foreach (var item in sw)
                item.Close();
        }
    }
}
