using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;


namespace IoBTAdapterDotNet.Models
{
    
    public interface IMedusaEntity
    {
        ISuccessOrFailure Slew();
    }

    public class MedusaEntity : IMedusaEntity
    {
        public string sourceGuid { get; set; }
        public string timeStamp { get; set; }
        public string personId { get; set; }

        public ISuccessOrFailure Slew() {
            Console.WriteLine("Medusa Slew");
            Console.Beep();

            var result = new Success() {
                Message = "Slew worked"
            };
            return result;
        }
    }


}
