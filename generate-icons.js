const fs = require('fs');
const { createCanvas } = require('canvas');

const sizes = [72, 96, 128, 144, 152, 192, 384, 512];
const outputDir = './wwwroot/images/icons';

// Ensure output directory exists
if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir, { recursive: true });
}

sizes.forEach(size => {
    const canvas = createCanvas(size, size);
    const ctx = canvas.getContext('2d');

    // Create gradient background (Aurora Tech colors)
    const gradient = ctx.createLinearGradient(0, 0, size, size);
    gradient.addColorStop(0, '#4A90E2');  // Primary blue
    gradient.addColorStop(1, '#50E3C2');  // Secondary teal
    ctx.fillStyle = gradient;
    ctx.fillRect(0, 0, size, size);

    // Add rounded corners
    ctx.globalCompositeOperation = 'destination-in';
    ctx.beginPath();
    const radius = size * 0.15;
    ctx.moveTo(radius, 0);
    ctx.lineTo(size - radius, 0);
    ctx.quadraticCurveTo(size, 0, size, radius);
    ctx.lineTo(size, size - radius);
    ctx.quadraticCurveTo(size, size, size - radius, size);
    ctx.lineTo(radius, size);
    ctx.quadraticCurveTo(0, size, 0, size - radius);
    ctx.lineTo(0, radius);
    ctx.quadraticCurveTo(0, 0, radius, 0);
    ctx.closePath();
    ctx.fill();

    ctx.globalCompositeOperation = 'source-over';

    // Draw "NT" text
    const fontSize = size * 0.4;
    ctx.font = `bold ${fontSize}px Arial`;
    ctx.fillStyle = '#FFFFFF';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText('NT', size / 2, size / 2);

    // Save PNG
    const buffer = canvas.toBuffer('image/png');
    const filename = `${outputDir}/icon-${size}x${size}.png`;
    fs.writeFileSync(filename, buffer);
    console.log(`Created ${filename}`);
});

console.log('\nAll icons generated successfully!');
