using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Script.Serialization;

namespace ADFEvilUpload.Library
{
	public class ClsEvilApi
	{
		public static ClsSQLite moDatabase = new ClsSQLite();
		private static string msCheckURL = "http://evilapi-env.ap-southeast-2.elasticbeanstalk.com/check?hash={0}";
		private static string msUploadURL = "http://evilapi-env.ap-southeast-2.elasticbeanstalk.com/upload";
	



		public static DataSet GetHashKey(string psHashKey)
		{
			DataSet loReturnDs = null;
			
			DataTable loLocalTb;
			DataTable loReturnTb;
			DataRow loReturnRow;
			Dictionary<string, object> loRemote;
			string lssql;
			
			
			// check from server
			loRemote = ClsEvilApi.CheckHashKey(psHashKey);
				loReturnDs = new DataSet();
			
			loReturnTb = new DataTable("Hash Key from EVIL");

			loReturnTb.Columns.Add("PropertyName");
			loReturnTb.Columns.Add("Value");
			foreach (string lskey in loRemote.Keys)
			{
				loReturnRow = loReturnTb.NewRow();
				loReturnTb.Rows.Add(loReturnRow);
				loReturnRow[0] = lskey;
				if (lskey == "errors")
				{

					System.Collections.ArrayList loError = (System.Collections.ArrayList)loRemote["errors"];
					loReturnRow[1]= string.Join(", ", loError.Cast<string>().ToArray());
				}
				else
				{
					loReturnRow[1] = loRemote[lskey];
				}

			
			}
		
			loReturnDs.Tables.Add(loReturnTb);


			//check from local
			lssql = "select * from CustomerOrder where Hash='{0}'";
			if (psHashKey == null)
			{
				psHashKey = "N/A";
			}
			loLocalTb = moDatabase.Query(String.Format(lssql, ClsSQLite.FixStr(psHashKey)));

			if (loLocalTb.Rows.Count > 0)
			{
				loReturnTb = new DataTable("Hash Key from local database.");

				loReturnTb.Columns.Add("PropertyName");
				loReturnTb.Columns.Add("Value");
				foreach (DataRow row in loLocalTb.Rows)
				{
					foreach (DataColumn col in loLocalTb.Columns)
					{
						loReturnRow = loReturnTb.NewRow();
						loReturnTb.Rows.Add(loReturnRow);
						loReturnRow[0] = col.ColumnName;
						loReturnRow[1] = row[col.ColumnName];

					}
				}
				loReturnDs.Tables.Add(loReturnTb);
			}


			return loReturnDs;
		}


		public static void init()
		{
			moDatabase.getConnection().Close();
		}


		private static Dictionary<string, object> CheckHashKey(string psHashKey)
		{
			
			Dictionary<string, object> loReturn;
			WebRequest loWebRequest;
			WebResponse loWebResponse;
            JavaScriptSerializer loJson = new JavaScriptSerializer();

            string lsJson = "";
			string lsURL;
			try
			{
				lsURL = string.Format(msCheckURL, psHashKey);
				loWebRequest = WebRequest.Create(lsURL);

				try
				{


					loWebResponse = loWebRequest.GetResponse();
					lsJson = new StreamReader(loWebResponse.GetResponseStream())
										  .ReadToEnd();

					loReturn = loJson.Deserialize<Dictionary<string, object>>(lsJson);
				}
				catch (WebException wex)
				{
					
					lsJson = new StreamReader(wex.Response.GetResponseStream())
										  .ReadToEnd();
					loReturn = loJson.Deserialize<Dictionary<string, object>>(lsJson);
				}

			}
			catch
			{
				lsJson = "";
				loReturn = null;
	
			}
			return loReturn;


		}
		private  static Dictionary<string, object> UploadCustomer(string psCustomerName, string psValue,string psFileName ,int piCusOrderID )
		{
			//string lsReturnHashkey = "";
			Dictionary<string, object> loReturn;
			WebRequest loWebRequest;
			WebResponse loWebResponse;

            JavaScriptSerializer loJson = new JavaScriptSerializer();
            string lsMessage;
			string lsSQL;
			Dictionary<string, object> loJsonValue = new Dictionary<string, object> ();
			string lsJson = "";
			try
			{


				loJsonValue.Add("property", "Trung Hung Tran");
				loJsonValue.Add("customer", psCustomerName);
				loJsonValue.Add("action", "order created");
				loJsonValue.Add("value", psValue);
				loJsonValue.Add("file", psFileName);
			
				loWebRequest = WebRequest.Create(msUploadURL);
				

				loWebRequest.Method = "POST";
				loWebRequest.ContentType = "application/json";
				lsJson = loJson.Serialize(loJsonValue);
				byte[] json = System.Text.Encoding.UTF8.GetBytes(lsJson);

				Stream requestStream = loWebRequest.GetRequestStream();
				requestStream.Write(json, 0, json.Length);
				requestStream.Close();
				try
				{
					
					loWebResponse = loWebRequest.GetResponse();
					lsJson = new StreamReader(loWebResponse.GetResponseStream())
										  .ReadToEnd();

					loReturn = loJson.Deserialize<Dictionary<string, object>>(lsJson);
		
					if (loReturn.ContainsKey("errors"))
					{
						System.Collections.ArrayList loError = (System.Collections.ArrayList)loReturn["errors"];

						lsMessage = string.Join(", ", loError.Cast<string>().ToArray()) + "";
						lsSQL = " update CustomerOrder set status  ='{0} : {1}' where CusOrderID ={2} ;  ";

						Library.ClsEvilApi.moDatabase.Exec(String.Format(lsSQL, loReturn["added"], ClsSQLite.FixStr(lsMessage), piCusOrderID));

					}
					else if (loReturn.ContainsKey("hash"))
					{

                        if (((string)loReturn["hash"]).Length == 32)
                        {
                            lsSQL = " update CustomerOrder set status  ='{0}', hash ='{1}' where CusOrderID ={2} ;  ";

                            Library.ClsEvilApi.moDatabase.Exec(String.Format(lsSQL, loReturn["added"], ClsSQLite.FixStr((string)loReturn["hash"]), piCusOrderID));
                        }
                        else
                        {
                            lsMessage = "Server Error Invalid hash return.";
                            lsSQL = " update CustomerOrder set status  ='{0} : {1}' where CusOrderID ={2} ;  ";


                            Library.ClsEvilApi.moDatabase.Exec(String.Format(lsSQL, "error", ClsSQLite.FixStr(lsMessage), piCusOrderID));
                        }
                    }
				}
				catch (WebException wex)
				{

					lsJson = new StreamReader(wex.Response.GetResponseStream())
										  .ReadToEnd();
					loReturn = loJson.Deserialize<Dictionary<string, object>>(lsJson);

					if (loReturn.ContainsKey("errors"))
					{
						System.Collections.ArrayList loError = (System.Collections.ArrayList)loReturn["errors"];

						lsMessage = string.Join(", ", loError.Cast<string>().ToArray());
						lsSQL = " update CustomerOrder set status  ='{0} : {1}' where CusOrderID ={2} ;  ";


						Library.ClsEvilApi.moDatabase.Exec(String.Format(lsSQL,"error", ClsSQLite.FixStr(lsMessage), piCusOrderID));

					}

				}


			


			}
			catch(Exception ex)
			{
          
                lsSQL = " update CustomerOrder set status  ='{0} : {1}' where CusOrderID ={2} ;  ";


                Library.ClsEvilApi.moDatabase.Exec(String.Format(lsSQL, "error", ClsSQLite.FixStr(ex.Message), piCusOrderID));
           
				loReturn = null;
			}
			return loReturn;
		}
		
		private static async System.Threading.Tasks.Task<Dictionary<string, object>> UploadCustomerAsync(string psCustomerName, string psValue,string psFileName,int piCusID)
		{

			Dictionary<string, object> loReturn;
			string lsMessage;
			Dictionary<string, object> loJsonValue = new Dictionary<string, object>();
            JavaScriptSerializer loJson = new JavaScriptSerializer();
            string lsJson = "";
			string lsSQL;
			try
			{


				loJsonValue.Add("property", "Trung Hung Tran");
				loJsonValue.Add("customer", psCustomerName);
				loJsonValue.Add("action", "order created");
				loJsonValue.Add("value", psValue);
				loJsonValue.Add("file",psFileName);
				lsJson = loJson.Serialize(loJsonValue);


			


				try
				{
					using (var loClient = new HttpClient())
					{
						loClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

						HttpResponseMessage loHttpRessult  =await loClient.PostAsync(msUploadURL, new StringContent(lsJson));
			
						lsJson = await loHttpRessult.Content.ReadAsStringAsync();
					}

					loReturn = loJson.Deserialize<Dictionary<string, object>>(lsJson);
					if (loReturn.ContainsKey("errors"))
					{
						System.Collections.ArrayList loError = (System.Collections.ArrayList)loReturn["errors"];

						lsMessage = string.Join(", ", loError.Cast<string>().ToArray());
						lsSQL = " update CustomerOrder set status  ='{0} : {1}' where CusOrderID ={2} ;  ";

						Library.ClsEvilApi.moDatabase.Exec(String.Format(lsSQL, loReturn["added"], ClsSQLite.FixStr(lsMessage), piCusID));

					}
					else if  (loReturn.ContainsKey("hash"))
					{
						lsSQL = " update CustomerOrder set status  ='{0}', hash ='{1}' where CusOrderID ={2} ;  ";

						Library.ClsEvilApi.moDatabase.Exec(String.Format(lsSQL, loReturn["added"], ClsSQLite.FixStr((string)loReturn["hash"]), piCusID));

					}
					
				}
				catch (WebException wex)
				{

					lsJson = new StreamReader(wex.Response.GetResponseStream())
										  .ReadToEnd();
					loReturn = loJson.Deserialize<Dictionary<string, object>>(lsJson);
				}





			}
			catch
			{
				loReturn = null;
			}
			return loReturn;
		}

        public static  void UploadRecordAsync(string psCustomer, string psValue, string psFileName)
        {
            Dictionary<string, object> loReturn;
            string lsSQL = "";
            DataTable loDataReturn;
            int liRecordID = 0;
            try
            {

                lsSQL = "insert into CustomerOrder(Property,Customer,action,value,file,hash,status)"
                            + " values('{0}','{1}','{2}','{3}','{4}','{5}','{6}') ; "
                            + " select last_insert_rowid() CusOrderID; ";


                loDataReturn = Library.ClsEvilApi.moDatabase.Query(String.Format(lsSQL,
                        "Trung Hung Tran", ClsSQLite.FixStr(psCustomer), "order created", ClsSQLite.FixStr(psValue), ClsSQLite.FixStr(psFileName), "", "uploading"));
                liRecordID = int.Parse(loDataReturn.Rows[0][0].ToString());

                loReturn =  UploadCustomer(psCustomer, psValue, psFileName, liRecordID);
            }
            catch (Exception ex)
            {
                lsSQL = " update CustomerOrder set status  ='{0}' where CusOrderID ={1} ;  ";

                Library.ClsEvilApi.moDatabase.Exec(String.Format(lsSQL, ClsSQLite.FixStr(ex.Message), liRecordID));
            }


        }

        //private static await UploadCustomerAsync(string psCustomer, string psValue, string psFileName, int liRecordID)
        //{
        //    throw new NotImplementedException();
        //}

        public static DataTable SearchCustomerOrder(string psCondition, string psCustomer )
		{
			DataTable loDatatable =null;
			string lsSQL;

			lsSQL = "select * from CustomerOrder "+  psCondition ;
			if (psCustomer != "")
			{
				lsSQL = lsSQL + String.Format(" and Customer like '%{0}%'", ClsSQLite.FixStr(psCustomer));
			}
			loDatatable = moDatabase.Query(lsSQL);
			return loDatatable;
		}
	}
}