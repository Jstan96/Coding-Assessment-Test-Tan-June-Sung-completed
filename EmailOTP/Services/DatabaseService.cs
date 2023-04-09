using EmailOTP.dto;
using EmailOTP.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EmailOTP.Services
{
    public class DatabaseService
    {
        private readonly IMongoCollection<Email> _emailsCollection;

        public DatabaseService(
            IOptions<EmailOTPDatabaseSeting> EmailOTPDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                EmailOTPDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                EmailOTPDatabaseSettings.Value.DatabaseName);

            _emailsCollection = mongoDatabase.GetCollection<Email>(
                EmailOTPDatabaseSettings.Value.EmailCollectionName);
        }

        public async Task<List<Email>> GetAsync() =>
            await _emailsCollection.Find(_ => true).ToListAsync();

        public async Task<Email?> GetAsync(string id) =>
            await _emailsCollection.Find(x => x.Username == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Email newBook) =>
            await _emailsCollection.InsertOneAsync(newBook);

        public async Task UpdateAsync(string id, Email updatedBook) =>
            await _emailsCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

        public async Task RemoveAsync(string id) =>
            await _emailsCollection.DeleteOneAsync(x => x.Id == id);
    }
}
