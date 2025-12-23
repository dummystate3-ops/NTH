using System;
using System.Collections.Generic;
using System.Linq;

namespace NovaToolsHub.Helpers;

public sealed class ToolDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Controller { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string IconHtml { get; init; } = string.Empty;
    /// <summary>
    /// High-level section used on the /tools hub (e.g., Developer, Converters, Business, Math, Productivity, Academic, Trending).
    /// </summary>
    public string Section { get; init; } = string.Empty;
    public string[] Badges { get; init; } = Array.Empty<string>();
    public string[] Tags { get; init; } = Array.Empty<string>();
}

public static class ToolRegistry
{
    private static readonly IReadOnlyList<ToolDefinition> _allTools = new[]
    {
        // Developer & Content Tools
        new ToolDefinition
        {
            Id = "json-formatter",
            Name = "JSON Formatter",
            Description = "Format, validate, and minify JSON with instant diff reports.",
            Controller = "Tools",
            Action = "JsonFormatter",
            Category = "Developer",
            IconHtml = "📄",
            Section = "Developer",
            Badges = new[] { "Validation" },
            Tags = new[] { "json", "dev", "format", "validator" }
        },
        new ToolDefinition
        {
            Id = "regex-tester",
            Name = "Regex Tester",
            Description = "Test and debug regular expressions with real-time matching.",
            Controller = "Tools",
            Action = "RegexTester",
            Category = "Developer",
            IconHtml = "🔍",
            Section = "Developer",
            Badges = new[] { "Debug" },
            Tags = new[] { "regex", "pattern", "test" }
        },
        new ToolDefinition
        {
            Id = "lorem-ipsum",
            Name = "Lorem Ipsum Generator",
            Description = "Generate placeholder text for designs, mockups, and wireframes.",
            Controller = "Tools",
            Action = "LoremIpsum",
            Category = "Content",
            IconHtml = "📝",
            Section = "Developer",
            Badges = new[] { "Text" },
            Tags = new[] { "text", "content", "lorem", "ipsum" }
        },
        new ToolDefinition
        {
            Id = "color-palette",
            Name = "Color Palette Generator",
            Description = "Create color palettes and gradients with exportable swatches.",
            Controller = "Tools",
            Action = "ColorPaletteGenerator",
            Category = "Design",
            IconHtml = "🎨",
            Section = "Developer",
            Badges = new[] { "Colors" },
            Tags = new[] { "color", "palette", "design" }
        },
        new ToolDefinition
        {
            Id = "pdf-merge",
            Name = "PDF Merge Tool",
            Description = "Combine multiple PDF documents into a single file in your browser.",
            Controller = "Tools",
            Action = "PdfMerge",
            Category = "Documents",
            IconHtml = "📚",
            Section = "Developer",
            Badges = new[] { "PDF", "Merge" },
            Tags = new[] { "pdf", "merge", "files" }
        },
        new ToolDefinition
        {
            Id = "favicon-generator",
            Name = "Favicon Generator",
            Description = "Create favicons and app icons from your logo or text.",
            Controller = "Tools",
            Action = "FaviconGenerator",
            Category = "Assets",
            IconHtml = "🖼️",
            Section = "Developer",
            Badges = new[] { "Icons" },
            Tags = new[] { "favicon", "icon" }
        },
        new ToolDefinition
        {
            Id = "image-compressor",
            Name = "Image Compressor",
            Description = "Compress images locally with adjustable quality and preview.",
            Controller = "ImageTools",
            Action = "Compressor",
            Category = "Images",
            IconHtml = "🗜️",
            Section = "Developer",
            Badges = new[] { "Optimization" },
            Tags = new[] { "compress", "images", "optimize" }
        },
        new ToolDefinition
        {
            Id = "qr-generator",
            Name = "QR Code Generator",
            Description = "Generate QR codes for URLs, text, and more.",
            Controller = "Tools",
            Action = "QrCodeGenerator",
            Category = "Sharing",
            IconHtml = "📱",
            Section = "Developer",
            Badges = new[] { "QR", "Codes" },
            Tags = new[] { "qr", "code", "sharing" }
        },

        // Converters & Calculators
        new ToolDefinition
        {
            Id = "unit-converter",
            Name = "Unit Converter",
            Description = "Convert between length, weight, volume, and more.",
            Controller = "Tools",
            Action = "UnitConverter",
            Category = "Converters",
            IconHtml = "📏",
            Section = "Converters",
            Badges = new[] { "Units" },
            Tags = new[] { "units", "length", "weight", "volume" }
        },
        new ToolDefinition
        {
            Id = "currency-converter",
            Name = "Currency Converter",
            Description = "Convert between 150+ world currencies with live rates.",
            Controller = "Tools",
            Action = "CurrencyConverter",
            Category = "Finance",
            IconHtml = "💱",
            Section = "Converters",
            Badges = new[] { "FX" },
            Tags = new[] { "fx", "exchange rate", "money", "currency" }
        },
        new ToolDefinition
        {
            Id = "loan-emi",
            Name = "Loan & EMI Calculator",
            Description = "Calculate loan EMIs, schedules, and total interest.",
            Controller = "Business",
            Action = "LoanCalculator",
            Category = "Finance",
            IconHtml = "💰",
            Section = "Converters",
            Badges = new[] { "Loans" },
            Tags = new[] { "loan", "emi", "business" }
        },
        new ToolDefinition
        {
            Id = "compound-interest",
            Name = "Compound Interest",
            Description = "Project savings growth with compound interest and contributions.",
            Controller = "Business",
            Action = "CompoundInterest",
            Category = "Finance",
            IconHtml = "📈",
            Section = "Converters",
            Badges = new[] { "Savings" },
            Tags = new[] { "savings", "interest", "compound" }
        },
        new ToolDefinition
        {
            Id = "pakistan-salary-tax",
            Name = "Pakistan Salary Tax",
            Description = "Estimate Pakistan salary income tax for 2025-26.",
            Controller = "Business",
            Action = "PakistanTaxCalculator",
            Category = "Pakistan",
            IconHtml = "🇵🇰",
            Section = "Converters",
            Badges = new[] { "Tax" },
            Tags = new[] { "tax", "pakistan", "salary" }
        },
        new ToolDefinition
        {
            Id = "bmi-calculator",
            Name = "BMI Calculator",
            Description = "Calculate Body Mass Index for health insights.",
            Controller = "Tools",
            Action = "BmiCalculator",
            Category = "Health",
            IconHtml = "⚖️",
            Section = "Converters",
            Badges = new[] { "BMI" },
            Tags = new[] { "bmi", "health" }
        },
        new ToolDefinition
        {
            Id = "age-calculator",
            Name = "Age Calculator",
            Description = "Calculate exact age in years, months, and days.",
            Controller = "Tools",
            Action = "AgeCalculator",
            Category = "Utility",
            IconHtml = "🎂",
            Section = "Converters",
            Badges = new[] { "Age" },
            Tags = new[] { "age", "birthday" }
        },
        new ToolDefinition
        {
            Id = "date-calculator",
            Name = "Date Calculator",
            Description = "Calculate date differences or add/subtract days.",
            Controller = "Tools",
            Action = "DateCalculator",
            Category = "Utility",
            IconHtml = "📅",
            Section = "Converters",
            Badges = new[] { "Dates" },
            Tags = new[] { "dates", "time", "days" }
        },
        new ToolDefinition
        {
            Id = "password-generator",
            Name = "Password Generator",
            Description = "Generate strong, secure passwords instantly.",
            Controller = "Tools",
            Action = "PasswordGenerator",
            Category = "Security",
            IconHtml = "🔒",
            Section = "Converters",
            Badges = new[] { "Passwords" },
            Tags = new[] { "password", "security", "random" }
        },

        // Pakistan finance (extra calculator not surfaced as a card on /tools yet)
        new ToolDefinition
        {
            Id = "pakistan-zakat",
            Name = "Pakistan Zakat Calculator",
            Description = "Calculate zakat on gold, cash, savings, and investments in Pakistan.",
            Controller = "Business",
            Action = "PakistanZakatCalculator",
            Category = "Pakistan",
            IconHtml = "🕌",
            Section = "Business",
            Badges = Array.Empty<string>(),
            Tags = new[] { "zakat", "pakistan" }
        },

        // Math & Education
        new ToolDefinition
        {
            Id = "equation-solver",
            Name = "Equation Solver",
            Description = "Solve linear, quadratic, and cubic equations.",
            Controller = "Math",
            Action = "EquationSolver",
            Category = "Math",
            IconHtml = "📐",
            Section = "Math",
            Badges = new[] { "Equations" },
            Tags = new[] { "equations", "algebra" }
        },
        new ToolDefinition
        {
            Id = "problem-solver",
            Name = "Problem Solver",
            Description = "General math problem solver.",
            Controller = "Math",
            Action = "ProblemSolver",
            Category = "Math",
            IconHtml = "🧩",
            Section = "Math",
            Badges = new[] { "Problems" },
            Tags = new[] { "problems", "math" }
        },
        new ToolDefinition
        {
            Id = "graph-plotter",
            Name = "Graph Plotter",
            Description = "Plot and analyze 2D mathematical functions.",
            Controller = "Math",
            Action = "GraphPlotter",
            Category = "Math",
            IconHtml = "📈",
            Section = "Math",
            Badges = new[] { "Graphs" },
            Tags = new[] { "graph", "plot" }
        },
        new ToolDefinition
        {
            Id = "expression-evaluator",
            Name = "Evaluator",
            Description = "Evaluate complex mathematical expressions.",
            Controller = "Math",
            Action = "ExpressionEvaluator",
            Category = "Math",
            IconHtml = "🧮",
            Section = "Math",
            Badges = new[] { "Expressions" },
            Tags = new[] { "expression", "math" }
        },
        new ToolDefinition
        {
            Id = "permutation-combination",
            Name = "Permutation & Combination",
            Description = "Calculate permutations and combinations.",
            Controller = "Math",
            Action = "PermutationCombination",
            Category = "Combinatorics",
            IconHtml = "🎲",
            Section = "Math",
            Badges = new[] { "nPr", "nCr" },
            Tags = new[] { "npr", "ncr", "combinatorics" }
        },
        new ToolDefinition
        {
            Id = "cryptography",
            Name = "Cryptography",
            Description = "Basic cryptographic ciphers tool.",
            Controller = "Math",
            Action = "Cryptography",
            Category = "Security",
            IconHtml = "🗝️",
            Section = "Math",
            Badges = new[] { "Ciphers" },
            Tags = new[] { "cipher", "crypto" }
        },
        new ToolDefinition
        {
            Id = "indicator-codes",
            Name = "Indicator Codes",
            Description = "Binary, decimal, and hex conversions.",
            Controller = "Math",
            Action = "IndicatorCodes",
            Category = "Developer",
            IconHtml = "🔢",
            Section = "Math",
            Badges = new[] { "Binary", "Hex" },
            Tags = new[] { "binary", "hex", "codes" }
        },

        // Productivity
        new ToolDefinition
        {
            Id = "image-resizer",
            Name = "Image Resizer",
            Description = "Resize, crop, and batch process images.",
            Controller = "ImageTools",
            Action = "AdvancedResizer",
            Category = "Images",
            IconHtml = "🖼️",
            Section = "Productivity",
            Badges = new[] { "Batch" },
            Tags = new[] { "resize", "crop", "images", "batch" }
        },
        new ToolDefinition
        {
            Id = "todo-list",
            Name = "To-Do List",
            Description = "Simple yet effective task management.",
            Controller = "Productivity",
            Action = "TodoList",
            Category = "Tasks",
            IconHtml = "✅",
            Section = "Productivity",
            Badges = new[] { "Todos" },
            Tags = new[] { "todo", "tasks" }
        },
        new ToolDefinition
        {
            Id = "kanban-board",
            Name = "Kanban Board",
            Description = "Visual project management with boards.",
            Controller = "Productivity",
            Action = "KanbanBoard",
            Category = "Projects",
            IconHtml = "📋",
            Section = "Productivity",
            Badges = new[] { "Kanban" },
            Tags = new[] { "kanban", "projects" }
        },
        new ToolDefinition
        {
            Id = "notes",
            Name = "Notes",
            Description = "Quick notepad with auto-save.",
            Controller = "Productivity",
            Action = "Notes",
            Category = "Writing",
            IconHtml = "📒",
            Section = "Productivity",
            Badges = new[] { "Notes" },
            Tags = new[] { "notes", "text" }
        },
        new ToolDefinition
        {
            Id = "pomodoro",
            Name = "Pomodoro Timer",
            Description = "Use the Pomodoro technique to focus in short sprints.",
            Controller = "Productivity",
            Action = "PomodoroTimer",
            Category = "Focus",
            IconHtml = "🍅",
            Section = "Productivity",
            Badges = new[] { "Timer" },
            Tags = new[] { "pomodoro", "focus", "timer" }
        },
        new ToolDefinition
        {
            Id = "time-tracker",
            Name = "Time Tracker",
            Description = "Track time for tasks and projects.",
            Controller = "Productivity",
            Action = "TimeTracker",
            Category = "Time",
            IconHtml = "⏱️",
            Section = "Productivity",
            Badges = new[] { "Tracking" },
            Tags = new[] { "time", "tracking" }
        },
        new ToolDefinition
        {
            Id = "stopwatch",
            Name = "Stopwatch",
            Description = "Precise timing with lap support.",
            Controller = "Productivity",
            Action = "Stopwatch",
            Category = "Time",
            IconHtml = "⏱️",
            Section = "Productivity",
            Badges = new[] { "Timer" },
            Tags = new[] { "stopwatch", "timer" }
        },
        new ToolDefinition
        {
            Id = "world-time",
            Name = "World Time Viewer",
            Description = "View current time across major world cities.",
            Controller = "Productivity",
            Action = "WorldTime",
            Category = "Time",
            IconHtml = "🌍",
            Section = "Productivity",
            Badges = new[] { "Zones" },
            Tags = new[] { "world time", "clock" }
        },
        new ToolDefinition
        {
            Id = "time-zone-converter",
            Name = "Time Zone Converter",
            Description = "Convert time between different time zones.",
            Controller = "Productivity",
            Action = "TimeZoneConverter",
            Category = "Time",
            IconHtml = "🌐",
            Section = "Productivity",
            Badges = new[] { "Zones" },
            Tags = new[] { "zones", "time", "meeting" }
        },
        new ToolDefinition
        {
            Id = "text-to-speech",
            Name = "Text-to-Speech",
            Description = "Convert text to spoken audio using your browser.",
            Controller = "Productivity",
            Action = "TextToSpeech",
            Category = "Audio",
            IconHtml = "🗣️",
            Section = "Productivity",
            Badges = new[] { "TTS" },
            Tags = new[] { "tts", "speech", "audio" }
        },

        // Business & Finance
        new ToolDefinition
        {
            Id = "profit-margin",
            Name = "Profit Margin Calculator",
            Description = "Calculate profit margins, markups, and optimal pricing.",
            Controller = "Business",
            Action = "ProfitMargin",
            Category = "Business",
            IconHtml = "💰",
            Section = "Business",
            Badges = new[] { "Margin" },
            Tags = new[] { "margin", "profit" }
        },
        new ToolDefinition
        {
            Id = "roi-analysis",
            Name = "ROI Analysis",
            Description = "Analyze return on investment for projects or campaigns.",
            Controller = "Business",
            Action = "RoiAnalysis",
            Category = "Finance",
            IconHtml = "📈",
            Section = "Business",
            Badges = new[] { "ROI" },
            Tags = new[] { "roi", "investment" }
        },
        new ToolDefinition
        {
            Id = "unbilled-hours",
            Name = "Unbilled Hours",
            Description = "Estimate the value of unbilled work and time.",
            Controller = "Business",
            Action = "UnbilledHours",
            Category = "Billing",
            IconHtml = "⏳",
            Section = "Business",
            Badges = new[] { "Hours" },
            Tags = new[] { "hours", "billing", "unbilled" }
        },
        new ToolDefinition
        {
            Id = "savings-comparison",
            Name = "Savings Comparison",
            Description = "Compare savings or investment options side by side.",
            Controller = "Business",
            Action = "SavingsComparison",
            Category = "Finance",
            IconHtml = "🛒",
            Section = "Business",
            Badges = new[] { "Savings" },
            Tags = new[] { "savings", "compare" }
        },
        new ToolDefinition
        {
            Id = "automation-planner",
            Name = "Automation Planner",
            Description = "Estimate ROI from automating manual processes.",
            Controller = "Business",
            Action = "AutomationPlanner",
            Category = "Automation",
            IconHtml = "🤖",
            Section = "Business",
            Badges = new[] { "ROI" },
            Tags = new[] { "automation", "roi" }
        },

        // Academic & Teacher Tools
        new ToolDefinition
        {
            Id = "quiz-builder",
            Name = "Online Quiz Builder",
            Description = "Create interactive quizzes and assessments for students.",
            Controller = "Academic",
            Action = "QuizBuilder",
            Category = "Study",
            IconHtml = "❓",
            Section = "Academic",
            Badges = new[] { "Quizzes" },
            Tags = new[] { "quiz", "assessment" }
        },
        new ToolDefinition
        {
            Id = "flashcards",
            Name = "Flashcard Generator",
            Description = "Create and study with digital flashcards.",
            Controller = "Academic",
            Action = "Flashcards",
            Category = "Study",
            IconHtml = "📇",
            Section = "Academic",
            Badges = new[] { "Cards" },
            Tags = new[] { "flashcards", "study" }
        },
        new ToolDefinition
        {
            Id = "grammar-helper",
            Name = "Grammar Helper",
            Description = "Check grammar and improve writing with AI assistance.",
            Controller = "Academic",
            Action = "GrammarHelper",
            Category = "Writing",
            IconHtml = "✍️",
            Section = "Academic",
            Badges = new[] { "Grammar" },
            Tags = new[] { "grammar", "writing", "ai" }
        },
        new ToolDefinition
        {
            Id = "formula-reference",
            Name = "Formula & Unit Reference",
            Description = "Reference for key math, physics, and chemistry formulas.",
            Controller = "Academic",
            Action = "FormulaReference",
            Category = "Reference",
            IconHtml = "📚",
            Section = "Academic",
            Badges = new[] { "Formulas" },
            Tags = new[] { "formulas", "reference" }
        },
        new ToolDefinition
        {
            Id = "whiteboard",
            Name = "Interactive Whiteboard",
            Description = "Draw diagrams and teach concepts on a digital whiteboard.",
            Controller = "Academic",
            Action = "Whiteboard",
            Category = "Collaboration",
            IconHtml = "🖊️",
            Section = "Academic",
            Badges = new[] { "Board" },
            Tags = new[] { "whiteboard", "teaching" }
        },
        new ToolDefinition
        {
            Id = "mind-map",
            Name = "Mind Map Creator",
            Description = "Visualize and organize ideas with mind maps.",
            Controller = "Academic",
            Action = "MindMap",
            Category = "Planning",
            IconHtml = "🧠",
            Section = "Academic",
            Badges = new[] { "Maps" },
            Tags = new[] { "mind map", "ideas" }
        },

        // Trending & AI
        new ToolDefinition
        {
            Id = "ai-writing-assistant",
            Name = "AI Writing Assistant",
            Description = "Draft content, summarize text, and improve writing with AI.",
            Controller = "Trending",
            Action = "AIWritingAssistant",
            Category = "AI",
            IconHtml = "🤖",
            Section = "Trending",
            Badges = new[] { "Writing", "AI" },
            Tags = new[] { "ai", "writing", "content" }
        },
        new ToolDefinition
        {
            Id = "poll-builder",
            Name = "Quick Poll Builder",
            Description = "Create instant polls and surveys with visual results.",
            Controller = "Trending",
            Action = "PollBuilder",
            Category = "Engagement",
            IconHtml = "📊",
            Section = "Trending",
            Badges = new[] { "Polls" },
            Tags = new[] { "polls", "survey" }
        },
        new ToolDefinition
        {
            Id = "rate-comparison",
            Name = "Rate Comparison",
            Description = "Compare multiple currencies and units side by side.",
            Controller = "Trending",
            Action = "RateComparison",
            Category = "Comparison",
            IconHtml = "📉",
            Section = "Trending",
            Badges = new[] { "Rates" },
            Tags = new[] { "rates", "currency", "compare" }
        },
        new ToolDefinition
        {
            Id = "encryption-tool",
            Name = "Encryption Tool",
            Description = "Encrypt and decrypt text securely in your browser.",
            Controller = "Trending",
            Action = "EncryptionTool",
            Category = "Security",
            IconHtml = "🔐",
            Section = "Trending",
            Badges = new[] { "Crypto" },
            Tags = new[] { "encryption", "crypto" }
        },
        new ToolDefinition
        {
            Id = "meme-generator",
            Name = "Meme Generator",
            Description = "Create funny memes in your browser.",
            Controller = "Trending",
            Action = "MemeGenerator",
            Category = "Fun",
            IconHtml = "🤣",
            Section = "Trending",
            Badges = new[] { "Memes" },
            Tags = new[] { "memes", "images" }
        }
    };

    private static readonly IReadOnlyDictionary<string, ToolDefinition> _byId =
        _allTools.ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<ToolDefinition> AllTools => _allTools;

    public static ToolDefinition? GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return _byId.TryGetValue(id, out var tool) ? tool : null;
    }

    public static IEnumerable<ToolDefinition> ForSection(string section)
    {
        if (string.IsNullOrWhiteSpace(section))
        {
            return Array.Empty<ToolDefinition>();
        }

        return _allTools.Where(t => string.Equals(t.Section, section, StringComparison.OrdinalIgnoreCase));
    }
}