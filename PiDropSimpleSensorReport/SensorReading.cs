using System;

namespace PiDropSimpleSensorReport
{
    public class SensorReading
    {
        public int Id { get; set; }
        public DateTime ReadingDateTime { get; set; }
        public string ReadingTag { get; set; }
        public double ReadingValue { get; set; }
    }
}