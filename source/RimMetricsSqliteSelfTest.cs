using System;
using System.IO;
using System.Data.SQLite;
using Verse;

namespace RimMetrics
{
    public static class RimMetricsSqliteSelfTest
    {
        public static void Init()
        {
            try
            {
                RimMetricsNativeESqlite3Loader.Preload();

                // string dir = Path.Combine(GenFilePaths.ConfigFolderPath, "RimMetrics");
                // Directory.CreateDirectory(dir);
                // string dbPath = Path.Combine(dir, "rimmetrics_test.sqlite");

                // using (var con = new SQLiteConnection("Data Source=" + dbPath))
                // {
                //     con.Open();
                //     using (var cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS test (id INTEGER PRIMARY KEY, t TEXT);", con))
                //         cmd.ExecuteNonQuery();

                //     using (var cmd = new SQLiteCommand("INSERT INTO test(t) VALUES (@t);", con))
                //     {
                //         cmd.Parameters.AddWithValue("@t", DateTime.UtcNow.ToString("o"));
                //         cmd.ExecuteNonQuery();
                //     }
                // }

                // UserLogger.Message("System.Data.SQLite self-test OK");
            }
            catch (Exception ex)
            {
                UserLogger.Error("Please report the error below to the developer:");
                UserLogger.Error("System.Data.SQLite self-test FAILED: " + ex);
            }
        }
    }
}
