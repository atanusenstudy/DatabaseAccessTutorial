using DAL.DataContext;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;

namespace DAL.Utilities
{
    public static class ConnectionTester
    {
        public static void TestAllConnections(IConfiguration configuration)
        {
            Console.WriteLine("=== Testing Database Connections ===");

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            Console.WriteLine($"Connection String: {connectionString}");

            TestSqlConnection(connectionString);
            TestEntityFrameworkConnection(configuration);
            TestDapperConnection(configuration);
            TestAdoNetConnection(configuration);
        }



        private static void TestSqlConnection(string connectionString)
        {
            try
            {
                Console.WriteLine("\n1. Testing raw SQL Connection...");
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("   ✓ Raw SQL Connection: SUCCESS");

                    // Test if database exists
                    using (var command = new SqlCommand(
                        "IF DB_ID('DatabaseAccessTutorialDB') IS NOT NULL SELECT 1 ELSE SELECT 0",
                        connection))
                    {
                        var exists = (int)command.ExecuteScalar();
                        Console.WriteLine($"   • Database exists: {(exists == 1 ? "YES" : "NO")}");
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ Raw SQL Connection: FAILED - {ex.Message}");
            }
        }

        private static void TestEntityFrameworkConnection(IConfiguration configuration)
        {
            try
            {
                Console.WriteLine("\n2. Testing Entity Framework Connection...");
                var optionsBuilder = new DbContextOptionsBuilder<EntityFrameworkDbContext>();
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

                using (var context = new EntityFrameworkDbContext(optionsBuilder.Options))
                {
                    if (context.Database.CanConnect())
                    {
                        Console.WriteLine("   ✓ Entity Framework Connection: SUCCESS");
                    }
                    else
                    {
                        Console.WriteLine("   ✗ Entity Framework Connection: Cannot connect");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ Entity Framework Connection: FAILED - {ex.Message}");
            }
        }

        private static void TestDapperConnection(IConfiguration configuration)
        {
            try
            {
                Console.WriteLine("\n3. Testing Dapper Connection...");
                var context = new DapperContext(configuration);
                using (var connection = context.CreateConnection())
                {
                    connection.Open();
                    Console.WriteLine("   ✓ Dapper Connection: SUCCESS");
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ Dapper Connection: FAILED - {ex.Message}");
            }
        }

        private static void TestAdoNetConnection(IConfiguration configuration)
        {
            try
            {
                Console.WriteLine("\n4. Testing ADO.NET Connection...");
                var context = new AdoNetContext(configuration);
                using (var connection = context.CreateConnection())
                {
                    connection.Open();
                    Console.WriteLine("   ✓ ADO.NET Connection: SUCCESS");
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ ADO.NET Connection: FAILED - {ex.Message}");
            }
        }
    }
}