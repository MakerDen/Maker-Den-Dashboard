using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTProcessor {
    public class Fact {
        public string Dev { get; set; }
        public string Type { get; set; }
        public double[] Val { get; set; }
        public string Unit { get; set; }
        public DateTime Utc { get; set; }
        public uint Id { get; set; }
        public string Geo { get; set; }
        public string Topic { get; set; }
    }
}
