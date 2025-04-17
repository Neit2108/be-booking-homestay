using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomestayBookingMsTest.Pages
{
    public class RegisterPage
    {
        private readonly IWebDriver _driver;

        public RegisterPage(IWebDriver driver) => _driver = driver;

        // Các input field
        public IWebElement FullNameInput => _driver.FindElement(By.Name("FullName"));
        public IWebElement IdentityCardInput => _driver.FindElement(By.Name("IdentityCard"));
        public IWebElement EmailInput => _driver.FindElement(By.Name("Email"));
        public IWebElement PhoneNumberInput => _driver.FindElement(By.Name("PhoneNumber"));
        public IWebElement HomeAddressInput => _driver.FindElement(By.Name("HomeAddress"));
        public IWebElement UsernameInput => _driver.FindElement(By.Name("Username"));
        public IWebElement PasswordInput => _driver.FindElement(By.Name("Password"));
        public IWebElement RegisterButton => _driver.FindElement(By.CssSelector("[data-testid='register-button']"));

        // Các phương thức điền thông tin
        public void FillForm(string fullName, string email, string password, string identityCard, string phoneNumber, string homeAddress, string username)
        {
            FullNameInput.SendKeys(fullName);
            IdentityCardInput.SendKeys(identityCard);
            EmailInput.SendKeys(email);
            PhoneNumberInput.SendKeys(phoneNumber);
            HomeAddressInput.SendKeys(homeAddress);
            UsernameInput.SendKeys(username);
            PasswordInput.SendKeys(password);
        }

        // Phương thức submit form
        public void Submit() => RegisterButton.Click();
    }

}
