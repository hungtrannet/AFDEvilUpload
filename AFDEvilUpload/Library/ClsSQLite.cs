using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SQLite;
using System.Text;

namespace ADFEvilUpload.Library
{
	public class ClsSQLite
	{

		
		private string msDBLocation = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\AFDEvil.db";
		public static string FixStr(string psValue )
		{
			string lsReturn;
            if (psValue == null)
            {
                psValue = "";
            }
            lsReturn = (psValue + "").Replace("'", "''");
			return lsReturn;
		}
		public System.Data.DataTable Query(string psSQL)
		{
			System.Data.DataTable loDataReturn = null ;
			System.Data.DataSet loDS = new System.Data.DataSet();
		
			SQLiteConnection loSql_con;
		
			SQLiteDataAdapter loSQLAdapter;
			loSql_con = getConnection();
		
 
            loSQLAdapter = new SQLiteDataAdapter(psSQL, loSql_con);

			loDS.Reset();
			loSQLAdapter.Fill(loDS);
			loDataReturn = loDS.Tables[0];

			loDS.Tables.Remove(loDataReturn);
			loDS.Clear();
			loDS.Dispose();
			loSql_con.Close();
            loSql_con.Dispose();
            loSQLAdapter.Dispose();



            return loDataReturn;
		}
		public int Exec( string psSQL)
		{
			int liRowReturn = 0;
			SQLiteConnection loSql_con;
			SQLiteCommand loSql_cmd;
			loSql_con = getConnection();
			loSql_cmd = loSql_con.CreateCommand();
			loSql_cmd.CommandText = psSQL;
			liRowReturn= loSql_cmd.ExecuteNonQuery();
			loSql_cmd.Dispose();
			loSql_con.Close();
            loSql_con.Dispose();
			return liRowReturn;
		}
		public  SQLiteConnection  getConnection()
		{
			string lsSQL="";
			try
			{
				SQLiteConnection loConnection;
				if (!System.IO.File.Exists(msDBLocation))
				{
					lsSQL = "create table CustomerOrder ( "
											+ "          CusOrderID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, "
											+ "          Property TEXT , "
                                            + "          Customer TEXT , "
                                            + "          Action   TEXT , "
                                            + "          Value    TEXT , "
											+ "          File     TEXT , "
											+ "          Hash     TEXT , "
											+ "          Status   TEXT  "
											+ "    );";



					SQLiteConnection.CreateFile(msDBLocation);
				}
				else
				{
					lsSQL = "";
				}


				loConnection=  new SQLiteConnection("Data Source=" + msDBLocation + ";Version=3");
				loConnection.Open();
				if (!lsSQL.Equals(""))
				{

					
					SQLiteCommand loSql_cmd;
					
					loSql_cmd = loConnection.CreateCommand();
					loSql_cmd.CommandText = lsSQL;
					loSql_cmd.ExecuteNonQuery();
					loSql_cmd.Dispose();
				

				}
				return loConnection;


			}
			catch(Exception ex)
			{ throw ex ; }

		}
	
		
	}
}