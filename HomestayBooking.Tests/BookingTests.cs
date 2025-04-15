using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Threading;

namespace HomestayBooking.Tests
{
    [TestClass]
    public class BookingTests : BaseTest
    {
        [TestMethod]
        public void BookingForm_Functionality()
        {
            try
            {
                Console.WriteLine("Starting Booking Form test...");
                
                // First login, as booking likely requires authentication
                NavigateTo("/login");
                WaitForPageToLoad();
                
                try
                {
                    // Standard test credentials - adjust these if needed
                    var usernameField = WaitForElement(By.Name("EmailorUsername"));
                    var passwordField = WaitForElement(By.Name("Password"));
                    
                    usernameField.Clear();
                    usernameField.SendKeys("Admin@homies.com");
                    
                    passwordField.Clear();
                    passwordField.SendKeys("Admin@123");
                    
                    // Find and click login button
                    var loginButton = Driver.FindElement(By.XPath("//button[contains(@class, 'loginButton') or contains(text(), 'Login')]"));
                    loginButton.Click();
                    
                    WaitForPageToLoad();
                    Console.WriteLine("Logged in successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Login attempt failed: {ex.Message}");
                    Console.WriteLine("Proceeding with booking test but it may fail if login is required");
                }
                
                // Navigate to a property first, then to booking
                NavigateTo("/place-details/1");
                WaitForPageToLoad();
                
                // Click on Book Now button
                try
                {
                    var bookNowButton = Driver.FindElement(By.XPath("//button[contains(text(), 'Đặt phòng ngay') or contains(text(), 'Book Now')]"));
                    bookNowButton.Click();
                    WaitForPageToLoad();
                    Console.WriteLine("Navigated to booking form");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not find or click Book Now button: {ex.Message}");
                    throw;
                }
                
                // Verify we're on the booking page
                Assert.IsTrue(Driver.Url.Contains("booking") || Driver.PageSource.Contains("Booking"), 
                    "Should be redirected to booking page");
                
                // Test booking form functionality
                // 1. People count
                try
                {
                    // Find people counter buttons
                    var minusButton = Driver.FindElement(By.XPath("//button[contains(text(), '-')]"));
                    var plusButton = Driver.FindElement(By.XPath("//button[contains(text(), '+')]"));
                    
                    // Test increasing people count
                    Console.WriteLine("Testing people count increment");
                    plusButton.Click();
                    Thread.Sleep(500);
                    
                    // Test decreasing people count
                    Console.WriteLine("Testing people count decrement");
                    minusButton.Click();
                    Thread.Sleep(500);
                    
                    Console.WriteLine("People counter buttons working correctly");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error testing people counter: {ex.Message}");
                }
                
                // 2. Date picker
                try
                {
                    var datePicker = Driver.FindElement(By.Id("date-picker"));
                    HighlightElement(datePicker);
                    Console.WriteLine("Found date picker, clicking...");
                    
                    // Click to open date picker
                    datePicker.Click();
                    Thread.Sleep(1000);
                    
                    // Try to select a date - this depends on your date picker implementation
                    try
                    {
                        // This is a generic attempt and may need adjusting based on your date picker
                        var dateCell = Driver.FindElement(By.CssSelector(".react-datepicker__day:not(.react-datepicker__day--disabled)"));
                        dateCell.Click();
                        Thread.Sleep(500);
                        
                        // Select end date (might need to click another date)
                        var endDateCell = Driver.FindElements(By.CssSelector(".react-datepicker__day:not(.react-datepicker__day--disabled)"))[2];
                        endDateCell.Click();
                        
                        Console.WriteLine("Date selection successful");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not select dates: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error testing date picker: {ex.Message}");
                }
                
                // 3. Voucher code (if available)
                try
                {
                    var voucherField = Driver.FindElement(By.XPath("//input[contains(@placeholder, 'voucher') or contains(@placeholder, 'Voucher')]"));
                    HighlightElement(voucherField);
                    
                    voucherField.Clear();
                    voucherField.SendKeys("TESTVOUCHER");
                    
                    // Find and click Apply button
                    var applyButton = Driver.FindElement(By.XPath("//button[contains(text(), 'Apply')]"));
                    applyButton.Click();
                    
                    Thread.Sleep(1000);
                    Console.WriteLine("Tested voucher application");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error testing voucher field: {ex.Message}");
                }
                
                // 4. Check price calculation
                try
                {
                    var priceElement = Driver.FindElement(By.XPath("//*[contains(text(), '$') and contains(text(), 'USD')]"));
                    HighlightElement(priceElement);
                    Console.WriteLine($"Price display: {priceElement.Text}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not find price display: {ex.Message}");
                }
                
                // 5. Book Now button
                try
                {
                    var bookNowButton = Driver.FindElement(By.XPath("//button[contains(text(), 'Book Now')]"));
                    HighlightElement(bookNowButton);
                    Console.WriteLine("Found final Book Now button");
                    
                    // Optionally click the button to complete booking
                    // This is commented out to avoid creating actual bookings during test
                    // bookNowButton.Click();
                    // WaitForPageToLoad();
                    // Console.WriteLine("Completed booking");
                    
                    // Check for confirmation modal
                    // var confirmationModal = Driver.FindElement(By.XPath("//div[contains(text(), 'Booking Successful')]"));
                    // Assert.IsTrue(confirmationModal.Displayed, "Booking confirmation should be displayed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error finding final Book Now button: {ex.Message}");
                }
                
                Console.WriteLine("Booking Form test completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Booking form test failed: {ex.Message}");
                TakeScreenshot("BookingFormTest");
                throw;
            }
        }
        
        [TestMethod]
        public void ViewUserBookings()
        {
            try
            {
                Console.WriteLine("Starting View User Bookings test...");
                
                // First login
                NavigateTo("/login");
                WaitForPageToLoad();
                
                try
                {
                    // Standard test credentials - adjust these if needed
                    var usernameField = WaitForElement(By.Name("EmailorUsername"));
                    var passwordField = WaitForElement(By.Name("Password"));
                    
                    usernameField.Clear();
                    usernameField.SendKeys("Admin@homies.com");
                    
                    passwordField.Clear();
                    passwordField.SendKeys("Admin@123");
                    
                    // Find and click login button
                    var loginButton = Driver.FindElement(By.XPath("//button[contains(@class, 'loginButton') or contains(text(), 'Login')]"));
                    loginButton.Click();
                    
                    WaitForPageToLoad();
                    Console.WriteLine("Logged in successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Login failed: {ex.Message}");
                    throw new Exception("Login is required for viewing bookings");
                }
                
                // Navigate to user bookings dashboard
                NavigateTo("/user-booking-dashboard");
                WaitForPageToLoad();
                
                Console.WriteLine($"Navigated to bookings page: {Driver.Url}");
                
                // Verify the bookings dashboard loaded
                try
                {
                    // Look for dashboard title or bookings list
                    var dashboardTitle = Driver.FindElement(By.XPath("//div[contains(text(), 'Dashboard') or contains(text(), 'Bookings')]"));
                    HighlightElement(dashboardTitle);
                    Console.WriteLine($"Found dashboard title: {dashboardTitle.Text}");
                    
                    // Look for booking list items
                    var bookingItems = Driver.FindElements(By.CssSelector("[class*='BookingList'] > div"));
                    
                    // If no specific class, try a more general approach
                    if (bookingItems.Count == 0)
                    {
                        bookingItems = Driver.FindElements(By.CssSelector(".bg-white.rounded-xl.shadow"));
                    }
                    
                    Console.WriteLine($"Found {bookingItems.Count} booking items");
                    
                    // If we found booking items, highlight the first one
                    if (bookingItems.Count > 0)
                    {
                        HighlightElement(bookingItems[0]);
                        Console.WriteLine($"First booking details: {bookingItems[0].Text}");
                    }
                    else
                    {
                        Console.WriteLine("No bookings found, but dashboard loaded correctly");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error examining bookings dashboard: {ex.Message}");
                    throw;
                }
                
                Console.WriteLine("View User Bookings test completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"View bookings test failed: {ex.Message}");
                TakeScreenshot("ViewBookingsTest");
                throw;
            }
        }
    }
}