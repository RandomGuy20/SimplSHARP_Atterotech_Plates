using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace MediaStreamerDevices.AtteroTech.WallPlates
{

    #region Enums
    public enum eDeviceCommand
    {
        Query = 0,
        ID = 1,
        GetDeviceName = 2,
        GetDeviceMac = 3,
        LoadPreset = 4,
        SavePreset = 5,
        OutputVolume = 6,
        OutputMute = 7,
        PhantomPower = 8,
        Reboot = 9,
        GetBlutoothMac = 10,
        GetBluToothFriendlyName = 11,
        SetBluToothName = 12,
        LockFrontPanel = 13,
        GetFrontPanel = 14,
        GetBluToothStatus = 15,
        CloseBlutoothConnections = 16,
        GetBTSong = 17,
        GetBTArtist = 18,
        GetBTAlbum = 19,
        SetLEDMode = 20
    }

    public enum eDeviceReply
    {
        Query = 0,
        ID = 1,
        GetDeviceName = 2,
        GetDeviceMac = 3,
        LoadPreset = 4,
        SavePreset = 5,
        OutputVolume = 6,
        OutputMute = 7,
        PhantomPower = 8,
        Reboot = 9,
        GetBlutoothMac = 10,
        GetBluToothFriendlyName = 11,
        SetBluToothName = 12,
        LockFrontPanel = 13,
        GetFrontPanel = 14,
        GetBluToothStatus = 15,
        CloseBlutoothConnections = 16,
        GetBTSong = 17,
        GetBTArtist = 18,
        GetBTAlbum = 19,
        Null = 20,
        LEDMode = 21
    }

    public enum eBlueToothStatus
    {
        Idle = 0,
        Discoverable = 1,
        ConnectedUnknownAVCRP = 2,
        ConnectedAVRCP_NotSupported = 3,
        ConnectedAVRCP_Supported = 4,
        ConnectedAVCRPandPDU_Supported = 5
    }
    public enum eOutput
    {
        All = 0,
        Input1 = 1,
        Input2 = 2
    }
    public enum eDeviceType
    {
        AxonD2i = 0,
        unD6IOBT = 1,
        unD6IO = 2,
        unDX2IOPlus = 3,
        unDX4l = 4

    }
    public enum eDeviceLockState
    {
        Lock,
        UnLock
    }

    #endregion

    #region Event Classes

    public class WPOutputEventArgs : EventArgs
    {
        public int[] Outputlevels;
        public bool[] MuteStates;

        public WPOutputEventArgs(int[] outputlevels, bool[] muteStates)
        {
            this.Outputlevels = outputlevels;
            this.MuteStates = muteStates;
        }
    }

    public class WPDeviceEventArgs : EventArgs
    {
        public bool DeviceIsInIDMode;
        public bool DeviceIsBusy;
        public int PresetLoaded;
        public bool PresetSaved;
        public bool[] PhantomPower;

        public WPDeviceEventArgs(bool isIDMode, int presetLoaded, bool presetSaved, bool[] phantomPowerState,bool deviceisbusy)
        {
            DeviceIsInIDMode = isIDMode;
            PresetLoaded = presetLoaded;
            PresetSaved = presetSaved;
            PhantomPower = phantomPowerState;
            DeviceIsBusy = deviceisbusy;
        }

    }

    public class WPLogicEventArgs
    {
        bool[] LogicInputState;
        bool[] LogicOutputState;
        int[] Voltage;


        public WPLogicEventArgs(bool[] logicInputState, bool[] logicOutputState, int[] voltage)
        {
            LogicInputState = logicInputState;
            LogicOutputState = logicOutputState;
            Voltage = voltage;
        }
    }

    public class WPFrontPanelEventArgs
    {
        public bool FrontPanelLock;

        public WPFrontPanelEventArgs(bool frontPanelLockedState)
        {
            FrontPanelLock = frontPanelLockedState;
        }
    }

    public class WPBluToothEventArgs : EventArgs
    {

        public string bluToothMac;
        public string bluToothFriendlyName;
        public bool bluToothClosedConnections;
        public eBlueToothStatus bluToothStatus;
        public string bluToothSong;
        public string bluToothArtist;
        public string bluToothAlbum;

        public WPBluToothEventArgs(string BluToothMac, string BluToothFriendlyname, bool closedConnections, eBlueToothStatus status, string song, string artist, string album)
        {
            bluToothMac = BluToothMac;
            bluToothFriendlyName = BluToothFriendlyname;
            bluToothClosedConnections = closedConnections;
            bluToothStatus = status;
            bluToothSong = song;
            bluToothArtist = artist;
            bluToothAlbum = album;

        }
    }

    #endregion



}