const alertContainer = document.getElementById('alertContainer');

function bootstrapAlert(description) {
    const alert = el('div', undefined, { class: 'alert alert-danger alert-dismissible fade show', role: 'alert' });

    alert.textContent = description;

    const closeBtn = el('button', undefined, { type: 'button', class: 'btn-close', 'data-bs-dismiss': 'alert', 'aria-label': 'Close' });
    alert.appendChild(closeBtn);

    alertContainer.appendChild(alert);
}
