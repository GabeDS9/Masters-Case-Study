using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace DataAccess
{
    public class ReticulationDBDataAccess
    {
        private const string ConnectionString = "mongodb://127.0.0.1:27017";
        private const string ReticulationCollection = "building_data";
        private const string EnergyCollection = "energy_data";
        private const string OccupancyCollection = "occupancy_data";
        private const string SolarCollection = "solar_data";
        private string DatabaseName = "";

        public ReticulationDBDataAccess(string db_name)
        {
            DatabaseName = db_name;
        }

        /*private IMongoCollection<T> ConnectToMongo<T>(in string collection)
        {
            var client = new MongoClient(ConnectionString);
            var db = client.GetDatabase(DatabaseName);
            return db.GetCollection<T>(collection);
        }

        public Task CreateBuilding(BuildingModel building)
        {
            var buildingCollection = ConnectToMongo<BuildingModel>(BuildingCollection);
            return buildingCollection.InsertOneAsync(building);
        }

        public Task CreateEnergyMeter(EnergyMeterModel energymeter)
        {
            var energyMeterCollection = ConnectToMongo<EnergyMeterModel>(EnergyCollection);
            return energyMeterCollection.InsertOneAsync(energymeter);
        }

        public Task UpdateEnergyMeter(EnergyMeterModel energyMeter)
        {
            var energyCollection = ConnectToMongo<EnergyMeterModel>(EnergyCollection);
            var filter = Builders<EnergyMeterModel>.Filter.Eq("Id", energyMeter.Id);
            return energyCollection.ReplaceOneAsync(filter, energyMeter, new ReplaceOptions { IsUpsert = true });
        }

        public async Task<List<EnergyMeterModel>> GetEnergyMeterReading(int meter_id, string timestamp)
        {
            var energymeterCollection = ConnectToMongo<EnergyMeterModel>(EnergyCollection);
            var results = await energymeterCollection.FindAsync(c => (c.Meter_ID == meter_id && c.Timestamp == timestamp));
            return results.ToList();
        }*/
    }
}
