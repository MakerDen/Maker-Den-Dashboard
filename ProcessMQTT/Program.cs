using MQTTProcessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessMQTT {
    class Program {

        
        static void Main(string[] args) {

            MQTTProcessor.MQTTManager m2m = new MQTTProcessor.MQTTManager("gloveboxWUS.cloudapp.net", "gb/#");
            m2m.OnDataEvent += m2m_OnDataEvent;

            Thread.Sleep(Timeout.Infinite);

        }

        static void m2m_OnDataEvent(object sender, EventArgs e) {
            var fact = ((MQTTManager.DataEventArgs)e).fact;
            System.Console.WriteLine(string.Format("{0}, {1}, {2}, {3}",fact.Topic, fact.Dev, fact.Type, fact.Val[0]));
        }
    }
}
