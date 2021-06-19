using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace MediaStreamerDevices.AtteroTech.WallPlates
{
    public class unDX4lSIMPL
    {
        #region Fields

        unDX4l plate;

        #endregion

        #region Properties

        #endregion

        #region Delegates

        public delegate void OutputMuteEventHandler(ushort output, ushort state);
        public delegate void OutputVolumeEventHandler(ushort output, ushort level);
        public delegate void DeviceBusyEventhandler(ushort state);
        public delegate void DeviceIsInIDModeEventhandler(ushort state);
        public delegate void LoadedPresetEventhandler(ushort preset);
        public delegate void SavedPresetEventHandler(ushort saved);
        public delegate void PhantomPowerEventHandler(ushort input, ushort state);

        #endregion

        #region Events

        public OutputMuteEventHandler onMuteChange { get; set; }
        public OutputVolumeEventHandler onVolumeChange { get; set; }
        public DeviceBusyEventhandler onDeviceBusy { get; set; }
        public DeviceIsInIDModeEventhandler onDeviceIDMode { get; set; }
        public LoadedPresetEventhandler onLoadedPreset { get; set; }
        public SavedPresetEventHandler onSavedPreset { get; set; }
        public PhantomPowerEventHandler onPhantomPower { get; set; }

        #endregion

        #region Constructors

        public void Initialize(SimplSharpString ipAddress)
        {
            string data = ipAddress.ToString();
            plate = new unDX4l(data);
            plate.onDeviceChange += new WallPlateBase.WallPlateDeviceEventHandler(plate_onDeviceChange);
            plate.onOutputChange += new WallPlateBase.WallPlateOutputEventHandler(plate_onOutputChange);
        }



        #endregion

        #region Internal Methods

        void plate_onOutputChange(WPOutputEventArgs args)
        {
            for (ushort i = 0; i < 2; i++)
            {
                onMuteChange(i++, Convert.ToUInt16(args.MuteStates[i]));
                onVolumeChange(i++, Convert.ToUInt16(args.Outputlevels[i]));
            }

        }

        void plate_onDeviceChange(WPDeviceEventArgs args)
        {
            onDeviceBusy(Convert.ToUInt16(args.DeviceIsBusy));
            onDeviceIDMode(Convert.ToUInt16(args.DeviceIsInIDMode));
            onLoadedPreset(Convert.ToUInt16(args.PresetLoaded));
            onSavedPreset(Convert.ToUInt16(args.PresetSaved));

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

        public void SavePreset(ushort value)
        {
            plate.SavePreset(value);
        }

        public void LoadPreset(ushort value)
        {
            plate.LoadPreset(value);
        }

        public void SetOutputVolume(ushort output, ushort level)
        {
            plate.SetOutputVolume((eOutput)output, level);
        }

        public void MuteOutput(ushort output, ushort state)
        {
            plate.SetOutputMute((eOutput)output, Convert.ToBoolean(state));
        }

        #endregion
    }
}