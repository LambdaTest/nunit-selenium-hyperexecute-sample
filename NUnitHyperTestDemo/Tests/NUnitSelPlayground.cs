using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using NUnit.Framework;
using OpenQA.Selenium.Support.UI;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Model;
using AventStack.ExtentReports.Reporter;
using System.IO;
using NUnit.Framework.Interfaces;
using System.Collections.Generic;

namespace NUnitLoginTest
{
    [TestFixture("chrome", "latest", "Windows 10")]
    [TestFixture("firefox", "latest", "Windows 10")]
    [Parallelizable(ParallelScope.Self)]

    [Category("LoginTest")]
    public class NUnitLoginSample
    {
        public static string gridURL = "@hub.lambdatest.com/wd/hub";
        private readonly string test_url = "https://the-internet.herokuapp.com/login";

        public static string lt_username = NUnitToDo.NUnitSeleniumSample.LT_USERNAME;
        public static string lt_access_key = NUnitToDo.NUnitSeleniumSample.LT_ACCESS_KEY;

        private readonly ThreadLocal<IWebDriver> driver = new ThreadLocal<IWebDriver>();
        private readonly string browser;
        private readonly string version;
        private readonly string os;

        public static ExtentReports? _extent;
        private static readonly object _extentLock = new object();
        public ExtentTest? _test;
        public string? TC_Name;
        public static string dirPath = "Reports//LoginTest";

        [OneTimeSetUp]
        protected void ExtentStart()
        {
            var path = System.Reflection.Assembly.GetCallingAssembly().Location;
            var actualPath = Path.GetDirectoryName(path)!;
            var projectPath = Path.GetFullPath(Path.Combine(actualPath, "..", "..", "..")) + Path.DirectorySeparatorChar;

            Directory.CreateDirectory(projectPath + dirPath);
            var reportPath = projectPath + dirPath + "//LoginTestReport.html";

            /* ExtentReports 5.x uses ExtentSparkReporter */
            var sparkReporter = new ExtentSparkReporter(reportPath);
            
            /* Configure reporter programmatically for ExtentReports 5 */
            sparkReporter.Config.Theme = AventStack.ExtentReports.Reporter.Config.Theme.Standard;
            sparkReporter.Config.DocumentTitle = "Login Test Report with NUnit and Extent Reports";
            sparkReporter.Config.ReportName = "Login Testing on HyperTest Grid";
            sparkReporter.Config.Encoding = "UTF-8";
            
            _extent = new ExtentReports();
            _extent.AttachReporter(sparkReporter);
            _extent.AddSystemInfo("Host Name", "Login Testing on HyperTest Grid");
            _extent.AddSystemInfo("Environment", "Windows Platform");
            _extent.AddSystemInfo("UserName", "User");
        }

        public NUnitLoginSample(string browser, string version, string os)
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
                    { "build", "[HyperTest] Selenium C# Login Demo" },
                    { "name", $"{TestContext.CurrentContext.Test.ClassName}:{TestContext.CurrentContext.Test.MethodName}" },
                    { "user", lt_username },
                    { "accessKey", lt_access_key }
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
                    { "build", "[HyperTest] Selenium C# Login Demo" },
                    { "name", $"{TestContext.CurrentContext.Test.ClassName}:{TestContext.CurrentContext.Test.MethodName}" },
                    { "user", lt_username },
                    { "accessKey", lt_access_key }
                });
                options = firefoxOptions;
            }
            else
            {
                throw new ArgumentException($"Unsupported browser: {browser}");
            }

            /* Stagger parallel fixture session creation to avoid hitting the hub simultaneously */
            Thread.Sleep(new Random().Next(0, 3000));

            driver.Value = new RemoteWebDriver(
                new Uri($"https://{lt_username}:{lt_access_key}{gridURL}"),
                options.ToCapabilities(),
                TimeSpan.FromSeconds(600));
        }

        [Test]
        public void LoginSuccessTest()
        {
            string context_name = TestContext.CurrentContext.Test.Name + " on " + browser + " " + version + " " + os;
            TC_Name = context_name;

            _test = _extent!.CreateTest(context_name);

            Console.WriteLine("Navigating to Login Page");
            driver.Value!.Navigate().GoToUrl(test_url);

            /* Wait for page to load */
            var wait = new WebDriverWait(driver.Value, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElement(By.Id("username")).Displayed);

            /* Enter username */
            IWebElement usernameField = driver.Value.FindElement(By.Id("username"));
            usernameField.Clear();
            usernameField.SendKeys("tomsmith");
            Console.WriteLine("Entered username: tomsmith");

            /* Enter password */
            IWebElement passwordField = driver.Value.FindElement(By.Id("password"));
            passwordField.Clear();
            passwordField.SendKeys("SuperSecretPassword!");
            Console.WriteLine("Entered password");

            /* Click Login button */
            IWebElement loginButton = driver.Value.FindElement(By.CssSelector("button[type='submit']"));
            loginButton.Click();
            Console.WriteLine("Clicked Login button");

            /* Wait for successful login - check for flash message */
            wait.Until(d => d.FindElement(By.Id("flash")).Displayed);

            /* Verify successful login */
            IWebElement flashMessage = driver.Value.FindElement(By.Id("flash"));
            string messageText = flashMessage.Text;
            
            Assert.That(messageText, Does.Contain("You logged into a secure area!"));
            Console.WriteLine("Login successful - verified flash message");

            /* Verify we're on the secure page */
            Assert.That(driver.Value.Url, Does.Contain("/secure"));
            Console.WriteLine("Verified URL contains /secure");
        }

        [Test]
        public void LoginFailureTest()
        {
            string context_name = TestContext.CurrentContext.Test.Name + " on " + browser + " " + version + " " + os;
            TC_Name = context_name;

            _test = _extent!.CreateTest(context_name);

            Console.WriteLine("Navigating to Login Page");
            driver.Value!.Navigate().GoToUrl(test_url);

            /* Wait for page to load */
            var wait = new WebDriverWait(driver.Value, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElement(By.Id("username")).Displayed);

            /* Enter invalid username */
            IWebElement usernameField = driver.Value.FindElement(By.Id("username"));
            usernameField.Clear();
            usernameField.SendKeys("invaliduser");
            Console.WriteLine("Entered invalid username");

            /* Enter invalid password */
            IWebElement passwordField = driver.Value.FindElement(By.Id("password"));
            passwordField.Clear();
            passwordField.SendKeys("wrongpassword");
            Console.WriteLine("Entered invalid password");

            /* Click Login button */
            IWebElement loginButton = driver.Value.FindElement(By.CssSelector("button[type='submit']"));
            loginButton.Click();
            Console.WriteLine("Clicked Login button");

            /* Wait for error message */
            wait.Until(d => d.FindElement(By.Id("flash")).Displayed);

            /* Verify error message appears */
            IWebElement flashMessage = driver.Value.FindElement(By.Id("flash"));
            string messageText = flashMessage.Text;
            
            Assert.That(messageText, Does.Contain("Your username is invalid!"));
            Console.WriteLine("Login failed as expected - verified error message");

            /* Verify we're still on the login page */
            Assert.That(driver.Value.Url, Does.Contain("/login"));
            Console.WriteLine("Verified still on login page");
        }

        [OneTimeTearDown]
        protected void ExtentClose()
        {
            Console.WriteLine("OneTimeTearDown");
            lock (_extentLock)
            {
                _extent?.Flush();
            }
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

            if (driver.Value != null)
            {
                switch (exec_status)
                {
                    case TestStatus.Failed:
                        logstatus = Status.Fail;
                        /* The older way of capturing screenshots */
                        Capture(driver.Value, fileName);
                        /* Capturing Screenshots using built-in methods in ExtentReports 5 */
                        var mediaEntity = CaptureScreenShot(driver.Value, fileName);
                        _test!.Log(Status.Fail, "Fail");
                        /* Usage of MediaEntityBuilder for capturing screenshots */
                        _test.Fail("ExtentReport 5 Capture: Test Failed", mediaEntity);
                        /* Usage of traditional approach for capturing screenshots */
                        _test.Log(Status.Fail, "Traditional Snapshot below: " + _test.AddScreenCaptureFromPath("Screenshots//" + fileName));
                        break;
                    case TestStatus.Passed:
                        logstatus = Status.Pass;
                        /* The older way of capturing screenshots */
                        Capture(driver.Value, fileName);
                        /* Capturing Screenshots using built-in methods in ExtentReports 5 */
                        mediaEntity = CaptureScreenShot(driver.Value, fileName);
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
                    ((IJavaScriptExecutor)driver.Value).ExecuteScript("lambda-status=" + (passed ? "passed" : "failed"));
                }
                finally
                {
                    driver.Value.Quit();
                }
            }
            else
            {
                _test?.Log(Status.Fail, "Test: " + TC_Name + " — driver was null (Init failed): " + stacktrace);
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
