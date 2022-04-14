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
                if ((status & 1) == 1) radioButton1.Checked = true; else radioButton1.Checked = false; 
                if ((status & 2) == 2) radioButton2.Checked = true; else radioButton2.Checked = false;
                if ((status & 4) == 4) radioButton3.Checked = true; else radioButton3.Checked= false;
                if ((status & 8) == 8) radioButton4.Checked = true; else radioButton4.Checked= false;
            }
            groupBox1.Text = myRelay.serialNumber;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            //myRelay.AllOn();
            if (radioButton1.Checked)
            {
                myRelay.Set(1, 0);
                myRelay.Set(8, 1);
            }
            //myRelay.AllOff();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                myRelay.Set(2, 0);
                myRelay.Set(7, 1);
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                myRelay.Set(3, 0);
                myRelay.Set(6, 1);
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
            {
                myRelay.Set(4, 0);
                myRelay.Set(5, 1);
            }
        }
    }
}
