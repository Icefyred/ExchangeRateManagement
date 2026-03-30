using ExchangeRateManagement.Domain.ExchangeRate;
using ExchangeRateManagement.Repository.Abstractions;
using ExchangeRateManagement.Service.Abstractions;
using ExchangeRateManagement.Service.ExchangeRateService;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExchangeRateManagement.Tests;

public class ExchangeRateTests
{
    private ExchangeRateService _service;
    private Mock<IExchangeRateRepository> _repositoryMock;
    private Mock<IExchangeRateProvider> _providerMock;
    private Mock<ILogger<ExchangeRateService>> _loggerMock;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IExchangeRateRepository>();
        _providerMock = new Mock<IExchangeRateProvider>();
        _loggerMock = new Mock<ILogger<ExchangeRateService>>();

        _service = new ExchangeRateService(
            _repositoryMock.Object,
            _providerMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task GetRateAsync_WhenNotInRepository_CallsProviderAndSaves()
    {
        // Arrange
        var baseCurrency = "USD";
        var targetCurrency = "EUR";

        _repositoryMock
            .Setup(r => r.GetByCurrencyPairAsync(baseCurrency, targetCurrency))
            .ReturnsAsync((ExchangeRate?)null);

        var fetchedRate = new ExchangeRate
        {
            BaseCurrency = baseCurrency,
            TargetCurrency = targetCurrency,
            Bid = 1.1m,
            Ask = 1.1m,
            LastUpdatedAt = DateTime.UtcNow
        };

        _providerMock
            .Setup(p => p.GetRateAsync(baseCurrency, targetCurrency))
            .ReturnsAsync(fetchedRate);

        // Act
        var result = await _service.GetRateAsync(baseCurrency, targetCurrency);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Bid, Is.EqualTo(1.1m));
        Assert.That(result, Is.EqualTo(fetchedRate));

        _repositoryMock.Verify(r => r.GetByCurrencyPairAsync(baseCurrency, targetCurrency), Times.Once);

        _providerMock.Verify(p => p.GetRateAsync(baseCurrency, targetCurrency), Times.Once);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ExchangeRate>()), Times.Once);

        _repositoryMock.Verify( r => r.UpdateAsync(It.IsAny<ExchangeRate>()), Times.Never);
    }

    [Test]
    public async Task GetRateAsync_WhenInRepository()
    {
        // Arrange
        var baseCurrency = "USD";
        var targetCurrency = "EUR";

        _repositoryMock
            .Setup(r => r.GetByCurrencyPairAsync(baseCurrency, targetCurrency))
            .ReturnsAsync(new ExchangeRate
            {
                BaseCurrency = baseCurrency,
                TargetCurrency = targetCurrency,
                Bid = 1.1m,
                Ask = 1.1m,
                LastUpdatedAt = DateTime.UtcNow
            });

        // Act
        var result = await _service.GetRateAsync(baseCurrency, targetCurrency);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Bid, Is.EqualTo(1.1m));

        _repositoryMock.Verify(r => r.GetByCurrencyPairAsync(baseCurrency, targetCurrency), Times.Once);
        _providerMock.Verify(p => p.GetRateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ExchangeRate>()), Times.Never);
    }

    [Test]
    public async Task GetRateAsync_WhenInRepository_ButDataIsStale()
    {
        // Arrange
        var baseCurrency = "USD";
        var targetCurrency = "EUR";

        _repositoryMock
            .Setup(r => r.GetByCurrencyPairAsync(baseCurrency, targetCurrency))
            .ReturnsAsync(new ExchangeRate
            {
                BaseCurrency = baseCurrency,
                TargetCurrency = targetCurrency,
                Bid = 1.1m,
                Ask = 1.1m,
                LastUpdatedAt = DateTime.UtcNow.AddHours(-1)
            });

        var fetchedRate = new ExchangeRate
        {
            BaseCurrency = baseCurrency,
            TargetCurrency = targetCurrency,
            Bid = 1.2m,
            Ask = 1.2m,
            LastUpdatedAt = DateTime.UtcNow
        };

        _providerMock
            .Setup(p => p.GetRateAsync(baseCurrency, targetCurrency))
            .ReturnsAsync(fetchedRate);
        // Act
        var result = await _service.GetRateAsync(baseCurrency, targetCurrency);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Bid, Is.EqualTo(1.2m));
        Assert.That(result, Is.EqualTo(fetchedRate));

        _repositoryMock.Verify(r => r.GetByCurrencyPairAsync(baseCurrency, targetCurrency), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ExchangeRate>()), Times.Once);
        _providerMock.Verify(p => p.GetRateAsync(baseCurrency, targetCurrency), Times.Once);
    }

    [Test]
    public async Task GetRateAsync_SameCurrency()
    {
        // Arrange
        var baseCurrency = "USD";
        var targetCurrency = "USD";
      
        // Act
        var result = await _service.GetRateAsync(baseCurrency, targetCurrency);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Bid, Is.EqualTo(1));

        _repositoryMock.Verify(r => r.GetByCurrencyPairAsync(baseCurrency, targetCurrency), Times.Never);
    }

    [Test]
    public async Task GetRateAsync_WhenStale_AndProviderFails()
    {
        // Arrange
        var baseCurrency = "USD";
        var targetCurrency = "EUR";

        var existingRate = new ExchangeRate
        {
            BaseCurrency = baseCurrency,
            TargetCurrency = targetCurrency,
            Bid = 1.1m,
            Ask = 1.1m,
            LastUpdatedAt = DateTime.UtcNow.AddHours(-1) // stale
        };

        _repositoryMock
            .Setup(r => r.GetByCurrencyPairAsync(baseCurrency, targetCurrency))
            .ReturnsAsync(existingRate);

        _providerMock
            .Setup(p => p.GetRateAsync(baseCurrency, targetCurrency))
            .ReturnsAsync((ExchangeRate?)null); // simulate failure

        // Act
        var result = await _service.GetRateAsync(baseCurrency, targetCurrency);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Bid, Is.EqualTo(1.1m)); // fallback to old value

        _providerMock.Verify(p => p.GetRateAsync(baseCurrency, targetCurrency), Times.Once);

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<ExchangeRate>()), Times.Never);
    }
}