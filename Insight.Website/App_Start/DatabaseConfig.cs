using Insight.Database;
using Insight.Database.Schema;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Insight.Website
{
    public static class DatabaseConfig
    {
        // TODO: make database migration a separate task that is explicitly invoked on production
        public static void UpgradeSchema(string connectionString)
        {
            // Needs admin permission on database if the database does not yet exist - does nothing if it does
            SchemaInstaller.CreateDatabase(connectionString);

            // Do upgrade in the background for faster startup during development
            // (new Thread(() =>
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // make sure our database exists
                    SchemaInstaller installer = new SchemaInstaller(connection);
                    new SchemaEventConsoleLogger().Attach(installer);

                    // load the schema from the embedded resources in this project
                    SchemaObjectCollection schema = new SchemaObjectCollection();
                    schema.Load(Assembly.GetExecutingAssembly());

                    // install the schema
                    installer.Install("test", schema);
                }
            }
            // )).Start();
        }
    }
}
