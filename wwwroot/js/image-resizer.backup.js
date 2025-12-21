// Image Resizer JavaScript Module
// Handles drag-and-drop, AJAX processing, and UI interactions

// Global state
let selectedFile = null;
let currentDownloadKey = null;

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    initializeDragAndDrop();
    initializeSliders();
    initializeFormControls();
});

// Drag and drop initialization
function initializeDragAndDrop() {
    const dropZone = document.getElementById('dropZone');
    const fileInput = document.getElementById('fileInput');

    if (!dropZone || !fileInput) return;

    // Prevent default drag behaviors
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        dropZone.addEventListener(eventName, preventDefaults, false);
        document.body.addEventListener(eventName, preventDefaults, false);
    });

    // Highlight drop zone when dragging over it
    ['dragenter', 'dragover'].forEach(eventName => {
        dropZone.addEventListener(eventName, highlight, false);
    });

    ['dragleave', 'drop'].forEach(eventName => {
        dropZone.addEventListener(eventName, unhighlight, false);
    });

    // Handle dropped files
    dropZone.addEventListener('drop', handleDrop, false);

    // Handle file input change
    fileInput.addEventListener('change', function(e) {
        if (e.target.files.length > 0) {
            handleFile(e.target.files[0]);
        }
    });

    // Keyboard accessibility
    dropZone.addEventListener('keydown', function(e) {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            fileInput.click();
        }
    });

    // Click to upload
    dropZone.addEventListener('click', function(e) {
        if (e.target !== fileInput) {
            fileInput.click();
        }
    });
}

function preventDefaults(e) {
    e.preventDefault();
    e.stopPropagation();
}

function highlight(e) {
    document.getElementById('dropZone').classList.add('border-[#4A90E2]', 'bg-blue-50');
}

function unhighlight(e) {
    const dropZone = document.getElementById('dropZone');
    if (!dropZone.querySelector('input:focus')) {
        dropZone.classList.remove('border-[#4A90E2]', 'bg-blue-50');
    }
}

function handleDrop(e) {
    const dt = e.dataTransfer;
    const files = dt.files;

    if (files.length > 0) {
        const fileInput = document.getElementById('fileInput');
        fileInput.files = files;
        handleFile(files[0]);
    }
}

function handleFile(file) {
    // Validate file type
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
    if (!allowedTypes.includes(file.type)) {
        showClientError('Invalid file type. Please upload a JPG, PNG, WebP, or GIF image.');
        return;
    }

    // Validate file size (10MB)
    const maxSize = 10 * 1024 * 1024;
    if (file.size > maxSize) {
        showClientError(`File size cannot exceed 10MB. Your file is ${(file.size / (1024 * 1024)).toFixed(2)}MB.`);
        return;
    }

    // Clear any previous errors
    hideClientError();

    // Update UI
    selectedFile = file;
    document.getElementById('dropZoneContent').classList.add('hidden');
    document.getElementById('filePreview').classList.remove('hidden');
    document.getElementById('fileName').textContent = file.name;
    document.getElementById('fileSize').textContent = formatFileSize(file.size);
    document.getElementById('processButton').disabled = false;
}

function showClientError(message) {
    const errorDiv = document.getElementById('clientError');
    const errorMessage = document.getElementById('clientErrorMessage');
    errorMessage.textContent = message;
    errorDiv.classList.remove('hidden');
    document.getElementById('processButton').disabled = true;
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
    // Quality slider
    const qualitySlider = document.getElementById('quality');
    const qualityDisplay = document.getElementById('qualityDisplay');
    if (qualitySlider && qualityDisplay) {
        qualitySlider.addEventListener('input', function() {
            qualityDisplay.textContent = this.value + '%';
        });
    }

    // Percentage slider
    const percentageSlider = document.getElementById('resizePercentage');
    const percentageDisplay = document.getElementById('percentageDisplay');
    if (percentageSlider && percentageDisplay) {
        percentageSlider.addEventListener('input', function() {
            percentageDisplay.textContent = this.value + '%';
        });
    }

    // Brightness slider
    const brightnessSlider = document.getElementById('brightness');
    const brightnessDisplay = document.getElementById('brightnessDisplay');
    if (brightnessSlider && brightnessDisplay) {
        brightnessSlider.addEventListener('input', function() {
            brightnessDisplay.textContent = parseFloat(this.value).toFixed(1);
        });
    }

    // Contrast slider
    const contrastSlider = document.getElementById('contrast');
    const contrastDisplay = document.getElementById('contrastDisplay');
    if (contrastSlider && contrastDisplay) {
        contrastSlider.addEventListener('input', function() {
            contrastDisplay.textContent = parseFloat(this.value).toFixed(1);
        });
    }
}

// Form controls initialization
function initializeFormControls() {
    // Mutual exclusivity between resize methods
    const widthInput = document.getElementById('targetWidth');
    const heightInput = document.getElementById('targetHeight');
    const percentageSlider = document.getElementById('resizePercentage');

    if (widthInput && percentageSlider) {
        widthInput.addEventListener('input', function() {
            if (this.value) {
                percentageSlider.value = 100;
                document.getElementById('percentageDisplay').textContent = '100%';
            }
        });
    }

    if (heightInput && percentageSlider) {
        heightInput.addEventListener('input', function() {
            if (this.value) {
                percentageSlider.value = 100;
                document.getElementById('percentageDisplay').textContent = '100%';
            }
        });
    }

    if (percentageSlider && widthInput && heightInput) {
        percentageSlider.addEventListener('input', function() {
            if (this.value != 100) {
                widthInput.value = '';
                heightInput.value = '';
            }
        });
    }
}

// Rotation control
function setRotation(degrees) {
    document.getElementById('rotationDegrees').value = degrees;
    
    // Update button states
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

// Process image via AJAX
async function processImage() {
    if (!selectedFile) {
        showClientError('Please select an image file first.');
        return;
    }

    // Show processing state
    document.getElementById('emptyState').classList.add('hidden');
    document.getElementById('processingState').classList.remove('hidden');
    document.getElementById('processButton').disabled = true;
    document.getElementById('processButtonText').textContent = 'Processing...';

    // Prepare form data
    const formData = new FormData();
    formData.append('UploadedFile', selectedFile);
    formData.append('TargetWidth', document.getElementById('targetWidth').value || '');
    formData.append('TargetHeight', document.getElementById('targetHeight').value || '');
    formData.append('MaintainAspectRatio', document.getElementById('maintainAspectRatio').checked);
    formData.append('ResizePercentage', document.getElementById('resizePercentage').value);
    formData.append('Quality', document.getElementById('quality').value);
    formData.append('TargetFileSizeKb', document.getElementById('targetFileSizeKb').value || '');
    formData.append('OutputFormat', document.getElementById('outputFormat').value);
    formData.append('RotationDegrees', document.getElementById('rotationDegrees').value);
    formData.append('FlipHorizontal', document.getElementById('flipHorizontal').checked);
    formData.append('FlipVertical', document.getElementById('flipVertical').checked);
    formData.append('ApplyGrayscale', document.getElementById('applyGrayscale').checked);
    formData.append('ApplySepia', document.getElementById('applySepia').checked);
    formData.append('Brightness', document.getElementById('brightness').value);
    formData.append('Contrast', document.getElementById('contrast').value);

    // Get anti-forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    formData.append('__RequestVerificationToken', token);

    try {
        const response = await fetch('/Tools/Image/ProcessAjax', {
            method: 'POST',
            body: formData,
            headers: {
                'RequestVerificationToken': token
            }
        });

        const result = await response.json();

        if (result.success) {
            displayResult(result);
        } else {
            showClientError(result.error || 'An error occurred while processing your image.');
            document.getElementById('processingState').classList.add('hidden');
            document.getElementById('emptyState').classList.remove('hidden');
        }
    } catch (error) {
        console.error('Error:', error);
        showClientError('Network error. Please check your connection and try again.');
        document.getElementById('processingState').classList.add('hidden');
        document.getElementById('emptyState').classList.remove('hidden');
    } finally {
        document.getElementById('processButton').disabled = false;
        document.getElementById('processButtonText').textContent = 'Process Image';
    }
}

// Display processing result
function displayResult(result) {
    // Hide processing state
    document.getElementById('processingState').classList.add('hidden');

    // Create result HTML
    const resultsContainer = document.getElementById('resultsContainer');
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
                    <div class="text-sm text-gray-800">${result.originalWidth} × ${result.originalHeight} px</div>
                    <div class="text-sm text-gray-600">${result.originalSize}</div>
                </div>
                
                <div class="bg-green-50 rounded-lg p-3">
                    <div class="text-xs text-green-600 font-medium mb-1">Processed</div>
                    <div class="text-sm text-gray-800">${result.processedWidth} × ${result.processedHeight} px</div>
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
                    <img src="data:${result.mimeType};base64,${result.imageData}" 
                         alt="Processed image" 
                         class="w-full h-auto rounded" />
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
    
    // Reset file input
    document.getElementById('fileInput').value = '';
    document.getElementById('dropZoneContent').classList.remove('hidden');
    document.getElementById('filePreview').classList.add('hidden');
    document.getElementById('processButton').disabled = true;
    selectedFile = null;
    currentDownloadKey = null;
}
