using Microsoft.EntityFrameworkCore;

namespace PiDropSimpleSensorReport
{
    public class SensorReadingContext : DbContext
    {
        private readonly string _dbName;

        public SensorReadingContext(string dbName)
        {
            _dbName = dbName;
        }

        public DbSet<SensorReading> SensorReadings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={_dbName}");
        }
    }
}