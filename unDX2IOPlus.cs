using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace MediaStreamerDevices.AtteroTech.WallPlates
{
    public class unDX2IOPlus : WallPlateBase
    {
        
        #region Fields

        #endregion

        #region Properties

        public string DeviceName { get { return GetDeviceName(); } }

        public string DeviceMac { get { return GetMacAddress(); } }

        #endregion

        #region Delegates

        #endregion

        #region Events

        #endregion

        #region Constructors

        public unDX2IOPlus(string IpAddress)
            : base(IpAddress)
        {
            try
            {
                this.supportsBluetooth = false;
                this.supportsGetDeviceName = true;
                this.supportsGetMACAddress = true;
                this.supportsOutputControls = true;
                this.maxAmountPreset = 9;
                this.plateType = eDeviceType.unDX2IOPlus;
           }
            catch (Exception ex)
            {
                SendDebug("Error in unD6Io Constructor is: " + ex);
            }

        }

        #endregion

        #region Internal Methods


        #endregion

        #region Public Methods

        #endregion
    }

}