using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsSenderService
{
    public sealed class Robot
    {
        private static readonly object look = new object();
        private static Robot instance = null;
        public static Robot Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (look)
                        {
                            if ( instance == null)
                            {
                                instance = new Robot();
                            }
                        }
                }
                return instance;
            }
        }
        IWebDriver driver;
        Stack<SmsRequest> RequestList;
        Object RequestListLock;
        Object DriverLock;
        public class EventArgs
        {
            public EventArgs(int code ,string message) {
                Code = code; 
                Message = message; 
            }
            public int Code { get; }
            public string Message { get; } // readonly
        }
        public delegate void EventHandler(object sender, EventArgs e);
        public event EventHandler RequestListEvent;

        Robot()
        {
            driver = new ChromeDriver();
            RequestList = new Stack<SmsRequest>();
            RequestListLock = new object();
            DriverLock = new object();
        }
        public void Clear()
        {
            driver.Close();
            driver.Quit();
            RequestList.Clear();
            RequestListLock = null;
            driver = null;
            instance = null;
            DriverLock = null;
        }
        public void PushRequest(SmsRequest request)
        {
            lock (RequestListLock)
            {
                RequestList.Push(request);
            }
            RequestListEvent?.Invoke(this, new EventArgs(1, "Push"));
        }
        public SmsRequest PopRequest()
        {
            SmsRequest item;
            lock (RequestListLock)
            {
                item= RequestList.Pop();
            }
            return item;
        }
        public bool isRequestListEmpty()
        {
            bool isEmpty;
            lock (RequestListLock)
            {
                if (RequestList.Count == 0)
                {
                    isEmpty = true;
                }
                else
                {
                    isEmpty = false;
                }
            }
            return isEmpty;
        }
        public SmsCallback Send(SmsRequest request)
        {
            SmsCallback callback = new SmsCallback(){ id=request.id, callbackurl=request.callbackurl };
            lock (DriverLock)
            {
                var stopwatch = new Stopwatch();
                try
                {
                    stopwatch.Start();
                    driver.Url = "http://m.home/index.html#sms";
                    IWebElement newSms = WaitUntilElementClickable(By.Id("smslist-new-sms"), 30);
                    newSms.Click();
                    IWebElement phone = WaitUntilElementClickable(By.Id("chosen-search-field-input"));
                    phone.Click();
                    phone.SendKeys(request.phonenumber);
                    phone.SendKeys(Keys.Return);
                    IWebElement chat = WaitUntilElementClickable(By.Id("chat-input"));
                    chat.SendKeys(request.message);
                    IWebElement sendButton = WaitUntilElementClickable(By.Id("btn-send"));
                    sendButton.Click();
                    sendButton = WaitUntilElementClickable(By.Id("btn-send"), 30);
                    IWebElement deleteButton = WaitUntilElementClickable(By.ClassName("smslist-item-delete"));
                    deleteButton.Click();
                    IWebElement sureButton = WaitUntilElementClickable(By.Id("yesbtn"));
                    sureButton.Click();
                    sendButton = WaitUntilElementClickable(By.Id("btn-send"), 30);
                    stopwatch.Stop();
                    callback.code = 0;
                    callback.message = "Başarılı";
                }
                catch (Exception Ex)
                {
                    callback.code = 1;
                    callback.message = Ex.Message;
                }
            }
            return callback;
        }
        public IWebElement WaitUntilElementClickable(By elementLocator, int timeout = 10)
        {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
                return wait.Until(ExpectedConditions.ElementToBeClickable(elementLocator));
        }
    }
}
