using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using HomestayBookingMsTest.Pages;
using System;
using System.Threading;
using OpenQA.Selenium.Support.UI;


namespace HomestayBookingMsTest.Tests
{
    [TestClass]
    public class LoginTests
    {
        private IWebDriver driver;
        private LoginPage loginPage;

        [TestInitialize]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Manage().Window.Maximize();

            loginPage = new LoginPage(driver);
        }

        [TestMethod]
        public void Login_With_Invalid_Credentials_Shows_ErrorModal()
        {
            loginPage.GoTo();
            loginPage.EnterUsername("wrong@example.com");
            loginPage.EnterPassword("wrongpassword");
            loginPage.ClickLogin();

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var modal = wait.Until(drv =>
            {
                var element = drv.FindElement(By.CssSelector("[data-testid='login-error-modal']"));
                return (element.Displayed && element.Enabled) ? element : null;
            });

            Assert.IsTrue(modal.Text.Contains("Đăng nhập thất bại"), "Modal lỗi không hiển thị");
        }

        [TestMethod]
        public void Login_With_Valid_Credentials_NavigatesToHomePage()
        {
            loginPage.GoTo();
            loginPage.EnterUsername("linh.123");
            loginPage.EnterPassword("Linh@123");
            loginPage.ClickLogin();

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var homeElement = wait.Until(drv => drv.FindElement(By.CssSelector("[data-testid='home-page']")));

            Assert.IsTrue(homeElement.Displayed, "Không chuyển đến trang chủ sau khi đăng nhập thành công.");
        }

        [TestCleanup]
        public void TearDown()
        {
            driver.Quit();
        }
    }
}
