using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace DDTrackMOPSServices.Controllers
{
    /// <summary>
    /// My Value Congroller
    /// </summary>
    /// 
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ValuesController : ApiController
    {
        // GET api/values
        /// <summary>
        /// My Get 
        /// </summary>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        /// <summary>
        /// My Get 
        /// </summary>
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        /// <summary>
        /// My Get 
        /// </summary>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        /// <summary>
        /// My Get 
        /// </summary>
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        /// <summary>
        /// My Get 
        /// </summary>
        public void Delete(int id)
        {
        }
    }
}
