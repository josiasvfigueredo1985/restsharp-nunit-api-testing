using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace RestSharpApiTesting.Helpers
{
    public static class CartDataSet
    {
        public static void CreateJsonDataSet(List<string> productIds, int quantity)
        {
            DataSet dataSet = new DataSet("Cart");
            DataTable table = new DataTable();

            table.TableName = "produtos";
            DataColumn idColumn = new DataColumn("idProduto", typeof(string));
            DataColumn itemColumn = new DataColumn("quantidade", typeof(int));

            table.Columns.Add(idColumn);
            table.Columns.Add(itemColumn);
            dataSet.Tables.Add(table);

            foreach (var id in productIds)
            {
                DataRow newRow = table.NewRow();
                newRow["idProduto"] = id;
                newRow["quantidade"] = quantity;
                table.Rows.Add(newRow);
            }

            dataSet.AcceptChanges();
            string path = GeneralHelpers.ReturnProjectPath() + "Json/CartDataSet.json";
            string json = JsonConvert.SerializeObject(dataSet, Formatting.Indented);
            StreamWriter sw = new StreamWriter(path);
            sw.WriteLine(json);
            sw.Close();

            Console.WriteLine(json);
        }
    }
}