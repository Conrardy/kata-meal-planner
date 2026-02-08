# DESIGN.md

## Design Implementation

- **Design System Approach**: Utility-first using Tailwind CSS with custom configuration
- **Styling Method**: Tailwind utility classes directly in HTML templates, CSS variables for core tokens

## Design System Files

- **Theme Config**: @frontend/tailwind.config.js, @frontend/src/styles.css
- **Design Components**: `frontend/src/app/shared/components/` (sidebar, reusable UI elements)
- **Style Guidelines**: Visual requirements defined in @docs/issues/issue-1.md, @docs/issues/issue-2.md, @docs/issues/issue-3.md, @docs/issues/issue-4.md

## Design System

- **Spacing Scale**: Tailwind default scale (0.25rem increments) - Applied via utility classes (`px-4`, `py-2`, `gap-3`, `space-y-1`)
- **Border Radius**: Tailwind default - `rounded-lg` (0.5rem), `rounded-xl` (0.75rem), `rounded-full` for badges
- **Shadows**: Tailwind default - `shadow-md` for elevated cards
- **Breakpoints**: Tailwind default - `lg:` prefix for desktop (1024px+)

- **Color Palette**: See @frontend/tailwind.config.js and @frontend/src/styles.css

  - Primary: `#FF8C00` (Orange) - CTAs, badges, active states, brand identity
  - Primary Light: `#FFD700` (Gold) - Borders, secondary actions, hover states
  - Gray Scale: Tailwind default - Text hierarchy (gray-900, gray-700, gray-400), borders (gray-200), backgrounds (gray-100, gray-50)
  - White: `#FFFFFF` - Cards, containers, sidebar background
  - Orange Hover: `orange-600` - Button hover states
  - Orange Background: `orange-50` - Active navigation states

- **Typography**: See @frontend/src/styles.css
  - Primary Font: System UI stack - Body text, all interfaces
  - System Stack: `-apple-system, BlinkMacSystemFont, Segoe UI, Roboto` - Cross-platform consistency
  - Fallback: `sans-serif`

## Component Standards and Variants

- **Button Variants**: 
  - Primary: `bg-primary text-white rounded-lg hover:bg-orange-600` - Main actions (Cook Now, Save)
  - Secondary: `border-2 border-primary-light text-primary rounded-lg hover:bg-orange-50` - Alternative actions (Swap Meal)
  - Text sizes: `text-sm font-medium` for button labels
  
- **Input States**: Not yet implemented - Defined in settings screens (@docs/issues/issue-2.md)
  - Dropdown: White background, light gray border, gray text placeholder
  
- **Card Patterns**: 
  - Meal Cards: `bg-white rounded-xl shadow-md` - Elevation via shadow, 12px spacing between cards
  - Recipe Cards: White background, subtle gray border - 16px padding, images 80x80px with 8px border radius
  - Layout: Relative positioning for badges, flex layouts for buttons

## Layout System

- **Grid System**: Flexbox-based - `flex`, `flex-col`, `flex-1` for responsive layouts
- **Container Widths**: Full width for mobile, `w-64` (16rem) for sidebar on desktop
- **Spacing Rules**: 
  - Card padding: `p-4` (1rem)
  - Button gaps: `gap-3` (0.75rem)
  - Navigation spacing: `space-y-1` (0.25rem)
  - Section margins: 12px vertical between meal cards

## Accessibility

- **Color Contrast**: WCAG AA compliance - Black text on white (`text-gray-900`), orange primary meets contrast requirements
- **Focus Management**: Hover states on all interactive elements (`hover:bg-gray-100`, `hover:bg-orange-50`)
- **Screen Reader**: `data-testid` attributes for testing, semantic HTML (`<article>`, `<nav>`, `<aside>`)
