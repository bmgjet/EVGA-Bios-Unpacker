using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Romunpacker
{
    public partial class Form1 : Form
    {
        //      EVGA Bios unpacker 0.4 by
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
            theDialog.Filter = "Update.exe|*.exe";
            theDialog.InitialDirectory = @"C:\";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                file = System.IO.File.ReadAllBytes(theDialog.FileName.ToString());

                //NVUFLASH_STRUCT
                byte[] biostag = new byte[] { 0x4E, 0x56, 0x55, 0x46, 0x4C, 0x41, 0x53, 0x48, 0x5f, 0x53, 0x54, 0x52, 0x55, 0x43, 0x54 };

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

                //type 1 bios
                int PowerTable = 564097 + address;
                PowerTable += 8969;
                byte[] PowerLimit = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    PowerLimit[i] = file[i + PowerTable];
                }
                PowerTable += 8969;
                byte[] BoostLimit = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    BoostLimit[i] = file[i + PowerTable];
                }
                PowerTable += 4443;
                int Speed = BitConverter.ToInt32(BoostLimit, 0) / 1000;
                int decWat = BitConverter.ToInt32(PowerLimit, 0) / 1000;

                //Switch to type 2 if invalid data.
                if (decWat > 1001 || decWat < 200)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        PowerLimit[i] = file[i + PowerTable];
                    }
                    PowerTable -= 9042;
                    for (int i = 0; i < 4; i++)
                    {
                        BoostLimit[i] = file[i + PowerTable];
                    }
                    Speed = BitConverter.ToInt32(PowerLimit, 0) / 1000;
                    decWat = BitConverter.ToInt32(BoostLimit, 0) / 1000;
                }

                //Switch to type 3
                if (decWat > 1001 || decWat < 200)
                {
                    PowerTable = 564097 + address + 0xCB4;
                    PowerTable += 8969;
                    for (int i = 0; i < 4; i++)
                    {
                        PowerLimit[i] = file[i + PowerTable];
                    }
                    PowerTable += 8969;
                    for (int i = 0; i < 4; i++)
                    {
                        BoostLimit[i] = file[i + PowerTable];
                    }
                    PowerTable += 4443;
                    Speed = BitConverter.ToInt32(BoostLimit, 0) / 1000;
                    decWat = BitConverter.ToInt32(PowerLimit, 0) / 1000;
                }

                


                byte[] crc = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    crc[i] = file[i + address + 96];
                }
                var ReadCert = BitConverter.ToUInt32(crc,0);


                byte[] rom = new byte[999424];
                for (int i = 0; i < 999424; i++)
                {
                    rom[i] = file[i + address + 144];
                }

                var CalculatedCert = HULKCert_Algo(rom, 999424);

                if (ReadCert == CalculatedCert)
                {
                    label6.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    label6.ForeColor = System.Drawing.Color.Red;
                }
                label6.Text = "CRC: " + ReadCert.ToString("X2") + " [" + CalculatedCert.ToString("X2") + "]";
                string cardtype = "";
                ushort DeviceID = BitConverter.ToUInt16(rom, 37744 + 6);
                if (DeviceID == 0x2204)
                    cardtype = "RTX 3090";
                else if (DeviceID == 0x2206)
                    cardtype = "RTX 3080";

                this.Text = "EVGA Bios Extractor 0.4 - "+ cardtype;

                if (Speed == 0)
                {
                    if (DeviceID == 0x2204)
                        Speed = 1695;
                    else
                        Speed = 1710;
                }

                filename = Encoding.ASCII.GetString(Biosname);
                label1.Text = "Bios: " + filename;
                label2.Text = "Version: " + Encoding.ASCII.GetString(BiosVersion);
                label4.Text = "PowerLimit: " + decWat.ToString() + "W";
                label5.Text = "Boost Clock: " + Speed.ToString() + "Mhz";

                if (Speed < 2100 && Speed > 1395)
                {
                    button1.Enabled = true;
                    button3.Enabled = true;
                }
                else
                {
                    button1.Enabled = false;
                    button3.Enabled = false;
                }
            }
            catch
            {
                button1.Enabled = false;
                button3.Enabled = false;
            }
        }



        private ulong HULKCert_Algo(byte[] param_1, long param_2)

        {
            uint uVar1;
            bool bVar2;
            uint local_28;
            uint local_24;
            uint local_18;

            local_24 = 0xffffffff;
            local_28 = 0;
            while (local_28 < param_2)
            {
                local_24 = local_24 ^ (uint)(param_1[local_28]) << 0x18;
                local_18 = 0;
                while (local_18 < 8)
                {
                    uVar1 = local_24 << 1;
                    bVar2 = (int)local_24 < 0;
                    local_24 = uVar1;
                    if (bVar2)
                    {
                        local_24 = uVar1 ^ 0x4c11db7;
                    }
                    local_18 = local_18 + 1;
                }
                local_28 = local_28 + 1;
            }
            return (ulong)local_24;
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
            label4.Text = "PowerLimit: ";
            label5.Text = "Boost Clock: ";
            startup();
        }


        internal static string StringFromHex(byte[] byte_0, int int_0, int int_1)
        {
            string @string = Encoding.ASCII.GetString(byte_0, int_0, int_1);
            int num = @string.IndexOf('\0');
            if (num > -1)
            {
                return @string.Substring(0, num).Trim();
            }
            return @string.Trim();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Open Bios Rom";
            theDialog.Filter = ".rom|*.rom";
            theDialog.InitialDirectory = @"C:\";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                byte[] pfile = System.IO.File.ReadAllBytes(theDialog.FileName.ToString());
                if (pfile.Length == 999424)
                {

                    string PBiosVersion = StringFromHex(pfile, 37569, 14);

                    DialogResult dialogResult = MessageBox.Show("Are you sure you want to patch the exe with this .rom?" + Environment.NewLine + label2.Text + " to " + PBiosVersion, "Patch:", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        //write bios version string
                        for (int i = 0; i < 14; i++)
                        {
                            file[i + address + 112] = pfile[i + 37569];
                        }

                        //Fix cert
                        var CalculatedCert = HULKCert_Algo(pfile, 999424);
                        byte[] HulkCert = BitConverter.GetBytes(CalculatedCert);
                        for (int i = 0; i < 4; i++)
                        {
                            file[i + address + 96] = HulkCert[i];
                        }


                        int patchedbyte = 0;
                        foreach (byte b in pfile)
                        {
                            file[address + patchedbyte + 112 + 32] = b;
                            patchedbyte++;
                        }

                        SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                        saveFileDialog1.Title = "Save Patched  update.exe";
                        saveFileDialog1.DefaultExt = "exe";
                        saveFileDialog1.Filter = "exe (*.exe)|*.exe";
                        saveFileDialog1.FilterIndex = 2;
                        saveFileDialog1.RestoreDirectory = true;
                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            File.WriteAllBytes(saveFileDialog1.FileName, file);
                            MessageBox.Show("File Saved");
                        }

                    }
                }
                else
                {
                    MessageBox.Show("Wrong length bios rom!");
                }
                }
            }
        }
    }