using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using OpenQA.Selenium.Support.UI;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Model;
using AventStack.ExtentReports.Reporter;
using NUnit.Framework.Interfaces;

namespace NUnitToDo
{
    [TestFixture("chrome", "latest", "Windows 10")]
    [TestFixture("firefox", "latest", "Windows 10")]
    [Parallelizable(ParallelScope.Self)]

    [Category("ToDoTest")]
    public class NUnitSeleniumSample
    {
        public static string LT_USERNAME = Environment.GetEnvironmentVariable("LT_USERNAME") ?? "LT_USERNAME";
        public static string LT_ACCESS_KEY = Environment.GetEnvironmentVariable("LT_ACCESS_KEY") ?? "LT_ACCESS_KEY";
        public static string gridURL = "@hub.lambdatest.com/wd/hub";

        private readonly ThreadLocal<IWebDriver> driver = new ThreadLocal<IWebDriver>();
        private readonly string browser;
        private readonly string version;
        private readonly string os;

        public static ExtentReports? _extent;
        public ExtentTest? _test;
        public string? TC_Name;
        public static string dirPath = "Reports//ToDoTest";

        [OneTimeSetUp]
        protected void ExtentStart()
        {
            var path = System.Reflection.Assembly.GetCallingAssembly().Location;
            var actualPath = Path.GetDirectoryName(path)!;
            var projectPath = Path.GetFullPath(Path.Combine(actualPath, "..", "..", "..")) + Path.DirectorySeparatorChar;

            Directory.CreateDirectory(projectPath + dirPath);

            var reportPath = projectPath + dirPath + "//ToDoTestReport.html";

            /* ExtentReports 5.x uses ExtentSparkReporter */
            var sparkReporter = new ExtentSparkReporter(reportPath);
            
            /* Configure reporter programmatically for ExtentReports 5 */
            sparkReporter.Config.Theme = AventStack.ExtentReports.Reporter.Config.Theme.Standard;
            sparkReporter.Config.DocumentTitle = "Report Demonstration with NUnit and Extent Reports";
            sparkReporter.Config.ReportName = "Demonstration of NUnit framework on HyperTest Grid";
            sparkReporter.Config.Encoding = "UTF-8";
            
            _extent = new ExtentReports();
            _extent.AttachReporter(sparkReporter);
            _extent.AddSystemInfo("Host Name", "ToDo Testing on HyperTest Grid");
            _extent.AddSystemInfo("Environment", "Windows Platform");
            _extent.AddSystemInfo("UserName", "User");
        }

        public NUnitSeleniumSample(string browser, string version, string os)
        {
            this.browser = browser;
            this.version = version;
            this.os = os;
        }

        [SetUp]
        public void Init()
        {
            /* Selenium 4 uses browser-specific Options classes instead of DesiredCapabilities */
            DriverOptions options;

            if (browser.ToLower() == "chrome")
            {
                var chromeOptions = new ChromeOptions();
                chromeOptions.BrowserVersion = version;
                chromeOptions.PlatformName = os;
                chromeOptions.AddAdditionalOption("LT:Options", new Dictionary<string, object>
                {
                    { "build", "[HyperTest] Selenium C# ToDo Demo" },
                    { "name", $"{TestContext.CurrentContext.Test.ClassName}:{TestContext.CurrentContext.Test.MethodName}" },
                    { "user", LT_USERNAME },
                    { "accessKey", LT_ACCESS_KEY }
                });
                options = chromeOptions;
            }
            else if (browser.ToLower() == "firefox")
            {
                var firefoxOptions = new FirefoxOptions();
                firefoxOptions.BrowserVersion = version;
                firefoxOptions.PlatformName = os;
                firefoxOptions.AddAdditionalOption("LT:Options", new Dictionary<string, object>
                {
                    { "build", "[HyperTest] Selenium C# ToDo Demo" },
                    { "name", $"{TestContext.CurrentContext.Test.ClassName}:{TestContext.CurrentContext.Test.MethodName}" },
                    { "user", LT_USERNAME },
                    { "accessKey", LT_ACCESS_KEY }
                });
                options = firefoxOptions;
            }
            else
            {
                throw new ArgumentException($"Unsupported browser: {browser}");
            }

            driver.Value = new RemoteWebDriver(
                new Uri($"https://{LT_USERNAME}:{LT_ACCESS_KEY}{gridURL}"),
                options.ToCapabilities(),
                TimeSpan.FromSeconds(600));

            Console.Out.WriteLine(driver);
        }

        [Test]
        public void Todotest()
        {
            string context_name = TestContext.CurrentContext.Test.Name + " on " + browser + " " + version + " " + os;
            TC_Name = context_name;

            _test = _extent!.CreateTest(context_name);

            Console.WriteLine("Navigating to todos app.");
            driver.Value!.Navigate().GoToUrl("https://ltqa-frontend.lambdatestinternal.com/sample-todo-app/");
            var wait = new WebDriverWait(driver.Value, TimeSpan.FromSeconds(20));
            wait.Until(d => d.FindElement(By.Name("li4")).Displayed);
            driver.Value.FindElement(By.Name("li4")).Click();
            Console.WriteLine("Clicking Checkbox");
            driver.Value.FindElement(By.Name("li5")).Click();
            IList<IWebElement> elems = driver.Value.FindElements(By.CssSelector("input[type='checkbox']:checked"));
            Assert.That(elems.Count, Is.EqualTo(2));
            Console.WriteLine("Entering Text");
            driver.Value.FindElement(By.Id("sampletodotext")).SendKeys("Yey, Let's add it to list");
            driver.Value.FindElement(By.Id("addbutton")).Click();
            string expectedText = "Yey, Let's add it to list";
            var lastItem = wait.Until(d =>
            {
                var items = d.FindElements(By.CssSelector("ul.list-unstyled li span[data-testid^='todo-text-']"));
                if (items.Count == 0) return null;
                var last = items[items.Count - 1];
                return last.Text.Trim() == expectedText ? last : null;
            });
            Assert.That(lastItem!.Text.Trim(), Is.EqualTo(expectedText));
        }

        [OneTimeTearDown]
        protected void ExtentClose()
        {
            Console.WriteLine("OneTimeTearDown");
            _extent?.Flush();
        }

        [TearDown]
        public void Cleanup()
        {
            bool passed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Passed;
            var exec_status = TestContext.CurrentContext.Result.Outcome.Status;
            var stacktrace = string.IsNullOrEmpty(TestContext.CurrentContext.Result.StackTrace) ? ""
                : string.Format("{0}", TestContext.CurrentContext.Result.StackTrace);
            Status logstatus = Status.Pass;
            string fileName;

            DateTime time = DateTime.Now;
            fileName = "Screenshot_" + time.ToString("h_mm_ss") + TC_Name + ".png";

            switch (exec_status)
            {
                case TestStatus.Failed:
                    logstatus = Status.Fail;
                    /* The older way of capturing screenshots */
                    Capture(driver.Value!, fileName);
                    /* Capturing Screenshots using built-in methods in ExtentReports 5 */
                    var mediaEntity = CaptureScreenShot(driver.Value!, fileName);
                    _test!.Log(Status.Fail, "Fail");
                    /* Usage of MediaEntityBuilder for capturing screenshots */
                    _test.Fail("ExtentReport 5 Capture: Test Failed", mediaEntity);
                    /* Usage of traditional approach for capturing screenshots */
                    _test.Log(Status.Fail, "Traditional Snapshot below: " + _test.AddScreenCaptureFromPath("Screenshots//" + fileName));
                    break;
                case TestStatus.Passed:
                    logstatus = Status.Pass;
                    /* The older way of capturing screenshots */
                    Capture(driver.Value!, fileName);
                    /* Capturing Screenshots using built-in methods in ExtentReports 5 */
                    mediaEntity = CaptureScreenShot(driver.Value!, fileName);
                    _test!.Log(Status.Pass, "Pass");
                    /* Usage of MediaEntityBuilder for capturing screenshots */
                    _test.Pass("ExtentReport 5 Capture: Test Passed", mediaEntity);
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
            _test?.Log(logstatus, "Test: " + TC_Name + " Status:" + logstatus + stacktrace);

            try
            {
                /* Logs the result to LambdaTest */
                ((IJavaScriptExecutor)driver.Value!).ExecuteScript("lambda-status=" + (passed ? "passed" : "failed"));
            }
            finally
            {
                /* Terminates the remote webdriver session */
                driver.Value?.Quit();
            }
        }

        public static string Capture(IWebDriver driver, string screenShotName)
        {
            ITakesScreenshot ts = (ITakesScreenshot)driver;
            Screenshot screenshot = ts.GetScreenshot();
            var path = System.Reflection.Assembly.GetCallingAssembly().Location;
            var actualPath = Path.GetDirectoryName(path)!;
            var reportPath = Path.GetFullPath(Path.Combine(actualPath, "..", "..", "..")) + Path.DirectorySeparatorChar;
            
            Directory.CreateDirectory(reportPath + dirPath + "//Screenshots");
            var finalPath = reportPath + dirPath + "//Screenshots//" + screenShotName;
            screenshot.SaveAsFile(finalPath);
            return reportPath;
        }

        public Media CaptureScreenShot(IWebDriver driver, string screenShotName)
        {
            ITakesScreenshot ts = (ITakesScreenshot)driver;
            var screenshot = ts.GetScreenshot().AsBase64EncodedString;

            return MediaEntityBuilder.CreateScreenCaptureFromBase64String(screenshot, screenShotName).Build();
        }
    }
}
