using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SoilAPI.Models;
using System;
using UnitTests.Helpers;

namespace SoilAPI.Tests
{
    [TestFixture]
    public class UploadEndpointTests
    {
        [Test]
        public async Task Upload_SoilRecordIsNull_ReturnsBadRequest()
        {

            await using var context = new MockDb().CreateDbContext();

            // Act
            var result = (BadRequest<string>) await SoilServices.Upload(context, null);

            //Assert
            Assert.That(result.Value, Is.EqualTo("Soil record is null."));

            //var notFoundResult = (NotFound) result.Result;

            //Assert.NotNull(notFoundResult);
        }
    }
}