using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Threading;

namespace HomestayBooking.Tests
{
    [TestClass]
    public class HomePageTests : BaseTest
    {
        [TestMethod]
        public void HomePage_LoadsSuccessfully()
        {
            try
            {
                Console.WriteLine("Starting Home page test...");
                
                // Navigate to home page
                NavigateTo("/");
                
                // Wait for page to load
                WaitForPageToLoad();
                
                // Log page details for debugging
                Console.WriteLine($"Page URL: {Driver.Url}");
                Console.WriteLine($"Page title: {Driver.Title}");
                
                // Try to find a specific element that should be on the page
                try
                {
                    var homeElement = Driver.FindElement(By.CssSelector("[class*='text-accent']"));
                    HighlightElement(homeElement);
                    Console.WriteLine($"Found home element with text: {homeElement.Text}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not find home element: {ex.Message}");
                }
                
                // Look for any element that should be on the page
                Assert.IsTrue(
                    Driver.PageSource.Contains("HomiesStay") || 
                    Driver.PageSource.Contains("Homies") || 
                    Driver.PageSource.Contains("Stay"),
                    "Home page should contain application name"
                );
                
                Console.WriteLine("Home page test passed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Home page test failed: {ex.Message}");
                TakeScreenshot("HomePageTest");
                throw;
            }
        }

        [TestMethod]
        public void ApplicationIsRunning()
        {
            try
            {
                Console.WriteLine("Checking if application is running...");
                
                // Navigate to the base URL
                Driver.Navigate().GoToUrl(BaseUrl);
                
                // Wait for the page
                WaitForPageToLoad();
                
                // Print page info
                Console.WriteLine($"Page URL: {Driver.Url}");
                Console.WriteLine($"Page title: {Driver.Title}");
                Console.WriteLine($"Page source length: {Driver.PageSource.Length}");
                
                // Take a screenshot
                TakeScreenshot("ApplicationCheck");
                
                // Pause to observe
                Console.WriteLine("Pausing to observe application...");
                Thread.Sleep(2000);
                
                Console.WriteLine("Application check completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Application check failed: {ex.Message}");
                throw;
            }
        }
        
        [TestMethod]
        public void Google_LoadsSuccessfully()
        {
            try
            {
                Console.WriteLine("Starting Google test...");
                
                // Test if Selenium itself works by navigating to Google
                Driver.Navigate().GoToUrl("https://www.google.com");
                WaitForPageToLoad();
                
                Console.WriteLine($"Page title: {Driver.Title}");
                
                Assert.IsTrue(Driver.Title.Contains("Google"), "Google page should load");
                
                Console.WriteLine("Google test passed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Google test failed: {ex.Message}");
                TakeScreenshot("GoogleTest");
                throw;
            }
        }
    }
}