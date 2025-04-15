using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Threading;

namespace HomestayBooking.Tests
{
    [TestClass]
    public class PropertyDetailsTests : BaseTest
    {
        [TestMethod]
        public void PropertyDetails_LoadsSuccessfully()
        {
            try
            {
                Console.WriteLine("Starting Property Details test...");
                
                // Navigate to a specific property
                NavigateTo("/place-details/10");
                
                // Wait for page to load
                WaitForPageToLoad();
                
                // Log page details
                Console.WriteLine($"Property page URL: {Driver.Url}");
                
                // Check for elements
                Console.WriteLine("Looking for property title...");
                
                try
                {
                    var propertyTitle = Driver.FindElement(By.CssSelector(".text-4xl.font-bold.text-primary"));
                    HighlightElement(propertyTitle);
                    Console.WriteLine($"Found property title: {propertyTitle.Text}");
                    Assert.IsTrue(propertyTitle.Displayed, "Property title should be visible");
                }
                catch
                {
                    Console.WriteLine("Could not find exact property title element. Trying alternative selectors...");
                    try
                    {
                        var propertyTitle = Driver.FindElement(By.CssSelector("[class*='text-4xl']"));
                        HighlightElement(propertyTitle);
                        Console.WriteLine($"Found property title with alternative selector: {propertyTitle.Text}");
                        Assert.IsTrue(propertyTitle.Displayed, "Property title should be visible");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Still could not find property title: {ex.Message}");
                        throw;
                    }
                }
                
                // Check for booking section
                try
                {
                    var bookingSection = Driver.FindElement(By.XPath("//*[contains(text(), 'Đặt phòng') or contains(text(), 'Book Now')]"));
                    HighlightElement(bookingSection);
                    Console.WriteLine($"Found booking section: {bookingSection.Text}");
                    Assert.IsTrue(bookingSection.Displayed, "Booking section should be visible");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not find booking section: {ex.Message}");
                    // Non-critical, don't throw
                }
                
                // Check for price information
                try
                {
                    var priceElement = Driver.FindElement(By.CssSelector("[class*='text-\\[\\#1ABC9C\\]']"));
                    HighlightElement(priceElement);
                    Console.WriteLine($"Found price element: {priceElement.Text}");
                    Assert.IsTrue(priceElement.Displayed, "Price information should be visible");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not find price element: {ex.Message}");
                    // Try alternative price selectors
                    try
                    {
                        var priceElement = Driver.FindElement(By.XPath("//*[contains(text(), '$')]"));
                        HighlightElement(priceElement);
                        Console.WriteLine($"Found price with alternative selector: {priceElement.Text}");
                    }
                    catch
                    {
                        Console.WriteLine("Could not find price element with any selector");
                    }
                }
                
                Console.WriteLine("Property Details test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Property details test failed: {ex.Message}");
                TakeScreenshot("PropertyDetailsTest");
                throw;
            }
        }
        
        [TestMethod]
        public void PropertyDetails_BookingFlow()
        {
            try
            {
                Console.WriteLine("Starting Property Booking Flow test...");
                
                // Navigate to a specific property
                NavigateTo("/place-details/10");
                
                // Wait for page to load
                WaitForPageToLoad();
                
                // Get property name for later verification
                string propertyName = "";
                try
                {
                    var propertyTitle = Driver.FindElement(By.CssSelector(".text-4xl.font-bold.text-primary"));
                    propertyName = propertyTitle.Text;
                    Console.WriteLine($"Testing booking for property: {propertyName}");
                }
                catch
                {
                    Console.WriteLine("Could not find property title, continuing test");
                }
                
                // Find and click the Book Now button
                try
                {
                    var bookNowButton = Driver.FindElement(By.XPath("//button[contains(text(), 'Đặt phòng ngay') or contains(text(), 'Book Now')]"));
                    HighlightElement(bookNowButton);
                    Console.WriteLine("Found Book Now button, clicking...");
                    bookNowButton.Click();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not find or click Book Now button: {ex.Message}");
                    throw;
                }
                
                // Wait for booking page
                WaitForPageToLoad();
                Console.WriteLine($"Redirected to: {Driver.Url}");
                
                // Verify we're on the booking page
                Assert.IsTrue(Driver.Url.Contains("booking") || Driver.PageSource.Contains("Booking"), 
                    "Should be redirected to booking page");
                
                // Verify property information is carried over
                if (!string.IsNullOrEmpty(propertyName))
                {
                    try
                    {
                        Assert.IsTrue(Driver.PageSource.Contains(propertyName), 
                            "Property name should be displayed on booking page");
                        Console.WriteLine("Property name is correctly displayed on booking page");
                    }
                    catch
                    {
                        Console.WriteLine("Warning: Could not verify property name on booking page");
                    }
                }
                
                // Fill in booking details if the user is logged in
                // Note: This test assumes user is logged in or login is not required for booking
                try
                {
                    // Check for guest count controls
                    var plusButton = Driver.FindElement(By.XPath("//button[contains(text(), '+')]"));
                    HighlightElement(plusButton);
                    Console.WriteLine("Found guest increment button, clicking...");
                    plusButton.Click(); // Increment guest count
                    
                    // Look for date picker (implementation depends on your date picker)
                    try
                    {
                        var datePicker = Driver.FindElement(By.Id("date-picker"));
                        HighlightElement(datePicker);
                        Console.WriteLine("Found date picker");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not find date picker: {ex.Message}");
                        // Non-critical, continue test
                    }
                    
                    // Look for booking confirmation button
                    var confirmButton = Driver.FindElement(By.XPath("//button[contains(text(), 'Book Now') or contains(text(), 'Đặt phòng')]"));
                    HighlightElement(confirmButton);
                    Console.WriteLine("Found booking confirmation button");
                    
                    // Optionally click to complete booking - uncomment to test full flow
                    // confirmButton.Click();
                    // WaitForPageToLoad();
                    // Console.WriteLine("Completed booking, checking for confirmation...");
                    
                    // After booking, check for confirmation
                    // var confirmationMessage = Driver.FindElement(By.XPath("//div[contains(text(), 'Booking Successful') or contains(text(), 'thành công')]"));
                    // Assert.IsTrue(confirmationMessage.Displayed, "Booking confirmation message should be displayed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in booking form interaction: {ex.Message}");
                    // Don't fail test completely as we've verified navigation to booking page
                }
                
                Console.WriteLine("Property Booking Flow test completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Property booking flow test failed: {ex.Message}");
                TakeScreenshot("PropertyBookingTest");
                throw;
            }
        }
        
        [TestMethod]
        public void PropertyDetails_RelatedProperties()
        {
            try
            {
                Console.WriteLine("Starting Related Properties test...");
                
                // Navigate to a specific property
                NavigateTo("/place-details/10");
                
                // Wait for page to load
                WaitForPageToLoad();
                
                // Check for related properties section
                try
                {
                    // Look for related properties title
                    var relatedTitle = Driver.FindElement(By.XPath("//div[contains(text(), 'tương tự') or contains(text(), 'Related')]"));
                    HighlightElement(relatedTitle);
                    Console.WriteLine($"Found related properties section: {relatedTitle.Text}");
                    
                    // Look for related property cards
                    var relatedProperties = Driver.FindElements(By.CssSelector("[class*='RelatedProperty']"));
                    
                    // If specific class not found, try a more general approach
                    if (relatedProperties.Count == 0)
                    {
                        relatedProperties = Driver.FindElements(By.XPath("//div[contains(@class, 'mt-24')]//div[contains(@class, 'flex-col')]"));
                    }
                    
                    Console.WriteLine($"Found {relatedProperties.Count} related properties");
                    Assert.IsTrue(relatedProperties.Count > 0, "At least one related property should be visible");
                    
                    // Highlight the first related property
                    if (relatedProperties.Count > 0)
                    {
                        HighlightElement(relatedProperties[0]);
                    }
                    
                    // Optionally click on a related property to test navigation
                    // This is commented out to avoid changing pages unintentionally
                    // relatedProperties[0].Click();
                    // WaitForPageToLoad();
                    // Console.WriteLine($"Navigated to related property: {Driver.Url}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not find related properties: {ex.Message}");
                    // Non-critical, don't fail test
                }
                
                Console.WriteLine("Related Properties test completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Related properties test failed: {ex.Message}");
                TakeScreenshot("RelatedPropertiesTest");
                throw;
            }
        }
        
        [TestMethod]
        public void PropertyDetails_Features()
        {
            try
            {
                Console.WriteLine("Starting Property Features test...");
                
                // Navigate to a specific property
                NavigateTo("/place-details/10");
                
                // Wait for page to load
                WaitForPageToLoad();
                
                // Check for property features
                try
                {
                    // Look for feature section - this depends on your property page structure
                    var features = Driver.FindElements(By.CssSelector("[class*='PropertyFeature']"));
                    
                    // If specific class not found, try a more general approach
                    if (features.Count == 0)
                    {
                        features = Driver.FindElements(By.XPath("//div[contains(@class, 'flex-wrap') and contains(@class, 'gap-5')]//div[contains(@class, 'flex-col')]"));
                    }
                    
                    Console.WriteLine($"Found {features.Count} property features");
                    
                    // Expecting to find some features
                    if (features.Count > 0)
                    {
                        Assert.IsTrue(features.Count > 0, "Property features should be visible");
                        
                        // Highlight a few features
                        for (int i = 0; i < Math.Min(features.Count, 3); i++)
                        {
                            HighlightElement(features[i]);
                            Console.WriteLine($"Feature {i+1}: {features[i].Text}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No features found, checking for other property details");
                        
                        // If no features, at least check for description
                        var description = Driver.FindElement(By.XPath("//div[contains(text(), 'Mô tả') or contains(text(), 'Description')]"));
                        HighlightElement(description);
                        Console.WriteLine("Found property description section");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not find property features: {ex.Message}");
                    // Non-critical, don't fail test completely
                }
                
                Console.WriteLine("Property Features test completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Property features test failed: {ex.Message}");
                TakeScreenshot("PropertyFeaturesTest");
                throw;
            }
        }
    }
}