const init = () => {
    window.scrollToBottom = (el, force = false) => {
        if (!el) return;
        const shouldStick = force || (el.scrollHeight - el.scrollTop - el.clientHeight < 80);
        if (shouldStick) {
            el.scrollTop = el.scrollHeight;
        }
    };

    window.measureElementHeight = (el) => {
        if (!el || !(el instanceof HTMLElement)) return 0;
        const rect = el.getBoundingClientRect();
        const computed = getComputedStyle(el);
        const marginTop = parseFloat(computed.marginTop) || 0;
        const marginBottom = parseFloat(computed.marginBottom) || 0;
        return rect.height + marginTop + marginBottom;
    };
}

window.addEventListener("load", init)