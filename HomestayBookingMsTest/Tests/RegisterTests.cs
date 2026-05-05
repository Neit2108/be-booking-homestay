// using OpenQA.Selenium.Chrome;
// using OpenQA.Selenium.Support.UI;
// using OpenQA.Selenium;
// using HomestayBookingMsTest.Pages;

// namespace HomestayBookingMsTest.Tests;
// [TestClass]
// public class RegisterTests
// {
//     private IWebDriver driver;

//     [TestInitialize]
//     public void SetUp()
//     {
//         driver = new ChromeDriver();
//         driver.Navigate().GoToUrl("http://localhost:5173/register");
//     }

//     [TestMethod]
//     public void Register_With_Valid_Data_Should_NavigateToLoginPage()
//     {
//         string uniqueEmail = $"test{DateTime.Now.Ticks}@mail.com";

//         var registerPage = new RegisterPage(driver);
//         registerPage.FillForm("Test User", uniqueEmail, "StrongPass123", "012345678901", "0912345678", "Hanoi", "testuser");
//         registerPage.Submit();

//         // Chờ và kiểm tra xem có thông báo thành công hay không
//         var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
//         var loginTitle = wait.Until(drv => drv.FindElement(By.CssSelector("h2.loginTitle"))); // Giả sử có title login ở trang đăng nhập

//         Assert.IsTrue(loginTitle.Text.Contains("Đăng nhập"), "Không quay lại trang đăng nhập sau khi đăng ký thành công.");
//     }


//     [TestMethod]
//     public void Register_With_Invalid_Data_Should_ShowErrorModal()
//     {
//         // Sử dụng email không hợp lệ hoặc thiếu thông tin
//         var registerPage = new RegisterPage(driver);
//         registerPage.FillForm("Test User", "", "weak", "012345678901", "0912345678", "Hanoi", "testuser");
//         registerPage.Submit();

//         // Chờ và kiểm tra xem modal lỗi có xuất hiện không
//         var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
//         var errorMessage = wait.Until(drv => drv.FindElement(By.CssSelector("register-error"))); // Giả sử có class errorMessage cho lỗi

//         Assert.IsTrue(errorMessage.Displayed, "Không hiển thị thông báo lỗi khi đăng ký thất bại.");
//     }

//     [TestCleanup]
//     public void TearDown() => driver.Quit();
// }

using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using HomestayBookingMsTest.Pages;
using SeleniumExtras.WaitHelpers;

namespace HomestayBookingMsTest.Tests;

[TestClass]
public class RegisterTests
{
    private IWebDriver driver;
    private WebDriverWait wait;
    public TestContext TestContext { get; set; }

    [TestInitialize]
    public void SetUp()
    {
        var options = new ChromeOptions();
        // options.AddArgument("--headless"); // Bỏ comment nếu muốn chạy ẩn

        driver = new ChromeDriver(options);
        driver.Manage().Window.Maximize();
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        driver.Navigate().GoToUrl("http://localhost:5173/register");
    }

    [TestMethod]
    public void Register_With_Valid_Data_Should_NavigateToLoginPage()
    {
        string uniqueEmail = $"test{DateTime.Now.Ticks}@mail.com";

        var registerPage = new RegisterPage(driver);
        registerPage.FillForm("Test User", uniqueEmail, "StrongPass123!", "012345678901", "0912345678", "Hanoi", "testuser");
        registerPage.Submit();
        Thread.Sleep(3000); // Tạm chờ 3s để modal kịp render
        TestContext.WriteLine("Đã nhấn nút Đăng ký");
        TestContext.WriteLine(driver.Url);
        // Kiểm tra URL hiện tại
        Assert.AreEqual("http://localhost:5173/login", driver.Url);
        Assert.IsTrue(driver.Url.Contains("login"), "URL không chứa 'login' sau khi đăng ký thành công.");


        // Chờ modal thành công hiển thị
        //var modal = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("[data-testid='register-success-modal']")));
        var modal = wait.Until(drv =>
        {
            var element = drv.FindElement(By.CssSelector("[data-testid='register-success-modal']"));
            return (element.Displayed && element.Enabled) ? element : null;
        });


        // Giả sử trong modal có nút "Đóng" có testid
        // var closeBtn = modal.FindElement(By.CssSelector("[data-testid='success-close-button']")); 
        // closeBtn.Click();
        var closeButton = modal.FindElement(By.CssSelector("[data-testid='success-close-button']"));
        closeButton.Click();

        // Chờ điều hướng tới trang login
        // var loginTitle = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("h2.loginTitle")));

        // Assert.IsTrue(loginTitle.Text.Contains("Đăng nhập"), "Không quay lại trang đăng nhập sau khi đăng ký thành công.");
        var loginTitle = wait.Until(driver => driver.FindElement(
            By.CssSelector("h2.loginTitle")));

        Assert.IsTrue(loginTitle.Text.Contains("Đăng nhập"));
    }

    [TestMethod]
    public void Register_With_Invalid_Data_Should_ShowErrorModal()
    {
        var registerPage = new RegisterPage(driver);
        registerPage.FillForm("Test User", "", "weak", "012345678901", "0912345678", "Hanoi", "testuser");
        registerPage.Submit();

        // Chờ modal lỗi xuất hiện
        //var errorModal = wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='register-error-modal']")));
        var errorModal = wait.Until(drv =>
        {
            var element = drv.FindElement(By.CssSelector("[data-testid='register-error-modal']"));
            return (element.Displayed && element.Enabled) ? element : null;
        });

        Assert.IsTrue(errorModal.Displayed, "Không hiển thị modal lỗi khi đăng ký thất bại.");
    }

    [TestCleanup]
    public void TearDown() => driver.Quit();
}
