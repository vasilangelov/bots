function el(name, properties) {
    const e = document.createElement(name);

    if (typeof properties === "object") {
        Object.assign(e, properties);
    }

    return e;
}

Date.fromUTCTime = function (milliseconds) {
    return new Date(milliseconds - (new Date().getTimezoneOffset() * 60000));
}
