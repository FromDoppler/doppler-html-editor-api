using System.Collections.Generic;

namespace Doppler.HtmlEditorApi.Weather
{
    public interface IWeatherForecastService
    {
        IEnumerable<WeatherForecast> GetForecasts();
    }
}
