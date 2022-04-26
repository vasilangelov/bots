function el(name, properties) {
    const e = document.createElement(name);

    if (typeof properties === "object") {
        Object.assign(e, properties);
    }

    return e;
}
