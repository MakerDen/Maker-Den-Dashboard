using IotAlerts.Properties;
using MQTTProcessor;
using RestSharp.Contrib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Twilio;


namespace MqttController {
    public partial class Form1 : Form {
        bool connected = false;
        bool twilioEnabled = false;
        bool onceOnly = false;
        UTF8Encoding encoder = new UTF8Encoding();
        MQTTManager mgr;


        public Form1() {
            InitializeComponent();


        }

        void mgr_OnDataEvent(object sender, EventArgs e) {
            var fact = ((MQTTManager.DataEventArgs)e).fact;
            int threshold = 0;


            string data = string.Format("Temp: {0}, Device: {1}, Time: {2}", fact.Val[0], fact.Dev, fact.Utc) + Environment.NewLine + txtData.Text;
            if (data.Length > 20000) { data = data.Substring(0, 20000); }
            if (InvokeRequired) {
                this.Invoke((MethodInvoker)delegate {
                    txtData.Text = data; // runs on UI thread
                });
            }
            else { txtData.Text = data; }

            if (!int.TryParse(txtThreshold.Text, out threshold)) { threshold = int.MaxValue; }

            if (fact.Val[0] > threshold && !onceOnly) {
                onceOnly = true;
                if (twilioEnabled) {
                    DoTwilio(txtMessage.Text, txtPhoneNumber.Text);
                }

            }
        }

        private void btnStart_Click(object sender, EventArgs e) {
            txtStatus.Text = string.Empty;
            onceOnly = false;
            if (!connected) {
                btnStart.Text = "Stop";
                mgr.Connect();
                connected = true;
            }
            else {
                btnStart.Text = "Start";
                mgr.OnDataEvent -= mgr_OnDataEvent;
                mgr.Disconnect();
                connected = false;
            }


        }

        private void btnStop_Click(object sender, EventArgs e) {

        }


        private void btnClear_Click(object sender, EventArgs e) {
            txtStatus.Text = string.Empty;
            txtData.Text = string.Empty;
        }

        private void button1_Click(object sender, EventArgs e) {

        }


        private void Form1_Load(object sender, EventArgs e) {
            mgr = new MQTTManager("gloveboxWUS.cloudapp.net", txtTopicName.Text, false);
            mgr.OnDataEvent += mgr_OnDataEvent;
        }

        private void button3_Click(object sender, EventArgs e) {
            txtStatus.Text = string.Empty;
            onceOnly = false;
            twilioEnabled = !twilioEnabled;
            btnEnableTwilio.Text = twilioEnabled ? "Disable Twilio" : "Enable Twilio";
        }

        private void button2_Click(object sender, EventArgs e) {

        }

        private void DoTwilio(string message, string callNumber) {
            //http://azure.microsoft.com/en-us/documentation/articles/twilio-dotnet-how-to-use-for-voice-sms/


            // Use your account SID and authentication token instead
            // of the placeholders shown here.
            string accountSID = "ACb3337450201c11d0d2ebc30047c95c81";
            string authToken = "4092e54c6d561a637670ff9449d1e3ac";

            // Create an instance of the Twilio client.
            TwilioRestClient client;
            client = new TwilioRestClient(accountSID, authToken);

            // Use the Twilio-provided site for the TwiML response.
            String Url = "http://twimlets.com/message";
            string encoded = HttpUtility.UrlEncode(message);
            Url = Url + "?Message%5B0%5D=" + encoded;

            // Instantiate the call options that are passed
            // to the outbound call
            CallOptions options = new CallOptions();

            // Set the call From, To, and URL values to use for the call.
            // This sample uses the sandbox number provided by
            // Twilio to make the call.
            options.From = "+61731068676";
            options.To = callNumber;
            options.Url = Url;

            // Make the call.
            var call = client.InitiateOutboundCall(options);


            if (InvokeRequired) {
                this.Invoke((MethodInvoker)delegate {
                    txtStatus.Text = "Twilo Message Sent";// runs on UI thread
                });
            }
            else { txtStatus.Text = "Twilo Message Sent"; }


        }
    }
}
