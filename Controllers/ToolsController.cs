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

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Unit Converter",
                "Convert between different units of length, weight, temperature, volume, and more. Fast, accurate, and easy to use.",
                $"{Request.Scheme}://{Request.Host}/Tools/UnitConverter",
                "UtilitySoftware"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "What types of units can I convert with this unit converter?",
                    "You can convert between length (millimeters, centimeters, meters, kilometers, inches, feet, yards, miles), weight (milligrams, grams, kilograms, metric tons, ounces, pounds), temperature (Celsius, Fahrenheit, Kelvin), volume (milliliters, liters, cubic meters, gallons, quarts, pints, cups, fluid ounces), area (square millimeters, square centimeters, square meters, square kilometers, square inches, square feet, square yards, acres, hectares), and speed (meters/second, kilometers/hour, miles/hour, knots)."
                ),
                (
                    "How accurate are the unit conversions?",
                    "Conversions are calculated using standard conversion constants and returned to several decimal places. For most everyday and educational use cases this level of precision is more than sufficient, but you should always consult official standards for critical engineering or scientific work."
                ),
                (
                    "Do I need to sign up or upload any files to use the unit converter?",
                    "No account or file upload is required. You simply enter a value, choose the source and target units, and the converter returns the result instantly in your browser."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

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

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Currency Converter",
                "Convert between major world currencies with live exchange rates. Real-time currency conversion tool.",
                $"{Request.Scheme}://{Request.Host}/Tools/CurrencyConverter",
                "FinanceApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "How often are currency rates updated in this converter?",
                    "Exchange rates are fetched from the CurrencyFreaks API and cached for a short period to keep the tool fast and reliable. This is suitable for general reference and quick comparisons, but intraday trading or large transfers should always use rates quoted directly by your bank or payment provider."
                ),
                (
                    "Why does the amount I see here differ from my bank's conversion rate?",
                    "Banks and payment providers typically add fees or markups on top of mid-market rates. Our currency converter uses API rates without additional fees, so the numbers may differ from what you are actually charged."
                ),
                (
                    "Can I use this currency converter for financial or tax decisions?",
                    "This converter is intended for informational purposes only. It is helpful for quick estimates, but you should always rely on official statements or your bank's quoted rates for binding financial, accounting, or tax decisions."
                )
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

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "BMI Calculator",
                "Calculate your Body Mass Index (BMI) and understand your health status. Free BMI calculator with metric and imperial units.",
                $"{Request.Scheme}://{Request.Host}/Tools/BmiCalculator",
                "HealthApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "Is BMI a perfect measure of health for everyone?",
                    "No. BMI is a simple height and weight ratio and does not }

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

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Age Calculator",
                "Calculate your exact age in years, months, days, hours, and minutes. Precise age calculation tool.",
                $"{Request.Scheme}://{Request.Host}/Tools/AgeCalculator",
                "UtilitySoftware"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "How is age calculated in this age calculator?",
                    "The tool calculates age based on full calendar years, months, and days between your date of birth and the selected target date. It also shows total months, weeks, and days, so you can see a detailed breakdown."
                ),
                (
                    "Can I calculate my age on a past or future date?",
                    "Yes. Enter your birth date and a target date in the future or past. The calculator will show how old you were or will be on that specific date, along with a countdown to your next birthday."
                ),
                (
                    "Does the age calculator account for leap years?",
                    "Yes. The calculation uses actual calendar dates, so leap years and varying month lengths are handled automatically. Time zones are not considered because the tool works at the date level, not the hour level."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

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

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Date Calculator",
                "Calculate date differences, add or subtract days, and perform date arithmetic.",
                $"{Request.Scheme}://{Request.Host}/Tools/DateCalculator",
                "UtilitySoftware"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "How does the date difference calculator work?",
                    "The date difference mode compares two calendar dates and returns the elapsed time in years, months, days, total months, total weeks, and total days. If you enter the dates in reverse order, the tool automatically swaps them so the earlier date comes first."
                ),
                (
                    "What does the add/subtract days feature do?",
                    "The add/subtract mode lets you pick a starting date, choose whether to add or subtract, and specify a number of days. The calculator then returns the resulting date, which is useful for deadlines, delivery estimates, and event planning."
                ),
                (
                    "Does the date calculator support business days or time zones?",
                    "This tool currently works with calendar days only and does not exclude weekends or holidays. It also does not adjust for time zones because it operates on dates rather than specific times of day."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

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

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Password Generator",
                "Generate secure passwords and check password strength with our advanced security tool.",
                $"{Request.Scheme}://{Request.Host}/Tools/PasswordGenerator",
                "SecurityApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "How strong are the passwords generated by this tool?",
                    "Generated passwords are created using a cryptographically secure random number generator and can include uppercase letters, lowercase letters, numbers, and symbols. You can also choose longer lengths (for example 16 characters or more) to significantly increase strength."
                ),
                (
                    "Does NovaTools Hub store my generated passwords?",
                    "The generator endpoint creates a password on the server and returns it over an encrypted HTTPS connection. The application does not intentionally persist the generated passwords, but you should still treat them as sensitive secrets and store them in a trusted password manager."
                ),
                (
                    "Is it safe to check an existing password with the strength checker?",
                    "The strength checker uses the zxcvbn library in your browser to analyse the password you type. As a best practice you should avoid pasting very sensitive or long-term production passwords into any website, and instead use this feature with sample or similar passwords when possible."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

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
            var url = Url.Action("QrCodeGenerator", "Tools", null, Request.Scheme) ?? $"{Request.Scheme}://{Request.Host}/Tools/QrCodeGenerator";

            SetSeoData("QR Code Generator", "Generate QR codes for URLs, text, contact info, and more.");

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "QR Code Generator",
                "Generate QR codes for URLs, text, contact info, WiFi, and more. Free online QR code creator.",
                url,
                "UtilitySoftware"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "What can I encode with this QR code generator?",
                    "You can generate QR codes for website links, plain text, email addresses, phone numbers, SMS messages, WiFi network details, and vCard-style contact information."
                ),
                (
                    "Are my QR code contents stored on NovaTools Hub?",
                    "The QR image is rendered in your browser based on the text you enter. The content is not intended to be stored long term on NovaTools Hub servers, but you should still avoid encoding highly sensitive secrets or credentials."
                ),
                (
                    "Will my QR codes keep working forever?",
                    "The QR image itself does not expire, but any links or services it points to might change over time. Always test your QR codes after printing and periodically re-check important codes that link to external pages."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View();
        }

        // Color Palette Generator
        public IActionResult ColorPaletteGenerator()
        {
            var url = Url.Action("ColorPaletteGenerator", "Tools", null, Request.Scheme) ?? $"{Request.Scheme}://{Request.Host}/Tools/ColorPaletteGenerator";

            SetSeoData("Color Palette Generator", "Generate harmonious color schemes, gradients, and export-ready palettes for your next project.");

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Color Palette Generator",
                "Create designer-friendly color palettes with complementary, analogous, triadic, tetradic, monochromatic, and split-complementary modes.",
                url,
                "DesignApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "What can I use these color palettes for?",
                    "The generator is designed for UI themes, branding systems, charts, illustrations, and any project where you need cohesive colors with good contrast and role labels like primary, secondary, and accent."
                ),
                (
                    "Does the tool help with accessibility and contrast?",
                    "Yes. It calculates luminance and contrast ratios between the lightest and darkest colors and surfaces helpful text color suggestions, so you can quickly see whether your palette is likely to meet basic readability guidelines."
                ),
                (
                    "Where are my saved palettes stored?",
                    "Saved palettes are stored locally in your browser using localStorage so they are available on the same device. They are not synced to a server or shared with other users."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View();
        }

        // JSON Formatter
        public IActionResult JsonFormatter()
        {
            SetSeoData("JSON Formatter", "Format, validate, and compare JSON payloads with instant insights.");

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "JSON Formatter",
                "Enterprise-grade JSON formatter with validation, minify, and diff capabilities for payload inspections.",
                $"{Request.Scheme}://{Request.Host}/Tools/JsonFormatter",
                "UtilitySoftware"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "What can I do with the JSON formatter tool?",
                    "You can validate JSON, pretty-print it with consistent indentation, minify it for transport, and compare two JSON documents side by side in diff mode using the built-in Monaco editor."
                ),
                (
                    "Is my JSON data sent anywhere or stored?",
                    "Your JSON is processed in your browser session for formatting and validation and is not intended to be stored long term on NovaTools Hub servers. Avoid pasting highly sensitive production secrets or credentials into any online tool whenever possible."
                ),
                (
                    "Does the formatter change the actual data?",
                    "Formatting and minifying only change whitespace and ordering in most cases—they do not change the semantic content of the JSON object itself. Always review the output before using it in production or committing it to source control."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View();
        }

        // Regex Tester
        public IActionResult RegexTester()
        {
            SetSeoData("Regex Tester", "Test, debug, and visualize regular expressions with live matches and flags.");

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Regex Tester",
                "Enterprise-grade regex tester with live highlighting, capture groups, replacement previews, and preset patterns.",
                $"{Request.Scheme}://{Request.Host}/Tools/RegexTester",
                "UtilitySoftware"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "Which flavor of regular expressions does this tester use?",
                    "The regex tester uses the JavaScript regular expression engine available in your browser. Most common tokens like character classes, quantifiers, groups, and basic lookahead work as in JavaScript, but some features from PCRE or other engines may not be supported."
                ),
                (
                    "What are the global, case-insensitive, multiline, and single-line flags?",
                    "The g flag finds all matches instead of just the first, i ignores case differences, m makes ^ and $ match the start and end of lines, and s lets the dot (.) match newlines as well. You can toggle these flags from the flags dropdown above the pattern input."
                ),
                (
                    "Is it safe to test production data in the regex tester?",
                    "The tool runs in your browser, but it is still best practice to avoid pasting highly sensitive personal or production data into any web-based tester. Use representative sample text whenever you can."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View();
        }

        // Favicon Generator
        public IActionResult FaviconGenerator()
        {
            var url = Url.Action("FaviconGenerator", "Tools", null, Request.Scheme) ?? $"{Request.Scheme}://{Request.Host}/Tools/FaviconGenerator";

            SetSeoData(
                "Favicon Generator",
                "Create crisp favicons and PWA icons from any logo with enterprise-grade presets."
            );

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Favicon Generator",
                "Generate favicon.ico files and multi-size PWA icons with safe padding, rounded corners, and gradient backgrounds.",
                url,
                "DesignApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "What icon sizes does the favicon generator support?",
                    "You can generate a full favicon set including classic ICO sizes (16x16, 32x32, 48x48) and larger PNG icons for modern browsers and PWA manifests, such as 64, 128, 180, 256, and 512 pixels."
                ),
                (
                    "Are my uploaded logos stored on your servers?",
                    "Uploaded images are processed to generate the icon set and are not intended to be stored long term. As with any online tool, avoid uploading highly sensitive or confidential brand assets if your policies do not allow it."
                ),
                (
                    "How do I use the generated icons in my site or app?",
                    "After downloading the ZIP, place the icons in your web root (for example under /images/icons/), reference favicon.ico and touch icons in your HTML head, and update your web app manifest to point at the larger PNG sizes."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

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
            var url = Url.Action("LoremIpsum", "Tools", null, Request.Scheme) ?? $"{Request.Scheme}://{Request.Host}/Tools/LoremIpsum";

            SetSeoData(
                "Lorem Ipsum Generator - Placeholder Text Generator",
                "Generate Lorem Ipsum placeholder text for your designs and mockups. Customize paragraphs, words, or lists with classic or modern styles."
            );

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Lorem Ipsum Generator",
                "Generate placeholder text for mockups, wireframes, and design prototypes with customizable length and format.",
                url,
                "UtilitySoftware"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "What is Lorem Ipsum text used for?",
                    "Lorem Ipsum is filler text used in design, layout, and prototyping so you can focus on typography, spacing, and hierarchy without being distracted by real content."
                ),
                (
                    "Does this generator create unique or translated Lorem Ipsum?",
                    "The default mode uses the classic Latin-like Lorem Ipsum source text. You can also mix in modern or \"hipster\" style words for more playful placeholder copy when needed."
                ),
                (
                    "Is any of the generated text stored or tracked?",
                    "No. The text is generated in your browser session and displayed on the page. It is not stored on the server or tied to your identity."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

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
