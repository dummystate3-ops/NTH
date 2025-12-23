# NovaTools Hub – Implementation Plan for Coding Agents

Date: 2025-12-21  
Related strategy docs:
- `docs/UI_UX_AUDIT_REPORT.md`
- `docs/UI_REDESIGN_PLAN.md`
- `docs/SEO_STRATEGY.md`
- `docs/MAJOR_TOOLS_AUDIT_REPORT.md`
- `docs/BACKGROUND_REMOVER_AUDIT.md`
- `docs/batch-processing-plan.md`

> **Purpose**: This document gives a **stepwise, non-ambiguous plan** for coding agents to implement the UI/UX and SEO strategies without guessing or hallucinating.  
> Always follow the guardrails in section 1 before editing code.

---

## 1. Global Guardrails (Must Follow)

1. **Never modify a file you haven’t read in this session.**
   - Use the repository tools (e.g., `filename_search`, `read_file`, or equivalent commands like `dir`, `cat`, `Get-Content`) to:
     - Confirm the file exists.
     - Read its full content before editing.

2. **Do not assume a file, route, or setting exists.**
   - If a plan step refers to `Controllers/ToolsController.cs`:
     - First confirm with a filename search.
     - If it does not exist:
       - Stop and update this plan or the relevant docs instead of inventing a new file name.

3. **Do not introduce new external dependencies unless explicitly instructed.**
   - For this plan:
     - Use the existing stack only:
       - ASP.NET Core MVC.
       - Tailwind CSS (already in `package.json`).
       - Existing frontend libs (`gsap` is allowed as it’s already present).
   - If you believe a new dependency is necessary, stop and leave a note instead of adding it.

4. **Prefer refactoring and extending existing patterns.**
   - Follow conventions from existing views, controllers, and CSS:
     - Use `container-custom`, `.section-padding`, `.card`, `.tool-card`, `.btn-*` where relevant.
     - Do not introduce inconsistent naming for similar components.

5. **Respect environment-specific behavior.**
   - AdSense and Analytics must remain consent-driven, as implemented in:
     - `Views/Shared/_AdSenseScript.cshtml`
     - `Views/Shared/_CookieConsent.cshtml`
   - Do not force ads or analytics to load without checking consent.

6. **Testing rule: no “fixed” claim without some verification.**
   - At minimum, after a change:
     - Build/run the application (`dotnet run`) or `dotnet test` if tests exist.
     - Manually verify relevant pages in a browser when possible.
   - If you cannot verify (e.g., no runtime available), explicitly mark changes as “untested”.

7. **No mass renames or structural changes that are not described here.**
   - Do not:
     - Rename namespaces, controllers, or routes globally.
     - Change the overall project structure.
   - Keep changes scoped and reversible.

8. **If reality and this plan disagree, reality wins.**
   - Example:
     - This plan expects `Views/Tools/Index.cshtml`, but it is missing or significantly different.
   - In that case:
     - Stop implementing that sub-step.
     - Add a small note/update in this document or the relevant audit doc instead of guessing.

---

## 2. Phased Implementation Overview

The work is split into phases. Within each phase, tasks must be executed **in order**.

1. **Phase A – Repository Discovery & Baseline Verification**
2. **Phase B – UI/UX System Foundation**
3. **Phase C – SEO & Information Architecture Foundation**
4. **Phase D – High-Value Tool Additions & Low-Value De-emphasis**
5. **Phase E – Ads & Monetization Experience**
6. **Phase F – Final QA, Performance & SEO Checks**

You do not have to complete all phases in one session, but **do not skip prerequisites** for a task.

---

## 3. Phase A – Repository Discovery & Baseline Verification

**Goal**: Confirm actual repository structure and align it with the strategy docs before making any UI or SEO changes.

### A.1: Confirm Key Files and Directories

**Required checks (all must succeed or be noted):**

1. Verify main directories:
   - `Views/`
   - `Views/Home/`
   - `Views/Tools/`
   - `Views/Shared/`
   - `wwwroot/css/`
   - `wwwroot/js/`
   - `Controllers/`
   - `docs/`

2. Verify the existence of the core layout and CSS:
   - `Views/Shared/_Layout.cshtml`
   - `wwwroot/css/site.css`
   - `tailwind.config.js`

3. Verify strategy docs are present:
   - `docs/UI_UX_AUDIT_REPORT.md`
   - `docs/UI_REDESIGN_PLAN.md`
   - `docs/SEO_STRATEGY.md`

If any expected file is missing:

- Do **not** create a replacement.
- Stop and add a short note at the end of this document under “Deviations from Expected Structure”.

### A.2: Scan Controllers for Tools and Routes

1. Use filename search to find controllers:
   - `Controllers/ToolsController.cs`
   - `Controllers/MathController.cs`
   - `Controllers/BusinessController.cs`
   - `Controllers/ProductivityController.cs`
   - `Controllers/AcademicController.cs`
   - `Controllers/TrendingController.cs`
   - `Controllers/SitemapController.cs` (if present).

2. For each controller found:
   - Read the file.
   - Note:
     - Action methods that render views.
     - Any API endpoints for tools (e.g., JSON endpoints).

**Do not** create a new controller if one is missing here unless a later phase explicitly requires it and the strategy specifically describes adding a new tool.

### A.3: Check Existing Tool Views

1. Confirm representative views:
   - `Views/Home/Index.cshtml`
   - `Views/Home/AllTools.cshtml`
   - `Views/Tools/Index.cshtml`
   - A few specific tools, e.g.:
     - `Views/Tools/UnitConverter.cshtml`
     - `Views/ImageTools/AdvancedResizer.cshtml`
     - `Views/ImageTools/BackgroundRemover.cshtml`

2. Read them to understand:
   - How tool cards are structured.
   - How the SEO-related metadata is set (titles/descriptions).
   - Where ads and sharing components are injected.

---

## 4. Phase B – UI/UX System Foundation

**Goal**: Implement the shared design system and key components described in `UI_REDESIGN_PLAN.md`, without altering business logic.

### B.1: Extend Design Tokens in `site.css`

**Inputs**:
- `wwwroot/css/site.css`
- `docs/UI_REDESIGN_PLAN.md` (color/surface token suggestions)

**Steps**:

1. Open `site.css` and locate the `:root { ... }` block and `.dark { ... }`.
2. Add or adjust CSS variables to include (names are suggestions; adapt carefully):
   - Surface tokens:
     - `--surface-base`, `--surface-raised`, `--surface-higher`
   - Text tokens:
     - `--text-strong`, `--text-default`, `--text-subtle`
   - Border tokens:
     - `--border-subtle`, `--border-strong`
3. Map existing usage:
   - Wherever `--color-bg`, `--color-surface`, `--color-text-dark` etc. are used, **do not remove** them.
   - Optionally, re-define some of them in terms of the new tokens if it does not break existing UI.
4. Update `.dark` to override the same set of variables consistently.

**Guardrails**:

- Do not remove the existing variables; only add or remap.
- After edits:
  - Run `npm run build:css`.
  - If build fails, revert to last known good state and correct the syntax.

### B.2: Tailwind Config Alignment

**Inputs**:
- `tailwind.config.js`
- `wwwroot/css/site.css`

**Steps**:

1. Read `tailwind.config.js`.
2. If necessary, add semantic color names that align with the new tokens:
   - `primary`, `secondary`, `accent`, etc., should already exist.
3. Ensure `content` includes:
   - `./Views/**/*.cshtml`
   - `./Areas/**/*.cshtml`
   - `./wwwroot/js/**/*.js`
   (Only adjust if the actual repo paths differ.)

**Guardrails**:

- Do not remove existing `content` paths unless they are clearly wrong.
- Do not add plugins unless explicitly requested.

### B.3: Introduce Shared UI Partials (If Not Present)

This phase defines reusable components but **only add them if they do not already exist**.

**Target partials** (under `Views/Shared/`):

1. `_SectionHeader.cshtml` – reusable section header (title, subtitle, optional icon/eyebrow).
2. `_ToolCard.cshtml` – reusable tool card markup.

**Steps**:

1. Search for any existing partials with similar responsibility, e.g.:
   - `_ToolCard`, `_SectionHeader`, `_FeatureCard`
   - If something similar exists, use it instead of adding new files.
2. If no such partials exist:
   - Create `_SectionHeader.cshtml` with:
     - Model or `ViewData`/`ViewBag` pattern that accepts:
       - `Title`, `Subtitle`, optional icon markup.
   - Create `_ToolCard.cshtml` that:
     - Accepts a small model or uses `ViewData` for:
       - `Title`, `Description`, `Url`, `IconHtml`, optional tags.
3. Use existing classes:
   - `.tool-card`, `.card`, `.tool-icon`, `.container-custom`, `.section-padding`.

**Guardrails**:

- Do not refactor any view to use these partials yet; just add the partials.
- Keep the markup closely aligned with what `Home/Index` and `Home/AllTools` currently use.

### B.4: Small Layout Enhancements in `_Layout.cshtml`

**Goal**: Add accessibility and minor structure improvements.

**Steps**:

1. Open `Views/Shared/_Layout.cshtml`.
2. Add a **skip link** at the top of `<body>`:
   - Example: a visually hidden `a href="#main-content"` that becomes visible on focus.
3. Ensure `<main>` has an `id="main-content"` or similar, to be the target of the skip link.
4. Do not change existing meta tags, PWA tags, or script includes beyond this.

**Guardrails**:

- Keep the header, navigation, and footer structures intact.
- Do not alter ads or analytics wiring here.

---

## 5. Phase C – SEO & Information Architecture Foundation

**Goal**: Implement technical SEO improvements and define hubs without changing business logic.

### C.1: Canonical URL & Title Consistency

**Steps**:

1. Verify how `ViewBag.PageTitle`, `ViewBag.MetaDescription`, and `ViewBag.CanonicalUrl` are set:
   - Read `Views/_ViewStart.cshtml`.
   - Sample a few views (e.g., `Home/Index`, `Home/AllTools`, `Tools/UnitConverter`).
2. Ensure `_Layout.cshtml`:
   - Uses `@ViewBag.CanonicalUrl` in the `<link rel="canonical">` when present.
3. For any high-priority tool page where `ViewBag.CanonicalUrl` is missing:
   - Add a consistent assignment in the view or controller:
     - Use the actual route the page is served from.
4. Do not invent slugs; use the current route unless strategy docs explicitly define a new one.

### C.2: Structured Data Helpers

**Goal**: Standardize JSON-LD for tools.

**Steps**:

1. Search for existing SEO helpers (e.g., `SeoHelper.GenerateSoftwareApplicationSchema`):
   - Confirm definition (likely under `Helpers/` or similar).
2. For each priority tool page (list in Phase D), ensure there is:
   - A `<script type="application/ld+json">` block output via the helper.
3. If a helper does not exist yet:
   - Before creating one, confirm:
     - There is no other utility performing similar JSON-LD generation.
   - Only then, create or extend a helper in the appropriate `Helpers/` file.

**Guardrails**:

- Use existing patterns (as seen in `UnitConverter`) as the template.
- Do not add arbitrary properties to JSON-LD beyond what is already used unless you can reference official schema docs.

### C.3: Hubs and Internal Linking

> Implementation of hubs is coordinated with tool changes in Phase D.  
> In this phase, you only prepare the structure, not the content.

**Steps**:

1. Identify if any dedicated hub views already exist:
   - `Views/Finance/Index.cshtml` (example, may not exist).
   - `Views/ImageTools/Index.cshtml` (if present).
2. If no dedicated hub views exist beyond `Home/AllTools` and `Tools/Index`:
   - Do **not** create new folders yet.
   - Document this finding in a note at the end of this file.

Hubs will be implemented/refined in Phase D with concrete tool lists.

---

## 6. Phase D – High-Value Tool Additions & Low-Value De-Emphasis

**Goal**: Add new, SEO-valuable tools and adjust navigation to emphasize them, while keeping the tool ecosystem stable.

### D.1: Define Priority Tool Set

Use `docs/SEO_STRATEGY.md` for the **priority list**. At minimum:

- Global finance:
  - Loan/EMI calculator.
  - Compound interest/savings calculator.
- Pakistan finance:
  - Pakistan salary/income tax calculator.
  - Pakistan zakat calculator.
- Optionally:
  - PKR EMI calculator variant.
  - First PDF tool (e.g. “Merge PDF”).

Before coding:

1. Confirm which of these already exist in some form:
   - Search controllers and views for “Loan”, “EMI”, “Tax”, “Zakat”, “PDF”.
2. If any are partially implemented or named differently:
   - Prefer enhancing those instead of adding duplicates.

### D.2: Adding a New Tool (Pattern)

If a required tool does **not** exist at all, follow this pattern.  
Example: “Loan Calculator” under `/Business/LoanCalculator`.

**Steps**:

1. Confirm the appropriate controller exists:
   - `Controllers/BusinessController.cs`.
     - If not found:
       - Verify another controller with finance-related tools exists.
       - Only if none exists, consider extending an existing controller (e.g., `ToolsController`) rather than creating a new one.
       - Document any deviation.

2. Add an action:
   - Example signature:
     - `public IActionResult LoanCalculator() => View();`
   - Do not add parameters unless already used in similar actions.

3. Confirm strategy docs are present:
   - `docs/UI_UX_AUDIT_REPORT.md`
   - `docs/UI_REDESIGN_PLAN.md`
   - `docs/SEO_STRATEGY.md`reate the view:
   - `Views/Business/LoanCalculator.cshtml`
   - Base it on an existing calculator template (e.g., `Views/Tools/BmiCalculator.cshtml` or `UnitConverter.cshtml` for layout).
   - Include:
     - H1, description, usage instructions.
     - A form or inputs for amount, rate, term.
     - Inline JS or separate JS using the same pattern as existing tools.
     - JSON-LD block using SEO helper.

4. Integrate into All Tools and hubs:
   - Add a card in `Views/Tools/Index.cscshtml` under the appropriate category (e.g., Business & Finance).
   - Add a card in `Views/Tools/Index.cshtml` if relevant.

**Guardrails**:

- Follow existing naming and style conventions.
- Do not add new routes in `Program.cs` unless absolutely required.

### D.3: Pakistan-Specific Tools

For:

- Pakistan salary tax calculator.
- Pakistan zakat calculator.

**Steps** (for each):

1. Check if any Pakistan-specific tools already exist.
2. If not, decide on a route and placement:
   - Example:
     - `Controllers/BusinessController.PakistanTaxCalculator`
     - View: `Views/Business/PakistanTaxCalculator.cshtml`
   - Or:
     - Introduce a dedicated `PakistanController` if the codebase already has country-specific patterns. Only do this if an existing pattern is found in controllers.
3. Implement the view:
   - Carefully hard-code FBR slabs or nisab thresholds **only when you have real values**.  
     - If not available in this repo or current docs, leave TODO comments and do **not** guess numbers.
   - Build content:
     - Explain the calculation and year of applicability.
     - Add FAQs.

4. SEO:
   - Use localized titles:
     - “Pakistan Income Tax Calculator – Salary Tax 2025–26 (PKR)”
   - Add FAQ schema (if helper exists) for common questions.

**Guardrails**:

- Do not fetch real-time data from external APIs unless the project already does this elsewhere.
- If you cannot verify current tax slabs or nisab values from in-repo docs, do not invent them.

### D.4: Low-Value Tools – De-Emphasis (Not Deletion)

Tools to de-emphasize are described in `docs/SEO_STRATEGY.md` (e.g., Meme Generator, Recipe Generator, generic Whiteboard, Mind Map).

**Steps**:

1. Confirm these tools’ views and nav entries exist.
2. **Do not delete** controllers or views.
3. Remove or reduce prominence:
   - Remove their cards from:
     - Home hero “Featured Tools” (if present).
     - Top sections in `Home/AllTools` and `Tools/Index`.
   - Optionally, move them to a “More Tools” or lower-priority section.

**Guardrails**:

- Keep direct URLs working to avoid 404s.
- Do not change their routes or action names.

---

## 7. Phase E – Ads & Monetization Experience

**Goal**: Make ad placements consistent, non-disruptive, and SEO-safe.

### E.1: Normalize Ad Partials

Relevant files:

- `Views/Shared/_AdInContent.cshtml`
- `Views/Shared/_AdSidebar.cshtml`
- `Views/Shared/_AdFooter.cshtml`
- `Views/Shared/_AdSenseScript.cshtml`
- `Views/Shared/_CookieConsent.cshtml`

**Steps**:

1. Read each ad partial and note:
   - Where the actual `<ins class="adsbygoogle">` is.
   - Where the placeholder is.
   - How `ViewBag.EnableAds` and other flags are used.

2. Update logic so that:
   - When real ads are enabled (PublisherId present and environment = production and consent given):
     - Show the `<ins>` block.
     - Hide or comment-out the placeholder.
   - When ads are disabled or in development:
     - Show only the placeholder.

3. Do not change consent flow; it must remain:
   - Cookie consent code → AdSense init function.

**Guardrails**:

- Do not hard-code publisher ID or ad slot values; leave placeholders where strategy docs expect real IDs to be filled via configuration.
- Avoid layout shifts:
   - Maintain a consistent outer container height where possible.

### E.2: Standard Ad Layout Patterns

**Steps** (pattern only; apply gradually):

1. Identify “standard calculator” pages (e.g., Unit Converter, BMI, Age, Date).
2. For each:
   - Ensure there is one in-content ad below the main card using `_AdInContent`.
3. For more complex tools (image tools, AI tools):
   - Prefer a sidebar ad (`_AdSidebar`) on desktop, or below content on mobile.

**Guardrails**:

- Do not add more than 2 ad units per tool page unless explicitly requested.
- Ensure “Advertisement” labels remain visible.

---

## 8. Phase F – Final QA, Performance & SEO Checks

**Goal**: Validate that the changes are stable, performant, and SEO-friendly.

### F.1: Functional QA

1. Build and run the application:
   - `dotnet run`
2. Navigate through:
   - Home.
   - All Tools.
   - Tools Index.
   - Newly added calculators.
   - Pakistan-specific tools.
3. Verify:
   - No runtime errors.
   - Navigation is consistent.
   - Ads appear only after consent (if running with realistic configuration).

### F.2: Visual QA

1. Check responsive behavior:
   - Mobile (narrow width).
   - Tablet.
   - Desktop.
2. Confirm:
   - Cards, buttons, and fonts align with the new design system.
   - Skip link appears and works when tabbing from the top.

### F.3: SEO & Performance Spot Checks

1. Use browser dev tools or automated tools (if available) to check:
   - Core web vitals where possible.
   - Presence of:
     - `<title>`, `<meta name="description">`
     - Canonical `<link>`
     - JSON-LD on priority tools.
2. Verify sitemap (if `SitemapController` exists):
   - Only includes live, high-value routes.

---

## 9. Deviations Log (For Agents to Update)

If you encounter discrepancies between this plan and the actual repo, record them here instead of guessing:

- Example:
  - `[2025-12-21] SitemapController not found. Sitemap is likely generated differently; skipping Phase C.3 modifications for now.`
  - `[2025-12-21] BusinessController already has a LoanCalculator action; reused it instead of creating a new one.`

Agents should append brief entries rather than rewriting this document.

---

This implementation plan must be considered the authoritative guide for coding agents working on NovaTools Hub’s UI/UX and SEO-related changes.  
When in doubt: **pause, verify with the repo, and document deviations instead of guessing.**