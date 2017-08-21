using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
using System.Windows.Forms;


using IpcContractClientInterface;
using AppLog = IpcUtil.Logging;
using System.Globalization;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace IPCTemplate
{
    public partial class UserForm : Form
    {
        /// <summary>Are we in design mode</summary>
        protected bool mDesignMode { get; private set; }

        #region Standard IPC Variables

        /// <summary>This ensures consistent read and write culture</summary>
        private NumberFormatInfo mNFI = new CultureInfo("en-GB", true).NumberFormat; // Force UN English culture

        /// <summary>Collection of all IPC channels, this object always exists.</summary>
        private Channels mChannels = new Channels();

        #endregion Standard IPC Variables

        #region Application Variables

        #region Status Enumeration Variables

        /// <summary>
        /// State of entire application
        /// </summary>
        private enum EApplicationState { Ready, Running, NotSetUp, NoConnection, Error };
        private EApplicationState mApplicationState = EApplicationState.NoConnection;

        /// <summary>
        /// Running state of application
        /// </summary>
        private enum ERunningState { Idle, CTScan, ShearBox, Error };
        private ERunningState mRunningState = ERunningState.Idle;

        /// <summary>
        /// Inspect-X State
        /// </summary>
        private enum EInspectXState { Idle, AcquireShadingCorrection, CTScanAcquiring, CTScanReconstructing, Error }
        private EInspectXState mInspectXState = EInspectXState.Idle;

        /// <summary>
        /// Inspect-X Connection state
        /// </summary>
        private Channels.EConnectionState mInspectXConnectionState = Channels.EConnectionState.NoServer;

        /// <summary>
        /// ShearBox State
        /// </summary>
        private enum EShearBoxState { Idle, InMotion, Error }
        private EShearBoxState mShearBoxState = EShearBoxState.Idle;

        /// <summary>
        /// USB Connection State
        /// </summary>
        private enum EUSBConnectionState { Connected, Disconnected, Error }
        private EUSBConnectionState mUSBConnectionState = EUSBConnectionState.Disconnected;

        #endregion Status Enumeration Variables

        #region Profile variables

        /// <summary> Selected Profile </summary>
        private string mCTProfile = "";
        /// <summary> Selected Profile Index </summary>
        private int mCTProfileIndex = 0;
        /// <summary> List of CT profiles </summary>
        private List<string> mCTProfileList = null;

        #endregion Profile variables

        #region Project settings

        /// <summary>
        /// Project Name
        /// </summary>
        private string mProjectName = "";

        /// <summary>
        /// Root Directory
        /// </summary>
        private string mRootDirectory = @"C:\Users\mbbxkpg2\Documents\CT_Data";

        /// <summary>
        /// Project Directory
        /// </summary>
        private string mProjectDirectory = "";

        /// <summary>
        /// Number of Shear Cycles
        /// </summary>
        private Int32 mNumberOfShearCycles = 0;

        /// <summary>
        /// Shear cycles completed?
        /// </summary>
        private Int32 mShearCyclesCompleted = 0;

        #endregion Project settings

        #region Indicator Flags

        // Shading Correction Flag
        private bool mShadingCorrectionCompleted = false;

        // Scan Flag
        private bool mCTScanCompleted = false;

        // User initiated stop
        private bool mStop = false;

        #endregion Indicator Flags


        /// <summary> Output log text that includes time and date stamps </summary>
        public string mOutputLogText = null;

        #endregion Application Variables

        public UserForm()
        {
            try
            {
                mDesignMode = (LicenseManager.CurrentContext.UsageMode == LicenseUsageMode.Designtime);
                InitializeComponent();
                if (!mDesignMode)
                {
                    // Tell normal logging who the parent window is.
                    AppLog.SetParentWindow = this;
                    AppLog.TraceInfo = true;
                    AppLog.TraceDebug = true;

                    mChannels = new Channels();
                    // Enable the channels that will be controlled by this application.
                    // For the generic IPC client this is all of them!
                    // This just sets flags, it does not actually open the channels.
                    mChannels.AccessApplication = true;
                    mChannels.AccessXray = false;
                    mChannels.AccessManipulator = true;
                    mChannels.AccessImageProcessing = false;
                    mChannels.AccessInspection = false;
                    mChannels.AccessInspection2D = false;
                    mChannels.AccessCT3DScan = true;
                    mChannels.AccessCT2DScan = false;
                }
                UpdateUI();
            }
            catch (Exception ex) { AppLog.LogException(ex); }
        }

        #region Channel connections

        /// <summary>Attach to channel and connect any event handlers</summary>
        /// <returns>Connection status</returns>
        private Channels.EConnectionState ChannelsAttach()
        {
            try
            {
                if (mChannels != null)
                {
                    Channels.EConnectionState State = mChannels.Connect();
                    if (State == Channels.EConnectionState.Connected)  // Open channels
                    {
                        // Attach event handlers (as required)

                        if (mChannels.Application != null)
                        {
                            mChannels.Application.mEventSubscriptionHeartbeat.Event +=
                                new EventHandler<CommunicationsChannel_Application.EventArgsHeartbeat>(EventHandlerHeartbeatApp);
                            mChannels.Application.mEventSubscriptionInspectXStatus.Event +=
                                new EventHandler<CommunicationsChannel_Application.EventArgsInspectXStatus>(EventHandlerInspectXStatus);
                            mChannels.Application.mEventSubscriptionInspectXAlarms.Event +=
                                new EventHandler<CommunicationsChannel_Application.EventArgsInspectXAlarms>(EventHandlerInspectXAlarms);
                        }


                        if (mChannels.Manipulator != null)
                        {
                            mChannels.Manipulator.mEventSubscriptionHeartbeat.Event +=
                                new EventHandler<CommunicationsChannel_Manipulator.EventArgsHeartbeat>(EventHandlerHeartbeatMan);
                            mChannels.Manipulator.mEventSubscriptionManipulatorMove.Event +=
                                new EventHandler<CommunicationsChannel_Manipulator.EventArgsManipulatorMoveEvent>(EventHandlerManipulatorMoveEvent);
                            mChannels.Manipulator.mEventSubscriptionAxisPositionChanged.Event +=
                                new EventHandler<CommunicationsChannel_Manipulator.EventArgsAxisPositionChanged>(EventHandlerSubscriptionAxisPositionChanged);
                        }

                        if (mChannels.CT3DScan != null)
                        {
                            mChannels.CT3DScan.mEventSubscriptionHeartbeat.Event +=
                                new EventHandler<CommunicationsChannel_CT3DScan.EventArgsHeartbeat>(EventHandlerHeartbeatCT3DScan);
                            mChannels.CT3DScan.mEventSubscriptionCTStatus.Event +=
                                new EventHandler<CommunicationsChannel_CT3DScan.EventArgsCTStatus>(EventHandlerSubscriptionScanStatus);
                            mChannels.CT3DScan.mEventSubscriptionShadingCorrectionsStatus.Event +=
                                new EventHandler<CommunicationsChannel_CT3DScan.EventArgsShadingCorrectionsStatus>(EventHandlerSubscriptionShadingCorrectionsStatus);
                        }


                    }
                    return State;
                }
            }
            catch (Exception ex) { AppLog.LogException(ex); }
            return Channels.EConnectionState.Error;
        }

        /// <summary>Detach channel and disconnect any event handlers</summary>
        /// <returns>true if OK</returns>
        private bool ChannelsDetach()
        {
            try
            {
                if (mChannels != null)
                {
                    // Detach event handlers

                    if (mChannels.Application != null)
                    {
                        mChannels.Application.mEventSubscriptionHeartbeat.Event -=
                            new EventHandler<CommunicationsChannel_Application.EventArgsHeartbeat>(EventHandlerHeartbeatApp);
                        mChannels.Application.mEventSubscriptionInspectXStatus.Event -=
                            new EventHandler<CommunicationsChannel_Application.EventArgsInspectXStatus>(EventHandlerInspectXStatus);
                        mChannels.Application.mEventSubscriptionInspectXAlarms.Event -=
                            new EventHandler<CommunicationsChannel_Application.EventArgsInspectXAlarms>(EventHandlerInspectXAlarms);
                    }


                    if (mChannels.Manipulator != null)
                    {
                        mChannels.Manipulator.mEventSubscriptionHeartbeat.Event -=
                            new EventHandler<CommunicationsChannel_Manipulator.EventArgsHeartbeat>(EventHandlerHeartbeatMan);
                        mChannels.Manipulator.mEventSubscriptionManipulatorMove.Event -=
                            new EventHandler<CommunicationsChannel_Manipulator.EventArgsManipulatorMoveEvent>(EventHandlerManipulatorMoveEvent);
                        mChannels.Manipulator.mEventSubscriptionAxisPositionChanged.Event -=
                            new EventHandler<CommunicationsChannel_Manipulator.EventArgsAxisPositionChanged>(EventHandlerSubscriptionAxisPositionChanged);
                    }

                    if (mChannels.CT3DScan != null)
                    {
                        mChannels.CT3DScan.mEventSubscriptionHeartbeat.Event -=
                            new EventHandler<CommunicationsChannel_CT3DScan.EventArgsHeartbeat>(EventHandlerHeartbeatCT3DScan);
                        mChannels.CT3DScan.mEventSubscriptionCTStatus.Event -=
                            new EventHandler<CommunicationsChannel_CT3DScan.EventArgsCTStatus>(EventHandlerSubscriptionScanStatus);
                        mChannels.CT3DScan.mEventSubscriptionShadingCorrectionsStatus.Event -=
                            new EventHandler<CommunicationsChannel_CT3DScan.EventArgsShadingCorrectionsStatus>(EventHandlerSubscriptionShadingCorrectionsStatus);
                    }

                    Thread.Sleep(100); // A breather for events to finish!
                    return mChannels.Disconnect(); // Close channels
                }
            }
            catch (Exception ex) { AppLog.LogException(ex); }
            return false;
        }

        #endregion Channel connections

        #region Heartbeat from host

        void EventHandlerHeartbeatApp(object aSender, CommunicationsChannel_Application.EventArgsHeartbeat e)
        {
            try
            {
                if (mChannels == null || mChannels.Application == null)
                    return;
                if (this.InvokeRequired)
                    this.BeginInvoke((MethodInvoker)delegate { EventHandlerHeartbeatApp(aSender, e); });
                else
                {
                    //your code goes here....
                }
            }
            catch (ObjectDisposedException) { } // ignore
            catch (Exception ex) { AppLog.LogException(ex); }
        }

        void EventHandlerHeartbeatMan(object aSender, CommunicationsChannel_Manipulator.EventArgsHeartbeat e)
        {
            try
            {
                if (mChannels == null || mChannels.Manipulator == null)
                    return;
                if (this.InvokeRequired)
                    this.BeginInvoke((MethodInvoker)delegate { EventHandlerHeartbeatMan(aSender, e); });
                else
                {
                    //your code goes here....
                }
            }
            catch (ObjectDisposedException) { } // ignore
            catch (Exception ex) { AppLog.LogException(ex); }
        }

        void EventHandlerHeartbeatCT3DScan(object aSender, CommunicationsChannel_CT3DScan.EventArgsHeartbeat e)
        {
            try
            {
                if (mChannels == null || mChannels.CT3DScan == null)
                    return;
                if (this.InvokeRequired)
                    this.BeginInvoke((MethodInvoker)delegate { EventHandlerHeartbeatCT3DScan(aSender, e); });
                else
                {
                    //your code goes here....
                }
            }
            catch (ObjectDisposedException) { } // ignore
            catch (Exception ex) { AppLog.LogException(ex); }
        }

        #endregion Heartbeat from host

        #region STATUS FROM HOST

        #region Application

        void EventHandlerInspectXStatus(object sender, CommunicationsChannel_Application.EventArgsInspectXStatus e)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    MethodInvoker Del = delegate { EventHandlerInspectXStatus(sender, e); }; this.Invoke(Del);
                }
                else
                {
                    switch (e.Status)
                    {
                        case Inspect_X_Definitions.InspectXStatus.AutoConditioning:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXStatus.AutomaticDoorOpen:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXStatus.Error:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXStatus.Homing:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXStatus.Idle:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXStatus.MainDoorProbablyOpen:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXStatus.Scanning:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXStatus.Uninitialised:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXStatus.Unknown:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXStatus.UpdatingShadingCorrections:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXStatus.WaitingForOperator:
                            //Your code goes here...
                            break;
                    }
                }
            }
            catch (Exception ex) { AppLog.LogException(ex); }
        }

        void EventHandlerInspectXAlarms(object sender, CommunicationsChannel_Application.EventArgsInspectXAlarms e)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    MethodInvoker Del = delegate { EventHandlerInspectXAlarms(sender, e); }; this.Invoke(Del);
                }
                else
                {
                    switch (e.Alarms)
                    {
                        case Inspect_X_Definitions.InspectXAlarms.AutomaticDoorFault:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.AutomaticDoorOpen:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.Coolant:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.DoorEdge:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.DoorServiceMode:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.FrameGrabber:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.InspectXNotResponding:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.LoaderInterlock:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.ManipulatorController:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.ManipulatorFollowingError:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.ManipulatorInterlock:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.ManipulatorOverRotateError:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.ManipulatorStoppedCTScanAbort:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.ManipulatorUnderRotateError:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.ManipulatorUnhomed:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.None:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.ReconstructionPC:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.ScanStoppedByOperator:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.TemperatureController:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.Unexpected:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.WaitingForOperator:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.XRayController:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.XRayFilamentDemand:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.XRayInterlock:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.XRayStabiliseFail:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.XRayTripCTScan:
                            //Your code goes here...
                            break;
                        case Inspect_X_Definitions.InspectXAlarms.XRayTripCTScanAbort:
                            //Your code goes here...
                            break;
                    }

                }
            }
            catch (Exception ex) { AppLog.LogException(ex); }
        }

        #endregion

        #region Manipulator

        void EventHandlerManipulatorMoveEvent(object aSender, CommunicationsChannel_Manipulator.EventArgsManipulatorMoveEvent e)
        {
            try
            {
                if (mChannels == null || mChannels.Manipulator == null)
                    return;
                if (this.InvokeRequired)
                    this.BeginInvoke((MethodInvoker)delegate { EventHandlerManipulatorMoveEvent(aSender, e); }); // Make it non blocking if called form this UI thread
                else
                {
                    Debug.Print(DateTime.Now.ToString("dd/MM/yyyy H:mm:ss.fff") + " : " + "e.Status=" + e.MoveEvent.ToString());
                    switch (e.MoveEvent)
                    {
                        case IpcContract.Manipulator.EMoveEvent.HomingStarted:
                            // Your code goes here...
                            break;
                        case IpcContract.Manipulator.EMoveEvent.HomingCompleted:
                            // Your code goes here...
                            break;
                        case IpcContract.Manipulator.EMoveEvent.ManipulatorStartedMoving:
                            // Your code goes here...
                            break;
                        case IpcContract.Manipulator.EMoveEvent.ManipulatorStoppedMoving:
                            // Your code goes here...
                            Debug.Print(DateTime.Now.ToString("dd/MM/yyyy H:mm:ss.fff") + " : " + "Current Position=" +
                                mChannels.Manipulator.Axis.Position(IpcContract.Manipulator.EAxisName.X) + " " +
                                mChannels.Manipulator.Axis.Position(IpcContract.Manipulator.EAxisName.Y) + " " +
                                mChannels.Manipulator.Axis.Position(IpcContract.Manipulator.EAxisName.Z) + " " +
                                mChannels.Manipulator.Axis.Position(IpcContract.Manipulator.EAxisName.Rotate) + " " +
                                mChannels.Manipulator.Axis.Position(IpcContract.Manipulator.EAxisName.Tilt) + " " +
                                mChannels.Manipulator.Axis.Position(IpcContract.Manipulator.EAxisName.Detector));
                            break;
                        case IpcContract.Manipulator.EMoveEvent.FilamentChangePositionGoStarted:
                            // Your code goes here...
                            break;
                        case IpcContract.Manipulator.EMoveEvent.GoCompleted:
                            // Your code goes here...
                            break;
                        case IpcContract.Manipulator.EMoveEvent.GoStarted:
                            // Your code goes here...
                            break;
                        case IpcContract.Manipulator.EMoveEvent.LoadPositionGoCompleted:
                            // Your code goes here...
                            break;
                        case IpcContract.Manipulator.EMoveEvent.LoadPositionGoStarted:
                            // Your code goes here...
                            break;
                        case IpcContract.Manipulator.EMoveEvent.Error:
                            // Your code goes here...
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex) { AppLog.LogException(ex); }
        }

        void EventHandlerDoorStateChanged(object aSender, CommunicationsChannel_Manipulator.EventArgsDoorStateChanged e)
        {
            try
            {

                if (mChannels == null || mChannels.Manipulator == null)
                    return;
                if (this.InvokeRequired)
                    this.BeginInvoke((MethodInvoker)delegate { EventHandlerDoorStateChanged(aSender, e); }); // Make it non blocking if called form this UI thread
                else
                {

                    switch (e.DoorStateChanged)
                    {
                        case IpcContract.Manipulator.EDoorStateChanged.Closed:
                            // Your code goes here...
                            break;
                        case IpcContract.Manipulator.EDoorStateChanged.Closing:
                            // Your code goes here...
                            break;
                        case IpcContract.Manipulator.EDoorStateChanged.Error:
                            // Your code goes here...
                            break;
                        case IpcContract.Manipulator.EDoorStateChanged.Opened:
                            // Your code goes here...
                            break;
                        case IpcContract.Manipulator.EDoorStateChanged.Opening:
                            // Your code goes here...
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex) { AppLog.LogException(ex); }
        }

        void EventHandlerSubscriptionAxisPositionChanged(object aSender, CommunicationsChannel_Manipulator.EventArgsAxisPositionChanged e)
        {
            try
            {

                if (mChannels == null || mChannels.Manipulator == null)
                    return;
                if (this.InvokeRequired)
                    this.BeginInvoke((MethodInvoker)delegate { EventHandlerSubscriptionAxisPositionChanged(aSender, e); }); // Make it non blocking if called form this UI thread
                else
                {
                    // Your code goes here...
                }
            }
            catch (Exception ex) { AppLog.LogException(ex); }
        }

        #endregion

        #region CT3DScan

        void EventHandlerSubscriptionScanStatus(object aSender, CommunicationsChannel_CT3DScan.EventArgsCTStatus e)
        {

            try
            {
                if (mChannels != null && mChannels.CT3DScan != null)
                {
                    if (this.InvokeRequired)
                        this.BeginInvoke((MethodInvoker)delegate { EventHandlerSubscriptionScanStatus(aSender, e); });
                    else
                    {
                        // Find out what type of status is being reported.
                        if (e.Status is Inspect_X_Definitions.CTStatusPreparingForAcquisition)
                        {
                            // Preparing for a CT scan
                            StatusHandler3DCTPreparation(e.Status as Inspect_X_Definitions.CTStatusPreparingForAcquisition);
                        }
                        else if (e.Status is Inspect_X_Definitions.CTStatusPreparationCompleted)
                        {
                            // Preparation for CT scan complete (status)
                            StatusHandler3DCTPreparationComplete(e.Status as Inspect_X_Definitions.CTStatusPreparationCompleted);
                        }
                        else if (e.Status is Inspect_X_Definitions.CTStatusAcquiring)
                        {
                            // CT scan in progress
                            StatusHandler3DCTAcquiring(e.Status as Inspect_X_Definitions.CTStatusAcquiring);
                        }
                        else if (e.Status is Inspect_X_Definitions.CTStatusAcquisitionCompleted)
                        {
                            // CT scan completed (status)
                            StatusHandler3DCTAcquistionCompleted(e.Status as Inspect_X_Definitions.CTStatusAcquisitionCompleted);
                        }
                        else if (e.Status is Inspect_X_Definitions.CTStatusReconstructing)
                        {
                            // Reconstruction in progress
                            StatusHandler3DCTReconstructing(e.Status as Inspect_X_Definitions.CTStatusReconstructing);
                        }
                        else if (e.Status is Inspect_X_Definitions.CTStatusReconstructionCompleted)
                        {
                            // Reconstruction complete
                            StatusHandler3DCTReconstructionComplete(e.Status as Inspect_X_Definitions.CTStatusReconstructionCompleted);
                        }
                        else if (e.Status is Inspect_X_Definitions.CTStatusConvertingVolume)
                        {
                            // Converting volume
                            StatusHandler3DCTConvertingVolume(e.Status as Inspect_X_Definitions.CTStatusConvertingVolume);
                        }
                        else if (e.Status is Inspect_X_Definitions.CTStatusVolumeConverted)
                        {
                            // Volume conversion complete
                            StatusHandler3DCTConvertingVolumeComplete(e.Status as Inspect_X_Definitions.CTStatusVolumeConverted);
                        }
                        else if (e.Status is Inspect_X_Definitions.CTStatusVolumeAnalysisRunning)
                        {
                            // Volume analysis
                            StatusHandler3DCTConvertingVolumeAnalysis(e.Status as Inspect_X_Definitions.CTStatusVolumeAnalysisRunning);
                        }
                        else if (e.Status is Inspect_X_Definitions.CTStatusVolumeAnalysisCompleted)
                        {
                            // Volume analysis complete
                            StatusHandler3DCTConvertingVolumeAnalysisComplete(e.Status as Inspect_X_Definitions.CTStatusVolumeAnalysisCompleted);
                        }
                        else if (e.Status is Inspect_X_Definitions.CTStatusException)
                        {
                            // Exception occurred
                            StatusHandler3DCTException(e.Status as Inspect_X_Definitions.CTStatusException);
                        }
                        else if (e.Status is Inspect_X_Definitions.CTStatusCompleted)
                        {
                            mCTScanCompleted = true;
                            mInspectXState = EInspectXState.Idle;
                            UpdateUI();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
            }
        }

        private void StatusHandler3DCTPreparation(Inspect_X_Definitions.CTStatusPreparingForAcquisition status)
        {
            try
            {
                // Preparing for CT scan

                switch (status.Detail)
                {
                    case Inspect_X_Definitions.CTStatusPreparingForAcquisition.CTPreparingDetail.Unknown:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusPreparingForAcquisition.CTPreparingDetail.WaitingForXrays:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusPreparingForAcquisition.CTPreparingDetail.StartingManipulator:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusPreparingForAcquisition.CTPreparingDetail.WaitingForManipulator:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusPreparingForAcquisition.CTPreparingDetail.ReturningManipulatorToStartPosition:
                        // Your code goes here
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
            }
        }

        private void StatusHandler3DCTPreparationComplete(Inspect_X_Definitions.CTStatusPreparationCompleted status)
        {
            try
            {
                // Preparation for CT scan complete (status)
                switch (status.Error)
                {
                    case Inspect_X_Definitions.CTStatusPreparationCompleted.CTPreparationError.Unknown:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusPreparationCompleted.CTPreparationError.NoError:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusPreparationCompleted.CTPreparationError.XraysFailedToStabilise:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusPreparationCompleted.CTPreparationError.CheckStateOfReconApp:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusPreparationCompleted.CTPreparationError.CTAgentDoesNotExpectOperatorID:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusPreparationCompleted.CTPreparationError.CTAgentNotResponding:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusPreparationCompleted.CTPreparationError.InvalidCtComMessageType:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusPreparationCompleted.CTPreparationError.OperatorCTDataFolderMissing:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusPreparationCompleted.CTPreparationError.OperatorCTDataFolderNotWriteable:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusPreparationCompleted.CTPreparationError.DiskFullOnReconstructionComputer:
                        // Your code goes here
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
            }
        }

        private void StatusHandler3DCTAcquiring(Inspect_X_Definitions.CTStatusAcquiring status)
        {
            try
            {
                switch (status.Detail)
                {
                    case Inspect_X_Definitions.CTStatusAcquiring.CTAcquiringDetail.Unknown:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusAcquiring.CTAcquiringDetail.Acquiring:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusAcquiring.CTAcquiringDetail.CreatingSinogram:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusAcquiring.CTAcquiringDetail.ReturningManipulatorToStartPosition:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusAcquiring.CTAcquiringDetail.RestartingXrays:
                        // Your code goes here
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
            }
        }

        private void StatusHandler3DCTAcquistionCompleted(Inspect_X_Definitions.CTStatusAcquisitionCompleted status)
        {
            try
            {

                switch (status.Error)
                {
                    case Inspect_X_Definitions.CTStatusAcquisitionCompleted.CTAcquisitionError.Unknown:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusAcquisitionCompleted.CTAcquisitionError.NoError:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusAcquisitionCompleted.CTAcquisitionError.CTAbortedAsNoXrays:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusAcquisitionCompleted.CTAcquisitionError.CheckStateOfReconApp:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusAcquisitionCompleted.CTAcquisitionError.CTAgentNotResponding:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusAcquisitionCompleted.CTAcquisitionError.InvalidCtComMessageType:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusAcquisitionCompleted.CTAcquisitionError.QueueReconstructionFailed:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusAcquisitionCompleted.CTAcquisitionError.FailedToObtainReconstructionProgressConnection:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusAcquisitionCompleted.CTAcquisitionError.AbortedByUser:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusAcquisitionCompleted.CTAcquisitionError.DroppedFrames:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusAcquisitionCompleted.CTAcquisitionError.BadFastCTCalibration:
                        // Your code goes here
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
            }
        }

        private void StatusHandler3DCTReconstructing(Inspect_X_Definitions.CTStatusReconstructing status)
        {
            try
            {
                if (mInspectXState != EInspectXState.CTScanReconstructing)
                {
                    mInspectXState = EInspectXState.CTScanReconstructing;
                    UpdateUI();
                }
                switch (status.Detail)
                {
                    case Inspect_X_Definitions.CTStatusReconstructing.CTReconstructionDetail.Unknown:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructing.CTReconstructionDetail.CalculatingCoR:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructing.CTReconstructionDetail.Reconstructing:
                        // Your code goes here
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
            }
        }

        private void StatusHandler3DCTReconstructionComplete(Inspect_X_Definitions.CTStatusReconstructionCompleted status)
        {
            try
            {
                switch (status.Error)
                {
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.Unknown:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.NoError:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.Aborted:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.CentreOfRotationFailure:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.ReconstructionDeviceLost:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.RunOutOfDiskSpace:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.EmptyVolumeMask:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.ExceptionCaught:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.ExternalModuleException:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.ExternalModuleFileMissing:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.FileAccessError:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.FileNameError:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.InvalidParameter:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.NotLicensed:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.OutOfMemory:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.ReconstructionParameterFileNotFound:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.ReconstructionNotStarted:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.ReconstructionStarted:
                        // Your code goes here
                        break;
                    //case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.VolumeAnalysisMacroError:
                    //    // Your code goes here
                    //    break;
                    //case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.VolumeAnalysisProjectLoadFailure:
                    //    // Your code goes here
                    //    break;
                    //case Inspect_X_Definitions.CTStatusReconstructionCompleted.CTReconstructionError.VolumeAnalysisStartFailure:
                    //    // Your code goes here
                    //    break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
            }
        }

        private void StatusHandler3DCTConvertingVolume(Inspect_X_Definitions.CTStatusConvertingVolume status)
        {
            try
            {
                // Your code here
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
            }
        }

        private void StatusHandler3DCTConvertingVolumeComplete(Inspect_X_Definitions.CTStatusVolumeConverted status)
        {
            try
            {
                switch (status.Error)
                {
                    case Inspect_X_Definitions.CTStatusVolumeConverted.CTConversionError.Unknown:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusVolumeConverted.CTConversionError.NoError:
                        // Your code goes here
                        break;
                    default:
                        // Your code goes here
                        break;
                }
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
            }
        }

        private void StatusHandler3DCTConvertingVolumeAnalysis(Inspect_X_Definitions.CTStatusVolumeAnalysisRunning status)
        {
            try
            {
                switch (status.Detail)
                {
                    case Inspect_X_Definitions.CTStatusVolumeAnalysisRunning.CTVolumeAnalysisDetail.Unknown:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusVolumeAnalysisRunning.CTVolumeAnalysisDetail.LoadingVolume:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusVolumeAnalysisRunning.CTVolumeAnalysisDetail.Analysing:
                        // Your code goes here
                        break;
                    default:
                        // Your code goes here
                        break;
                }
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
            }
        }

        private void StatusHandler3DCTConvertingVolumeAnalysisComplete(Inspect_X_Definitions.CTStatusVolumeAnalysisCompleted status)
        {
            try
            {
                switch (status.Error)
                {
                    case Inspect_X_Definitions.CTStatusVolumeAnalysisCompleted.CTVolumeAnalysisError.Unknown:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusVolumeAnalysisCompleted.CTVolumeAnalysisError.NoError:
                        // Your code goes here
                        break;
                    default:
                        // Your code goes here
                        break;
                }
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
            }
        }

        private void StatusHandler3DCTException(Inspect_X_Definitions.CTStatusException status)
        {
            try
            {
                switch (status.Error)
                {
                    case Inspect_X_Definitions.CTStatusException.CTExceptionError.Unknown:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusException.CTExceptionError.Unused:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusException.CTExceptionError.System:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusException.CTExceptionError.FileAccess:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusException.CTExceptionError.FileNotFound:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusException.CTExceptionError.ReconstructionComputer:
                        // Your code goes here
                        break;
                    default:
                        // Your code goes here
                        break;
                }
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
            }
        }

        void EventHandlerSubscriptionShadingCorrectionsStatus(object aSender, CommunicationsChannel_CT3DScan.EventArgsShadingCorrectionsStatus e)
        {
            try
            {
                if (mChannels != null && mChannels.CT3DScan != null)
                {
                    if (this.InvokeRequired)
                        this.BeginInvoke((MethodInvoker)delegate { EventHandlerSubscriptionShadingCorrectionsStatus(aSender, e); });
                    else
                    {
                        // Debug.Print(DateTime.Now.ToString("dd/MM/yyyy H:mm:ss.fff") + " : " + "e.Status=" + e.Status.ToString());
                        // Check for end of update
                        if (e.Status is Inspect_X_Definitions.ShadingCorrectionStatusCompleted)
                        {
                            // Check if update finished O.K.
                            if ((e.Status as Inspect_X_Definitions.ShadingCorrectionStatusCompleted).OK)
                            {
                                mShadingCorrectionCompleted = true;
                                mInspectXState = EInspectXState.Idle;
                                UpdateUI();
                            }
                            else
                            {
                                DisplayLog("Error in acquiring shading corrections");
                                mInspectXState = EInspectXState.Error;
                                UpdateUI();
                            }
                        }
                        else
                        {
                            // Deal with message giving progress of update
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
            }
        }

        #endregion

        #endregion Status from host

        #region UI functions

        #region User Interface

        #region Form Layout

        /// <summary>
        /// Delegate function for updating UI
        /// </summary>
        private delegate void UpdateUIEventHandler();

        /// <summary>
        /// Update the UI
        /// </summary>
        private void UpdateUI()
        {
            // If not on UI thread then use delegate and invoke
            if (this.InvokeRequired)
            {
                this.BeginInvoke((UpdateUIEventHandler)UpdateUI);
                return;
            }

            // Check connections and settings
            if (ConnectionOK())
                ProjectSettingsOK();

            // Layout Connect and Disconnect buttons
            LayoutConnectDiconnectButtons();
            // Format Inspect-X Connection status
            FormatInspectXConnectionStatus();
            // Format Application status
            FormatApplicationStatus();
            // Format Current Running State
            FormatCurrentRunningState();
            // Format Current Inspect-X State
            FormatInspectXStatus();

        }

        /// <summary>
        /// Layout Connect and Disconnect buttons
        /// </summary>
        private void LayoutConnectDiconnectButtons()
        {
            switch (mInspectXConnectionState)
            {
                case Channels.EConnectionState.Connected:
                    btn_CTConnect.Enabled = false;
                    btn_CTDisconnect.Enabled = true;
                    break;
                case Channels.EConnectionState.NoServer:
                    btn_CTConnect.Enabled = true;
                    btn_CTDisconnect.Enabled = false;
                    break;
                case Channels.EConnectionState.Error:
                    btn_CTConnect.Enabled = false;
                    btn_CTDisconnect.Enabled = false;
                    break;
            }
        }

        /// <summary>
        /// Function checking whether Connection is OK
        /// </summary>
        /// <returns>True if connections are ok; false otherwise</returns>
        private bool ConnectionOK()
        {
            try
            {
                if (mInspectXConnectionState == Channels.EConnectionState.Connected)
                    return true;
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
                mApplicationState = EApplicationState.Error;
                DisplayLog("Error in connection");
            }
            return false;
        }

        /// <summary>
        /// Function for checking whether project settings are ok 
        /// </summary>
        private void ProjectSettingsOK()
        {
            bool projectNameOK = false;
            bool rootDirectoryOK = false;
            bool profileOK = false;
            try
            {
                projectNameOK = !String.IsNullOrWhiteSpace(mProjectName);
                rootDirectoryOK = Directory.Exists(mRootDirectory);
                profileOK = !String.IsNullOrWhiteSpace(mCTProfile);

                // If it is not already running then we can change the Application state
                if (mApplicationState != EApplicationState.Running)
                {
                    if (projectNameOK && rootDirectoryOK && profileOK)
                        mApplicationState = EApplicationState.Ready;
                    else
                        mApplicationState = EApplicationState.NotSetUp;
                }
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
                mApplicationState = EApplicationState.Error;
                DisplayLog("Error in checking settings");
            }

        }

        /// <summary>
        /// Load UI
        /// </summary>
        private void LoadUI()
        {
            try
            {
                // Set project name text box to default value
                textBox_ProjectName.Text = mProjectName;
                // set root directory text box to default value
                textBox_RootDirectory.Text = mRootDirectory;
                // Update profile list
                UpdateProfileList();
                // Update UI
                UpdateUI();
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
                mApplicationState = EApplicationState.Error;
                DisplayLog("Error in loading UI");
            }
        }

        #endregion Form Layout

        #region Panel - CT Connection

        /// <summary>
        /// Function called when btn_CTConnect is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_CTConnect_Click(object sender, EventArgs e)
        {
            try
            {
                mInspectXConnectionState = ChannelsAttach();
                if (mInspectXConnectionState == Channels.EConnectionState.Connected)
                {
                    DisplayLog("Connected to Inspect-X");
                    mApplicationState = EApplicationState.NotSetUp;
                    LoadUI();
                }
                else
                    DisplayLog("Error connecting to Inspect-X");
                UpdateUI();
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
                mInspectXConnectionState = Channels.EConnectionState.Error;
                DisplayLog("Error in connecting to Inspect-X");
                MessageBox.Show("Error: Connecting to Inspect-X\r\nPlease make sure Inspect-X is running on this machine");
                UpdateUI();
            }
        }

        /// <summary>
        /// Function called when btn_CTDisconnect is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_CTDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                bool ChannelsDetatched = ChannelsDetach();
                if (ChannelsDetatched)
                {
                    mInspectXConnectionState = Channels.EConnectionState.NoServer;
                    DisplayLog("Disconnected from Inspect-X");
                }
                else
                {
                    mInspectXConnectionState = Channels.EConnectionState.Error;
                    DisplayLog("Error when disconnecting from Inspect-X");
                }
                UpdateUI();
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
                mInspectXConnectionState = Channels.EConnectionState.Error;
                DisplayLog("Error in disconnecting from Inspect-X");
                UpdateUI();
            }

        }


        #endregion Panel - CT Connection

        #region Panel - Project Settings

        /// <summary>
        /// Function called when ProjectName textbox text is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_ProjectName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                mProjectName = textBox_ProjectName.Text;
                UpdateUI();
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
                mApplicationState = EApplicationState.Error;
                DisplayLog("Error in setting project name");
            }

        }

        /// <summary>
        /// Function called when Root Directory is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_RootDirectory_TextChanged(object sender, EventArgs e)
        {
            try
            {
                mRootDirectory = textBox_RootDirectory.Text;
                UpdateUI();
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
                mApplicationState = EApplicationState.Error;
                DisplayLog("Error in setting root project directory");
            }
        }

        /// <summary>
        /// Function called when numeric number of shear cycles is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDown_NumberOfShearCycles_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                mNumberOfShearCycles = (int)Math.Round(numericUpDown_NumberOfShearCycles.Value);
                UpdateUI();
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
                mApplicationState = EApplicationState.Error;
                DisplayLog("Error in setting number of shear cycles");
            }
        }

        #endregion Panel - Project Settings

        #region Panel - CT Profile

        /// <summary>
        /// Refresh the profile list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Refresh_Click(object sender, EventArgs e)
        {
            try
            {
                // Update profile list
                UpdateProfileList();
                // Update UI
                UpdateUI();
            }
            catch (Exception ex) { AppLog.LogException(ex); }
        }

        /// <summary>
        /// Select a CT profile using the combobox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Retrieve index 
                RetrieveIndex();
                // Update UI
                UpdateUI();

            }
            catch (Exception ex) { AppLog.LogException(ex); }
        }

        /// <summary>
        /// Browse for CT profile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Browse_Click(object sender, EventArgs e)
        {
            if (mDesignMode)
                return;

            try
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.SelectedPath = mRootDirectory;

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textBox_RootDirectory.Text = fbd.SelectedPath;
                    mRootDirectory = fbd.SelectedPath;
                    UpdateUI();
                }
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
                mApplicationState = EApplicationState.Error;
                DisplayLog("Error in selecting root folder");
            }
        }

        private void UpdateProfileList()
        {
            mCTProfileList = mChannels.CT3DScan.ProfileList();
            comboBoxProfile.DataSource = mCTProfileList;
            DisplayLog("Profile list updated");
            RetrieveIndex();
        }

        private void RetrieveIndex()
        {
            // Retrieve selected profile index
            mCTProfileIndex = comboBoxProfile.SelectedIndex;
            // If correctly selected then application state is ready. 
            if (mCTProfileIndex >= 0)
            {
                // Set Application state
                mApplicationState = EApplicationState.Ready;
                // Set profile
                mCTProfile = mCTProfileList[mCTProfileIndex];
                // Notify that profile loaded
                DisplayLog(@"Profile '" + mCTProfile + @"' loaded");
            }
        }

        #endregion Panel - CT Profile

        #region Panel - System Status

        /// <summary>
        /// Format Connection status
        /// </summary>
        private void FormatInspectXConnectionStatus()
        {
            switch (mInspectXConnectionState)
            {
                case Channels.EConnectionState.Connected:
                    box_InspectXConnection.BackColor = Color.Green;
                    box_InspectXConnection.Text = "Connected";
                    panel_CTProfile.Enabled = true;
                    panel_USBConnection.Enabled = false;
                    panel_ProjectSettings.Enabled = true;
                    break;
                case Channels.EConnectionState.NoServer:
                    box_InspectXConnection.BackColor = Color.Gray;
                    box_InspectXConnection.Text = "No Server";
                    panel_CTProfile.Enabled = false;
                    panel_USBConnection.Enabled = false;
                    panel_ProjectSettings.Enabled = false;
                    btn_Start.Enabled = false;
                    break;
                case Channels.EConnectionState.Error:
                    box_InspectXConnection.BackColor = Color.Red;
                    box_InspectXConnection.Text = "Connection Error";
                    panel_CTProfile.Enabled = false;
                    panel_USBConnection.Enabled = false;
                    panel_ProjectSettings.Enabled = false;
                    btn_Start.Enabled = false;
                    break;
            }
        }

        /// <summary>
        /// Format Application Status
        /// </summary>
        private void FormatApplicationStatus()
        {
            switch (mApplicationState)
            {
                case EApplicationState.Ready:
                    box_ApplicationState.BackColor = Color.Green;
                    box_ApplicationState.Text = "Ready";
                    panel_InspectXConnection.Enabled = true;
                    panel_CTProfile.Enabled = true;
                    panel_USBConnection.Enabled = false;
                    panel_ProjectSettings.Enabled = true;
                    btn_Start.Enabled = true;
                    btn_Start.Visible = true;
                    btn_Stop.Enabled = false;
                    btn_Stop.Visible = false;
                    break;
                case EApplicationState.Running:
                    box_ApplicationState.BackColor = Color.Blue;
                    box_ApplicationState.Text = "Running";
                    panel_InspectXConnection.Enabled = false;
                    panel_CTProfile.Enabled = false;
                    panel_USBConnection.Enabled = false;
                    panel_ProjectSettings.Enabled = false;
                    btn_Start.Enabled = false;
                    btn_Start.Visible = false;
                    btn_Stop.Enabled = true;
                    btn_Stop.Visible = true;
                    break;
                case EApplicationState.NotSetUp:
                    box_ApplicationState.BackColor = Color.Gray;
                    box_ApplicationState.Text = "Not Set Up";
                    panel_InspectXConnection.Enabled = true;
                    panel_CTProfile.Enabled = true;
                    panel_USBConnection.Enabled = false;
                    panel_ProjectSettings.Enabled = true;
                    btn_Start.Enabled = false;
                    btn_Start.Visible = true;
                    btn_Stop.Enabled = false;
                    btn_Stop.Visible = false;
                    break;
                case EApplicationState.NoConnection:
                    box_ApplicationState.BackColor = Color.Red;
                    box_ApplicationState.Text = "No Connection";
                    panel_InspectXConnection.Enabled = true;
                    panel_CTProfile.Enabled = false;
                    panel_USBConnection.Enabled = false;
                    panel_ProjectSettings.Enabled = false;
                    btn_Start.Enabled = false;
                    btn_Start.Visible = true;
                    btn_Stop.Enabled = false;
                    btn_Stop.Visible = false;
                    break;
                case EApplicationState.Error:
                    box_ApplicationState.BackColor = Color.Red;
                    box_ApplicationState.Text = "Application Error";
                    panel_InspectXConnection.Enabled = true;
                    panel_CTProfile.Enabled = false;
                    panel_USBConnection.Enabled = false;
                    panel_ProjectSettings.Enabled = false;
                    btn_Start.Enabled = false;
                    btn_Start.Visible = true;
                    btn_Stop.Enabled = false;
                    btn_Stop.Visible = false;
                    break;
            }
        }


        /// <summary>
        /// Format Application Status
        /// </summary>
        private void FormatInspectXStatus()
        {
            if (mApplicationState == EApplicationState.Running)
            {
                switch (mInspectXState)
                {
                    case EInspectXState.Idle:
                        box_CurrentInspectXState.BackColor = Color.Green;
                        box_CurrentInspectXState.Text = "Idle";
                        break;
                    case EInspectXState.AcquireShadingCorrection:
                        box_CurrentInspectXState.BackColor = Color.Blue;
                        box_CurrentInspectXState.Text = "Shading Correction";
                        break;
                    case EInspectXState.CTScanAcquiring:
                        box_CurrentInspectXState.BackColor = Color.Blue;
                        box_CurrentInspectXState.Text = "Acquiring CT Scan";
                        break;
                    case EInspectXState.CTScanReconstructing:
                        box_CurrentInspectXState.BackColor = Color.Blue;
                        box_CurrentInspectXState.Text = "Reconstructing CT Scan";
                        break;
                    case EInspectXState.Error:
                        box_CurrentInspectXState.BackColor = Color.Red;
                        box_CurrentInspectXState.Text = "Error";
                        break;
                    default:
                        box_CurrentInspectXState.BackColor = SystemColors.Control;
                        box_CurrentInspectXState.Text = "";
                        break;
                }
            }
            else
            {
                box_CurrentInspectXState.BackColor = SystemColors.Control;
                box_CurrentInspectXState.Text = "";
            }
        }

        /// <summary>
        /// Format Application Status
        /// </summary>
        private void FormatCurrentRunningState()
        {
            if (mApplicationState == EApplicationState.Running)
            {
                switch (mRunningState)
                {
                    case ERunningState.Idle:
                        box_CurrentRunningState.BackColor = Color.Green;
                        box_CurrentRunningState.Text = "Idle";
                        break;
                    case ERunningState.CTScan:
                        box_CurrentRunningState.BackColor = Color.Blue;
                        box_CurrentRunningState.Text = "CT Scan";
                        break;
                    case ERunningState.ShearBox:
                        box_CurrentRunningState.BackColor = Color.Blue;
                        box_CurrentRunningState.Text = "Shear box";
                        break;
                    case ERunningState.Error:
                        box_CurrentRunningState.BackColor = Color.Red;
                        box_CurrentRunningState.Text = "Red";
                        break;
                    default:
                        box_CurrentInspectXState.BackColor = SystemColors.Control;
                        box_CurrentInspectXState.Text = "";
                        break;
                }
            }
            else
            {
                box_CurrentRunningState.BackColor = SystemColors.Control;
                box_CurrentRunningState.Text = "";
            }
        }


        #endregion Panel - System Status

        #endregion User Interface

        #endregion UI function

        #region Output

        /// <summary>
        /// Create directory
        /// </summary>
        public void CreateDirectory()
        {
            // Create folders for test results
            string mTempFolderName = mProjectName + @"_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-tt");
            mProjectDirectory = mRootDirectory + @"\" + mTempFolderName;

            if (!Directory.Exists(mProjectDirectory))
            {
                Directory.CreateDirectory(mProjectDirectory);
            }
            else
            {
                mProjectDirectory += "2"; //just in case the folder already exists
                Directory.CreateDirectory(mProjectDirectory);
            }
        }

        /// <summary>
        /// Dumps info onto text box
        /// </summary>
        /// <param name="info"></param>
        public void DisplayLog(String info)
        {
            if (this.InvokeRequired)
                this.Invoke((MethodInvoker)delegate
                {
                    textBoxLog.Text += info + "\r\n";
                    mOutputLogText += DateTime.Now.ToString("dd/MM/yyyy H:mm:ss.fff") + " : " + info + "\r\n";
                    textBoxLog.SelectionStart = textBoxLog.Text.Length;
                    textBoxLog.ScrollToCaret();
                });
            else
            {

                textBoxLog.Text += info + "\r\n";
                mOutputLogText += DateTime.Now.ToString("dd/MM/yyyy H:mm:ss.fff") + " : " + info + "\r\n";
                textBoxLog.SelectionStart = textBoxLog.Text.Length;
                textBoxLog.ScrollToCaret();
            }
        }

        /// <summary>
        /// Function for clearing the output display box. Note the Output Log is not cleared. 
        /// </summary>
        public void DisplayLogClear()
        {
            if (this.InvokeRequired)
                this.Invoke((MethodInvoker)delegate
                {
                    textBoxLog.Clear();
                });
            else
            {
                textBoxLog.Clear();
            }
        }

        /// <summary>
        /// Prints output onto a log file
        /// </summary>
        public void OutputLogFile()
        {
            try
            {
                // Grab output text from textbox log
                string outputtext = textBoxLog.Text.ToString();

                // Filename
                string outputfilename = mProjectDirectory + @"\" + mProjectName + @".log";

                // open file
                System.IO.StreamWriter logfile = new StreamWriter(outputfilename, true);

                // Write to file
                logfile.Write(outputtext);

                //Close file
                logfile.Close();
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
            }
        }

        /// <summary>
        /// Prints output onto a log file
        /// </summary>
        /// <param name="aFilename"> Filename (note that .log will automatically be appended)</param>
        public void OutputLogFile(string aFilename)
        {
            try
            {
                // Grab output text from textbox log
                string outputtext = textBoxLog.Text.ToString();


                // Filename
                string outputfilename = aFilename + @".log";

                // open file
                System.IO.StreamWriter logfile = new StreamWriter(outputfilename, true);

                // Write to file
                logfile.Write(outputtext);

                //Close file
                logfile.Close();
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
            }
        }

        /// <summary>
        /// Prints output onto a log file
        /// </summary>
        public void WriteOutputFile(string aFilenamePostFix, string aOutputText)
        {
            try
            {
                // Filename
                string outputfilename = mProjectDirectory + @"\" + mProjectName + @"." + aFilenamePostFix;

                // open file
                System.IO.StreamWriter outputfile = new StreamWriter(outputfilename, true);

                // Write to file
                outputfile.Write(aOutputText);

                //Close file
                outputfile.Close();
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
            }
        }

        #endregion Output

        #region XCT Scan

        void XCTScan(string ScanName)
        {

            // Update running state
            mRunningState = ERunningState.CTScan;
            // Layout UI
            UpdateUI();

            // Response variables
            Inspect_X_Definitions.CTResponse CTresponse;
            Inspect_X_Definitions.ShadingCorrectionResponse scResponse;

            // Sample Info
            System.Collections.Generic.Dictionary<string, Object> sampleInfo =
                new System.Collections.Generic.Dictionary<string, Object>();

            sampleInfo.Add("Dataset name", ScanName);

            // Check Shading correction validity for the next hour
            scResponse = mChannels.CT3DScan.ValidateShadingCorrections(mCTProfile, 60 * 60);

            if (scResponse is Inspect_X_Definitions.ShadingCorrectionResponseValidity && !backgroundWorker_MainRoutine.CancellationPending)
            {
                // If not valid for next hour then update the shading correction
                if (!(scResponse as Inspect_X_Definitions.ShadingCorrectionResponseValidity).Valid)
                {
                    DisplayLog("Shading corrections not valid. Updating shading corrections...");

                    Debug.Print(DateTime.Now.ToString("dd/MM/yyyy H:mm:ss.fff") + " : " + "Current Position=" +
                            mChannels.Manipulator.Axis.Position(IpcContract.Manipulator.EAxisName.X) + " " +
                            mChannels.Manipulator.Axis.Position(IpcContract.Manipulator.EAxisName.Y) + " " +
                            mChannels.Manipulator.Axis.Position(IpcContract.Manipulator.EAxisName.Z) + " " +
                            mChannels.Manipulator.Axis.Position(IpcContract.Manipulator.EAxisName.Rotate) + " " +
                            mChannels.Manipulator.Axis.Position(IpcContract.Manipulator.EAxisName.Tilt) + " " +
                            mChannels.Manipulator.Axis.Position(IpcContract.Manipulator.EAxisName.Detector));

                    // Acquire shading correction
                    mInspectXState = EInspectXState.AcquireShadingCorrection;
                    mShadingCorrectionCompleted = false;
                    UpdateUI();
                    scResponse = mChannels.CT3DScan.UpdateShadingCorrection(mCTProfile);
                    // Wait until shading corrections finish
                    while (!mShadingCorrectionCompleted && !backgroundWorker_MainRoutine.CancellationPending)
                        Thread.Sleep(100);
                    if (!backgroundWorker_MainRoutine.CancellationPending)
                        DisplayLog("Shading corrections updated");
                }
            }

            if (!backgroundWorker_MainRoutine.CancellationPending)
            {
                // Start run
                DisplayLog("Starting CT Scan named '" + ScanName + "'");
                mCTScanCompleted = false;
                CTresponse = mChannels.CT3DScan.Run(mCTProfile, sampleInfo);

                // Check whether scan was accepted
                if (CTresponse is Inspect_X_Definitions.CTResponseOK && !backgroundWorker_MainRoutine.CancellationPending)
                {
                    // Set application state
                    mInspectXState = EInspectXState.CTScanAcquiring;
                }
                else if (backgroundWorker_MainRoutine.CancellationPending)
                    DisplayLog("Scan '" + ScanName + "' immediately cancelled by user");
                else
                {
                    // Set application state
                    mInspectXState = EInspectXState.Error;
                }
            }
            UpdateUI();

            // Wait until scan completed
            while (!mCTScanCompleted && !backgroundWorker_MainRoutine.CancellationPending)
                Thread.Sleep(500);

            if (!backgroundWorker_MainRoutine.CancellationPending)
                DisplayLog("Scan '" + ScanName + "' completed successfully");
            else
                DisplayLog("Scan '" + ScanName + "' terminated by user");

            // Update running state
            mRunningState = ERunningState.Idle;
            // Layout UI
            UpdateUI();
        }

        #endregion XCT Scan


        #region Main Routine

        /// <summary>
        /// Main Routine that runs on mMainRoutineThread
        /// </summary>
        private void MainRoutine()
        {
            if (!backgroundWorker_MainRoutine.CancellationPending)
            XCTScan(mProjectName + "_Test");
        }

        #endregion Main Routine

        private void btn_Start_Click(object sender, EventArgs e)
        {
            mApplicationState = EApplicationState.Running;
            UpdateUI();
            backgroundWorker_MainRoutine.RunWorkerAsync();
        }

        private void backgroundWorker_MainRoutine_DoWork(object sender, DoWorkEventArgs e)
        {
            // Run Main Routine
            MainRoutine();
            Thread.Sleep(5000);
        }

        private void backgroundWorker_MainRoutine_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mApplicationState = EApplicationState.Ready;
            UpdateUI();
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            try
            {
                DisplayLog("User initiated cancellation of process");

                // If application is running then try to cancel
                if (mApplicationState == EApplicationState.Running)
                {
                    if (mInspectXState == EInspectXState.AcquireShadingCorrection)
                    {
                        mChannels.CT3DScan.AbortShadingCorrectionUpdate();
                    }
                    else if (mInspectXState == EInspectXState.CTScanAcquiring)
                    {
                        mChannels.CT3DScan.Abort();
                    }
                    // Cancel background worker
                    backgroundWorker_MainRoutine.CancelAsync();
                    Thread.Sleep(5000);
                    mApplicationState = EApplicationState.Ready;
                    UpdateUI();
                }
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
                mApplicationState = EApplicationState.Error;
                DisplayLog("Error when clicking Stop button");
            }
        }





    }
}