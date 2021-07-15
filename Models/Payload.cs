using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;


namespace IoBTAdapterDotNet.Models
{
    [System.Serializable]
    public class UDTO_Base
    {
        public string sourceGuid { get; set; }
        public string timeStamp { get; set; }
        public string personId { get; set; }
    }



    [System.Serializable]
    public class UDTO_Command : UDTO_Base
    {
        public string targetGuid { get; set; }
        public string command { get; set; }
        public List<string> args { get; set; }
    }


}
