using OpenQA.Selenium;

namespace HomestayBookingMsTest.Pages
{
    public class LoginPage
    {
        private readonly IWebDriver driver;

        // Constructor
        public LoginPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        // Elements
        private IWebElement LoginButton => driver.FindElement(By.CssSelector("[data-testid='login-button']"));
        private IWebElement EmailInput => driver.FindElement(By.CssSelector("[data-testid='email-input']"));
        private IWebElement PasswordInput => driver.FindElement(By.CssSelector("[data-testid='password-input']"));
        private IWebElement ModalMessage => driver.FindElement(By.ClassName("modal-message")); // tùy vào component bạn dùng

        // Actions
        public void GoTo()
        {
            driver.Navigate().GoToUrl("http://localhost:5173/login");
        }

        public void EnterUsername(string username)
        {
            EmailInput.Clear();
            EmailInput.SendKeys(username);
        }

        public void EnterPassword(string password)
        {
            PasswordInput.Clear();
            PasswordInput.SendKeys(password);
        }

        public void ClickLogin()
        {
            LoginButton.Click();
        }

        public bool IsLoginFailedModalVisible()
        {
            try
            {
                return ModalMessage.Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public string GetModalText()
        {
            return ModalMessage.Text;
        }
    }
}
