using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using NUnit.Framework;
using System.Threading;
using OpenQA.Selenium.Support.UI;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System.IO;
using NUnit.Framework.Interfaces;

namespace NUnitSeleniumPlayground
{
    [TestFixture("chrome", "latest-1", "Windows 10")]
    [TestFixture("firefox", "latest-1", "Windows 10")]
    // [TestFixture("chrome", "latest-1", "Linux")]
    // [TestFixture("firefox", "latest-1", "Linux")]
    [Parallelizable(ParallelScope.Self)]

    [Category("SeleniumPlayGround")]
    public class NUnitSeleniumSample2
    {
        public static string gridURL = "@hub.lambdatest.com/wd/hub";
        private String test_url = "https://www.lambdatest.com/selenium-playground/";

        public static string lt_username = NUnitToDo.NUnitSeleniumSample.LT_USERNAME;
        public static string lt_access_key = NUnitToDo.NUnitSeleniumSample.LT_ACCESS_KEY;

        ThreadLocal<IWebDriver> driver = new ThreadLocal<IWebDriver>();
        private String browser;
        private String version;
        private String os;

        public static ExtentReports _extent;
        public ExtentTest _test;
        public String TC_Name;
        public static String dirPath = "Reports//SeleniumPlaygroundTest";

        /* In ideal cases, it is recommended to create a single report that has links to the resective tests */
        /* A seperate report is created here for demonstration of download artifacts management functionality */
        [OneTimeSetUp]
        protected void ExtentStart()
        {
            var path = System.Reflection.Assembly.GetCallingAssembly().CodeBase;
            var actualPath = path.Substring(0, path.LastIndexOf("bin"));
            var projectPath = new Uri(actualPath).LocalPath;

            Directory.CreateDirectory(projectPath.ToString() + dirPath);
            var reportPath = projectPath + dirPath + "//SeleniumPlaygroundReport.html";

            /* For Version 3 */
            /* var htmlReporter = new ExtentV3HtmlReporter(reportPath); */
            /* For version 4 --> Creates Index.html */
            var htmlReporter = new ExtentHtmlReporter(reportPath);
            _extent = new ExtentReports();
            _extent.AttachReporter(htmlReporter);
            _extent.AddSystemInfo("Host Name", "Selenium Playground Testing on HyperTest Grid");
            _extent.AddSystemInfo("Environment", "Windows Platform");
            _extent.AddSystemInfo("UserName", "User");
            htmlReporter.LoadConfig(projectPath + "Configurations//report-config.xml");
        }

        public NUnitSeleniumSample2(String browser, String version, String os)
        {
            this.browser = browser;
            this.version = version;
            this.os = os;
        }

        [SetUp]
        public void Init()
        {

            DesiredCapabilities capabilities = new DesiredCapabilities();
            capabilities.SetCapability(CapabilityType.BrowserName, browser);
            capabilities.SetCapability(CapabilityType.Version, version);
            capabilities.SetCapability(CapabilityType.Platform, os);
            capabilities.SetCapability("build", "[HyperTest] Selenium C# Playground Demo");

            capabilities.SetCapability("user", NUnitToDo.NUnitSeleniumSample.LT_USERNAME);
            capabilities.SetCapability("accessKey", NUnitToDo.NUnitSeleniumSample.LT_ACCESS_KEY);

            capabilities.SetCapability("name",
            String.Format("{0}:{1}",
            TestContext.CurrentContext.Test.ClassName,
            TestContext.CurrentContext.Test.MethodName));
            driver.Value = new RemoteWebDriver(new Uri("https://" + lt_username + ":" + lt_access_key + gridURL),
                            capabilities, TimeSpan.FromSeconds(600));

            Console.Out.WriteLine(driver);
        }

        [Test]
        public void SeleniumPlayGroundTest()
        {
            String context_name = TestContext.CurrentContext.Test.Name + " on " + browser + " " + version + " " + os;
            TC_Name = context_name;

            _test = _extent.CreateTest(context_name);

            Console.WriteLine("Navigating to Selenium Playground");
            driver.Value.Navigate().GoToUrl(test_url);


            IWebElement element = driver.Value.FindElement(By.XPath("//a[.='Input Form Submit']"));
            element.Click();

            String current_url = driver.Value.Url;
            Console.WriteLine("Current URL is " + current_url);

            IWebElement name = driver.Value.FindElement(By.XPath("//input[@id='name']"));
            name.SendKeys("Testing");

            IWebElement email_address = driver.Value.FindElement(By.Id("inputEmail4"));
            email_address.SendKeys("testing@testing.com");

            IWebElement password = driver.Value.FindElement(By.XPath("//input[@name='password']"));
            password.SendKeys("password");

            IWebElement company = driver.Value.FindElement(By.CssSelector("#company"));
            company.SendKeys("LambdaTest");

            IWebElement website = driver.Value.FindElement(By.CssSelector("#websitename"));
            website.SendKeys("https://wwww.lambdatest.com");

            IWebElement countryDropDown = driver.Value.FindElement(By.XPath("//select[@name='country']"));
            SelectElement selectElement = new SelectElement(countryDropDown);
            selectElement.SelectByText("United States");

            IWebElement city = driver.Value.FindElement(By.XPath("//input[@id='inputCity']"));
            city.SendKeys("San Jose");

            IWebElement address1 = driver.Value.FindElement(By.CssSelector("[placeholder='Address 1']"));
            address1.SendKeys("Googleplex, 1600 Amphitheatre Pkwy");

            IWebElement address2 = driver.Value.FindElement(By.CssSelector("[placeholder='Address 2']"));
            address2.SendKeys(" Mountain View, CA 94043");

            IWebElement state = driver.Value.FindElement(By.CssSelector("#inputState"));
            state.SendKeys("California");

            IWebElement zipcode = driver.Value.FindElement(By.CssSelector("#inputZip"));
            zipcode.SendKeys("94088");

            /* Click on the Submit button */
            IWebElement submit_button = driver.Value.FindElement(By.CssSelector(".btn"));
            submit_button.Click();

            /* Assert if the page contains a certain text */
            bool bValue = driver.Value.PageSource.Contains("Thanks for contacting us, we will get back to you shortly");

            if (bValue)
            {
                Console.WriteLine("Input Form Demo successful");
            }
            else
            {
                Console.WriteLine("Input Form Demo failed");
            }
        }

        [OneTimeTearDown]
        protected void ExtentClose()
        {
            Console.WriteLine("OneTimeTearDown");
            _extent.Flush();
        }

        [TearDown]
        public void Cleanup()
        {
            bool passed = TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Passed;
            var exec_status = TestContext.CurrentContext.Result.Outcome.Status;
            var stacktrace = string.IsNullOrEmpty(TestContext.CurrentContext.Result.StackTrace) ? ""
            : string.Format("{0}", TestContext.CurrentContext.Result.StackTrace);
            Status logstatus = Status.Pass;
            String screenShotPath, fileName;

            DateTime time = DateTime.Now;
            fileName = "Screenshot_" + time.ToString("h_mm_ss") + TC_Name + ".png";

            switch (exec_status)
            {
                case TestStatus.Failed:
                    logstatus = Status.Fail;
                    /* The older way of capturing screenshots */
                    screenShotPath = Capture(driver.Value, fileName);
                    /* Capturing Screenshots using built-in methods in ExtentReports 4 */
                    var mediaEntity = CaptureScreenShot(driver.Value, fileName);
                    _test.Log(Status.Fail, "Fail");
                    /* Usage of MediaEntityBuilder for capturing screenshots */
                    _test.Fail("ExtentReport 4 Capture: Test Failed", mediaEntity);
                    /* Usage of traditional approach for capturing screenshots */
                    _test.Log(Status.Fail, "Traditional Snapshot below: " + _test.AddScreenCaptureFromPath("Screenshots//" + fileName));
                    break;
                case TestStatus.Passed:
                    logstatus = Status.Pass;
                    /* The older way of capturing screenshots */
                    screenShotPath = Capture(driver.Value, fileName);
                    /* Capturing Screenshots using built-in methods in ExtentReports 4 */
                    mediaEntity = CaptureScreenShot(driver.Value, fileName);
                    _test.Log(Status.Pass, "Pass");
                    /* Usage of MediaEntityBuilder for capturing screenshots */
                    _test.Pass("ExtentReport 4 Capture: Test Passed", mediaEntity);
                    /* Usage of traditional approach for capturing screenshots */
                    _test.Log(Status.Pass, "Traditional Snapshot below: " + _test.AddScreenCaptureFromPath("Screenshots//" + fileName));
                    break;
                case TestStatus.Inconclusive:
                    logstatus = Status.Warning;
                    break;
                case TestStatus.Skipped:
                    logstatus = Status.Skip;
                    break;
                default:
                    break;
            }
            _test.Log(logstatus, "Test: " + TC_Name + " Status:" + logstatus + stacktrace);

            try
            {
                ((IJavaScriptExecutor)driver.Value).ExecuteScript("lambda-status=" + (passed ? "passed" : "failed"));
            }
            finally
            {

                driver.Value.Quit();
            }
        }

        public static string Capture(IWebDriver driver, String screenShotName)
        {
            ITakesScreenshot ts = (ITakesScreenshot)driver;
            Screenshot screenshot = ts.GetScreenshot();
            var pth = System.Reflection.Assembly.GetCallingAssembly().CodeBase;
            var actualPath = pth.Substring(0, pth.LastIndexOf("bin"));
            var reportPath = new Uri(actualPath).LocalPath;
            Directory.CreateDirectory(reportPath + dirPath + "//Screenshots");
            var finalpth = pth.Substring(0, pth.LastIndexOf("bin")) + dirPath + "//Screenshots//" + screenShotName;
            var localpath = new Uri(finalpth).LocalPath;
            screenshot.SaveAsFile(localpath, ScreenshotImageFormat.Png);
            return reportPath;
        }

        public MediaEntityModelProvider CaptureScreenShot(IWebDriver driver, String screenShotName)
        {
            ITakesScreenshot ts = (ITakesScreenshot)driver;
            var screenshot = ts.GetScreenshot().AsBase64EncodedString;

            return MediaEntityBuilder.CreateScreenCaptureFromBase64String(screenshot, screenShotName).Build();
        }
    }
}
