using Microsoft.AspNetCore.Mvc;
using NovaToolsHub.Models.ViewModels;
using NovaToolsHub.Services;
using NovaToolsHub.Helpers;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace NovaToolsHub.Controllers
{
    public class ToolsController : Controller
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<ToolsController> _logger;

        public ToolsController(ICurrencyService currencyService, ILogger<ToolsController> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        // GET: Tools Dashboard
        public IActionResult Index()
        {
            SetSeoData("All Tools", "Browse all calculators and converters available on NovaTools Hub");
            return View();
        }

        // Unit Converter
        public IActionResult UnitConverter()
        {
            SetSeoData("Unit Converter", "Convert between different units of length, weight, temperature, volume, and more");
            ViewBag.JsonLdSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Unit Converter",
                "Convert between different units of length, weight, temperature, volume, and more. Fast, accurate, and easy to use.",
                $"{Request.Scheme}://{Request.Host}/Tools/UnitConverter",
                "UtilitySoftware"
            );
            return View();
        }

        [HttpPost]
        public IActionResult ConvertUnit([FromBody] UnitConversionRequest request)
        {
            try
            {
                var result = PerformUnitConversion(request.Value, request.FromUnit, request.ToUnit, request.Category);
                return Json(new { success = true, result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting units");
                return Json(new { success = false, error = "Conversion failed. Please check your inputs." });
            }
        }

        // Currency Converter
        public IActionResult CurrencyConverter()
        {
            SetSeoData("Currency Converter", "Convert between major world currencies with live exchange rates");
            ViewBag.JsonLdSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Currency Converter",
                "Convert between major world currencies with live exchange rates. Real-time currency conversion tool.",
                $"{Request.Scheme}://{Request.Host}/Tools/CurrencyConverter",
                "FinanceApplication"
            );
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConvertCurrency([FromBody] CurrencyConversionRequest request)
        {
            try
            {
                var rate = await _currencyService.GetExchangeRateAsync(request.FromCurrency, request.ToCurrency);
                var result = request.Amount * rate;

                return Json(new
                {
                    success = true,
                    result = Math.Round(result, 2),
                    rate = Math.Round(rate, 4),
                    fromCurrency = request.FromCurrency,
                    toCurrency = request.ToCurrency
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting currency");
                return Json(new { success = false, error = "Currency conversion failed. Please try again." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableCurrencies()
        {
            try
            {
                var rates = await _currencyService.GetExchangeRatesAsync("USD");
                var currencies = rates.Keys.OrderBy(c => c).ToList();
                return Json(new { success = true, currencies });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available currencies");
                // Return common currencies as fallback
                var fallback = new[] { "USD", "EUR", "GBP", "JPY", "CNY", "INR", "PKR", "CAD", "AUD", "SAR", "AED", "CHF" };
                return Json(new { success = true, currencies = fallback });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRates()
        {
            try
            {
                var rates = await _currencyService.GetExchangeRatesAsync("USD");
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
                return Json(new
                {
                    success = true,
                    baseCurrency = "USD",
                    rates = rates.ToDictionary(kvp => kvp.Key, kvp => Math.Round(kvp.Value, 6)),
                    timestamp = timestamp,
                    count = rates.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all rates");
                return Json(new { success = false, error = "Failed to fetch rates" });
            }
        }

        // BMI Calculator
        public IActionResult BmiCalculator()
        {
            SetSeoData("BMI Calculator", "Calculate your Body Mass Index (BMI) and understand your health status");
            ViewBag.JsonLdSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "BMI Calculator",
                "Calculate your Body Mass Index (BMI) and understand your health status. Free BMI calculator with metric and imperial units.",
                $"{Request.Scheme}://{Request.Host}/Tools/BmiCalculator",
                "HealthApplication"
            );
            return View();
        }

        [HttpPost]
        public IActionResult CalculateBmi([FromBody] BmiRequest request)
        {
            try
            {
                double heightInMeters = request.HeightUnit == "cm" ? request.Height / 100 : request.Height;
                double weightInKg = request.WeightUnit == "lbs" ? request.Weight * 0.453592 : request.Weight;

                double bmi = weightInKg / (heightInMeters * heightInMeters);
                string category = GetBmiCategory(bmi);
                string advice = GetBmiAdvice(bmi);

                return Json(new
                {
                    success = true,
                    bmi = Math.Round(bmi, 1),
                    category,
                    advice
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating BMI");
                return Json(new { success = false, error = "BMI calculation failed. Please check your inputs." });
            }
        }

        // Age Calculator
        public IActionResult AgeCalculator()
        {
            SetSeoData("Age Calculator", "Calculate your exact age in years, months, days, and more");
            ViewBag.JsonLdSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Age Calculator",
                "Calculate your exact age in years, months, days, hours, and minutes. Precise age calculation tool.",
                $"{Request.Scheme}://{Request.Host}/Tools/AgeCalculator",
                "UtilitySoftware"
            );
            return View();
        }

        [HttpPost]
        public IActionResult CalculateAge([FromBody] AgeRequest request)
        {
            try
            {
                var birthDate = DateTime.Parse(request.BirthDate);
                var targetDate = string.IsNullOrEmpty(request.TargetDate)
                    ? DateTime.Today
                    : DateTime.Parse(request.TargetDate);

                if (birthDate > targetDate)
                {
                    return Json(new { success = false, error = "Birth date cannot be after target date." });
                }

                var age = CalculateDetailedAge(birthDate, targetDate);
                return Json(new { success = true, age });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating age");
                return Json(new { success = false, error = "Age calculation failed. Please check your dates." });
            }
        }

        // Date Calculator
        public IActionResult DateCalculator()
        {
            SetSeoData("Date Calculator", "Calculate date differences and add or subtract days from any date");
            ViewBag.JsonLdSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Date Calculator",
                "Calculate date differences, add or subtract days, and perform date arithmetic.",
                $"{Request.Scheme}://{Request.Host}/Tools/DateCalculator",
                "UtilitySoftware"
            );
            return View();
        }

        // PDF Merge Tool
        public IActionResult PdfMerge()
        {
            SetSeoData("PDF Merge Tool", "Merge multiple PDF files into a single PDF securely in your browser.");
            var url = $"{Request.Scheme}://{Request.Host}/Tools/PdfMerge";

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "PDF Merge Tool",
                "Merge multiple PDF files into a single PDF securely in your browser. No files are uploaded to the server.",
                url,
                "UtilitySoftware"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "Do my PDF files get uploaded to the server when I merge them?",
                    "No. The PDF merge tool runs completely in your browser using JavaScript. Your PDF files are never uploaded to or stored on NovaTools Hub servers."
                ),
                (
                    "Is there a limit to how many PDFs I can merge at once?",
                    "You can add multiple PDF files and rearrange them before merging. Extremely large files or very long documents may be limited by your browser memory."
                ),
                (
                    "Will the quality or security of my PDFs be affected?",
                    "The merged PDF is created by combining the pages of your original files without altering their contents. You should avoid uploading confidential PDFs on shared or public devices."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View();
        }

        [HttpPost]
        public IActionResult CalculateDateDifference([FromBody] DateDifferenceRequest request)
        {
            try
            {
                var startDate = DateTime.Parse(request.StartDate);
                var endDate = DateTime.Parse(request.EndDate);

                if (startDate > endDate)
                {
                    var temp = startDate;
                    startDate = endDate;
                    endDate = temp;
                }

                var difference = CalculateDateDiff(startDate, endDate);
                return Json(new { success = true, difference });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating date difference");
                return Json(new { success = false, error = "Date calculation failed. Please check your inputs." });
            }
        }

        [HttpPost]
        public IActionResult AddSubtractDays([FromBody] DateAddSubtractRequest request)
        {
            try
            {
                var startDate = DateTime.Parse(request.StartDate);
                var resultDate = request.Operation == "add"
                    ? startDate.AddDays(request.Days)
                    : startDate.AddDays(-request.Days);

                return Json(new
                {
                    success = true,
                    resultDate = resultDate.ToString("yyyy-MM-dd"),
                    formattedDate = resultDate.ToString("MMMM dd, yyyy")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in date operation");
                return Json(new { success = false, error = "Date operation failed. Please check your inputs." });
            }
        }

        // Password Generator
        public IActionResult PasswordGenerator()
        {
            SetSeoData("Password Generator", "Generate secure passwords and check password strength with our advanced security tool");
            ViewBag.JsonLdSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Password Generator",
                "Generate secure passwords and check password strength with our advanced security tool.",
                $"{Request.Scheme}://{Request.Host}/Tools/PasswordGenerator",
                "SecurityApplication"
            );
            return View();
        }

        [HttpPost]
        public IActionResult GeneratePassword([FromBody] PasswordGeneratorRequest request)
        {
            try
            {
                var password = GenerateSecurePassword(
                    request.Length,
                    request.IncludeUppercase,
                    request.IncludeLowercase,
                    request.IncludeNumbers,
                    request.IncludeSymbols
                );

                return Json(new { success = true, password });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating password");
                return Json(new { success = false, error = "Password generation failed." });
            }
        }

        // QR Code Generator
        public IActionResult QrCodeGenerator()
        {
            SetSeoData("QR Code Generator", "Generate QR codes for URLs, text, contact info, and more");
            ViewBag.JsonLdSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "QR Code Generator",
                "Generate QR codes for URLs, text, contact info, and more. Free online QR code creator.",
                $"{Request.Scheme}://{Request.Host}/Tools/QrCodeGenerator",
                "UtilitySoftware"
            );
            return View();
        }

        // Color Palette Generator
        public IActionResult ColorPaletteGenerator()
        {
            SetSeoData("Color Palette Generator", "Generate harmonious color schemes, gradients, and export-ready palettes for your next project.");
            ViewBag.JsonLdSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Color Palette Generator",
                "Create designer-friendly color palettes with complementary, analogous, triadic, tetradic, monochromatic, and split-complementary modes.",
                $"{Request.Scheme}://{Request.Host}/Tools/ColorPaletteGenerator",
                "DesignApplication"
            );
            return View();
        }

        // JSON Formatter
        public IActionResult JsonFormatter()
        {
            SetSeoData("JSON Formatter", "Format, validate, and compare JSON payloads with instant insights.");
            ViewBag.JsonLdSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "JSON Formatter",
                "Enterprise-grade JSON formatter with validation, minify, and diff capabilities for payload inspections.",
                $"{Request.Scheme}://{Request.Host}/Tools/JsonFormatter",
                "UtilitySoftware"
            );
            return View();
        }

        // Regex Tester
        public IActionResult RegexTester()
        {
            SetSeoData("Regex Tester", "Test, debug, and visualize regular expressions with live matches and flags.");
            ViewBag.JsonLdSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Regex Tester",
                "Enterprise-grade regex tester with live highlighting, capture groups, replacement previews, and preset patterns.",
                $"{Request.Scheme}://{Request.Host}/Tools/RegexTester",
                "UtilitySoftware"
            );
            return View();
        }

        // Favicon Generator
        public IActionResult FaviconGenerator()
        {
            SetSeoData(
                "Favicon Generator",
                "Create crisp favicons and PWA icons from any logo with enterprise-grade presets."
            );
            ViewBag.JsonLdSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Favicon Generator",
                "Generate favicon.ico files and multi-size PWA icons with safe padding, rounded corners, and gradient backgrounds.",
                $"{Request.Scheme}://{Request.Host}/Tools/FaviconGenerator",
                "DesignApplication"
            );
            return View();
        }

        #region Helper Methods

        private double PerformUnitConversion(double value, string fromUnit, string toUnit, string category)
        {
            // Convert to base unit first, then to target unit
            return category switch
            {
                "length" => ConvertLength(value, fromUnit, toUnit),
                "weight" => ConvertWeight(value, fromUnit, toUnit),
                "temperature" => ConvertTemperature(value, fromUnit, toUnit),
                "volume" => ConvertVolume(value, fromUnit, toUnit),
                "area" => ConvertArea(value, fromUnit, toUnit),
                "speed" => ConvertSpeed(value, fromUnit, toUnit),
                _ => throw new ArgumentException("Invalid category")
            };
        }

        private double ConvertLength(double value, string from, string to)
        {
            // Convert to meters first
            double meters = from switch
            {
                "mm" => value / 1000,
                "cm" => value / 100,
                "m" => value,
                "km" => value * 1000,
                "in" => value * 0.0254,
                "ft" => value * 0.3048,
                "yd" => value * 0.9144,
                "mi" => value * 1609.344,
                _ => throw new ArgumentException("Invalid from unit")
            };

            // Convert from meters to target
            return to switch
            {
                "mm" => meters * 1000,
                "cm" => meters * 100,
                "m" => meters,
                "km" => meters / 1000,
                "in" => meters / 0.0254,
                "ft" => meters / 0.3048,
                "yd" => meters / 0.9144,
                "mi" => meters / 1609.344,
                _ => throw new ArgumentException("Invalid to unit")
            };
        }

        private double ConvertWeight(double value, string from, string to)
        {
            // Convert to kg first
            double kg = from switch
            {
                "mg" => value / 1000000,
                "g" => value / 1000,
                "kg" => value,
                "ton" => value * 1000,
                "oz" => value * 0.0283495,
                "lbs" => value * 0.453592,
                _ => throw new ArgumentException("Invalid from unit")
            };

            // Convert from kg to target
            return to switch
            {
                "mg" => kg * 1000000,
                "g" => kg * 1000,
                "kg" => kg,
                "ton" => kg / 1000,
                "oz" => kg / 0.0283495,
                "lbs" => kg / 0.453592,
                _ => throw new ArgumentException("Invalid to unit")
            };
        }

        private double ConvertTemperature(double value, string from, string to)
        {
            // Convert to Celsius first
            double celsius = from switch
            {
                "C" => value,
                "F" => (value - 32) * 5 / 9,
                "K" => value - 273.15,
                _ => throw new ArgumentException("Invalid from unit")
            };

            // Convert from Celsius to target
            return to switch
            {
                "C" => celsius,
                "F" => celsius * 9 / 5 + 32,
                "K" => celsius + 273.15,
                _ => throw new ArgumentException("Invalid to unit")
            };
        }

        private double ConvertVolume(double value, string from, string to)
        {
            // Convert to liters first
            double liters = from switch
            {
                "ml" => value / 1000,
                "l" => value,
                "m3" => value * 1000,
                "gal" => value * 3.78541,
                "qt" => value * 0.946353,
                "pt" => value * 0.473176,
                "cup" => value * 0.236588,
                "floz" => value * 0.0295735,
                _ => throw new ArgumentException("Invalid from unit")
            };

            // Convert from liters to target
            return to switch
            {
                "ml" => liters * 1000,
                "l" => liters,
                "m3" => liters / 1000,
                "gal" => liters / 3.78541,
                "qt" => liters / 0.946353,
                "pt" => liters / 0.473176,
                "cup" => liters / 0.236588,
                "floz" => liters / 0.0295735,
                _ => throw new ArgumentException("Invalid to unit")
            };
        }

        private double ConvertArea(double value, string from, string to)
        {
            // Convert to square meters first
            double sqm = from switch
            {
                "mm2" => value / 1000000,
                "cm2" => value / 10000,
                "m2" => value,
                "km2" => value * 1000000,
                "in2" => value * 0.00064516,
                "ft2" => value * 0.092903,
                "yd2" => value * 0.836127,
                "ac" => value * 4046.86,
                "ha" => value * 10000,
                _ => throw new ArgumentException("Invalid from unit")
            };

            // Convert from square meters to target
            return to switch
            {
                "mm2" => sqm * 1000000,
                "cm2" => sqm * 10000,
                "m2" => sqm,
                "km2" => sqm / 1000000,
                "in2" => sqm / 0.00064516,
                "ft2" => sqm / 0.092903,
                "yd2" => sqm / 0.836127,
                "ac" => sqm / 4046.86,
                "ha" => sqm / 10000,
                _ => throw new ArgumentException("Invalid to unit")
            };
        }

        private double ConvertSpeed(double value, string from, string to)
        {
            // Convert to m/s first
            double ms = from switch
            {
                "ms" => value,
                "kmh" => value / 3.6,
                "mph" => value * 0.44704,
                "kn" => value * 0.514444,
                _ => throw new ArgumentException("Invalid from unit")
            };

            // Convert from m/s to target
            return to switch
            {
                "ms" => ms,
                "kmh" => ms * 3.6,
                "mph" => ms / 0.44704,
                "kn" => ms / 0.514444,
                _ => throw new ArgumentException("Invalid to unit")
            };
        }

        private string GetBmiCategory(double bmi)
        {
            return bmi switch
            {
                < 18.5 => "Underweight",
                < 25 => "Normal weight",
                < 30 => "Overweight",
                _ => "Obese"
            };
        }

        private string GetBmiAdvice(double bmi)
        {
            return bmi switch
            {
                < 18.5 => "You may want to gain weight. Consult with a healthcare professional.",
                < 25 => "You're at a healthy weight. Keep up the good work!",
                < 30 => "Consider a balanced diet and regular exercise to reach a healthy weight.",
                _ => "It's important to consult with a healthcare professional about weight management."
            };
        }

        private object CalculateDetailedAge(DateTime birthDate, DateTime targetDate)
        {
            int years = targetDate.Year - birthDate.Year;
            int months = targetDate.Month - birthDate.Month;
            int days = targetDate.Day - birthDate.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(targetDate.Year, targetDate.Month - 1);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            var totalDays = (targetDate - birthDate).Days;
            var totalMonths = years * 12 + months;
            var totalWeeks = totalDays / 7;

            return new
            {
                years,
                months,
                days,
                totalDays,
                totalWeeks,
                totalMonths,
                nextBirthday = GetNextBirthday(birthDate, targetDate)
            };
        }

        private string GetNextBirthday(DateTime birthDate, DateTime targetDate)
        {
            var nextBirthday = new DateTime(targetDate.Year, birthDate.Month, birthDate.Day);
            if (nextBirthday < targetDate)
            {
                nextBirthday = nextBirthday.AddYears(1);
            }
            var daysUntil = (nextBirthday - targetDate).Days;
            return $"{daysUntil} days ({nextBirthday:MMMM dd, yyyy})";
        }

        private object CalculateDateDiff(DateTime start, DateTime end)
        {
            var totalDays = (end - start).Days;
            var years = end.Year - start.Year;
            var months = end.Month - start.Month;
            var days = end.Day - start.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(end.Year, end.Month - 1);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            return new
            {
                years,
                months,
                days,
                totalDays,
                totalWeeks = totalDays / 7,
                totalMonths = years * 12 + months
            };
        }

        private string GenerateSecurePassword(int length, bool uppercase, bool lowercase, bool numbers, bool symbols)
        {
            const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
            const string numberChars = "0123456789";
            const string symbolChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            var charSets = new List<string>();
            if (uppercase) charSets.Add(uppercaseChars);
            if (lowercase) charSets.Add(lowercaseChars);
            if (numbers) charSets.Add(numberChars);
            if (symbols) charSets.Add(symbolChars);

            if (charSets.Count == 0)
            {
                charSets.Add(lowercaseChars); // Default to lowercase if nothing selected
            }

            var allChars = string.Join("", charSets);
            var password = new List<char>();

            // Use cryptographically secure random number generator
            using var rng = RandomNumberGenerator.Create();

            // Step 1: Ensure at least one character from each selected type
            foreach (var charSet in charSets)
            {
                var bytes = new byte[1];
                rng.GetBytes(bytes);
                password.Add(charSet[bytes[0] % charSet.Length]);
            }

            // Step 2: Fill remaining length with random characters from all sets
            var remainingLength = length - password.Count;
            if (remainingLength > 0)
            {
                var randomBytes = new byte[remainingLength];
                rng.GetBytes(randomBytes);
                for (int i = 0; i < remainingLength; i++)
                {
                    password.Add(allChars[randomBytes[i] % allChars.Length]);
                }
            }

            // Step 3: Shuffle the password to avoid predictable positions
            var shuffleBytes = new byte[password.Count];
            rng.GetBytes(shuffleBytes);
            var shuffled = password
                .Select((c, i) => new { Char = c, Order = shuffleBytes[i] })
                .OrderBy(x => x.Order)
                .Select(x => x.Char)
                .ToArray();

            return new string(shuffled);
        }


        public IActionResult LoremIpsum()
        {
            SetSeoData(
                "Lorem Ipsum Generator - Placeholder Text Generator",
                "Generate Lorem Ipsum placeholder text for your designs and mockups. Customize paragraphs, words, or lists with classic or modern styles."
            );
            ViewBag.JsonLdSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Lorem Ipsum Generator",
                "Generate placeholder text for mockups, wireframes, and design prototypes with customizable length and format.",
                $"{Request.Scheme}://{Request.Host}/Tools/LoremIpsum",
                "UtilitySoftware"
            );
            return View();
        }

        private void SetSeoData(string title, string description)
        {
            ViewData["PageTitle"] = $"{title} | NovaTools Hub";
            ViewData["MetaDescription"] = description;
            ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}{Request.Path}";
        }

        #endregion
    }
}
