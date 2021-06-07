using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace SmsSenderService
{
    public class SmsController:ApiController
    {
        public Robot RobotInstance = Robot.Instance;

        public SmsResponse Post([FromBody] SmsRequest request)
        {
            RobotInstance.PushRequest(request);
            return new SmsResponse { code = 100, id = request.id, message="İşlem sıraya alındı." };
        }
    }
}
