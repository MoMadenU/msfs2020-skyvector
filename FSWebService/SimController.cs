using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using FSWebService;
using Microsoft.FlightSimulator.SimConnect;


namespace FSWebService {

    public class LatLon 
    {
        public double Latitude;
        public double Longitude;
    }

    public class SimController
    {
        private const int WM_USER_SIMCONNECT = 0x0402;
        private FSWebServiceForm form;

        private SimConnect sc = null;
        private System.Timers.Timer pollingTimer = new System.Timers.Timer(1000);

        public bool IsConnected;

        public LatLon userPos = new LatLon();
        
        enum DEFINITIONS
        {
            Struct1,
        }


        enum DATA_REQUESTS
        {
            REQUEST_1,
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        struct Struct1
        {
            public double latitude;
            public double longitude;
        };

        public SimController(FSWebServiceForm _form)
        {
            form = _form;
        }

        public void ProcessReceive()
        {
            sc?.ReceiveMessage();
        }
     
        public void Connect() 
        {
            if (sc == null) 
            {
                try {
                    sc = new SimConnect("FSWebService Controller", form.Handle, WM_USER_SIMCONNECT, null, 0);
                    sc.OnRecvOpen += OnRecvOpen;
                    sc.OnRecvException += OnRecvException;
                    sc.OnRecvQuit += OnRecvQuit;
                    
                    sc.AddToDataDefinition(DEFINITIONS.Struct1, "Plane Latitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    sc.AddToDataDefinition(DEFINITIONS.Struct1, "Plane Longitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    
                    sc.RegisterDataDefineStruct<Struct1>(DEFINITIONS.Struct1);
                    
                    sc.OnRecvSimobjectDataBytype += OnRecvSimobjectDataByType;

                    IsConnected = true;
                }

                catch (Exception ex) {

                    Console.WriteLine("Unable to connect to Sim");
                   // form.AddText("Unable to connect to Sim");
                    return;
                }

                pollingTimer.Elapsed += (sender, e) =>
                {
                    sc.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_1, DEFINITIONS.Struct1, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
                };
                pollingTimer.Start();
            }
        }

        public void Disconnect()
        {
            IsConnected = false;
            pollingTimer.Stop();
            if (sc != null)
            {
                sc.Dispose();
                sc = null;
            }

            //        sender.TryDisableWebServer();
        }

        private void OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Console.WriteLine("OnRecvOpen");
            form.AddText("Sim Running\n");
        }

        private void OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data) 
        {
            Console.WriteLine("OnRecvException");
        }

        private void OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data) 
        {
            Disconnect();
        }

        private void OnRecvSimobjectDataByType(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            switch ((DATA_REQUESTS)data.dwRequestID)
            {
                case DATA_REQUESTS.REQUEST_1:
                    Struct1 s1 = (Struct1)data.dwData[0];
                    userPos.Latitude = s1.latitude;
                    userPos.Longitude = s1.longitude;
                 //   Console.WriteLine("lat: " + userPos.Latitude + " lon: " +userPos.Longitude);
                    break;

                default:
                    Console.WriteLine("Unknown request ID: " + data.dwRequestID);
                    break;
            }
        }
    }
}
