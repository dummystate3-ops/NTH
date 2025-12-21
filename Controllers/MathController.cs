using Microsoft.AspNetCore.Mvc;
using NovaToolsHub.Models.ViewModels;
using NovaToolsHub.Helpers;

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
            ViewBag.JsonLdSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Equation Solver",
                "Solve linear, quadratic, and cubic equations with step-by-step solutions. Get real and complex roots instantly.",
                $"{Request.Scheme}://{Request.Host}/Math/EquationSolver",
                "EducationalApplication"
            );
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
            ViewBag.JsonLdSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Permutation & Combination Calculator",
                "Calculate permutations (nPr) and combinations (nCr) with detailed explanations. Perfect for probability and statistics.",
                $"{Request.Scheme}://{Request.Host}/Math/PermutationCombination",
                "EducationalApplication"
            );
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
            return View(model);
        }
    }
}
