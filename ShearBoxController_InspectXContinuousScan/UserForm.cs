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

namespace ShearBoxController_InspectXContinuousScan
{
    public partial class ShearBoxController_FlyScanForm : Form
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
        private FTDIdevice.EUSBStatus mUSBConnectionState = FTDIdevice.EUSBStatus.Disconnected;

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
        /// Number of Shear Cycles to perform
        /// </summary>
        private Int32 mNumberOfShearCycles = 0;

        /// <summary>
        /// Number of shear cycles completed?
        /// </summary>
        private Int32 mShearCyclesCompleted = 0;

        /// <summary>
        /// Manually rotate manipulator back to zero?
        /// </summary>
        private bool mManualReturnManipulatorToZero = false;

        /// <summary>
        /// Angles to return to zero through
        /// </summary>
        private float[] ReturnAngles = new float[] { 270.0F, 180.0F, 90.0F, 0.0F };

        #endregion Project settings

        #region USB and shearbox variables

        /// <summary> FTDI Device for shearbox </summary>
        private FTDIdevice_bitbang mFtdiDevice = null;

        #endregion USB and shearbox variables

        #region Indicator Flags

        // Shading Correction Flag
        private bool mShadingCorrectionCompleted = false;

        // Scan Flag
        private bool mCTScanCompleted = false;

        // Stop process?
        private bool mStop = false;

        // Scan aborted
        private bool mScanAbortedSuccessfully = false;

        // Manipulator Go started
        private bool mManipulatorGoStarted = false;

        // Manipulator Go completed
        private bool mManipulatorGoCompleted = false;

        // Manipulator Homing started
        private bool mManipulatorHomingStarted = false;

        // Manipulator Homing completed
        private bool mManipulatorHomingCompleted = false;

        #endregion Indicator Flags


        /// <summary> Output log text that includes time and date stamps </summary>
        public string mOutputLogText = null;

        #endregion Application Variables

        public ShearBoxController_FlyScanForm()
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
                            mManipulatorHomingStarted = true;
                            break;
                        case IpcContract.Manipulator.EMoveEvent.HomingCompleted:
                            mManipulatorHomingCompleted = true;
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
                            mManipulatorGoCompleted = true;
                            break;
                        case IpcContract.Manipulator.EMoveEvent.GoStarted:
                            mManipulatorGoStarted = true;
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
                        //Debug.Print("e.Status="+e.Status.ToString());
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
                //Debug.Print("status.Error=" + status.Error.ToString());
                switch (status.Error)
                {
                    case Inspect_X_Definitions.CTStatusAcquisitionCompleted.CTAcquisitionError.Unknown:
                        // Your code goes here
                        break;
                    case Inspect_X_Definitions.CTStatusAcquisitionCompleted.CTAcquisitionError.NoError:
                        DisplayLog("Acquisition complete");
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
                        mScanAbortedSuccessfully = true;
                        mInspectXState = EInspectXState.Idle;
                        UpdateUI();
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
                    DisplayLog("Reconstructing Volume");
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
                        DisplayLog("Reconstruction complete");
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
            CheckConnectionsSettings();

            // Layout Inspect-X Connect and Disconnect buttons
            LayoutInspectXConnectDisconnectButtons();
            // Format Inspect-X Connection status
            FormatInspectXConnectionStatus();
            // Layout USB Connect and Disconnect buttons
            LayoutUSBConnectDisconnectButtons();
            // Format USB Connection status
            FormatUSBConnectionStatus();
            // Format Application status
            FormatApplicationStatus();
            // Format Current Running State
            FormatCurrentRunningState();
            // Format Current Inspect-X State
            FormatInspectXStatus();
            // Format Current Shearbox State
            FormatShearboxStatus();

        }

        /// <summary>
        /// Layout Connect and Disconnect buttons
        /// </summary>
        private void LayoutInspectXConnectDisconnectButtons()
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
        private bool InspectXConnectionOK()
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
                DisplayLog("Error in Inspect-X connection");
            }
            return false;
        }

        /// <summary>
        /// Layout Connect and Disconnect buttons
        /// </summary>
        private void LayoutUSBConnectDisconnectButtons()
        {
            switch (mUSBConnectionState)
            {
                case FTDIdevice.EUSBStatus.Connected:
                    btn_USBConnect.Enabled = false;
                    btn_USBDisconnect.Enabled = true;
                    break;
                case FTDIdevice.EUSBStatus.Disconnected:
                    btn_USBConnect.Enabled = true;
                    btn_USBDisconnect.Enabled = false;
                    break;
                case FTDIdevice.EUSBStatus.Error:
                    btn_USBConnect.Enabled = false;
                    btn_USBDisconnect.Enabled = false;
                    break;
            }
        }

        /// <summary>
        /// Function to check connection and project settings
        /// </summary>
        private void CheckConnectionsSettings()
        {
            if (InspectXConnectionOK() && USBConnectionOK())
                ProjectSettingsOK();
            else
                mApplicationState = EApplicationState.NoConnection;
        }

        /// <summary>
        /// Function checking whether Connection is OK
        /// </summary>
        /// <returns>True if connections are ok; false otherwise</returns>
        private bool USBConnectionOK()
        {
            try
            {
                if (mUSBConnectionState == FTDIdevice.EUSBStatus.Connected)
                    return true;
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
                mApplicationState = EApplicationState.Error;
                DisplayLog("Error in USB connection");
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
                // set manually return tick box to default value
                checkBox_ManualReturnManipulatorToZero.Checked = mManualReturnManipulatorToZero;
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

        private void CheckIfStopped()
        {
            if (mStop)
            {
                btn_Start.Enabled = false;
                btn_Stop.Enabled = false;
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

        #region Panel - USB Connection

        // see region Shearbox

        #endregion Panel - USB Connection

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

        /// <summary>
        /// Function on UI for setting tick box for manually return manipulator to zero
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_ManualReturnManipulatorToZero_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                mManualReturnManipulatorToZero = checkBox_ManualReturnManipulatorToZero.Checked;
                UpdateUI();
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
                mApplicationState = EApplicationState.Error;
                DisplayLog("Error in setting manual return to zero");
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
                // Set textbox to profile
                textBox_Profile.Text = mCTProfile;
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
            int previousindex = mCTProfileIndex;
            string previousprofile = mCTProfile;
            mCTProfileList = mChannels.CT3DScan.ProfileList();
            comboBoxProfile.DataSource = mCTProfileList;
            DisplayLog("Profile list updated");
            textBox_Profile.Text = previousprofile;
            if (previousindex == -1)
                mCTProfile = previousprofile;
            comboBoxProfile.SelectedIndex = previousindex;
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
                // Set textbox
                textBox_Profile.Text = mCTProfile;
                // Notify that profile loaded
                DisplayLog(@"Profile '" + mCTProfile + @"' loaded");
            }
            if (mCTProfileIndex == -1)
            {
                // Set Application state
                mApplicationState = EApplicationState.Ready;
                // Set profile
                mCTProfile = textBox_Profile.Text;
                // Notify that profile loaded
                DisplayLog(@"Profile '" + mCTProfile + @"' loaded");
            }
        }

        /// <summary>
        /// Browse for profile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_BrowseProfile_Click(object sender, EventArgs e)
        {
            // Open dialog
            DialogResult openCTprofiledialog = openFileDialogCTProfile.ShowDialog();
            // If dialog successful then change profile accordingly
            if (openCTprofiledialog == System.Windows.Forms.DialogResult.OK)
            {
                textBox_Profile.Text = openFileDialogCTProfile.FileName;
                comboBoxProfile.SelectedIndex = -1;
                mCTProfileIndex = -1;
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
                    panel_ProjectSettings.Enabled = true;
                    break;
                case Channels.EConnectionState.NoServer:
                    box_InspectXConnection.BackColor = Color.Gray;
                    box_InspectXConnection.Text = "No Server";
                    panel_CTProfile.Enabled = false;
                    panel_ProjectSettings.Enabled = false;
                    btn_Start.Enabled = false;
                    break;
                case Channels.EConnectionState.Error:
                    box_InspectXConnection.BackColor = Color.Red;
                    box_InspectXConnection.Text = "Connection Error";
                    panel_CTProfile.Enabled = false;
                    panel_ProjectSettings.Enabled = false;
                    btn_Start.Enabled = false;
                    break;
            }
        }

        /// <summary>
        /// Format Connection status
        /// </summary>
        private void FormatUSBConnectionStatus()
        {
            switch (mUSBConnectionState)
            {
                case FTDIdevice.EUSBStatus.Connected:
                    box_USBConnection.BackColor = Color.Green;
                    box_USBConnection.Text = "Connected";
                    panel_CTProfile.Enabled = true;
                    panel_ProjectSettings.Enabled = true;
                    break;
                case FTDIdevice.EUSBStatus.Disconnected:
                    box_USBConnection.BackColor = Color.Gray;
                    box_USBConnection.Text = "Disconnected";
                    panel_CTProfile.Enabled = false;
                    panel_ProjectSettings.Enabled = false;
                    btn_Start.Enabled = false;
                    break;
                case FTDIdevice.EUSBStatus.Error:
                    box_USBConnection.BackColor = Color.Red;
                    box_USBConnection.Text = "Connection Error";
                    panel_CTProfile.Enabled = false;
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
                    panel_USBConnection.Enabled = true;
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
                    panel_USBConnection.Enabled = true;
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
                    panel_USBConnection.Enabled = true;
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
                    panel_USBConnection.Enabled = true;
                    panel_ProjectSettings.Enabled = false;
                    btn_Start.Enabled = false;
                    btn_Start.Visible = true;
                    btn_Stop.Enabled = false;
                    btn_Stop.Visible = false;
                    break;
            }

            // Check if stop initiated
            CheckIfStopped();
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
                        box_CurrentRunningState.BackColor = SystemColors.Control;
                        box_CurrentRunningState.Text = "";
                        break;
                }
            }
            else
            {
                box_CurrentRunningState.BackColor = SystemColors.Control;
                box_CurrentRunningState.Text = "";
            }
        }

        /// <summary>
        /// Format Shearbox Status
        /// </summary>
        private void FormatShearboxStatus()
        {
            if (mApplicationState == EApplicationState.Running)
            {
                switch (mShearBoxState)
                {
                    case EShearBoxState.Idle:
                        box_CurrentShearBoxState.BackColor = Color.Green;
                        box_CurrentShearBoxState.Text = "Idle";
                        break;
                    case EShearBoxState.InMotion:
                        box_CurrentShearBoxState.BackColor = Color.Blue;
                        box_CurrentShearBoxState.Text = "In motion";
                        break;
                    case EShearBoxState.Error:
                        box_CurrentShearBoxState.BackColor = Color.Red;
                        box_CurrentShearBoxState.Text = "Error";
                        break;
                    default:
                        box_CurrentShearBoxState.BackColor = SystemColors.Control;
                        box_CurrentShearBoxState.Text = "";
                        break;
                }
            }
            else
            {
                box_CurrentShearBoxState.BackColor = SystemColors.Control;
                box_CurrentShearBoxState.Text = "";
            }
        }

        #endregion Panel - System Status

        #region Start-Stop buttons

        /// <summary>
        /// Function run when start button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Start_Click(object sender, EventArgs e)
        {
            mApplicationState = EApplicationState.Running;
            UpdateUI();
            backgroundWorker_MainRoutine.RunWorkerAsync();
        }

        /// <summary>
        /// Function run when stop button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Stop_Click(object sender, EventArgs e)
        {
            try
            {
                mStop = true;
                backgroundWorker_Abort.RunWorkerAsync();
                UpdateUI();
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
                mApplicationState = EApplicationState.Error;
                DisplayLog("Error when clicking Stop button");
            }
        }

        #endregion Start-Stop buttons

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

                // Log message
                DisplayLog("Written " + outputfilename);

                // Write to file
                logfile.WriteLine(DateTime.Now.ToString("dd/MM/yyyy H:mm:ss.fff") + " : Written " + outputfilename);

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

                // Log message
                DisplayLog("Written " + outputfilename);

                // Write to file
                logfile.WriteLine(DateTime.Now.ToString("dd/MM/yyyy H:mm:ss.fff") + " : Written " + outputfilename);

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

                // Log message
                DisplayLog("Written " + outputfilename);

                // Write to file
                outputfile.WriteLine(DateTime.Now.ToString("dd/MM/yyyy H:mm:ss.fff") + " : Written " + outputfilename);

                //Close file
                outputfile.Close();

            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
            }
        }

        #endregion Output

        #region Manipulator Functions

        /// <summary>
        /// Function to manually return manipulator to zero
        /// </summary>
        private void ReturnManipulatorToZero()
        {
            DisplayLog("Returning manipulator to zero...");
            // Loop through each value in the list
            for (int i = 0; i < ReturnAngles.Length; i++)
            {
                // Set flags to zero
                mManipulatorGoStarted = false;
                mManipulatorGoCompleted = false;

                // Set target
                mChannels.Manipulator.Axis.Target(IpcContract.Manipulator.EAxisName.Rotate, ReturnAngles[i]);
                // Start moving
                mChannels.Manipulator.Axis.Go(IpcContract.Manipulator.EAxisName.Rotate);
                // Wait until stopped moving
                while (!mManipulatorGoStarted || !mManipulatorGoCompleted)
                    Thread.Sleep(100);
            }
            DisplayLog("Manipulator returned to zero...");
        }

        #endregion Manipulator Functions

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
                    DisplayLog("Acquiring projections");
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

            // If needed, return manipulator manually to zero
            if (mManualReturnManipulatorToZero)
                ReturnManipulatorToZero();

            // Final status messages
            if (!backgroundWorker_MainRoutine.CancellationPending)
                DisplayLog("Scan '" + ScanName + "' completed successfully");
            else
                DisplayLog("Scan '" + ScanName + "' terminated by user");

            // Update State
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

            // Create Directory 
            CreateDirectory();

            // Reset number of shears completed
            mShearCyclesCompleted = 0;

            // Check homing
            if (!mChannels.Manipulator.Axis.Homed(IpcContract.Manipulator.EAxisName.All))
            {
                // Indicator flags
                mManipulatorHomingStarted = false;
                mManipulatorHomingCompleted = false;
                DisplayLog("Homing Axis");
                // Home all axes
                mChannels.Manipulator.Axis.Home(IpcContract.Manipulator.EAxisName.All, true, false);
                // Wait until finished
                while (!mManipulatorHomingStarted || !mManipulatorHomingCompleted)
                    Thread.Sleep(100);
            }

            // Run Circular XCT Scan
            if (!backgroundWorker_MainRoutine.CancellationPending)
                XCTScan(mProjectName + "_" + mShearCyclesCompleted.ToString("000"));

            if (mNumberOfShearCycles > 0 && !backgroundWorker_MainRoutine.CancellationPending)
            {

                // Set number of shears to zero
                mShearCyclesCompleted = 0;

                // Update shearbox state
                mShearBoxState = EShearBoxState.InMotion;
                mRunningState = ERunningState.ShearBox;
                UpdateUI();
                // Send signal to shear box
                SendSignal(mFtdiDevice);

                // Clear buffer
                mUSBConnectionState = mFtdiDevice.PurgeBuffer();
                UpdateUI();
                // Restart communications
                mUSBConnectionState = mFtdiDevice.RestartCommunications();
                UpdateUI();
                // Last button state reset
                mFtdiDevice.lastButtonState = false;

                // Cycle and wait for signal. 
                while (mShearCyclesCompleted < mNumberOfShearCycles
                    && mUSBConnectionState == FTDIdevice.EUSBStatus.Connected
                    && !backgroundWorker_MainRoutine.CancellationPending)
                {
                    DisplayLog("Waiting for signal");
                    mFtdiDevice.UARTProcessRxSignal();
                }
            }

            // Check if cancelled by user
            if (backgroundWorker_MainRoutine.CancellationPending)
                DisplayLog("Program aborted by user");
            else
                DisplayLog(mShearCyclesCompleted.ToString() + " shear cycles completed successfully");

            // Create complete log file
            WriteOutputFile(@"output.log", mOutputLogText);

            // Clear mOutputLogText
            mOutputLogText = "";

            // Update running state
            mRunningState = ERunningState.Idle;

            //Reset UI
            UpdateUI();

        }

        /// <summary>
        /// Abort routine
        /// </summary>
        private void AbortRoutine()
        {
            DisplayLog("User initiated cancellation of process");

            // If application is running then try to cancel
            if (mApplicationState == EApplicationState.Running)
            {
                // Cancel background worker
                backgroundWorker_MainRoutine.CancelAsync();
                // Cancel individual processes
                if (mInspectXState == EInspectXState.AcquireShadingCorrection)
                {
                    mChannels.CT3DScan.AbortShadingCorrectionUpdate();
                }
                else if (mInspectXState == EInspectXState.CTScanAcquiring)
                {
                    // set flag
                    mScanAbortedSuccessfully = false;
                    // abort scan
                    mChannels.CT3DScan.Abort();
                    // Wait until scan aborts
                    while (!mScanAbortedSuccessfully)
                        Thread.Sleep(100);
                }
                // Wait for things to settle down
                Thread.Sleep(5000);
               
            }
        }

        #region Background workers

        /// <summary>
        /// Do Work Background worker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker_MainRoutine_DoWork(object sender, DoWorkEventArgs e)
        {
            // Run Main Routine
            MainRoutine();
            Thread.Sleep(5000);
        }

        /// <summary>
        /// Run Completed Background Worker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker_MainRoutine_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mApplicationState = EApplicationState.Ready;
            UpdateUI();
        }

        #endregion Background workers

        #endregion Main Routine

        #region ShearBox

        #region UI functions


        /// <summary>
        /// Disconnect USB device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_USBDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (mUSBConnectionState == FTDIdevice.EUSBStatus.Connected)
                    mUSBConnectionState = mFtdiDevice.CloseDevice();
                UpdateUI();
            }
            catch (Exception ex) { AppLog.LogException(ex); }
        }

        /// <summary>
        /// Connect USB device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_USBConnect_Click(object sender, EventArgs e)
        {
            try
            {
                InitialiseUSB();
                UpdateUI();
            }
            catch (Exception ex) { AppLog.LogException(ex); }
        }


        #endregion UI functions

        #region Special USB functions
        /// <summary>
        /// String of functions that initialises the USB
        /// </summary>
        private void InitialiseUSB()
        {
            DisplayLog("Connecting USB device...");

            // FTDI device in bitbang mode
            mFtdiDevice = new FTDIdevice_bitbang(this);

            // Subscribe to appropriate events
            mFtdiDevice.RisingEdgeSignal += new RisingEdgeSignalEventHandler(RisingSignalRecieved);

            // Open device
            mUSBConnectionState = mFtdiDevice.OpenDevice();
            UpdateUI();

            // Enable bit bang
            mUSBConnectionState = mFtdiDevice.EnableBitBang();
            UpdateUI();

            // Set characteristics
            mUSBConnectionState = mFtdiDevice.SetUpDeviceParameters();
            UpdateUI();

            // Pause read communications
            mUSBConnectionState = mFtdiDevice.PauseCommunications();
            UpdateUI();

            // Purge communications
            mUSBConnectionState = mFtdiDevice.PurgeBuffer();
            UpdateUI();

            // Check that connection was successful
            if (mUSBConnectionState == FTDIdevice.EUSBStatus.Connected)
                DisplayLog("USB Device successfully connected");
        }

        /// <summary>
        /// Function called when a rising signal is recieved
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void RisingSignalRecieved(object source, EventArgs e)
        {
            // Cast the FTDI from the source
            FTDIdevice ftdiDevice = (FTDIdevice)source;
            // Pause communications
            mUSBConnectionState = ftdiDevice.PauseCommunications();
            UpdateUI();
            // Clear the buffer
            mUSBConnectionState = ftdiDevice.PurgeBuffer();
            UpdateUI();

            DisplayLog("Signal Recieved");
            // Update Shearbox state
            mShearBoxState = EShearBoxState.Idle;
            mRunningState = ERunningState.Idle;
            UpdateUI();

            // When signal recieved that shear completed then increase counter
            ++mShearCyclesCompleted;

            // Run Circular XCT Scan (after checking that program has not been cancelled)
            if (!backgroundWorker_MainRoutine.CancellationPending)
                XCTScan(mProjectName + "_" + mShearCyclesCompleted.ToString("000"));

            if (mShearCyclesCompleted < mNumberOfShearCycles && !backgroundWorker_MainRoutine.CancellationPending)
            {

                // Update Shearbox state
                mRunningState = ERunningState.ShearBox;
                mShearBoxState = EShearBoxState.InMotion;
                UpdateUI();
                // Send signal to shear box
                SendSignal(ftdiDevice);

                // Restart communications
                mUSBConnectionState = ftdiDevice.RestartCommunications();
                UpdateUI();
            }
        }

        /// <summary>
        /// Send a signal to the USB device
        /// </summary>
        /// <param name="ftdiDevice"> FTDI device object </param>
        private void SendSignal(FTDIdevice ftdiDevice)
        {
            DisplayLog("Sending Signal");

            // high signal
            mUSBConnectionState = ftdiDevice.Write(0x01);
            UpdateUI();

            // wait
            Thread.Sleep(500);

            // low signal
            mUSBConnectionState = ftdiDevice.Write(0x00);
            UpdateUI();

            // wait (3 seconds)
            Thread.Sleep(3000);

            DisplayLog("Signal Sent. Waiting for response from shearbox...");
        }
        #endregion Special USB functions

        /// <summary>
        /// Additional calls for form closing to ensure that USB is closed properly. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (mUSBConnectionState == FTDIdevice.EUSBStatus.Connected)
                    mFtdiDevice.CloseDevice();
                UpdateUI();
            }
            catch (Exception ex) { AppLog.LogException(ex); }
        }

        #endregion ShearBox

        private void backgroundWorker_Abort_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                AbortRoutine();
            }
            catch (Exception ex)
            {
                AppLog.LogException(ex);
                mApplicationState = EApplicationState.Error;
                DisplayLog("Error aborting");
            }
        }

        private void backgroundWorker_Abort_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mApplicationState = EApplicationState.Ready;
            mStop = false;
            UpdateUI();
        }














    }
}