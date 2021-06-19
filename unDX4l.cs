using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace MediaStreamerDevices.AtteroTech.WallPlates
{
    public class unDX4l:WallPlateBase
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        #region Delegates

        #endregion

        #region Events

        #endregion

        #region Constructors

        public unDX4l(string IpAddress)
            : base(IpAddress)
        {
            try
            {
                this.supportsBluetooth = false;
                this.supportsGetDeviceName = false;
                this.supportsGetMACAddress = false;
                this.supportsOutputControls = true;
                this.maxAmountPreset = 9;
                this.plateType = eDeviceType.unDX4l;
           }
            catch (Exception ex)
            {
                SendDebug("Error in unDX4l Constructor is: " + ex);
            }

        }

        #endregion

        #region Internal Methods


        #endregion

        #region Public Methods

        #endregion
    }
}