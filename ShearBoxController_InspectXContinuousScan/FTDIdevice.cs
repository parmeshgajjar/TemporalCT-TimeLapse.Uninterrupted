using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using FTD2XX_NET;

namespace ShearBoxController_InspectXContinuousScan
{



    class FTDIdevice
    {
        // Standard constructor
        public FTDIdevice()
        {
        }
        
        // overloaded Standard constructor that accepts form variable
        public FTDIdevice(ShearBoxController_InspectXContinuousScanForm aParentForm)
        {
            mParentForm = aParentForm;
        }

        /// <summary> Parent Form that it links to </summary>
        protected ShearBoxController_InspectXContinuousScanForm mParentForm;

        #region Device Constants
        // Device constants
        protected const byte PIN_TX = 0x01;  // Orange wire on FTDI cable
        protected const byte PIN_RX = 0x02;  // Yellow
        protected const byte PIN_RTS = 0x04; // Green
        protected const byte PIN_CTS = 0x08; // Brown
        protected const byte PIN_DTR = 0x10;
        protected const byte PIN_DSR = 0x20;
        protected const byte PIN_DCD = 0x40;
        protected const byte PIN_RI = 0x80;

        UInt32 ftdiDeviceCount = 0;

        #endregion Device Constants

        #region Variables
        // Status variable
        protected FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;
        // Buffer size for read buffers
        protected const uint bufferSize = 256;
        // Buffer
        protected byte[] buffer = new Byte[bufferSize];
        /// <summary> Status of the USB </summary>
        public enum EUSBStatus { Connected, Disconnected, Error}
        #endregion Variables

        // Create new instance of the FTDI device class
        public FTDI myFtdiDevice = new FTDI();

        /// <summary>
        /// Count number of connected devices and open by serial number
        /// </summary>
        public EUSBStatus OpenDevice()
        {
            // Determine the number of FTDI devices connected to the machine
            ftStatus = myFtdiDevice.GetNumberOfDevices(ref ftdiDeviceCount);
            // Check status
            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                mParentForm.DisplayLog("\tNumber of FTDI devices: " + ftdiDeviceCount.ToString());

            }
            else
            {
                // Wait for a key press
                mParentForm.DisplayLog("\tFailed to get number of devices (error " + ftStatus.ToString() + ")");
                return EUSBStatus.Error;
            }

            // If no devices available, return
            if (ftdiDeviceCount == 0)
            {
                // Wait for a key press
                mParentForm.DisplayLog("\tFailed to get number of devices (error " + ftStatus.ToString() + ")");
                return EUSBStatus.Error;
            }

            // Allocate storage for device info list
            FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];

            // Populate our device list
            ftStatus = myFtdiDevice.GetDeviceList(ftdiDeviceList);

            // Open first device in our list by serial number
            ftStatus = myFtdiDevice.OpenBySerialNumber(ftdiDeviceList[0].SerialNumber);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                mParentForm.DisplayLog("\tFailed to open device (error " + ftStatus.ToString() + ")");
                return EUSBStatus.Error;
            }

            mParentForm.DisplayLog("\tUSB Device : opened successfully");
            return EUSBStatus.Connected;
        }

        /// <summary>
        /// Reset any devices connected to machine
        /// </summary>
        public void ResetDevice()
        {
            // Reset port FTDI is connected to. 
            myFtdiDevice.ResetPort();
        }

        /// <summary>
        /// Set up device parameters
        /// </summary>
        public EUSBStatus SetUpDeviceParameters()
        {
            // Set up device data parameters       

            // Set Baud rate to 300
            ftStatus = myFtdiDevice.SetBaudRate(300);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                mParentForm.DisplayLog("\tFailed to set Baud rate (error " + ftStatus.ToString() + ")");
                return EUSBStatus.Error;
            }

            // Set data characteristics - Data bits, Stop bits, Parity
            ftStatus = myFtdiDevice.SetDataCharacteristics(FTDI.FT_DATA_BITS.FT_BITS_8, FTDI.FT_STOP_BITS.FT_STOP_BITS_1, FTDI.FT_PARITY.FT_PARITY_NONE);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                mParentForm.DisplayLog("\tFailed to set data characteristics (error " + ftStatus.ToString() + ")");
                return EUSBStatus.Error;
            }

            mParentForm.DisplayLog("\tUSB Device : device parameters set successfully");
            return EUSBStatus.Connected;
        }

        /// <summary>
        /// Single byte to be written to device
        /// </summary>
        /// <param name="c"> Single byte to be written to pin </param>
        public EUSBStatus Write(byte c)
        {
            UInt32 numBytesWritten = 0;

            byte[] cArray = new byte[1];

            cArray[0] = c;

            ftStatus = myFtdiDevice.Write(cArray, cArray.Length, ref numBytesWritten);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                mParentForm.DisplayLog("\tFailed to write to pin (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return EUSBStatus.Error;
            }

            mParentForm.DisplayLog("\tUSB Device : pin state equals " + c.ToString() + " written successfully");
            return EUSBStatus.Connected;

        }

        /// <summary>
        /// Close open FTDI device
        /// </summary>
        public EUSBStatus CloseDevice()
        {
            ftStatus = myFtdiDevice.Close();
            return EUSBStatus.Disconnected;
        }

        /// <summary>
        /// Pause communications
        /// </summary>
        public EUSBStatus PauseCommunications()
        {
            // Pause read communications
            ftStatus = myFtdiDevice.StopInTask();

            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                mParentForm.DisplayLog("\tFailed to pause communications (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return EUSBStatus.Error;
            }

            mParentForm.DisplayLog("\tUSB Device : communication paused successfully");
            return EUSBStatus.Connected;
        }

        /// <summary>
        /// Purge buffer
        /// </summary>
        public EUSBStatus PurgeBuffer()
        {
            // Purge (any) previous data in buffer.
            ftStatus = myFtdiDevice.Purge(FTDI.FT_PURGE.FT_PURGE_RX);

            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                mParentForm.DisplayLog("\tFailed to purge data in buffer (error " + ftStatus.ToString() + ")");
                return EUSBStatus.Error;
            }

            mParentForm.DisplayLog("\tUSB Device : data buffer purged successfully");
            return EUSBStatus.Connected;
        }
        
        /// <summary>
        /// Restart communications
        /// </summary>
        public EUSBStatus RestartCommunications()
        {
            // Restart (read) communications
            ftStatus = myFtdiDevice.RestartInTask();

            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                mParentForm.DisplayLog("\tFailed to restart read communications (error " + ftStatus.ToString() + ")");
                Console.ReadKey();
                return EUSBStatus.Error;
            }

            mParentForm.DisplayLog("\tUSB Device : communication restarted successfully");
            return EUSBStatus.Connected;

        }

        /// <summary>
        /// Read data from input buffer. 
        /// Will read minimum between amount in buffer and buffer size
        /// </summary>
        /// <param name="bytesInQueue"> Number of bytes waiting in read queue </param>
        /// <param name="bytesRecieved"> Number of bytes successfully read </param>
        /// <returns></returns>
        public EUSBStatus Read(uint bytesInQueue, ref uint bytesRecieved)
        {
            // Read as many bytes as we have and will fit in the buffer
            uint bytesToRead = Math.Min(bytesInQueue, bufferSize);

            // Read data
            ftStatus = myFtdiDevice.Read(buffer, bytesToRead, ref bytesRecieved);

            if (ftStatus != FTDI.FT_STATUS.FT_OK) // If error
            {
                // Wait for a key press
                mParentForm.DisplayLog("\tFailed to read data (error " + ftStatus.ToString() + ")");
                return EUSBStatus.Error;
            }

            //mParentForm.DisplayLog("\tUSB Device : read successfully");
            return EUSBStatus.Connected;
        }




    }
}
