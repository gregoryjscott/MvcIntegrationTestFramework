First commit came straight from the demo download at Steve Sanderson http://blog.stevensanderson.com/2009/06/11/integration-testing-your-aspnet-mvc-application/.  I then borrowed some ideas from the Chris Ortman version (http://github.com/chrisortman/MvcIntegrationTest), but kept the same API feel as the original framework.

It's still a work in progress, but it mostly works.  You have to do the post build steps that Steve Sanderson describes in his blog.  Also, it assumes that your MVC application project directory and test project directory are side by side.

/MvcApplication - mvc app
/MvcApplication.Tests - contains integration tests

Example test:

AppHost.Simulate("MyMvcApp").Start(browsingSession =>
{
    var loginResult = browsingSession.Post("Users/Login/", new { UserName = "aaa", Password = "bbb" });
    Assert.That(loginResult.Response.StatusCode, Is.EqualTo(200));

    var result = browsingSession.Post("Money/Create/", new { Amount = "1,000,000" });
    Assert.That(result.Response.StatusCode, Is.EqualTo(200));
});


