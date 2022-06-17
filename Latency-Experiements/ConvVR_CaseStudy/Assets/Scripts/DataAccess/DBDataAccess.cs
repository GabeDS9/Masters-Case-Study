using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModels;

namespace DataAccess
{
    public class DBDataAccess
    {
        private const string ConnectionString = "mongodb://127.0.0.1:27017";
        private const string EnergyCollection = "energy_data";
        private string DatabaseName = "Conv_Energy_Data";
        private IMongoCollection<T> ConnectToMongo<T>(in string collection)
        {
            var client = new MongoClient(ConnectionString);
            var db = client.GetDatabase(DatabaseName);
            return db.GetCollection<T>(collection);
        }
        public Task CreateEnergyReading(EnergyMeterModel energymeter)
        {
            var energyMeterCollection = ConnectToMongo<EnergyMeterModel>(EnergyCollection);
            return energyMeterCollection.InsertOneAsync(energymeter);
        }
        public async Task<List<EnergyMeterModel>> GetEnergyMeterReading(int meter_id, string timestamp)
        {
            var energymeterCollection = ConnectToMongo<EnergyMeterModel>(EnergyCollection);
            var results = await energymeterCollection.FindAsync(c => (c.Meter_ID == meter_id && c.TimestampDay == timestamp));
            return results.ToList();
        }
        public void DeleteDatabase(string dbName)
        {
            var client = new MongoClient(ConnectionString);
            Task.Run(() => client.DropDatabaseAsync(dbName));
        }
    }
}
