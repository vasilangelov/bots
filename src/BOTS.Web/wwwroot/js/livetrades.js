var connection = new signalR.HubConnectionBuilder()
    .withUrl('/Currencies/Live')
    .build();

const numberFormat = /^[0-9]+(?:.[0-9]{1,2})?$/m;

const currencyRateDiv = document.getElementById('currencyRate');
const currencyPairSelect = document.getElementById('currencyPair');
const tradingWindowsSelect = document.getElementById('tradingWindows');
const barrierContainer = document.getElementById('barrierContainer');
const payoutInput = document.getElementById('payoutInput');
const timeRemainingField = document.getElementById('timeRemaining');
const portfolioContainer = document.getElementById('activeBets');
const balanceContainer = document.getElementById('balance');

let activeBets = [];
let windowInfo = [];
let currentCurrencyPair;
let currentSubscribedTradingWindow;
let activeTimer;
let payout = Number(payoutInput.value);

function addCurrencySubscription() {
    currentCurrencyPair = Number(currencyPairSelect.value);

    connection.invoke('AddCurrencyRateSubscription', currentCurrencyPair);
}

function removeCurrencySubscription() {
    return connection.invoke('RemoveCurrencyRateSubscription', currentCurrencyPair);
}

function requestTradingWindows() {
    connection
        .invoke('GetActiveTradingWindows', currentCurrencyPair);
}

function requestActiveBets() {
    connection.invoke('GetActiveBets');
}

function changeButtonPrice(btn, payout, value) {
    if (value >= payout) {
        btn.disabled = true;
        btn.textContent = payout.toFixed(2);
    } else if (value <= 0) {
        btn.disabled = true;
        btn.textContent = Number(0).toFixed(2);
    } else {
        btn.disabled = false;
        btn.textContent = value.toFixed(2);
    }
}

function renderBarriers() {
    windowInfo.forEach((w, i) => {
        let barrier = barrierContainer.children[i];

        if (barrier === undefined) {
            const tr = el('tr');

            const th = el('th', { scope: 'row' });

            const tdh = el('td');
            const tdl = el('td');

            const btnh = el('button', { className: 'btn btn-primary container-fluid' }, { 'data-barrier-number': windowInfo.length - i - 1, 'data-prediction': 'Higher' });
            const btnl = el('button', { className: 'btn btn-danger container-fluid' }, { 'data-barrier-number': windowInfo.length - i - 1, 'data-prediction': 'Lower' });

            tdh.appendChild(btnh);
            tdl.appendChild(btnl);

            tr.appendChild(th);
            tr.appendChild(tdh);
            tr.appendChild(tdl);

            barrierContainer.appendChild(tr);

            barrier = barrierContainer.children[i];
        }

        barrier.querySelector('th').textContent = w.barrier;

        const higherEntry = Math.round(w.high * payout * 100) / 100;
        const higherBtn = barrier.querySelector('td > button.btn-primary');
        changeButtonPrice(higherBtn, payout, higherEntry);

        const lowerEntry = Math.round(w.low * payout * 100) / 100;
        const lowerBtn = barrier.querySelector('td > button.btn-danger');
        changeButtonPrice(lowerBtn, payout, lowerEntry);
    });
}

// TODO: when remove rendered bet???
function renderBets() {
    portfolioContainer.innerHTML = '';

    activeBets.forEach(bet => {
        const tr = el('tr');

        tr.appendChild(el('td', { textContent: bet.id }));
        tr.appendChild(el('td', { textContent: `${bet.currencyPair} is ${bet.type} than ${bet.barrier} on ${new Date(bet.endsOn).toLocaleString()}` }));
        tr.appendChild(el('td', { textContent: bet.entryFee.toFixed(2) }));
        tr.appendChild(el('td', { textContent: bet.payout.toFixed(2) }));

        portfolioContainer.appendChild(tr);
    });
}

connection.on('UpdateCurrencyRate', (cr) => {
    currencyRateDiv.textContent = cr;
});

connection.on('UpdateTimer', ({ end }) => {
    if (!!activeTimer) {
        clearInterval(activeTimer);
        activeTimer = undefined;
    }

    const endDate = new Date(end);

    activeTimer = setInterval(() => {
        const remaining = endDate.getTime() - Date.now();

        if (remaining < 0) {
            currentSubscribedTradingWindow = undefined;
            requestTradingWindows();
        }

        const hours = Math.floor(Math.abs(remaining / 1000 / 60 / 60)).toString().padStart(2, '0');
        const minutes = Math.floor(Math.abs(remaining / 1000 / 60)).toString().padStart(2, '0');
        const seconds = Math.floor(Math.abs(remaining / 1000 % 60)).toString().padStart(2, '0');
        const sign = remaining < 0 ? '-' : '';

        timeRemainingField.textContent = `${sign}${hours}:${minutes}:${seconds}`;
    }, 500);
});

connection.on('SetTradingWindows', (windows) => {
    tradingWindowsSelect.innerHTML = '';

    windows.forEach(({ id, start, end }) => {
        const startCurrent = new Date(start).toLocaleTimeString();
        const endCurrent = new Date(end).toLocaleTimeString();

        const displayName = `${startCurrent} - ${endCurrent}`;

        tradingWindowsSelect.appendChild(el('option', { value: id, textContent: displayName }));
    });

    if (!currentSubscribedTradingWindow && windows.length > 0) {
        const { id } = windows[0];

        if (currentSubscribedTradingWindow !== undefined) {
            connection.invoke('RemoveTradingWindowSubscription', currentSubscribedTradingWindow);
        }

        currentSubscribedTradingWindow = id;
        connection.invoke('AddTradingWindowSubscription', id);
    }
});

connection.on('UpdateTradingWindow', (windows) => {
    windowInfo = [];

    windows.forEach((w, i) => {
        windowInfo[i] = w;
    });

    renderBarriers();
});

connection.on('DisplayBet', (betInfo) => {
    activeBets.push(betInfo);
    renderBets();
});

connection.on('SetActiveBets', (bets) => {
    activeBets = bets;
    renderBets();
});

connection.on('UpdateBalance', (balance) => {
    balanceContainer.textContent = balance.toFixed(2);
});

connection.start()
    .then(addCurrencySubscription)
    .then(requestTradingWindows)
    .then(requestActiveBets);

currencyPairSelect.addEventListener('change', () => {
    removeCurrencySubscription()
        .then(addCurrencySubscription)
        .then(requestTradingWindows);
});

tradingWindowsSelect.addEventListener('focusin', requestTradingWindows);

tradingWindowsSelect.addEventListener('change', () => {
    const id = tradingWindowsSelect.value;

    if (currentSubscribedTradingWindow !== undefined) {
        connection.invoke('RemoveTradingWindowSubscription', currentSubscribedTradingWindow);
    }

    currentSubscribedTradingWindow = id;
    connection.invoke('AddTradingWindowSubscription', id);
});

payoutInput.addEventListener('input', (e) => {
    const val = e.target.value;
    if (!numberFormat.test(val)) {
        return;
    }

    payout = Number(val);
    renderBarriers();
});

barrierContainer.addEventListener('click', (e) => {
    if (e.target instanceof HTMLButtonElement) {
        const { barrierNumber, prediction } = e.target.dataset;

        connection.invoke('PlaceTradingWindowBet', tradingWindowsSelect.value, prediction, Number(barrierNumber), payout);
    }
});
