function copyToClipboard() {
    const input = document.getElementById('shareUrl');
    input.select();
    input.setSelectionRange(0, 99999); // For mobile devices
    document.execCommand('copy');

    // Change button text temporarily
    const button = document.querySelector('.copy-link-btn');
    const originalHTML = button.innerHTML;
    button.innerHTML = '<i class="bi bi-check"></i> Copied!';
    button.style.background = 'var(--success)';

    setTimeout(() => {
        button.innerHTML = originalHTML;
        button.style.background = 'var(--primary)';
    }, 2000);
}
