using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Timers;
using Microsoft.FlightSimulator.SimConnect;
using Timer = System.Timers.Timer;

namespace WebMap
{
    public class ServerOnly
    {
        private static WebServer server = null;
        private static string port = "8001";
        private static SimConnect sim = new SimConnect();
        private static Timer timer = new Timer(1000);
        public static void Main()
        {
            timer.Elapsed += OnTimer;
            timer.Start();

            Thread.Sleep(-1);
        }

        private static void OnTimer(object sender, ElapsedEventArgs e)
        { 
timer.Stop();
            Debug.WriteLine("Connecting to Sim...");
            if (sim.IsConnected)
            {
                timer.Stop();
                Debug.WriteLine("Starting Web Service...");
                server = new WebServer(ReceiveCallback, "http://+:" + port + "/");
                server.Run();
                return;
            }
            sim.Connect();

        }
        private static string ReceiveCallback(HttpListenerRequest request) 
        {
            // System.Diagnostics.Debug.WriteLine("{0}: {1}", request.UserHostAddress, request.RawUrl);
            string rawRequest = request.RawUrl;
            string[] parse = rawRequest.Split("?".ToCharArray());
            string command = parse[0];
            string parmstr =  parse.Length > 1 ? parse[1] : null;
            
            
            switch (command)
            {
                case "/get?position":
                {   
                    Debug.WriteLine($"Request: {command} params: {parmstr}");
                    return string.Format("{{ \"type\": \"Point\", \"coordinates\": [{0}, {1}] }}", sim.userPos.Latitude, sim.userPos.Longitude);
                }
                case "/api/chartDataFPL":
                {
                    return "{\"edition\":2011,\"maxzoom\":21,\"validfrom\":\"2020-10-08 09:01:00\",\"rasterMaxFrames\":11,\"scale\":10,\"subtype\":\"Sectional\",\"lon\":-71.1036,\"rasterKey\":\"POdCKqsDBmp3FdJYbY7ZeHdV7Nk\",\"srs\":{\"x_0\":-20037508.3428,\"xr\":76.43702829,\"y_0\":20037508.3428,\"proj\":\"sm\",\"lat_0\":0,\"lat_1\":0,\"lon_0\":0,\"yr\":-76.43702829,\"lat_2\":0},\"height\":524288,\"expires\":18,\"name\":\"World VFR\",\"prefs\":{\"windZulu\":1602979200,\"windMB\":\"300\"},\"tileservers\":\"https://t.skyvector.com/V7pMh4zRihf1nr61,https://t.skyvector.com/V7pMh4zRihf1nr61\",\"userpref\":{},\"validto\":\"2020-11-05 09:01:00\",\"width\":524288,\"lat\":42.5335,\"currTime\":1602985399,\"alaska\":0,\"rasterMaxSize\":1200,\"protoid\":301,\"type\":\"vfr\"}";
                }
                default:
                {
                    string filename = "../../scripts/" + command.Replace("../","");
                    if (!File.Exists(filename))
                        return string.Empty;

                    return File.ReadAllText(filename);
                }
            }
        }
    }
}
