using Microsoft.AspNetCore.Mvc;
using NovaToolsHub.Models.ViewModels;

namespace NovaToolsHub.Controllers
{
    public class ProductivityController : Controller
    {
        /// <summary>
        /// World Time Viewer - Display current time in major world cities
        /// </summary>
        public IActionResult WorldTime()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "World Time Viewer - Current Time Across Time Zones | NovaTools Hub",
                MetaDescription = "View current time in major cities worldwide. Real-time world clock with multiple time zones, perfect for international coordination and travel planning.",
                CanonicalUrl = "/productivity/world-time"
            };
            return View(model);
        }

        /// <summary>
        /// Time Zone Converter - Convert time between different time zones
        /// </summary>
        public IActionResult TimeZoneConverter()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Time Zone Converter - Convert Time Between Zones | NovaTools Hub",
                MetaDescription = "Convert time between time zones instantly. Schedule meetings, coordinate events, and plan international calls with our accurate time zone converter.",
                CanonicalUrl = "/productivity/time-zone-converter"
            };
            return View(model);
        }

        /// <summary>
        /// Stopwatch - Precision timing with lap support
        /// </summary>
        public IActionResult Stopwatch()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Online Stopwatch with Lap Timer | NovaTools Hub",
                MetaDescription = "Accurate online stopwatch with lap timing support. Perfect for workouts, cooking, studying, and any activity requiring precise time tracking.",
                CanonicalUrl = "/productivity/stopwatch"
            };
            return View(model);
        }

        /// <summary>
        /// Notes/Notepad - Simple note-taking with localStorage persistence
        /// </summary>
        public IActionResult Notes()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Online Notepad - Quick Notes with Auto-Save | NovaTools Hub",
                MetaDescription = "Simple, fast online notepad with automatic saving. Take quick notes, draft ideas, and store text snippets securely in your browser.",
                CanonicalUrl = "/productivity/notes"
            };
            return View(model);
        }

        /// <summary>
        /// Text-to-Speech - Convert text to spoken audio using browser API
        /// </summary>
        public IActionResult TextToSpeech()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Text to Speech Converter - Free Online TTS | NovaTools Hub",
                MetaDescription = "Convert text to speech online for free. Listen to documents, articles, and notes with natural-sounding voices using our text-to-speech tool.",
                CanonicalUrl = "/productivity/text-to-speech"
            };
            return View(model);
        }

        /// <summary>
        /// Time Tracker - Simple task and time tracking
        /// </summary>
        public IActionResult TimeTracker()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Time Tracker - Track Tasks and Projects | NovaTools Hub",
                MetaDescription = "Track time spent on tasks and projects. Simple time tracking tool for productivity monitoring, freelancers, and project management.",
                CanonicalUrl = "/productivity/time-tracker"
            };
            return View(model);
        }

        /// <summary>
        /// To-Do List - CRUD-enabled task management with localStorage
        /// </summary>
        public IActionResult TodoList()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "To-Do List - Task Manager with Priority Tracking | NovaTools Hub",
                MetaDescription = "Organize tasks with our smart to-do list. Create, edit, prioritize, and track your daily tasks with automatic saving and easy management.",
                CanonicalUrl = "/productivity/todo-list"
            };
            return View(model);
        }

        /// <summary>
        /// Pomodoro Timer - Focus timer with configurable work/break intervals
        /// </summary>
        public IActionResult PomodoroTimer()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Pomodoro Timer - Focus & Productivity Technique | NovaTools Hub",
                MetaDescription = "Boost productivity with the Pomodoro Technique. Configurable work/break intervals, session tracking, and browser notifications for focused work.",
                CanonicalUrl = "/productivity/pomodoro-timer"
            };
            return View(model);
        }

        /// <summary>
        /// Kanban Board - Visual task management with drag-and-drop
        /// </summary>
        public IActionResult KanbanBoard()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Kanban Board - Visual Task Management | NovaTools Hub",
                MetaDescription = "Manage projects visually with our Kanban board. Drag-and-drop tasks between columns, track progress, and boost team productivity.",
                CanonicalUrl = "/productivity/kanban-board"
            };
            return View(model);
        }
    }
}
