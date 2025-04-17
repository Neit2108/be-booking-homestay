using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using HomestayBookingMsTest.Pages;

namespace HomestayBookingMsTest.Tests;
[TestClass]
public class RegisterTests
{
    private IWebDriver driver;

    [TestInitialize]
    public void SetUp()
    {
        driver = new ChromeDriver();
        driver.Navigate().GoToUrl("http://localhost:5173/register");
    }

    [TestMethod]
    public void Register_With_Valid_Data_Should_NavigateToLoginPage()
    {
        string uniqueEmail = $"test{DateTime.Now.Ticks}@mail.com";

        var registerPage = new RegisterPage(driver);
        registerPage.FillForm("Test User", uniqueEmail, "StrongPass123", "012345678901", "0912345678", "Hanoi", "testuser");
        registerPage.Submit();

        // Chờ và kiểm tra xem có thông báo thành công hay không
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        var loginTitle = wait.Until(drv => drv.FindElement(By.CssSelector("h2.loginTitle"))); // Giả sử có title login ở trang đăng nhập

        Assert.IsTrue(loginTitle.Text.Contains("Đăng nhập"), "Không quay lại trang đăng nhập sau khi đăng ký thành công.");
    }


    [TestMethod]
    public void Register_With_Invalid_Data_Should_ShowErrorModal()
    {
        // Sử dụng email không hợp lệ hoặc thiếu thông tin
        var registerPage = new RegisterPage(driver);
        registerPage.FillForm("Test User", "", "weak", "012345678901", "0912345678", "Hanoi", "testuser");
        registerPage.Submit();

        // Chờ và kiểm tra xem modal lỗi có xuất hiện không
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        var errorMessage = wait.Until(drv => drv.FindElement(By.CssSelector(".errorMessage"))); // Giả sử có class errorMessage cho lỗi

        Assert.IsTrue(errorMessage.Displayed, "Không hiển thị thông báo lỗi khi đăng ký thất bại.");
    }

    [TestCleanup]
    public void TearDown() => driver.Quit();
}
