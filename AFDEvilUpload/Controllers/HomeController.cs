using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ADFEvilUpload.Controllers
{
	public class HomeController : Controller
	{
		[HttpPost]
		public ActionResult UploadCSV(HttpPostedFileBase poFileUpload)
		{

			string lsMessage = "";
            if (poFileUpload != null)
            {
                if (poFileUpload.ContentLength > 0)
                {
                    if (poFileUpload.FileName.EndsWith(".csv"))
                    {



                        using (StreamReader loCsvReader = new StreamReader(poFileUpload.InputStream))
                        {
                            string lsFullData = loCsvReader.ReadToEnd();
                            string[] lasRow = lsFullData.Split('\r');



                            for (int liIndex = 0; liIndex < lasRow.Count(); liIndex++)
                            {

                                string lsRow;
                                string[] loaColumn;
                                lsRow = lasRow[liIndex];
                                if (lsRow.IndexOf(",") >= 0)
                                {
                                    loaColumn = lasRow[liIndex].Split(',');
                                }
                                else
                                {
                                    loaColumn = new string[2] { lsRow, "" };
                                }
                                Task.Run(() => Library.ClsEvilApi.UploadRecordAsync(loaColumn[0], loaColumn[1], Path.GetFileName(poFileUpload.FileName))).ConfigureAwait(true);



                            }

                            lsMessage = String.Format("{0} records has been processed.", lasRow.Count());

                        }



                        ViewBag.Message = lsMessage;
                        return View("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("File", "This file format is not supported");
                        return View("Index");
                    }


                    // was everything ok?
                    //  if (success)

                    //    else
                    //  return View("UploadFail");
                }
                else
                {
                    ModelState.AddModelError("File", "Please Upload Your file");
                }
            }

			return RedirectToAction("Index", new { error = "Please upload a file..." });
		}

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult CheckUpload()  
		{
            Hashtable loView= new Hashtable();
			DataTable loData;
			ViewBag.Message = "Check All upload record and status.";
			string lsStatus = Request["optStatus"];
			string lsStatusCondition;
            
			string lsCustomer = Request["txtCustomer"] + "";
           
            

            if ( lsStatus  == "Success")
			{
				lsStatusCondition = " where Status = 'true' ";

			}
			else if (lsStatus == "Uploading")
			{
				lsStatusCondition = " where Status='uploading' ";
			}
			else if (lsStatus == "Error")
			{
				lsStatusCondition = " where Status not in ('uploading' ,'true') ";
			}
			else
			{
				lsStatus = "All";
				lsStatusCondition = " where 1=1 ";
			}
            loView.Add("optStatus", lsStatus);
            loView.Add("txtCustomer", lsCustomer);
            loData =Library.ClsEvilApi.SearchCustomerOrder(lsStatusCondition, lsCustomer);



			ViewBag.Message = String.Format("{0} records found, seach for status ={1} ,customer like '{2}'",loData.Rows.Count,lsStatus,lsCustomer );
            loView.Add("DataReturn", loData);
            return View("CheckUpload", loView);
		}
		
		public ActionResult CheckHashRecord()
		{
			DataSet loDataSet = new DataSet();
		
			
			string lsHashKey = Request["txtHashKey"];
			if (lsHashKey + ""  != "")
			{
				ViewBag.Message = "Hash key:" + lsHashKey;

				loDataSet = Library.ClsEvilApi.GetHashKey(lsHashKey);
			
				



			}
			return View("CheckHashRecord",loDataSet);
		}
	}
}