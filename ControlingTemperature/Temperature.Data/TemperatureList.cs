using System;
using System.Globalization;

namespace Temperature.Data
{
    public record TemperatureList
    {

        public int Id { get; set; } // ID
        public DateTime DateTime { get; set; } // tid
        public string Place { get; set; } // Plats
        public double Temperature { get; set; } // temp
        public double Humidity { get; set; } // Fuktighet

        public static TemperatureList ParseFromCSV(string line)
        {
            var columns = line.Split(',');

            return new TemperatureList
            {
                DateTime = DateTime.Parse(columns[0]),  
                Place = columns[1],
                Temperature = double.Parse(columns[2], CultureInfo.InvariantCulture),
                Humidity = double.Parse(columns[3])

            };
        }
    }
}
