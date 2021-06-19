using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace MediaStreamerDevices.AtteroTech.WallPlates
{
    public class unD6IOBT:WallPlateBase
    {
        #region Fields

        CTimer btAudioQueryTimer;

        private int count;

        #endregion

        #region Properties

        public string BluToothMacAddress { get { return this.blutoothMAc; } }
        public string BluToothSong { get { return this.bluToothSong; } }
        public string BluToothAlbum { get { return this.bluToothAlbum; } }
        public string BluToothArtist { get { return this.bluToothArtist; } }
        public string BluToothName { get { return this.blueToothFriendlyName; } }
        public string DeviceMac { get { return GetMacAddress(); } }
        public string DeviceName { get { return GetDeviceName(); } }

        public bool FrontPanelLockState { get { return this.frontPanelLocked; } }

        public eBlueToothStatus BluToothStatus { get { return this.bluToothStatus; } }

        #endregion

        #region Delegates

        #endregion

        #region Events

        #endregion

        #region Constructors

        public unD6IOBT(string IpAddress):base(IpAddress)
        {
            try
            {
                this.supportsBluetooth = true;
                this.supportsGetDeviceName = true;
                this.supportsGetMACAddress = true;
                this.supportsOutputControls = true;
                this.maxAmountPreset = 9;
                this.blueToothFriendlyName = "";
                this.bluToothAlbum = "";
                this.bluToothArtist = "";
                this.blutoothMAc = "";
                this.bluToothSong = "";
                this.bluToothStatus = eBlueToothStatus.Idle;
                this.plateType = eDeviceType.unD6IOBT;

                btAudioQueryTimer = new CTimer(BTAudioCallback, null, 0, 50);
                count = 0;
            }
            catch (Exception ex)
            {
                SendDebug("Error in unD6Io Constructor is: " + ex);
            }

        }

        #endregion

        #region Internal Methods

        void BTAudioCallback(object obj)
        {
            count++;
            if (count == 1)
	        {
                QCommand(CommandBuilder(eDeviceCommand.GetBTSong));
	        }
            else if (count == 2)
            {
                QCommand(CommandBuilder(eDeviceCommand.GetBTArtist));
            }
            else if (count == 3)
            {
                QCommand(CommandBuilder(eDeviceCommand.GetBTAlbum));
            }
            else if (count == 4)
            {
                QCommand(CommandBuilder(eDeviceCommand.GetFrontPanel));
                count = 0;
            }
        }

        #endregion

        #region Public Methods


        public void GetBlueToothMac()
        {
            try
            {
                QCommand(CommandBuilder(eDeviceCommand.GetBlutoothMac));
            }
            catch (Exception ex)
            {
                SendDebug("Error in unD6IO GetBluToothMAc is: " + ex);
            }

        }

        public void SetBluetoothName(string data)
        {
            try
            {
                if (data.Length < 33)
                {
                    QCommand(CommandBuilder(eDeviceCommand.SetBluToothName, data));
                    SendDebug("Attempting to set unD6IO BT name to: " + data);
                }
                else
                {
                    SendDebug("unD6IO-BT Set Blutooth Name error: Name is too Long cannot be more than 32 characters");
                }
            }
            catch (Exception ex)
            {
                SendDebug("Error in unD6IO SetBluToothName is: " + ex);
            }

        }

        /// <summary>
        /// Toggle the Lock State
        /// </summary>
        public void LockFrontPanel()
        {
            try
            {
                var data = this.frontPanelLocked ? CommandBuilder(eDeviceCommand.LockFrontPanel, 1) : CommandBuilder(eDeviceCommand.LockFrontPanel, 0);
                QCommand(data);
            }
            catch (Exception ex)
            {
                SendDebug("Error in unD6IO Lock Front Panel is is: " + ex);
            }
        }

        /// <summary>
        /// Lock or Unlock Discretely
        /// </summary>
        public void LockFrontPanel(eDeviceLockState state)
        {
            try
            {
                string data = "";
                switch (state)
                {
                    case eDeviceLockState.Lock:
                        data =  CommandBuilder(eDeviceCommand.LockFrontPanel, 1);
                        SendDebug("Setting unD6IO panel to LOCK");
                        break;
                    case eDeviceLockState.UnLock:
                        data = CommandBuilder(eDeviceCommand.LockFrontPanel, 0);
                        SendDebug("Setting unD6IO panel to UNLOCK");
                        break;
                    default:
                        break;
                }
                QCommand(data);
            }
            catch (Exception ex)
            {
                SendDebug("Error in unD6IO Lock Front Panel is is: " + ex);
            }
        }

        public void GetBlutoothStatus()
        {
            try
            {
                QCommand(CommandBuilder(eDeviceCommand.GetBluToothStatus));
            }
            catch (Exception ex)
            {
                SendDebug("Error in unD6IO GetBluToothStatus is: " + ex);
            }

        }

        public void ClearBlutoothConnections()
        {
            try
            {
                QCommand(CommandBuilder(eDeviceCommand.CloseBlutoothConnections));
            }
            catch (Exception ex)
            {
                SendDebug("Error in unD6IO Clear Blu Tooth Connections is: " + ex);
            }

        }

        #endregion
    }
}