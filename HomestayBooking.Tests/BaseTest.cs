using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace HomestayBooking.Tests
{
    public abstract class BaseTest
    {
        protected IWebDriver Driver { get; private set; }
        protected WebDriverWait Wait { get; private set; }
        protected string BaseUrl { get; private set; } = "http://localhost:5173";
        protected TestContext TestContext { get; set; }

        [TestInitialize]
        public virtual void TestInitialize()
        {
            try
            {
                // Setup ChromeDriver with optimized settings
                new DriverManager().SetUpDriver(new ChromeConfig());
                
                var options = new ChromeOptions();
                options.AddArgument("--start-maximized");
                options.AddArgument("--disable-extensions");
                options.AddArgument("--disable-infobars");
                options.AddArgument("--disable-notifications");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--no-sandbox");
                
                // Start browser with optimization for faster loading
                options.PageLoadStrategy = PageLoadStrategy.Eager;
                
                Driver = new ChromeDriver(options);
                
                // Set optimized timeouts
                Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
                Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
                Driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(30);
                
                Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TestInitialize: {ex.Message}");
                throw;
            }
        }

        [TestCleanup]
        public virtual void TestCleanup()
        {
            // Reduced wait time at end
            Thread.Sleep(3000);
            
            try
            {
                Driver?.Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TestCleanup: {ex.Message}");
            }
            finally
            {
                Driver?.Dispose();
                Driver = null;
            }
        }

        protected void NavigateTo(string url)
        {
            string fullUrl = $"{BaseUrl}{url}";
            Console.WriteLine($"Navigating to: {fullUrl}");
            
            Driver.Navigate().GoToUrl(fullUrl);
            
            // Reduced wait after navigation
            Thread.Sleep(500);
        }

        protected IWebElement WaitForElement(By locator, int timeoutInSeconds = 10)
        {
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => {
                    try {
                        var element = drv.FindElement(locator);
                        return element.Displayed ? element : null;
                    }
                    catch {
                        return null;
                    }
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine($"Timeout waiting for element: {locator}");
                TakeScreenshot($"ElementWaitTimeout_{DateTime.Now:yyyyMMddHHmmss}");
                throw new Exception($"Element not found: {locator}", ex);
            }
        }

        protected void WaitForPageToLoad(int timeoutInSeconds = 10)
        {
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutInSeconds));
                wait.Until(driver => 
                    ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
                
                // Reduced wait after page load
                Thread.Sleep(300);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error waiting for page to load: {ex.Message}");
                throw;
            }
        }
        
        protected void TakeScreenshot(string fileName)
        {
            try
            {
                if (Driver != null)
                {
                    Screenshot screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                    string path = $"{fileName}.png";
                    screenshot.SaveAsFile(path);
                    Console.WriteLine($"Screenshot saved: {path}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to take screenshot: {ex.Message}");
            }
        }
        
        protected void HighlightElement(IWebElement element)
        {
            try
            {
                var jsDriver = (IJavaScriptExecutor)Driver;
                string originalStyle = element.GetAttribute("style");
                
                jsDriver.ExecuteScript(
                    "arguments[0].setAttribute('style', 'background: yellow; border: 2px solid red;');", 
                    element);
                
                Thread.Sleep(300); // Reduced highlight time
                
                jsDriver.ExecuteScript(
                    "arguments[0].setAttribute('style', arguments[1]);", 
                    element, 
                    originalStyle);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to highlight element: {ex.Message}");
            }
        }
    }
}