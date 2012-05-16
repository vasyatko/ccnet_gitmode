using System;
using System.IO.Ports;
using System.IO;
using System.Windows.Forms;

namespace ThoughtWorks.CruiseControl.CCTrayLib.X10
{
    /// <summary>
    /// This class controls the CM11a X10 Controller.  It is responsible for all
    /// IO operations between the CM11a Hardware and the Module classes.  
    /// </summary>
    public partial class Cm11LowLevelDriver : IX10LowLevelDriver
    {
        [Flags]
        private enum CM11aHouseCode
        {
            A = 0x60,
            B = 0xE0,
            C = 0x20,
            D = 0xA0,
            E = 0x10,
            F = 0x90,
            G = 0x50,
            H = 0xD0,
            I = 0x70,
            J = 0xF0,
            K = 0x30,
            L = 0xB0,
            M = 0X00,
            N = 0x80,
            O = 0x40,
            P = 0xC0
        }

        [Flags]
        private enum CM11aDeviceCode
        {
            ONE = 0x06,
            TWO = 0x0E,
            THREE = 0x02,
            FOUR = 0x0A,
            FIVE = 0x01,
            SIX = 0x09,
            SEVEN = 0x05,
            EIGHT = 0x0D,
            NINE = 0x07,
            TEN = 0x0F,
            ELEVEN = 0x03,
            TWELVE = 0x0B,
            THIRTEEN = 0X00,
            FOURTEEN = 0x08,
            FIFTEEN = 0x04,
            SIXTEEN = 0x0C
        }

        [Flags]
        private enum Header
        {
            Address = 0x04,
            ExtendedTransmission = 0x05,
            Function = 0x06
        }

        private CM11aHouseCode x10HouseCode; //House code to control
        private Cm11LowLevelDriverWorker worker;
        private const ControllerType controller = ControllerType.CM11; //Controller Type
        private System.Threading.Thread workerThread;

        /// <summary>
        /// Constructor for the X10CM11aController object.
        /// </summary>
        /// <param name="houseCode">House Code to control</param>
        /// <param name="comPort">Comm port the CM11a is on</param>
        /// <param name="baudRate">BaudRate of the CM11a </param>
        /// <param name="parity">Parity for serial Communications</param>
        /// <param name="dataBits">Databits for serial Commnunication</param>
        /// <param name="stopBits">Stop bits for serial Communication</param>
        public Cm11LowLevelDriver(String houseCode, string comPort, int baudRate, Parity parity, int dataBits,
                                  StopBits stopBits)
        {
            x10HouseCode = (CM11aHouseCode)Enum.Parse(typeof(CM11aHouseCode), houseCode);
            worker = new Cm11LowLevelDriverWorker(x10HouseCode, comPort, baudRate, parity, dataBits, stopBits);
            worker.Error += new EventHandler<Cm11LowLevelDriverError>(worker_Error);

            try
            {
                workerThread = new System.Threading.Thread(worker.StartProcessing);
                workerThread.IsBackground = true;
                workerThread.Start();
            }
            catch (ApplicationException ae)
            {
                throw (new ApplicationException("Error Initializing CM11 X10 Controller", ae));
            }
        }

        ~Cm11LowLevelDriver()
        {
            worker.CloseDriver();
        }
        void worker_Error(object sender, Cm11LowLevelDriverError e)
        {
            // Used for tracing
            System.Diagnostics.Debug.WriteLine("Cm11LowLevelDriver: Error: " + e.Message);
        }

        /// <summary>
        /// Constructor with most of the defaults for the CM11a already filled in
        /// </summary>
        /// <param name="comPort">COM port the CM11a in on</param>
        /// <param name="houseCode">The House Code to controll</param>
        public Cm11LowLevelDriver(String houseCode, string comPort)
            : this(houseCode, comPort, 4800, Parity.None, 8, StopBits.One)
        {
        }

        /// <summary>
        /// Sends the X10 Command to the CM11a
        /// </summary>
        /// <param name="buffer">Command to send in an array of bytes</param>
        /// <param name="count">size of command array</param>
        private void Send(byte[] buffer, int count)
        {
            worker.AddMessage(new Cm11Message(buffer, count));
        }

        /// <summary>
        /// Determines the actual Device Code as it appears to the CM11a
        /// </summary>
        /// <param name="deviceCode">Device Code to operate on</param>
        /// <returns>CM11a representation of the device code</returns>
        private CM11aDeviceCode GetCM11aDeviceCode(int deviceCode)
        {
            switch (deviceCode)
            {
                case 1:
                    return (CM11aDeviceCode.ONE);
                case 2:
                    return (CM11aDeviceCode.TWO);
                case 3:
                    return (CM11aDeviceCode.THREE);
                case 4:
                    return (CM11aDeviceCode.FOUR);
                case 5:
                    return (CM11aDeviceCode.FIVE);
                case 6:
                    return (CM11aDeviceCode.SIX);
                case 7:
                    return (CM11aDeviceCode.SEVEN);
                case 8:
                    return (CM11aDeviceCode.EIGHT);
                case 9:
                    return (CM11aDeviceCode.NINE);
                case 10:
                    return (CM11aDeviceCode.TEN);
                case 11:
                    return (CM11aDeviceCode.ELEVEN);
                case 12:
                    return (CM11aDeviceCode.TWELVE);
                case 13:
                    return (CM11aDeviceCode.THIRTEEN);
                case 14:
                    return (CM11aDeviceCode.FOURTEEN);
                case 15:
                    return (CM11aDeviceCode.FIFTEEN);
                case 16:
                    return (CM11aDeviceCode.SIXTEEN);
            }
            throw (new ApplicationException("Device Code out of range"));
        }

        /// <summary>
        /// This is the interface for actually sending individual commands to a particular device
        /// </summary>
        /// <param name="deviceCode">Device code to be operated on</param>
        /// <param name="deviceCommand">Command to send to device</param>
        /// <param name="lightLevel">if Dimming or Brightening then this is a percentage to raise or lower
        /// light level.  If not Dimming or Brightening then this parameter is ignored </param>
        public void ControlDevice(int deviceCode, Function deviceCommand, int lightLevel)
        {
            byte[] buffer = new byte[2];

            if (Function.Bright == deviceCommand || Function.Dim == deviceCommand)
            {
                if (lightLevel > 0 && lightLevel <= 100)
                {
                    lightLevel = (int)(22.0 * (((double)lightLevel) / 100.0));
                    lightLevel = lightLevel << 3;
                }
                else
                {
                    throw (new ApplicationException("Invalid Light Level"));
                }
            }
            else
            {
                lightLevel = 0;
            }
            deviceCode = (int)GetCM11aDeviceCode(deviceCode);
            buffer[0] = (byte)Header.Address;
            buffer[1] = (byte)(((byte)x10HouseCode) | ((byte)deviceCode));
            Send(buffer, 2);
         
            buffer[0] = (byte)(((byte)lightLevel) | ((byte)Header.Function));
            buffer[1] = (byte)(((byte)x10HouseCode) | ((byte)deviceCommand));
            Send(buffer, 2);
        }

        public void ResetStatus(Label statusLabel)
        {
        }

        /// <summary>
        /// Tells driver to close down and release all locked up resouces.
        /// </summary>
        public void CloseDriver()
        {
            if (worker != null && workerThread!=null)
                if (workerThread.IsAlive)
                {
                    // Tell worker thread to stop processing message queue and release resources
                    worker.StopProcessing();

                    // Give thread 3 secs to clean up before killing it.
                    workerThread.Join(3000);
                    if (workerThread.IsAlive)
                        workerThread.Abort();

                    workerThread = null;
                }
        }

        /// <summary>
        /// Sends the Turn All Units Off code
        /// </summary>
        public void TurnAllUnitsOff()
        {
            byte[] buffer = new byte[2];

            buffer[0] = (byte)Header.Address;
            buffer[1] = (byte)(((byte)x10HouseCode) | ((byte)CM11aDeviceCode.ONE));
            Send(buffer, 2);
         
            buffer[0] = (byte)Header.Function;
            buffer[1] = (byte)(((byte)x10HouseCode) | ((byte)Function.AllUnitsOff));
            Send(buffer, 2);
        }

        /// <summary>
        /// Sends the Turn All Lamps On Signal
        /// </summary>
        public void TurnAllLampsOn()
        {
            byte[] buffer = new byte[2];

            buffer[0] = (byte)Header.Address;
            buffer[1] = (byte)(((byte)x10HouseCode) | ((byte)CM11aDeviceCode.ONE));
            Send(buffer, 2);

            buffer[0] = (byte)Header.Function;
            buffer[1] = (byte)(((byte)x10HouseCode) | ((byte)Function.AllLightsOn));
            Send(buffer, 2);

        }

        /// <summary>
        /// Turn All Lamps Off call the TurnAllUntisOff command.
        /// There is a seperate signal for turn all lamps off but does not seem to
        /// work on the CM17a or CM11a controllers
        /// </summary>
        public void TurnAllLampsOff()
        {
            TurnAllUnitsOff();
        }

        /// <summary>
        /// Returns the ControllerType of this object.
        /// </summary>
        public ControllerType Controller
        {
            get { return (controller); }
        }
       
    }
}
