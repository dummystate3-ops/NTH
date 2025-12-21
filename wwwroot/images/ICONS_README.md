# Icon Generation Instructions

## Required PWA Icons

Generate the following icon sizes for PWA support:

- 72x72
- 96x96
- 128x128
- 144x144
- 152x152
- 192x192
- 384x384
- 512x512

## Design Guidelines

- **Logo**: "NC" letters in white on gradient background
- **Gradient**: From #4A90E2 (primary) to #50E3C2 (secondary)
- **Shape**: Rounded square with 20% border radius
- **Font**: Bold, sans-serif (Inter or similar)
- **Export**: PNG format with transparency

## Tools for Generation

1. **Online**: 
   - https://realfavicongenerator.net/
   - https://favicon.io/

2. **Design Software**:
   - Figma
   - Adobe Illustrator
   - Canva

3. **Quick Generation**:
   ```
   Create a single 512x512 source image and use an icon generator
   to create all required sizes automatically.
   ```

## Placement

All icon files should be placed in:
```
wwwroot/images/icons/
```

## Favicon

Place `favicon.ico` in the `wwwroot/` directory.

## Default Open Graph Image

Create a default OG image (1200x630px) at:
```
wwwroot/images/og-default.png
```

Design should include:
- NovaCalc Hub logo
- Tagline: "Calculators & Tools for Everyone"
- Aurora Tech color scheme
