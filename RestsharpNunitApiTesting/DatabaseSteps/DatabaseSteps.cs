using RestSharpApiTesting.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RestSharpApiTesting.DatabaseSteps
{
    public static class DatabaseOperations
    {
        #region Products

        public static void InsertCreatedProductIntoDb(string productId)
        {
            string query = File.ReadAllText(GeneralHelpers.ReturnProjectPath() + "Queries/InsertProductIds.sql", Encoding.UTF8);
            query = query.Replace("$produto_id", productId);

            DatabaseHelpers.ExecuteQueryMySQL(query);

            ExtentReportHelpers.AddTestInfo(2, "PARAMETERS: produto_id = " + productId);
        }

        public static void DeleteAllProductIds()
        {
            string query = File.ReadAllText(GeneralHelpers.ReturnProjectPath() + "Queries/DeleteProductIds.sql", Encoding.UTF8);

            DatabaseHelpers.ExecuteQueryMySQL(query);

            ExtentReportHelpers.AddTestInfo(2, "PARAMETERS: Column: produto_id");
        }

        public static List<string> GetProductIds()
        {
            string query = File.ReadAllText(GeneralHelpers.ReturnProjectPath() + "Queries/GetProductIds.sql", Encoding.UTF8);

            ExtentReportHelpers.AddTestInfo(2, "PARAMETERS: Column: produto_id");
            return DatabaseHelpers.GetMySqlQueryData(query);
        }

        public static void DeleteProductId(string productId)
        {
            string query = File.ReadAllText(GeneralHelpers.ReturnProjectPath() + "Queries/DeleteProductById.sql", Encoding.UTF8);
            query = query.Replace("$produto_id", productId);

            DatabaseHelpers.ExecuteQueryMySQL(query);

            ExtentReportHelpers.AddTestInfo(2, "PARAMETERS: Column: produto_id");
        }

        #endregion Products

        #region Users

        public static void InsertCreatedUserIntoDb(string name, string email, string password, string administrator, string userId)
        {
            string query = File.ReadAllText(GeneralHelpers.ReturnProjectPath() + "Queries/InsertUserData.sql", Encoding.UTF8);
            query = query.Replace("$nome", name);
            query = query.Replace("$email", email);
            query = query.Replace("$password", password);
            query = query.Replace("$administrador", administrator);
            query = query.Replace("$id", userId);
            DatabaseHelpers.ExecuteQueryMySQL(query);

            ExtentReportHelpers.AddTestInfo(2, $"PARAMETERS: User data: \nName: {name}\nID: {userId}");
        }

        public static List<string> GetCreatedUsers()
        {
            string query = File.ReadAllText(GeneralHelpers.ReturnProjectPath() + "Queries/GetCreatedUsers.sql", Encoding.UTF8);

            ExtentReportHelpers.AddTestInfo(2, $"PARAMETERS: Executed query: {query}");
            return DatabaseHelpers.GetMySqlQueryData(query);
        }

        public static void DeleteUserById(string userId)
        {
            string query = File.ReadAllText(GeneralHelpers.ReturnProjectPath() + "Queries/DeleteUserById.sql", Encoding.UTF8);
            query = query.Replace("$id", userId);

            DatabaseHelpers.ExecuteQueryMySQL(query);

            ExtentReportHelpers.AddTestInfo(2, $"PARAMETERS: Executed query: {query}");
        }

        public static void DeleteAllCreatedUsers()
        {
            string query = File.ReadAllText(GeneralHelpers.ReturnProjectPath() + "Queries/DeleteAllCreatedUsers.sql", Encoding.UTF8);

            DatabaseHelpers.ExecuteQueryMySQL(query);

            ExtentReportHelpers.AddTestInfo(2, $"PARAMETERS: Executed query: {query}");
        }

        #endregion Users
    }
}