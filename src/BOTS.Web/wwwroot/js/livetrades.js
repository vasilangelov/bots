var connection = new signalR.HubConnectionBuilder()
    .withUrl('/Currencies/Live')
    .build();

const numberFormat = /^[0-9]+(?:.[0-9]{1,2})?$/m;

const currencyRateDiv = document.getElementById('currencyRate');
const currencyPairSelect = document.getElementById('currencyPair');
const bettingOptionsSelect = document.getElementById('bettingOptions');
const barrierContainer = document.getElementById('barrierContainer');
const payoutInput = document.getElementById('payoutInput');
const timeRemainingField = document.getElementById('timeRemaining');
const portfolioContainer = document.getElementById('activeBets');
const balanceContainer = document.getElementById('balance');

let activeBets = [];
let barriers = [];
let currentCurrencyPair;
let currentSubscribedBettingOption;
let activeTimer;
let payout = Number(payoutInput.value);

function addCurrencySubscription() {
    currentCurrencyPair = Number(currencyPairSelect.value);

    connection.invoke('AddCurrencyRateSubscription', currentCurrencyPair)
        .then(() => {
            if (currentSubscribedBettingOption !== undefined) {
                connection.invoke('RemoveBettingOptionSubscription', currentSubscribedBettingOption)
                    .then(() => {
                        currentSubscribedBettingOption = undefined;
                    })
                    .then(requestBettingOptionsForCurrencyPair);
            }
        })
}

function removeCurrencySubscription() {
    return connection.invoke('RemoveCurrencyRateSubscription', currentCurrencyPair);
}

function requestBettingOptionsForCurrencyPair() {
    return connection
        .invoke('GetBettingOptionsForCurrencyPair', currentCurrencyPair);
}

function requestActiveBets() {
    connection.invoke('GetActiveBets');
}

function updateBarriers() {
    connection.invoke('GetBarriers', currentSubscribedBettingOption);
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
    barriers.forEach((b, i) => {
        let barrier = barrierContainer.children[i];

        if (barrier === undefined) {
            const tr = el('tr');

            const th = el('th', { scope: 'row' });

            const tdh = el('td');
            const tdl = el('td');

            const btnh = el('button', { className: 'btn btn-primary container-fluid' }, { 'data-barrier-value': b.barrier, 'data-prediction': 'Higher' });
            const btnl = el('button', { className: 'btn btn-danger container-fluid' }, { 'data-barrier-value': b.barrier, 'data-prediction': 'Lower' });

            tdh.appendChild(btnh);
            tdl.appendChild(btnl);

            tr.appendChild(th);
            tr.appendChild(tdh);
            tr.appendChild(tdl);

            barrierContainer.appendChild(tr);

            barrier = barrierContainer.children[i];
        }

        barrier.querySelector('th').textContent = b.barrier;

        const higherEntry = Math.round(b.high * payout * 100) / 100;
        const higherBtn = barrier.querySelector('td > button.btn-primary');
        changeButtonPrice(higherBtn, payout, higherEntry);

        const lowerEntry = Math.round(b.low * payout * 100) / 100;
        const lowerBtn = barrier.querySelector('td > button.btn-danger');
        changeButtonPrice(lowerBtn, payout, lowerEntry);
    });
}

const contractLocalizationMap = {
    'en-US': '{0} is {1} than {2} on {3}',
    'bg-BG': '{0} е {1} от {2} в {3}'
};

const predictionLocalizationMap = {
    'en-US': { 'Higher': 'higher', 'Lower': 'lower' },
    'bg-BG': { 'Higher': 'по-висок', 'Lower': 'по-нисък' }
};

function renderBets() {
    portfolioContainer.innerHTML = '';

    for (const bet of activeBets) {
        const tr = el('tr');

        tr.appendChild(el('td', { textContent: bet.id }));
        const culture = getCulture();

        const messageFormat = contractLocalizationMap[culture] ?? contractLocalizationMap['en-US'];
        const betTypeFormatMap = predictionLocalizationMap[culture] ?? predictionLocalizationMap['en-US'];

        const betType = betTypeFormatMap[bet.type];

        const displayMessage = messageFormat.format(bet.currencyPair, betType, bet.barrierPrediction.toFixed(6), new Date(bet.endsOn).toLocaleString());

        tr.appendChild(el('td', { textContent: displayMessage }));
        tr.appendChild(el('td', { textContent: bet.entryFee.toFixed(2) }));
        tr.appendChild(el('td', { textContent: bet.payout.toFixed(2) }));

        portfolioContainer.appendChild(tr);
    }
}

connection.on('UpdateCurrencyRate', (cr) => {
    currencyRateDiv.textContent = cr.toFixed(6);
});

connection.on('UpdateTimer', (end) => {
    if (!!activeTimer) {
        clearInterval(activeTimer);
        activeTimer = undefined;
    }

    const endDate = new Date(end);

    let renewBettingOptionsTimeout;

    activeTimer = setInterval(() => {
        const remaining = endDate.getTime() - Date.now();

        if (remaining < 0 && renewBettingOptionsTimeout === undefined) {
            renewBettingOptionsTimeout = setTimeout(() => {
                currentSubscribedBettingOption = undefined;
                requestBettingOptionsForCurrencyPair().then(() => {
                    renewBettingOptionsTimeout = undefined;
                });
            }, 1000);
        }

        const hours = Math.floor(Math.abs(remaining / 1000 / 60 / 60)).toString().padStart(2, '0');
        const minutes = Math.floor(Math.abs(remaining / 1000 / 60)).toString().padStart(2, '0');
        const seconds = Math.floor(Math.abs(remaining / 1000 % 60)).toString().padStart(2, '0');
        const sign = remaining < 0 ? '-' : '';

        timeRemainingField.textContent = `${sign}${hours}:${minutes}:${seconds}`;
    }, 500);
});

connection.on('SetBettingOptions', (bettingOptions) => {
    bettingOptionsSelect.innerHTML = '';

    bettingOptions.forEach(({ id, start, end }) => {
        const startCurrent = new Date(start).toLocaleTimeString();
        const endCurrent = new Date(end).toLocaleTimeString();

        const displayName = `${startCurrent} - ${endCurrent}`;

        bettingOptionsSelect.appendChild(el('option', { value: id, textContent: displayName }));
    });

    if (!currentSubscribedBettingOption && bettingOptions.length > 0) {
        const { id } = bettingOptions[0];

        if (currentSubscribedBettingOption !== undefined) {
            connection.invoke('RemoveBettingOptionSubscription', currentSubscribedBettingOption);
        }

        currentSubscribedBettingOption = id;
        barrierContainer.innerHTML = '';
        connection.invoke('AddBettingOptionSubscription', id).then(updateBarriers);
    }
});

connection.on('UpdateBarriers', (bettingOptions) => {
    barriers = bettingOptions.sort((a, b) => b.barrier - a.barrier);

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

connection.on('RemoveEndedBet', (id) => {
    activeBets = activeBets.filter(bet => bet.tradingWindowId !== id);

    renderBets();
});

connection.on('UpdateBalance', (balance) => {
    balanceContainer.textContent = balance.toFixed(2);
});

connection.on('DisplayError', bootstrapAlert);

connection.start()
    .then(addCurrencySubscription)
    .then(requestBettingOptionsForCurrencyPair)
    .then(requestActiveBets);

currencyPairSelect.addEventListener('change', () => {
    removeCurrencySubscription()
        .then(addCurrencySubscription)
        .then(requestBettingOptionsForCurrencyPair);
});

bettingOptionsSelect.addEventListener('focusin', requestBettingOptionsForCurrencyPair);

bettingOptionsSelect.addEventListener('change', () => {
    const id = bettingOptionsSelect.value;

    if (currentSubscribedBettingOption !== undefined) {
        connection.invoke('RemoveBettingOptionSubscription', currentSubscribedBettingOption);
    }

    currentSubscribedBettingOption = id;
    barrierContainer.innerHTML = '';
    connection.invoke('AddBettingOptionSubscription', id).then(updateBarriers);
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
        const { barrierValue, prediction } = e.target.dataset;

        connection.invoke('PlaceBet', bettingOptionsSelect.value, prediction, Number(barrierValue), payout);
    }
});
