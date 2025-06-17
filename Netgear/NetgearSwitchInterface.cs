using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Netgear.Events;
using Netgear.Resources;
using Netgear.Communications;

namespace Netgear
{
    public class NetgearSwitchInterface
    {
        private NetgearSwitch parent;
        private CTimer pollTimer;
        private ushort pollTimeInterval;

        private bool IsDebug;

        private ushort parentDeviceID;
        public ushort ParentDeviceID
        {
            get { return this.parentDeviceID; }
            set
            {
                if (this.parentDeviceID != value)
                {
                    this.parentDeviceID = value;
                    this.DebugMessage("Parent Device ID Assigned: {0}", this.parentDeviceID);
                    NetgearSwitch parent = NetgearDevices.FindNetgearSwitchDevice(this.parentDeviceID);
                    if (parent != null)
                    {
                        this.parent = parent;
                        this.DebugMessage("Parent Switch {0} @ {1} Retrieved", parent.ID, parent.SymbolName);
                        bool registered = this.parent.RegisterSwitchInterface(this);
                        this.DebugMessage("Parent {0} @ {1} Registration Result {2}", parent.ID, parent.SymbolName, registered == true ? "Success" : "Failure");
                        //fire registration event
                        if (this.Registered != null) { this.Registered(this, new DigitalAnalogPayloadEventArgs(registered)); }
                    }
                    else { this.DebugMessage("Parent Switch Not Available or Defined"); }
                }
            }
        }

        private string symbolName = String.Empty;
        public string SymbolName
        {
            get { return this.symbolName; }
            set
            {
                if (this.symbolName != value)
                {
                    this.symbolName = value;
                    this.DebugMessage("Symbol Name Assigned");
                }
            }
        }

        private ushort port;
        public ushort Port 
        {
            get { return this.port; }
            private set
            {
                if (this.port != value)
                {
                    this.port = value;
                    this.DebugMessage("Port Changed: {0}", this.port);
                    if (this.PortChanged != null) this.PortChanged(this, new DigitalAnalogPayloadEventArgs(this.port));
                }
            }
        }

        private string description = String.Empty;
        public string Description 
        {
            get { return this.description; }
            private set
            {
                if (this.description != value)
                {
                    this.description = value;
                    this.DebugMessage("Description Changed: {0}", this.description);
                    if (this.DescriptionChanged != null) this.DescriptionChanged(this, new StringPayloadEventArgs(this.description));
                }
            }
        }

        private ushort administratorMode;
        public ushort AdministratorMode 
        {
            get { return this.administratorMode; }
            private set
            {
                if (this.administratorMode != value)
                {
                    this.administratorMode = value;
                    this.DebugMessage("Administrator Mode Changed: {0}", this.administratorMode);
                    if (this.AdministratorModeChanged != null) this.AdministratorModeChanged(this, new DigitalAnalogPayloadEventArgs(this.administratorMode));
                }
            }
        }

        public RateLimit RateLimitUnicast { get; private set; }
        public RateLimit RateLimitBroadcast { get; private set; }
        public RateLimit RateLimitMulticast { get; private set; }

        private PortStatus status;
        public PortStatus Status 
        {
            get { return this.status; }
            private set
            {
                if (this.status != value)
                {
                    this.status = value;
                    this.DebugMessage("Status Changed: {0}", this.status);
                    if (this.StatusChanged != null) this.StatusChanged(this, new DigitalAnalogPayloadEventArgs((ushort)this.status));
                }
            }
        }

        public PortPOEStatus poeStatus;
        public PortPOEStatus POEStatus 
        {
            get { return this.poeStatus; }
            private set
            {
                if (this.poeStatus != value)
                {
                    this.poeStatus = value;
                    this.DebugMessage("POE Status Changed: {0}", this.poeStatus);
                    if (this.PowerOverEthernetStatusChanged != null) this.PowerOverEthernetStatusChanged(this, new DigitalAnalogPayloadEventArgs((ushort)this.poeStatus));
                }
            }
        }

        private PortMode portMode; 
        public PortMode PortMode 
        {
            get { return this.portMode; }
            private set
            {
                if (this.portMode != value)
                {
                    this.portMode = value;
                    this.DebugMessage("Port Mode Changed: {0}", this.portMode);
                    if (this.PortModeChanged != null) this.PortModeChanged(this, new DigitalAnalogPayloadEventArgs((ushort)this.portMode));
                }
            }
        }

        private List<ushort> vlanMembership = new List<ushort>();
        public List<ushort> VLANMembership 
        { 
            get { return this.vlanMembership; }
            private set
            {
                if (value != null)
                {
                    List<ushort> newVlanMembership = value;
                    //create a string representing our current membership
                    string currentVLANMembership = String.Join(", ", this.vlanMembership.Select(item => item.ToString()).ToArray());
                    //create a string representing the new vlan membership
                    string newVLANMembership = String.Join(", ", this.vlanMembership.Select(item => item.ToString()).ToArray());
                    //check if they are different
                    if (newVLANMembership != currentVLANMembership)
                    {
                        this.vlanMembership = value;
                        this.DebugMessage("VLAN Membership Changed: {0}", newVLANMembership);
                        this.VLANMembershipDetails = newVLANMembership;
                        //fire event
                        if (this.VLANMembershipDetailsChanged != null) { this.VLANMembershipDetailsChanged(this, new StringPayloadEventArgs(this.VLANMembershipDetails)); }
                    }
                }
            }
        }

        private string VLANMembershipDetails = String.Empty;

        private string txBytes = String.Empty;
        public string TransmittedDataBytes 
        {
            get { return this.txBytes; }
            private set
            {
                if (this.txBytes != value)
                {
                    this.txBytes = value;
                    this.DebugMessage("Tx Bytes Changed: {0}", this.txBytes);
                    if (this.TransmittedDataBytesChanged != null) this.TransmittedDataBytesChanged(this, new StringPayloadEventArgs(this.txBytes));
                }
            }
        }

        private string rxBytes = String.Empty;
        public string ReceivedDataBytes
        {
            get { return this.rxBytes; }
            private set
            {
                if (this.rxBytes != value)
                {
                    this.rxBytes = value;
                    this.DebugMessage("Rx Bytes Changed: {0}", this.rxBytes);
                    if (this.ReceivedDataBytesChanged != null) this.ReceivedDataBytesChanged(this, new StringPayloadEventArgs(this.rxBytes));
                }
            }
        }

        private string rxSpeed = String.Empty;
        public string ReceiveSpeed
        {
            get { return this.rxSpeed; }
            private set
            {
                if (this.rxSpeed != value)
                {
                    this.rxSpeed = value;
                    this.DebugMessage("Rx Speed Changed: {0}", this.rxSpeed);
                    if (this.ReceiveSpeedChanged != null) this.ReceiveSpeedChanged(this, new StringPayloadEventArgs(this.rxSpeed));
                }
            }
        }

        private string txSpeed = String.Empty;
        public string TransmitSpeed
        {
            get { return this.txSpeed; }
            private set
            {
                if (this.txSpeed != value)
                {
                    this.txSpeed = value;
                    this.DebugMessage("Tx Speed Changed: {0}", this.txSpeed);
                    if (this.TransmitSpeedChanged != null) this.TransmitSpeedChanged(this, new StringPayloadEventArgs(this.txSpeed));
                }
            }
        }

        private ushort txRate;
        public ushort TransmitRate
        {
            get { return this.txRate; }
            private set
            {
                if (this.txRate != value)
                {
                    this.txRate = value;
                    this.DebugMessage("Tx Rate Changed: {0}", this.txRate);
                    //fire event
                }
            }
        }

        private ushort nativeVLAN;
        public ushort NativeVLAN
        {
            get { return this.nativeVLAN; }
            private set
            {
                if (this.nativeVLAN != value)
                {
                    this.nativeVLAN = value;
                    this.DebugMessage("Native VLAN Changed: {0}", this.nativeVLAN);
                    if (this.NativeVLANChanged != null) { this.NativeVLANChanged(this, new DigitalAnalogPayloadEventArgs(this.NativeVLAN)); }
                }
            }
        }

        private ushort linkTrap;
        public ushort LinkTrap
        {
            get { return this.linkTrap; }
            private set
            {
                if (this.linkTrap != value)
                {
                    this.linkTrap = value;
                    this.DebugMessage("Link Trap Changed: {0}", this.linkTrap);
                    if (this.LinkTrapChanged != null) { this.LinkTrapChanged(this, new DigitalAnalogPayloadEventArgs(this.LinkTrap)); }
                }
            }
        }

        private string errorsReceived = String.Empty;
        public string ErrorsReceived
        {
            get { return this.errorsReceived; }
            private set
            {
                if (this.errorsReceived != value)
                {
                    this.errorsReceived = value;
                    this.DebugMessage("Errors Received Changed: {0}", this.errorsReceived);
                    if (this.ErrorsReceivedChanged != null) this.ErrorsReceivedChanged(this, new StringPayloadEventArgs(this.errorsReceived));
                }
            }
        }

        private string errorPacketsReceived = String.Empty;
        public string ErrorPacketsReceived
        {
            get { return this.errorPacketsReceived; }
            private set
            {
                if (this.errorPacketsReceived != value)
                {
                    this.errorPacketsReceived = value;
                    this.DebugMessage("Rx Bytes Class Changed: {0}", this.errorPacketsReceived);
                    if (this.ErrorPacketsReceivedChanged != null) this.ErrorPacketsReceivedChanged(this, new StringPayloadEventArgs(this.errorPacketsReceived));
                }
            }
        }

        private string droppedPackets = String.Empty;
        public string DroppedPackets
        {
            get { return this.droppedPackets; }
            set
            {
                if (this.droppedPackets != value)
                {
                    this.droppedPackets = value;
                    this.DebugMessage("Dropped Packets Changed: {0}", this.droppedPackets);
                    if (this.DroppedPacketsChanged != null) this.DroppedPacketsChanged(this, new StringPayloadEventArgs(this.droppedPackets));

                }
            }
        }

        private string macAddress = String.Empty;
        public string MAC 
        {
            get { return this.macAddress; }
            private set
            {
                if (this.macAddress != value)
                {
                    this.macAddress = value;
                    this.DebugMessage("MAC Address Changed: {0}", this.macAddress);
                    if (this.MACChanged != null) this.MACChanged(this, new StringPayloadEventArgs(this.macAddress));
                }
            }
        }

        private PortLinkSpeed linkSpeed;
        public PortLinkSpeed LinkSpeed
        {
            get { return this.linkSpeed; }
            private set
            {
                if (this.linkSpeed != value)
                {
                    this.linkSpeed = value;
                    this.DebugMessage("Link Speed Changed: {0}", this.linkSpeed);
                    if (this.LinkSpeedChanged != null) this.LinkSpeedChanged(this, new DigitalAnalogPayloadEventArgs((ushort)this.linkSpeed));
                }
            }
        }

        private PortInterfaceSpeed interfaceSpeed;
        public PortInterfaceSpeed InterfaceSpeed 
        {
            get { return this.interfaceSpeed; }
            private set
            {
                if (this.interfaceSpeed != value)
                {
                    this.interfaceSpeed = value;
                    this.DebugMessage("Interface Speed Changed: {0}", this.interfaceSpeed);
                    if (this.InterfaceSpeedChanged != null) this.InterfaceSpeedChanged(this, new DigitalAnalogPayloadEventArgs((ushort)this.interfaceSpeed));
                }
            }
        }

        private DuplexMode duplex;
        public DuplexMode Duplex 
        {
            get { return this.duplex; }
            private set
            {
                if (this.duplex != value)
                {
                    this.duplex = value;
                    this.DebugMessage("Duplex Changed: {0}", this.duplex);
                    if (this.DuplexChanged != null) this.DuplexChanged(this, new DigitalAnalogPayloadEventArgs((ushort)this.duplex));
                }
            }
        }

        private int frameSize;
        public int FrameSize 
        {
            get { return this.frameSize; }
            private set
            {
                if (this.frameSize != value)
                {
                    this.frameSize = value;
                    this.DebugMessage("Frame Size Changed: {0}", this.frameSize);
                    if (this.FrameSizeChanged != null) this.FrameSizeChanged(this, new DigitalAnalogPayloadEventArgs(this.frameSize));
                }
            }
        }

        private string scheduleName = String.Empty;
        public string ScheduleName
        {
            get { return this.scheduleName; }
            private set
            {
                if (this.scheduleName != value)
                {
                    this.scheduleName = value;
                    this.DebugMessage("Schedule Name Changed: {0}", this.scheduleName);
                    if (this.ScheduleNameChanged != null) { this.ScheduleNameChanged(this, new StringPayloadEventArgs(this.ScheduleName)); }
                }
            }
        }

        private ushort maxFrameSize;
        public ushort MaxFrameSize
        {
            get { return this.maxFrameSize; }
            private set
            {
                if (this.maxFrameSize != value)
                {
                    this.maxFrameSize = value;
                    this.DebugMessage("Max Frame Changed: {0}", this.maxFrameSize);
                    if (this.MaxFrameSizeChanged != null) { this.MaxFrameSizeChanged(this, new DigitalAnalogPayloadEventArgs(this.MaxFrameSize)); }
                }
            }
        }

        private ushort defaultVLANPriority;
        public ushort DefaultVLANPriority
        {
            get { return this.defaultVLANPriority; }
            private set
            {
                if (this.defaultVLANPriority != value)
                {
                    this.defaultVLANPriority = value;
                    this.DebugMessage("Default VLAN Priority Changed: {0}", this.defaultVLANPriority);
                    if (this.DefaultVLANPriorityChanged != null) { this.DefaultVLANPriorityChanged(this, new DigitalAnalogPayloadEventArgs(this.DefaultVLANPriority)); }
                }
            }
        }

        private ushort poeCapable;
        public ushort POECapable
        {
            get { return this.poeCapable; }
            private set
            {
                if (this.poeCapable != value)
                {
                    this.poeCapable = value;
                    this.DebugMessage("POE Capable Changed: {0}", this.poeCapable);
                    if (this.POECapableChanged != null) { this.POECapableChanged(this, new DigitalAnalogPayloadEventArgs(this.poeCapable)); }
                }
            }
        }

        private ushort poeEnabled;
        public ushort POEEnabled
        {
            get { return this.poeEnabled; }
            private set
            {
                if (this.poeEnabled != value)
                {
                    this.poeEnabled = value;
                    this.DebugMessage("POE Enabled Changed: {0}", this.poeEnabled);
                    if (this.POEEnabledChanged != null) { this.POEEnabledChanged(this, new DigitalAnalogPayloadEventArgs(this.POEEnabled)); }
                }
            }
        }

        private ushort flowControl;
        public ushort FlowControl 
        {
            get { return this.flowControl; }
            private set
            {
                if (this.flowControl != value)
                {
                    this.flowControl = value;
                    this.DebugMessage("Flow Control Changed: {0}", this.flowControl);
                    if (this.FlowControlChanged != null) this.FlowControlChanged(this, new DigitalAnalogPayloadEventArgs(this.flowControl));
                }
            }
        }

        private ushort lacp;
        public ushort LACP 
        {
            get { return this.lacp; }
            private set
            {
                if (this.lacp != value)
                {
                    this.lacp = value;
                    this.DebugMessage("LACP Changed: {0}", this.lacp);
                    if (this.LACPChanged != null) this.LACPChanged(this, new DigitalAnalogPayloadEventArgs(this.lacp));
                }
            }
        }

        private ushort mirrored;
        public ushort Mirrored 
        {
            get { return this.mirrored; }
            private set
            {
                if (this.mirrored != value)
                {
                    this.mirrored = value;
                    this.DebugMessage("Mirrored Changed: {0}", this.mirrored);
                    if (this.MirroredChanged != null) this.MirroredChanged(this, new DigitalAnalogPayloadEventArgs(this.mirrored));
                }
            }
        }

        private ushort stp;
        public ushort STP 
        {
            get { return this.stp; }
            private set
            {
                if (this.stp != value)
                {
                    this.stp = value;
                    this.DebugMessage("STP Changed: {0}", this.stp);
                    if (this.STPChanged != null) this.STPChanged(this, new DigitalAnalogPayloadEventArgs(this.stp));
                }
            }
        }

        private PortSTPState stpStatus;
        public PortSTPState STPStatus 
        {
            get { return this.stpStatus; }
            private set
            {
                if (this.stpStatus != value)
                {
                    this.stpStatus = value;
                    this.DebugMessage("STP Status Changed: {0}", this.stpStatus);
                    if (this.STPStatusChanged != null) this.STPStatusChanged(this, new DigitalAnalogPayloadEventArgs((ushort)this.stpStatus));
                }
            }
        }

        private PortOperationalStatus operationalStatus;
        public PortOperationalStatus OperationalStatus 
        {
            get { return this.operationalStatus; }
            private set
            {
                if (this.operationalStatus != value)
                {
                    this.operationalStatus = value;
                    this.DebugMessage("Operational Status Changed: {0}", this.operationalStatus);
                    if (this.OperationalStatusChanged != null) this.OperationalStatusChanged(this, new DigitalAnalogPayloadEventArgs((ushort)this.operationalStatus));

                }
            }
        }

        private PortPOEPowerClass poeClass;
        public PortPOEPowerClass POEClass 
        {
            get { return this.poeClass; }
            private set
            {
                if (this.poeClass != value)
                {
                    this.poeClass = value;
                    this.DebugMessage("POE Class Changed: {0}", this.poeClass);
                    if (this.POEClassChanged != null) this.POEClassChanged(this, new DigitalAnalogPayloadEventArgs((ushort)this.poeClass));
                }
            }
        }

        private PortPOEPowerLimit poePowerLimit;
        public PortPOEPowerLimit POEPowerLimit
        {
            get { return this.poePowerLimit; }
            private set
            {
                if (this.poePowerLimit != value)
                {
                    this.poePowerLimit = value;
                    this.DebugMessage("POE Limit Changed: {0}", this.poePowerLimit);
                    if (this.POEPowerLimitChanged != null) this.POEPowerLimitChanged(this, new DigitalAnalogPayloadEventArgs((ushort)this.poePowerLimit));
                }
            }
        }

        private ushort poePowerLimitMilliwatts = Numbers.POEPowerLimitMillwattsDefault;
        public ushort POEPowerLimitMilliwatts
        {
            get { return this.poePowerLimitMilliwatts; }
            private set
            {
                if (this.poePowerLimitMilliwatts != value)
                {
                    this.poePowerLimitMilliwatts = value;
                    this.DebugMessage("POE Limit Milliwatts Changed: {0}", this.poePowerLimitMilliwatts);
                    if (this.POEPowerLimitMilliwattsChanged != null) this.POEPowerLimitMilliwattsChanged(this, new DigitalAnalogPayloadEventArgs((ushort)this.poePowerLimitMilliwatts));
                }
            }
        }

        private ushort currentPowerMilliwatts;
        public ushort CurrentPOEPowerMilliwatts
        {
            get { return this.currentPowerMilliwatts; }
            private set
            {
                if (this.currentPowerMilliwatts != value)
                {
                    this.currentPowerMilliwatts = value;
                    this.DebugMessage("Current Power Draw Milliwatts Changed: {0}", this.currentPowerMilliwatts);
                    if (this.CurrentPOEPowerMilliwattsChanged != null) this.CurrentPOEPowerMilliwattsChanged(this, new DigitalAnalogPayloadEventArgs((ushort)this.currentPowerMilliwatts));
                }
            }
        }

        private PortAuthorizationStatus authorization;
        public PortAuthorizationStatus Authorization 
        {
            get { return this.authorization; }
            private set
            {
                if (this.authorization != value) 
                {
                    this.authorization = value;
                    this.DebugMessage("Authorization Changed: {0}", this.authorization);
                    if (this.AuthorizationChanged != null) this.AuthorizationChanged(this, new DigitalAnalogPayloadEventArgs((ushort)this.authorization));
                }
            }
        }

        private NeighborInfo neighbor = new NeighborInfo();
        public NeighborInfo Neighbor 
        {
            get { return this.neighbor; }
            private set
            {
                if (!this.neighbor.Equals(value))
                {
                    this.neighbor = value;
                    this.DebugMessage("Neighbor Changed: {0}", this.neighbor);
                    if (this.NeighborChanged != null) this.NeighborChanged(this, new DeviceInterfaceNeighborPayloadEventArgs(this.neighbor));
                }
            }
        }

        private List<ForwardingDatabaseEntry> forwardingDatabaseEntries = new List<ForwardingDatabaseEntry>();
        public List<ForwardingDatabaseEntry> ForwardingDatabaseEntries 
        {
            get { return this.forwardingDatabaseEntries; }
            set
            {
                if (this.forwardingDatabaseEntries != value && value != null)
                {
                    if (this.CheckForwardingDatabaseForChanges(value)) 
                    {
                        this.forwardingDatabaseEntries = value;
                        this.ForwardingDatabaseEntriesChanged(); 
                    }
                    else { this.DebugMessage("Not Updating Forwarding Database"); }
                }
                else { this.DebugMessage("Object Provided Invalid, or Identical to the Existing Value"); }
            }
        }

        public event DigitalAnalogPayload Registered;

        public event DigitalAnalogPayload AdministratorModeChanged;
        public event DigitalAnalogPayload AuthorizationChanged;
        public event DigitalAnalogPayload CurrentPOEPowerMilliwattsChanged;
        public event StringPayload DescriptionChanged;
        public event StringPayload DroppedPacketsChanged;
        public event DigitalAnalogPayload DuplexChanged;
        public event DigitalAnalogPayload DefaultVLANPriorityChanged;
        public event StringPayload ErrorPacketsReceivedChanged;
        public event StringPayload ErrorsReceivedChanged;
        public event DigitalAnalogPayload FlowControlChanged;
        public event DigitalAnalogPayload FrameSizeChanged;
        public event DigitalAnalogPayload LACPChanged;
        public event DigitalAnalogPayload LinkTrapChanged;
        public event StringPayload MACChanged;
        public event DigitalAnalogPayload MirroredChanged;
        public event DigitalAnalogPayload MaxFrameSizeChanged;
        public event DigitalAnalogPayload NativeVLANChanged;
        public event DigitalAnalogPayload OperationalStatusChanged;
        public event DigitalAnalogPayload POEClassChanged;
        public event DigitalAnalogPayload PortChanged;
        public event DigitalAnalogPayload PortModeChanged;
        public event DigitalAnalogPayload PowerOverEthernetStatusChanged;
        public event DigitalAnalogPayload POEPowerLimitChanged;
        public event DigitalAnalogPayload POEPowerLimitMilliwattsChanged;
        public event DigitalAnalogPayload POEEnabledChanged;
        public event DigitalAnalogPayload POECapableChanged;
        public event StringPayload ReceivedDataBytesChanged;
        public event StringPayload ReceiveSpeedChanged;
        public event StringPayload ScheduleNameChanged;
        public event DigitalAnalogPayload LinkSpeedChanged;
        public event DigitalAnalogPayload InterfaceSpeedChanged;
        public event DigitalAnalogPayload StatusChanged;
        public event DigitalAnalogPayload STPChanged;
        public event DigitalAnalogPayload STPStatusChanged;
        public event StringPayload TransmitSpeedChanged;
        public event StringPayload TransmittedDataBytesChanged;
        public event StringPayload VLANMembershipDetailsChanged;

        //neighbor events

        public event DeviceInterfaceNeighborPayload NeighborChanged;

        public event DigitalAnalogArrayPayload VLANs;
        public event StringArrayPayload MACs;
        public event DigitalAnalogArrayPayload Types;

        public NetgearSwitchInterface()
        {
            this.pollTimer = new CTimer(this.OnPollTimerExpired, Timeout.Infinite);
            this.SymbolName = Strings.DefaultSymbolName;
            this.RateLimitBroadcast = new RateLimit();
            this.RateLimitMulticast = new RateLimit();
            this.RateLimitUnicast = new RateLimit();
            this.Status = PortStatus.UNKNOWN;
            NetgearDevices.DeviceAdded += this.OnNetgearSwitchAdded;
        }

        public void Initialize(string name, ushort port, ushort parentID, ushort interval)
        {
            this.SymbolName = name;
            this.Port = port;
            this.ParentDeviceID = parentID;
            this.pollTimeInterval = (ushort)(interval * 10);
            //this.pollTimer.Reset(this.pollTimeInterval, this.pollTimeInterval);
        }

        private void OnDebug(object sender, DigitalAnalogPayloadEventArgs args)
        {
            this.IsDebug = Conversion.ConvertToBool(args.Payload);
        }

        public void EnableDebug(ushort debug)
        {
            this.IsDebug = Conversion.ConvertToBool(debug);
        }

        public void EnablePoll(ushort poll)
        {
            if (Conversion.ConvertToBool(poll)) { this.pollTimer.Reset(this.pollTimeInterval, this.pollTimeInterval); }
            else { this.pollTimer.Stop(); }
        }

        /// <summary>
        /// print the message if debug mode is enabled
        /// </summary>
        /// <param name="msg">a format string</param>
        /// <param name="objects">the array of objects to print inside the format string</param>
        private void DebugMessage(string msg, params object[] objects)
        {
            if (this.IsDebug) { this.Message(msg, objects); }
        }

        /// <summary>
        /// prints a message to the console
        /// </summary>
        /// <param name="msg">a format string</param>
        /// <param name="objects">the array of objects to print inside the format string</param>
        private void Message(string msg, params object[] objects)
        {
            CrestronConsole.PrintLine(String.Format("Netgear Switch Interface {2} @ {0}: {1}", this.SymbolName, msg, this.Port), objects);
            Console.WriteLine(String.Format("Netgear Switch Interface {2} @ {0}: {1}", this.SymbolName, msg, this.Port), objects);
        }

        private void OnPollTimerExpired(object sender)
        {
            if (this.parent != null)
            {
                this.parent.GetPortDetails(this.Port);
            }
        }

        private void OnNetgearSwitchAdded(NetgearSwitch payload)
        {
            if (this.parent == null) //only assign if we havent already found a parent
            {
                if (payload != null) //make sure the payload isnt null somehow
                {
                    if (payload.ID == this.ParentDeviceID) //make sure the payload is one we give a shit about
                    {
                        this.parent = payload; //assign the payload
                        this.DebugMessage("Newly Added Parent {0} @ {1} Assigned", this.parent.ID, this.parent.SymbolName); //notify
                        bool registered = this.parent.RegisterSwitchInterface(this); //register with the parent
                        this.DebugMessage("Parent {0} @ {1} Registration Result {2}", parent.ID, parent.SymbolName, registered == true ? "Success" : "Failure");
                        //fire the registered event
                        if (this.Registered != null) { this.Registered(this, new DigitalAnalogPayloadEventArgs(registered)); }
                    }
                }
            }
        }

        public void UpdatePortPOEConfiguration(DevicePortPOEConfigurationRequestResponsePayload payload)
        {
            this.DebugMessage("Updating POE Configuration");

            this.POEClass = payload.Class;
            this.CurrentPOEPowerMilliwatts = payload.CurrentPowerMilliwatts;
            this.POEEnabled = Conversion.ConvertToUshort(payload.Enable);
            //payload.Port;
            this.POEPowerLimit = payload.PowerLimit;
            this.POEPowerLimitMilliwatts = payload.PowerLimitMilliwatts;
            //payload.Reset;
            this.POEStatus = payload.Status;
        }

        public void UpdatePortConfiguration(DevicePortConfigurationRequestResponsePayload payload)
        {
            this.DebugMessage("Updating Port Configuration");

            if (payload.Status == PortStatus.LINK_DOWN)
            {
                //if the port is down, it cannot have any forwarding database entries
                this.ForwardingDatabaseEntries = new List<ForwardingDatabaseEntry>();
                //if the port is down, it also cannot have a neighbor info;
                this.Neighbor = new NeighborInfo();
            }

            this.AdministratorMode = payload.AdministratorMode;
            this.DefaultVLANPriority = payload.DefaultVLANPriority;
            this.Description = payload.Description;
            this.Duplex = payload.Duplex;
            this.MaxFrameSize = payload.MaxFrameSize;
            this.POECapable = Conversion.ConvertToUshort(payload.POECapable);
            this.Port = payload.Port;
            this.RateLimitBroadcast = payload.RateLimitBroadcast;
            this.RateLimitMulticast = payload.RateLimitMulticast;
            this.RateLimitUnicast = payload.RateLimitUnicast;
            this.ScheduleName = payload.ScheduleName;
            this.LinkSpeed = payload.Speed;
            this.Status = payload.Status;
            this.TransmitRate = payload.TransmitRate;
            this.LinkTrap = payload.Trap;
            this.PortMode = payload.Type;
            this.NativeVLAN = payload.VLAN;
        }

        public void UpdatePortStatistics(DevicePortStatisticsResponsePayload payload)
        {
            this.DebugMessage("Updating Port Statistics");

            if (payload.Status == PortStatus.LINK_DOWN)
            {
                //if the port is down, it cannot have any forwarding database entries
                this.ForwardingDatabaseEntries = new List<ForwardingDatabaseEntry>();
                //if the port is down, it also cannot have a neighbor info;
                this.Neighbor = new NeighborInfo();
            }

            this.AdministratorMode = payload.AdministratorMode;
            this.Authorization = payload.Authorization;
            this.Description = payload.Description;
            this.DroppedPackets = payload.DroppedPackets.ToString();
            this.Duplex = payload.Duplex;
            this.ErrorPacketsReceived = payload.ErrorPacketsReceived.ToString();
            this.ErrorsReceived = payload.ErrorsReceived.ToString();
            this.FlowControl = payload.FlowControl;
            this.FrameSize = payload.FrameSize;
            this.LACP = payload.LACP;
            this.MAC = payload.MAC;
            this.Mirrored = payload.Mirrored;
            this.Neighbor = payload.Neighbor;
            this.OperationalStatus = payload.OperationalStatus;
            this.POEClass = payload.POEClass;
            this.Port = payload.Port;
            this.PortMode = payload.PortMode;
            this.POEStatus = payload.PowerOverEthernetStatus;
            this.ReceivedDataBytes = payload.ReceivedDataBytes.ToString();
            this.ReceiveSpeed = payload.ReceiveSpeed;
            this.InterfaceSpeed = payload.Speed;
            this.Status = payload.Status;
            this.STP = payload.STP;
            this.STPStatus = payload.STPStatus;
            this.TransmitSpeed = payload.TransmitSpeed;
            this.TransmittedDataBytes = payload.TransmittedDataBytes.ToString();
            this.VLANMembership = payload.VLANMembership;
        }

        private bool CheckForwardingDatabaseForChanges(List<ForwardingDatabaseEntry> newDatabase)
        {
            //by default we will assume that the databases are teh same
            bool result = false;
            //create two strings to easily check the databases for differences
            string currentMACEntries = String.Join(",", this.ForwardingDatabaseEntries.Select(entry => entry.MAC).ToArray());
            string newMACEntries = String.Join(",", newDatabase.Select(entry => entry.MAC).ToArray());
            //if these strings are different, then we want to fire off the event, so we should return true, indicating changes
            this.DebugMessage("Old Fowarding Database {0}, New Forwarding Database {1}", currentMACEntries, newMACEntries);
            if (currentMACEntries != newMACEntries) { result = true; }
            //return the result
            return result;
        }

        private void ForwardingDatabaseEntriesChanged()
        {
            this.DebugMessage("Updating Forwarding Database: {0} Entries", this.ForwardingDatabaseEntries.Count); 

            if (this.VLANs != null) { this.VLANs(this, new DigitalAnalogArrayPayloadEventArgs(this.ForwardingDatabaseEntries.Select(entry => (ushort)entry.VLAN).ToArray(), this.ForwardingDatabaseEntries.Count)); }
            if (this.Types != null) { this.Types(this, new DigitalAnalogArrayPayloadEventArgs(this.ForwardingDatabaseEntries.Select(entry => (ushort)entry.DatabaseEntryType).ToArray(), this.ForwardingDatabaseEntries.Count)); }
            if (this.MACs != null) { this.MACs(this, new StringArrayPayloadEventArgs(this.ForwardingDatabaseEntries.Select(entry => entry.MAC).ToArray(), this.ForwardingDatabaseEntries.Count)); }
        }

        public void UpdateForwardingDatabaseEntries(List<ForwardingDatabaseEntry> entries)
        {
            this.ForwardingDatabaseEntries = entries;
        }

        public void EnablePOE(ushort enable)
        {
            if (this.parent != null)
            {
                this.DebugMessage("{0} POE", enable == 1 ? "Enable" : "Disable");
                this.parent.PortEnablePOE(this, Conversion.ConvertToBool(enable));
            }
            else { this.DebugMessage("No Valid Parent | Enable POE Message"); }
        }

        public void ResetPOE()
        {
            if (this.parent != null)
            {
                this.DebugMessage("Resetting POE");
                this.parent.PortResetPOE(this);
            }
            else { this.DebugMessage("No Valid Parent | Reset POE Message"); }
        }
    }
}