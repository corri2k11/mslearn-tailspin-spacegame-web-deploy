using System;
using System.Collections;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Support.UI;

//NOTE: HomePageTest will need an IWebDriver member variable
//      "IWebDriver" IS the programming interface to use to 
//      launch a Web Browser and interact with the page content
//
//      NuGet package for each browser installs the driver software
//      in the bin dir, wich is alongside the compiled test code.
//      This tells the concrete driver class where to find the underlying driver code/software
//
//Test Case Data
//**************
//
//-In NUnit you can provide data to your tests in different ways.
//Here we use "TestCase attribute" which takes argumkents that it later passes back to the test method when it runs.
//We can have multiple "TestCase attributes", not only one, that each test a different feature of the app.
//Each "TestCase attribute" produces a test case that's included in the report that appears at the end of the pipeline run.

namespace UITests
{
    //Tell NUnit Testing Framework/Runner to run entire set of tests definned in the class, fixture will be ran multiple times, one for each browser type to test on.-
    [TestFixture("Firefox")]
    public class HomePageTest 
    {
        private IWebDriver _driver;
        private string _browserTest;

        public HomePageTest(string browserTest) 
        {
            this._browserTest = browserTest;
        }

        //Attribute tells NUnit Testing Framework to run this method only one time per test fixture run
        [OneTimeSetUp]
        public void Setup()
        {
            //Need to assign/set our IWebDriver variable to a concrete class instance that implements this interface 
            //and for the browser we're testing on
            var cwd = Environment.CurrentDirectory;  //current dir

            switch (this._browserTest)
            {
                case "Chrome":
                    this._driver = new ChromeDriver(cwd);
                    break;
                case "Firefox":
                    this._driver = new FirefoxDriver(cwd);
                    break;
                case "IE":
                    this._driver = new InternetExplorerDriver(cwd);
                    break;
                default:
                    throw new ArgumentException($"'{this._browserTest}': Unknown browser");
            }

            //Navigate to the SpaceGameWeb Site
            string url = Environment.GetEnvironmentVariable("SITE_URL");
            _driver.Navigate().GoToUrl(url+"/");
        }

        //Test Methods (Based off the manual tests | Good practice to give them descriptive names that exactly/precisely describe what the test does)
        [TestCase("download-btn","pretend-modal")]
        [TestCase("screen-01","screen-modal")]
        [TestCase("profile-1","profile-modal-1")]
        public void ClickLinkById_ShouldDisplayModalById(string linkId, string modalId) 
        {
            //Test Goal:
            //    Verify proper modal window appears after clicking the link
            //Steps/procedure:
            // 1. Locate link by id, then click on it
            // 2. Locate resulting window modal
            // 3. Close the modal
            // 4. Verify modal was displayed correctly/accordingly

            //Skip test if driver could not be loaded. Happens when underlying browser is NOT INSTALLED
            if(this._driver == null) 
            {
                Assert.Ignore();
                return;
            }

            //OPEN MODAL WINDOW
            //*****************

            //Locate element, click it, OPEN modal
            MyClickElement( MyFindElement( By.Id(linkId) ) );
            
            //Locate resulting modal window
            IWebElement modal = MyFindElement( By.Id(modalId) );
            
            //Record whether the modal window was successfully displayed or not
            bool modalDisplayed = (modal != null && modal.Displayed);

            //CLOSE MODAL WINDOW
            //******************
            if(modalDisplayed)
            {
                //Click modal CLOSE button
                MyClickElement( MyFindElement( By.ClassName("close"), modal) );
                
                //Wait for modal window to close and for main page to be clickable again
                MyFindElement( By.TagName("body") );
            }

            //Assert modal window was displayed correctly, otherwise test will fail
            Assert.That( modalDisplayed, Is.True );
        }

        //*******************************************
        //Helper Methods (Callable from test methods)
        //*******************************************

        //Two actions to be repeated over and over throughout the test:
        //Helper methods are general enough to use in almost any test, we can later add more as we need them

        //1. Finding elements on the page by id, returns IWebElement class
        //2. Click found element on the page (such as buttons or links)
        //   Selenium provides ways to programmatically click a button or link 
        //   by using JavaScript via "IJavaScriptExecutor" class
        //   ChromeDriver, FirefoxDriver and InternetExplorerDriver all support/implement the IJavaScriptExecutor interface, so have the same methods.

        //Find
        public IWebElement MyFindElement(By byLocator, IWebElement parentEl = null, int timeout = 10)
        {
            //Wait for specified condition to be true, like a page load event... then lookup element!
            //Use "By" class to locate/find elements, it has methods that allow you to find the element by id, name, html tag, css class, etc.
            return new WebDriverWait(_driver, TimeSpan.FromSeconds(timeout))
                .Until(c => {
                    IWebElement foundEl = null;
                    if(parentEl != null) 
                        foundEl = parentEl.FindElement(byLocator);
                    else
                        foundEl = _driver.FindElement(byLocator);
                    
                    //return true after element is displayed and able to receive user input
                    return (foundEl != null && 
                            foundEl.Displayed &&   //Element has to be displayed and enabled, otherwise return null in Until() method...
                            foundEl.Enabled) ? foundEl : null;
                });
        }

        //Click Found Element
        public void MyClickElement(IWebElement el)
        {
            //IJavaScriptExecutor enables us to execute JS code during the tests.
            //Call "IJavaScriptExecutor.ExecuteScript" method to run the js click() method on the underlying HTML object
            var js = _driver as IJavaScriptExecutor;
            js.ExecuteScript("arguments[0].click();", el);
        }
    }
}