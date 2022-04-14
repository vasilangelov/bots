var connection = new signalR.HubConnectionBuilder()
    .withUrl('/Currencies/Live')
    .build();

const currencyRate = document.getElementById('currencyRate');
const currencyPair = document.getElementById('currencyPair');
const currencyInfo = {
    left: undefined,
    right: undefined
};

function addCurrencySubscription() {
    const { left, right } = currencyPair.options[currencyPair.selectedIndex].dataset;

    currencyInfo.left = left;
    currencyInfo.right = right;

    connection.invoke('AddCurrencySubscription', left, right);
}

function removeCurrencySubscription() {
    return connection.invoke('RemoveCurrencySubscription', currencyInfo.left, currencyInfo.right);
}

connection.on('CurrencyRateUpdate', (cr) => {
    currencyRate.textContent = cr;
});

connection.start().then(addCurrencySubscription);

currencyPair.addEventListener('change', () => {
    removeCurrencySubscription()
        .then(addCurrencySubscription);
});
