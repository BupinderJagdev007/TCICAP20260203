using Moq;

namespace TPICAP.Tests
{
    /*
     These tests do not cater to testing multi threading use 
     These tests also assume that above a threshold we do not raise a sell, 
     No tests written to cater for sell.

     
     */

    public class OrderTests
    {
        const decimal threshold = 500;
        const string code = "VOD.L";
        const decimal price1 = 100;
        const decimal price2 = 200;
        const decimal price6 = 600;


        [Fact]
        public void OrderTest_UnderThreshold_ExecutesBuyOnceCorrectly()
        {
            // Arrange
            var orderServiceMock = new Mock<IOrderService>();
            var sut = new Order(orderServiceMock.Object, threshold);

            // Act
            sut.RespondToTick(code, price1);

            // Assert
            orderServiceMock.Verify(e => e.Buy(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()), 
                Times.AtMostOnce);

        }

        [Fact]
        public void OrderTest_OverThreshold_ExecutesNoBuy()
        {
            // Arrange
            var orderServiceMock = new Mock<IOrderService>();
            var sut = new Order(orderServiceMock.Object, threshold);

            // Act
            sut.RespondToTick(code, price6);

            // Assert
            orderServiceMock.Verify(e => e.Buy(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()),
                Times.Never);

        }

        [Fact]
        public void OrderTest_TwoUnderThreshold_SecondExecuteDoesNothing()
        {
            // Arrange
            var orderServiceMock = new Mock<IOrderService>();
            var sut = new Order(orderServiceMock.Object, threshold);

            // Act
            sut.RespondToTick(code, price1);
            sut.RespondToTick(code, price2);

            // Assert
            orderServiceMock.Verify(e => e.Buy(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()),
                Times.AtMostOnce);

        }

        [Fact]
        public void OrderTest_AboveThenBelowThreshold_OnlyFiresBuyForSecond()
        {
            PlacedEventArgs? capturedEvent = null;
            // Arrange
            var orderServiceMock = new Mock<IOrderService>();
            var sut = new Order(orderServiceMock.Object, threshold);

            sut.Placed += (PlacedEventArgs e) => { capturedEvent = e; };

            // Act
            sut.RespondToTick(code, price6);
            sut.RespondToTick(code, price2);

            // Assert
            orderServiceMock.Verify(e => e.Buy(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()),
                Times.AtMostOnce);

            Assert.NotNull(capturedEvent);
            Assert.Equal(code, capturedEvent.Code);
            Assert.Equal(price2, capturedEvent.Price);

        }

        [Fact]
        public void OrderTest_BuyThresholdThrowException_ExecuteButRaisesErroredEventCorrectly()
        {
            const string anyExceptionMessage = "Throw Any Exception for test";
            ErroredEventArgs? capturedError = null;
            // Arrange
            var orderServiceMock = new Mock<IOrderService>();
            var sut = new Order(orderServiceMock.Object, threshold);

            orderServiceMock
                .Setup(e => e.Buy(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()))
                .Throws(new Exception(anyExceptionMessage));

            sut.Errored += (ErroredEventArgs e) => { capturedError = e; };

            // Act
            sut.RespondToTick(code, price1);

            // Assert
            orderServiceMock.Verify(e => e.Buy(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()),
                Times.AtMostOnce);

            Assert.NotNull(capturedError);
            Assert.Equal(code, capturedError.Code);
            Assert.Equal(price1, capturedError.Price);
            Assert.Equal(anyExceptionMessage, capturedError.GetException().Message);

        }

        [Fact]
        public void OrderTest_ExceptionPreventsSubsequentReuse()
        {
            const string anyExceptionMessage = "Throw Any Exception for test";
            ErroredEventArgs? capturedError = null;
            PlacedEventArgs? capturedPlaced = null;

            // Arrange
            var orderServiceMock = new Mock<IOrderService>();
            var sut = new Order(orderServiceMock.Object, threshold);

            orderServiceMock
                .Setup(e => e.Buy(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()))
                .Throws(new Exception(anyExceptionMessage));

            sut.Errored += (ErroredEventArgs e) => { capturedError = e; };
            sut.Placed += (PlacedEventArgs e) => { capturedPlaced = e; };

            // Act
            sut.RespondToTick(code, price1);
            sut.RespondToTick(code, price2);

            // Assert
            orderServiceMock.Verify(e => e.Buy(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()),
                Times.AtMostOnce);

            Assert.NotNull(capturedError); // excception fires first
            Assert.Null(capturedPlaced); // not fired because instance already threw error

        }


    }
}
