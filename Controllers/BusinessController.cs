using Microsoft.AspNetCore.Mvc;
using NovaToolsHub.Models.ViewModels;
using NovaToolsHub.Helpers;

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
        var model = new BasePageViewModel
        {
            PageTitle = "Business Loan Calculator - NovaTools Hub",
            MetaDescription = "Calculate business loan payments, EMI schedules, and interest breakdowns. Plan your business financing with confidence.",
            CanonicalUrl = "/business/loancalculator"
        };

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
}
