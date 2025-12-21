using Microsoft.AspNetCore.Mvc;
using NovaToolsHub.Models.ViewModels;
using NovaToolsHub.Helpers;
using System.Collections.Generic;

namespace NovaToolsHub.Controllers;

/// <summary>
/// Controller for business and financial calculator tools
/// </summary>
public class BusinessController : Controller
{
    public IActionResult ProfitMargin()
    {
        var model = new BasePageViewModel
        {
            PageTitle = "Profit Margin & Pricing Calculator - NovaTools Hub",
            MetaDescription = "Calculate profit margins, markup percentages, and optimal pricing for your products and services. Free business calculator tool.",
            CanonicalUrl = "/business/profitmargin"
        };
        ViewBag.JsonLdSchema = SeoHelper.GenerateFinancialServiceSchema(
            "Profit Margin Calculator",
            "Calculate profit margins, markup percentages, and optimal pricing for your products and services.",
            $"{Request.Scheme}://{Request.Host}/Business/ProfitMargin"
        );

        return View(model);
    }

    public IActionResult RoiAnalysis()
    {
        var model = new BasePageViewModel
        {
            PageTitle = "ROI Analysis Calculator - NovaTools Hub",
            MetaDescription = "Calculate return on investment (ROI) for your business projects and investments. Visualize ROI trends and payback periods.",
            CanonicalUrl = "/business/roianalysis"
        };
        ViewBag.JsonLdSchema = SeoHelper.GenerateFinancialServiceSchema(
            "ROI Analysis Calculator",
            "Calculate return on investment (ROI) for your business projects and investments. Visualize ROI trends and payback periods.",
            $"{Request.Scheme}://{Request.Host}/Business/RoiAnalysis"
        );

        return View(model);
    }

    public IActionResult UnbilledHours()
    {
        var model = new BasePageViewModel
        {
            PageTitle = "Unbilled Hours Estimator - NovaTools Hub",
            MetaDescription = "Track and estimate the value of unbilled hours for consulting, freelancing, and service businesses.",
            CanonicalUrl = "/business/unbilledhours"
        };

        return View(model);
    }

    public IActionResult SavingsComparison()
    {
        var model = new BasePageViewModel
        {
            PageTitle = "Product Savings Comparison - NovaTools Hub",
            MetaDescription = "Compare multiple product options side-by-side to find the best value. Analyze costs, features, and savings potential.",
            CanonicalUrl = "/business/savingscomparison"
        };

        return View(model);
    }

    public IActionResult LoanCalculator()
    {
        var url = Url.Action("LoanCalculator", "Business", null, Request.Scheme) ?? string.Empty;

        var model = new BasePageViewModel
        {
            PageTitle = "Business Loan & EMI Calculator | NovaTools Hub",
            MetaDescription = "Calculate business loan EMIs, payment schedules, and total interest with a detailed amortization table. Supports multiple currencies and payment frequencies.",
            CanonicalUrl = url
        };

        var financialSchema = SeoHelper.GenerateFinancialServiceSchema(
            "Business Loan Calculator",
            "Calculate business loan EMIs, payment schedules, and total interest with a detailed amortization table.",
            url
        );

        var loanFaqs = new List<(string Question, string Answer)>
        {
            (
                "What is an EMI in a business loan?",
                "An EMI (Equated Monthly Installment) is the fixed payment you make each period to repay your business loan. It covers both principal and interest based on your loan amount, interest rate, tenure, and payment frequency."
            ),
            (
                "Does this calculator store any of my financial data?",
                "No. All loan calculations run entirely in your browser and are not sent to or stored on NovaTools Hub servers."
            ),
            (
                "Can I use this calculator for different currencies and payment schedules?",
                "Yes. You can choose between multiple currencies and payment frequencies such as monthly, quarterly, or annual payments. The EMI and amortization schedule will adjust accordingly."
            )
        };

        var loanFaqSchema = SeoHelper.GenerateFaqPageSchema(loanFaqs);
        ViewBag.JsonLdSchema = $"[{financialSchema},{loanFaqSchema}]";

        return View(model);
    }

    public IActionResult CompoundInterest()
    {
        var url = Url.Action("CompoundInterest", "Business", null, Request.Scheme) ?? string.Empty;

        var model = new BasePageViewModel
        {
            PageTitle = "Compound Interest & Savings Calculator | NovaTools Hub",
            MetaDescription = "Estimate how your savings grow over time with compound interest. Visualize contributions, interest earned, and total future value.",
            CanonicalUrl = url
        };

        var savingsSchema = SeoHelper.GenerateFinancialServiceSchema(
            "Compound Interest & Savings Calculator",
            "Estimate how your savings grow over time with compound interest and optional monthly contributions.",
            url
        );

        var savingsFaqs = new List<(string Question, string Answer)>
        {
            (
                "What does this compound interest calculator show?",
                "This calculator projects how your savings can grow over time based on your initial amount, monthly contributions, annual interest rate, compounding frequency, and duration."
            ),
            (
                "Is the result guaranteed by a bank or financial institution?",
                "No. The projections are for educational purposes only and assume a constant annual rate. Real returns will vary based on market conditions and product fees."
            ),
            (
                "Are my savings inputs sent to the server?",
                "No. All compound interest calculations happen locally in your browser. NovaTools Hub does not store your amounts, rates, or durations."
            )
        };

        var savingsFaqSchema = SeoHelper.GenerateFaqPageSchema(savingsFaqs);
        ViewBag.JsonLdSchema = $"[{savingsSchema},{savingsFaqSchema}]";

        return View(model);
    }

    public IActionResult AutomationPlanner()
    {
        var model = new BasePageViewModel
        {
            PageTitle = "Task Automation ROI Planner - NovaTools Hub",
            MetaDescription = "Estimate time savings and ROI from automating business tasks. Plan your automation strategy with data-driven insights.",
            CanonicalUrl = "/business/automationplanner"
        };

        return View(model);
    }

    public IActionResult PakistanTaxCalculator()
    {
        var url = Url.Action("PakistanTaxCalculator", "Business", null, Request.Scheme) ?? string.Empty;

        var model = new BasePageViewModel
        {
            PageTitle = "Pakistan Salary Tax Calculator 2025-26 | NovaTools Hub",
            MetaDescription = "Calculate estimated income tax for salaried individuals in Pakistan for tax year 2025-26 using current FBR slabs.",
            CanonicalUrl = url
        };

        var taxSchema = SeoHelper.GenerateFinancialServiceSchema(
            "Pakistan Salary Tax Calculator",
            "Estimate Pakistan salary income tax for tax year 2025-26 using current FBR slabs for salaried individuals.",
            url
        );

        var taxFaqs = new List<(string Question, string Answer)>
        {
            (
                "Which tax year does this Pakistan salary tax calculator use?",
                "This tool is configured for Pakistan tax year 2025-26 and uses the latest publicly available FBR slabs for salaried individuals."
            ),
            (
                "Does this calculator cover all allowances and exemptions?",
                "No. The calculator works on your annual taxable salary after any exemptions or allowances. You should adjust your input to reflect taxable income or consult a tax professional."
            ),
            (
                "Is the result an official tax determination?",
                "No. This is an educational estimate only and does not replace an official FBR calculation or professional advice."
            )
        };

        var taxFaqSchema = SeoHelper.GenerateFaqPageSchema(taxFaqs);
        ViewBag.JsonLdSchema = $"[{taxSchema},{taxFaqSchema}]";

        return View(model);
    }

    public IActionResult PakistanZakatCalculator()
    {
        var url = Url.Action("PakistanZakatCalculator", "Business", null, Request.Scheme) ?? string.Empty;

        var model = new BasePageViewModel
        {
            PageTitle = "Pakistan Zakat Calculator (Gold, Cash & Savings) | NovaTools Hub",
            MetaDescription = "Calculate Zakat on gold, cash, savings and investments in Pakistan using gold and silver nisab thresholds.",
            CanonicalUrl = url
        };

        var zakatSchema = SeoHelper.GenerateFinancialServiceSchema(
            "Pakistan Zakat Calculator",
            "Calculate Zakat on gold, cash, savings and investments in Pakistan using gold and silver nisab thresholds.",
            url
        );

        var zakatFaqs = new List<(string Question, string Answer)>
        {
            (
                "How does this zakat calculator determine if I meet nisab?",
                "The calculator compares your net zakatable wealth (after deducting eligible liabilities) with the nisab threshold you enter based on current gold or silver prices."
            ),
            (
                "Does the calculator use live gold and silver prices?",
                "No. You must provide updated nisab values in PKR based on current gold and silver prices in Pakistan to keep the calculation accurate."
            ),
            (
                "Is this calculator a religious verdict (fatwa)?",
                "No. It is an educational tool to help with basic zakat estimates. For personal rulings, you should consult a qualified scholar."
            )
        };

        var zakatFaqSchema = SeoHelper.GenerateFaqPageSchema(zakatFaqs);
        ViewBag.JsonLdSchema = $"[{zakatSchema},{zakatFaqSchema}]";

        return View(model);
    }
}
