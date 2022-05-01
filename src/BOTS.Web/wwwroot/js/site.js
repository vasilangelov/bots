function el(name, properties) {
    const e = document.createElement(name);

    if (typeof properties === "object") {
        Object.assign(e, properties);
    }

    return e;
}

Date.prototype.toPlotlyFormat = function () {
    const year = this.getFullYear().toString();
    const month = this.getMonth().toString().padStart(2, '0');
    const day = this.getDate().toString().padStart(2, '0');
    const hour = this.getHours().toString().padStart(2, '0');
    const minutes = this.getMinutes().toString().padStart(2, '0');
    const seconds = this.getSeconds().toString().padStart(2, '0');
    const milliseconds = this.getMilliseconds().toString().padStart(4, '0');

    return `${year}-${month}-${day} ${hour}:${minutes}:${seconds}.${milliseconds}`;
}

Date.fromPlotlyFormat = (function () {
    const plotlyDateFormat = /^(?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2})(?: (?<hour>[0-9]{2}):(?<minutes>[0-9]{2})(?::(?<seconds>[0-9]{2})(?:\.(?<milliseconds>[0-9]{1,4}))?)?)?$/m;

    return function (str) {
        const { year, month, day, hour, minutes, seconds, milliseconds } = Object.entries(plotlyDateFormat.exec(str).groups).reduce((acc, [key, value]) => {
            acc[key] = Number(value);

            if (Number.isNaN(acc[key])) {
                acc[key] = 0;
            }

            return acc;
        }, {});

        return new Date(year, month, day, hour, minutes, seconds, milliseconds);
    }
})();
