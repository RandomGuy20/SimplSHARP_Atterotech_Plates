using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using UDPLibrary;

namespace MediaStreamerDevices.AtteroTech.WallPlates
{
    public class WallPlateBase
    {
        #region Fields

        protected UDP udpClient;

        private CTimer initialPollTimer;
        private CTimer commandQTimer;
        private CTimer responseQTimer;
        private CTimer btClosedTimer;

        private CTimer ackTimer;

        private CrestronQueue<string> commandQ;
        private CrestronQueue<string> responseQ;

        private StringBuilder rxData = new StringBuilder();

        protected eDeviceType plateType { get; set; }


        private int loadedPreset;
        private int[] outputLevel;
        private byte carriageReturn = 0x0d;


        private bool[] outputMute;
        private bool[] phantomPower;
        private bool isIDMode;
        private bool initialPollDone;
        private bool isBusy;
        private bool queryWasLast;
        private bool presetWasSaved;
        private bool ackReceived;
        protected bool frontPanelLocked;
        protected bool LedState;


        private string ipaddress;
        protected string macAddress;
        protected string deviceName;
        private string lastSentCommand;


        protected string blutoothMAc;
        protected string blueToothFriendlyName;
        protected bool bluToothClosedConnections;
        protected eBlueToothStatus bluToothStatus;
        protected string bluToothSong;
        protected string bluToothArtist;
        protected string bluToothAlbum;





        protected string[] plateCommand = new string[21] {"QUERY","ID {0}","GETDEVICENAME","GETMAC","LOAD {0}","SAVE {0}","OV {0} {1}" , "OM {0} {1}","PP {0} {1}",
                                                             "REBOOT","BTMAC","BTN","BTN {1}","BTL {0}","BTL","BTS","BCC","BTSONG","BTARTIST","BTALBUM","LED {0}"};

        protected string[] plateReply = new string[21] {"ACK QUERY","ACK ID ","ACK GETDEVICENAME ","ACK GETMAC ","ACK LOAD ","ACK SAVE ","ACK OV ","ACK OM ","ACK PP ", 
                                                            "RebootFb","ACK BTMAC ACK ","ACK BTN ","BluetoothSet","FRONT PANEL SET FB","ACK BTL ","ACK BTS ","ACK CBC",
                                                            "ACK BTSONG \"","ACK BTARTIST \"","ACK BTALBUM \"","ACK LED " };

        private string[] queryReply = new string[22] { "", "ID=OFF", "GETDEVICENAME=", "GETMAC=", "LOAD=", "", "OV", "OM", "PP", "", "BTMAC=", "", "", "", "BTL=", "BTS=", "CBC=", "BTSONG=", "BTARTIST=", "BTALBUM=","","LED=" };



        protected bool supportsBluetooth { get; set; }
        protected bool supportsGetDeviceName { get; set; }
        protected bool supportsGetMACAddress { get; set; }
        protected bool supportsOutputControls { get; set; }
        protected bool supportsFrontPanelLock { get; set; }
        protected bool supportsLEDControl { get; set; }


        protected int maxAmountPreset { get; set; }
        protected int supportedInputs { get; set; }

        protected int[] logicInputVoltage;

       
        #endregion

        #region Properties

        public int       LoadedPreset { get { return loadedPreset; } }
        public int[]     OutputLevel { get { return outputLevel; } }
        public int          MaxAmountOfPresets { get { return maxAmountPreset; } }

        public bool[]    OutputMute { get { return outputMute; } }
        public bool[]    PhantomPower { get { return phantomPower; } }
        public bool      DeviceIsInIDMode { get { return isIDMode;   } }
        public bool      DeviceIsBusy { get { return isBusy; } }
        public bool      Debug { get; set; }

        public string IpAddress { get { return ipaddress; } }  

        #endregion

        #region Delegates
        
        public delegate void WallPlateOutputEventHandler(WPOutputEventArgs args);
        public delegate void WallPlateDeviceEventHandler(WPDeviceEventArgs args);
        public delegate void WallPlateFrontPanelEventhandler(WPFrontPanelEventArgs args);
        public delegate void WallPlateBlueToothEventHandler(WPBluToothEventArgs args);

        #endregion

        #region Events

        public event WallPlateOutputEventHandler onOutputChange;
        public event WallPlateDeviceEventHandler onDeviceChange;
        public event WallPlateFrontPanelEventhandler onFrontPanelChange;
        public event WallPlateBlueToothEventHandler onBluToothChange;


        #endregion

        #region Constructors

        internal WallPlateBase()
        {

        } 

        public WallPlateBase(string IpAddress)
        {

            try
            {
                loadedPreset = 0;
                this.outputLevel = new int[2] { 0, 0 };
                this.outputMute = new bool[2] { false, false };
                this.phantomPower = new bool[2] { false, false };
                isIDMode = false;
                ackReceived = false;


                ipaddress = IpAddress;

                commandQ = new CrestronQueue<string>();
                responseQ = new CrestronQueue<string>();

                commandQTimer = new CTimer(ProcessCommand, null, 0, 15);
                responseQTimer = new CTimer(ProcessResponse, null, 0, 15);

                udpClient = new UDP();
                udpClient.ipAddress = ipaddress;
                udpClient.port = 49494;
                udpClient.RXData += new UDP.ReceivedString(udpClient_RXData);
                udpClient.Initialize();

                initialPollTimer = new CTimer(InitialPoll, null, 5000, 0);
                initialPollDone = false;

            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine("\nError within WallPlate Base Constructor is: " + ex);
                ErrorLog.Error("\nError within WallPlate Base Constructor is: " + ex);
            }

        }
                
        #endregion

        #region Internal Methods

        protected void SendDebug(string dataToSend)
        {
            if (Debug)
            {
                CrestronConsole.PrintLine("\nPlate " + plateType.ToString() + " Debug message is: " + dataToSend);
                ErrorLog.Error("\nPlate " + plateType.ToString() + " Debug message is: " + dataToSend);
            }

        }

        private eDeviceReply GetMatchPosition(string data, string[] arr)
        {

            try
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    if (data == arr[i])
                        return (eDeviceReply)i;
                }
                return eDeviceReply.Null;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("\n Error in GetMatchPosition is: " + e);
                ErrorLog.Error("\n Error in GetMatchPosition is: " + e);
                return eDeviceReply.Null;
            }

        }

        private string GetDeviceString(int startPosition, int length, string data)
        {
            string temp;
            temp = data.Substring(startPosition, length);
            return temp;
        }

        private string ConcatArrayReply(string data, int index)
        {
            return data + index + " ";
        }

       /* internal void RouteToParser(eDeviceReply reply, string data)
        {
            switch (reply)
            {
                case eDeviceReply.ID:
                    break;
                case eDeviceReply.GetDeviceName:
                    break;
                case eDeviceReply.GetDeviceMac:
                    break;
                case eDeviceReply.LoadPreset:
                    break;
                case eDeviceReply.SavePreset:
                    break;
                case eDeviceReply.OutputVolume:
                    break;
                case eDeviceReply.OutputMute:
                    break;
                case eDeviceReply.PhantomPower:
                    break;
                case eDeviceReply.Reboot:
                    break;
                case eDeviceReply.GetBlutoothMac:
                    break;
                case eDeviceReply.GetBluToothFriendlyName:
                    break;
                case eDeviceReply.SetBluToothName:
                    break;
                case eDeviceReply.LockFrontPanel:
                    break;
                case eDeviceReply.GetFrontPanel:
                    break;
                case eDeviceReply.GetBluToothStatus:
                    break;
                case eDeviceReply.CloseBlutoothConnections:
                    break;
                case eDeviceReply.GetBTSong:
                    break;
                case eDeviceReply.GetBTArtist:
                    break;
                case eDeviceReply.GetBTAlbum:
                    break;
                case eDeviceReply.Null:
                    break;
                default:
                    break;
            }
        }*/

        private void ParseQueryArray(int[] array, string replyString, string dataString)
        {
            try
            {
                int index = Convert.ToInt32(GetDeviceString(replyString.Length, 1, dataString));
                string temp = replyString + index + "=";
                array[index] = Convert.ToInt32(GetDeviceString(temp.Length, dataString.Length - temp.Length, dataString));
            }
            catch (Exception ex)
            {
                SendDebug("Error when Parsing Query array for Int[] is: " + ex.Message);
            }
        }

        private void ParseQueryBool(bool[] array, string replyString, string dataString, bool state)
        {
            try
            {
                int index = Convert.ToInt32(GetDeviceString(replyString.Length, 1, dataString));
                string temp = replyString + index + "=";
                array[index] = state;
            }
            catch (Exception ex)
            {
                SendDebug("Error when Parsing Query array for Bool[] is: " + ex.Message);
            }
        }

        private void ParseQueryResponse(string data)
        {

            try
            {
                string queryData = data.Substring(plateReply[(int)eDeviceReply.Query].Length, data.Length - plateReply[(int)eDeviceReply.Query].Length);
                string trueState = "ON";
                eDeviceReply reply;


                string[] queryArray = queryData.Split(new char[0]);
                foreach (var item in queryArray)
                    SendDebug("The Query Return items: " + item);

                for (int i = 0; i < queryArray.Length; i++)
                {

                    for (int j = 1; j < plateReply.Length; j++)
                    {
                        if (queryArray[i].Contains(plateReply[j]))
                        {
                            int length = queryArray[i].Length - queryReply[j].Length;
                            reply = (eDeviceReply)i;
                            SendDebug("Atterotech Parsing Query, found Match in query array at pos: " + j + " Array String is: " + queryArray[i] + " And the Contain Match is: " + plateReply[j]);
                            switch (reply)
                            {
                                case eDeviceReply.ID:
                                    isIDMode = queryArray[i].Contains(trueState) ? true : false;
                                    break;
                                case eDeviceReply.GetDeviceName:
                                    deviceName = GetDeviceString(queryReply[j].Length, length, queryArray[i]);
                                    break;
                                case eDeviceReply.GetDeviceMac:
                                    macAddress = GetDeviceString(queryReply[j].Length, length, queryArray[i]);
                                    break;
                                case eDeviceReply.LoadPreset:
                                    loadedPreset = Convert.ToInt32(GetDeviceString(queryReply[j].Length, length, queryArray[i]));
                                    break;
                                case eDeviceReply.OutputVolume:
                                    ParseQueryArray(outputLevel, plateReply[j], queryArray[i]);
                                    loadedPreset = 0;
                                    break;
                                case eDeviceReply.OutputMute:
                                    ParseQueryBool(outputMute, plateReply[j], queryArray[i], queryArray[i].Contains(trueState) ? true : false);
                                    loadedPreset = 0;
                                    break;
                                case eDeviceReply.PhantomPower:
                                    ParseQueryBool(phantomPower, plateReply[j], queryArray[i], queryArray[i].Contains(trueState) ? true : false);
                                    loadedPreset = 0;
                                    break;
                                case eDeviceReply.GetBlutoothMac:
                                    blutoothMAc = GetDeviceString(queryReply[j].Length, length, queryArray[i]);
                                    break;
                                case eDeviceReply.GetBluToothFriendlyName:
                                    blueToothFriendlyName = GetDeviceString(queryReply[j].Length, length, queryArray[i]);
                                    break;
                                case eDeviceReply.LockFrontPanel:
                                    break;
                                case eDeviceReply.GetFrontPanel:
                                    frontPanelLocked = Convert.ToBoolean(Convert.ToInt32(GetDeviceString(queryReply[j].Length, length, queryArray[i])));
                                    break;
                                case eDeviceReply.GetBluToothStatus:
                                    bluToothStatus = (eBlueToothStatus)Convert.ToInt32(GetDeviceString(queryReply[j].Length, length, queryArray[i]));
                                    break;
                                case eDeviceReply.CloseBlutoothConnections:
                                    break;
                                case eDeviceReply.GetBTSong:
                                    bluToothSong = GetDeviceString(queryReply[j].Length, length, queryArray[i]);
                                    break;
                                case eDeviceReply.GetBTArtist:
                                    bluToothArtist = GetDeviceString(queryReply[j].Length, length, queryArray[i]);
                                    break;
                                case eDeviceReply.GetBTAlbum:
                                    bluToothAlbum = GetDeviceString(queryReply[j].Length, length, queryArray[i]);
                                    break;
                                case eDeviceReply.Null:
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

                onOutputChange(new WPOutputEventArgs(outputLevel,outputMute));
                onFrontPanelChange(new WPFrontPanelEventArgs(frontPanelLocked));
                onDeviceChange(new WPDeviceEventArgs(isIDMode,loadedPreset,presetWasSaved,phantomPower,ackReceived));
                onBluToothChange(new WPBluToothEventArgs(blutoothMAc, blueToothFriendlyName,bluToothClosedConnections, bluToothStatus, bluToothSong, bluToothArtist, bluToothAlbum));
            }
            catch (Exception ex)
            {
                SendDebug("Atterotech Error parsing Query response is: " + ex.Message);
            }

        }

        private void ParseDeviceResponse(eDeviceReply reply, string data)
        {
            try
            {
                int replyInt = (int)reply;
                int startPos = plateReply[replyInt].Length;
                int length = data.Length - startPos;
                string plateFB = plateReply[replyInt];
                presetWasSaved = false;

                switch (reply)
                {
                    case eDeviceReply.ID:
                        isIDMode = Convert.ToBoolean(GetDeviceString(startPos, length, data));
                        break;
                    case eDeviceReply.GetDeviceName:
                        deviceName = GetDeviceString(startPos, length, data);
                        break;
                    case eDeviceReply.GetDeviceMac:
                        macAddress = GetDeviceString(startPos, length, data);
                        break;
                    case eDeviceReply.LoadPreset:
                        loadedPreset = Convert.ToInt32(GetDeviceString(startPos, length, data));
                        break;
                    case eDeviceReply.SavePreset:
                        presetWasSaved = true;
                        break;
                    case eDeviceReply.PhantomPower:
                        var index = Convert.ToInt32(GetDeviceString(startPos, 1, data));
                        phantomPower[index] = Convert.ToBoolean(GetDeviceString(ConcatArrayReply(plateFB, index).Length, 1, data));
                        break;
                    case eDeviceReply.Reboot:
                        break;
                    default:
                        break;
                }
                onDeviceChange(new WPDeviceEventArgs(isIDMode, loadedPreset, presetWasSaved, phantomPower,ackReceived));

            }
            catch (Exception ex)
            {
                SendDebug("Error when parsing Device related Feedback is: " + ex.Message);
            }

        }

        private void ParseOutputResponse(eDeviceReply reply, string data)
        {
            try
            {
                int replyInt = (int)reply;
                int startPos = plateReply[replyInt].Length;
                int length = data.Length - startPos;
                int index = Convert.ToInt32(GetDeviceString(startPos, 1, data));
                string plateFB = plateReply[replyInt];
                presetWasSaved = false;

                switch (reply)
                {
                    case eDeviceReply.OutputVolume:
                        outputLevel[index] = ScaleFromDevice(Convert.ToInt32(GetDeviceString(ConcatArrayReply(plateFB, index).Length, 1, data)));
                        break;
                    case eDeviceReply.OutputMute:
                        outputMute[index] = Convert.ToBoolean(GetDeviceString(ConcatArrayReply(plateFB, index).Length, 1, data));
                        break;
                    default:
                        break;
                }

                onOutputChange(new WPOutputEventArgs(outputLevel, outputMute));
            }
            catch (Exception ex)
            {
                SendDebug("Error parsing Output response is: " + ex.Message);
            }


        }

        private void ParseLEDResponse(eDeviceReply reply, string data)
        {
            int replyInt = (int)reply;
            int startPos = plateReply[replyInt].Length;
            int length = data.Length - startPos;
            string plateFB = plateReply[replyInt];

            LedState = Convert.ToBoolean(Convert.ToInt32(GetDeviceString(startPos, length, data)));
        }

        private void ParseFrontPanelResponse(eDeviceReply reply, string data)
        {

            try
            {
                int replyInt = (int)reply;
                int startPos = plateReply[replyInt].Length;
                int length = data.Length - startPos;
                string plateFB = plateReply[replyInt];

                switch (reply)
                {

                    case eDeviceReply.LockFrontPanel:
                    case eDeviceReply.GetFrontPanel:
                        frontPanelLocked = Convert.ToBoolean(GetDeviceString(startPos, length, data));
                        break;
                    default:
                        break;
                }

                onFrontPanelChange(new WPFrontPanelEventArgs(frontPanelLocked));
            }
            catch (Exception ex)
            {
                SendDebug("Error Parsing Front Panel Response is: " + ex.Message);
            }

        }

        private void ParseBlutoothResponse(eDeviceReply reply, string data)
        {
            try
            {
                int replyInt = (int)reply;
                int startPos = plateReply[replyInt].Length;
                int length = data.Length - startPos;
                string plateFB = plateReply[replyInt];
                switch (reply)
                {

                    case eDeviceReply.GetBlutoothMac:
                        blutoothMAc = GetDeviceString(startPos, length, data);
                        break;
                    case eDeviceReply.GetBluToothFriendlyName:
                    case eDeviceReply.SetBluToothName:
                        blueToothFriendlyName = GetDeviceString(startPos, length, data);
                        break;
                    case eDeviceReply.GetBluToothStatus:
                        bluToothStatus = (eBlueToothStatus)Convert.ToInt32(GetDeviceString(startPos, length, data));
                        break;
                    case eDeviceReply.CloseBlutoothConnections:
                        bluToothClosedConnections = true;

                        if (!btClosedTimer.Disposed)
                        {
                            btClosedTimer.Stop();
                            btClosedTimer.Dispose();
                        }

                        btClosedTimer = new CTimer(BTClosedConnCallback, null, 1500, 0);
                        break;
                    case eDeviceReply.GetBTSong:
                        bluToothSong = GetDeviceString(startPos, length, data);
                        break;
                    case eDeviceReply.GetBTArtist:
                        bluToothArtist = GetDeviceString(startPos, length, data); ;
                        break;
                    case eDeviceReply.GetBTAlbum:
                        bluToothAlbum = GetDeviceString(startPos, length, data); ;
                        break;
                    default:
                        break;
                }

                onBluToothChange(new WPBluToothEventArgs(blutoothMAc, blueToothFriendlyName, bluToothClosedConnections, bluToothStatus, bluToothSong, bluToothArtist, bluToothAlbum));
            }
            catch (Exception ex)
            {
                SendDebug("Error Parsing Bluetooth Response is: " + ex.Message);
            }
        }

        private void SendPostPresetQuery(string data)
        {

        }
        /// <summary>
        /// This method will Q a command to be sent to the wall plate
        /// </summary>
        /// <param name="data"></param>
        protected void QCommand(string data)
        {
            try
            {
                if (data.Length > 0 && data != string.Empty)
                    commandQ.Enqueue(data);
                SendDebug("Adding this string:-- " + data + "-- To Command Q");
            }
            catch (Exception err)
            {
                CrestronConsole.PrintLine("\nWallPlateBAseError QCommand is: " + err);
                ErrorLog.Error("\nWallPlateBAseError QCommand is: " + err);
            }
        }

        private void ProcessResponse(object obj)
        {
            try
            {
                var data = responseQ.TryToDequeue();
                rxData.Append(data);
                SendDebug("Atterotech Response is: " + data);
                if (!isBusy && rxData.Length > 0 && rxData.ToString().Contains((char)13))
                {
                    isBusy = true;
                    while (rxData.ToString().Contains((char)13))
                    {
                        var endOfLine = rxData.ToString().IndexOf("\n");
                        var collector = rxData.ToString().Substring(0, endOfLine);
                        SendDebug("\n Process Response WallPlate Reply is: " + collector);
                        ParseResponse(collector);
                    }
                    isBusy = false;
                }
            }
            catch (Exception err)
            {
                CrestronConsole.PrintLine("\nWallPlateBAseError Process Response is: " + err);
                ErrorLog.Error("\nWallPlateBAseError Process Response is: " + err);
                isBusy = false;
            }
        }

        private void ParseResponse(string data)
        {
            try
            {
                var pos = GetMatchPosition(data, plateReply);
                switch (pos)
                {
                    case eDeviceReply.Query:
                        ParseQueryResponse(data);
                        break;
                    case eDeviceReply.ID:
                    case eDeviceReply.GetDeviceName:
                    case eDeviceReply.GetDeviceMac:
                    case eDeviceReply.LoadPreset:
                    case eDeviceReply.SavePreset:
                        ParseDeviceResponse(pos, data);
                        break;
                    case eDeviceReply.OutputVolume:
                    case eDeviceReply.OutputMute:
                        ParseOutputResponse(pos, data);
                        break;
                    case eDeviceReply.PhantomPower:
                    case eDeviceReply.Reboot:
                        ParseDeviceResponse(pos, data);
                        break;
                    case eDeviceReply.GetBlutoothMac:
                    case eDeviceReply.GetBluToothFriendlyName:
                    case eDeviceReply.SetBluToothName:
                        ParseBlutoothResponse(pos, data);
                        break;
                    case eDeviceReply.LockFrontPanel:
                     case eDeviceReply.GetFrontPanel:
                        ParseFrontPanelResponse(pos, data);
                        break;
                    case eDeviceReply.GetBluToothStatus:
                    case eDeviceReply.CloseBlutoothConnections:
                    case eDeviceReply.GetBTSong:
                    case eDeviceReply.GetBTArtist:
                    case eDeviceReply.GetBTAlbum:
                        ParseBlutoothResponse(pos, data);
                        break;
                    case eDeviceReply.Null:
                        break;
                    case eDeviceReply.LEDMode:
                        ParseLEDResponse(pos, data);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("\n Error in Parse Response is: " + e);
                ErrorLog.Error("\n Error in Parse Response is: " + e);
            }

        }

        private void ProcessCommand(object obj)
        {
            try
            {
                    if (!commandQ.IsEmpty && !isBusy && ackReceived)
                    {
                        isBusy = true;
                        onDeviceChange(new WPDeviceEventArgs(isIDMode, loadedPreset, presetWasSaved, phantomPower, ackReceived));
                        var data = commandQ.TryToDequeue();
                        SendDebug("Command Being sent to the plate is: " + data);

                        queryWasLast = data.Contains("QUERY") ? true : false;
                        lastSentCommand = data;
                        if (data.Length > 0 && data != null)
                            udpClient.SendData(data);
                        ackReceived = false;
                    }
                    else
                        commandQ.Clear();
                    isBusy = false;

            }
            catch (Exception err)
            {
                CrestronConsole.PrintLine("\n WallPlateBAse Process Command Error is: " + err);
                ErrorLog.Error("\n WallPlateBAse Process Command Error is: " + err);
            }
        }

        /// <summary>
        /// If the Command requires an Output and a value
        /// </summary>
        /// <param name="command"></param>
        /// <param name="output"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected string CommandBuilder(eDeviceCommand command, eOutput output, object value)
        {
            try
            {
                var dataToSend = "";
                dataToSend = string.Format(plateCommand[(int)command], output.ToString("D"), value.ToString()) + carriageReturn;
                return dataToSend;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("\nError in Command Builder is: " + e);
                ErrorLog.Error("\nError in Command Builder is: " + e);
                return "";
            }
        }

        /// <summary>
        /// If the command only requires a value
        /// </summary>
        /// <param name="command"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected string CommandBuilder(eDeviceCommand command, object value)
        {
            try
            {
                var dataToSend = "";
                dataToSend = string.Format(plateCommand[(int)command], value.ToString()) + carriageReturn;
                return dataToSend;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("\nError in Command Builder is: " + e);
                ErrorLog.Error("\nError in Command Builder is: " + e);
                return "";
            }
        }

        /// <summary>
        /// If the command requires no further parameters
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected string CommandBuilder(eDeviceCommand command)
        {
            try
            {
                var dataToSend = "";
                dataToSend = string.Format(plateCommand[(int)command]) + carriageReturn;
                return dataToSend;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("\nError in Command Builder is: " + e);
                ErrorLog.Error("\nError in Command Builder is: " + e);
                return "";
            }
        }

        private int ScaleToDevice(int value)
        {
            try
            {
                int oldRange = 100;
                int newRange = 60;
                int oldMin = 0;
                int newMin = -60;
                double math = (value * newRange / oldRange) + (newMin - oldMin);
                return (int)math;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("\n Error in ScaleToDevice is: " + e);
                ErrorLog.Error("\n Error in ScaleToDevice is: " + e);
                return 0;
            }

        }

        private int ScaleFromDevice(int level)
        {
            try
            {
                int newMax = 100;
                int newMin = 0;
                int oldMax = 0;
                int oldMin = -60;
                double newvalue = (level - oldMin) * (newMax - newMin) / (oldMax - oldMin);
                return (int)newvalue;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("\n Error in ScaleToDevice is: " + e);
                ErrorLog.Error("\n Error in ScaleToDevice is: " + e);
                return 0;
            }

        }

        private void InitialPoll(object obj)
        {
            try
            {
                // Send query all command, and have the RX loop through to very a general query command was sent out. 
                commandQ.Enqueue(plateCommand[(int)eDeviceCommand.Query] + carriageReturn);
                SendDebug("Atterotech Initial Poll has been Sent");

                if (!ackTimer.Disposed)
                {
                    ackTimer.Stop();
                    ackTimer.Dispose();
                }
                ackTimer = new CTimer(VerifyAckTimer, null, 100, 00);
                ackReceived = false;
            }
            catch (Exception ex)
            {
                SendDebug("Atterotech Error when Sending initial Poll is: " + ex.Message);
            }

        }

        private void VerifyAckTimer(object obj)
        {
            // Method to send same command until and ack has been received
            if (!ackReceived)
            {
                udpClient.SendData(lastSentCommand);
                onDeviceChange(new WPDeviceEventArgs(isIDMode, loadedPreset, presetWasSaved, phantomPower, ackReceived));
            }
            else
            {
                ackTimer.Stop();
                ackTimer.Dispose();
                onDeviceChange(new WPDeviceEventArgs(isIDMode, loadedPreset, presetWasSaved, phantomPower, ackReceived));
            }
        }

        private void udpClient_RXData(string data)
        {
            responseQ.Enqueue(data);
            SendDebug("Atterotech RX Data is: " + data);
        }

        void BTClosedConnCallback(object obj)
        {
            bluToothClosedConnections = false;
            onBluToothChange(new WPBluToothEventArgs(blutoothMAc, blueToothFriendlyName, bluToothClosedConnections, bluToothStatus, bluToothSong, bluToothArtist, bluToothAlbum));
            btClosedTimer.Stop();
            btClosedTimer.Dispose();
        }
        
        #endregion

        #region Public Methods
        /// <summary>
        /// Incoming Volume should be from 0-100, 
        /// will return volume range back in 0 - 100 range
        /// The Axon D2i Does not support this function
        /// </summary>
        public void SetOutputVolume(eOutput output, int level)
        {
            try
            {
                if (supportsOutputControls && initialPollDone)
                    QCommand(CommandBuilder(eDeviceCommand.OutputVolume, output,ScaleToDevice(level) ));
            }
            catch (Exception e)
            {
                SendDebug("Error setting output volume is: " + e);
            }

        }

        public void SetOutputMute(eOutput output, bool mute)
        {
            try
            {
                if (supportsOutputControls && initialPollDone)
                    QCommand(CommandBuilder(eDeviceCommand.OutputMute, output, Convert.ToInt32(mute)));
            }
            catch (Exception e)
            {
                SendDebug("Error setting output mute is: " + e);
            }

        }

        public void IdDevice()
        {
            try
            {
                if (initialPollDone)
                {
                    if (isIDMode)
                        QCommand(CommandBuilder(eDeviceCommand.ID,0));
                    else
                        QCommand(CommandBuilder(eDeviceCommand.ID, 1));
                }
            }
            catch (Exception e)
            {
                SendDebug("Error setting ID Mode is: " + e);
            }

        }

        public void SetPhantomPower(eOutput input, bool state)
        {
            try 
	        {	        
		         if (initialPollDone)
                    QCommand(CommandBuilder(eDeviceCommand.PhantomPower,input,Convert.ToInt32(state)));
	        }
	        catch (Exception e)
	        {
                SendDebug("Error Loading preset is: " + e.Message);
	        }

        }

        /// <summary>
        /// Not Supported By Axon D2i
        /// </summary>
        /// <param name="preset"></param>
        public void LoadPreset(int preset)
        {

            try 
	        {	        
		        if (preset <= maxAmountPreset && initialPollDone)
                    QCommand(CommandBuilder(eDeviceCommand.LoadPreset,preset));

                SendDebug("Loading Preset #: " + preset);
	        }
	        catch (Exception e)
	        {
                SendDebug("Error Loading preset is: " + e.Message);
	        }

        }
        /// <summary>
        /// Not Supported By Axon D2i
        /// </summary>
        /// <param name="preset"></param>
        public void SavePreset(int preset)
        {
            try
            {
                if (preset <= maxAmountPreset && initialPollDone)
                    QCommand(CommandBuilder(eDeviceCommand.SavePreset, preset));
            }
            catch (Exception e)
            {
                SendDebug("Error Saving Preset is: " + e.Message);
            }

        }

        public void ResetToFactoryDefault()
        {
            if (initialPollDone)
            {

            }
        }

        protected string GetMacAddress()
        {
            if (supportsGetMACAddress && initialPollDone)
                return macAddress;
            else
                return "This device does not support this function";
        }

        protected string GetDeviceName()
        {
            if (supportsGetDeviceName && initialPollDone)
                return deviceName;
            else
                return "This device does not support this function";
        }

        protected bool GetLEDState()
        {
            if (supportsLEDControl && initialPollDone)
                return LedState;
            else
                return false;
        }
      
        #endregion
    }
}