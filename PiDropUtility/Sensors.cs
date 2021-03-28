using System;
using System.Device.I2c;
using System.Threading.Tasks;
using Iot.Device.Bmxx80;
using Iot.Device.Common;
using Iot.Device.Si7021;
using static Crayon.Output;

namespace PiDropUtility
{
    public static class Sensors
    {
        /// <summary>
        ///     Support for temperature and pressure via a BMP280 sensor and I2C.
        /// </summary>
        /// <returns></returns>
        public static async Task<(double? temperatureFahrenheit, double? pressureMillibars)>
            GetBmp280TemperatureAndPressure()
        {
            // bus id on the raspberry pi 3 and 4
            const int busId = 1;
            // set this to the current sea level pressure in the area for correct altitude readings
            var defaultSeaLevelPressure = WeatherHelper.MeanSeaLevel;

            I2cConnectionSettings i2CSettings = new(busId, Bmx280Base.DefaultI2cAddress);
            var i2CDevice = I2cDevice.Create(i2CSettings);
            using var i2CBmp280 = new Bmp280(i2CDevice)
            {
                TemperatureSampling = Sampling.HighResolution,
                PressureSampling = Sampling.HighResolution
            };

            // Perform a synchronous measurement
            var readResult = await i2CBmp280.ReadAsync();

            var temperature = readResult.Temperature;
            Console.WriteLine($"Temperature: {Green($"{readResult.Temperature?.DegreesFahrenheit:0.#}\u00B0F")}");

            var pressure = readResult.Pressure;
            Console.WriteLine($"Pressure: {Green($"{readResult.Pressure?.Millibars:0.##}mb")}");

            if (temperature == null || pressure == null)
                return (readResult.Temperature?.DegreesFahrenheit, readResult.Pressure?.Millibars);

            var altitude =
                WeatherHelper.CalculateAltitude(pressure.Value, defaultSeaLevelPressure, temperature.Value);
            Console.WriteLine($"Calculated Altitude: {Green($"{altitude.Feet:0,000}")}");

            return (readResult.Temperature?.DegreesFahrenheit, readResult.Pressure?.Millibars);
        }

        public static (double temperatureFahrenheit, double relativeHumidityPercent) GetSiTemperatureAndHumidity()
        {
            // bus id on the raspberry pi 3 and 4
            const int busId = 1;
            // set this to the current sea level pressure in the area for correct altitude readings

            I2cConnectionSettings i2CSettings = new(busId, Si7021.DefaultI2cAddress);
            var i2CDevice = I2cDevice.Create(i2CSettings);
            using var i2CSi7021 = new Si7021(i2CDevice);

            var temperature = i2CSi7021.Temperature;
            Console.WriteLine($"Temperature: {Green($"{temperature.DegreesFahrenheit:0.#}\u00B0F")}");
            
            var humidity = i2CSi7021.Humidity;
            Console.WriteLine($"Humidity: {Green($"{humidity.Percent:0}%")}");

            return (temperature.DegreesFahrenheit, humidity.Percent);
        }
    }
}