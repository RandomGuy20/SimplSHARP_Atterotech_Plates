using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace MediaStreamerDevices.AtteroTech.WallPlates
{
    public class unD6IO:WallPlateBase
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

        public unD6IO(string IpAddress):base(IpAddress)
        {
            this.supportsBluetooth = false;
            this.supportsGetDeviceName = false;
            this.supportsGetMACAddress = false;
            this.supportsOutputControls = true;
            this.maxAmountPreset = 9;
            this.plateType = eDeviceType.unD6IO;
        }

        #endregion
        
        #region Internal Methods

        #endregion

        #region Public Methods


        #endregion
    }
}