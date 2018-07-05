using DDTrackPlusCommon.Models;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;

using Serilog;

namespace DDTrackMOPSServices.Controllers
{
    /// <summary>
    /// Sets the Supplier Information for use in each Order and also the main system for use in the Portal
    /// </summary>
    public class CustomerDeliveryController : CommonApiController
    {
        private CustomerDeliveryDataController dc = new CustomerDeliveryDataController();


        List<CustomerDeliveryDetails> validList = new List<CustomerDeliveryDetails>();

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomerDeliveryController()
        {
            _controllerName = "CustomerDeliveryController";
        }

        private void CheckModel(CustomerDeliveryDetails item)
        {
            string caller = _actionName;
            _actionName = "CheckModel";
            ModelState.Clear();

            this.Validate(item);

            string JSON = item.ToJSON();

            if (ModelState.IsValid)
            {
                Log.Information(String.Format("{0} {1}", getCaller("MODEL VALID"), JSON));
                validList.Add(item);
            }
            else
            {
                Log.Error(String.Format("{0} {1}", getCaller("MODEL INVALID"), JSON));
                if (!ModelState.IsValid)
                {
                    List<string> errs = cc.GetErrorListFromModelState(ModelState);

                    foreach (string s in errs)
                    {
                        Log.Error(String.Format("{0} {1}", getCaller(), s));
                    }
                }
            }
        }
       // /// <summary>
       // /// 
       // /// 
       // /// </summary>
       // /// <returns></returns>
       //[HttpGet]
       // public async Task<IHttpActionResult> Post()
       // {
       //     byte[] json = await Request.Content.ReadAsByteArrayAsync();
       //     var str = System.Text.Encoding.Default.GetString(json);



       //     return Ok();

       // }

        /// <summary>
        /// Add a new customer address
        /// Accepts a list of CustomerDeliveryDetails
        /// with multiple options
        /// </summary>
        ///   
      
        [HttpPost]
        [ResponseType(typeof(bool))]
        public IHttpActionResult PostNewItem( List<CustomerDeliveryDetails> items)
        {
            _actionName = "PostNewItem";
            bool auth = cc.CheckAuthorization(Request);
            if (!auth)
            {
                Log.Error(String.Format("{0} not authorized , invalid Bearer Key", getCaller()));
                Log.Error(String.Format("{0} Returning 401 Unauthorized", getCaller()));
                return Unauthorized();
            }
            if ( items == null  )
            {
                Log.Error(String.Format("{0} Returning 400 Bad Request as parameter items = null ", getCaller()));
               

                return BadRequest();
            }

            foreach (CustomerDeliveryDetails item in items)
            {
                CheckModel(item);
            }


            // add items
            try
            {
                string Feedback = "";
                foreach (CustomerDeliveryDetails i in items)
                {
                    long Id = 0;
                    returnValue rc = dc.AddCustomerDeliveryDetails (i, out Feedback,out Id);
                    if (rc != returnValue.RETURN_SUCCESS)
                    {
                        Log.Error(String.Format("{0} Returning 400 Bad Request", getCaller()));

                        return BadRequest(String.Format("{0} : Item {1}", Feedback , i.AccountNumber ));
                    }
                    else
                    {
                        Log.Information(String.Format("{0} {1}", getCaller("SUCCESS"), _actionName, ""));
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error(String.Format("{0} {1}", getCaller() ,  ex.Message));
                Log.Error(String.Format("{0} Returning 400 Bad Request", getCaller()));

                return BadRequest("Customer Address Not Added");
            }


            if (validList.Count == 0)
            {
                Log.Error(String.Format("{0} Returning 400 Bad Request", getCaller()));
                return BadRequest("Customer Address Not Added");
            }


            if (validList.Count != items.Count)
            {
                return Json(validList);
            }

            Log.Information(String.Format("{0} Returning 200 OK", getCaller()));
            return Ok();

        }

      
    }
}