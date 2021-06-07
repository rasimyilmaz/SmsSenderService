using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SmsSenderService
{
    public partial class Service1 : ServiceBase
    {
        public IDisposable httpListener;
        public Robot RobotInstance;
        public HttpClient httpClient;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            RobotInstance = Robot.Instance;
            RobotInstance.RequestListEvent += StackChanged;
            httpClient = new HttpClient();
            httpListener = WebApp.Start<Startup>("http://localhost:12345");
        }
        public void StackChanged(object sender, Robot.EventArgs e)
        {
            if (e.Code == 1)
            {
                SmsRequest request = RobotInstance.PopRequest();
                //SmsCallback callback= RobotInstance.Send(request);
                //SendCallback(callback);
            }
        }
        public async Task<HttpResponseMessage> SendCallback(SmsCallback callback)
        {
            Uri address = new Uri("https://www.odeonparkotel.com/test2.php");
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("id", callback.id),
                new KeyValuePair<string, string>("code", callback.code.ToString()),
                new KeyValuePair<string, string>("message", callback.message),
                new KeyValuePair<string, string>("timestamp", callback.timestamp)
            });
            var response = await httpClient.PostAsync(address, formContent);
            return response;
        }
        protected override void OnStop()
        {
            httpListener.Dispose();
            RobotInstance.Clear();
        }
    }
}
