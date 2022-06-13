using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using DataAccess.Models;

namespace DataAccess
{
    public class BuildingDBDataAccess
    {
        private const string ConnectionString = "mongodb://127.0.0.1:27017";
        private const string BuildingCollection = "building_data";
        private const string EnergyCollection = "energy_data";
        private const string OccupancyCollection = "occupancy_data";
        private const string SolarCollection = "solar_data";
        private string DatabaseName = "";
        
        public BuildingDBDataAccess(string db_name)
        {
            DatabaseName = db_name;
        }

        private IMongoCollection<T> ConnectToMongo<T>(in string collection)
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
        public async Task<List<EnergyMeterModel>> GetBuildingEnergyReading(string building, string timestamp, string type)
        {
            var energymeterCollection = ConnectToMongo<EnergyMeterModel>(EnergyCollection);
            if (type == null)
            {
                var results = await energymeterCollection.FindAsync(c => (c.EnergyMeter_name == building && c.Timestamp == timestamp && (c.Type != "Day Max" || c.Type != "Month Max" || c.Type != "Year Max")));
                return results.ToList();
            }
            else
            {
                var results = await energymeterCollection.FindAsync(c => (c.EnergyMeter_name == building && c.Timestamp == timestamp && c.Type == type));
                return results.ToList();
            }
        }
        public async Task<List<EnergyMeterModel>> GetEnergyMeterReading(int meter_id, string timestamp, string type)
        {
            var energymeterCollection = ConnectToMongo<EnergyMeterModel>(EnergyCollection);
            if (type == null)
            {
                var results = await energymeterCollection.FindAsync(c => (c.Meter_ID == meter_id && c.Timestamp == timestamp && (c.Type != "Day Max" || c.Type != "Month Max" || c.Type != "Year Max")));
                return results.ToList();
            }
            else
            {
                var results = await energymeterCollection.FindAsync(c => (c.Meter_ID == meter_id && c.Timestamp == timestamp && c.Type == type));
                return results.ToList();
            }
           
        }
        public async Task<List<EnergyMeterModel>> GetLatestEnergyReading()
        {
            var energymeterCollection = ConnectToMongo<EnergyMeterModel>(EnergyCollection);
            var results = await energymeterCollection.FindAsync(c => (c.Type == "Current"));
            return results.ToList();
        }
        public async Task DeleteDatabase(string dbName)
        {
            var client = new MongoClient(ConnectionString);
            await client.DropDatabaseAsync(dbName);
        }
        /*public async Task<List<ChoreModel>> GetAllChores()
        {
            var choresCollection = ConnectToMongo<ChoreModel>(ChoreCollection);
            var results = await choresCollection.FindAsync(_ => true);
            return results.ToList();
        }

        public async Task<List<ChoreModel>> GetAllChoresForAUser(UserModel user)
        {
            var choresCollection = ConnectToMongo<ChoreModel>(ChoreCollection);
            var results = await choresCollection.FindAsync(c => c.AssignedTo.Id == user.Id);
            return results.ToList();
        }

        public Task CreateUser(UserModel user)
        {
            var usersCollection = ConnectToMongo<UserModel>(UserCollection);
            return usersCollection.InsertOneAsync(user);
        }

        public Task CreateChore(ChoreModel chore)
        {
            var choresCollection = ConnectToMongo<ChoreModel>(ChoreCollection);
            return choresCollection.InsertOneAsync(chore);
        }

        public Task UpdateChore(ChoreModel chore)
        {
            var choresCollection = ConnectToMongo<ChoreModel>(ChoreCollection);
            var filter = Builders<ChoreModel>.Filter.Eq("Id", chore.Id);
            return choresCollection.ReplaceOneAsync(filter, chore, new ReplaceOptions{ IsUpsert = true });
        }

        public Task DeleteChore(ChoreModel chore)
        {
            var choresCollection = ConnectToMongo<ChoreModel>(ChoreCollection);
            return choresCollection.DeleteOneAsync(c => c.Id == chore.Id);
        }

        public async Task CompleteChore(ChoreModel chore)
        {
            var choresCollection = ConnectToMongo<ChoreModel>(ChoreCollection);
            var filter = Builders<ChoreModel>.Filter.Eq("Id", chore.Id);
            await choresCollection.ReplaceOneAsync(filter, chore);

            var choreHistoryCollection = ConnectToMongo<ChoreHistoryModel>(ChoreHistoryCollection);
            await choreHistoryCollection.InsertOneAsync(new ChoreHistoryModel(chore));
        }*/

    }
}
