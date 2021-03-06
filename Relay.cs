using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using static FTD2XX_NET.FTDI;

namespace RCS4
{
    class Relay
    {
        public FTDI? ftdi = new();
        public readonly List<string> comList = new();
        readonly List<uint> comIndex = new();
        public readonly uint devcount = 0;
        string comPort = "";
        int relayNum = 0;
        public string serialNumber = "";
        readonly List<string> serialNums = new();
        public string? errMsg = null;
        readonly bool debug = true;
        readonly string debugFile = Path.GetTempPath() + "rcs4.log";
#pragma warning disable IDE1006 // Naming Styles
        static int __LINE__([System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
#pragma warning restore IDE1006 // Naming Styles
        {
            return lineNumber;
        }
#pragma warning disable IDE1006 // Naming Styles
        static string __FILE__([System.Runtime.CompilerServices.CallerFilePath] string fileName = "")
#pragma warning restore IDE1006 // Naming Styles
        {
            return fileName;
        }
#pragma warning disable IDE1006 // Naming Styles
        static string __FUNC__([System.Runtime.CompilerServices.CallerMemberName] string fileName = "")
#pragma warning restore IDE1006 // Naming Styles
        {
            return fileName;
        }
        public Relay()
        {
            if (File.Exists(debugFile)) File.Delete(debugFile);
            try
            {
                ftdi.SetBaudRate(9600);
                ftdi.GetNumberOfDevices(ref devcount);
                if (devcount == 0)
                {
                    errMsg = "No devices found";
                    return;
                }
                FT_DEVICE_INFO_NODE[] nodes = new FT_DEVICE_INFO_NODE[devcount];
                FT_STATUS status = ftdi.GetDeviceList(nodes);
                uint index = 0;
                uint nRelays = 0;
                if (debug)
                {
                    File.AppendAllText(debugFile, "devcount=" + devcount + "\n");
                }
                foreach (FT_DEVICE_INFO_NODE node in nodes)
                {
                    //if (node.Description.Contains("FT245R"))
                    File.AppendAllText(debugFile, "==========================");
                    File.AppendAllText(debugFile, "SerialNumber=" + node.SerialNumber + "\n");
                    File.AppendAllText(debugFile, "Description=" + node.Description + "\n");
                    File.AppendAllText(debugFile, "ID=" + node.ID + "\n");
                    File.AppendAllText(debugFile, "Type=" + node.Type.ToString() + "\n");
                    var mySerialNumber = node.SerialNumber.Length > 3 ? node.SerialNumber[..3] : "";
                    if (mySerialNumber.Equals("DAE"))
                    {
                        nRelays++;
                        ftdi.OpenByIndex(index);
                        //Nothing unique in the EEPROM to show 4 or 8 channel
                        FT232R_EEPROM_STRUCTURE ee232r = new();
                        ftdi.ReadFT232REEPROM(ee232r);
                        ftdi.GetCOMPort(out string comport);
                        File.AppendAllText(debugFile, "ComPort=" + comport + "\n");
                        //Close();
                        comList.Add(comport);
                        comIndex.Add(index);
                        serialNums.Add(node.SerialNumber);
                    }
                    ++index;
                }
                devcount = nRelays;
                if (devcount == 0)
                {
                    MyMessageBox("Denkovi relay not found", __FILE__(), __LINE__());
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }
        }

        public void Open(bool close = false)
        {
            Open(comPort, close);
        }

        public void Open(string comPortNew, bool close = false)
        {
            errMsg = null;
            if (ftdi == null)
            {
                ftdi = new FTDI();
            }
            else
            {
                //DebugMsg.DebugAddMsg(DebugMsg.DebugEnum.LOG, "FTDI close#1\n");
                ftdi.Close();
            }
            try
            {
                int index = comList.IndexOf(comPortNew);
                if (index < 0)
                {
                    index = -1;
                    return;
                }
                ftdi.OpenByIndex(comIndex[index]);
                ftdi.SetBitMode(0xff, 0x01);
                comPort = comPortNew;
                relayNum = (int)index + 1; // index is 0-based, our relayNum is 1-based for the GUI
                serialNumber = serialNums[index];
                if (close)
                {
                    //DebugMsg.DebugAddMsg(DebugMsg.DebugEnum.LOG, "FTDI close#2\n");
                    ftdi.Close();
                    ftdi = null;
                }
                //AllOff();
            }
            catch (Exception ex)
            {
                errMsg = "Relay Open failed\n"+ex.Message + "\n";
                //DebugMsg.DebugAddMsg(DebugMsg.DebugEnum.ERR, errMsg);
                return;
            }
            // this msg doesn't display...why??
            //DebugMsg.DebugAddMsg(DebugMsg.DebugEnum.ERR, "Relay#"+relayNum+" opened\n");
        }


        public string SerialNumber()
        {
            return serialNumber;
        }

        public int RelayNumber()
        {
            return relayNum;
        }

        public List<string> ComList()
        {
            return comList;
        }

        public uint DevCount()
        {
            return devcount;
        }
        /*
        public Relay(string comPort, string baud)
        {
            FT_DEVICE_INFO_NODE[] nodes = new FT_DEVICE_INFO_NODE[devcount];
            FT_STATUS status = ftdi.GetDeviceList(nodes);
            ftdi.OpenByIndex(0);
            ftdi.SetBitMode(0xff, 0x01);
            ftdi.GetCOMPort(out string comport);
            if (comport.Length == 0) comport = "Not detected";
            //richTextBox1.AppendText("COM Port: " + comport + "\n" + nodes[0].Description + "\n");
            //Init();
        }
        */

        ~Relay()
        {
            Close();
        }

        public void Close()
        {
            // Put some delays in here as the 8-channel relay was turning on some relays
            // I had 4-channel as Relay1 and 8-channel as Relay2
            // The delays seem to have cured that problem
            if (ftdi != null && ftdi.IsOpen)
            {
                //AllOff();
                //DebugMsg.DebugAddMsg(DebugMsg.DebugEnum.LOG, "FTDI close#3\n");
                ftdi.Close();
                Thread.Sleep(200);
            }
            ftdi = null;
            Thread.Sleep(200);
        }
        public void AllOn()
        {
            //Open();
            errMsg = null;
            try
            {
                Set(1, 1); // Turn off all relays
                Set(2, 1); // Relay 1
                Set(3, 1);
                Set(4, 1); // Relay 2
                Set(5, 1);
                Set(6, 1); // Relay 3
                Set(7, 1);
                Set(8, 1); // Relay 4
            }
            catch (Exception ex)
            {
                errMsg = "Relay AllOff failed\n" + ex.Message;
            }
            //ftdi.Close();
        }

        public void AllOff()
        {
            //Open();
            errMsg = null;
            try
            {
                Set(1, 0); // Turn off all relays
                Set(2, 0);
                Set(3, 0);
                Set(4, 0);
                Set(5, 0);
                Set(6, 0);
                Set(7, 0);
                Set(8, 0);
            }
            catch (Exception ex)
            {
                errMsg = "Relay AllOff failed\n"+ex.Message;
            }
            //ftdi.Close();
        }

        public bool IsOpen()
        {
            if (ftdi == null) return false;
            return ftdi.IsOpen;
        }

        public bool Status(int nRelay)
        {
            if (ftdi == null)
            {
                //Debug
                return false;
            }
            Monitor.Enter(ftdi);
            //Open();
            errMsg = null;
            try
            {
                byte bitModes = 0;
                ftdi.GetPinStates(ref bitModes);
                if ((bitModes & (1 << (nRelay - 1))) != 0)
                {
                    //DebugMsg.DebugAddMsg(DebugMsg.DebugEnum.LOG, "FTDI close#4\n");
                    Close();
                    Monitor.Exit(ftdi);
                    return true;
                }
            }
            catch (Exception ex)
            {
                errMsg = "Relay Status failed\n"+ex.Message;
            }
            Monitor.Exit(ftdi);
            return false;
        }

        public byte Status()
        {
            if (ftdi == null)
            {
                MyMessageBox("ftdi == null??", __FUNC__(), __LINE__());
                return 0xff;
            }
            Monitor.Enter(ftdi);
            bool local = false;
            if (ftdi!=null && !ftdi.IsOpen) // then we'll open and close the device inside here
            {
                Open();
                local = true;
            }
            errMsg = null;
            byte bitModes = 0x00;
            try
            {
                if (ftdi == null)
                {
                    MyMessageBox("ftdi == null??", __FUNC__(), __LINE__());
                    return 0xff;
                }
                FTD2XX_NET.FTDI.FT_STATUS status = ftdi.GetPinStates(ref bitModes);
                if (status != FT_STATUS.FT_OK)
                {
                    MyMessageBox("status != FT_OK", __FUNC__(), __LINE__());
                    //DebugMsg.DebugAddMsg(DebugMsg.DebugEnum.LOG, "FTDI status != FT_STATUS_.FT_OK, " + status + " != " + FT_STATUS.FT_OK +"\n");
                    errMsg = "Oops!!";
                    ftdi.CyclePort();
                    Monitor.Exit(ftdi);
                    return 0xff;
                }
            }
            catch (Exception ex)
            {
                errMsg = "Relay Status failed\n"+ex.Message;
            }
            if (local)
            {
                Close();
            }
            if (ftdi == null)
            {
                MyMessageBox("ftdi == null??", __FUNC__(), __LINE__());
            }
            else
            {
                Monitor.Exit(ftdi);
            }
            return bitModes;
        }

        static void MyMessageBox(string msg, string func, int line)
        {
            MessageBox.Show(msg + "\n" + func + "(" + line + ")");
        }
        public void Init()
        {
            if (ftdi == null)
            {
                MyMessageBox("FTDI == null??", __FUNC__(), __LINE__());
                return;
            }
            Open();
            errMsg = null;
            try
            {
                byte bitModes = 0;
                ftdi.GetPinStates(ref bitModes);
            }
            catch (Exception ex)
            {
                errMsg = "Relay Init failed\n"+ex.Message;
            }
           Close();
        }

        public void Set(int nRelay, byte status)
        {
            if (ftdi == null)
            {
                Open(false);
                MyMessageBox("ftdi == null??", __FUNC__(), __LINE__());
                return;
            }
            Monitor.Enter(ftdi);
            errMsg = null;
            try
            {
                // Get status
                byte[] data = { 0x00, 0x00, 0x00 };
                uint nWritten = 0;
                byte flags;
                byte bitModes = 0x00;
                if (ftdi == null)
                {
                    MyMessageBox("ftdi == null??", __FUNC__(), __LINE__());
                    return;
                }

                ftdi.GetPinStates(ref bitModes);

                if (status != 0) // Then we OR the added relay in
                {
                    var relayBit = 1u << (nRelay - 1);
                    flags = (byte)(bitModes | relayBit);
                }
                else // Just set the requested relay and turn all others off
                {
                    var relayBit = 1u << (nRelay - 1);
                    //flags = (byte)(bitModes & (~(relayBit)));
                    flags = (byte)relayBit;
                }
                data[2] = flags;
                ftdi.Write(data, data.Length, ref nWritten);
                if (nWritten == 0)
                {
                    Close();
                    errMsg = "Unable to write to relay...disconnected?";
                    Monitor.Exit(ftdi);
                    return;
                    //throw new Exception("Unable to write to relay...disconnected?");
                }
                Thread.Sleep(100);
                byte bitModes2 = 0x00;
                ftdi.GetPinStates(ref bitModes2);
                if (status != 0) // check we set it
                {
                    if ((bitModes2 & (1u << (nRelay - 1))) == 0)
                    {
                        errMsg = "Relay did not get set!";
                        Close();
                        Monitor.Exit(ftdi);
                        return;
                    }
                }
                //Status();
            }
            catch (Exception ex)
            {
                errMsg = "Relay Set failed\n"+ex.Message;
            }
            //Monitor.Exit(ftdi);
        }
    }
}
