using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace MediaStreamerDevices.AtteroTech.WallPlates
{
    public class AxonD2iSIMPL
    {
        #region Fields

        AxonD2i plate;

        #endregion

        #region Properties

        #endregion

        #region Delegates

        public delegate void LEDStateEventHandler(ushort state);
        public delegate void OutputMuteEventHandler(ushort output, ushort state);
        public delegate void DeviceBusyEventhandler(ushort state);
        public delegate void DeviceIsInIDModeEventhandler(ushort state);
        public delegate void PhantomPowerEventHandler(ushort input, ushort state);

        #endregion

        #region Events

        public LEDStateEventHandler onLEDChange { get; set; }
        public OutputMuteEventHandler onMuteChange { get; set; }
        public DeviceBusyEventhandler onDeviceBusy { get; set; }
        public DeviceIsInIDModeEventhandler onDeviceIDMode { get; set; }
        public PhantomPowerEventHandler onPhantomPower { get; set; }

        #endregion

        #region Constructors

        public void Initialize(SimplSharpString ipAddress)
        {
            string data = ipAddress.ToString();
            plate = new AxonD2i(data);
            plate.onDeviceChange += new WallPlateBase.WallPlateDeviceEventHandler(plate_onDeviceChange);
            plate.onOutputChange += new WallPlateBase.WallPlateOutputEventHandler(plate_onOutputChange);
        }
        
        #endregion

        #region Internal Methods

        void plate_onOutputChange(WPOutputEventArgs args)
        {
            for (ushort i = 0; i < 2; i++)
                onMuteChange(i++, Convert.ToUInt16(args.MuteStates[i]));
        }

        void plate_onDeviceChange(WPDeviceEventArgs args)
        {
            onDeviceBusy(Convert.ToUInt16(args.DeviceIsBusy));
            onDeviceIDMode(Convert.ToUInt16(args.DeviceIsInIDMode));
            onLEDChange(Convert.ToUInt16(plate.LEDState));

            for (ushort i = 0; i <= args.PhantomPower.Length; i++)
                onPhantomPower(i++, Convert.ToUInt16(args.PhantomPower[i]));

        }

        #endregion

        #region Public Methods

        public void Debug(ushort value)
        {
            plate.Debug = Convert.ToBoolean(value);
        }

        public void IDDevice()
        {
            plate.IdDevice();
        }

        public void PhantomPower(ushort output, ushort state)
        {
            plate.SetPhantomPower((eOutput)output, Convert.ToBoolean(state));
        }


        public void SetLEDState(ushort state)
        {
            plate.SetLEDMode(Convert.ToBoolean(state));
        }

        public void MuteOutput(ushort output, ushort state)
        {
            plate.SetOutputMute((eOutput)output, Convert.ToBoolean(state));
        }

        #endregion
    }
}