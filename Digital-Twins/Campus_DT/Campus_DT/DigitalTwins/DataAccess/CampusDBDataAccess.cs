using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using DataAccess.Models;

namespace DataAccess
{
    public class CampusDBDataAccess
    {
        private const string ConnectionString = "mongodb://127.0.0.1:27017";
        private const string CampusCollection = "campus_data";
        private const string PrecinctCollection = "precinct_data";
        private const string EnergyCollection = "energy_data";
        private const string SolarCollection = "solar_data";
        private string DatabaseName = "";

        public CampusDBDataAccess(string db_name)
        {
            DatabaseName = db_name;
        }

        private IMongoCollection<T> ConnectToMongo<T>(in string collection)
        {
            var client = new MongoClient(ConnectionString);
            var db = client.GetDatabase(DatabaseName);
            return db.GetCollection<T>(collection);
        }

        public Task CreateCampus(CampusModel campus)
        {
            var campusCollection = ConnectToMongo<CampusModel>(CampusCollection);
            return campusCollection.InsertOneAsync(campus);
        }

        public Task CreateEnergyReading(EnergyMeterModel energymeter)
        {
            var energyMeterCollection = ConnectToMongo<EnergyMeterModel>(EnergyCollection);
            return energyMeterCollection.InsertOneAsync(energymeter);
        }

        public async Task<List<EnergyMeterModel>> GetEnergyReading(string campus, string timestamp)
        {
            var energymeterCollection = ConnectToMongo<EnergyMeterModel>(EnergyCollection);
            var results = await energymeterCollection.FindAsync(c => (c.EnergyMeter_name == campus && c.Timestamp == timestamp));
            return results.ToList();
        }
        public Task UpdateEnergyMeter(EnergyMeterModel energyMeter)
        {
            var energyCollection = ConnectToMongo<EnergyMeterModel>(EnergyCollection);
            var filter = Builders<EnergyMeterModel>.Filter.Eq("Id", energyMeter.Id);
            return energyCollection.ReplaceOneAsync(filter, energyMeter, new ReplaceOptions { IsUpsert = true });
        }
        public async Task<List<EnergyMeterModel>> GetLatestEnergyReading()
        {
            var energymeterCollection = ConnectToMongo<EnergyMeterModel>(EnergyCollection);
            var results = await energymeterCollection.FindAsync(c => (c.EnergyMeter_name == "Current"));
            return results.ToList();
        }
        public async Task DeleteDatabase(string dbName)
        {
            var client = new MongoClient(ConnectionString);
            await client.DropDatabaseAsync(dbName);
        }
    }
}
