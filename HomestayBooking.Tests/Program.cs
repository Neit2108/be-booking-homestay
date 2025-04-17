//using OpenQA.Selenium;
//using OpenQA.Selenium.Chrome;
//using System;
//using System.Threading;

//class Program
//{
//    static void Main(string[] args)
//    {
//        using IWebDriver driver = new ChromeDriver();

//        // Truy cập trang web ReactJS
//        driver.Navigate().GoToUrl("http://localhost:5173/login");

//        // Tìm và nhập email
//        var emailInput = driver.FindElement(By.Id("email")); // hoặc By.Name, By.XPath
//        emailInput.SendKeys("test@example.com");

//        // Tìm và nhập mật khẩu
//        var passwordInput = driver.FindElement(By.Id("password"));
//        passwordInput.SendKeys("your_password");

//        // Submit form (click nút login)
//        var loginButton = driver.FindElement(By.CssSelector("button[type='submit']"));
//        loginButton.Click();

//        // Chờ chuyển hướng và kiểm tra có thành công không
//        Thread.Sleep(3000); // hoặc dùng WebDriverWait để tốt hơn

//        Console.WriteLine("Current URL: " + driver.Url);

//        // Sau đó có thể tiếp tục test điều hướng, nhấn menu, thêm sản phẩm,...
//        driver.Quit();
//    }
//}
