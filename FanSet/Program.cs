using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;

namespace FanSet
{
    internal class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
        }
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();

            Dictionary<string, int> pairs = new Dictionary<string, int>();

            int j;
            string k;
            for (int i = 0; i < args.Length-1; i+=2)
            {
                k = args[i].ToLower();

                if (int.TryParse(args[i + 1], out j))
                {
                    if (j >= 0 && j <= 100)
                    {
                        if (!pairs.ContainsKey(k))
                            pairs.Add(k, j);
                        else
                            pairs[k] = j;
                    }
                }
            }

            Computer computer = new Computer
            {
                IsCpuEnabled = false,
                IsGpuEnabled = false,
                IsMemoryEnabled = false,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsNetworkEnabled = false,
                IsStorageEnabled = false
            };

            computer.Open();
            computer.Accept(new UpdateVisitor());

            var sensors = GetAllControlSensors(computer);

            SetFanSpeeds(sensors, pairs);
            //computer.Close(); //-- resets the fans, bad
        }
        static ISensor[] GetAllControlSensors(Computer computer)
        {
            List<ISensor> sensorList = new List<ISensor>();
            foreach (var hardware in computer.Hardware)
            {
                foreach (var subhardware in hardware.SubHardware)
                {
                    foreach (var sensor in subhardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Control)
                        {
                            sensorList.Add(sensor);
                        }
                    }
                }
            }
            return sensorList.ToArray();
        }

        static void SetFanSpeeds(ISensor[] sensors, Dictionary<string, int> pairs)
        {
            int j;
            string k;
            foreach (var sensor in sensors)
            {
                if (pairs.Count == 0)
                {
                    Console.Write(sensor.Name + " (" + sensor.Identifier.ToString() + ") = ");
                    Console.Write(Math.Round(sensor.Value ?? 0));
                    Console.WriteLine('%');
                }
                else
                {
                    k = sensor.Identifier.ToString();

                    if (pairs.TryGetValue(k, out j))
                    {
                        sensor.Control.SetSoftware(j);
                        Console.Write(sensor.Name + " (" + sensor.Identifier.ToString() + ") Set ");
                        Console.Write(j);
                        Console.WriteLine("%");
                    }
                }
            }
        }
    }
}
