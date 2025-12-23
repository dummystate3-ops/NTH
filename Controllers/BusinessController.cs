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
        var url = Url.Action("ProfitMargin", "Business", null, Request.Scheme) ?? string.Empty;

        var model = new BasePageViewModel
        {
            PageTitle = "Profit Margin & Pricing Calculator - NovaTools Hub",
            MetaDescription = "Calculate profit margins, markup percentages, and optimal pricing for your products and services. Free business calculator tool.",
            CanonicalUrl = url
        };

        var financialSchema = SeoHelper.GenerateFinancialServiceSchema(
            "Profit Margin Calculator",
            "Calculate profit margins, markup percentages, and optimal pricing for your products and services.",
            url
        );

        var faqs = new List<(string Question, string Answer)>
        {
            (
                "What can I use the profit margin calculator for?",
                "You can use this tool to compare cost price and selling price, calculate profit per unit, profit margin and markup percentage, and estimate total profit for different quantities. It helps you sense-check pricing for products, services, or packages."
            ),
            (
                "Are my pricing and cost numbers stored anywhere?",
                "No. All profit and pricing calculations run directly in your browser. The inputs you enter are not sent to or stored on NovaTools Hub servers, but you should still avoid typing highly sensitive account details into any calculator."
            ),
            (
                "Is this calculator a substitute for accounting or tax advice?",
                "No. The results are for planning and educational purposes only. They do not take into account taxes, overhead allocation, discounts, or industry-specific rules. Always consult your accountant or finance team before making major pricing decisions."
            )
        };

        var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
        ViewBag.JsonLdSchema = $"[{financialSchema},{faqSchema}]";

        return View(model);
    }

    public IActionResult RoiAnalysis()
    {
        var url = Url.Action("RoiAnalysis", "Business", null, Request.Scheme) ?? string.Empty;

        var model = new BasePageViewModel
        {
            PageTitle = "ROI Analysis Calculator - NovaTools Hub",
            MetaDescription = "Calculate return on investment (ROI) for your business projects and investments. Visualize ROI trends and payback periods.",
            CanonicalUrl = url
        };

        var financialSchema = SeoHelper.GenerateFinancialServiceSchema(
            "ROI Analysis Calculator",
            "Calculate return on investment (ROI) for your business projects and investments. Visualize ROI trends and payback periods.",
            url
        );

        var faqs = new List<(string Question, string Answer)>
        {
            (
                "What does this ROI calculator help me understand?",
                "The calculator estimates total returns, net profit, overall ROI percentage, annualized ROI, and payback period over a time horizon you choose. It is useful for comparing projects, tools, or automation initiatives at a high level."
            ),
            (
                "Are my investment figures sent to your servers?",
                "ROI calculations are performed in your browser after you enter the inputs. The tool is not designed to store your amounts long term, but as a best practice you should avoid entering very sensitive financial account information into any website."
            ),
            (
                "Can I rely on this ROI analysis for investment decisions?",
                "Treat the output as an approximate model only. The calculator assumes consistent returns and does not account for taxes, financing, risk, or detailed cash-flow timing. For real-world investment or budgeting decisions, review the numbers with your finance team or advisor."
            )
        };

        var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
        ViewBag.JsonLdSchema = $"[{financialSchema},{faqSchema}]";

        return View(model);
    }

    public IActionResult UnbilledHours()
    {
        var url = Url.Action("UnbilledHours", "Business", null, Request.Scheme) ?? string.Empty;

        var model = new BasePageViewModel
        {
            PageTitle = "Unbilled Hours Estimator - NovaTools Hub",
            MetaDescription = "Track and estimate the value of unbilled hours for consulting, freelancing, and service businesses.",
            CanonicalUrl = url
        };

        var financialSchema = SeoHelper.GenerateFinancialServiceSchema(
            "Unbilled Hours Estimator",
            "Track and estimate the value of unbilled hours for consulting, freelancing, and service businesses.",
            url
        );

        var faqs = new List<(string Question, string Answer)>
        {
            (
                "What does the unbilled hours estimator do?",
                "It helps you log client or project work that has not yet been invoiced, calculate total unbilled hours, and estimate the value of that work based on your hourly rate so you can protect revenue and cash flow."
            ),
            (
                "Where is my unbilled hours data stored, and can I clear it?",
                "Entries are stored locally in your browser using localStorage so they persist between visits on the same device. You can remove individual entries or clear all data at any time using the controls in the tool."
            ),
            (
                "Does this replace a full time tracking or invoicing system?",
                "No. This is a lightweight helper for estimating unbilled work. For full time tracking, invoicing, and client reporting, you should use dedicated accounting, billing, or project management software."
            )
        };

        var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
        ViewBag.JsonLdSchema = $"[{financialSchema},{faqSchema}]";

        return View(model);
    }

    public IActionResult SavingsComparison()
    {
        var url = Url.Action("SavingsComparison", "Business", null, Request.Scheme) ?? string.Empty;

        var model = new BasePageViewModel
        {
            PageTitle = "Product Savings Comparison - NovaTools Hub",
            MetaDescription = "Compare multiple product options side-by-side to find the best value. Analyze costs, features, and savings potential.",
            CanonicalUrl = url
        };

        var financialSchema = SeoHelper.GenerateFinancialServiceSchema(
            "Product Savings Comparison Tool",
            "Compare multiple product options side-by-side to find the best value over 1, 3, and 5 years.",
            url
        );

        var faqs = new List<(string Question, string Answer)>
        {
            (
                "What can I compare with the product savings tool?",
                "You can compare subscriptions, SaaS plans, payment tiers, or other recurring services by entering setup costs, monthly fees, and optional annual discounts to see the total cost over different time horizons."
            ),
            (
                "Are my comparison inputs uploaded or stored?",
                "No. All comparisons, charts, and recommendations are calculated in your browser. The values you enter are not sent to NovaTools Hub servers."
            ),
            (
                "How should I interpret the savings recommendation?",
                "The recommendation is based purely on projected total cost over time. It does not account for qualitative factors such as support quality, feature depth, integration needs, or contract terms, so always review those before making a final decision."
            )
        };

        var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
        ViewBag.JsonLdSchema = $"[{financialSchema},{faqSchema}]";

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
        var url = Url.Action("AutomationPlanner", "Business", null, Request.Scheme) ?? string.Empty;

        var model = new BasePageViewModel
        {
            PageTitle = "Task Automation ROI Planner - NovaTools Hub",
            MetaDescription = "Estimate time savings and ROI from automating business tasks. Plan your automation strategy with data-driven insights.",
            CanonicalUrl = url
        };

        var financialSchema = SeoHelper.GenerateFinancialServiceSchema(
            "Task Automation ROI Planner",
            "Estimate time savings, annual savings, and ROI from automating recurring business tasks.",
            url
        );

        var faqs = new List<(string Question, string Answer)>
        {
            (
                "How does the task automation ROI planner estimate savings?",
                "The planner uses your current hours per week, hourly rate, expected percentage of time saved, and automation cost to estimate hours saved per year, annual savings, ROI percentage, and payback period in months."
            ),
            (
                "Is my automation planning data stored on your servers?",
                "Task entries are stored locally in your browser using localStorage so that you can revisit them later on the same device. They are not sent to NovaTools Hub servers, and you can clear them at any time using the in-tool controls or your browser settings."
            ),
            (
                "Can I use this tool as a final business case for automation?",
                "Treat the output as a starting point for discussions. It does not automatically include every cost such as change management, ongoing vendor fees, or quality impacts. Always review the assumptions with stakeholders before approving an automation project."
            )
        };

        var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
        ViewBag.JsonLdSchema = $"[{financialSchema},{faqSchema}]";

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
