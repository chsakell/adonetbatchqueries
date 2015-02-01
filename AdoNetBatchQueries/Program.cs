using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace AdoNetBatchQueries
{
    class Program
    {
        public const string connString = "Data Source=.; Initial Catalog=ProductStore;Integrated Security = SSPI;";

        static void Main(string[] args)
        {
            //QuerySingleResultSet();
            //QueryMultipleResultSets();
            //QueryMultipleResultSetsParameterized();
            //QueryRelatedRecords();
        }

        protected static void QuerySingleResultSet()
        {
            string selectStament = "SELECT * FROM Product";

            SqlConnection conn = new SqlConnection(connString);

            // Create the SELECT command, open the connection
            SqlCommand cmd = new SqlCommand(selectStament, conn);
            conn.Open();

            // Create a DataReader object and read the results..
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    Console.WriteLine("Product ID: {0}\tName: {1}\tPrice: {2}\tUnits in Stock: {3}",
                        dr["ProductID"], dr["ProductName"], dr["ProductPrice"], dr["ProductUnitsInStock"]);
                }
            }
        }

        protected static void QueryMultipleResultSets()
        {
            // The Batch Query
            string selectStament = "SELECT * FROM Product WHERE ProductID=4; " +
                                   "SELECT * FROM [Order] WHERE OrderProductID=4";

            SqlConnection conn = new SqlConnection(connString);

            // Create the SELECT command, open the connection
            SqlCommand cmd = new SqlCommand(selectStament, conn);
            conn.Open();

            // Create a DataReader object and read the multiple result sets..
            // using the NextResult function
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                int resultSet = 0;
                do
                {
                    Console.WriteLine("Result Set: {0}", ++resultSet);
                    while (dr.Read())
                    {
                        for(int i=0; i<dr.FieldCount; i++) {
                        Console.Write(dr[i] + " ");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                    Console.WriteLine();
                } while (dr.NextResult());
            }
        }

        protected static void QueryMultipleResultSetsParameterized()
        {
            // The Batch Query
            string selectStament = "SELECT * FROM Product WHERE ProductID=@ProductID; " +
                                   "SELECT * FROM [Order] WHERE OrderProductID=@OrderProductID";

            SqlConnection conn = new SqlConnection(connString);

            // Create the SELECT command, open the connection
            SqlCommand cmd = new SqlCommand(selectStament, conn);
            cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = 4;
            cmd.Parameters.Add("@OrderProductID", SqlDbType.Int).Value = 4;
            conn.Open();

            // Create a DataReader object and read the multiple result sets..
            // using the NextResult function
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                int resultSet = 0;
                do
                {
                    Console.WriteLine("Result Set: {0}", ++resultSet);
                    while (dr.Read())
                    {
                        for (int i = 0; i < dr.FieldCount; i++)
                        {
                            Console.Write(dr[i] + " ");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                    Console.WriteLine();
                } while (dr.NextResult());
            }
        }

        protected static void QueryRelatedRecords()
        {
            string parentStatement = "SELECT * FROM Product";
            string childStatement = "SELECT * FROM [Order]";

            // Create the DataSet to fill and an SqlDataAdapter
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter(parentStatement, connString);

            // Fill Parent Table in the DataSet
            //da.FillSchema(ds, SchemaType.Source, "Product");
            da.Fill(ds, "Product");

            da = new SqlDataAdapter(childStatement, connString);
            // Fill Child Table in the DataSet
            //da.FillSchema(ds, SchemaType.Source, "Order");
            da.Fill(ds, "Order");

            // Define the Relationship between the two Tables in the DataSet
            DataRelation dr = new DataRelation("Product_Order",
                ds.Tables["Product"].Columns["ProductID"],
                ds.Tables["Order"].Columns["OrderProductID"]);
            ds.Relations.Add(dr);

            // Use the GetChildRows() to Get child (order) recodrs for a product
            for (int i = 0; i < ds.Tables["Product"].Rows.Count;i++ )
            {
                DataRow productRow = ds.Tables["Product"].Rows[i];
                Console.WriteLine("Product {0}, {1}, {2}", productRow[0], productRow[1], productRow[2]);
                Console.WriteLine("Orders:");
                foreach (DataRow childOrderRow in productRow.GetChildRows(dr))
                {
                    Console.WriteLine("Order {0}, Date: {1}, Customer: {2}, Quantity {3}",
                        childOrderRow[0], childOrderRow[1], childOrderRow["OrderCustomer"], childOrderRow["OrderQuantity"]);
                }
                Console.WriteLine("---------------------------------------------------------------------------------");
            }
        }
    }
}
