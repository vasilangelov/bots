(function () {
    const timeElements = document.querySelectorAll('[data-display-time]');

    for (const el of timeElements) {
        el.textContent = new Date(el.dataset.displayTime).toLocaleString();
    }
})();
