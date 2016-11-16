using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WcfChatSample.Service;
using System.Security.Cryptography;

namespace WcfChatSample.Server.DB
{
    public class SqliteDbProvider : IDbProvider 
    {
        private SqliteDbContext _db = null;
        private object _users_lock = new object();

        public SqliteDbProvider()
        {
            _db = new SqliteDbContext();
        }
        
        public void AddMessage(IDbMessage message)
        {
            _db.Messages.Add(new MessageEntity() { Date = message.Date, Username = message.Username, Text = message.Text });
            _db.SaveChanges();
        }

        public IDbMessage[] GetLastMessages(int? count)
        {
            if (!count.HasValue)
            {
                return _db
                    .Messages.OrderByDescending(m => m.Date)
                    .ToArray();
            }
            else
            {
                return _db.Messages.OrderByDescending(m => m.Date)
                    .Take(count.Value > 0 ? count.Value : 10)
                    .ToArray();
            }
        }

        public LoginResult Login(string username, string password)
        {
            UserEntity usr = null;

            lock(_users_lock)
            {
                usr = _db.Users.FirstOrDefault(u => u.Username == username);

                if (usr == null)
                {
                    usr = new UserEntity() { Username = username, IsAdmin = false };
                    usr.Password = GeneratePassword(usr, password);

                    _db.Users.Add(usr);
                    _db.SaveChanges();

                    return LoginResult.User;
                }
            }

            var pass = GeneratePassword(new UserEntity() { Username = username }, password);

            if (pass.Equals(usr.Password))
            {
                return usr.IsAdmin ? LoginResult.Admin : LoginResult.User;
            }

            return LoginResult.None;
        }

        public void SetAdmin(string username, string password)
        {
            var usr = new UserEntity() { Username = username, IsAdmin = true };
            usr.Password = GeneratePassword(usr, password);

            lock (_users_lock)
            {
                var usrs = from u in _db.Users
                    where u.Username == username
                    select u;

                if (usrs.Any())
                {
                    _db.Users.RemoveRange(usrs);
                }

                _db.Users.Add(usr);
                _db.SaveChanges();
            }
        }

        private byte[] GeneratePassword(UserEntity usr, string password)
        {
            var encrypted = new List<byte> (ProtectedData.Protect(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(usr.Username), DataProtectionScope.LocalMachine));

            while(encrypted.Count < 50)
            {
                encrypted.Add(encrypted[encrypted.Count / 2]);
            }

            return encrypted.Take(50).ToArray();
        }
    }
}
