using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoBTAdapterDotNet.Models
{
   
    public interface ISuccessOrFailure
    {
        bool Status { get; set; }
        string Message { get; set; }
    }

    public class Success : ISuccessOrFailure
    {
        public bool Status { get; set; }
        public string Message { get; set; }

        public Success()
        {
            Status = true;
            Message = string.Empty;
        }
    }

    public class Failure : ISuccessOrFailure
    {
        public bool Status { get; set; }
        public string Message { get; set; }

        public Failure()
        {
            Status = false;
            Message = string.Empty;
        }
    }


    public class ContextWrapper<T>
    {
        static Guid GLOBALGuid = Guid.NewGuid();

        public DateTime dateTime { get; set; }
        public Guid sourceGuid { get; set; }
        public Guid messageGuid { get; set; }
        public int length { get; set; }
        public String payloadType { get; set; }
        public ICollection<T> payload { get; set; }
        public bool hasError { get; set; }
        public string message { get; set; }

        public ContextWrapper(T obj, string error = "")
        {
            this.dateTime = DateTime.UtcNow;
            this.sourceGuid = ContextWrapper<T>.GLOBALGuid;
            this.messageGuid = Guid.NewGuid();

            this.payloadType = obj == null ? "NONE" : obj.GetType().Name;
            this.payload = new List<T>() { obj };
            this.length = this.payload.Count;

            this.hasError = error != string.Empty ? true : false;
            this.message = error != string.Empty ? error : string.Empty;
        }

        public ContextWrapper(ICollection<T> list, string error = "") : this(list.FirstOrDefault(), error)
        {
            this.payload = list;
            this.length = payload.Count;
        }


        public ContextWrapper(IEnumerable<T> list, string error = "") : this(list.FirstOrDefault(), error)
        {
            this.payload = list.ToArray();
            this.length = payload.Count;
        }

        public ContextWrapper(string error)
        {
            this.length = 0;
            this.dateTime = DateTime.UtcNow;
            this.payloadType = typeof(T).Name;
            this.payload = new List<T>() { };
            this.hasError = true;
            this.message = error;
        }

    }
}
