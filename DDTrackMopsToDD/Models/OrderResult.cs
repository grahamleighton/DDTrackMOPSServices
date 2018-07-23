using DDTrackPlusCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DDTrackMopsToDD.Models
{
    public class OrderResult
    {
        public OrderKey OrderKey { get; set; }
        private List<string> _result;
        public List<string> Result { get { return _result; } }


        public OrderResult()
        {
            OrderKey = new OrderKey();
            _result = new List<string>();
        }

        public void Reset()
        {
            _result.Clear();
            OrderKey.Reset();
        }
        public void Reset(Order o)
        {
            _result.Clear();
            OrderKey.Reset();
            OrderKey.ImportFromOrder(o);
        }
        public void setResult(string result)
        {
            _result.Add(result);
        }
    }
}