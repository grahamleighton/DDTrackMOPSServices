using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace DDTrackMOPSServices.Controllers
{


    public class CommonController : System.Web.Mvc.Controller
    {
        public List<string> GetErrorListFromModelState(ModelStateDictionary modelState)
        {
            List<string> result = new List<string>();

            List<string> errorKeys = new List<string>();
            List<string> errorValues = new List<string>();

            foreach (string k in modelState.Keys)
            {
                errorKeys.Add(k);
            }
            foreach ( ModelState ms in modelState.Values )
            {
                errorValues.Add("1");
                foreach ( ModelError e in ms.Errors)
                {
                    result.Add(errorKeys[errorValues.Count-1] + " : " + e.ErrorMessage);
                }
            }


            /*            var query = from state in modelState.Values
                                    from error in state.Errors
                                    select state.Value+" "+error.ErrorMessage;

                        var errorList = query.ToList();
                        */
            return result;
        }

        public bool CheckAuthorization(HttpRequestMessage req)
        {
            bool Authorized = false;
            if (req.Headers.Contains("Authorization"))
            {
                string token = req.Headers.GetValues("Authorization").First();
                if (!String.IsNullOrEmpty(token))
                {
                    char[] charr = { ' ' };

                    string[] entries = token.Split(charr);
                  
                    if (entries.Count() == 2)
                    {
                        if (entries[0] == "Bearer")
                        {
                            string key = entries[1];
                            if (key == ConfigurationManager.AppSettings["key"])
                            {
                                Authorized = true;
                            }
                        }

                    }
                }

            }
            if (!Authorized)
            {
                Log.Information("Unauthorized Access");
            }
            return Authorized;
        }
    }
}
