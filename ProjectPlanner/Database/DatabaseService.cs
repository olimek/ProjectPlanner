using Microsoft.Maui.Devices.Sensors;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectPlanner.Database
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
        }

        public static async Task<DatabaseService> CreateAsync(string dbPath)
        {
            var service = new DatabaseService(dbPath);
            await service.InitializeAsync().ConfigureAwait(false);
            return service;
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            await _database.CreateTableAsync<Project>().ConfigureAwait(false);
        }
    }
}