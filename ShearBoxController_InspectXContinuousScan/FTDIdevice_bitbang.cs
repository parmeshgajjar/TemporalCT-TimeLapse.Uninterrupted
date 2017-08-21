using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using FTD2XX_NET;

namespace ShearBoxController_InspectXContinuousScan
{

    // Delegate for Rising Edge Signal Event Handler
    public delegate void RisingEdgeSignalEventHandler(object source, EventArgs e);
    // Delegate for Fallign Edge Signal Event Handler
    public delegate void FallingEdgeSignalEventHandler(object source, EventArgs e);

    class FTDIdevice_bitbang : FTDIdevice
    {
        #region Constructors

        // Normal constructor
        public FTDIdevice_bitbang()
        {

        }

        // Overloaded constructor
        public FTDIdevice_bitbang(ShearBoxController_InspectXContinuousScanForm aParentForm)
        {
            mParentForm = aParentForm;
        }

        #endregion Constructors

        #region Variables


        // Last button state
        public bool lastButtonState = false;
        // Current button state
        public bool buttonState;

        #endregion Variables

        #region Events

        // Event declarations
        public event RisingEdgeSignalEventHandler RisingEdgeSignal;
        public event FallingEdgeSignalEventHandler FallingEdgeSignal;

        // Event virtual methods
        public virtual void OnRisingEdgeSignal(EventArgs e)
        {
            if (RisingEdgeSignal != null) // Check if subscribers to event, else don't fire
                RisingEdgeSignal(this, e);
        }
        public virtual void OnFallingEdgeSignal(EventArgs e)
        {
            if (FallingEdgeSignal != null) // Check if subscribers to event, else don't fire
                FallingEdgeSignal(this, e);
        }

        #endregion Events


        /// <summary>
        /// Enable bit bang mode on TX lines
        /// </summary>
        public EUSBStatus EnableBitBang()
        {
            // Enable bit bang mode
            ftStatus = myFtdiDevice.SetBitMode(PIN_TX, FTDI.FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                mParentForm.DisplayLog("\tFailed to set Baud rate (error " + ftStatus.ToString() + ")");
                return EUSBStatus.Error;
            }

            mParentForm.DisplayLog("\tUSB Device : bit-bang enabled successfully");
            return EUSBStatus.Connected;
        }

        /// <summary>
        /// Convert buffer to binary signal by comparing with Read Pin
        /// </summary>
        /// <param name="bytesRecieved"> Number of bytes recieved (number of bytes to process) </param>
        public void ConvertToBinarySignal(uint bytesRecieved)
        {
            // Loop through bytes read and detect any changes in pin state
            for (uint k = 0; k < bytesRecieved; k++)
            {
                buttonState = PIN_RX == (buffer[k] & PIN_RX);
                
                if (buttonState != lastButtonState) // if button state has changed
                {
                    lastButtonState = buttonState;
                    if (buttonState) // on up signal
                    {
                        OnRisingEdgeSignal(new EventArgs());
                        if (RisingEdgeSignal != null) // If subscribed to rising edge then exit
                            return;
                    }
                    else
                    {
                        OnFallingEdgeSignal(new EventArgs());
                        if (FallingEdgeSignal != null) // If subscribed to falling edge then exit
                            return;
                    }
                }
                lastButtonState = buttonState;
            }
        }

        /// <summary>
        /// Process input stream on Rx pin of USB device
        /// </summary>
        public void UARTProcessRxSignal()
        {
            // Number of bytes waiting to be read in buffer
            uint bytesInQueue = 0;
            // Number of bytes recieved
            uint bytesRecieved = 0;
            // Microsleep time
            int SleepUSec = 20; // Read data every 20ms
            // Reset button state
            buttonState = false;
            do
            {
                // Check number of bytes waiting to be read
                myFtdiDevice.GetRxBytesAvailable(ref bytesInQueue);

                // Loop while there is data to process
                while (bytesInQueue > 0)
                {
                    // Read into buffer
                    Read(bytesInQueue, ref bytesRecieved);

                    // Convert signal to binary
                    ConvertToBinarySignal(bytesRecieved);

                    // Update the number of bytes waiting to be read
                    myFtdiDevice.GetRxBytesAvailable(ref bytesInQueue);
                }
                Thread.Sleep(SleepUSec); // Sleep if need be
            }
            while (!lastButtonState);
        }

    }
}
