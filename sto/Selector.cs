using sto.DataObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sto
{
    public partial class Selector : Form
    {
        private readonly List<SerieHolder> serieHolders;
        private readonly Main main;
        public Dictionary<TreeNode, Vater> Dic { get; set; } = new Dictionary<TreeNode, Vater>();
        public Selector(Main main, List<SerieHolder> serieHolders)
        {
            this.serieHolders = serieHolders;
            this.main = main;

            InitializeComponent();
        }

        private void Selector_Load(object sender, EventArgs e)
        {
            foreach (SerieHolder holder in serieHolders)
            {
                var t = new TreeNode(holder.Serie.Name) { ForeColor = (holder.Serie.Enable ? Color.Black : Color.Gray) };
                Dic.Add(t, holder.Serie);
                foreach (var staffel in holder.Serie.Staffeln)
                {
                    var t2 = new TreeNode(staffel.Name) { ForeColor = (staffel.Enable ? Color.Black : Color.Gray) };
                    Dic.Add(t2, staffel);
                    foreach (var folge in staffel.folgen)
                    {
                        var t3 = new TreeNode(folge.Name) { ForeColor = (folge.Enable ? Color.Black : Color.Gray) };
                        Dic.Add(t3, folge);
                        t2.Nodes.Add(t3);
                    }
                    t.Nodes.Add(t2);
                }
                treeView1.Nodes.Add(t);
            }

        }
        private void Selector_FormClosing(object sender, FormClosingEventArgs e)
        {
            main.Enabled = true;

            foreach (var item in serieHolders)
                new Thread(() =>
                {
                    item.RefeshHosts();
                    item.Save();
                }).Start();
        }

        private void TreeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var f = Dic[treeView1.SelectedNode];
                f.Enable = !f.Enable;
                if (f.Enable)
                    treeView1.SelectedNode.ForeColor = Color.Black;
                else
                    treeView1.SelectedNode.ForeColor = Color.Gray;
            }
        }
    }
}
