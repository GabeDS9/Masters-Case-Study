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
    }
}
