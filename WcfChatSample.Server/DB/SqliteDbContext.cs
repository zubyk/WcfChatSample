using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcfChatSample.Server.DB
{
    internal class SqliteDbContext : DbContext
    {
        public DbSet<MessageEntity> Messages { get; set; }
        public DbSet<UserEntity> Users { get; set; }

        public SqliteDbContext()
            : base("name=SqliteDb")
        {
            Database.SetInitializer<SqliteDbContext>(new SqliteDbContextInitializer());
        }

        private class SqliteDbContextInitializer : CreateDatabaseIfNotExists<SqliteDbContext>
        {
            private readonly string _createSql =
                @"CREATE TABLE IF NOT EXISTS [Messages] (
                    [Id] integer UNIQUE NOT NULL
                    , [Date] datetime NOT NULL
                    , [Username] nvarchar(50)
                    , [Text] nvarchar
                    , PRIMARY KEY (Id)   
                );
                
                CREATE TABLE IF NOT EXISTS [Users] (
                    [Id] integer UNIQUE NOT NULL
                    , [Username] nvarchar(50) NOT NULL
                    , [Password] binary(50) NOT NULL
                    , [IsAdmin] bit NOT NULL DEFAULT 0
                    , PRIMARY KEY (Id)   
                );";
            
            public override void InitializeDatabase(SqliteDbContext context)
            {
                context.Database.CreateIfNotExists();
                context.Database.ExecuteSqlCommand(_createSql);
            }
        }
    }
}
