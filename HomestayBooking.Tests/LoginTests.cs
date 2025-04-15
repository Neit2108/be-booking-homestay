using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Threading;

namespace HomestayBooking.Tests
{
    [TestClass]
    public class LoginTests : BaseTest
    {
        [TestMethod]
        public void Login_WithValidCredentials()
        {
            try
            {
                Console.WriteLine("Starting Login test with valid credentials...");
                
                // Navigate to login page
                NavigateTo("/login");
                
                // Wait for login page to load
                WaitForPageToLoad();
                
                // Log current page for debugging
                Console.WriteLine($"Login page URL: {Driver.Url}");
                
                // Use explicit waits with descriptive error messages
                Console.WriteLine("Looking for username field...");
                var usernameField = WaitForElement(By.Name("EmailorUsername"));
                HighlightElement(usernameField);
                
                Console.WriteLine("Looking for password field...");
                var passwordField = WaitForElement(By.Name("Password"));
                HighlightElement(passwordField);
                
                // Fill credentials
                usernameField.Clear();
                usernameField.SendKeys("Admin@homies.com");
                
                passwordField.Clear();
                passwordField.SendKeys("Admin@123");
                
                // Find and click login button
                IWebElement loginButton = FindLoginButton();
                if (loginButton == null)
                {
                    throw new Exception("Could not find login button");
                }
                
                HighlightElement(loginButton);
                Console.WriteLine("Clicking login button...");
                loginButton.Click();
                
                // Wait for redirect
                WaitForPageToLoad();
                
                // Log page after login attempt
                Console.WriteLine($"After login URL: {Driver.Url}");
                
                // Check for successful login indicators
                try 
                {
                    var userElement = Driver.FindElement(By.XPath("//*[contains(@class, 'userMenu') or contains(@class, 'user-menu')]"));
                    HighlightElement(userElement);
                    Assert.IsTrue(userElement.Displayed, "User menu should be visible after login");
                }
                catch
                {
                    Console.WriteLine("Could not find user menu, checking for other logged-in indicators...");
                    
                    // Check if we've been redirected to dashboard or home
                    bool isLoggedIn = Driver.Url.Contains("dashboard") || 
                                     (Driver.Url.Contains("profile")) || 
                                     Driver.PageSource.Contains("Logout");
                    
                    Assert.IsTrue(isLoggedIn, "User should be logged in and redirected to appropriate page");
                }
                
                Console.WriteLine("Login test completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login test failed: {ex.Message}");
                TakeScreenshot("LoginTest");
                throw;
            }
        }
        
        [TestMethod]
        public void Login_WithInvalidCredentials()
        {
            try
            {
                Console.WriteLine("Starting Login test with invalid credentials...");
                
                // Navigate to login page
                NavigateTo("/login");
                
                // Wait for login page to load
                WaitForPageToLoad();
                
                // Find form elements
                var usernameField = WaitForElement(By.Name("EmailorUsername"));
                var passwordField = WaitForElement(By.Name("Password"));
                
                // Fill with invalid credentials
                usernameField.Clear();
                usernameField.SendKeys("invaliduser");
                
                passwordField.Clear();
                passwordField.SendKeys("wrongpassword");
                
                // Find and click login button
                IWebElement loginButton = FindLoginButton();
                if (loginButton == null)
                {
                    throw new Exception("Could not find login button");
                }
                
                HighlightElement(loginButton);
                Console.WriteLine("Clicking login button with invalid credentials...");
                loginButton.Click();
                
                // Wait for response
                Thread.Sleep(1000);
                
                // Check for error message
                try
                {
                    var errorMessage = Driver.FindElement(By.XPath("//*[contains(@class, 'error') or contains(@class, 'errorMessage') or contains(text(), 'thất bại') or contains(text(), 'failed')]"));
                    HighlightElement(errorMessage);
                    Console.WriteLine($"Found error message: {errorMessage.Text}");
                    Assert.IsTrue(errorMessage.Displayed, "Error message should be displayed for invalid login");
                }
                catch
                {
                    // Still on login page is also a valid failure indicator
                    Assert.IsTrue(Driver.Url.Contains("login"), "User should remain on login page after failed login");
                }
                
                Console.WriteLine("Invalid login test completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid login test failed: {ex.Message}");
                TakeScreenshot("InvalidLoginTest");
                throw;
            }
        }
        
        [TestMethod]
        public void Login_AdminCredentials()
        {
            try
            {
                Console.WriteLine("Starting Admin Login test...");
                
                // Navigate to login page
                NavigateTo("/login");
                
                // Wait for login page to load
                WaitForPageToLoad();
                
                // Log current page for debugging
                Console.WriteLine($"Login page URL: {Driver.Url}");
                
                // Find form elements
                var usernameField = WaitForElement(By.Name("EmailorUsername"));
                var passwordField = WaitForElement(By.Name("Password"));
                
                // Clear fields and enter admin credentials
                usernameField.Clear();
                usernameField.SendKeys("Admin@homies.com");
                
                passwordField.Clear();
                passwordField.SendKeys("Admin@123");
                
                Console.WriteLine("Admin credentials entered");
                
                // Find login button
                IWebElement loginButton = FindLoginButton();
                if (loginButton == null)
                {
                    throw new Exception("Could not find login button");
                }
                
                // Click login button
                HighlightElement(loginButton);
                Console.WriteLine("Clicking login button...");
                loginButton.Click();
                
                // Wait for redirect
                WaitForPageToLoad();
                
                // Log page after login attempt
                Console.WriteLine($"After login URL: {Driver.Url}");
                
                // Verify successful admin login
                try
                {
                    // Check for admin dashboard elements or admin-specific content
                    var adminElement = Driver.FindElement(By.XPath("//*[contains(text(), 'Admin Dashboard') or contains(text(), 'Dashboard')]"));
                    HighlightElement(adminElement);
                    Console.WriteLine($"Found admin element: {adminElement.Text}");
                    Assert.IsTrue(adminElement.Displayed, "Admin dashboard element should be visible");
                }
                catch
                {
                    // Alternatively, check for user menu that would indicate logged in state
                    var userMenu = Driver.FindElement(By.XPath("//*[contains(@class, 'userMenu') or contains(@class, 'user-menu')]"));
                    HighlightElement(userMenu);
                    Console.WriteLine("Found user menu element");
                    Assert.IsTrue(userMenu.Displayed, "User menu should be visible after admin login");
                }
                
                Console.WriteLine("Admin Login test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Admin Login test failed: {ex.Message}");
                TakeScreenshot("AdminLoginTest");
                throw;
            }
        }
        
        // Helper method to find login button
        private IWebElement FindLoginButton()
        {
            try
            {
                return Driver.FindElement(By.CssSelector("button.loginButton"));
            }
            catch
            {
                try
                {
                    return Driver.FindElement(By.CssSelector("[class*='loginButton']"));
                }
                catch
                {
                    // Try a more general approach
                    var buttons = Driver.FindElements(By.TagName("button"));
                    foreach (var button in buttons)
                    {
                        if (button.Text.Contains("Login") || button.Text.Contains("Sign in") || 
                            button.Text.Contains("Đăng nhập"))
                        {
                            return button;
                        }
                    }
                    return null;
                }
            }
        }
    }
}