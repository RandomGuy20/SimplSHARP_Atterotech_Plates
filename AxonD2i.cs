using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace MediaStreamerDevices.AtteroTech.WallPlates
{
    public class AxonD2i:WallPlateBase
    {
        #region Fields


        #endregion

        #region Properties

        public string DeviceMac { get { return GetMacAddress(); } }
        public string DeviceName { get { return GetDeviceName(); } }
        
        public bool LEDState { get{return GetLEDState();}}

        #endregion

        #region Delegates

        #endregion

        #region Events

        #endregion

        #region Constructors

        public AxonD2i(string IpAddress)
            : base(IpAddress)
        {
            try
            {
                this.supportsBluetooth = false;
                this.supportsGetDeviceName = true;
                this.supportsGetMACAddress = true;
                this.supportsOutputControls = false;
                this.supportsLEDControl = true;
                this.maxAmountPreset = 9;
                this.plateType = eDeviceType.AxonD2i;
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
        
        /// <summary>
        /// Toggle
        /// </summary>
        public void SetLEDMode()
        {
            try
            {
                var data = LEDState ? CommandBuilder(eDeviceCommand.SetLEDMode,1) : CommandBuilder(eDeviceCommand.SetLEDMode,0);
                
                QCommand(data);
            }
            catch (Exception ex)
            {
                SendDebug("Error in unD6IO GetBluToothMAc is: " + ex);
            }

        }

        /// <summary>
        /// Discrete Controls
        /// </summary>
        public void SetLEDMode(bool state)
        {
            try
            {
                var data = state ? CommandBuilder(eDeviceCommand.SetLEDMode, 1) : CommandBuilder(eDeviceCommand.SetLEDMode, 0);

                QCommand(data);
            }
            catch (Exception ex)
            {
                SendDebug("Error in unD6IO GetBluToothMAc is: " + ex);
            }

        }
        
        #endregion
    }
}