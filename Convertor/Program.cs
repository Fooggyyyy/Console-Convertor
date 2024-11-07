using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Threading.Tasks.Sources;
using System.Threading.Tasks.Dataflow;

public static class Programm
{
    private static readonly string ApiKey = "b9f22aa72cff35aebeb487d8e64638c3";
    private static readonly string BaseUrl = "http://api.exchangeratesapi.io/v1/latest";


    static async Task Main(string[] args)
    {
        Console.WriteLine("Введите исходную валюту:");
        string fromCurrency = Console.ReadLine().ToUpper();

        Console.WriteLine("Введите целевую валюту:");
        string toCurrency = Console.ReadLine().ToUpper();

        Console.WriteLine("Введите сумму для конвертации:");
        if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
        {
            Console.WriteLine("Некорректная сумма.");
            return;
        }

        try
        {
            decimal convertedAmount = await ConvertCurrency(fromCurrency, toCurrency, amount);
            Console.WriteLine($"{amount} {fromCurrency} = {convertedAmount} {toCurrency}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static async Task<decimal> ConvertCurrency(string fromCurrency, string toCurrency, decimal amount)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = $"{BaseUrl}?access_key={ApiKey}&symbols={fromCurrency},{toCurrency}";

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseBody);

            if (json["rates"]?[fromCurrency] == null || json["rates"]?[toCurrency] == null)
            {
                throw new Exception("Одна из указанных валют не найдена.");
            }

            decimal fromRate = (decimal)json["rates"][fromCurrency];
            decimal toRate = (decimal)json["rates"][toCurrency];

            decimal eurAmount = amount / fromRate;  
            return eurAmount * toRate;           
        }
    }

}