using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace MediaStreamerDevices.AtteroTech.WallPlates
{
    public class unD6IOBTSIMPL
    {
        #region Fields

        unD6IOBT plate;

        #endregion

        #region Properties

        #endregion

        #region Delegates


        public delegate void DeviceInformationEventHandler(SimplSharpString deviceMac, SimplSharpString btMac, SimplSharpString BTfriendlyName);

        public delegate void DeviceBluToothStatusEventHandler(ushort status);
        
        public delegate void BluToothAudioEventHandler(SimplSharpString song, SimplSharpString artist, SimplSharpString album);
        public delegate void DeviceFrontPanelStatusEventHandler(ushort status);


        public delegate void OutputMuteEventHandler(ushort output, ushort state);
        public delegate void OutputVolumeEventHandler(ushort output, ushort level);
        public delegate void DeviceBusyEventhandler(ushort state);
        public delegate void DeviceIsInIDModeEventhandler(ushort state);
        public delegate void LoadedPresetEventhandler(ushort preset);
        public delegate void SavedPresetEventHandler(ushort saved);
        public delegate void PhantomPowerEventHandler(ushort input, ushort state);

        #endregion

        #region Events

        public DeviceInformationEventHandler onBtInfo {get;set;}
        public BluToothAudioEventHandler onBtAudio {get;set;}
        public DeviceBluToothStatusEventHandler onBtStatus {get;set;}
        public DeviceFrontPanelStatusEventHandler onFrontpanelChange {get;set;}

        public OutputMuteEventHandler onMuteChange {get;set;}
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
            plate = new unD6IOBT(data);
            plate.onBluToothChange += new WallPlateBase.WallPlateBlueToothEventHandler(plate_onBluToothChange);
            plate.onDeviceChange += new WallPlateBase.WallPlateDeviceEventHandler(plate_onDeviceChange);
            plate.onOutputChange += new WallPlateBase.WallPlateOutputEventHandler(plate_onOutputChange);
            plate.onFrontPanelChange += new WallPlateBase.WallPlateFrontPanelEventhandler(plate_onFrontPanelChange);

        }

        #endregion
        
        #region Internal Methods

        void plate_onFrontPanelChange(WPFrontPanelEventArgs args)
        {
            onFrontpanelChange(Convert.ToUInt16(args.FrontPanelLock));
        }

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
                onPhantomPower(i++,Convert.ToUInt16(args.PhantomPower[i]));
        }

        void plate_onBluToothChange(WPBluToothEventArgs args)
        {

            onBtAudio(args.bluToothSong,args.bluToothArtist,args.bluToothAlbum);
            onBtInfo(plate.DeviceMac,args.bluToothMac,args.bluToothFriendlyName);
            onBtStatus((ushort)args.bluToothStatus);
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

        public void LoadPreset(ushort data)
        {
            plate.LoadPreset(data);
        }

        public void SavePreset(ushort data)
        {
            plate.SavePreset(data);
        }

        public void SetOutputVolume(ushort output, ushort level)
        {
            plate.SetOutputVolume((eOutput)output, level);
        }

        public void MuteOutput(ushort output, ushort state)
        {
            plate.SetOutputMute((eOutput)output,Convert.ToBoolean(state));
        }

        public void PhantomPower(ushort output, ushort state)
        {
            plate.SetPhantomPower((eOutput)output, Convert.ToBoolean(state));
        }
        
        public void SetBluetoothName(SimplSharpString data)
        {
            var info = data.ToString();
            plate.SetBluetoothName(info);
        }
        
        public void LockFrontPanel()
        {
            plate.LockFrontPanel();
        }

        public void ClearBlutoothConnections()
        {
            plate.ClearBlutoothConnections();
        }

        #endregion
    }
}