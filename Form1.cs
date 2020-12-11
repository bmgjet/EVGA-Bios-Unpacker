using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Romunpacker
{
    public partial class Form1 : Form
    {
//      EVGA Bios unpacker 0.2 by
//     ▄▀▀█▄▄   ▄▀▀▄ ▄▀▄  ▄▀▀▀▀▄          ▄█  ▄▀▀█▄▄▄▄  ▄▀▀▀█▀▀▄ 
//    ▐ ▄▀   █ █  █ ▀  █ █          ▄▀▀▀█▀ ▐ ▐  ▄▀   ▐ █    █  ▐ 
//      █▄▄▄▀  ▐  █    █ █    ▀▄▄  █    █      █▄▄▄▄▄  ▐   █     
//      █   █    █    █  █     █ █ ▐    █      █    ▌     █      
//     ▄▀▄▄▄▀  ▄▀   ▄▀   ▐▀▄▄▄▄▀ ▐   ▄   ▀▄   ▄▀▄▄▄▄    ▄▀       
//    █    ▐   █    █    ▐            ▀▀▀▀    █    ▐   █         
//    ▐        ▐    ▐                         ▐        ▐ 

        public byte[] file;
        public string filename;
        public int address;
        public Form1()
        {
            InitializeComponent();
            this.BringToFront();
        }

        string Search(byte[] src, byte[] pattern)
        {
            string foundaddress = "";
            int c = src.Length - pattern.Length + 1;
            int j;
            for (int i = 0; i < c; i++)
            {
                for (j = pattern.Length - 1; j >= 1 && src[i + j] == pattern[j]; j--) ;
                if (j == 0) foundaddress += i + ",";
            }
            return foundaddress;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        bool isDigits(string s)
        {
            if (s == null || s == "") return false;

            for (int i = 0; i < s.Length; i++)
                if ((s[i] ^ '0') > 9)
                    return false;

            return true;
        }

        public void startup()
        {
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open EVGA Update";
            theDialog.Filter = "Update.exe|Update.exe";
            theDialog.InitialDirectory = @"C:\";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                file = System.IO.File.ReadAllBytes(theDialog.FileName.ToString());
                
                //NVUFLASH_STRUCT
                byte[] biostag = new byte[] {0x4E,0x56,0x55,0x46,0x4C,0x41,0x53,0x48,0x5f,0x53,0x54,0x52,0x55,0x43,0x54 };

                string found = Search(file, biostag);
                string[] result = found.Split(',');

                foreach (string s in result)
                {
                    if (isDigits(s))
                    {
                            listBox1.Items.Add(s);
                    }
                }
            }
            else
            {
                this.Close();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string text = listBox1.GetItemText(listBox1.SelectedItem);
                address = int.Parse(text);


                byte[] Biosname = new byte[19];
                for (int i = 0; i < 19; i++)
                {
                    Biosname[i] = file[i + address + 29];
                }

                byte[] BiosVersion = new byte[14];
                for (int i = 0; i < 14; i++)
                {
                    BiosVersion[i] = file[i + address + 112];
                }

                filename = Encoding.ASCII.GetString(Biosname);
                label1.Text = "Bios: " + filename;
                label2.Text = "Version: " + Encoding.ASCII.GetString(BiosVersion);
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = "C:\\";      
            saveFileDialog1.Title = "Save Bios File";
            saveFileDialog1.DefaultExt = "rom";
            saveFileDialog1.FileName = filename;
            saveFileDialog1.Filter = "rom files (*.rom)|*.rom";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                byte[] rom = new byte[999424];
                for (int i = 0; i < 999424; i++)
                {
                    rom[i] = file[i + address + 144];
                }
                File.WriteAllBytes(saveFileDialog1.FileName, rom);
                MessageBox.Show("File Saved");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            label2.Text = "Version: ";
            label1.Text = "Bios: ";
            startup();
        }
    }
}
