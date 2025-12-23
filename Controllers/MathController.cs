using Microsoft.AspNetCore.Mvc;
using NovaToolsHub.Models.ViewModels;
using NovaToolsHub.Helpers;
using System.Collections.Generic;

namespace NovaToolsHub.Controllers
{
    public class MathController : Controller
    {
        /// <summary>
        /// Equation Solver - Solves linear, quadratic, and cubic equations
        /// </summary>
        public IActionResult EquationSolver()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Equation Solver - Linear, Quadratic & Cubic | NovaTools Hub",
                MetaDescription = "Solve linear, quadratic, and cubic equations with step-by-step solutions. Get real and complex roots instantly with our free online equation solver.",
                CanonicalUrl = Url.Action("EquationSolver", "Math", null, Request.Scheme) ?? string.Empty
            };

            SetSeoData(model);

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Equation Solver",
                "Solve linear, quadratic, and cubic equations with step-by-step solutions. Get real and complex roots instantly.",
                $"{Request.Scheme}://{Request.Host}/Math/EquationSolver",
                "EducationalApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "Which types of equations can this equation solver handle?",
                    "The equation solver can handle linear equations (ax + b = 0), quadratic equations (ax² + bx + c = 0), and cubic equations (ax³ + bx² + cx + d = 0). For cubic equations it uses a numerical method to find a real root."
                ),
                (
                    "Does the equation solver show step-by-step working?",
                    "Yes. For linear and quadratic equations the solver shows each step, including isolating the variable and applying the quadratic formula when necessary."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View(model);
        }

        /// <summary>
        /// Permutation & Combination Calculator
        /// </summary>
        public IActionResult PermutationCombination()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Permutation & Combination Calculator | NovaTools Hub",
                MetaDescription = "Calculate permutations (nPr) and combinations (nCr) with detailed explanations. Perfect for probability, statistics, and combinatorics problems.",
                CanonicalUrl = Url.Action("PermutationCombination", "Math", null, Request.Scheme) ?? string.Empty
            };

            SetSeoData(model);

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Permutation & Combination Calculator",
                "Calculate permutations (nPr) and combinations (nCr) with detailed explanations. Perfect for probability and statistics.",
                $"{Request.Scheme}://{Request.Host}/Math/PermutationCombination",
                "EducationalApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "What is the difference between permutation and combination?",
                    "Use permutations (nPr) when order matters, such as assigning 1st, 2nd, and 3rd place. Use combinations (nCr) when order does not matter, such as choosing a team of people where roles are equal."
                ),
                (
                    "When should I use nPr versus nCr in probability problems?",
                    "If different arrangements of the same items count as distinct outcomes, use nPr. If all arrangements of the same selected items are considered the same outcome, use nCr."
                ),
                (
                    "Can this calculator handle large values of n and r?",
                    "Yes. The calculator uses factorials directly for smaller values and logarithmic approximations for large values to reduce overflow, returning results in standard or scientific notation where appropriate."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View(model);
        }

        /// <summary>
        /// Cryptography Tool - Basic cipher encoding/decoding
        /// </summary>
        public IActionResult Cryptography()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Cryptography Tool - Caesar & Substitution Cipher | NovaTools Hub",
                MetaDescription = "Encode and decode messages using Caesar cipher and substitution cipher. Learn basic cryptography concepts with our interactive encryption tool.",
                CanonicalUrl = Url.Action("Cryptography", "Math", null, Request.Scheme) ?? string.Empty
            };

            SetSeoData(model);

            ViewBag.JsonLdSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Cryptography Tool",
                "Encode and decode messages using Caesar cipher and substitution cipher. Learn basic cryptography concepts with our interactive encryption tool.",
                $"{Request.Scheme}://{Request.Host}/Math/Cryptography",
                "EducationalApplication"
            );

            return View(model);
        }

        /// <summary>
        /// Expression Evaluator - Safely evaluate mathematical expressions
        /// </summary>
        public IActionResult ExpressionEvaluator()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Math Expression Evaluator | NovaTools Hub",
                MetaDescription = "Evaluate complex mathematical expressions safely. Supports functions like sin, cos, sqrt, log, and more. Advanced calculator for students and professionals.",
                CanonicalUrl = Url.Action("ExpressionEvaluator", "Math", null, Request.Scheme) ?? string.Empty
            };

            SetSeoData(model);

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Math Expression Evaluator",
                "Evaluate complex mathematical expressions with advanced functions like sin, cos, sqrt, and log.",
                $"{Request.Scheme}://{Request.Host}/Math/ExpressionEvaluator",
                "EducationalApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "What kinds of expressions can the math expression evaluator handle?",
                    "You can evaluate arithmetic expressions, trigonometric functions, logarithms, powers, roots, absolute values, and functions like max, min, ceil, and floor. The tool is built on math.js for robust parsing."
                ),
                (
                    "Is my input executed as arbitrary code?",
                    "No. The expression evaluator uses the math.js library, which parses your input as mathematical expressions in a sandboxed environment rather than executing arbitrary JavaScript code."
                ),
                (
                    "Why do I sometimes see results in scientific notation?",
                    "Very large or very small numbers are shown in scientific notation to keep results readable. This is expected behavior when working with extreme values."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View(model);
        }

        /// <summary>
        /// Indicator Codes Reference - Math symbols and notations guide
        /// </summary>
        public IActionResult IndicatorCodes()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Mathematical Indicator Codes Reference | NovaTools Hub",
                MetaDescription = "Complete reference guide for mathematical symbols, notation, and indicator codes. Essential resource for students and math professionals.",
                CanonicalUrl = Url.Action("IndicatorCodes", "Math", null, Request.Scheme) ?? string.Empty
            };

            SetSeoData(model);

            ViewBag.JsonLdSchema = SeoHelper.GenerateWebPageSchema(
                "Mathematical Indicator Codes Reference",
                "Complete reference guide for mathematical symbols, notation, and indicator codes.",
                $"{Request.Scheme}://{Request.Host}/Math/IndicatorCodes",
                $"{Request.Scheme}://{Request.Host}/images/og-default.png"
            );

            return View(model);
        }

        /// <summary>
        /// Interactive Problem Solver - Step-by-step solutions
        /// </summary>
        public IActionResult ProblemSolver()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Interactive Math Problem Solver with Steps | NovaTools Hub",
                MetaDescription = "Solve math problems with detailed step-by-step explanations. Learn how to solve equations, simplify expressions, and master algebra concepts.",
                CanonicalUrl = Url.Action("ProblemSolver", "Math", null, Request.Scheme) ?? string.Empty
            };

            SetSeoData(model);

            var learningSchema = SeoHelper.GenerateLearningResourceSchema(
                "Interactive Math Problem Solver",
                "Interactive math problem solver with detailed step-by-step explanations for equations, simplification, and factoring.",
                $"{Request.Scheme}://{Request.Host}/Math/ProblemSolver",
                "High School, College"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "What kinds of math problems can the problem solver explain?",
                    "The interactive problem solver focuses on core algebra topics: linear equations, quadratic equations, simplifying expressions, and factoring quadratics. It breaks each solution into clear, annotated steps."
                ),
                (
                    "Is this tool intended for homework answers or learning?",
                    "The tool is designed as a learning aid. It shows you how to solve problems step by step so you can understand the techniques, not just copy the final answer."
                ),
                (
                    "Does the problem solver support advanced topics like calculus?",
                    "Currently the problem solver focuses on pre-algebra and algebra topics. For calculus or higher-level math you can use the graph plotter and expression evaluator tools alongside standard textbooks."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{learningSchema},{faqSchema}]";

            return View(model);
        }

        /// <summary>
        /// Graph Plotter - Interactive function graphing
        /// </summary>
        public IActionResult GraphPlotter()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Interactive Graph Plotter - Function Graphing Tool | NovaTools Hub",
                MetaDescription = "Plot mathematical functions and equations with our interactive graph plotter. Visualize functions, explore calculus, and analyze mathematical relationships.",
                CanonicalUrl = Url.Action("GraphPlotter", "Math", null, Request.Scheme) ?? string.Empty
            };

            SetSeoData(model);

            ViewBag.JsonLdSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Interactive Graph Plotter",
                "Plot mathematical functions and equations with an interactive graphing tool.",
                $"{Request.Scheme}://{Request.Host}/Math/GraphPlotter",
                "EducationalApplication"
            );

            return View(model);
        }

        private void SetSeoData(BasePageViewModel model)
        {
            ViewBag.PageTitle = model.PageTitle;
            ViewBag.MetaDescription = model.MetaDescription;
            ViewBag.CanonicalUrl = model.CanonicalUrl;
        }
    }
}
