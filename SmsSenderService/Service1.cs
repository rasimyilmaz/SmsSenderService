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
                SmsCallback callback= RobotInstance.Send(request);
                Task task = SendCallback(callback);
            }
        }
        public async Task SendCallback(SmsCallback callback)
        {
            httpClient.BaseAddress = new Uri(callback.callbackurl);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                HttpResponseMessage response = await httpClient.PostAsJsonAsync("/", callback);
            }
            catch (Exception Ex)
            {

            }
        }

        protected override void OnStop()
        {
            httpListener.Dispose();
            RobotInstance.Clear();
        }
    }
}
