using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FlurrySharp
{
    public partial class SettingsForm : Form
    {
        int selectedItem=0;

        public int SelectedItem
        {
            get { return selectedItem; }
            set
            {
                comboBox1.SelectedIndex =selectedItem = value;
            }
        }

        public FlurrySpec[] SpecItems
        {
            set
            {
                foreach (FlurrySpec s in value)
                {
                    comboBox1.Items.Add(s.name);
                }
            }
        }

        public SettingsForm()
        {
            InitializeComponent();
        }

        private void comboSelChanged(object sender, EventArgs e)
        {
            selectedItem = comboBox1.SelectedIndex;
        }
    }
}