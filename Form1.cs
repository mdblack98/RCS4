using System;
using System.IO.Ports;

namespace RCS4
{
    public partial class Form1 : Form
    {
        Relay myRelay;
        public Form1()
        {
            InitializeComponent();
            myRelay = new Relay();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (myRelay.comList.Count > 0)
            {
                myRelay.Open(myRelay.comList[0]);
                int status = myRelay.Status();
                if (status == 2) radioButton1.Checked = true; else radioButton1.Checked = false; 
                if (status == 8) radioButton2.Checked = true; else radioButton2.Checked = false;
                if (status == 32) radioButton3.Checked = true; else radioButton3.Checked= false;
                if (status == 128) radioButton4.Checked = true; else radioButton4.Checked= false;
            }
            groupBox1.Text = myRelay.serialNumber;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            //myRelay.AllOn();
            myRelay.Set(2, radioButton1.Checked ? (byte)1:(byte)0);
            //myRelay.AllOff();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            myRelay.Set(4, radioButton2.Checked ? (byte)1 : (byte)0);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            myRelay.Set(6, radioButton3.Checked ? (byte)1 : (byte)0);
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            myRelay.Set(8, radioButton4.Checked ? (byte)1 : (byte)0);
        }
    }
}
