using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using DataAccess.Models;

namespace DataAccess
{
    public class PrecinctDBDataAccess
    {
        private const string ConnectionString = "mongodb://127.0.0.1:27017";
        private const string PrecinctCollection = "precinct_data";
        private const string BuildingCollection = "building_data";
        private const string EnergyCollection = "energy_data";
        private const string SolarCollection = "solar_data";
        private string DatabaseName = "";

        public PrecinctDBDataAccess(string db_name)
        {
            DatabaseName = db_name;
        }

        private IMongoCollection<T> ConnectToMongo<T>(in string collection)
        {
            var client = new MongoClient(ConnectionString);
            var db = client.GetDatabase(DatabaseName);
            return db.GetCollection<T>(collection);
        }

        public Task CreatePrecinct(PrecinctModel precinct)
        {
            var precinctCollection = ConnectToMongo<PrecinctModel>(PrecinctCollection);
            return precinctCollection.InsertOneAsync(precinct);
        }
    }
}
