using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Threading;

namespace HomestayBooking.Tests
{
    [TestClass]
    public class RegisterTests : BaseTest
    {
        [TestMethod]
        public void Registration_Success()
        {
            try
            {
                Console.WriteLine("Starting Registration Success test...");
                
                // Navigate to registration page
                NavigateTo("/register");
                
                // Wait for registration page to load
                WaitForPageToLoad();
                
                // Generate unique values for registration
                string uniqueId = DateTime.Now.ToString("yyyyMMddHHmmss");
                string testEmail = $"test{uniqueId}@example.com";
                string testUsername = $"testuser{uniqueId}";
                
                Console.WriteLine($"Using test email: {testEmail}");
                Console.WriteLine($"Using test username: {testUsername}");
                
                // Fill in registration form
                try {
                    FillRegistrationForm(
                        fullName: $"Test User {uniqueId}",
                        identityCard: $"1234{uniqueId.Substring(uniqueId.Length - 8)}",
                        email: testEmail,
                        phone: $"098{uniqueId.Substring(uniqueId.Length - 7)}",
                        address: "Test Address",
                        username: testUsername,
                        password: "Test@123"
                    );
                    
                    Console.WriteLine("Registration form completed");
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error filling registration form: {ex.Message}");
                    TakeScreenshot("RegistrationFormError");
                    throw;
                }
                
                // Find and click register button
                IWebElement registerButton = FindRegisterButton();
                if (registerButton == null)
                {
                    throw new Exception("Could not find register button");
                }
                
                // Click register button
                HighlightElement(registerButton);
                Console.WriteLine("Clicking register button...");
                registerButton.Click();
                
                // Wait for processing
                WaitForPageToLoad();
                Thread.Sleep(1000); // Extra wait for registration processing
                
                // Check for success message or successful registration indication
                try
                {
                    var successMessage = Driver.FindElement(By.XPath("//*[contains(text(), 'Đăng ký thành công') or contains(text(), 'Registration successful') or contains(text(), 'successfully')]"));
                    HighlightElement(successMessage);
                    Console.WriteLine($"Found success message: {successMessage.Text}");
                    Assert.IsTrue(successMessage.Displayed, "Registration success message should be visible");
                }
                catch
                {
                    // Check if redirected to login page which could indicate success
                    if (Driver.Url.Contains("login"))
                    {
                        Console.WriteLine("Redirected to login page, registration appears successful");
                    }
                    else
                    {
                        throw new Exception("Could not verify registration success");
                    }
                }
                
                Console.WriteLine("Registration Success test completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration Success test failed: {ex.Message}");
                TakeScreenshot("RegistrationSuccessTest");
                throw;
            }
        }
        
        [TestMethod]
        public void Registration_Failure()
        {
            try
            {
                Console.WriteLine("Starting Registration Failure test...");
                
                // Navigate to registration page
                NavigateTo("/register");
                
                // Wait for registration page to load
                WaitForPageToLoad();
                
                // Fill in the form with invalid data to trigger failure
                try {
                    // Use a very short password to trigger validation error
                    FillRegistrationForm(
                        fullName: "Test User",
                        identityCard: "123456789012", // This should be a valid format
                        email: "invalid@example", // Invalid email format
                        phone: "1234", // Too short phone number
                        address: "Test Address",
                        username: "test",
                        password: "123" // Too short password
                    );
                    
                    Console.WriteLine("Registration form filled with invalid data");
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error filling registration form: {ex.Message}");
                    TakeScreenshot("RegistrationFormError");
                    throw;
                }
                
                // Find and click register button
                IWebElement registerButton = FindRegisterButton();
                if (registerButton == null)
                {
                    throw new Exception("Could not find register button");
                }
                
                // Click register button
                HighlightElement(registerButton);
                Console.WriteLine("Clicking register button...");
                registerButton.Click();
                
                // Wait for response
                Thread.Sleep(1000);
                
                // Check for error messages or validation errors
                try
                {
                    // Look for error messages anywhere on the page
                    var errorElements = Driver.FindElements(By.XPath("//*[contains(@class, 'error') or contains(@class, 'invalid') or contains(@class, 'errorMessage')]"));
                    
                    if (errorElements.Count > 0)
                    {
                        foreach (var error in errorElements)
                        {
                            if (error.Displayed)
                            {
                                HighlightElement(error);
                                Console.WriteLine($"Found error message: {error.Text}");
                            }
                        }
                        
                        Assert.IsTrue(errorElements.Count > 0, "Validation error messages should be displayed");
                        Console.WriteLine("Found validation errors as expected");
                    }
                    else
                    {
                        // Check for failed registration message or modal
                        var failMessage = Driver.FindElement(By.XPath("//*[contains(text(), 'thất bại') or contains(text(), 'failed') or contains(text(), 'error')]"));
                        HighlightElement(failMessage);
                        Console.WriteLine($"Found failure message: {failMessage.Text}");
                        Assert.IsTrue(failMessage.Displayed, "Registration failure message should be visible");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not find expected error messages: {ex.Message}");
                    throw;
                }
                
                Console.WriteLine("Registration Failure test completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration Failure test failed: {ex.Message}");
                TakeScreenshot("RegistrationFailureTest");
                throw;
            }
        }
        
        // Helper method to fill registration form
        private void FillRegistrationForm(string fullName, string identityCard, string email, 
                                        string phone, string address, string username, string password)
        {
            // Find and fill each form field
            var fullNameField = WaitForElement(By.Name("FullName"));
            fullNameField.Clear();
            fullNameField.SendKeys(fullName);
            
            var identityCardField = WaitForElement(By.Name("IdentityCard"));
            identityCardField.Clear();
            identityCardField.SendKeys(identityCard);
            
            var emailField = WaitForElement(By.Name("Email"));
            emailField.Clear();
            emailField.SendKeys(email);
            
            var phoneField = WaitForElement(By.Name("PhoneNumber"));
            phoneField.Clear();
            phoneField.SendKeys(phone);
            
            var addressField = WaitForElement(By.Name("HomeAddress"));
            addressField.Clear();
            addressField.SendKeys(address);
            
            var usernameField = WaitForElement(By.Name("Username"));
            usernameField.Clear();
            usernameField.SendKeys(username);
            
            var passwordField = WaitForElement(By.Name("Password"));
            passwordField.Clear();
            passwordField.SendKeys(password);
        }
        
        // Helper method to find register button
        private IWebElement FindRegisterButton()
        {
            try
            {
                return Driver.FindElement(By.CssSelector("button.registerButton"));
            }
            catch
            {
                try
                {
                    return Driver.FindElement(By.CssSelector("[class*='registerButton']"));
                }
                catch
                {
                    // Try a more general approach
                    var buttons = Driver.FindElements(By.TagName("button"));
                    foreach (var button in buttons)
                    {
                        if (button.Text.Contains("Register") || button.Text.Contains("Sign up") || 
                            button.Text.Contains("Đăng ký"))
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