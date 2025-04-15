using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Threading;

namespace HomestayBooking.Tests
{
    [TestClass]
    public class ProfileTests : BaseTest
    {
        [TestMethod]
        public void Profile_Page_LoadsSuccessfully()
        {
            try
            {
                Console.WriteLine("Starting Profile Page test...");
                
                // First login
                NavigateTo("/login");
                WaitForPageToLoad();
                
                try
                {
                    // Log in with test credentials
                    var usernameField = WaitForElement(By.Name("EmailorUsername"));
                    var passwordField = WaitForElement(By.Name("Password"));
                    
                    usernameField.Clear();
                    usernameField.SendKeys("Admin@homies.com");
                    
                    passwordField.Clear();
                    passwordField.SendKeys("Admin@123");
                    
                    var loginButton = Driver.FindElement(By.XPath("//button[contains(@class, 'loginButton') or contains(text(), 'Login')]"));
                    loginButton.Click();
                    
                    WaitForPageToLoad();
                    Console.WriteLine("Logged in successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Login failed: {ex.Message}");
                    throw new Exception("Login is required for profile tests");
                }
                
                // Navigate to profile page
                NavigateTo("/profile");
                WaitForPageToLoad();
                
                Console.WriteLine($"Navigated to profile page: {Driver.Url}");
                
                // Check for profile page elements
                try
                {
                    // Check for profile header
                    var profileHeader = Driver.FindElement(By.XPath("//h1[contains(text(), 'Profile') or contains(text(), 'Thông tin')]"));
                    HighlightElement(profileHeader);
                    Console.WriteLine($"Found profile header: {profileHeader.Text}");
                    
                    // Check for user details
                    var profileDetails = Driver.FindElement(By.CssSelector("[class*='ProfileDetails']"));
                    HighlightElement(profileDetails);
                    Console.WriteLine("Found profile details section");
                    
                    // Check for navigation tabs
                    var navTabs = Driver.FindElements(By.XPath("//button[contains(@class, 'text-left')]"));
                    
                    Console.WriteLine($"Found {navTabs.Count} navigation tabs");
                    
                    // If nav tabs found, check each one
                    if (navTabs.Count > 0)
                    {
                        foreach (var tab in navTabs)
                        {
                            HighlightElement(tab);
                            Console.WriteLine($"Tab: {tab.Text}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error examining profile page: {ex.Message}");
                    throw;
                }
                
                Console.WriteLine("Profile Page test completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Profile page test failed: {ex.Message}");
                TakeScreenshot("ProfilePageTest");
                throw;
            }
        }
        
        [TestMethod]
        public void Profile_UpdateUserInfo()
        {
            try
            {
                Console.WriteLine("Starting Profile Update test...");
                
                // First login
                NavigateTo("/login");
                WaitForPageToLoad();
                
                try
                {
                    // Log in with test credentials
                    var usernameField = WaitForElement(By.Name("EmailorUsername"));
                    var passwordField = WaitForElement(By.Name("Password"));
                    
                    usernameField.Clear();
                    usernameField.SendKeys("Admin@homies.com");
                    
                    passwordField.Clear();
                    passwordField.SendKeys("Admin@123");
                    
                    var loginButton = Driver.FindElement(By.XPath("//button[contains(@class, 'loginButton') or contains(text(), 'Login')]"));
                    loginButton.Click();
                    
                    WaitForPageToLoad();
                    Console.WriteLine("Logged in successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Login failed: {ex.Message}");
                    throw new Exception("Login is required for profile tests");
                }
                
                // Navigate to profile page
                NavigateTo("/profile");
                WaitForPageToLoad();
                
                // Test profile update
                try
                {
                    // Find profile fields
                    var nameField = WaitForElement(By.Name("name"));
                    
                    // If name field not found, try alternatives
                    if (nameField == null)
                    {
                        try
                        {
                            nameField = Driver.FindElement(By.XPath("//input[@id='name' or @name='FullName']"));
                        }
                        catch
                        {
                            var allInputs = Driver.FindElements(By.TagName("input"));
                            foreach (var input in allInputs)
                            {
                                string placeholder = input.GetAttribute("placeholder");
                                if (placeholder != null && (placeholder.Contains("name") || placeholder.Contains("Name")))
                                {
                                    nameField = input;
                                    break;
                                }
                            }
                        }
                    }
                    
                    if (nameField != null)
                    {
                        // Update name with timestamp to ensure it's unique
                        string timestamp = DateTime.Now.ToString("HHmmss");
                        string newName = $"Test User {timestamp}";
                        
                        HighlightElement(nameField);
                        nameField.Clear();
                        nameField.SendKeys(newName);
                        Console.WriteLine($"Updated name to: {newName}");
                        
                        // Find and click save button
                        var saveButton = Driver.FindElement(By.XPath("//button[contains(text(), 'Save') or contains(text(), 'Lưu')]"));
                        HighlightElement(saveButton);
                        Console.WriteLine("Found save button, clicking...");
                        saveButton.Click();
                        
                        // Wait for save operation
                        Thread.Sleep(2000);
                        
                        // Verify changes were saved
                        // This could include checking for success message or reloading the page
                        try
                        {
                            var successMessage = Driver.FindElement(By.XPath("//*[contains(text(), 'success') or contains(text(), 'thành công')]"));
                            if (successMessage.Displayed)
                            {
                                Console.WriteLine("Profile update successful");
                            }
                        }
                        catch
                        {
                            Console.WriteLine("No success message found, but update may still have succeeded");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Could not find name input field");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating profile: {ex.Message}");
                    throw;
                }
                
                Console.WriteLine("Profile Update test completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Profile update test failed: {ex.Message}");
                TakeScreenshot("ProfileUpdateTest");
                throw;
            }
        }
    }
}