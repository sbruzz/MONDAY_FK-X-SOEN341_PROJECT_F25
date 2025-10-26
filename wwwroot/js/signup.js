let currentStep = 1;
const totalSteps = 3;

function updateProgress() {
    const progressBar = document.getElementById('progressBar');
    const percentage = ((currentStep - 1) / (totalSteps - 1)) * 100;
    progressBar.style.width = percentage + '%';

    // Update step indicators
    document.querySelectorAll('.step').forEach((step, index) => {
        const stepNumber = index + 1;
        if (stepNumber < currentStep) {
            step.classList.add('completed');
            step.classList.remove('active');
        } else if (stepNumber === currentStep) {
            step.classList.add('active');
            step.classList.remove('completed');
        } else {
            step.classList.remove('active', 'completed');
        }
    });

    // Show/hide form steps
    document.querySelectorAll('.form-step').forEach(step => {
        step.classList.remove('active');
    });
    document.querySelector(`.form-step[data-step="${currentStep}"]`).classList.add('active');

    // Update review data if on step 3
    if (currentStep === 3) {
        document.getElementById('review-name').textContent = document.getElementById('name').value;
        document.getElementById('review-email').textContent = document.getElementById('email').value;
        document.getElementById('review-studentId').textContent = document.getElementById('studentId').value;
        document.getElementById('review-program').textContent = document.getElementById('program').value;
        document.getElementById('review-year').textContent = document.getElementById('year').value;
        document.getElementById('review-phone').textContent = document.getElementById('phone').value || 'Not provided';
    }
}

function nextStep() {
    // Validate current step
    const currentStepEl = document.querySelector(`.form-step[data-step="${currentStep}"]`);
    const inputs = currentStepEl.querySelectorAll('input[required], select[required]');
    let valid = true;

    inputs.forEach(input => {
        if (!input.value) {
            input.classList.add('is-invalid');
            valid = false;
        } else {
            input.classList.remove('is-invalid');
        }
    });

    if (!valid) {
        return;
    }

    if (currentStep < totalSteps) {
        currentStep++;
        updateProgress();
    }
}

function prevStep() {
    if (currentStep > 1) {
        currentStep--;
        updateProgress();
    }
}

// Initialize
updateProgress();
