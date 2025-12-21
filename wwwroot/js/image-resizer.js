// Advanced Image Resizer interactions
// Handles drag-and-drop, mode switching, single/batch processing, and UI updates

const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
const MAX_BATCH_FILES = 10;
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];

let currentMode = 'single';
let currentDownloadKey = null;
let cropper = null;
let originalPreviewUrl = null;
const batchOriginalMap = new Map();

document.addEventListener('DOMContentLoaded', function () {
    setMode('single');
    initializeDragAndDrop();
    initializeSliders();
    initializeFormControls();
    initializeWatermarkOpacity();
    initializeCropPresetListener();
});

// Mode management
function setMode(mode) {
    currentMode = mode;
    const singleBtn = document.getElementById('singleModeBtn');
    const batchBtn = document.getElementById('batchModeBtn');
    const fileInput = document.getElementById('fileInput');

    if (!singleBtn || !batchBtn || !fileInput) return;

    if (mode === 'single') {
        singleBtn.classList.add('bg-blue-600', 'text-white');
        singleBtn.classList.remove('bg-gray-200', 'text-gray-700');
        batchBtn.classList.remove('bg-blue-600', 'text-white');
        batchBtn.classList.add('bg-gray-200', 'text-gray-700');
        fileInput.multiple = false;
        document.getElementById('batchResults')?.classList.add('hidden');
    } else {
        batchBtn.classList.add('bg-blue-600', 'text-white');
        batchBtn.classList.remove('bg-gray-200', 'text-gray-700');
        singleBtn.classList.remove('bg-blue-600', 'text-white');
        singleBtn.classList.add('bg-gray-200', 'text-gray-700');
        fileInput.multiple = true;
    }

    clearResults();
}

// Drag and drop initialization
function initializeDragAndDrop() {
    const dropZone = document.getElementById('dropZone');
    const fileInput = document.getElementById('fileInput');

    if (!dropZone || !fileInput) return;

    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        dropZone.addEventListener(eventName, preventDefaults, false);
        document.body.addEventListener(eventName, preventDefaults, false);
    });

    ['dragenter', 'dragover'].forEach(eventName => {
        dropZone.addEventListener(eventName, highlight, false);
    });

    ['dragleave', 'drop'].forEach(eventName => {
        dropZone.addEventListener(eventName, unhighlight, false);
    });

    dropZone.addEventListener('drop', handleDrop, false);

    fileInput.addEventListener('change', function (e) {
        if (!e.target.files || e.target.files.length === 0) return;
        if (currentMode === 'batch') {
            handleBatchSelection(e.target.files);
        } else {
            handleFile(e.target.files[0]);
        }
    });

    dropZone.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            fileInput.click();
        }
    });

    dropZone.addEventListener('click', function (e) {
        if (e.target !== fileInput) {
            fileInput.click();
        }
    });
}

function preventDefaults(e) {
    e.preventDefault();
    e.stopPropagation();
}

function highlight() {
    const dz = document.getElementById('dropZone');
    if (dz) {
        dz.classList.add('border-[#4A90E2]', 'bg-blue-50');
    }
}

function unhighlight() {
    const dz = document.getElementById('dropZone');
    if (dz) {
        dz.classList.remove('border-[#4A90E2]', 'bg-blue-50');
    }
}

function handleDrop(e) {
    const dt = e.dataTransfer;
    const files = dt?.files;
    if (!files || files.length === 0) return;

    const fileInput = document.getElementById('fileInput');
    if (fileInput) {
        fileInput.files = files;
    }

    if (currentMode === 'batch') {
        handleBatchSelection(files);
    } else {
        handleFile(files[0]);
    }
}

function validateFile(file) {
    if (!ALLOWED_TYPES.includes(file.type)) {
        showClientError('Invalid file type. Please upload a JPG, PNG, WebP, or GIF image.');
        return false;
    }

    if (file.size > MAX_FILE_SIZE) {
        showClientError(`File size cannot exceed 10MB. Your file is ${(file.size / (1024 * 1024)).toFixed(2)}MB.`);
        return false;
    }

    return true;
}

function handleFile(file) {
    if (!validateFile(file)) return;

    hideClientError();

    if (originalPreviewUrl) {
        URL.revokeObjectURL(originalPreviewUrl);
    }
    originalPreviewUrl = URL.createObjectURL(file);

    document.getElementById('dropZoneContent')?.classList.add('hidden');
    document.getElementById('filePreview')?.classList.remove('hidden');
    document.getElementById('fileName').textContent = file.name;
    document.getElementById('fileSize').textContent = formatFileSize(file.size);
    document.getElementById('processButton').disabled = false;

    initializeCropper(originalPreviewUrl);
}

function handleBatchSelection(files) {
    if (!files || files.length === 0) {
        showClientError('Please select at least one image.');
        return;
    }

    if (files.length > MAX_BATCH_FILES) {
        showClientError(`Maximum ${MAX_BATCH_FILES} images allowed in batch mode.`);
        return;
    }

    batchOriginalMap.clear();
    if (originalPreviewUrl) {
        URL.revokeObjectURL(originalPreviewUrl);
        originalPreviewUrl = null;
    }

    for (const file of files) {
        if (!validateFile(file)) {
            return;
        }
        const baseName = file.name.replace(/\.[^/.]+$/, '');
        const url = URL.createObjectURL(file);
        batchOriginalMap.set(baseName.toLowerCase(), url);
    }

    hideClientError();

    const totalSize = Array.from(files).reduce((sum, f) => sum + f.size, 0);
    document.getElementById('dropZoneContent')?.classList.add('hidden');
    document.getElementById('filePreview')?.classList.remove('hidden');
    document.getElementById('fileName').textContent = `${files.length} files selected`;
    document.getElementById('fileSize').textContent = formatFileSize(totalSize);
    document.getElementById('processButton').disabled = false;

    // Use first file for crop preview
    const first = files[0];
    if (first) {
        const url = batchOriginalMap.get(first.name.replace(/\.[^/.]+$/, '').toLowerCase());
        initializeCropper(url);
    }
}

function showClientError(message) {
    const errorDiv = document.getElementById('clientError');
    const errorMessage = document.getElementById('clientErrorMessage');
    if (errorMessage) errorMessage.textContent = message;
    if (errorDiv) errorDiv.classList.remove('hidden');
    const processBtn = document.getElementById('processButton');
    if (processBtn) processBtn.disabled = true;
}

function hideClientError() {
    const errorDiv = document.getElementById('clientError');
    if (errorDiv) {
        errorDiv.classList.add('hidden');
    }
}

function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
}

// Slider initialization
function initializeSliders() {
    const qualitySlider = document.getElementById('quality');
    const qualityDisplay = document.getElementById('qualityDisplay');
    if (qualitySlider && qualityDisplay) {
        qualitySlider.addEventListener('input', function () {
            qualityDisplay.textContent = this.value + '%';
        });
    }

    const percentageSlider = document.getElementById('resizePercentage');
    const percentageDisplay = document.getElementById('percentageDisplay');
    if (percentageSlider && percentageDisplay) {
        percentageSlider.addEventListener('input', function () {
            percentageDisplay.textContent = this.value + '%';
        });
    }

    const brightnessSlider = document.getElementById('brightness');
    const brightnessDisplay = document.getElementById('brightnessDisplay');
    if (brightnessSlider && brightnessDisplay) {
        brightnessSlider.addEventListener('input', function () {
            brightnessDisplay.textContent = parseFloat(this.value).toFixed(1);
        });
    }

    const contrastSlider = document.getElementById('contrast');
    const contrastDisplay = document.getElementById('contrastDisplay');
    if (contrastSlider && contrastDisplay) {
        contrastSlider.addEventListener('input', function () {
            contrastDisplay.textContent = parseFloat(this.value).toFixed(1);
        });
    }
}

function initializeWatermarkOpacity() {
    const opacitySlider = document.getElementById('watermarkOpacity');
    const opacityDisplay = document.getElementById('opacityDisplay');
    if (opacitySlider && opacityDisplay) {
        opacitySlider.addEventListener('input', function () {
            opacityDisplay.textContent = this.value + '%';
        });
    }
}

// Form controls initialization
function initializeFormControls() {
    const widthInput = document.getElementById('targetWidth');
    const heightInput = document.getElementById('targetHeight');
    const percentageSlider = document.getElementById('resizePercentage');
    const socialPreset = document.getElementById('socialPreset');

    if (widthInput && percentageSlider) {
        widthInput.addEventListener('input', function () {
            if (this.value) {
                percentageSlider.value = 100;
                document.getElementById('percentageDisplay').textContent = '100%';
            }
        });
    }

    if (heightInput && percentageSlider) {
        heightInput.addEventListener('input', function () {
            if (this.value) {
                percentageSlider.value = 100;
                document.getElementById('percentageDisplay').textContent = '100%';
            }
        });
    }

    if (percentageSlider && widthInput && heightInput) {
        percentageSlider.addEventListener('input', function () {
            if (this.value !== '100') {
                widthInput.value = '';
                heightInput.value = '';
            }
        });
    }

    if (socialPreset) {
        socialPreset.addEventListener('change', function () {
            applySocialPreset(this.value);
        });
    }
}

function initializeCropPresetListener() {
    const preset = document.getElementById('cropPreset');
    if (preset) {
        preset.addEventListener('change', function () {
            setCropAspect(this.value);
        });
    }
}

// Rotation control
function setRotation(degrees) {
    const rotationInput = document.getElementById('rotationDegrees');
    if (rotationInput) {
        rotationInput.value = degrees;
    }

    document.querySelectorAll('.rotation-btn').forEach(btn => {
        btn.classList.remove('active', 'border-[#50E3C2]', 'bg-teal-50');
        btn.classList.add('border-gray-300');
    });

    const activeBtn = document.querySelector(`[data-rotation="${degrees}"]`);
    if (activeBtn) {
        activeBtn.classList.add('active', 'border-[#50E3C2]', 'bg-teal-50');
        activeBtn.classList.remove('border-gray-300');
    }
}

// Process single image via AJAX
async function processImage() {
    const fileInput = document.getElementById('fileInput');
    const file = fileInput?.files?.[0];
    if (!file) {
        showClientError('Please select an image file first.');
        return;
    }

    if (!validateFile(file)) return;

    document.getElementById('emptyState')?.classList.add('hidden');
    document.getElementById('processingState')?.classList.remove('hidden');
    const processBtn = document.getElementById('processButton');
    if (processBtn) {
        processBtn.disabled = true;
        document.getElementById('processButtonText').textContent = 'Processing...';
    }

    const formData = buildSharedFormData();
    formData.append('UploadedFile', file);

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token) {
        formData.append('__RequestVerificationToken', token);
    }

    try {
        const response = await fetch('/Tools/Image/ProcessAjax', {
            method: 'POST',
            body: formData,
            headers: token ? { 'RequestVerificationToken': token } : {}
        });

        const result = await response.json();

        if (result.success) {
            displayResult(result);
        } else {
            showClientError(result.error || 'An error occurred while processing your image.');
            document.getElementById('processingState')?.classList.add('hidden');
            document.getElementById('emptyState')?.classList.remove('hidden');
        }
    } catch (error) {
        console.error('Error:', error);
        showClientError('Network error. Please check your connection and try again.');
        document.getElementById('processingState')?.classList.add('hidden');
        document.getElementById('emptyState')?.classList.remove('hidden');
    } finally {
        if (processBtn) {
            processBtn.disabled = false;
            document.getElementById('processButtonText').textContent = 'Process Image(s)';
        }
    }
}

// Build shared form data for single/batch
function buildSharedFormData() {
    const formData = new FormData();
    formData.append('TargetWidth', document.getElementById('targetWidth')?.value || '');
    formData.append('TargetHeight', document.getElementById('targetHeight')?.value || '');
    formData.append('MaintainAspectRatio', document.getElementById('maintainAspectRatio')?.checked || false);
    formData.append('ResizePercentage', document.getElementById('resizePercentage')?.value || '');
    formData.append('Quality', document.getElementById('quality')?.value || '85');
    formData.append('TargetFileSizeKb', document.getElementById('targetFileSizeKb')?.value || '');
    formData.append('OutputFormat', document.getElementById('outputFormat')?.value || 'jpeg');
    formData.append('RotationDegrees', document.getElementById('rotationDegrees')?.value || '0');
    formData.append('FlipHorizontal', document.getElementById('flipHorizontal')?.checked || false);
    formData.append('FlipVertical', document.getElementById('flipVertical')?.checked || false);
    formData.append('ApplyGrayscale', document.getElementById('applyGrayscale')?.checked || false);
    formData.append('ApplySepia', document.getElementById('applySepia')?.checked || false);
    formData.append('Brightness', document.getElementById('brightness')?.value || '1.0');
    formData.append('Contrast', document.getElementById('contrast')?.value || '1.0');

    // Crop settings
    formData.append('CropPreset', document.getElementById('cropPreset')?.value || 'none');
    formData.append('CropX', document.getElementById('cropX')?.value || '');
    formData.append('CropY', document.getElementById('cropY')?.value || '');
    formData.append('CropWidth', document.getElementById('cropWidth')?.value || '');
    formData.append('CropHeight', document.getElementById('cropHeight')?.value || '');

    // Watermark settings
    formData.append('WatermarkText', document.getElementById('watermarkText')?.value || '');
    const watermarkFile = document.getElementById('watermarkImageFile')?.files?.[0];
    if (watermarkFile) {
        formData.append('WatermarkImageFile', watermarkFile);
    }
    formData.append('WatermarkPosition', document.getElementById('watermarkPosition')?.value || 'BottomRight');
    const opacity = document.getElementById('watermarkOpacity')?.value || '50';
    formData.append('WatermarkOpacity', (parseInt(opacity, 10) / 100).toString());
    formData.append('WatermarkFontFamily', document.getElementById('watermarkFontFamily')?.value || 'Arial');
    formData.append('WatermarkFontSize', document.getElementById('watermarkFontSize')?.value || '32');
    formData.append('WatermarkColor', document.getElementById('watermarkColor')?.value || '#ffffff');

    return formData;
}

// Process batch images
async function processBatch() {
    const fileInput = document.getElementById('fileInput');
    const files = fileInput?.files;

    if (!files || files.length === 0) {
        showClientError('Please select at least one image.');
        return;
    }

    if (files.length > MAX_BATCH_FILES) {
        showClientError(`Maximum ${MAX_BATCH_FILES} images allowed in batch mode.`);
        return;
    }

    for (const file of files) {
        if (!validateFile(file)) {
            return;
        }
    }

    hideClientError();
    showProcessing(files.length);

    const formData = buildSharedFormData();
    for (const file of files) {
        formData.append('files', file);
    }

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token) {
        formData.append('__RequestVerificationToken', token);
    }

    try {
        const response = await fetch('/Tools/Image/ProcessBatch', {
            method: 'POST',
            body: formData,
            headers: token ? { 'RequestVerificationToken': token } : {}
        });

        const result = await response.json();

        if (result.success) {
            displayBatchResults(result);
        } else {
            showClientError(result.error || 'Batch processing failed.');
        }
    } catch (error) {
        console.error('Batch processing error:', error);
        showClientError('Network error occurred. Please try again.');
    } finally {
        hideProcessing();
    }
}

// Display single result
function displayResult(result) {
    document.getElementById('processingState')?.classList.add('hidden');

    const resultsContainer = document.getElementById('resultsContainer');
    const processedSrc = `data:${result.mimeType};base64,${result.imageData}`;
    const originalSrc = originalPreviewUrl || '';
    resultsContainer.innerHTML = `
        <div id="processingResults" class="bg-white rounded-2xl shadow-xl p-6 animate-fadeIn">
            <div class="flex items-center justify-between mb-4">
                <h3 class="text-lg font-semibold text-gray-800">Processing Result</h3>
                <button onclick="clearResults()" class="text-gray-400 hover:text-gray-600 transition-colors">
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                    </svg>
                </button>
            </div>
            
            <div class="space-y-3 mb-6">
                <div class="bg-blue-50 rounded-lg p-3">
                    <div class="text-xs text-blue-600 font-medium mb-1">Original</div>
                    <div class="text-sm text-gray-800">${result.originalWidth} x ${result.originalHeight} px</div>
                    <div class="text-sm text-gray-600">${result.originalSize}</div>
                </div>
                
                <div class="bg-green-50 rounded-lg p-3">
                    <div class="text-xs text-green-600 font-medium mb-1">Processed</div>
                    <div class="text-sm text-gray-800">${result.processedWidth} x ${result.processedHeight} px</div>
                    <div class="text-sm text-gray-600">${result.processedSize}</div>
                </div>
                
                ${result.compressionRatio > 0 ? `
                <div class="bg-purple-50 rounded-lg p-3">
                    <div class="text-xs text-purple-600 font-medium mb-1">Compression</div>
                    <div class="text-lg font-bold text-purple-700">${result.compressionRatio}% smaller</div>
                </div>
                ` : ''}
            </div>

            <div class="mb-4">
                <div class="text-sm font-medium text-gray-700 mb-2">Preview</div>
                <div class="border border-gray-200 rounded-lg p-2 bg-gray-50 overflow-hidden">
                    ${originalSrc ? `
                    <img-comparison-slider class="w-full block">
                        <img slot="first" src="${originalSrc}" alt="Original image" class="w-full h-auto rounded" />
                        <img slot="second" src="${processedSrc}" alt="Processed image" class="w-full h-auto rounded" />
                    </img-comparison-slider>` : `
                    <img src="${processedSrc}"
                         alt="Processed image"
                         class="w-full h-auto rounded" />`
                    }
                </div>
            </div>

            <a href="/Tools/Image/Download?key=${result.downloadKey}" 
               class="block w-full text-center px-4 py-3 bg-gradient-to-r from-green-500 to-teal-500 text-white font-semibold rounded-lg shadow hover:shadow-lg transition-all duration-200 transform hover:-translate-y-0.5">
                <svg class="inline-block w-5 h-5 mr-2 -mt-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"></path>
                </svg>
                Download Image
            </a>
        </div>
    `;

    currentDownloadKey = result.downloadKey;
}

// Display batch results
function displayBatchResults(result) {
    const batchResults = document.getElementById('batchResults');
    const filesList = document.getElementById('batchFilesList');
    const downloadAllLink = document.getElementById('downloadAllLink');

    if (!batchResults || !filesList || !downloadAllLink) return;

    filesList.innerHTML = '';

    result.results.forEach(img => {
        const fileDiv = document.createElement('div');
        fileDiv.className = 'p-3 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors';

        const originalUrl = batchOriginalMap.get((img.fileName || '').toLowerCase());
        const processedSrc = img.imageData && img.mimeType ? `data:${img.mimeType};base64,${img.imageData}` : '';

        fileDiv.innerHTML = `
            <div class="flex items-center justify-between mb-2">
                <div class="flex items-center space-x-3">
                    <svg class="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                    </svg>
                    <span class="text-sm font-medium text-gray-700">${img.fileName}</span>
                    <span class="text-xs text-gray-500">(${img.processedSize || 'processed'})</span>
                </div>
                <a href="/Tools/Image/Download?key=${img.downloadKey}" 
                   class="px-4 py-2 bg-blue-600 text-white text-sm font-semibold rounded-lg hover:bg-blue-700 transition-colors">
                    Download
                </a>
            </div>
            ${processedSrc ? `
            <div class="border border-gray-200 rounded-lg p-2 bg-white">
                ${originalUrl ? `
                <img-comparison-slider class="w-full block">
                    <img slot="first" src="${originalUrl}" alt="${img.fileName} original" class="w-full h-auto rounded" />
                    <img slot="second" src="${processedSrc}" alt="${img.fileName} processed" class="w-full h-auto rounded" />
                </img-comparison-slider>` : `
                <img src="${processedSrc}" alt="${img.fileName} processed" class="w-full h-auto rounded" />
                `}
            </div>` : ''}
        `;
        filesList.appendChild(fileDiv);
    });

    downloadAllLink.href = `/Tools/Image/DownloadBatch?batchKey=${result.batchKey}`;
    batchResults.classList.remove('hidden');
    batchResults.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
}

// Show processing indicator for batch
function showProcessing(count) {
    let indicator = document.getElementById('processingIndicator');
    if (!indicator) {
        indicator = document.createElement('div');
        indicator.id = 'processingIndicator';
        indicator.className = 'fixed top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 bg-white rounded-xl shadow-2xl p-6 z-50';
        indicator.innerHTML = `
            <div class="text-center">
                <div class="animate-spin rounded-full h-16 w-16 border-b-4 border-blue-600 mx-auto mb-4"></div>
                <p class="text-lg font-semibold text-gray-800">Processing ${count} image${count > 1 ? 's' : ''}...</p>
                <p class="text-sm text-gray-600 mt-2">Please wait</p>
            </div>
        `;
        document.body.appendChild(indicator);
    }
    indicator.classList.remove('hidden');
}

function hideProcessing() {
    const indicator = document.getElementById('processingIndicator');
    if (indicator) {
        indicator.classList.add('hidden');
    }
}

// Cropper helpers
function initializeCropper(imageUrl) {
    const imageEl = document.getElementById('cropperImage');
    const placeholder = document.getElementById('cropperPlaceholder');
    if (!imageEl) return;

    if (cropper) {
        cropper.destroy();
        cropper = null;
    }

    imageEl.src = imageUrl;
    imageEl.onload = function () {
        if (placeholder) placeholder.classList.add('hidden');
        imageEl.classList.remove('opacity-0');

        cropper = new Cropper(imageEl, {
            viewMode: 1,
            responsive: true,
            autoCropArea: 0.9,
            aspectRatio: getAspectFromPreset(),
            preview: '#cropperPreview',
            movable: true,
            zoomable: true,
            scalable: false
        });
    };
}

function getAspectFromPreset() {
    const preset = document.getElementById('cropPreset')?.value || 'none';
    switch (preset) {
        case '1:1':
            return 1;
        case '4:3':
            return 4 / 3;
        case '16:9':
            return 16 / 9;
        case '3:2':
            return 3 / 2;
        default:
            return NaN;
    }
}

function setCropAspect(preset) {
    const select = document.getElementById('cropPreset');
    if (select) {
        select.value = preset;
    }

    if (cropper) {
        cropper.setAspectRatio(preset === 'none' ? NaN : getAspectFromPreset());
    }
}

function applyCropSelection() {
    if (!cropper) return;
    const data = cropper.getData(true);
    document.getElementById('cropX').value = Math.max(0, Math.round(data.x));
    document.getElementById('cropY').value = Math.max(0, Math.round(data.y));
    document.getElementById('cropWidth').value = Math.max(1, Math.round(data.width));
    document.getElementById('cropHeight').value = Math.max(1, Math.round(data.height));
}

function resetCropSelection() {
    if (cropper) {
        cropper.reset();
    }
    document.getElementById('cropX').value = '';
    document.getElementById('cropY').value = '';
    document.getElementById('cropWidth').value = '';
    document.getElementById('cropHeight').value = '';
}

function applySocialPreset(preset) {
    const widthInput = document.getElementById('targetWidth');
    const heightInput = document.getElementById('targetHeight');
    const percentageSlider = document.getElementById('resizePercentage');
    const percentageDisplay = document.getElementById('percentageDisplay');

    const presets = {
        'instagram-square': { w: 1080, h: 1080 },
        'instagram-story': { w: 1080, h: 1920 },
        'facebook-cover': { w: 820, h: 312 },
        'twitter-header': { w: 1500, h: 500 },
        'youtube-thumbnail': { w: 1280, h: 720 }
    };

    if (preset && presets[preset]) {
        const { w, h } = presets[preset];
        if (widthInput) widthInput.value = w;
        if (heightInput) heightInput.value = h;
        if (percentageSlider) {
            percentageSlider.value = 100;
            if (percentageDisplay) percentageDisplay.textContent = '100%';
        }
    } else {
        if (widthInput) widthInput.value = '';
        if (heightInput) heightInput.value = '';
    }
}

// Clear results and reset form
function clearResults() {
    document.getElementById('resultsContainer').innerHTML = `
        <div id="emptyState" class="bg-white rounded-2xl shadow-xl p-6">
            <h3 class="text-lg font-semibold text-gray-800 mb-4">Processing Info</h3>
            <div class="text-center py-12 text-gray-500">
                <svg class="mx-auto h-20 w-20 text-gray-300 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"></path>
                </svg>
                <p class="text-sm">Upload and process an image to see results</p>
            </div>
        </div>
        <div id="processingState" class="hidden bg-white rounded-2xl shadow-xl p-6">
            <div class="text-center py-12">
                <div class="inline-block animate-spin rounded-full h-16 w-16 border-4 border-gray-200 border-t-[#4A90E2] mb-4"></div>
                <p class="text-lg font-medium text-gray-800">Processing your image...</p>
                <p class="text-sm text-gray-500 mt-2">This may take a few moments</p>
            </div>
        </div>
    `;
    
    document.getElementById('batchResults')?.classList.add('hidden');
    document.getElementById('batchFilesList')?.replaceChildren();
    document.getElementById('downloadAllLink')?.setAttribute('href', '#');

    const fileInput = document.getElementById('fileInput');
    if (fileInput) {
        fileInput.value = '';
        fileInput.multiple = currentMode === 'batch';
    }
    document.getElementById('dropZoneContent')?.classList.remove('hidden');
    document.getElementById('filePreview')?.classList.add('hidden');
    document.getElementById('processButton').disabled = true;
    currentDownloadKey = null;
    hideClientError();
}
