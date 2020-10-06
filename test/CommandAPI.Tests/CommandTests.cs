using System;
using CommandAPI.Models;
using Xunit;

namespace CommandAPI.Tests
{
    public class CommandTests : IDisposable
    {
        private Command testCommand;

        public CommandTests()
        {
            var testCommand = new Command
            {
                HowTo = "Do something",
                Platform = "xUnit",
                CommandLine = "dotnet test"
            };  
        }
        
        [Fact]
        public void CanChangeHowTo()
        {
            // Act
            testCommand.HowTo = "Execute Unit Tests";
            Assert.Equal("Execute Unit Tests", testCommand.HowTo);
        }
        
        [Fact]
        public void CanChangeCommandLine()
        {
            // Act
            testCommand.CommandLine = "dotnet test";
            Assert.Equal("dotnet test", testCommand.CommandLine);
        }
        
        

        public void Dispose()
        {
            testCommand = null;
        }
    }
}