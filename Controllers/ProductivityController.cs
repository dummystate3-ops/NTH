using Microsoft.AspNetCore.Mvc;
using NovaToolsHub.Models.ViewModels;
using NovaToolsHub.Helpers;
using System.Collections.Generic;

namespace NovaToolsHub.Controllers
{
    public class ProductivityController : Controller
    {
        /// <summary>
        /// World Time Viewer - Display current time in major world cities
        /// </summary>
        public IActionResult WorldTime()
        {
            var url = Url.Action("WorldTime", "Productivity", null, Request.Scheme) ?? string.Empty;

            var model = new BasePageViewModel
            {
                PageTitle = "World Time Viewer - Current Time Across Time Zones | NovaTools Hub",
                MetaDescription = "View current time in major cities worldwide. Real-time world clock with multiple time zones, perfect for international coordination and travel planning.",
                CanonicalUrl = url
            };

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "World Time Viewer",
                "View current time in major cities worldwide with live time zone information and custom city support.",
                url,
                "UtilityApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "How accurate is the world time shown in this tool?",
                    "The clocks use your device time and the browser's time zone database (IANA time zones) to calculate local times for each city. As long as your system clock and time zone are configured correctly, the displayed times should be accurate for everyday use."
                ),
                (
                    "Will my custom cities be saved for future visits?",
                    "Yes. Custom cities you add are stored in your browser's local storage, so they reappear when you return from the same device and browser. They are not synced across devices or user accounts."
                ),
                (
                    "Does the World Time Viewer handle daylight saving time changes?",
                    "Yes. Time calculations rely on the browser's time zone rules, which automatically adjust for daylight saving time where applicable."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View(model);
        }

        /// <summary>
        /// Time Zone Converter - Convert time between different time zones
        /// </summary>
        public IActionResult TimeZoneConverter()
        {
            var url = Url.Action("TimeZoneConverter", "Productivity", null, Request.Scheme) ?? string.Empty;

            var model = new BasePageViewModel
            {
                PageTitle = "Time Zone Converter - Convert Time Between Zones | NovaTools Hub",
                MetaDescription = "Convert time between time zones instantly. Schedule meetings, coordinate events, and plan international calls with our accurate time zone converter.",
                CanonicalUrl = url
            };

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Time Zone Converter",
                "Convert time between different time zones instantly with automatic daylight saving time handling.",
                url,
                "UtilityApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "Does the time zone converter handle daylight saving time automatically?",
                    "Yes. The converter uses the browser's time zone database (IANA time zones), which automatically applies daylight saving time rules where they are in effect."
                ),
                (
                    "Is this tool suitable for scheduling international meetings?",
                    "It is designed to help you quickly see what a given date and time looks like in another time zone, which is ideal for planning calls and meetings. For critical events, always double‑check with your calendar or meeting tool as well."
                ),
                (
                    "Why might the converted time differ from what another app shows?",
                    "Differences usually come from mismatched system time, outdated time zone rules on a device, or using a different reference time zone. Make sure your device's clock and time zone are correct and cross‑check when planning important events."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View(model);
        }

        /// <summary>
        /// Stopwatch - Precision timing with lap support
        /// </summary>
        public IActionResult Stopwatch()
        {
            var url = Url.Action("Stopwatch", "Productivity", null, Request.Scheme) ?? string.Empty;

            var model = new BasePageViewModel
            {
                PageTitle = "Online Stopwatch with Lap Timer | NovaTools Hub",
                MetaDescription = "Accurate online stopwatch with lap timing support. Perfect for workouts, cooking, studying, and any activity requiring precise time tracking.",
                CanonicalUrl = url
            };

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Online Stopwatch",
                "Accurate online stopwatch with lap timing and basic statistics, ideal for workouts, study sessions, and everyday timing.",
                url,
                "UtilityApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "How precise is this online stopwatch?",
                    "The stopwatch updates every 10 milliseconds using your browser's timer APIs. This is more than precise enough for everyday timing, but it is not intended for laboratory‑grade or official competition timing."
                ),
                (
                    "Are my lap times saved if I refresh or close the page?",
                    "Lap times are stored only in memory while the page is open. If you refresh or close the tab, the current timer state and laps are cleared."
                ),
                (
                    "Can I control the stopwatch with the keyboard?",
                    "Yes. You can use the Space bar to start or stop the timer, the L key to record a lap while it is running, and the R key to reset."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View(model);
        }

        /// <summary>
        /// Notes/Notepad - Simple note-taking with workspace-backed persistence
        /// </summary>
        public IActionResult Notes()
        {
            var url = Url.Action("Notes", "Productivity", null, Request.Scheme) ?? string.Empty;

            var model = new BasePageViewModel
            {
                PageTitle = "Online Notepad - Quick Notes with Auto-Save | NovaTools Hub",
                MetaDescription = "Simple, fast online notepad with automatic saving. Take quick notes, draft ideas, and store text snippets in your browser workspace.",
                CanonicalUrl = url
            };

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Online Notepad",
                "Take quick notes and drafts in a browser-based notepad with automatic saving and workspace-backed storage.",
                url,
                "ProductivityApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "Where are my notes stored when I use this online notepad?",
                    "Notes are stored in a workspace associated with your browser, using a combination of local identifiers and server-side storage. This lets you come back to the same browser and continue where you left off without creating an account."
                ),
                (
                    "Is this notepad suitable for storing sensitive or confidential information?",
                    "No. Treat this as a convenience tool for everyday notes, drafts, and ideas, not as an encrypted vault. Avoid storing passwords, highly sensitive personal data, or confidential production information."
                ),
                (
                    "Can I export or back up my notes?",
                    "Yes. You can download the current note as a plain text file or copy its contents to your clipboard for backup in your preferred notes or document system."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View(model);
        }

        /// <summary>
        /// Text-to-Speech - Convert text to spoken audio using browser API
        /// </summary>
        public IActionResult TextToSpeech()
        {
            var url = Url.Action("TextToSpeech", "Productivity", null, Request.Scheme) ?? string.Empty;

            var model = new BasePageViewModel
            {
                PageTitle = "Text to Speech Converter - Free Online TTS | NovaTools Hub",
                MetaDescription = "Convert text to speech online for free. Listen to documents, articles, and notes with natural-sounding voices using our text-to-speech tool.",
                CanonicalUrl = url
            };

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Text to Speech Converter",
                "Convert text to natural-sounding speech in your browser using the built-in Speech Synthesis API.",
                url,
                "UtilityApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "Does my text get sent to a server when I use text-to-speech?",
                    "The tool uses your browser's built-in Speech Synthesis API to generate audio locally. The app itself does not send your text to NovaTools Hub servers, though your browser or operating system may use underlying voice engines depending on your platform."
                ),
                (
                    "Which browsers work best with this text-to-speech tool?",
                    "Modern versions of Chrome, Edge, and Safari generally offer the best support and the widest range of voices. Firefox support is more limited and may not expose all features."
                ),
                (
                    "Can I download the spoken audio as a file?",
                    "The Web Speech API does not provide a direct way to export audio. If you need a recording, you can use screen or system audio recording tools while the text is being spoken."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View(model);
        }

        /// <summary>
        /// Time Tracker - Simple task and time tracking
        /// </summary>
        public IActionResult TimeTracker()
        {
            var url = Url.Action("TimeTracker", "Productivity", null, Request.Scheme) ?? string.Empty;

            var model = new BasePageViewModel
            {
                PageTitle = "Time Tracker - Track Tasks and Projects | NovaTools Hub",
                MetaDescription = "Track time spent on tasks and projects. Simple time tracking tool for productivity monitoring, freelancers, and project management.",
                CanonicalUrl = url
            };

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Time Tracker",
                "Track time spent on tasks and projects in your browser with simple project grouping and CSV export.",
                url,
                "BusinessApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "Where is my time tracking data stored?",
                    "Projects and time entries are stored in your browser's local storage. They persist on this device and browser, but are not synced across devices or accounts."
                ),
                (
                    "Can I export my tracked time?",
                    "Yes. You can export your time entries as a CSV file and open it in Excel, Google Sheets, or other reporting tools for further analysis or invoicing."
                ),
                (
                    "Is this time tracker accurate enough for billing clients?",
                    "The timer is suitable for everyday tracking and generating approximate timesheets. For strict billing or compliance requirements, you may want to cross‑check with your primary time tracking or invoicing system."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View(model);
        }

        /// <summary>
        /// To-Do List - CRUD-enabled task management backed by a workspace
        /// </summary>
        public IActionResult TodoList()
        {
            var url = Url.Action("TodoList", "Productivity", null, Request.Scheme) ?? string.Empty;

            var model = new BasePageViewModel
            {
                PageTitle = "To-Do List - Task Manager with Priority Tracking | NovaTools Hub",
                MetaDescription = "Organize tasks with our smart to-do list. Create, edit, prioritize, and track your daily tasks with automatic saving and easy management.",
                CanonicalUrl = url
            };

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "To-Do List Manager",
                "Manage tasks with priorities, categories, and completion tracking in a browser-based to-do list linked to your workspace.",
                url,
                "UtilityApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "How does the to-do list store my tasks?",
                    "Tasks are stored in a workspace associated with your browser using NovaTools Hub's productivity API. This lets you return from the same browser and continue managing your list without creating a traditional login."
                ),
                (
                    "Will my tasks sync between different devices or browsers?",
                    "By default, workspaces are tied to your current browser environment. The tool is designed for convenient personal use on the same device rather than full multi-device account sync."
                ),
                (
                    "Is it safe to store sensitive information in task titles?",
                    "You should avoid putting highly sensitive data, passwords, or confidential information into task titles. Treat the to-do list as a convenience planner, not a secure secrets manager."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View(model);
        }

        /// <summary>
        /// Pomodoro Timer - Focus timer with configurable work/break intervals
        /// </summary>
        public IActionResult PomodoroTimer()
        {
            var url = Url.Action("PomodoroTimer", "Productivity", null, Request.Scheme) ?? string.Empty;

            var model = new BasePageViewModel
            {
                PageTitle = "Pomodoro Timer - Focus & Productivity Technique | NovaTools Hub",
                MetaDescription = "Boost productivity with the Pomodoro Technique. Configurable work/break intervals, session tracking, and browser notifications for focused work.",
                CanonicalUrl = url
            };

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Pomodoro Timer",
                "Use a Pomodoro timer with configurable work and break intervals, session counters, and optional browser notifications.",
                url,
                "UtilityApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "What happens if I close the tab while a Pomodoro session is running?",
                    "The timer runs in your browser. If you close the tab or refresh the page, the active countdown stops, though completed sessions from the current day are kept in local history."
                ),
                (
                    "Do I need to enable notifications for the timer to work?",
                    "Notifications are optional. The timer itself works without them, but enabling browser notifications lets you receive alerts when work or break sessions end, even if the tab is in the background."
                ),
                (
                    "Can I customize work and break durations?",
                    "Yes. You can adjust work, short break, and long break durations in the settings panel and choose whether breaks should auto‑start after a work session."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View(model);
        }

        /// <summary>
        /// Kanban Board - Visual task management with drag-and-drop
        /// </summary>
        public IActionResult KanbanBoard()
        {
            var url = Url.Action("KanbanBoard", "Productivity", null, Request.Scheme) ?? string.Empty;

            var model = new BasePageViewModel
            {
                PageTitle = "Kanban Board - Visual Task Management | NovaTools Hub",
                MetaDescription = "Manage projects visually with our Kanban board. Drag-and-drop tasks between columns, track progress, and boost team productivity.",
                CanonicalUrl = url
            };

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Kanban Board",
                "Visual task management board with drag-and-drop cards backed by a workspace-aware productivity API.",
                url,
                "BusinessApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "How are my Kanban cards stored?",
                    "Cards are stored in a workspace associated with your browser using NovaTools Hub's productivity API. This lets you reopen the board in the same browser and continue where you left off without a full account system."
                ),
                (
                    "Is this Kanban board intended for team-wide project management?",
                    "It is best suited for personal workflows, small experiments, or lightweight planning. It does not include advanced team features like roles, permissions, or real-time multi-user collaboration."
                ),
                (
                    "Can I share my Kanban board with someone else?",
                    "There is no built-in multi-user sharing or login for boards. If you need to collaborate, you can export summaries or screenshots and use a dedicated team project management tool for shared access."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View(model);
        }
    }
}
