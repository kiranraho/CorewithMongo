using System;
using System.Threading.Tasks;
using CoreApiWithMongo.API.Model;
using MongoDB.Driver;

namespace CoreApiWithMongo.API.Services
{
    public class UserServices
    {

        private readonly IMongoCollection<User> _users;
        public UserServices(IBookstoreDatabaseSettings _settings)
        {
            var connectionstring= new MongoClient(_settings.ConnectionString);
            var database= connectionstring.GetDatabase(_settings.DatabaseName);
            _users=database.GetCollection<User>(_settings.UserCollectionName);
        }
     public async Task<bool> IsUserExists(string Username)
     {

         var user = await _users.Find(x=> x.Username==Username).FirstOrDefaultAsync();
         if(user==null)
            return true;

            return false;
     }

    public async Task<User> Login(string username,string password)
    {
        var userExists=await _users.Find(x=> x.Username==username).FirstOrDefaultAsync();
        if(userExists==null)
        return null;

        if(!verifythehash(userExists.PasswordHash,userExists.PasswordSalt,password))
        return null;
        return userExists;

    }


    private bool verifythehash(byte[] passwordHash, byte[] passwordSalt, string password)
        {
        using(var Hash= new System.Security.Cryptography.HMACSHA512( passwordSalt))
            {
                var computeHash = Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)); 
                for(int i =0;i<computeHash.Length;i++)
                {

                    if(computeHash[i]!=passwordHash[i])
                    return false;
                }
                return true;
            }    
        }

        public async Task<User> Register(User user ,string password)
        {
            byte[] passwordhash,passwordSalt;

                CreatePasswordHas(out passwordhash,out passwordSalt,password);
                user.PasswordHash=passwordhash;
                user.PasswordSalt=passwordSalt;
                await _users.InsertOneAsync(user);
                return user;


        }

        private void CreatePasswordHas(out byte[] passwordhash, out byte[] passwordSalt, string password)
        {
           using(var Hash= new System.Security.Cryptography.HMACSHA512())
           {
               passwordSalt=Hash.Key;
               passwordhash=Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
           }
        }
    }
}