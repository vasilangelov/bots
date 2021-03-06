const xPeriod = 3000;
const maxData = 10;
let chart;
let timeRange;

const chartTypeSelect = document.getElementById('chartTypeSelect');

const layout = {
    dragmode: 'zoom',
    xaxis: {
        type: 'date',
    },
    yaxis: {
        type: 'linear',
    },
    margin: {
        r: 20,
        t: 25,
        b: 40,
        l: 60,
    },
};

const trace = {
    x: [],
    open: [],
    low: [],
    high: [],
    close: [],
    line: { color: 'rgba(31,119,180,1)' },
    increasing: { line: { color: '#17BECF' } },
    decreasing: { line: { color: '#7F7F7F' } },
    type: chartTypeSelect.options[chartTypeSelect.selectedIndex].textContent.toLowerCase(),
    xaxis: 'x',
    yaxis: 'y',
    xperiod: xPeriod,
};

const data = [trace];

connection.on('SetCurrencyRateHistory', (histories) => {
    trace.x = [];
    trace.open = [];
    trace.low = [];
    trace.high = [];
    trace.close = [];

    histories.forEach(({ time, open, low, high, close }) => {
        trace.x.push(new Date(time).toPlotlyFormat());
        trace.open.push(open);
        trace.low.push(low);
        trace.high.push(high);
        trace.close.push(close);
    });

    const endDate = Date.fromPlotlyFormat(trace.x[trace.x.length - 1]);

    endDate.setMilliseconds(endDate.getMilliseconds() + xPeriod);

    timeRange = [trace.x[trace.x.length - 1 - maxData], endDate.toPlotlyFormat()];

    layout.xaxis.range = timeRange;

    Plotly.react('chart', data, layout, { responsive: true });

    if (!chart) {
        chart = document.getElementById('chart');

        let timeoutHandle;
        let lastEventData;

        chart.on('plotly_relayout', (e) => {
            if (e?.xaxis?.auto === true) {
                return;
            }

            lastEventData = e;

            if (timeoutHandle !== undefined) {
                clearTimeout(timeoutHandle);
                timeoutHandle = undefined;
            }

            timeoutHandle = setTimeout(() => {
                let from, to;

                if (typeof lastEventData['xaxsis.range[0]'] === 'string' &&
                    typeof lastEventData['xaxsis.range[1]'] === 'string') {
                    from = lastEventData['xaxis.range[0]'];
                    to = lastEventData['xaxis.range[1]'];
                } else if (Array.isArray(lastEventData['xaxis.range'])) {
                    const [fromString, toString] = lastEventData['xaxis.range'];
                    from = fromString;
                    to = toString;
                } else {
                    return;
                }

                timeRange = [from, to];

                timeoutHandle = undefined;
            }, 500);
        });
    }
});

connection.on('AddCurrencyRateHistory', ({ time, open, low, high, close }) => {
    try {
        timeRange = timeRange.map(x => new Date(Date.fromPlotlyFormat(x).getTime() + xPeriod).toPlotlyFormat());
    } catch { }

    Plotly.extendTraces(
        'chart',
        {
            x: [[new Date(time).toPlotlyFormat()]],
            open: [[open]],
            low: [[low]],
            high: [[high]],
            close: [[close]],
        },
        [0]
    );

    Plotly.relayout('chart', {
        xaxis: {
            auto: true,
            range: timeRange,
        }
    });
});

chartTypeSelect.addEventListener('change', () => {
    const chartType = chartTypeSelect.options[chartTypeSelect.selectedIndex].textContent.toLowerCase();

    trace.type = chartType;

    Plotly.update('chart', data, layout);
});
